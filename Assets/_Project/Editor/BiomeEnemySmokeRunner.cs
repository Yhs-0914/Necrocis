using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NecrocisEditor
{
    /// <summary>Run in an isolated batch project; never changes the user's open scene.</summary>
    public static class BiomeEnemySmokeRunner
    {
        private const string ConfigRoot = "Assets/_Project/Data/BiomeConfigs/";
        private static readonly string[] Biomes = { "Stomach", "Intestine", "Liver", "Lung" };
        private static readonly List<EnemySpawnRuleConfig> Rules = new List<EnemySpawnRuleConfig>();
        private static readonly List<string> Results = new List<string>();
        private static int index, phase, defeatEvents, experience;
        private static float started, playerHealth;
        private static double deadline;
        private static EnemyController enemy;
        private static SpriteRenderer renderer;
        private static Sprite previousSprite;
        private static bool projectileSeen;

        public static void Run()
        {
            if (!Application.isBatchMode)
                throw new InvalidOperationException("Use an isolated batch project for this smoke test.");
            try
            {
                ValidateAssetsAndScenes();
                SaveService.UseStorageRootForTests(Path.GetFullPath("Temp/BiomeEnemyTestSave"));
                Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
                EditorSettings.enterPlayModeOptionsEnabled = true;
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity", OpenSceneMode.Single);
                EditorApplication.playModeStateChanged += OnPlayMode;
                deadline = EditorApplication.timeSinceStartup + 150;
                EditorApplication.update += Tick;
                EditorApplication.isPlaying = true;
            }
            catch (Exception e) { Finish(false, e.ToString()); }
        }

        private static void ValidateAssetsAndScenes()
        {
            var salts = new HashSet<int>();
            var sprites = new HashSet<Sprite>();
            foreach (string biome in Biomes)
            {
                var config = AssetDatabase.LoadAssetAtPath<BiomeConfig>(ConfigRoot + biome + "BiomeConfig.asset");
                Require(config != null && config.enemySpawnConfig != null, biome + " config missing");
                var rules = config.GetEnemySpawnRules().Where(r => r.poissonSalt >= 1601 && r.poissonSalt <= 1612).ToArray();
                Require(rules.Length == 3, biome + " must have exactly three exclusive enemies");
                foreach (var r in rules)
                {
                    Require(r.name.StartsWith(biome, StringComparison.Ordinal), "Wrong biome: " + r.name);
                    Require(!r.isElite && !r.splitsOnDeath && !r.chargesAtPlayer && !r.leavesDebrisOnDeath, r.name + " elite flag");
                    Require(r.maxHealth >= 4 && r.maxHealth <= 8 && r.attackDamage == 1, r.name + " stats");
                    Require(r.density > 0 && r.maxAlive > 0 && r.enableContactDamage, r.name + " spawn/contact");
                    Require(salts.Add(r.poissonSalt), "Duplicate pool salt");
                    foreach (var frames in new[] { r.idleSprites, r.moveSprites, r.attackSprites, r.deathSprites })
                    {
                        Require(frames != null && frames.Length == 4, r.name + " missing animation frames");
                        foreach (var sprite in frames)
                        {
                            Require(sprite != null && sprites.Add(sprite), r.name + " missing/reused sprite reference");
                            var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite));
                            Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled, r.name + " filtering");
                            Require(sprite.texture.width == 128 && sprite.texture.height == 128, r.name + " size");
                        }
                    }
                    Rules.Add(r);
                }
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/" + biome + ".unity", OpenSceneMode.Single);
                var bridge = UnityEngine.Object.FindFirstObjectByType<ProceduralBiomeBridge>();
                Require(bridge != null && bridge.GetBiomeConfig() == config, biome + " scene config mismatch");
                Results.Add(biome + ": 3 exclusive rules, scene reference and 48 sprite imports verified");
            }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                LevelUpManager.OnLevelUp = null; // No level-up UI pauses in this disposable test run.
                LevelUpManager.OnExpGained += amount => experience += amount;
                phase = 0;
            }
        }

        private static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Runtime smoke timed out at enemy " + index + ", phase " + phase);
                if (!EditorApplication.isPlaying || PlayerController.Instance == null || PlayerController.Instance.HealthComponent == null) return;
                Time.timeScale = 1;
                if (index == Rules.Count) { Finish(true, "12/12 runtime interactions passed"); return; }
                var player = PlayerController.Instance;
                var health = player.HealthComponent;
                var rule = Rules[index];
                if (phase == 0)
                {
                    health.ResetHealth();
                    var position = player.transform.position + Vector3.forward * (rule.isRanged ? 4f : 1.2f);
                    enemy = EnemyController.Acquire(null, rule.name, EnemyController.GetPoolArchetypeId(rule));
                    enemy.Configure(null, rule, position, position);
                    enemy.SetAiSuppressed(true);
                    Invoke(enemy, "EnsurePlayerTransform");
                    Require(!enemy.IsElite && enemy.StatusEffects != null && enemy.GetComponent<EnemyContactDamage>() != null, rule.name + " components");
                    renderer = enemy.GetComponentInChildren<SpriteRenderer>();
                    Require(renderer.sprite == rule.idleSprites[0], rule.name + " idle start");
                    previousSprite = renderer.sprite;
                    started = Time.time; phase = 1;
                }
                else if (phase == 1 && Time.time - started > 0.21f)
                {
                    Require(renderer.sprite != previousSprite, rule.name + " idle not animating");
                    enemy.SetMoveAnimation(); previousSprite = renderer.sprite;
                    Require(previousSprite == rule.moveSprites[0], rule.name + " move start");
                    started = Time.time; phase = 2;
                }
                else if (phase == 2 && Time.time - started > 0.21f)
                {
                    Require(renderer.sprite != previousSprite, rule.name + " move not animating");
                    health.ResetHealth(); playerHealth = health.CurrentHealth;
                    Require(enemy.TryPerformAttack(0), rule.name + " attack did not start");
                    Require(renderer.sprite == rule.attackSprites[0], rule.name + " attack frame");
                    projectileSeen = false; started = Time.time; phase = 3;
                }
                else if (phase == 3)
                {
                    projectileSeen |= EnemyProjectile.ActiveEnemyProjectiles.Any(p => p != null && p.IsLaunched);
                    if (Time.time - started < 1.4f) return;
                    Require(!enemy.IsAttackAnimPlaying, rule.name + " attack did not finish");
                    Require(health.CurrentHealth < playerHealth, rule.name + " attack caused no damage");
                    if (rule.isRanged) Require(projectileSeen, rule.name + " no projectile");
                    EnemyProjectile.ReturnProjectilesOwnedBy(enemy);
                    health.ResetHealth();
                    Require(enemy.GetComponent<EnemyContactDamage>().TryApplyTo(player), rule.name + " contact damage");
                    Require(health.CurrentHealth < health.MaxHealth, rule.name + " contact health");
                    float before = enemy.Stats.CurrentHealth;
                    enemy.TakeDamage(1);
                    Require(enemy.Stats.CurrentHealth < before, rule.name + " incoming damage");
                    enemy.StatusEffects.ApplyStun(0.2f);
                    Require(enemy.StatusEffects.IsStunned && !enemy.TryPerformAttack(5), rule.name + " stun");
                    enemy.StatusEffects.ResetEffects();
                    defeatEvents = 0; experience = 0;
                    enemy.Defeated += CountDefeat;
                    // Killing during an attack must replace its callback with the death sequence.
                    Require(enemy.TryPerformAttack(5), rule.name + " second attack");
                    enemy.TakeDamage(999);
                    Require(enemy.IsDead && enemy.IsDeathAnimPlaying && !enemy.GetComponent<Collider>().enabled, rule.name + " death transition");
                    Require(renderer.sprite == rule.deathSprites[0], rule.name + " death sprite");
                    Require(defeatEvents == 1 && experience > 0, rule.name + " defeat/experience");
                    enemy.TakeDamage(999);
                    Require(defeatEvents == 1, rule.name + " duplicate defeat event");
                    started = Time.time; phase = 4;
                }
                else if (phase == 4 && Time.time - started > 1f)
                {
                    Require(!enemy.gameObject.activeSelf, rule.name + " death did not return to pool");
                    enemy.Defeated -= CountDefeat;
                    var recycled = EnemyController.Acquire(null, rule.name, EnemyController.GetPoolArchetypeId(rule));
                    Require(recycled == enemy, rule.name + " pool reuse");
                    var position = player.transform.position + Vector3.forward * 10;
                    recycled.Configure(null, rule, position, position);
                    recycled.SetAiSuppressed(true);
                    Require(!recycled.IsDead && !recycled.IsDeathAnimPlaying && !recycled.IsAttackAnimPlaying && recycled.GetComponent<Collider>().enabled, rule.name + " reset flags");
                    Require(recycled.Stats.CurrentHealth == recycled.Stats.MaxHealth && !recycled.StatusEffects.IsStunned, rule.name + " reset stats/status");
                    Require(renderer.sprite == rule.idleSprites[0], rule.name + " respawn idle");
                    recycled.ReleaseToPool();
                    Results.Add(rule.name + ": idle/move/attack, " + (rule.isRanged ? "projectile" : "melee") + " damage, contact, incoming damage, stun, interrupted-attack death, XP, pool reuse PASS");
                    index++; phase = 0;
                }
            }
            catch (Exception e) { Finish(false, e.ToString()); }
        }

        private static void CountDefeat(EnemyController _) => defeatEvents++;
        private static void Invoke(object instance, string method) => instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, null);
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Finish(bool passed, string message)
        {
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/BiomeEnemySmoke.txt", string.Join("\n", Results) + "\n" + (passed ? "PASS: " : "FAIL: ") + message);
            if (passed) Debug.Log("[BiomeEnemySmoke] PASS " + message); else Debug.LogError("[BiomeEnemySmoke] FAIL " + message);
            EditorApplication.Exit(passed ? 0 : 1);
        }
    }
}
