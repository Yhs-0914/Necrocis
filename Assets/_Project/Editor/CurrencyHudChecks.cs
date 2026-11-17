using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NecrocisEditor
{
    public static class CurrencyHudChecks
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static IEnumerator scenario;
        private static bool started, passed, previousEnabled;
        private static EnterPlayModeOptions previousOptions;
        private static double deadline;
        private static int assertions;

        public static void Run()
        {
            if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Use an isolated batch project.");
            passed = started = false; assertions = 0;
            previousEnabled = EditorSettings.enterPlayModeOptionsEnabled; previousOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            GameplayFocusOwner.TestFocusOverride = true;
            SaveService.UseStorageRootForTests(Path.GetFullPath("tmp/currency-hud-" + Guid.NewGuid().ToString("N")));
            SessionRuntimeBindings.Register();
            if (!SaveService.TryBeginNewGame(GameDifficulty.Normal, out var error)) throw new InvalidOperationException(error);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += OnState;
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup > deadline) { Debug.LogError("Currency HUD checks timed out."); Finish(); return; }
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || Time.frameCount < 3) return;
            try
            {
                if (!started) { started = true; scenario = Checks(); }
                if (!scenario.MoveNext()) { passed = true; Debug.Log("CURRENCY_HUD_PASS assertions=" + assertions); EditorApplication.isPlaying = false; }
            }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.isPlaying = false; }
        }

        private static IEnumerator Checks()
        {
            InputManager.Instance.Focus.SetNativeFocused(true); InputManager.ResetSessionClock();
            Time.timeScale = 1;
            PlayerCurrencyUI currency = null;
            foreach (string scene in new[] { "Hub", "FinalBoss" })
            {
                EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/" + scene + ".unity", new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;
                double until = EditorApplication.timeSinceStartup + 20;
                while (EditorApplication.timeSinceStartup < until)
                {
                    currency = UnityEngine.Object.FindFirstObjectByType<PlayerCurrencyUI>();
                    if (SceneManager.GetActiveScene().name == scene && currency != null && currency.Badge != null) break;
                    yield return null;
                }
                Require(currency != null && currency.Badge != null && currency.CoinImage.sprite != null, scene + " auto-installed currency HUD with coin asset");
                var player = currency.GetComponent<PlayerController>();
                player.enabled = false; player.GetComponent<PlayerAttack>().enabled = false;
                player.GetComponent<PlayerClassSkillController>().enabled = false;
                player.GetComponent<Rigidbody>().isKinematic = true;
                Require(currency.Amount == (scene == "Hub" ? 0 : 1250), scene + " display amount survives scene change");
                if (scene == "Hub") SaveService.CreditCurrency(1250);
                Require(currency.AmountText.text == "1,250", "grouped currency number");
                Require(currency.Badge.GetComponentsInChildren<Graphic>().All(value => !value.raycastTarget)
                    && !currency.Badge.GetComponent<CanvasGroup>().blocksRaycasts, "passive HUD does not block gameplay clicks");
                var composition = player.GetComponent<PlayerCompositionRoot>();
                composition.InstallHud(); composition.InstallHud();
                Require(player.GetComponents<PlayerCurrencyUI>().Length == 1
                    && GameObject.FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None).Count(value => value.name == "CurrencyBadge") == 1,
                    scene + " repeated HUD install does not duplicate currency");
                if (scene == "FinalBoss")
                {
                    var arena = UnityEngine.Object.FindFirstObjectByType<FinalBossArena>();
                    var map = UnityEngine.Object.FindFirstObjectByType<ProceduralMap.MapGenerator>();
                    until = EditorApplication.timeSinceStartup + 15;
                    while (map != null && !map.IsReady && EditorApplication.timeSinceStartup < until) yield return null;
                    Require(arena != null && map != null && map.IsReady, "final boss terrain ready for HUD preview");
                    Vector3 stand = arena.UVToWorld(new Vector2(.5f,.2f)); stand.y = arena.SpawnPosition.y;
                    player.UnlockY(); player.SpawnAt(stand);
                    var follow = UnityEngine.Object.FindFirstObjectByType<DontStarveCamera>();
                    if (follow != null) { follow.SetTarget(player.transform); follow.SnapToTarget(); }
                    // Preview the real gauge component while the encounter is still at its entrance.
                    if (UnityEngine.Object.FindFirstObjectByType<FinalBossScreenHealthBar>() == null)
                    {
                        var gauge = FinalBossScreenHealthBar.Create(arena.transform,4,4);
                        gauge.SetSealProgress(0,4);
                        gauge.BindSeals(arena.GetComponent<FinalBossPhaseOneController>().Pillars);
                    }
                }
                for (int i = 0; i < 3; i++) yield return null;
                CheckSeparation(currency, scene);
                foreach (var size in scene == "Hub" ? new[] { new Vector2Int(1920,1080) }
                    : new[] { new Vector2Int(1920,1080),new Vector2Int(1024,768),new Vector2Int(2560,1080) })
                {
                    RenderPreview(currency, scene, size.x, size.y);
                    yield return null;
                }
            }
            var stats = currency.GetComponent<PlayerStats>().RuntimeStats;
            float originalMax = stats.MaxHealth;
            stats.SetBaseStat(CharacterStatType.MaxHealth, 52); stats.SetCurrentHealth(52);
            for (int i = 0; i < 3; i++) yield return null;
            CheckSeparation(currency, "wrapped hearts");
            currency.SetAmount(long.MaxValue);
            Require(currency.AmountText.text == "9,223,372,036,854,775,807" && currency.Badge.sizeDelta.x <= 320,
                "large amount stays in compact bounded badge");
            currency.SetAmount(-1); Require(currency.Amount == 0 && currency.AmountText.text == "0", "negative display input clamps to zero");
            currency.enabled = false; Require(!currency.Badge.gameObject.activeSelf, "disabled HUD hides badge");
            currency.enabled = true; Require(currency.Badge.gameObject.activeSelf, "reenabled HUD restores same badge");
            stats.SetBaseStat(CharacterStatType.MaxHealth, originalMax); currency.SetAmount(0);
        }

        private static Rect ScreenRect(RectTransform rect, Camera camera)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            Vector2 first = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            Vector2 last = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            return Rect.MinMaxRect(first.x,first.y,last.x,last.y);
        }

        private static void CheckSeparation(PlayerCurrencyUI currency, string context)
        {
            Canvas.ForceUpdateCanvases();
            typeof(PlayerCurrencyUI).GetMethod("LateUpdate", Members).Invoke(currency, null);
            var heart = currency.GetComponent<PlayerHeartUI>();
            Rect coin = ScreenRect(currency.Badge, null), hearts = ScreenRect(heart.HeartContainer, null);
            Require(!coin.Overlaps(hearts) && coin.yMax <= hearts.yMin, context + " currency below hearts without overlap");
            Require(coin.xMin >= 0 && coin.yMin >= 0 && coin.xMax <= Screen.width + 1 && coin.yMax <= Screen.height + 1,
                context + " badge within screen bounds");
            var gauges = UnityEngine.Object.FindObjectsByType<FinalBossScreenHealthBar>(FindObjectsSortMode.None);
            if (context == "FinalBoss") Require(gauges.Length > 0, "final boss preview includes real boss gauge");
            foreach (var gauge in gauges)
            {
                var panel = gauge.transform.Find("StageGaugePanel") as RectTransform;
                Require(panel == null || !coin.Overlaps(ScreenRect(panel,null)), context + " no overlap with final boss gauge");
            }
        }

        private static void RenderPreview(PlayerCurrencyUI currency, string scene, int width, int height)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            var camera = Camera.main;
            Require(camera != null, "preview scene has game camera");
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Where(value => value.isRootCanvas && value.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
            var previous = RenderTexture.active; var previousTarget = camera.targetTexture;
            var target = new RenderTexture(width,height,24);
            try
            {
                foreach (var gauge in UnityEngine.Object.FindObjectsByType<FinalBossScreenHealthBar>(FindObjectsSortMode.None))
                    gauge.GetComponent<CanvasGroup>().alpha = 1;
                camera.targetTexture = target;
                foreach (var canvas in canvases)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = .5f;
                    var scaler = canvas.GetComponent<CanvasScaler>();
                    if (scaler != null) { scaler.enabled = false; scaler.enabled = true; }
                }
                Canvas.ForceUpdateCanvases();
                typeof(PlayerCurrencyUI).GetMethod("LateUpdate",Members).Invoke(currency,null);
                Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
                var image = new Texture2D(width,height,TextureFormat.RGBA32,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
                Directory.CreateDirectory("tmp/currency-hud-previews");
                File.WriteAllBytes("tmp/currency-hud-previews/"+scene+"-"+width+"x"+height+".png", image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
                Rect rect = ScreenRect(currency.Badge,camera);
                Require(rect.xMin >= -1 && rect.yMin >= -1 && rect.xMax <= width+1 && rect.yMax <= height+1,
                    scene+" "+width+"x"+height+" currency stays on screen");
            }
            finally
            {
                foreach (var canvas in canvases) if (canvas != null) { canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.worldCamera=null; }
                camera.targetTexture=previousTarget; RenderTexture.active=previous; target.Release(); UnityEngine.Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        private static void Require(bool value,string label) { assertions++; if (!value) throw new InvalidOperationException(label); }
        private static void OnState(PlayModeStateChange state) { if (state==PlayModeStateChange.EnteredEditMode) Finish(); }
        private static void Finish()
        {
            EditorApplication.update-=Tick; EditorApplication.playModeStateChanged-=OnState;
            EditorSettings.enterPlayModeOptionsEnabled=previousEnabled; EditorSettings.enterPlayModeOptions=previousOptions;
            GameplayFocusOwner.TestFocusOverride=null; Time.timeScale=1; SaveService.ResetStaticStateForTests(); EditorApplication.Exit(passed?0:1);
        }
    }
}
