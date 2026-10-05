using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public enum DustClumpPhase { Inactive, Ready, Windup, Exposed, Recovery }

    [DisallowMultipleComponent]
    public sealed class DustClumpElitePattern : MonsterPatternController
    {
        private EnemyController enemy;
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private Rigidbody physicsBody;
        private BoxCollider hitbox;
        private SpriteRenderer body, gatherBody;
        private GameObject marker, gather, shadow;
        private DustCloud cloud;
        private Vector3 anchor, coreCenter, launch, direction, fullSize, fullCenter, coreSize, coreColliderCenter;
        private Sprite[] idle, move, preparation, core, death, coreDeath, clouds, dissolve;
        private Sprite release, recovery, hit;
        private float trigger, grace, windup, offset, radius, speed, duration, recoverySeconds, rearm, nextReady, nextSafety, hitUntil;
        private float idleSeconds, moveSeconds, coreSeconds, cloudSeconds, dissolveSeconds, opacity, overlapOpacity;
        private Color dangerColor;
        private Vector2 shadowSize, coreShadowSize;
        private bool coreExposed;
        private Vector3 Position => physicsBody != null ? physicsBody.position : transform.position;
        public DustClumpPhase Phase { get; private set; }
        public bool CoreExposed => coreExposed;
        public Vector3 CoreCenter => coreCenter;
        public Vector3 LockedDirection => direction;
        public Vector3 LaunchPosition => launch;
        public GameObject TelegraphObject => marker;
        public DustCloud ActiveCloud => cloud;
        public float WindupDuration => windup;
        public float CloudRadius => radius;
        public float CloudSpeed => speed;
        public float CloudLifetime => duration;
        public float NextReadyTime => nextReady;
        public int ReleaseCount { get; private set; }
        public int RejectedPaths { get; private set; }
        public float ReleasedAt { get; private set; }

        public void Initialize(EnemyController source, DustClumpPatternSettings settings)
        {
            EndSpawn(); enemy = source; physicsBody = source.GetComponent<Rigidbody>(); hitbox = source.GetComponent<BoxCollider>();
            lifetime = source.GetComponent<EnemyPatternLifetime>() ?? source.gameObject.AddComponent<EnemyPatternLifetime>();
            lifetime.Bind(source); generation = source.SpawnGeneration; anchor = coreCenter = Position;
            fullSize = source.Config.colliderSize; fullCenter = source.Config.colliderCenter;
            trigger = settings.triggerDistance; grace = settings.spawnGraceSeconds; windup = settings.windupSeconds;
            offset = settings.launchOffset; radius = settings.cloudRadius; speed = settings.cloudSpeed; duration = settings.cloudLifetimeSeconds;
            recoverySeconds = settings.recoverySeconds; rearm = source.GetRearmCooldown(settings.rearmSeconds);
            var art = settings.presentation;
            idle = (Sprite[])art.idleFrames.Clone(); move = (Sprite[])art.moveFrames.Clone(); preparation = (Sprite[])art.preparationFrames.Clone();
            core = (Sprite[])art.coreFrames.Clone(); death = (Sprite[])art.deathFrames.Clone(); coreDeath = (Sprite[])art.coreDeathFrames.Clone();
            clouds = (Sprite[])art.cloudFrames.Clone(); dissolve = (Sprite[])art.dissolveFrames.Clone();
            release = art.release; recovery = art.recovery; hit = art.hit;
            idleSeconds = art.idleFrameSeconds; moveSeconds = art.moveFrameSeconds; coreSeconds = art.coreFrameSeconds;
            cloudSeconds = art.cloudFrameSeconds; dissolveSeconds = art.dissolveFrameSeconds; opacity = art.cloudOpacity; overlapOpacity = art.playerOverlapOpacity;
            coreSize = art.coreColliderSize; coreColliderCenter = art.coreColliderCenter; dangerColor = art.dangerFillColor;
            shadowSize = art.groundShadowSize; coreShadowSize = art.coreShadowSize;
            source.Config.deathAnimationSpeed = art.deathFrameSeconds;
            source.BindPatternDirections(null); source.AlignPatternFeetToGround(); source.SetAiSuppressed(true); source.SetPatternPositionLocked(false);
            body = source.transform.Find("Visual").GetComponent<SpriteRenderer>(); SetGeometry(false); SetFrame(idle[0]);
            if (shadow == null) { shadow = new GameObject("Dust_GroundShadow"); shadow.transform.SetParent(transform, false); shadow.AddComponent<SpriteRenderer>(); }
            var sr = shadow.GetComponent<SpriteRenderer>(); sr.sprite = art.groundDisc; sr.color = art.groundShadowColor; sr.sortingOrder = 56;
            shadow.transform.rotation = Quaternion.Euler(90, 0, 0); shadow.SetActive(true); LateUpdate();
            nextReady = Time.time + grace; nextSafety = hitUntil = 0; ReleaseCount = RejectedPaths = 0; ReleasedAt = 0; direction = Vector3.zero;
            Phase = DustClumpPhase.Ready; source.DamageTaken += OnDamaged; source.Defeated += OnDefeated; enabled = true;
        }
        private void Update()
        {
            if (lifetime == null || !lifetime.IsCurrent(generation)) { EndSpawn(); return; }
            if (Phase != DustClumpPhase.Ready || IsStunned()) return;
            if (TryBeginAttack()) return;
            var player = PlayerController.Instance;
            bool chase = player != null && !player.IsDead && enemy.IsPlayerInChaseRange() && !enemy.IsOutOfLeash();
            Vector3 delta = (chase ? player.transform.position : anchor) - Position; delta.y = 0;
            float stop = chase ? trigger * .8f : .15f; bool moving = delta.magnitude > stop && enemy.Stats.MoveSpeed > 0;
            if (moving) enemy.MoveByExternalPattern(delta.normalized * Mathf.Min(enemy.Stats.MoveSpeed * Time.deltaTime, delta.magnitude - stop));
            SetFrame(Time.time < hitUntil ? hit : (moving ? move : idle)[Mathf.FloorToInt(Time.time / (moving ? moveSeconds : idleSeconds)) % 2]);
        }
        public bool TryBeginAttack()
        {
            if (enemy == null || lifetime == null || !lifetime.IsCurrent(generation) || Phase != DustClumpPhase.Ready
                || IsStunned() || Time.time < nextReady || Time.time < nextSafety || cloud != null) return false;
            var player = PlayerController.Instance;
            if (player == null || player.IsDead || enemy.IsOutOfLeash() || !enemy.IsPlayerInChaseRange()) return false;
            Vector3 delta = player.transform.position - Position; delta.y = 0;
            if (delta.sqrMagnitude < .0001f || delta.sqrMagnitude > trigger * trigger
                || !GasSacOrb.IsVisible(Position + Vector3.up * .6f) || !GasSacOrb.HasClearPath(Position, player.transform.position)) return false;
            Vector3 target = Position + delta.normalized * offset;
            if (!DustCloud.CanTravel(Position, target, radius)) { RejectedPaths++; nextSafety = Time.time + .25f; return false; }
            coreCenter = Position; direction = delta.normalized; launch = target;
            if (BiomeManager.Active != null) launch.y = BiomeManager.Active.GetGroundHeight(launch);
            return lifetime.TryStartCycle(Cycle());
        }
        private IEnumerator Cycle()
        {
            uint token = generation; var damage = enemy.CreatePatternDamage(DustClumpPatternSettings.DamageId, 0);
            Phase = DustClumpPhase.Windup; enemy.SetPatternPositionLocked(true);
            try
            {
                marker = new GameObject("P01_FullDangerCircle"); marker.transform.position = launch + Vector3.up * .055f;
                Own(marker); EnemyGroundTelegraph.Circle(marker.transform, radius, dangerColor);
                float began = Time.time;
                while (Time.time - began < windup)
                {
                    if (IsStunned()) yield break;
                    float t = (Time.time - began) / windup;
                    if (t < 1f / 3) SetFrame(preparation[Mathf.Min(2, Mathf.FloorToInt(t * 9))]);
                    else
                    {
                        SetGeometry(true); SetFrame(core[0]);
                        if (gather == null) { gather = new GameObject("P01_GatheringDust"); Own(gather); gatherBody = DustCloud.CreateVisual(gather.transform, clouds, radius, opacity); }
                        gather.transform.position = Vector3.Lerp(coreCenter, launch, Mathf.InverseLerp(1f / 3, 1, t));
                        gatherBody.sprite = clouds[Mathf.FloorToInt(Time.time / cloudSeconds) % 4];
                        var camera = DontStarveCamera.GetActiveCamera(); if (camera != null) gatherBody.transform.rotation = camera.transform.rotation;
                    }
                    yield return null;
                }
                if (IsStunned() || !DustCloud.CanTravel(coreCenter, launch, radius) || !GasSacOrb.IsVisible(launch + Vector3.up * .6f)) yield break;
                Clear(ref gather); gatherBody = null; SetGeometry(true); SetFrame(release);
                cloud = DustCloud.Launch(lifetime, generation, launch, direction, damage, marker, radius, speed, duration,
                    clouds, dissolve, cloudSeconds, dissolveSeconds, opacity, overlapOpacity);
                ReleaseCount++; ReleasedAt = Time.time; Phase = DustClumpPhase.Exposed;
                while (cloud != null)
                {
                    SetFrame(Time.time - ReleasedAt < .1f ? release : core[Mathf.FloorToInt(Time.time / coreSeconds) % 2]);
                    yield return null;
                }
                Clear(ref marker); Phase = DustClumpPhase.Recovery; SetGeometry(false); SetFrame(recovery);
                yield return new WaitForSeconds(recoverySeconds);
            }
            finally
            {
                Clear(ref marker); Clear(ref gather); if (cloud != null) lifetime.ReleaseOwned(cloud.gameObject); cloud = null;
                if (enemy != null) enemy.SetPatternPositionLocked(false);
                if (enemy != null && enemy.SpawnGeneration == token && lifetime.IsCurrent(token))
                { SetGeometry(false); SetFrame(idle[0]); nextReady = Time.time + rearm; Phase = DustClumpPhase.Ready; }
            }
        }
        private void SetGeometry(bool exposed, bool changeDeath = true)
        {
            coreExposed = exposed;
            if (enemy == null || enemy.Config == null) return;
            enemy.Config.colliderSize = exposed ? coreSize : fullSize; enemy.Config.colliderCenter = exposed ? coreColliderCenter : fullCenter;
            if (hitbox != null) { hitbox.size = enemy.Config.colliderSize; hitbox.center = enemy.Config.colliderCenter; }
            if (changeDeath) enemy.Config.deathSprites = exposed ? coreDeath : death;
        }
        private void SetFrame(Sprite frame) { enemy.SetPatternFrame(frame); if (body != null) body.flipX = false; }
        private bool IsStunned() => enemy.StatusEffects != null && enemy.StatusEffects.IsStunned;
        private void OnDamaged(EnemyController source, float damage) { if (damage > 0) hitUntil = Time.time + .12f; }
        private void OnDefeated(EnemyController source) => EndSpawn();
        private void Own(GameObject go) => lifetime.Own(go, item => { item.SetActive(false); Destroy(item); });
        private void Clear(ref GameObject go) { if (go != null && lifetime != null) lifetime.ReleaseOwned(go); go = null; }
        private void LateUpdate()
        {
            if (gatherBody != null) DustCloud.ApplyPlayerOverlapOpacity(gatherBody, opacity, overlapOpacity);
            if (shadow == null || !shadow.activeSelf) return;
            Vector3 point = transform.position; if (BiomeManager.Active != null) point.y = BiomeManager.Active.GetGroundHeight(point);
            shadow.transform.position = point + Vector3.up * .035f;
            var size = coreExposed ? coreShadowSize : shadowSize; shadow.transform.localScale = new Vector3(size.x * .5f, size.y * .5f, 1);
        }
        public override void EndSpawn()
        {
            if (enemy != null) { enemy.DamageTaken -= OnDamaged; enemy.Defeated -= OnDefeated; enemy.SetPatternPositionLocked(false); }
            lifetime?.Cancel(); Clear(ref marker); Clear(ref gather); cloud = null; gatherBody = null;
            SetGeometry(false, false); if (shadow != null) shadow.SetActive(false);
            Phase = DustClumpPhase.Inactive; enabled = false;
        }
        private void OnDisable() { if (Phase != DustClumpPhase.Inactive) EndSpawn(); }
    }
}
