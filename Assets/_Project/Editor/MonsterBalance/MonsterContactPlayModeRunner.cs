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
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    // Contact checks use natural physics callbacks, never invoke TryApplyTo to fake a collision.
    public static class MonsterContactPlayModeRunner
    {
        private static readonly List<string> results = new List<string>();
        private static readonly List<EnemyController> actors = new List<EnemyController>();
        private static readonly Dictionary<Object, string> backups = new Dictionary<Object, string>();
        private static readonly BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static PlayerController player;
        private static Health health;
        private static bool started, passed, oldBackground, oldOptionsEnabled;
        private static EnterPlayModeOptions oldOptions;
        private static SceneSetup[] oldScenes;
        private static float oldTimeScale, playerY;
        private static double deadline;
        private static int enteredFrame;
        private static string storage;
        private static Vector3 center;
        private static GameObject factoryObject, lungObject;
        private static MidBossArenaController factory;
        private static MonsterBalanceCatalog catalog;
        private static readonly Dictionary<string, EnemySpawnRuleConfig> rules = new Dictionary<string, EnemySpawnRuleConfig>();
        private static readonly Dictionary<string, MidBossDefinition> bosses = new Dictionary<string, MidBossDefinition>();
        private const string Report = "Logs/MonsterContact-all-results.txt";

        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("변경 중인 씬을 먼저 저장하세요.");
            oldScenes = EditorSceneManager.GetSceneManagerSetup(); started = passed = false; results.Clear(); actors.Clear(); backups.Clear(); rules.Clear(); bosses.Clear();
            catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath);
            Require(catalog != null && catalog.GetValidationErrors().Count == 0, "catalog valid");
            storage = Path.Combine(Path.GetTempPath(), "necrocis-contact-" + Guid.NewGuid().ToString("N")); SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled; oldOptions = EditorSettings.enterPlayModeOptions;
            oldBackground = Application.runInBackground; oldTimeScale = Time.timeScale;
            EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions |= EnterPlayModeOptions.DisableDomainReload;
            Application.runInBackground = true; deadline = EditorApplication.timeSinceStartup + 360;
            EditorApplication.playModeStateChanged += OnPlay; EditorApplication.update += Tick;
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity"); EditorApplication.isPlaying = true;
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) enteredFrame = Time.frameCount;
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlay;
            foreach (var b in backups) if (b.Key != null) { EditorJsonUtility.FromJsonOverwrite(b.Value, b.Key); EditorUtility.ClearDirty(b.Key); } backups.Clear();
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled; EditorSettings.enterPlayModeOptions = oldOptions;
            Application.runInBackground = oldBackground; Time.timeScale = oldTimeScale;
            SaveService.ResetStaticStateForTests(); DifficultyBalanceService.ResetForTests();
            if (Directory.Exists(storage)) Directory.Delete(storage, true);
            File.WriteAllLines(Report, results); Debug.Log("[MonsterContact] " + (passed ? "ALL PASS" : "FAIL"));
            if (oldScenes != null) EditorSceneManager.RestoreSceneManagerSetup(oldScenes);
        }
        private static void Tick()
        {
            if (EditorApplication.isPlaying && !started) Application.runInBackground = true;
            if (EditorApplication.timeSinceStartup > deadline) { deadline = double.PositiveInfinity; Fail(new TimeoutException("Contact checks timed out")); return; }
            if (started || !EditorApplication.isPlaying || Time.frameCount-enteredFrame < 15) return;
            player = PlayerController.Instance; if (player == null || player.HealthComponent == null) return;
            health = player.HealthComponent; started = true; Time.timeScale = 1; player.StartCoroutine(Guard(Checks()));
        }
        private static IEnumerator Guard(IEnumerator root)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(root);
            while (stack.Count > 0)
            {
                object next;
                try { var c = stack.Peek(); if (!c.MoveNext()) { (c as IDisposable)?.Dispose(); stack.Pop(); continue; } next = c.Current; if (next is IEnumerator nested) { stack.Push(nested); continue; } }
                catch (Exception e) { Fail(e); yield break; }
                yield return next;
            }
            passed = true; EditorApplication.isPlaying = false;
        }
        private static IEnumerator Checks()
        {
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 1000, true);
            typeof(InputManager).GetMethod("SetActionsEnabled", Private).Invoke(InputManager.Instance, new object[] { false });
            center = new Vector3(player.transform.position.x, 0, player.transform.position.z); playerY = player.transform.position.y;
            factoryObject = new GameObject("ContactAuditBossFactory"); factory = factoryObject.AddComponent<MidBossArenaController>();
            foreach (string guid in AssetDatabase.FindAssets("t:EnemySpawnConfig", new[] { "Assets/_Project" }))
            foreach (var rule in AssetDatabase.LoadAssetAtPath<EnemySpawnConfig>(AssetDatabase.GUIDToAssetPath(guid)).GetEnemySpawnRules())
            { Require(rule.monsterDefinition != null && rule.addCollider, "spawn has definition and collider: " + rule.name); rules[rule.monsterDefinition.monsterId] = rule; }
            foreach (string name in new[] { "Intestine", "Liver", "Stomach", "Lung" })
            {
                var b = AssetDatabase.LoadAssetAtPath<BossArenaConfig>("Assets/_Project/Data/BiomeConfigs/" + name + "BossArenaConfig.asset").midBossArena.boss;
                bosses[b.bossRule.monsterDefinition.monsterId] = b;
                rules[b.bossRule.monsterDefinition.monsterId] = (EnemySpawnRuleConfig)typeof(MidBossArenaController).GetMethod("BuildRuntimeBossRule", Private).Invoke(factory, new object[] { b, b.patternType });
            }
            foreach (var config in catalog.biomeEliteSpawns)
                foreach (var rule in config.monsters)
                    rules[rule.monsterDefinition.monsterId] = rule;
            var intestineActor = Spawn(rules["boss.intestine"]); var intestine = intestineActor.gameObject.AddComponent<IntestineBossPattern>();
            intestine.Initialize(intestineActor, center, null, bosses["boss.intestine"].intestinePattern);
            rules["normal.intestine-parasite"] = (EnemySpawnRuleConfig)typeof(IntestineBossPattern).GetMethod("GetParasiteRule", Private).Invoke(intestine, null);
            int implemented = catalog.monsters.Count(d => !d.monsterId.StartsWith("elite.") || d.pattern != null);
            Require(rules.Count == implemented, "all implemented monster definitions have real runtime rules"); Clean();
            foreach (var d in catalog.monsters)
                Require(d.contact.enabled && d.contact.damageCoefficient > 0 && d.statSets.All(s => s.attackPower > 0), "positive contact on " + d.monsterId);
            Pass($"AUDIT all {catalog.monsters.Count} definitions contact enabled and positive; {rules.Count} implemented runtime rules plus {catalog.monsters.Count - rules.Count} unimplemented configuration templates");

            foreach (var difficulty in new[] { GameDifficulty.Normal, GameDifficulty.Hard })
            {
                Clean(); SaveService.MarkFinalBossDefeated(); Require(SaveService.TryBeginNewGame(difficulty, out string error), error);
                foreach (var d in catalog.monsters.Where(d => !rules.ContainsKey(d.monsterId)))
                {
                    var values = new MonsterBalanceRuntime(d, catalog.difficultyCatalog.Get(difficulty), catalog.progression, 0);
                    Require(values.ContactEnabled && values.Current.ContactDamage > 0, "future contact source");
                    Pass($"DATA ONLY {difficulty}/{d.monsterId}: contact={values.Current.ContactDamage}; pattern/map not implemented or registered");
                }
                foreach (var entry in rules.OrderBy(x => x.Key).Where(x => x.Key != "boss.lung"))
                {
                    foreach (var phase in entry.Value.monsterDefinition.statSets)
                    {
                        Clean(); Move(center + Vector3.right * 12); var enemy = Spawn(entry.Value);
                        if (bosses.TryGetValue(entry.Key, out var b)) ConfigureBoss(enemy, b, phase.id);
                        else if (phase.id != "Default") enemy.ApplyBalancePhase(phase.id, true);
                        yield return Touch(enemy, Vector3.right, difficulty + "/" + entry.Key + "/" + phase.id);
                    }
                }
                yield return LungChecks(difficulty);
            }
            yield return GuardsAndInspector();
            Clean(); Pass("complete: source assets restored after Inspector test, no production map or save changes");
        }
        private static EnemyController Spawn(EnemySpawnRuleConfig rule)
        {
            var e = EnemyController.Acquire(null, "ContactAudit_" + rule.name, EnemyController.GetPoolArchetypeId(rule));
            e.Configure(null, rule, center, center); e.SuppressExperienceReward = true;
            foreach (var p in e.GetComponents<MonsterPatternController>())
            {
                if (p is HelicoSpiralElitePattern || p is OilFilmElitePattern)
                {
                    // Retain their narrow, pattern-owned body contact, while isolating attacks.
                    p.GetType().GetField("nextReady", Private).SetValue(p, float.PositiveInfinity);
                    e.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
                }
                else p.EndSpawn();
            }
            e.SetAiSuppressed(true); actors.Add(e); return e;
        }
        private static void ConfigureBoss(EnemyController enemy, MidBossDefinition b, string phase)
        {
            switch (b.patternType)
            {
                case MidBossPatternType.Intestine:
                    var i = enemy.GetComponent<IntestineBossPattern>() ?? enemy.gameObject.AddComponent<IntestineBossPattern>(); i.Initialize(enemy, center, null, b.intestinePattern); i.SetEncounterActive(false); if (phase == "Phase2") i.ForcePhase2ForDebug(); break;
                case MidBossPatternType.Liver:
                    var l = enemy.GetComponent<LiverBossPattern>() ?? enemy.gameObject.AddComponent<LiverBossPattern>(); l.Initialize(enemy, center, null, b.liverPattern); l.SetEncounterActive(false); if (phase == "Phase2") l.ForcePhase2ForDebug(); break;
                case MidBossPatternType.Stomach:
                    var s = enemy.GetComponent<StomachBossPattern>() ?? enemy.gameObject.AddComponent<StomachBossPattern>(); s.Initialize(enemy, center, null, b.stomachPattern); s.SetEncounterActive(false); if (phase == "Phase2") s.ForcePhase2ForDebug(); break;
            }
            enemy.SetAiSuppressed(true); enemy.SetIgnoreMidBossArenaRestriction(true);
        }
        private static IEnumerator Touch(EnemyController enemy, Vector3 aim, string label)
        {
            var c = enemy.GetComponent<Collider>();
            Require(c.enabled && enemy.Balance.ContactEnabled && !Physics.GetIgnoreLayerCollision(c.gameObject.layer, player.HitCollider.gameObject.layer), "live contact preconditions " + label);
            float extent = c.bounds.extents.x + player.HitCollider.bounds.extents.x;
            Vector3 start = enemy.GetComponent<Rigidbody>().position + aim * (extent + .8f);
            Move(start); yield return new WaitForFixedUpdate(); health.ResetHealth(); float before = health.CurrentHealth;
            for (int n = 0; n < 70 && health.CurrentHealth == before; n++) { player.TryMoveByWorld(-aim * .05f); yield return new WaitForFixedUpdate(); }
            float expected = Units(enemy.Balance.Current.ContactDamage);
            Equal(expected, before-health.CurrentHealth, "natural collision " + label + " collider=" + c.GetType().Name + " trigger=" + c.isTrigger);
            Require(health.IsInvincible, "contact starts hurt invincibility"); float after = health.CurrentHealth;
            for (int n = 0; n < 8; n++) { player.TryMoveByWorld(-aim * .05f); yield return new WaitForFixedUpdate(); }
            Equal(after, health.CurrentHealth, "no frame-by-frame duplicate while invincible");
            Pass($"PHYSICS {label}: raw={enemy.Balance.Current.ContactDamage:F3}, actual HP damage={expected}, {(c.isTrigger ? "trigger" : "collision")} walking hit, invincibility blocks duplicates");
        }
        private static IEnumerator LungChecks(GameDifficulty difficulty)
        {
            foreach (bool second in new[] { false, true })
            {
                Clean(); Move(center + Vector3.right * 12); var boss = Spawn(rules["boss.lung"]);
                lungObject = new GameObject("ContactAuditLung"); var lung = lungObject.AddComponent<LungBossPattern>(); lung.Initialize(boss, center, null, bosses["boss.lung"].lungPattern);
                var brothers = new List<EnemyController>(); lung.ForEachEncounterBoss(b => { b.SuppressExperienceReward = true; brothers.Add(b); if (!actors.Contains(b)) actors.Add(b); });
                if (second) { boss.TakeDamage(100000); boss = brothers.First(b => !b.IsDead); Require(lung.CurrentPhaseName == "Phase2", "real sibling death switches lung phase"); }
                lung.SetEncounterActive(true);
                foreach (string name in new[] { "nextWindGustTime", "nextPhase2GasTime", "nextDiveTime" }) typeof(LungBossPattern).GetField(name, Private).SetValue(lung, float.PositiveInfinity);
                var gas = (float[])typeof(LungBossPattern).GetField("nextGasTime", Private).GetValue(lung); for (int i = 0; i < gas.Length; i++) gas[i] = float.PositiveInfinity;
                var contact = (float[])typeof(LungBossPattern).GetField("nextContactTime", Private).GetValue(lung); for (int i = 0; i < contact.Length; i++) contact[i] = 0;
                foreach (var other in brothers) if (other != boss && !other.IsDead) { other.GetComponent<Rigidbody>().position = center + Vector3.left * 25; other.transform.position = center + Vector3.left * 25; }
                yield return new WaitForFixedUpdate(); // Let initial sibling placement reach the physics world.
                Vector3 point = boss.GetComponent<Rigidbody>().position; Move(point + Vector3.right * 2.3f); yield return new WaitForFixedUpdate(); health.ResetHealth(); float hp = health.CurrentHealth;
                for (int n = 0; n < 70 && health.CurrentHealth == hp; n++) { player.TryMoveByWorld(Vector3.left * .05f); yield return new WaitForFixedUpdate(); }
                float expected = Units(boss.Balance.Current.ContactDamage); Equal(expected, hp-health.CurrentHealth, "native lung owned contact: boss="+boss.transform.position+" player="+player.transform.position+" active="+lung.enabled+" phase="+lung.CurrentPhaseName);
                Require(boss.PatternOwnsContact, "lung disables duplicate generic route");
                float after = health.CurrentHealth; yield return new WaitForSeconds(.2f); Equal(after, health.CurrentHealth, "lung no common plus radial duplicate");
                Pass($"OWNED {difficulty}/boss.lung/{(second ? "Phase2" : "Default")}: live pattern contact damage={expected}, generic route suppressed, no double hit");
            }
        }
        private static IEnumerator GuardsAndInspector()
        {
            Clean(); SaveService.TryBeginNewGame(GameDifficulty.Normal, out _); var rule = rules["elite.h-03"];
            foreach (var aim in new[] { Vector3.forward, Vector3.left, Vector3.back })
            { Clean(); var e = Spawn(rule); yield return Touch(e, aim, "Normal/H-03/direction=" + aim); }
            var target = actors.Last(); Move(center + Vector3.right * 10); health.ResetHealth(); var d = rule.monsterDefinition;
            backups[d] = EditorJsonUtility.ToJson(d);
            using (var data = new SerializedObject(d)) { data.FindProperty("contact.damageCoefficient").floatValue = 1.5f; data.ApplyModifiedPropertiesWithoutUndo(); }
            Equal(1, target.Balance.Current.ContactDamage, "existing contact snapshot remains 1");
            Clean(); var edited = Spawn(rule); yield return Touch(edited, Vector3.right, "Inspector next H-03 coefficient=1.5");
            Equal(3, edited.Balance.Current.ContactDamage, "edited coefficient produces 3");
            EditorJsonUtility.FromJsonOverwrite(backups[d], d); EditorUtility.ClearDirty(d); backups.Remove(d);
            Clean(); var dead = Spawn(rule); Move(center + Vector3.right * 8); dead.TakeDamage(100000); health.ResetHealth();
            Move(center); float hp = health.CurrentHealth; yield return new WaitForSeconds(.2f); Equal(hp, health.CurrentHealth, "dead body cannot damage");
            dead.ReleaseToPool(); yield return new WaitForFixedUpdate(); Equal(hp, health.CurrentHealth, "released body cannot damage");
            Pass("death/pool release cannot damage; Inspector coefficient affects next spawn only and original .5 restored");
            foreach (var e in actors) if (e != null && e.gameObject.activeSelf) e.ReleaseToPool();
        }
        private static void Move(Vector3 p) { p.y = playerY; player.SpawnAt(p); DontStarveCamera.Instance?.SnapToTarget(); }
        private static void Clean()
        {
            if (lungObject != null) { lungObject.GetComponent<LungBossPattern>()?.SetEncounterActive(false); Object.Destroy(lungObject); lungObject = null; }
            foreach (var e in actors) if (e != null && e.gameObject.activeSelf) e.ReleaseToPool(); actors.Clear();
        }
        private static float Units(float amount) => amount <= 0 ? 0 : Mathf.Max(1, Mathf.Floor(amount+.5001f));
        private static void Equal(float wanted, float actual, string label) => Require(Mathf.Abs(wanted-actual)<.01f, label+$": expected {wanted}, actual {actual}");
        private static void Require(bool value, string label) { if (!value) throw new InvalidOperationException(label); }
        private static void Pass(string text) { results.Add("PASS "+text); Debug.Log("[MonsterContact] PASS "+text); }
        private static void Fail(Exception error) { results.Add("FAIL "+error); Debug.LogException(error); EditorApplication.isPaused=false; EditorApplication.isPlaying=false; }
    }
}
