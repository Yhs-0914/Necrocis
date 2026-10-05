using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public sealed partial class GasSacElitePattern
    {
        private bool orbEnabled;
        private float orbExit, orbEnter, orbMax, orbWindup, orbSpeed, orbLifetime, orbRadius, orbRecovery, orbRearm;
        public GasSacAttackKind PreferredAttack { get; private set; }
        public GasSacAttackKind SelectedAttack { get; private set; }
        public GasSacOrb ActiveOrb { get; private set; }
        public Vector3 LockedOrbDirection { get; private set; }
        public int OrbCount { get; private set; }
        public bool OrbEnabled => orbEnabled;
        public float OrbWindupDuration => orbWindup;
        public float OrbSpeed => orbSpeed;
        public float OrbLifetime => orbLifetime;
        public float OrbRecoveryDuration => orbRecovery;

        private void CaptureOrbSettings(GasSacPatternSettings settings)
        {
            orbEnabled = settings.orbEnabled;
            orbExit = settings.orbExitDistance; orbEnter = settings.orbEnterDistance; orbMax = settings.orbMaxDistance;
            orbWindup = settings.orbWindupSeconds; orbSpeed = settings.orbSpeed;
            orbLifetime = settings.orbLifetimeSeconds; orbRadius = settings.orbHitRadius;
            orbRecovery = settings.orbRecoverySeconds; orbRearm = enemy.GetRearmCooldown(settings.orbRearmSeconds);
            PreferredAttack = settings.boundaryPriority; SelectedAttack = GasSacAttackKind.Burst;
            ActiveOrb = null; OrbCount = 0; LockedOrbDirection = Vector3.zero;
        }

        public bool TryBeginAttack()
        {
            if (!orbEnabled) return TryBeginBurst();
            if (enemy == null || lifetime == null || !lifetime.IsCurrent(generation) || Phase != GasSacPhase.Ready
                || (enemy.StatusEffects != null && enemy.StatusEffects.IsStunned)) return false;
            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead || !enemy.IsPlayerInChaseRange() || enemy.IsOutOfLeash()) return false;
            Vector3 delta = player.transform.position - enemy.transform.position; delta.y = 0;
            float distance = delta.magnitude;
            if (distance <= orbExit) PreferredAttack = GasSacAttackKind.Burst;
            else if (distance >= orbEnter) PreferredAttack = GasSacAttackKind.Orb;
            if (PreferredAttack == GasSacAttackKind.Burst) return TryBeginBurst();
            if (Time.time < nextReady || ActiveOrb != null || distance > orbMax) return false;
            enemy.SetPatternFacing(delta);
            if (!CanLaunchOrb(player, delta)) return false;
            LockedOrbDirection = delta.normalized;
            SelectedAttack = GasSacAttackKind.Orb;
            return lifetime.TryStartCycle(OrbCycle());
        }

        private bool CanLaunchOrb(PlayerController player, Vector3 direction)
        {
            float height = orbRadius;
            return player != null && !player.IsDead && enemy.IsPlayerInChaseRange() && !enemy.IsOutOfLeash()
                && GasSacOrb.IsVisible(enemy.transform.position + Vector3.up * .6f)
                && GasSacOrb.IsVisible(GasSacOrb.GetFlightPosition(enemy.transform.position, height))
                && GasSacOrb.IsVisible(GasSacOrb.GetFlightPosition(player.transform.position, height))
                && GasSacOrb.TryGetLaunchPosition(enemy, direction, orbRadius, out _)
                && GasSacOrb.HasClearPath(enemy.transform.position, player.transform.position);
        }

        private IEnumerator OrbCycle()
        {
            EnemyDamageRequest damage = enemy.CreatePatternDamage(GasSacPatternSettings.OrbDamageId, 0);
            Phase = GasSacPhase.OrbWindup;
            enemy.CommitPatternFacing(LockedOrbDirection);
            try
            {
                float elapsed = 0;
                while (elapsed < orbWindup)
                {
                    if (enemy.StatusEffects != null && enemy.StatusEffects.IsStunned) yield break;
                    float t = Mathf.Clamp01(elapsed / orbWindup);
                    // A simple projectile is telegraphed by the body pose, without a ground arrow or range UI.
                    enemy.SetPatternFrame(inflation[Mathf.Min(inflation.Length - 1, Mathf.FloorToInt(t * inflation.Length))]);
                    shadowPoseScale = Mathf.Lerp(1, 1.12f, t);
                    yield return null; elapsed += Time.deltaTime;
                }
                if (!lifetime.IsCurrent(generation) || (enemy.StatusEffects != null && enemy.StatusEffects.IsStunned)) yield break;
                // Leaving the screen during the tell cancels the shot; the recovery/rearm still applies once.
                if (CanLaunchOrb(PlayerController.Instance, LockedOrbDirection))
                {
                    ActiveOrb = GasSacOrb.Launch(enemy, lifetime, generation, LockedOrbDirection, damage,
                        orbSpeed, orbLifetime, orbRadius, puffSprite, filledCircle, gasColor);
                    if (ActiveOrb != null) OrbCount++;
                }
                Phase = GasSacPhase.OrbRecovery;
                enemy.SetPatternFrame(deflated); shadowPoseScale = 1.15f;
                elapsed = 0;
                while (elapsed < orbRecovery) { yield return null; elapsed += Time.deltaTime; }
            }
            finally
            {
                if (enemy != null && enemy.SpawnGeneration == generation) enemy.ReleasePatternFacing();
                if (enemy != null && lifetime.IsCurrent(generation))
                {
                    enemy.SetPatternFrame(idle); shadowPoseScale = 1;
                    nextReady = Time.time + orbRearm;
                    Phase = GasSacPhase.Ready;
                }
            }
        }

    }
}
