using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public enum InflammationEmberPhase { Inactive, Ready, Windup, Recovery }

    [DisallowMultipleComponent]
    public sealed class InflammationEmberElitePattern : MonsterPatternController
    {
        private EnemyController enemy;
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private Vector3 anchor;
        private float maxDistance, stopDistance, windup, recoverySeconds, rearm, nextReady, hitUntil;
        private float thornSpeed, thornLifetime, thornLength, thornRadius, idleSeconds, moveSeconds, releaseSeconds, thornFrameSeconds;
        private Sprite[] idle, move, preparation, thornFrames;
        private Sprite release, recovery, hit;
        private GameObject shadow;
        public InflammationEmberPhase Phase { get; private set; }
        public Vector3 LockedDirection { get; private set; }
        public EmberThorn ActiveThorn { get; private set; }
        public int ShotCount { get; private set; }
        public float WindupDuration => windup;
        public float RecoveryDuration => recoverySeconds;
        public float NextReadyTime => nextReady;
        public float ThornLength => thornLength;
        public float ThornRadius => thornRadius;

        public void Initialize(EnemyController source, InflammationEmberPatternSettings settings)
        {
            EndSpawn(); enemy = source;
            lifetime = source.GetComponent<EnemyPatternLifetime>() ?? source.gameObject.AddComponent<EnemyPatternLifetime>();
            lifetime.Bind(source); generation = source.SpawnGeneration; anchor = transform.position;
            maxDistance = settings.counterMaxDistance; stopDistance = settings.approachStopDistance;
            windup = settings.windupSeconds; recoverySeconds = settings.recoverySeconds; rearm = source.GetRearmCooldown(settings.rearmSeconds);
            thornSpeed = settings.thornSpeed; thornLifetime = settings.thornLifetimeSeconds;
            thornLength = settings.thornLength; thornRadius = settings.thornRadius;
            var art = settings.presentation;
            idle = (Sprite[])art.idleFrames.Clone(); move = (Sprite[])art.moveFrames.Clone(); preparation = (Sprite[])art.preparationFrames.Clone();
            thornFrames = (Sprite[])art.thornFrames.Clone(); release = art.release; recovery = art.recovery; hit = art.hit;
            idleSeconds = art.idleFrameSeconds; moveSeconds = art.moveFrameSeconds;
            releaseSeconds = art.releaseFrameSeconds; thornFrameSeconds = art.thornFrameSeconds;
            source.Config.deathSprites = (Sprite[])art.deathFrames.Clone(); source.Config.deathAnimationSpeed = art.deathFrameSeconds;
            source.BindPatternDirections(art.directionalPresentation);
            source.AlignPatternFeetToGround(); source.SetPatternFrame(idle[0]); source.SetAiSuppressed(true);
            if (PlayerController.Instance != null) source.SetPatternFacing(PlayerController.Instance.transform.position - transform.position, false);
            CreateShadow(art); Phase = InflammationEmberPhase.Ready; ShotCount = 0; ActiveThorn = null;
            nextReady = Time.time; hitUntil = 0; LockedDirection = Vector3.zero;
            source.DamageTaken += OnDamaged; source.Defeated += OnDefeated; enabled = true;
        }

        private void Update()
        {
            if (lifetime == null || !lifetime.IsCurrent(generation)) { EndSpawn(); return; }
            if (Phase != InflammationEmberPhase.Ready || IsStunned()) return;
            var player = PlayerController.Instance;
            bool chase = player != null && !player.IsDead && enemy.IsPlayerInChaseRange() && !enemy.IsOutOfLeash();
            Vector3 delta = (chase ? player.transform.position : anchor) - transform.position; delta.y = 0;
            float stop = chase ? stopDistance : .15f;
            bool moving = delta.magnitude > stop && enemy.Stats.MoveSpeed > 0;
            if (moving) enemy.MoveByExternalPattern(delta.normalized * Mathf.Min(enemy.Stats.MoveSpeed * Time.deltaTime, delta.magnitude - stop));
            if (moving || chase) enemy.SetPatternFacing(delta, false);
            var frames = moving ? move : idle; float seconds = moving ? moveSeconds : idleSeconds;
            enemy.SetPatternFrame(Time.time < hitUntil ? hit : frames[Mathf.FloorToInt(Time.time / seconds) % frames.Length]);
        }

        private void OnDamaged(EnemyController source, float damage)
        {
            if (damage <= 0 || source.IsDead || lifetime == null || !lifetime.IsCurrent(generation)) return;
            if (Phase != InflammationEmberPhase.Ready) return; // Extra hits never change a committed pose, timer or direction.
            hitUntil = Time.time + .12f;
            var player = PlayerController.Instance;
            if (Time.time < nextReady || ActiveThorn != null || IsStunned() || !CanCounter(player)) return;
            Vector3 direction = player.transform.position - transform.position; direction.y = 0;
            if (direction.sqrMagnitude < .0001f) return;
            LockedDirection = direction.normalized;
            enemy.SetPatternFacing(LockedDirection, false);
            lifetime.TryStartCycle(CounterCycle());
        }

        private bool CanCounter(PlayerController player)
        {
            if (player == null || player.IsDead || enemy.IsOutOfLeash()) return false;
            Vector3 delta = player.transform.position - transform.position; delta.y = 0;
            return delta.sqrMagnitude <= maxDistance * maxDistance
                && GasSacOrb.IsVisible(transform.position + Vector3.up * .6f)
                && GasSacOrb.IsVisible(GasSacOrb.GetFlightPosition(player.transform.position, thornRadius))
                && GasSacOrb.HasClearPath(transform.position, player.transform.position);
        }

        private IEnumerator CounterCycle()
        {
            enemy.CommitPatternFacing(LockedDirection, false);
            var damage = enemy.CreatePatternDamage(InflammationEmberPatternSettings.DamageId, 0);
            Phase = InflammationEmberPhase.Windup;
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
                if (CanCounter(PlayerController.Instance))
                {
                    ActiveThorn = EmberThorn.Launch(enemy, lifetime, generation, LockedDirection, damage,
                        thornSpeed, thornLifetime, thornLength, thornRadius, thornFrames, thornFrameSeconds);
                    if (ActiveThorn != null) ShotCount++;
                }
                Phase = InflammationEmberPhase.Recovery; elapsed = 0;
                while (elapsed < recoverySeconds)
                {
                    enemy.SetPatternFrame(elapsed < releaseSeconds ? release : recovery);
                    yield return null; elapsed += Time.deltaTime;
                }
            }
            finally
            {
                if (enemy != null && enemy.SpawnGeneration == generation) enemy.ReleasePatternFacing();
                if (enemy != null && lifetime.IsCurrent(generation))
                { enemy.SetPatternFrame(idle[0]); nextReady = Time.time + rearm; Phase = InflammationEmberPhase.Ready; }
            }
        }

        private bool IsStunned() => enemy.StatusEffects != null && enemy.StatusEffects.IsStunned;
        private void OnDefeated(EnemyController source) => EndSpawn();
        private void CreateShadow(InflammationEmberPresentation art)
        {
            if (shadow == null)
            { shadow = new GameObject("Ember_GroundShadow"); shadow.transform.SetParent(transform, false); shadow.AddComponent<SpriteRenderer>(); }
            var renderer = shadow.GetComponent<SpriteRenderer>(); renderer.sprite = art.groundDisc; renderer.color = art.groundShadowColor; renderer.sortingOrder = 56;
            shadow.transform.rotation = Quaternion.Euler(90, 0, 0);
            shadow.transform.localScale = new Vector3(art.groundShadowSize.x * .5f, art.groundShadowSize.y * .5f, 1);
            shadow.SetActive(true); LateUpdate();
        }
        private void LateUpdate()
        {
            if (shadow == null || !shadow.activeSelf) return;
            Vector3 position = transform.position;
            if (BiomeManager.Active != null) position.y = BiomeManager.Active.GetGroundHeight(position);
            shadow.transform.position = position + Vector3.up * .035f;
        }
        public override void EndSpawn()
        {
            if (enemy != null) { enemy.DamageTaken -= OnDamaged; enemy.Defeated -= OnDefeated; enemy.ReleasePatternFacing(); }
            lifetime?.Cancel();
            if (shadow != null) shadow.SetActive(false);
            ActiveThorn = null; Phase = InflammationEmberPhase.Inactive; enabled = false;
        }
        private void OnDisable() { if (Phase != InflammationEmberPhase.Inactive) EndSpawn(); }
    }
}
