using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public enum PollenInvaderPhase { Inactive, Ready, Windup, Recovery, Rotation }

    [DisallowMultipleComponent]
    public sealed class PollenInvaderElitePattern : MonsterPatternController
    {
        private EnemyController enemy;
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private Vector3 anchor;
        private float trigger, stop, windup, spread, speed, duration, radius, recoveryTime, rearm, nextReady, hitUntil;
        private float idleSeconds, moveSeconds, releaseSeconds, pelletSeconds;
        private Sprite[] idle, move, preparation, pellets, rotation;
        private bool followupEnabled;
        private float rotationSeconds, angleOffset;
        private Sprite release, recovery, hit;
        private GameObject shadow;
        public PollenInvaderPhase Phase { get; private set; }
        public Vector3 LockedDirection { get; private set; }
        public Vector3 LaunchPosition { get; private set; }
        public Vector3 OpeningPosition { get; private set; }
        public PollenPellet[] ActivePellets { get; private set; } = new PollenPellet[0];
        public PollenVolley LastVolley { get; private set; }
        public PollenVolley FirstVolley { get; private set; }
        public PollenVolley SecondVolley { get; private set; }
        public PollenPellet[] FirstWavePellets { get; private set; } = new PollenPellet[0];
        public PollenPellet[] SecondWavePellets { get; private set; } = new PollenPellet[0];
        public PollenCycleHitBudget CycleHitBudget { get; private set; }
        public Vector3 SecondLockedDirection { get; private set; }
        public bool FollowupEnabled => followupEnabled;
        public float RotationDuration => rotationSeconds;
        public float SignedRotationOffset { get; private set; }
        public float RotationStartedAt { get; private set; }
        public float SecondReleasedAt { get; private set; }
        public int CycleVolleyCount { get; private set; }
        public int VolleyCount { get; private set; }
        public float WindupDuration => windup;
        public float PelletRadius => radius;
        public float SpreadHalfAngle => spread;
        public float NextReadyTime => nextReady;
        public float ReleasedAt { get; private set; }

        public void Initialize(EnemyController source, PollenInvaderPatternSettings settings)
        {
            EndSpawn(); enemy = source;
            lifetime = source.GetComponent<EnemyPatternLifetime>() ?? source.gameObject.AddComponent<EnemyPatternLifetime>();
            lifetime.Bind(source); generation = source.SpawnGeneration; anchor = transform.position;
            trigger = settings.triggerDistance; stop = settings.approachStopDistance; windup = settings.windupSeconds;
            spread = settings.spreadHalfAngle; speed = settings.pelletSpeed; duration = settings.pelletLifetimeSeconds; radius = settings.pelletRadius;
            followupEnabled = settings.followup.enabled;
            rotationSeconds = followupEnabled ? settings.followup.rotationSeconds : 0;
            angleOffset = followupEnabled ? settings.followup.angleOffset : 0;
            recoveryTime = followupEnabled ? settings.followup.recoverySeconds : settings.recoverySeconds;
            rearm = source.GetRearmCooldown(followupEnabled ? settings.followup.rearmSeconds : settings.rearmSeconds);
            var art = settings.presentation;
            idle = (Sprite[])art.idleFrames.Clone(); move = (Sprite[])art.moveFrames.Clone(); preparation = (Sprite[])art.preparationFrames.Clone();
            pellets = (Sprite[])art.pelletFrames.Clone(); release = art.release; recovery = art.recovery; hit = art.hit;
            rotation = followupEnabled ? (Sprite[])art.rotationFrames.Clone() : new Sprite[0];
            idleSeconds = art.idleFrameSeconds; moveSeconds = art.moveFrameSeconds; releaseSeconds = art.releaseFrameSeconds; pelletSeconds = art.pelletFrameSeconds;
            source.Config.deathSprites = (Sprite[])art.deathFrames.Clone(); source.Config.deathAnimationSpeed = art.deathFrameSeconds;
            source.BindPatternDirections(art.directionalPresentation); source.AlignPatternFeetToGround(); source.SetPatternFrame(idle[0]);
            source.SetAiSuppressed(true); source.SetPatternPositionLocked(false);
            nextReady = Time.time + settings.spawnGraceSeconds; hitUntil = 0; VolleyCount = 0; LastVolley = null; ActivePellets = new PollenPellet[0];
            FirstWavePellets = SecondWavePellets = new PollenPellet[0]; FirstVolley = SecondVolley = null; CycleHitBudget = null; CycleVolleyCount = 0;
            SecondLockedDirection = Vector3.zero; RotationStartedAt = SecondReleasedAt = SignedRotationOffset = 0;
            Phase = PollenInvaderPhase.Ready; source.DamageTaken += OnDamaged; source.Defeated += OnDefeated;
            CreateShadow(art); enabled = true;
        }
        private void Update()
        {
            if (lifetime == null || !lifetime.IsCurrent(generation)) { EndSpawn(); return; }
            if (Phase != PollenInvaderPhase.Ready || IsStunned()) return;
            if (TryBeginAttack()) return;
            var player = PlayerController.Instance;
            bool chase = player != null && !player.IsDead && enemy.IsPlayerInChaseRange() && !enemy.IsOutOfLeash();
            Vector3 delta = (chase ? player.transform.position : anchor) - transform.position; delta.y = 0;
            float distance = chase ? stop : .15f;
            bool moving = delta.magnitude > distance && enemy.Stats.MoveSpeed > 0;
            if (moving) enemy.MoveByExternalPattern(delta.normalized * Mathf.Min(enemy.Stats.MoveSpeed * Time.deltaTime, delta.magnitude - distance));
            if (moving || chase) enemy.SetPatternFacing(delta, false);
            var bank = moving ? move : idle;
            enemy.SetPatternFrame(Time.time < hitUntil ? hit : bank[Mathf.FloorToInt(Time.time / (moving ? moveSeconds : idleSeconds)) % bank.Length]);
        }
        public bool TryBeginAttack()
        {
            if (enemy == null || lifetime == null || !lifetime.IsCurrent(generation) || Phase != PollenInvaderPhase.Ready
                || IsStunned() || Time.time < nextReady || lifetime.OwnedObjectCount > 0) return false;
            var player = PlayerController.Instance;
            if (player == null || player.IsDead || enemy.IsOutOfLeash() || !enemy.IsPlayerInChaseRange()) return false;
            Vector3 target = player.transform.position, delta = target - transform.position; delta.y = 0;
            if (delta.sqrMagnitude < .0001f || delta.sqrMagnitude > trigger * trigger
                || !GasSacOrb.IsVisible(target) || !GasSacOrb.HasClearPath(transform.position, target)) return false;
            enemy.SetPatternFacing(delta, false);
            if (!PollenPellet.TryGetLaunchPosition(enemy, radius, out var start)) return false;
            delta = target - start; delta.y = 0; if (delta.sqrMagnitude < .0001f) return false;
            LockedDirection = delta.normalized; LaunchPosition = start;
            return lifetime.TryStartCycle(FireCycle());
        }
        private IEnumerator FireCycle()
        {
            // Preserve the selected body view; projectile aim was computed from that view's opening.
            enemy.CommitPatternFacing(Vector3.zero, false); enemy.SetPatternPositionLocked(true);
            var damage = enemy.CreatePatternDamage(PollenInvaderPatternSettings.DamageId, 0);
            var secondDamage = followupEnabled ? enemy.CreatePatternDamage(PollenInvaderPatternSettings.FollowupDamageId, 0) : default;
            CycleHitBudget = new PollenCycleHitBudget(); CycleVolleyCount = 0;
            ActivePellets = FirstWavePellets = SecondWavePellets = new PollenPellet[0];
            FirstVolley = SecondVolley = null;
            SecondLockedDirection = Vector3.zero; RotationStartedAt = SecondReleasedAt = 0;
            Phase = PollenInvaderPhase.Windup;
            try
            {
                float elapsed = 0;
                while (elapsed < windup)
                {
                    if (IsStunned()) yield break;
                    enemy.SetPatternFrame(preparation[Mathf.Min(preparation.Length - 1, Mathf.FloorToInt(elapsed / windup * preparation.Length))]);
                    yield return null; elapsed += Time.deltaTime;
                }
                if (!lifetime.IsCurrent(generation) || IsStunned()) yield break;
                bool firstFired = EmitWave(LockedDirection, damage, false);
                bool secondFired = false;
                if (followupEnabled && firstFired)
                {
                    Phase = PollenInvaderPhase.Rotation; RotationStartedAt = Time.time;
                    SignedRotationOffset = enemy.PatternFacing == EnemyFacing.Left ? -angleOffset : angleOffset;
                    SecondLockedDirection = Quaternion.AngleAxis(SignedRotationOffset, Vector3.up) * LockedDirection;
                    elapsed = 0; bool interrupted = false;
                    float releaseHold = Mathf.Min(releaseSeconds, rotationSeconds * .25f);
                    while (elapsed < rotationSeconds)
                    {
                        if (IsStunned()) { interrupted = true; break; }
                        float progress = Mathf.Clamp01((elapsed - releaseHold) / (rotationSeconds - releaseHold));
                        enemy.SetPatternFrame(elapsed < releaseHold ? release : rotation[Mathf.Min(rotation.Length - 1, Mathf.FloorToInt(progress * rotation.Length))]);
                        yield return null; elapsed += Time.deltaTime;
                    }
                    if (!interrupted && lifetime.IsCurrent(generation) && !IsStunned()) secondFired = EmitWave(SecondLockedDirection, secondDamage, true);
                }
                Phase = PollenInvaderPhase.Recovery; elapsed = 0;
                while (elapsed < recoveryTime)
                {
                    Sprite pose = secondFired ? rotation[rotation.Length - 1] : release;
                    enemy.SetPatternFrame(elapsed < releaseSeconds && (!followupEnabled || secondFired) ? pose : recovery);
                    yield return null; elapsed += Time.deltaTime;
                }
            }
            finally
            {
                if (enemy != null && enemy.SpawnGeneration == generation)
                {
                    enemy.ReleasePatternFacing(); enemy.SetPatternPositionLocked(false);
                    if (lifetime.IsCurrent(generation)) { enemy.SetPatternFrame(idle[0]); nextReady = Time.time + rearm; Phase = PollenInvaderPhase.Ready; }
                }
            }
        }
        private bool EmitWave(Vector3 centerDirection, EnemyDamageRequest damage, bool second)
        {
            if (!PollenPellet.TryGetLaunchPosition(enemy, radius, out var start)) return false;
            LaunchPosition = start; OpeningPosition = enemy.GetPatternVisualOrigin(); LastVolley = new PollenVolley(damage, CycleHitBudget);
            var wave = new PollenPellet[3];
            for (int i = 0; i < wave.Length; i++)
            {
                var direction = Quaternion.AngleAxis((i - 1) * spread, Vector3.up) * centerDirection;
                wave[i] = PollenPellet.Launch(enemy, lifetime, generation, LastVolley, start, direction, speed, duration, radius, pellets, pelletSeconds);
            }
            var combined = new PollenPellet[ActivePellets.Length + wave.Length];
            System.Array.Copy(ActivePellets, combined, ActivePellets.Length); System.Array.Copy(wave, 0, combined, ActivePellets.Length, wave.Length); ActivePellets = combined;
            if (second) { SecondWavePellets = wave; SecondVolley = LastVolley; SecondReleasedAt = Time.time; }
            else { FirstWavePellets = wave; FirstVolley = LastVolley; ReleasedAt = Time.time; }
            VolleyCount++; CycleVolleyCount++; return true;
        }
        private bool IsStunned() => enemy.StatusEffects != null && enemy.StatusEffects.IsStunned;
        private void OnDamaged(EnemyController source, float damage) { if (damage > 0 && Phase == PollenInvaderPhase.Ready) hitUntil = Time.time + .12f; }
        private void OnDefeated(EnemyController source) => EndSpawn();
        private void CreateShadow(PollenInvaderPresentation art)
        {
            if (shadow == null) { shadow = new GameObject("Pollen_GroundShadow"); shadow.transform.SetParent(transform, false); shadow.AddComponent<SpriteRenderer>(); }
            var renderer = shadow.GetComponent<SpriteRenderer>(); renderer.sprite = art.groundDisc; renderer.color = art.groundShadowColor; renderer.sortingOrder = 56;
            shadow.transform.rotation = Quaternion.Euler(90, 0, 0); shadow.transform.localScale = new Vector3(art.groundShadowSize.x * .5f, art.groundShadowSize.y * .5f, 1);
            shadow.SetActive(true); LateUpdate();
        }
        private void LateUpdate()
        {
            if (shadow == null || !shadow.activeSelf) return;
            var p = transform.position; if (BiomeManager.Active != null) p.y = BiomeManager.Active.GetGroundHeight(p);
            shadow.transform.position = p + Vector3.up * .035f;
        }
        public override void EndSpawn()
        {
            if (enemy != null) { enemy.DamageTaken -= OnDamaged; enemy.Defeated -= OnDefeated; enemy.ReleasePatternFacing(); enemy.SetPatternPositionLocked(false); }
            lifetime?.Cancel(); if (shadow != null) shadow.SetActive(false);
            ActivePellets = FirstWavePellets = SecondWavePellets = new PollenPellet[0]; Phase = PollenInvaderPhase.Inactive; enabled = false;
        }
        private void OnDisable() { if (Phase != PollenInvaderPhase.Inactive) EndSpawn(); }
    }
}
