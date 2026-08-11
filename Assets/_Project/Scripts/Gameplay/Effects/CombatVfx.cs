using System;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Central, prefab-free combat feedback layer. All spawned objects are pooled and built at runtime,
    /// so the effects work in every biome without scene-by-scene setup.
    /// </summary>
    public static class CombatVfx
    {
        private const string ImpactPoolName = "CombatVfx.Impact";
        private const string MeleeArcPoolName = "CombatVfx.MeleeArc";
        private const string ItemPickupPoolName = "CombatVfx.ItemPickup";
        private const string LevelUpPoolName = "CombatVfx.LevelUp";
        private const string JobChangePoolName = "CombatVfx.JobChange";
        private const string BossEncounterPoolName = "CombatVfx.BossEncounter.V3";

        private static readonly Func<GameObject> CreateImpactFunc = CombatImpactVfx.CreateObject;
        private static readonly Func<GameObject> CreateMeleeArcFunc = MeleeArcVfx.CreateObject;
        private static readonly Func<GameObject> CreateItemPickupFunc = ItemPickupVfx.CreateObject;
        private static readonly Func<GameObject> CreateLevelUpFunc = LevelUpVfx.CreateObject;
        private static readonly Func<GameObject> CreateJobChangeFunc = JobChangeVfx.CreateObject;
        private static readonly Func<GameObject> CreateBossEncounterFunc = BossEncounterVfx.CreateObject;

        private static readonly Color BloodRed = new Color(0.96f, 0.035f, 0.11f, 0.92f);
        private static readonly Color WarmCell = new Color(1f, 0.5f, 0.18f, 0.85f);
        private static readonly Color NecroticPurple = new Color(0.38f, 0.035f, 0.28f, 0.86f);
        private static readonly Color BoneWhite = new Color(1f, 0.93f, 0.72f, 0.95f);

        public static void PlayEnemySpawn(EnemyController enemy)
        {
            if (enemy == null || !TryGetVisualContext(enemy.transform, out Vector3 center, out float scale))
            {
                return;
            }

            if (DontStarveCamera.GetActiveCamera() == null || !IsNearView(center))
            {
                return;
            }

            Vector3 ground = enemy.transform.position + Vector3.up * 0.12f;
            SpawnImpact(
                ground,
                Vector3.zero,
                scale * 0.7f,
                new Color(0.55f, 0.08f, 0.28f, 0.48f),
                new Color(0.94f, 0.3f, 0.34f, 0.55f),
                4,
                3,
                0.28f,
                true,
                0f);
        }

        public static void PlayEnemyHit(EnemyController enemy, float damage, bool lethal)
        {
            if (enemy == null || !TryGetVisualContext(enemy.transform, out Vector3 center, out float targetScale))
            {
                return;
            }

            CombatHitFlash flash = enemy.GetComponent<CombatHitFlash>();
            if (flash == null)
            {
                flash = enemy.gameObject.AddComponent<CombatHitFlash>();
            }
            flash.Flash(lethal ? BoneWhite : new Color(1f, 0.72f, 0.62f, 1f), lethal ? 0.1f : 0.065f);

            if (!IsNearView(center))
            {
                return;
            }

            Vector3 direction = Vector3.zero;
            PlayerController player = PlayerController.Instance;
            if (player != null)
            {
                direction = center - player.transform.position;
                direction.y = 0f;
            }

            float damageScale = Mathf.Clamp(0.82f + Mathf.Log10(1f + Mathf.Max(0f, damage)) * 0.24f, 0.82f, 1.45f);
            float scale = targetScale * damageScale * (lethal ? 1.3f : 0.78f);
            SpawnImpact(
                center,
                direction,
                scale,
                lethal ? BloodRed : new Color(1f, 0.12f, 0.18f, 0.86f),
                lethal ? NecroticPurple : WarmCell,
                lethal ? 18 : 8,
                lethal ? 7 : 3,
                lethal ? 0.46f : 0.25f,
                lethal,
                0.78f);

            AddCameraShake(lethal ? 0.16f : 0.045f, lethal ? 0.22f : 0.09f);
        }

        public static void PlayPlayerHit(Transform player, Vector3 sourcePosition, float damage, bool lethal)
        {
            if (player == null || !TryGetVisualContext(player, out Vector3 center, out float scale))
            {
                return;
            }

            Vector3 direction = center - sourcePosition;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.back;
            }

            SpawnImpact(
                center,
                direction,
                scale * (lethal ? 1.15f : 0.75f),
                BloodRed,
                BoneWhite,
                lethal ? 16 : 7,
                lethal ? 7 : 3,
                lethal ? 0.42f : 0.24f,
                lethal,
                0.82f);

            float damageWeight = Mathf.Clamp01(Mathf.Log10(1f + Mathf.Max(0f, damage)) * 0.35f);
            DamageVignetteOverlay.Pulse(lethal ? 0.42f : Mathf.Lerp(0.16f, 0.25f, damageWeight), lethal ? 0.55f : 0.28f);
            AddCameraShake(lethal ? 0.25f : Mathf.Lerp(0.09f, 0.15f, damageWeight), lethal ? 0.32f : 0.18f);
        }

        public static void PlayMeleeSwing(Vector3 origin, Vector3 direction, float radius)
        {
            GameObject arcObject = RuntimePool.Acquire(MeleeArcPoolName, CreateMeleeArcFunc);
            if (arcObject != null && arcObject.TryGetComponent(out MeleeArcVfx arc))
            {
                arc.Show(origin, direction, radius, 0.17f);
            }
            else
            {
                RuntimePool.Release(arcObject);
            }

            Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            SpawnImpact(
                origin + forward * Mathf.Min(radius * 0.42f, 0.9f) + Vector3.up * 0.35f,
                forward,
                Mathf.Clamp(radius * 0.22f, 0.35f, 0.8f),
                BoneWhite,
                BloodRed,
                5,
                1,
                0.17f,
                false,
                0.92f);
        }

        public static void PlayRangedMuzzle(Vector3 origin, Vector3 direction)
        {
            SpawnImpact(
                origin,
                direction,
                0.48f,
                BoneWhite,
                WarmCell,
                7,
                2,
                0.19f,
                true,
                0.94f);
        }

        public static void PlayProjectileImpact(Vector3 position, Vector3 direction)
        {
            SpawnImpact(
                position,
                direction,
                0.38f,
                BoneWhite,
                new Color(1f, 0.18f, 0.2f, 0.82f),
                5,
                1,
                0.17f,
                false,
                0.9f);
        }

        public static void PlayHostileProjectileImpact(Vector3 position, Vector3 direction)
        {
            SpawnImpact(
                position,
                direction,
                0.52f,
                WarmCell,
                NecroticPurple,
                6,
                2,
                0.2f,
                true,
                0.72f);
        }

        public static void PlayDash(Transform player, SpriteRenderer sourceRenderer, Vector3 direction, float duration)
        {
            if (player == null || sourceRenderer == null)
            {
                return;
            }

            CombatDashTrail trail = player.GetComponent<CombatDashTrail>();
            if (trail == null)
            {
                trail = player.gameObject.AddComponent<CombatDashTrail>();
            }
            trail.Begin(sourceRenderer, direction, duration);

            SpawnImpact(
                player.position + Vector3.up * 0.35f,
                -direction,
                0.6f,
                new Color(1f, 0.72f, 0.62f, 0.7f),
                new Color(0.75f, 0.04f, 0.22f, 0.72f),
                7,
                3,
                0.22f,
                true,
                0.88f);
        }

        public static void PlayItemPickup(
            Vector3 visualPosition,
            Transform collector,
            PlayerItemCategory category)
        {
            if (collector == null || !IsNearView(visualPosition))
            {
                return;
            }

            Color primary = GetItemCategoryColor(category);
            Color accent = Color.Lerp(primary, BoneWhite, 0.72f);
            Vector3 collectorCenter = collector.position + Vector3.up * 0.7f;
            float collectorScale = 1f;
            if (TryGetPlayerPresentationContext(
                    collector,
                    out _,
                    out Vector3 presentationCenter,
                    out float presentationScale))
            {
                collectorCenter = presentationCenter;
                collectorScale = presentationScale;
            }

            GameObject pickupObject = RuntimePool.Acquire(ItemPickupPoolName, CreateItemPickupFunc);
            if (pickupObject != null && pickupObject.TryGetComponent(out ItemPickupVfx pickupVfx))
            {
                pickupVfx.Show(
                    visualPosition,
                    collector,
                    collectorCenter,
                    primary,
                    accent,
                    collectorScale);
            }
            else
            {
                RuntimePool.Release(pickupObject);
            }
        }

        public static void PlayLevelUp(Transform player)
        {
            if (player == null
                || !TryGetPlayerPresentationContext(
                    player,
                    out Vector3 groundPosition,
                    out Vector3 center,
                    out float scale)
                || !IsNearView(center))
            {
                return;
            }

            GameObject levelUpObject = RuntimePool.Acquire(LevelUpPoolName, CreateLevelUpFunc);
            if (levelUpObject != null && levelUpObject.TryGetComponent(out LevelUpVfx levelUpVfx))
            {
                levelUpVfx.Show(player, groundPosition, center, scale);
            }
            else
            {
                RuntimePool.Release(levelUpObject);
            }

        }

        public static void PlayJobChange(Transform player, JobType job)
        {
            if (player == null
                || job == JobType.None
                || !TryGetPlayerPresentationContext(
                    player,
                    out Vector3 groundPosition,
                    out Vector3 center,
                    out float scale)
                || !IsNearView(center))
            {
                return;
            }

            GetJobColors(job, out Color primary, out Color accent);
            GameObject jobChangeObject = RuntimePool.Acquire(
                JobChangePoolName,
                CreateJobChangeFunc);
            if (jobChangeObject != null
                && jobChangeObject.TryGetComponent(out JobChangeVfx jobChangeVfx))
            {
                jobChangeVfx.Show(
                    player,
                    groundPosition,
                    center,
                    scale,
                    job,
                    primary,
                    accent);
            }
            else
            {
                RuntimePool.Release(jobChangeObject);
            }

        }

        public static void PlayBossEncounter(
            EnemyController boss,
            BiomeType biome,
            bool addCameraShake = true)
        {
            if (boss == null || boss.IsDead)
            {
                return;
            }

            if (TryPlayBossEncounterNow(boss, biome, addCameraShake))
            {
                return;
            }

            BossEncounterVfxPending pending = boss.GetComponent<BossEncounterVfxPending>();
            if (pending == null)
            {
                pending = boss.gameObject.AddComponent<BossEncounterVfxPending>();
            }
            pending.Arm(boss, biome, addCameraShake);
        }

        internal static bool TryPlayBossEncounterNow(
            EnemyController boss,
            BiomeType biome,
            bool addCameraShake)
        {
            if (boss == null || boss.IsDead)
            {
                return true;
            }

            if (!TryGetBossPresentationContext(
                    boss.transform,
                    out Vector3 groundPosition,
                    out Vector3 center,
                    out float scale))
            {
                return true;
            }

            if (!IsBossEntranceVisible(center))
            {
                return false;
            }

            GetBossEncounterColors(biome, out Color primary, out Color accent);
            GameObject encounterObject = RuntimePool.Acquire(
                BossEncounterPoolName,
                CreateBossEncounterFunc);
            if (encounterObject != null
                && encounterObject.TryGetComponent(out BossEncounterVfx encounterVfx))
            {
                encounterVfx.Show(
                    boss.transform,
                    groundPosition,
                    center,
                    scale,
                    biome,
                    primary,
                    accent,
                    addCameraShake);
            }
            else
            {
                RuntimePool.Release(encounterObject);
            }

            return true;
        }

        private static void SpawnImpact(
            Vector3 position,
            Vector3 direction,
            float scale,
            Color primary,
            Color secondary,
            int fragments,
            int mist,
            float duration,
            bool ring,
            float directionalBias)
        {
            if (!IsNearView(position))
            {
                return;
            }

            GameObject impactObject = RuntimePool.Acquire(ImpactPoolName, CreateImpactFunc);
            if (impactObject == null || !impactObject.TryGetComponent(out CombatImpactVfx impact))
            {
                RuntimePool.Release(impactObject);
                return;
            }

            impact.Show(
                position,
                direction,
                Mathf.Clamp(scale, 0.08f, 4f),
                primary,
                secondary,
                fragments,
                mist,
                duration,
                ring,
                directionalBias);
        }

        private static bool TryGetVisualContext(Transform target, out Vector3 center, out float scale)
        {
            center = target != null ? target.position + Vector3.up * 0.7f : Vector3.zero;
            scale = 1f;
            if (target == null)
            {
                return false;
            }

            if (TargetAttachedEffect.TryGetTargetBounds(target, out Bounds bounds))
            {
                center = bounds.center;
                float planarSize = Mathf.Max(bounds.size.x, bounds.size.z);
                float verticalSize = bounds.size.y * 0.55f;
                scale = Mathf.Clamp(Mathf.Max(planarSize, verticalSize), 0.65f, 3.2f);
            }
            return true;
        }

        private static bool TryGetPlayerPresentationContext(
            Transform player,
            out Vector3 groundPosition,
            out Vector3 center,
            out float scale)
        {
            groundPosition = player != null
                ? player.position + Vector3.up * 0.035f
                : Vector3.zero;
            center = groundPosition + Vector3.up * 0.7f;
            scale = 1f;

            if (player == null)
            {
                return false;
            }

            SpriteRenderer spriteRenderer = player.GetComponentInChildren<SpriteRenderer>(false);
            if (spriteRenderer == null
                || !spriteRenderer.enabled
                || spriteRenderer.sprite == null)
            {
                if (!TryGetVisualContext(player, out center, out scale))
                {
                    return false;
                }

                scale = Mathf.Clamp(scale * 0.82f, 0.65f, 1.55f);
                return true;
            }

            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            Transform spriteTransform = spriteRenderer.transform;
            Vector3 localBottomLeft = new Vector3(
                spriteBounds.min.x,
                spriteBounds.min.y,
                spriteBounds.center.z);
            Vector3 localBottomRight = new Vector3(
                spriteBounds.max.x,
                spriteBounds.min.y,
                spriteBounds.center.z);
            Vector3 localTopCenter = new Vector3(
                spriteBounds.center.x,
                spriteBounds.max.y,
                spriteBounds.center.z);
            Vector3 localBottomCenter = new Vector3(
                spriteBounds.center.x,
                spriteBounds.min.y,
                spriteBounds.center.z);

            Vector3 worldBottomLeft = spriteTransform.TransformPoint(localBottomLeft);
            Vector3 worldBottomRight = spriteTransform.TransformPoint(localBottomRight);
            Vector3 worldBottomCenter = spriteTransform.TransformPoint(localBottomCenter);
            Vector3 worldTopCenter = spriteTransform.TransformPoint(localTopCenter);
            center = spriteTransform.TransformPoint(spriteBounds.center);

            float visualWidth = Vector3.Distance(worldBottomLeft, worldBottomRight);
            float visualHeight = Vector3.Distance(worldBottomCenter, worldTopCenter);
            scale = Mathf.Clamp(Mathf.Max(visualWidth, visualHeight * 0.55f), 0.65f, 1.55f);

            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera == null)
            {
                groundPosition.x = worldBottomCenter.x;
                groundPosition.z = worldBottomCenter.z;
                return true;
            }

            Vector3 leftScreen = camera.WorldToScreenPoint(worldBottomLeft);
            Vector3 rightScreen = camera.WorldToScreenPoint(worldBottomRight);
            Vector3 footScreen = (leftScreen + rightScreen) * 0.5f;
            Ray footRay = camera.ScreenPointToRay(footScreen);

            if (Mathf.Abs(footRay.direction.y) <= 0.0001f)
            {
                groundPosition.x = worldBottomCenter.x;
                groundPosition.z = worldBottomCenter.z;
                return true;
            }

            float distance = (groundPosition.y - footRay.origin.y) / footRay.direction.y;
            if (distance < 0f)
            {
                groundPosition.x = worldBottomCenter.x;
                groundPosition.z = worldBottomCenter.z;
                return true;
            }

            groundPosition = footRay.GetPoint(distance);
            groundPosition.y = player.position.y + 0.035f;
            return true;
        }

        private static bool TryGetBossPresentationContext(
            Transform boss,
            out Vector3 groundPosition,
            out Vector3 center,
            out float scale)
        {
            groundPosition = boss != null
                ? boss.position + Vector3.up * 0.04f
                : Vector3.zero;
            center = groundPosition + Vector3.up;
            scale = 1.35f;

            if (boss == null)
            {
                return false;
            }

            SpriteRenderer spriteRenderer = boss.GetComponentInChildren<SpriteRenderer>(false);
            if (spriteRenderer == null
                || !spriteRenderer.enabled
                || spriteRenderer.sprite == null)
            {
                if (!TryGetVisualContext(boss, out center, out scale))
                {
                    return false;
                }

                scale = Mathf.Clamp(scale, 0.9f, 3.4f);
                return true;
            }

            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            Transform spriteTransform = spriteRenderer.transform;
            Vector3 localBottomLeft = new Vector3(
                spriteBounds.min.x,
                spriteBounds.min.y,
                spriteBounds.center.z);
            Vector3 localBottomRight = new Vector3(
                spriteBounds.max.x,
                spriteBounds.min.y,
                spriteBounds.center.z);
            Vector3 localTopCenter = new Vector3(
                spriteBounds.center.x,
                spriteBounds.max.y,
                spriteBounds.center.z);
            Vector3 localBottomCenter = new Vector3(
                spriteBounds.center.x,
                spriteBounds.min.y,
                spriteBounds.center.z);

            Vector3 worldBottomLeft = spriteTransform.TransformPoint(localBottomLeft);
            Vector3 worldBottomRight = spriteTransform.TransformPoint(localBottomRight);
            Vector3 worldBottomCenter = spriteTransform.TransformPoint(localBottomCenter);
            Vector3 worldTopCenter = spriteTransform.TransformPoint(localTopCenter);
            center = spriteTransform.TransformPoint(spriteBounds.center);

            float visualWidth = Vector3.Distance(worldBottomLeft, worldBottomRight);
            float visualHeight = Vector3.Distance(worldBottomCenter, worldTopCenter);
            scale = Mathf.Clamp(Mathf.Max(visualWidth, visualHeight * 0.58f), 0.9f, 3.4f);

            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera == null)
            {
                groundPosition.x = worldBottomCenter.x;
                groundPosition.z = worldBottomCenter.z;
                return true;
            }

            Vector3 leftScreen = camera.WorldToScreenPoint(worldBottomLeft);
            Vector3 rightScreen = camera.WorldToScreenPoint(worldBottomRight);
            Vector3 footScreen = (leftScreen + rightScreen) * 0.5f;
            Ray footRay = camera.ScreenPointToRay(footScreen);

            if (Mathf.Abs(footRay.direction.y) <= 0.0001f)
            {
                groundPosition.x = worldBottomCenter.x;
                groundPosition.z = worldBottomCenter.z;
                return true;
            }

            float distance = (groundPosition.y - footRay.origin.y) / footRay.direction.y;
            if (distance < 0f)
            {
                groundPosition.x = worldBottomCenter.x;
                groundPosition.z = worldBottomCenter.z;
                return true;
            }

            groundPosition = footRay.GetPoint(distance);
            groundPosition.y = boss.position.y + 0.04f;
            return true;
        }

        private static void GetJobColors(JobType job, out Color primary, out Color accent)
        {
            switch (job)
            {
                case JobType.Warrior:
                    primary = new Color(0.92f, 0.055f, 0.12f, 0.96f);
                    accent = new Color(1f, 0.58f, 0.12f, 1f);
                    break;
                case JobType.Mage:
                    primary = new Color(0.58f, 0.1f, 0.95f, 0.96f);
                    accent = new Color(0.22f, 0.92f, 1f, 1f);
                    break;
                case JobType.Archer:
                    primary = new Color(0.18f, 0.86f, 0.32f, 0.96f);
                    accent = new Color(1f, 0.84f, 0.2f, 1f);
                    break;
                default:
                    primary = new Color(0.95f, 0.16f, 0.32f, 0.96f);
                    accent = new Color(1f, 0.94f, 0.7f, 1f);
                    break;
            }
        }

        private static void GetBossEncounterColors(
            BiomeType biome,
            out Color primary,
            out Color accent)
        {
            switch (biome)
            {
                case BiomeType.Intestine:
                    primary = new Color(0.34f, 0.18f, 0.07f, 0.96f);
                    accent = new Color(0.68f, 0.9f, 0.3f, 1f);
                    break;
                case BiomeType.Liver:
                    primary = new Color(0.62f, 0.015f, 0.09f, 0.98f);
                    accent = new Color(0.65f, 0.22f, 0.9f, 1f);
                    break;
                case BiomeType.Stomach:
                    primary = new Color(0.46f, 0.9f, 0.08f, 0.96f);
                    accent = new Color(1f, 0.38f, 0.43f, 1f);
                    break;
                case BiomeType.Lung:
                    primary = new Color(0.65f, 0.9f, 1f, 0.94f);
                    accent = new Color(1f, 0.78f, 0.24f, 1f);
                    break;
                default:
                    primary = new Color(0.66f, 0.04f, 0.26f, 0.96f);
                    accent = BoneWhite;
                    break;
            }
        }

        private static bool IsNearView(Vector3 worldPosition)
        {
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera == null)
            {
                return true;
            }

            Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
            return viewport.z > 0f
                && viewport.x >= -0.3f
                && viewport.x <= 1.3f
                && viewport.y >= -0.3f
                && viewport.y <= 1.3f;
        }

        private static bool IsBossEntranceVisible(Vector3 worldPosition)
        {
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera == null)
            {
                return true;
            }

            Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
            return viewport.z > 0f
                && viewport.x >= 0.08f
                && viewport.x <= 0.92f
                && viewport.y >= 0.1f
                && viewport.y <= 0.9f;
        }

        private static Color GetItemCategoryColor(PlayerItemCategory category)
        {
            return category switch
            {
                PlayerItemCategory.AttackStyle => new Color(1f, 0.28f, 0.08f, 0.95f),
                PlayerItemCategory.HighRiskHighReturn => new Color(0.76f, 0.08f, 0.62f, 0.95f),
                PlayerItemCategory.SurvivalDefense => new Color(0.18f, 0.88f, 0.92f, 0.95f),
                PlayerItemCategory.SummonPet => new Color(0.28f, 0.92f, 0.42f, 0.95f),
                PlayerItemCategory.ChainClear => new Color(0.32f, 0.56f, 1f, 0.95f),
                PlayerItemCategory.BossSpecialized => new Color(1f, 0.72f, 0.12f, 0.98f),
                PlayerItemCategory.FunRandom => new Color(1f, 0.24f, 0.78f, 0.95f),
                _ => new Color(0.96f, 0.08f, 0.2f, 0.95f)
            };
        }

        private static void AddCameraShake(float strength, float duration)
        {
            DontStarveCamera camera = DontStarveCamera.Instance;
            camera?.AddCombatImpulse(strength, duration);
        }
    }
}
