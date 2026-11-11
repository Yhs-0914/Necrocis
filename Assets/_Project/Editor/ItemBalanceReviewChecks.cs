using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NecrocisEditor
{
    // Batch-only regression checks in an empty scene and isolated save storage.
    public static class ItemBalanceReviewChecks
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> checks = new List<string>();
        private static bool passed, started, oldOptionsEnabled;
        private static EnterPlayModeOptions oldOptions;
        private static double deadline;

        public static void Run()
        {
            if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run in an isolated Unity batch project.");
            checks.Clear(); passed = false; started = false;
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            oldOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            SaveService.UseStorageRootForTests(Path.GetFullPath("tmp/item-review-saves"));
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("ReviewAudioListener").AddComponent<AudioListener>();
            deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.playModeStateChanged += OnState;
            EditorApplication.update += Timeout;
            EditorApplication.isPlaying = true;
        }

        private static void Timeout()
        {
            if (!started && EditorApplication.isPlaying && !EditorApplication.isCompiling && Time.frameCount > 2)
            {
                started = true;
                Execute();
                return;
            }
            if (EditorApplication.timeSinceStartup < deadline) return;
            checks.Add("FAIL: timeout");
            Finish();
        }

        private static void OnState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode) Finish();
        }

        private static void Execute()
        {
            try
            {
                var go = new GameObject("ItemReviewPlayer");
                PlayerCompositionRoot.EnsureGameplay(go);
                var stats = go.GetComponent<PlayerStats>();
                stats.EnsureInitialized();
                var inventory = go.GetComponent<PlayerItemManager>();
                inventory.PopulateBasicProjectileTemplateItems();
                var effects = go.GetComponent<PlayerItemCombatEffects>();
                Require(inventory.ItemEntries.Count == 68, "catalog contains 68 items");
                float attack = stats.AttackPower, health = stats.MaxHealth;

                Acquire(inventory, "forbidden_growth");
                Equal(attack + 3, stats.AttackPower, "forbidden growth grants useful attack");
                Equal(health - 2, stats.MaxHealth, "forbidden growth health cost");
                Require(inventory.RemoveItem("forbidden_growth"), "remove forbidden growth");
                Equal(attack, stats.AttackPower, "removing curse restores attack");
                Equal(health, stats.MaxHealth, "removing curse restores maximum health");

                float range = stats.AttackRange, move = stats.MoveSpeed;
                Acquire(inventory, "dense_muscle");
                Equal(attack + 1, stats.AttackPower, "dense muscle attack");
                Equal(range, stats.AttackRange, "dense muscle only grants attack");
                Acquire(inventory, "vital_cell");
                Equal(health + 1, stats.MaxHealth, "vital cell grants health");
                Equal(move, stats.MoveSpeed, "vital cell does not grant movement");
                Acquire(inventory, "elongated_fiber");
                Acquire(inventory, "elongated_fiber");
                Equal(range * 1.2f, stats.AttackRange, "new range item stacks additively");
                Require(inventory.RemoveItem("elongated_fiber"), "remove one range stack");
                Equal(range * 1.1f, stats.AttackRange, "removing one range stack preserves the other");
                Acquire(inventory, "active_fiber");
                Acquire(inventory, "active_fiber");
                Equal(move + 2, stats.MoveSpeed, "new movement item stacks");
                Require(inventory.RemoveItem("active_fiber"), "remove one movement stack");
                Equal(move + 1, stats.MoveSpeed, "removing one movement stack preserves the other");
                float attackSpeed = stats.AttackSpeed;
                Acquire(inventory, "swift_cell");
                Equal(attackSpeed * 1.15f, stats.AttackSpeed, "swift cell keeps fifteen percent bonus");
                var saved = inventory.CaptureSavedItems();
                inventory.ClearAllItems();
                Equal(range, stats.AttackRange, "clearing removes new range item");
                Equal(move, stats.MoveSpeed, "clearing removes new movement item");
                inventory.RestoreSavedItems(saved);
                Equal(range * 1.1f, stats.AttackRange, "new range item restores exactly once");
                Equal(move + 1, stats.MoveSpeed, "new movement item restores exactly once");
                inventory.ClearAllItems();

                Acquire(inventory, "double_core"); Acquire(inventory, "triple_core");
                Equal(4, effects.GetForwardProjectileCount(), "multi-core combination adds a fourth shot");
                Equal(2, effects.GetForwardProjectileCount() * effects.GetForwardProjectileDamageMultiplier(4), "combined volley has 200 percent total damage");
                inventory.ClearAllItems();
                Acquire(inventory, "muscle_spasm");
                Equal(1, effects.GetAttackCooldownMultiplier(), "muscle spasm preserves ranged speed");
                Equal(1.25f, effects.GetAttackCooldownMultiplier(true), "muscle spasm melee tradeoff remains");
                inventory.ClearAllItems();

                Acquire(inventory, "blood_contract");
                Equal(1.25f, effects.GetIncomingDamageMultiplier(), "unfinished contract has cost");
                for (int i = 0; i < 80; i++) effects.NotifyEnemyDefeatedByPlayer(null);
                Equal(health + 4, stats.MaxHealth, "contract earns all four health rewards");
                Equal(1, effects.GetIncomingDamageMultiplier(), "completed contract removes cost");
                saved = inventory.CaptureSavedItems();
                inventory.ClearAllItems(); inventory.RestoreSavedItems(saved);
                Equal(1, effects.GetIncomingDamageMultiplier(), "contract completion survives save restoration");
                Equal(health + 4, stats.MaxHealth, "restored contract health does not compound");
                inventory.ClearAllItems();

                stats.Heal(health);
                Acquire(inventory, "overheated_organ");
                float current = stats.CurrentHealth;
                for (int i = 0; i < 10; i++) effects.NotifyBasicAttackPerformed(3, ~0, 8, Vector3.forward);
                Equal(current, stats.CurrentHealth, "overheating cannot damage player");
                Equal(1, effects.GetAttackCooldownMultiplier(), "overheat resets stacks after burst");
                inventory.ClearAllItems();

                Acquire(inventory, "decay_organ");
                var timed = effects.GetType().GetProperty("Timed", Private).GetValue(effects);
                var decay = timed.GetType().GetField("decayRuntime", Private).GetValue(timed);
                decay.GetType().GetMethod("Restore").Invoke(decay, new object[] {
                    new SavedItemStateData { decayOrganElapsedSeconds = 540.1f }, Time.time,
                    timed.GetType().GetProperty("PersistentSettings").GetValue(timed) });
                Invoke(effects, "UpdateDecayOrganState");
                Equal(3, effects.GetOutgoingBasicDamageFlatBonus(), "decay rewards sustained combat with attack +3");
                inventory.ClearAllItems();
                Acquire(inventory, "mutation_rampage");
                for (int i = 0; i < 30; i++)
                {
                    Invoke(effects, "ApplyRandomMutationRampageModifier");
                    Require(stats.AttackPower >= attack && stats.MoveSpeed >= move && stats.AttackSpeed >= 1,
                        "mutation never weakens stats " + i);
                }
                inventory.ClearAllItems();

                var target = Enemy(new Vector3(0, 0, 4));
                var nearby = Enemy(new Vector3(1, 0, 4));
                Acquire(inventory, "explosive_blood_cell");
                effects.ApplyBeamExplosion(target.transform.position, target, 7);
                Equal(93, nearby.Stats.CurrentHealth, "beam explosion damages adjacent target");
                Equal(100, target.Stats.CurrentHealth, "beam explosion does not double-hit primary");
                Acquire(inventory, "beam_organ");
                float baseBeam = effects.BeamDamageMultiplier;
                Acquire(inventory, "piercing_mucus"); Acquire(inventory, "reflux_organ"); Acquire(inventory, "vascular_reflection");
                Equal(baseBeam * 1.55f, effects.BeamDamageMultiplier, "trajectory items improve beam damage");
                Acquire(inventory, "hypertrophy_cell"); Acquire(inventory, "pulse_bullet");
                Equal(.8f * 1.45f * 1.2f, effects.BeamRadius, "beam inherits size effects");
                inventory.ClearAllItems();

                var status = target.GetComponent<EnemyStatusEffectController>();
                status.Initialize(target);
                status.ApplyPoison(4, 1, 10); status.ApplyPoison(3, 1, .5f);
                Equal(10, Get<float>(status, "poisonTickDamage"), "weak infection cannot replace strong poison");
                status.ApplyBleed(3.2f, .8f, 8); status.ApplyBleed(3.2f, .8f, 1);
                Equal(8, Get<float>(status, "bleedTickDamage"), "weak child projectile cannot replace strong bleed");
                status.ResetEffects();

                var puddle = AcidPuddle.Spawn(target.transform.position, .5f, 4, 2, 1);
                float pendingTick = Get<float>(puddle, "nextTickTime");
                for (int i = 0; i < 20; i++)
                    Require(AcidPuddle.Spawn(target.transform.position, .5f, 4, 2, 1) == puddle, "rapid hits refresh existing acid " + i);
                Equal(pendingTick, Get<float>(puddle, "nextTickTime"), "acid refresh preserves pending tick");
                AcidPuddle.Spawn(new Vector3(30, 0, 0), .5f, 4, 2, 1);
                Require(AcidPuddle.Spawn(new Vector3(60, 0, 0), .5f, 4, 2, 1) == null, "third area cannot evict unticked acid");
                Set(puddle, "nextTickTime", Time.time - 1);
                Invoke(puddle, "Update");
                Equal(100f, target.Stats.CurrentHealth, "main health ledger retains the first half-damage tick");
                Set(puddle, "nextTickTime", Time.time - 1);
                Invoke(puddle, "Update");
                Equal(99f, target.Stats.CurrentHealth, "acid ticks accumulate through main health accounting");
                bool? companionAllowsHost = null;
                var originalReaction = EnemyDefeatBindings.PlayerKillReaction;
                try
                {
                    EnemyDefeatBindings.PlayerKillReaction = (enemy, allowHost) => companionAllowsHost = allowHost;
                    var victim = Enemy(new Vector3(0, 0, 8));
                    victim.TakeCompanionDamage(100, HealthEffectSource.Legacy);
                    Require(companionAllowsHost == false, "companion kill keeps credit without host-chain spawning");
                }
                finally { EnemyDefeatBindings.PlayerKillReaction = originalReaction; }
                inventory.ClearAllItems();
                Acquire(inventory, "reflux_organ");
                var returnTarget = Enemy(new Vector3(0, 0, 10));
                var projectileObject = new GameObject("ReviewReturningProjectile");
                projectileObject.transform.position = returnTarget.transform.position;
                var projectile = projectileObject.AddComponent<Projectile>();
                projectile.Launch(Vector3.forward, 3, ~0, 20, effects);
                var handleHit = typeof(Projectile).GetMethod("HandleHit", Private);
                var targetCollider = returnTarget.GetComponent<Collider>();
                handleHit.Invoke(projectile, new object[] { targetCollider });
                Equal(97, returnTarget.Stats.CurrentHealth, "outbound projectile hits normally");
                Get<HashSet<long>>(projectile, "hitEnemyIds").Clear();
                Set(projectile, "returning", true);
                handleHit.Invoke(projectile, new object[] { targetCollider });
                Equal(97, returnTarget.Stats.CurrentHealth, "return pass cannot re-hit within 0.2 seconds");
                var hitTimes = Get<Dictionary<long, float>>(projectile, "lastEnemyHitTimes");
                foreach (long key in new List<long>(hitTimes.Keys)) hitTimes[key] = Time.time - .21f;
                handleHit.Invoke(projectile, new object[] { targetCollider });
                Equal(96, returnTarget.Stats.CurrentHealth, "return hit resumes after cooldown using main health accounting");
                UnityEngine.Object.Destroy(projectileObject);
                passed = true;
            }
            catch (Exception error)
            {
                checks.Add("FAIL: " + error);
                Debug.LogException(error);
            }
            EditorApplication.isPlaying = false;
        }

        private static EnemyController Enemy(Vector3 position)
        {
            var go = new GameObject("ItemReviewEnemy");
            go.transform.position = position;
            var enemy = go.AddComponent<EnemyController>();
            enemy.Configure(null, new EnemySpawnRuleConfig { name = "Review", maxHealth = 100, expReward = 0 }, position, position);
            return enemy;
        }

        private static void Finish()
        {
            EditorApplication.update -= Timeout;
            EditorApplication.playModeStateChanged -= OnState;
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled;
            EditorSettings.enterPlayModeOptions = oldOptions;
            SaveService.ResetStaticStateForTests();
            Directory.CreateDirectory("output/item-review");
            File.WriteAllLines("output/item-review/unity-checks.txt", checks);
            Debug.Log("[ItemReview] " + (passed ? "PASS" : "FAIL") + ": " + checks.Count + " checks");
            EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void Acquire(PlayerItemManager inventory, string id) => Require(inventory.TryAcquireItem(id, out _), "acquire " + id);
        private static void Require(bool value, string label) { if (!value) throw new Exception(label); checks.Add("PASS: " + label); }
        private static void Equal(float expected, float actual, string label) => Require(Mathf.Abs(expected - actual) < .001f, label + " (" + actual + ")");
        private static void Set(object target, string name, object value) => target.GetType().GetField(name, Private).SetValue(target, value);
        private static T Get<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static void Invoke(object target, string name) => target.GetType().GetMethod(name, Private).Invoke(target, null);
    }
}
