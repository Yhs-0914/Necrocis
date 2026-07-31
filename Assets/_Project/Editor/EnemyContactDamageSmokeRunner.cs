using System;
using System.IO;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NecrocisEditor
{
    public static class EnemyContactDamageSmokeRunner
    {
        private const string HubScenePath = "Assets/_Project/Scenes/Hub.unity";
        private const float ExpectedDamage = 1f;
        private const float MinimumKnockback = 0.2f;

        private static string testStorageRoot;
        private static int enteredPlayFrame;
        private static bool regularHitVerified;
        private static bool bossHitVerified;
        private static PlayerController player;
        private static Health health;
        private static EnemyController enemy;
        private static EnemyContactDamage contactDamage;
        private static bool hitPending;
        private static bool pendingBossHit;
        private static int hitFrame;
        private static Vector3 positionBeforeHit;
        private static bool previousEnterPlayModeOptionsEnabled;
        private static EnterPlayModeOptions previousEnterPlayModeOptions;

        public static void Run()
        {
            testStorageRoot = Path.Combine(
                Path.GetTempPath(),
                $"necrocis-contact-damage-smoke-{Guid.NewGuid():N}");
            SaveService.UseStorageRootForTests(testStorageRoot);
            if (!SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error))
            {
                throw new InvalidOperationException($"Normal test save 생성 실패: {error}");
            }

            previousEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            previousEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions =
                previousEnterPlayModeOptions | EnterPlayModeOptions.DisableDomainReload;

            regularHitVerified = false;
            bossHitVerified = false;
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
            EditorSceneManager.OpenScene(HubScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        private static void HandlePlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                enteredPlayFrame = Time.frameCount;
                EditorApplication.update += Tick;
                return;
            }

            if (change != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            RestoreTestState();
            if (!regularHitVerified || !bossHitVerified)
            {
                Debug.LogError("[EnemyContactDamageSmoke] FAIL - 검증 단계가 완료되지 않았습니다.");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log(
                "[EnemyContactDamageSmoke] PASS - regular/boss damage, knockback, invincibility and flash verified");
            EditorApplication.Exit(0);
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || Time.frameCount - enteredPlayFrame < 12)
            {
                return;
            }

            if (player == null)
            {
                if (!TryCreateTestActors())
                {
                    return;
                }
            }

            if (!regularHitVerified)
            {
                if (!hitPending)
                {
                    BeginHitVerification("일반 적", false);
                    return;
                }

                if (TryFinishKnockbackVerification("일반 적"))
                {
                    regularHitVerified = true;
                }
                return;
            }

            if (health.IsInvincible)
            {
                return;
            }

            if (!bossHitVerified)
            {
                if (!hitPending)
                {
                    enemy.SetIgnoreMidBossArenaRestriction(true);
                    BeginHitVerification("보스", true);
                    return;
                }

                if (TryFinishKnockbackVerification("보스"))
                {
                    bossHitVerified = true;
                    EditorApplication.isPlaying = false;
                }
            }
        }

        private static bool TryCreateTestActors()
        {
            player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (player == null || player.HealthComponent == null || player.IsDead)
            {
                return false;
            }

            health = player.HealthComponent;
            EnemySpawnRuleConfig config = new EnemySpawnRuleConfig
            {
                name = "ContactDamageSmokeEnemy",
                maxHealth = 10f,
                attackDamage = 1f,
                enableContactDamage = true,
                contactDamage = ExpectedDamage,
                contactKnockbackDistance = 0.45f,
                addCollider = true,
                isTrigger = true,
                chaseRadius = 0f,
                activationRadius = 0f
            };

            Vector3 spawnPosition = player.transform.position + Vector3.left * 0.1f;
            enemy = EnemyController.Acquire(null, config.name, 984501);
            enemy.Configure(null, config, spawnPosition, spawnPosition);
            enemy.SetAiSuppressed(true);
            contactDamage = enemy.GetComponent<EnemyContactDamage>();
            if (contactDamage == null)
            {
                Fail("공용 EnemyContactDamage가 적에게 연결되지 않았습니다.");
                return false;
            }

            return true;
        }

        private static void BeginHitVerification(string actorLabel, bool bossHit)
        {
            enemy.transform.position = player.transform.position + Vector3.left * 0.1f;
            float healthBefore = health.CurrentHealth;
            positionBeforeHit = player.transform.position;

            if (!contactDamage.TryApplyTo(player))
            {
                Fail($"{actorLabel} 접촉 피해가 적용되지 않았습니다.");
                return;
            }

            float appliedDamage = healthBefore - health.CurrentHealth;
            if (Mathf.Abs(appliedDamage - ExpectedDamage) > 0.001f)
            {
                Fail($"{actorLabel} 접촉 피해가 1이 아닙니다. 실제값: {appliedDamage}");
                return;
            }

            if (!health.IsInvincible)
            {
                Fail($"{actorLabel} 피격 직후 무적 상태가 시작되지 않았습니다.");
                return;
            }

            if (!HasHitFlashColor(player))
            {
                Fail($"{actorLabel} 피격 점멸 색상이 적용되지 않았습니다.");
                return;
            }

            float healthAfterFirstHit = health.CurrentHealth;
            if (contactDamage.TryApplyTo(player)
                || Mathf.Abs(health.CurrentHealth - healthAfterFirstHit) > 0.001f)
            {
                Fail($"{actorLabel} 무적 시간 중 중복 접촉 피해가 발생했습니다.");
                return;
            }

            pendingBossHit = bossHit;
            hitFrame = Time.frameCount;
            hitPending = true;
        }

        private static bool TryFinishKnockbackVerification(string actorLabel)
        {
            if (!hitPending || Time.frameCount <= hitFrame + 2)
            {
                return false;
            }

            float movedDistance = Vector3.Distance(positionBeforeHit, player.transform.position);
            if (movedDistance < MinimumKnockback)
            {
                Fail($"{actorLabel} 접촉 넉백이 적용되지 않았습니다. 실제 이동: {movedDistance}");
                return false;
            }

            bool completedBossHit = pendingBossHit;
            hitPending = false;
            pendingBossHit = false;
            if (completedBossHit && actorLabel != "보스")
            {
                Fail("보스 접촉 검증 단계가 일반 적 단계와 섞였습니다.");
                return false;
            }

            return true;
        }

        private static bool HasHitFlashColor(PlayerController target)
        {
            Color expected = new Color(1f, 0.18f, 0.12f, 1f);
            SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                if (Approximately(renderers[index].color, expected))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Approximately(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.01f
                && Mathf.Abs(a.g - b.g) < 0.01f
                && Mathf.Abs(a.b - b.b) < 0.01f
                && Mathf.Abs(a.a - b.a) < 0.01f;
        }

        private static void Fail(string message)
        {
            Debug.LogError($"[EnemyContactDamageSmoke] FAIL - {message}");
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            RestoreTestState();
            EditorApplication.Exit(1);
        }

        private static void RestoreTestState()
        {
            Time.timeScale = 1f;
            player = null;
            health = null;
            enemy = null;
            contactDamage = null;
            hitPending = false;
            pendingBossHit = false;
            EditorSettings.enterPlayModeOptionsEnabled = previousEnterPlayModeOptionsEnabled;
            EditorSettings.enterPlayModeOptions = previousEnterPlayModeOptions;
            SaveService.ResetStaticStateForTests();
            if (!string.IsNullOrEmpty(testStorageRoot) && Directory.Exists(testStorageRoot))
            {
                Directory.Delete(testStorageRoot, true);
            }
            testStorageRoot = null;
        }
    }
}
