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

        private static readonly Func<GameObject> CreateImpactFunc = CombatImpactVfx.CreateObject;
        private static readonly Func<GameObject> CreateMeleeArcFunc = MeleeArcVfx.CreateObject;

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

        private static void AddCameraShake(float strength, float duration)
        {
            DontStarveCamera camera = DontStarveCamera.Instance;
            camera?.AddCombatImpulse(strength, duration);
        }
    }
}
