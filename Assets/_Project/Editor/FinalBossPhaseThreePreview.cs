using System;
using System.IO;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NecrocisEditor
{
    /// <summary>Renders production VFX in an isolated preview scene without loading gameplay or saves.</summary>
    public static class FinalBossPhaseThreePreview
    {
        [MenuItem("Tools/Necrocis/Final Boss/Validate and Render Sprite Animations")]
        public static void ValidateAndRenderAnimations()
        {
            FinalBossPhaseThreeChecks.Validate();
            string output = Environment.GetEnvironmentVariable("NECROCIS_PREVIEW_OUTPUT");
            if (string.IsNullOrEmpty(output)) output = "Exports/FinalBossConcepts/2026-10-05-animations/Unity";
            Directory.CreateDirectory(output);
            Scene scene = EditorSceneManager.NewPreviewScene();
            GameObject root = EditorUtility.CreateGameObjectWithHideFlags("AnimationPreview", HideFlags.HideAndDontSave);
            SceneManager.MoveGameObjectToScene(root, scene);
            try
            {
                GameObject cameraObject = new GameObject("Camera", typeof(Camera));
                cameraObject.transform.SetParent(root.transform, false);
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.scene = scene;
                camera.orthographic = true;
                camera.orthographicSize = 5.4f;
                camera.aspect = 1f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.075f, .06f, .1f);
                camera.transform.position = new Vector3(0f, 4.5f, -15f);
                GameObject artwork = new GameObject("Artwork", typeof(SpriteRenderer));
                artwork.transform.SetParent(root.transform, false);
                FinalBossSpriteAnimator animation = artwork.AddComponent<FinalBossSpriteAnimator>();
                if (!animation.Initialize()) throw new InvalidOperationException("Missing animation frames");
                artwork.transform.localScale = Vector3.one * (9.5f / animation.IdleSprite.bounds.size.x);
                foreach (FinalBossSpriteAnimator.Pose pose in Enum.GetValues(typeof(FinalBossSpriteAnimator.Pose)))
                {
                    for (int frame = 0; frame < 4; frame++)
                    {
                        animation.Play(pose, 1.6f, restart: true);
                        animation.Advance(frame * .4f + .001f);
                        Capture(camera, Path.Combine(output, pose + "_" + frame.ToString("00") + ".png"), 768, 768);
                    }
                }
                File.WriteAllText(Path.Combine(output, "validation-result.txt"),
                    "PASS: 24 imported sprites; consistent size/pivots; state transitions, loops, melee impact/recovery and terminal death; existing geometry/hitboxes; 24 Unity-rendered previews.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
        }

        [MenuItem("Tools/Necrocis/Final Boss/Render Phase Three Combat Previews")]
        public static void Render()
        {
            string output = Environment.GetEnvironmentVariable("NECROCIS_PREVIEW_OUTPUT");
            if (string.IsNullOrEmpty(output)) output = "Exports/FinalBossConcepts/PhaseThreePolish";
            Directory.CreateDirectory(output);
            Scene scene = EditorSceneManager.NewPreviewScene();
            GameObject root = EditorUtility.CreateGameObjectWithHideFlags("PhaseIII_Preview", HideFlags.HideAndDontSave);
            SceneManager.MoveGameObjectToScene(root, scene);
            try
            {
                var cameraGo = new GameObject("Camera"); cameraGo.transform.SetParent(root.transform, false);
                Camera camera = cameraGo.AddComponent<Camera>(); camera.scene = scene;
                camera.orthographic = true; camera.orthographicSize = 10.2f;
                camera.aspect = 16f / 9f; camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.045f, .035f, .062f);
                camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
                camera.transform.position = new Vector3(2f, 23f, -23f);
                SpriteRenderer boss = Sprite(root.transform, "MobileCerebrum", new Vector3(-2f, .2f, 1f), 8.1f, camera);
                boss.sortingOrder = 5040;
                // A subdued floor grid makes depth and contact easy to judge.
                for (int i = -18; i <= 18; i += 3)
                {
                    LineRenderer x = FinalBossNeuralVfx.CreateLine(root.transform, "FloorGrid", .016f, 100);
                    x.positionCount = 2; x.SetPositions(new[] {new Vector3(-20f, 0f, i), new Vector3(20f, 0f, i)});
                    x.startColor = x.endColor = new Color(.22f, .14f, .2f, .23f);
                    LineRenderer z = FinalBossNeuralVfx.CreateLine(root.transform, "FloorGrid", .016f, 100);
                    z.positionCount = 2; z.SetPositions(new[] {new Vector3(i, 0f, -20f), new Vector3(i, 0f, 20f)});
                    z.startColor = z.endColor = x.startColor;
                }
                Vector3 origin = new Vector3(-2f, 1.9f, 1f);
                var effects = new GameObject("PreviewEffects"); effects.transform.SetParent(root.transform, false);
                FinalBossTentacleVisual limb = FinalBossTentacleVisual.Create(effects.transform, camera);
                limb.Pose(origin, new Vector3(8f, .65f, -3f), 2.4f, .35f, 1f);
                Capture(camera, Path.Combine(output, "01-tentacle-thrust.png"));
                for (int frame = 0; frame < 8; frame++)
                {
                    float t = frame / 7f;
                    float reach = frame < 2 ? Mathf.Lerp(2.8f, 1.4f, t * 7f) : frame < 4 ? 10f : Mathf.Lerp(10f, .8f, (frame - 4) / 3f);
                    limb.Pose(origin, origin + new Vector3(.93f, 0f, -.37f) * reach + Vector3.down * 1.25f,
                        2.4f, frame < 2 ? 1.8f : .3f + (1f - reach / 10f), frame < 2 ? .4f : 1f,
                        frame == 7 ? .1f : 1f);
                    Capture(camera, Path.Combine(output, "motion-" + frame.ToString("00") + ".png"));
                }
                UnityEngine.Object.DestroyImmediate(effects);
                effects = new GameObject("Fan"); effects.transform.SetParent(root.transform, false);
                for (int i = -2; i <= 2; i++)
                {
                    Vector3 dir = Quaternion.Euler(0f, i * 27f, 0f) * Vector3.back;
                    FinalBossTentacleVisual.Create(effects.transform, camera).Pose(origin,
                        new Vector3(-2f, .6f, 1f) + dir * 9f, 1.6f, i * .16f, 1f);
                }
                Capture(camera, Path.Combine(output, "02-tentacle-fan.png"));
                UnityEngine.Object.DestroyImmediate(effects);
                var shield = FinalBossNeuralVfx.Create(root.transform, boss.bounds.center,
                    FinalBossNeuralVfx.Shape.Shield, 4.65f, new Color(.22f, .78f, 1f, .85f), 0f, camera);
                shield.RenderAt(.7f);
                Capture(camera, Path.Combine(output, "03-reflection-shell.png"));
                UnityEngine.Object.DestroyImmediate(shield.gameObject);
                var burst = FinalBossNeuralVfx.Create(root.transform, new Vector3(6f, .1f, -3f),
                    FinalBossNeuralVfx.Shape.Burst, 2.1f, new Color(.24f, .76f, 1f, .9f), .52f, camera);
                burst.RenderAt(.17f);
                var bomb = Sprite(root.transform, "CellBomb", new Vector3(4f, 4f, 0f), 2.1f, camera);
                Capture(camera, Path.Combine(output, "04-cell-impact.png"));
                UnityEngine.Object.DestroyImmediate(bomb.gameObject);
                UnityEngine.Object.DestroyImmediate(burst.gameObject);
                var swipe = FinalBossNeuralVfx.Create(root.transform, new Vector3(-2f, .1f, 1f),
                    FinalBossNeuralVfx.Shape.Sweep, 7.5f, new Color(.22f, .8f, 1f, .8f), 0f, camera);
                swipe.SetSweep(-40f, 90f); swipe.RenderAt(.3f);
                var sweepingLimb = FinalBossTentacleVisual.Create(root.transform, camera);
                Vector3 direction = new Vector3(Mathf.Cos(-40f * Mathf.Deg2Rad), 0f, Mathf.Sin(-40f * Mathf.Deg2Rad));
                sweepingLimb.Pose(origin, new Vector3(-2f, .6f, 1f) + direction * 7.5f, 1.8f, -1.1f, 1f);
                Capture(camera, Path.Combine(output, "05-tentacle-sweep.png"));
                UnityEngine.Object.DestroyImmediate(swipe.gameObject);
                UnityEngine.Object.DestroyImmediate(sweepingLimb.gameObject);
                var boundary = FinalBossNeuralVfx.Create(root.transform, Vector3.zero,
                    FinalBossNeuralVfx.Shape.Boundary, 1f, Color.cyan, 0f, camera);
                boundary.SetBoundary(new Rect(-9f, -6f, 19f, 14f)); boundary.RenderAt(.7f);
                Capture(camera, Path.Combine(output, "06-closing-boundary.png"));
                UnityEngine.Object.DestroyImmediate(boundary.gameObject);
                if (ShaderUtil.ShaderHasError(Resources.Load<Shader>("FinalBoss/PhaseThree/NeuralEnergy")))
                    throw new InvalidOperationException("Neural energy shader failed to compile.");
                File.WriteAllText(Path.Combine(output, "render-result.txt"), "PASS: production tentacle meshes, energy shader, shield, burst and native-aspect sprites rendered at 1920x1080.");
                Debug.Log("[FinalBoss III] Previews saved to " + Path.GetFullPath(output));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
        }

        private static SpriteRenderer Sprite(Transform parent, string name, Vector3 position, float width, Camera camera)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = position;
            go.transform.rotation = camera.transform.rotation;
            var sprite = go.AddComponent<SpriteRenderer>(); sprite.sprite = Resources.Load<Sprite>("FinalBoss/PhaseThree/" + name);
            go.transform.localScale = Vector3.one * (width / sprite.sprite.bounds.size.x); sprite.sortingOrder = 5050;
            return sprite;
        }

        private static void Capture(Camera camera, string path, int width = 1920, int height = 1080)
        {
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0); image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null; RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(image); target.Release(); UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
