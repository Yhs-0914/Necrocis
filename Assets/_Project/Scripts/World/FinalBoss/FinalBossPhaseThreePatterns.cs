using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public sealed partial class FinalBossPhaseThreeController
    {
        public enum MobilePattern { None, DashRain, GrabThrow, TentacleCombo, HomingSwarm, Reflection, CellBombs }
        [SerializeField, Min(10f)] private float collapseDuration = 45f;
        [SerializeField, Min(.5f)] private float patternRecovery = 1.8f;
        private bool attacking;
        private bool reflectingAppearance;
        private float chargeEnd, chargeDuration;
        private Transform effectRoot;
        private bool collapseStarted;
        private float collapseElapsed, nextBoundaryHit, nextTimerUpdate;
        private float nextImpactAccent;
        private Rect safeArea;
        private LineRenderer boundaryVisual;
        private Material boundaryMaterial;
        private readonly FinalBossTelegraph[] collapsedFloor = new FinalBossTelegraph[4];
        private FinalBossNeuralVfx neuralBoundary;
        private PlayerController grabbedPlayer;
        private Vector3 grabSafePosition;
        private string patternLabel = "CORE EXPOSED", patternHint = "ATTACK THE MOVING CORE";
        private static readonly Color WarningColor = new Color(1f, .12f, .24f, .48f);
        private static readonly Color ImpactColor = new Color(.35f, .9f, 1f, .6f);
        public MobilePattern CurrentMobilePattern { get; private set; }
        public bool IsTimeAttack => collapseStarted;
        public float TimeRemaining => Mathf.Max(0f, collapseDuration - collapseElapsed);
        private Transform EffectRoot => effectRoot;
        private FinalBossPhaseTwoController Owner => GetComponent<FinalBossPhaseTwoController>();

        private IEnumerator PatternLoop()
        {
            yield return new WaitForSeconds(1f);
            var bag = new List<MobilePattern>();
            MobilePattern previous = MobilePattern.None;
            while (IsActive)
            {
                if (bag.Count == 0)
                {
                    for (int i = 1; i <= 6; i++) bag.Add((MobilePattern)i);
                    for (int i = bag.Count - 1; i > 0; i--)
                    {
                        int j = Random.Range(0, i + 1);
                        MobilePattern swap = bag[i]; bag[i] = bag[j]; bag[j] = swap;
                    }
                    if (bag[0] == previous) { MobilePattern swap = bag[0]; bag[0] = bag[1]; bag[1] = swap; }
                }
                CurrentMobilePattern = bag[0]; bag.RemoveAt(0); previous = CurrentMobilePattern;
                attacking = true;
                switch (CurrentMobilePattern)
                {
                    case MobilePattern.DashRain: yield return DashRain(); break;
                    case MobilePattern.GrabThrow: yield return GrabThrow(); break;
                    case MobilePattern.TentacleCombo: yield return TentacleCombo(); break;
                    case MobilePattern.HomingSwarm: yield return HomingSwarm(); break;
                    case MobilePattern.Reflection: yield return Reflection(); break;
                    case MobilePattern.CellBombs: yield return CellBombs(); break;
                }
                // Prevent unavoidable overlaps with the next grab or tentacle combo.
                while (IsActive && FinalBossHomingProjectile.ActiveCount > 0) yield return null;
                attacking = false;
                CurrentMobilePattern = MobilePattern.None;
                Announce("CORE EXPOSED", "ATTACK NOW / KEEP YOUR DISTANCE");
                yield return new WaitForSeconds(collapseStarted ? 1.1f : patternRecovery);
            }
        }

        private void Announce(string label, string hint)
        {
            patternLabel = label; patternHint = hint;
            Owner?.SetMobilePattern(label, TimerHint(), 2f);
        }

        private string TimerHint() => collapseStarted
            ? $"{(TimeRemaining > 0f ? Mathf.CeilToInt(TimeRemaining) + "s" : "OVERTIME")}  /  {patternHint}"
            : patternHint;

        private WaitForSeconds Windup(float seconds)
        {
            chargeDuration = seconds; chargeEnd = Time.time + seconds;
            return new WaitForSeconds(seconds);
        }

        private Vector3 PlayerPosition => PlayerController.Instance != null ? PlayerController.Instance.transform.position : Position;
        private static Vector3 Flat(Vector3 value) { value.y = 0f; return value; }
        private Vector3 DirectionToPlayer() => Flat(PlayerPosition - Position).sqrMagnitude > .001f
            ? Flat(PlayerPosition - Position).normalized : Vector3.back;
        private Vector3 Floor(Vector3 value) { value.y = GroundHeight(value) + .06f; return value; }

        private void PoseTentacle(FinalBossTentacleVisual limb, Vector3 origin, Vector3 direction,
            float reach, float width, float curl, float energy, float alpha = 1f)
        {
            if (limb == null) return;
            limb.Pose(origin + Vector3.up * 1.9f, Floor(origin + direction * reach) + Vector3.up * .6f,
                width, curl, energy, alpha);
        }

        private IEnumerator PrepareTentacle(FinalBossTentacleVisual limb, Vector3 origin,
            Vector3 direction, float width, float seconds)
        {
            Windup(seconds);
            for (float elapsed = 0f; elapsed < seconds; elapsed += Time.deltaTime)
            {
                float t = Mathf.Clamp01(elapsed / seconds);
                PoseTentacle(limb, origin, direction, Mathf.Lerp(2.8f, 1.4f, t), width,
                    Mathf.Lerp(1.2f, 2f, t), t * .65f, Mathf.Min(1f, t * 5f));
                yield return null;
            }
        }
        private FinalBossTelegraph Circle(Vector3 center, float radius, float duration)
        {
            var warning = FinalBossTelegraph.CreateCircle(EffectRoot, Floor(center), radius, duration + .15f, WarningColor);
            warning.UseNeuralStyle(duration);
            return warning;
        }
        private FinalBossTelegraph Lane(Vector3 origin, Vector3 direction, float length, float width, float duration)
        {
            var warning = FinalBossTelegraph.CreateRectangle(EffectRoot, Floor(origin + direction * length * .5f),
                new Vector2(width, length), duration + .15f, WarningColor);
            warning.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            warning.UseNeuralStyle(duration);
            return warning;
        }

        private IEnumerator DashRain()
        {
            Announce("NEURAL RUSH", "LEAVE THE DASH LANE / WATCH THE SKY");
            Vector3 origin = Position, direction = DirectionToPlayer();
            const float distance = 12f, windup = 1.15f;
            var lane = Lane(origin, direction, distance, 3.2f, windup);
            yield return Windup(windup);
            lane.Dismiss();
            AudioManager.Instance?.PlaySFX("IntestineBossImpact", .7f);
            bool hit = false;
            float nextEcho = 0f;
            bodyLean = -Mathf.Sign(Vector3.Dot(direction, Vector3.right)) * 12f;
            for (float t = 0f; t < .55f; t += Time.deltaTime)
            {
                Vector3 before = Position;
                if (!TryMove(before, direction * (distance / .55f) * Time.deltaTime)) break;
                if (t >= nextEcho) { StartCoroutine(DashEcho()); nextEcho = t + .075f; }
                PlayerController player = PlayerController.Instance;
                if (!hit && player != null && SegmentDistance(player.transform.position, before, Position) < 1.8f)
                { player.TakeDamage(2f); hit = true; }
                yield return null;
            }
            bodyLean = 0f;
            DontStarveCamera.Instance?.AddCombatImpulse(.2f, .18f);
            for (int wave = 0; wave < 3; wave++)
            {
                Vector3 target = PlayerPosition;
                for (int i = 0; i < 7; i++)
                {
                    float angle = i * Mathf.PI * 2f / 7f + wave * .35f;
                    Vector3 point = i == 0 ? target : target + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (3f + wave);
                    StartCoroutine(FallingCell(point, 1.35f, 1f, false));
                }
                yield return new WaitForSeconds(.65f);
            }
            yield return new WaitForSeconds(1.8f);
        }

        private IEnumerator GrabThrow()
        {
            Announce("SYNAPTIC GRASP", "STEP OUT OF THE NARROW CLAW LANE");
            Vector3 origin = Position, direction = DirectionToPlayer();
            var warning = Lane(origin, direction, 9f, 2.4f, 1.3f);
            FinalBossTentacleVisual claw = FinalBossTentacleVisual.Create(EffectRoot);
            yield return PrepareTentacle(claw, origin, direction, 2.1f, 1.3f);
            warning.Dismiss();
            bool attempted = false;
            for (float t = 0f; t < .3f; t += Time.deltaTime)
            {
                float length = Mathf.Lerp(1.4f, 9f, 1f - Mathf.Pow(1f - Mathf.Clamp01(t / .22f), 3f));
                PoseTentacle(claw, origin, direction, length, 2.1f, .3f, 1f);
                PlayerController player = PlayerController.Instance;
                if (!attempted && player != null && InLane(player.transform.position, origin, direction, length, 1.2f))
                {
                    attempted = true;
                    if (player.BeginBossDisplacement())
                    { grabbedPlayer = player; grabSafePosition = player.transform.position; break; }
                }
                yield return null;
            }
            if (grabbedPlayer != null)
            {
                Announce("CAUGHT", "THE CLAW IS THROWING YOU BACK");
                Vector3 start = grabSafePosition;
                for (float t = 0f; t < .3f && grabbedPlayer != null && !grabbedPlayer.IsDead; t += Time.deltaTime)
                {
                    Vector3 held = start + Vector3.up * (Mathf.Sin(t / .3f * Mathf.PI * .5f) * 2f);
                    grabbedPlayer.SetBossDisplacementPosition(held);
                    claw.Pose(origin + Vector3.up * 1.9f, held + Vector3.up * .5f, 2.1f, 1f, 1f);
                    yield return null;
                }
                Vector3 landing = start;
                for (float d = .4f; d <= 7f; d += .4f)
                {
                    Vector3 next = start + direction * d;
                    if (!arena.CanTraverse(landing, next, new Vector2(.7f, .5f))
                        || !CanCrossBoundary(landing, next, new Vector2(.7f, .5f))) break;
                    landing = next;
                }
                var landingWarning = Circle(landing, 1.5f, .7f);
                for (float t = 0f; t < .65f && grabbedPlayer != null && !grabbedPlayer.IsDead; t += Time.deltaTime)
                {
                    float n = Mathf.Clamp01(t / .65f);
                    Vector3 pos = Vector3.Lerp(start, landing, n);
                    pos.y = GroundHeight(pos) + Mathf.Lerp(2f, 0f, n) + Mathf.Sin(n * Mathf.PI) * 2f;
                    grabbedPlayer.SetBossDisplacementPosition(pos);
                    PoseTentacle(claw, origin, direction, Mathf.Lerp(Flat(start - origin).magnitude, 1f, Mathf.SmoothStep(0f, 1f, n)),
                        2.1f, Mathf.Sin(n * Mathf.PI) * -1.2f, 1f - n, 1f - n);
                    yield return null;
                }
                landingWarning.Dismiss();
                if (grabbedPlayer != null && !grabbedPlayer.IsDead)
                {
                    landing.y = GroundHeight(landing);
                    grabbedPlayer.SetBossDisplacementPosition(landing);
                    grabSafePosition = landing;
                    grabbedPlayer.TakeDamage(2f);
                    Impact(landing, 1.5f);
                }
                ReleaseGrab();
            }
            else
            {
                for (float t = 0f; t < .35f; t += Time.deltaTime)
                {
                    float n = t / .35f;
                    PoseTentacle(claw, origin, direction, Mathf.Lerp(9f, .7f, Mathf.SmoothStep(0f, 1f, n)), 2.1f, n, .5f, 1f - n);
                    yield return null;
                }
            }
            if (claw != null) Destroy(claw.gameObject);
        }

        private void ReleaseGrab()
        {
            if (grabbedPlayer == null) return;
            // Coroutine cancellation must never leave an airborne, input-locked player behind.
            Vector3 position = grabSafePosition; position.y = GroundHeight(position);
            grabbedPlayer.SetBossDisplacementPosition(position);
            grabbedPlayer.EndBossDisplacement();
            grabbedPlayer = null;
        }

        private IEnumerator TentacleCombo()
        {
            Announce("TENTACLE I / PIERCE", "SIDESTEP THE STRAIGHT THRUST");
            yield return Thrust(DirectionToPlayer(), 11f, 2.4f, .95f);
            yield return new WaitForSeconds(.35f);
            Announce("TENTACLE II / SPLIT", "MOVE INTO THE GAPS BETWEEN THE CLAWS");
            Vector3 direction = DirectionToPlayer(), origin = Position;
            var warnings = new List<FinalBossTelegraph>();
            var directions = new List<Vector3>();
            var limbs = new List<FinalBossTentacleVisual>();
            for (int i = -2; i <= 2; i++)
            {
                Vector3 dir = Quaternion.AngleAxis(i * 27f, Vector3.up) * direction;
                directions.Add(dir); warnings.Add(Lane(origin, dir, 9f, 1.6f, 1f));
                limbs.Add(FinalBossTentacleVisual.Create(EffectRoot));
            }
            Windup(1f);
            for (float t = 0f; t < 1f; t += Time.deltaTime)
            {
                for (int i = 0; i < limbs.Count; i++)
                    PoseTentacle(limbs[i], origin, directions[i], Mathf.Lerp(2.8f, 1.4f, t), 1.6f,
                        (i - 2) * .45f, t * .65f, Mathf.Min(1f, t * 5f));
                yield return null;
            }
            foreach (var warning in warnings) warning.Dismiss();
            for (int i = 0; i < directions.Count; i++) StartCoroutine(AnimateThrust(origin, directions[i], 9f, 1.6f, limbs[i]));
            yield return new WaitForSeconds(.75f);
            Announce("TENTACLE III / REAP", "GET OUTSIDE THE CIRCULAR SWEEP");
            origin = Position;
            var circle = Circle(origin, 7.5f, 1.2f);
            FinalBossTentacleVisual sweep = FinalBossTentacleVisual.Create(EffectRoot);
            yield return PrepareTentacle(sweep, origin, Vector3.right, 1.8f, 1.2f);
            circle.Dismiss();
            var wake = FinalBossNeuralVfx.Create(EffectRoot, Floor(origin), FinalBossNeuralVfx.Shape.Sweep,
                7.5f, new Color(.22f, .8f, 1f, .8f));
            bool hit = false;
            for (float t = 0f; t < .85f; t += Time.deltaTime)
            {
                Vector3 dir = Quaternion.AngleAxis(360f * t / .85f, Vector3.up) * Vector3.right;
                PoseTentacle(sweep, origin, dir, 7.5f, 1.8f, -1.1f, 1f);
                wake.SetSweep(-360f * t / .85f, 70f);
                PlayerController player = PlayerController.Instance;
                if (!hit && player != null && InLane(player.transform.position, origin, dir, 7.5f, .9f))
                { player.TakeDamage(2f); hit = true; }
                yield return null;
            }
            Destroy(wake.gameObject);
            for (float t = 0f; t < .28f; t += Time.deltaTime)
            {
                float n = t / .28f;
                PoseTentacle(sweep, origin, Vector3.right, Mathf.Lerp(7.5f, .8f, n), 1.8f, n * 1.5f, 1f - n, 1f - n);
                yield return null;
            }
            if (sweep != null) Destroy(sweep.gameObject);
        }

        private IEnumerator Thrust(Vector3 direction, float length, float width, float windup)
        {
            Vector3 origin = Position;
            var warning = Lane(origin, direction, length, width, windup);
            var limb = FinalBossTentacleVisual.Create(EffectRoot);
            yield return PrepareTentacle(limb, origin, direction, width, windup);
            warning.Dismiss();
            yield return AnimateThrust(origin, direction, length, width, limb);
        }

        private IEnumerator AnimateThrust(Vector3 origin, Vector3 direction, float length, float width,
            FinalBossTentacleVisual prepared = null)
        {
            FinalBossTentacleVisual sprite = prepared != null ? prepared : FinalBossTentacleVisual.Create(EffectRoot);
            bool hit = false;
            for (float t = 0f; t < .62f; t += Time.deltaTime)
            {
                float extension = t < .16f ? 1f - Mathf.Pow(1f - t / .16f, 3f)
                    : t < .28f ? 1f : 1f - Mathf.SmoothStep(0f, 1f, (t - .28f) / .34f);
                float reach = Mathf.Lerp(.8f, length, extension);
                PoseTentacle(sprite, origin, direction, reach, width, (1f - extension) * 1.6f,
                    extension, Mathf.Min(1f, (.62f - t) * 8f));
                PlayerController player = PlayerController.Instance;
                if (!hit && t < .28f && player != null && InLane(player.transform.position, origin, direction, reach, width * .5f))
                { player.TakeDamage(2f); hit = true; }
                yield return null;
            }
            if (sprite != null) Destroy(sprite.gameObject);
        }

        private IEnumerator HomingSwarm()
        {
            Announce("SYNAPTIC SWARM", "A LARGE SWARM IS FORMING / KEEP MOVING");
            var warning = Circle(Position, 4f, 1.15f);
            var ports = new FinalBossNeuralVfx[6];
            for (int i = 0; i < ports.Length; i++)
            {
                Vector3 port = Position + Quaternion.Euler(0f, i * 60f, 0f) * Vector3.forward * 3.1f + Vector3.up * 1.6f;
                ports[i] = FinalBossNeuralVfx.Create(EffectRoot, port, FinalBossNeuralVfx.Shape.Halo, .55f,
                    new Color(.24f, .85f, 1f, .85f));
            }
            yield return Windup(1.15f);
            warning.Dismiss();
            for (int wave = 0; wave < 3; wave++)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector3 direction = Quaternion.AngleAxis(i / 2 * 60f + (i % 2 == 0 ? -10f : 10f) + wave * 6f,
                        Vector3.up) * Vector3.forward;
                    FinalBossHomingProjectile.Launch(EffectRoot, ports[i / 2].transform.position,
                        direction, 1f, 5.2f, 48f, 4.5f, "FinalBoss/PhaseThree/HomingNeuralCell");
                }
                AudioManager.Instance?.PlaySFX("LiverBloodThrow", .5f);
                yield return new WaitForSeconds(.65f);
            }
            foreach (var port in ports) if (port != null) Destroy(port.gameObject);
        }

        private IEnumerator Reflection()
        {
            Announce("REFLECTION INCOMING", "STOP FIRING WHEN THE CYAN SHIELD CLOSES");
            var warning = Circle(Position, 4f, 1.2f);
            yield return Windup(1.2f);
            warning.Dismiss();
            var shield = FinalBossNeuralVfx.Create(EffectRoot, bossRenderer.bounds.center,
                FinalBossNeuralVfx.Shape.Shield, 4.65f, new Color(.22f, .78f, 1f, .85f));
            Owner?.SetMobileReflection(true);
            reflectingAppearance = true;
            Announce("REFLECTION ACTIVE", "HOLD FIRE / THE CORE IS REFLECTING DAMAGE");
            yield return new WaitForSeconds(3f);
            Owner?.SetMobileReflection(false);
            reflectingAppearance = false;
            if (shield != null) Destroy(shield.gameObject);
        }

        private IEnumerator CellBombs()
        {
            Announce("CELLULAR APOCALYPSE", "WATCH THE MARKED LANDING ZONES");
            for (int wave = 0; wave < 4; wave++)
            {
                Vector3 target = PlayerPosition;
                StartCoroutine(FallingCell(target, 2.1f, 1.25f, true));
                for (int i = 0; i < 6; i++)
                {
                    float angle = i * Mathf.PI / 3f + wave * .45f;
                    Vector3 point = target + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * Random.Range(4f, 8f);
                    if (arena.IsWalkable(point, Vector2.one * .3f)) StartCoroutine(FallingCell(point, 1.8f, 1.25f, true));
                }
                yield return new WaitForSeconds(.8f);
            }
            yield return new WaitForSeconds(1.8f);
        }

        private IEnumerator FallingCell(Vector3 point, float radius, float windup, bool bomb)
        {
            point = Floor(point);
            var warning = Circle(point, radius, windup);
            yield return new WaitForSeconds(windup);
            warning.Dismiss();
            SpriteRenderer sprite = AttackSprite(bomb ? "CellBomb" : "HomingNeuralCell", point, Vector3.right, radius);
            var fallTrail = sprite != null ? AddEnergyTrail(sprite.transform, .16f, .28f) : null;
            if (sprite != null)
            {
                Camera camera = DontStarveCamera.GetActiveCamera();
                if (camera != null) sprite.transform.rotation = camera.transform.rotation * Quaternion.Euler(0f, 0f, bomb ? 0f : -90f);
            }
            for (float t = 0f; t < .4f; t += Time.deltaTime)
            {
                if (sprite != null)
                {
                    float n = Mathf.Clamp01(t / .4f);
                    sprite.transform.position = point + Vector3.up * (12f * (1f - n * n));
                    if (fallTrail != null && t <= Time.deltaTime) fallTrail.Clear();
                }
                yield return null;
            }
            if (sprite != null) Destroy(sprite.gameObject);
            PlayerController player = PlayerController.Instance;
            if (player != null && Flat(player.transform.position - point).magnitude <= radius) player.TakeDamage(bomb ? 2f : 1f);
            Impact(point, radius);
        }

        private SpriteRenderer AttackSprite(string name, Vector3 position, Vector3 direction, float width)
        {
            Sprite sprite = Resources.Load<Sprite>("FinalBoss/PhaseThree/" + name)
                ?? Resources.Load<Sprite>("FinalBoss/PhaseTwo/" + name);
            if (sprite == null) return null;
            var go = new GameObject("PhaseIII_" + name);
            go.transform.SetParent(EffectRoot, true);
            go.transform.position = Floor(position) + Vector3.up * .2f;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.sortingOrder = 5050;
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera != null) go.transform.rotation = camera.transform.rotation;
            go.transform.localScale = Vector3.one * (width / Mathf.Max(.01f, sprite.bounds.size.x));
            return renderer;
        }

        private void Impact(Vector3 point, float radius)
        {
            FinalBossNeuralVfx.Create(EffectRoot, Floor(point), FinalBossNeuralVfx.Shape.Burst,
                radius, new Color(.24f, .76f, 1f, .9f), .52f);
            if (Time.time >= nextImpactAccent)
            {
                nextImpactAccent = Time.time + .12f;
                DontStarveCamera.Instance?.AddCombatImpulse(.08f, .1f);
                AudioManager.Instance?.PlaySFX("StomachImpact", .4f);
            }
        }

        internal static TrailRenderer AddEnergyTrail(Transform parent, float width, float duration)
        {
            var go = new GameObject("NeuralFlightTrail"); go.transform.SetParent(parent, false);
            var trail = go.AddComponent<TrailRenderer>();
            trail.sharedMaterial = FinalBossNeuralVfx.EnergyMaterial;
            trail.time = duration; trail.minVertexDistance = .1f;
            trail.startWidth = width; trail.endWidth = 0f;
            trail.startColor = new Color(.4f, .94f, 1f, .6f);
            trail.endColor = new Color(.15f, .3f, .65f, 0f);
            trail.sortingOrder = 5044; trail.emitting = true;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return trail;
        }

        private IEnumerator DashEcho()
        {
            var go = new GameObject("NeuralRushAfterimage"); go.transform.SetParent(EffectRoot, false);
            go.transform.SetPositionAndRotation(bossVisual.position, bossVisual.rotation);
            go.transform.localScale = bossVisual.lossyScale;
            var echo = go.AddComponent<SpriteRenderer>(); echo.sprite = bossRenderer.sprite;
            echo.sharedMaterial = bossRenderer.sharedMaterial; echo.flipX = bossRenderer.flipX;
            echo.sortingOrder = bossRenderer.sortingOrder - 1;
            for (float t = 0f; t < .24f; t += Time.deltaTime)
            { echo.color = new Color(.3f, .72f, 1f, .19f * (1f - t / .24f)); yield return null; }
            Destroy(go);
        }

        private static bool InLane(Vector3 point, Vector3 origin, Vector3 direction, float length, float halfWidth)
        {
            Vector3 delta = Flat(point - origin);
            float along = Vector3.Dot(delta, direction);
            return along >= 0f && along <= length && (delta - direction * along).magnitude <= halfWidth;
        }

        private static float SegmentDistance(Vector3 point, Vector3 start, Vector3 end)
        {
            Vector3 delta = Flat(end - start);
            float t = delta.sqrMagnitude > .0001f ? Mathf.Clamp01(Vector3.Dot(Flat(point - start), delta) / delta.sqrMagnitude) : 0f;
            return Flat(point - start - delta * t).magnitude;
        }

        private void UpdateCollapse()
        {
            if (!collapseStarted && damageTarget.Stats.HealthNormalized <= .1f)
            {
                collapseStarted = true; collapseElapsed = 0f;
                AudioManager.Instance?.PlaySFX("BossPhaseChange", .8f);
                var go = new GameObject("PhaseIII_CollapsingNeuralBoundary");
                go.transform.SetParent(EffectRoot, false);
                boundaryVisual = go.AddComponent<LineRenderer>();
                boundaryMaterial = new Material(Shader.Find("Sprites/Default"));
                boundaryVisual.sharedMaterial = boundaryMaterial;
                boundaryVisual.useWorldSpace = true; boundaryVisual.loop = true;
                boundaryVisual.positionCount = 4; boundaryVisual.widthMultiplier = .45f;
                boundaryVisual.sortingOrder = 5090;
                boundaryVisual.startColor = boundaryVisual.endColor = new Color(.3f, 1f, .92f);
                for (int i = 0; i < 4; i++)
                {
                    collapsedFloor[i] = FinalBossTelegraph.CreateRectangle(boundaryVisual.transform, Floor(Position),
                        Vector2.one, 3600f, new Color(.06f, .01f, .13f, .86f), true);
                }
                neuralBoundary = FinalBossNeuralVfx.Create(boundaryVisual.transform, Vector3.zero,
                    FinalBossNeuralVfx.Shape.Boundary, 1f, ImpactColor);
                boundaryVisual.enabled = false;
                Announce("TERMINAL COLLAPSE", "THE ARENA IS SHRINKING / FINISH THE CORE");
            }
            if (!collapseStarted) return;
            collapseElapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(collapseElapsed / collapseDuration));
            Vector2 center = arena.WorldSize * .5f;
            Vector2 half = Vector2.Lerp(new Vector2(arena.WorldSize.x * .5f - 5f, arena.WorldSize.y * .5f - 7f), new Vector2(7f, 6f), t);
            safeArea = new Rect(center - half, half * 2f);
            SetCollapsedFloor(0, new Rect(0f, 0f, safeArea.xMin, arena.WorldSize.y));
            SetCollapsedFloor(1, new Rect(safeArea.xMax, 0f, arena.WorldSize.x - safeArea.xMax, arena.WorldSize.y));
            SetCollapsedFloor(2, new Rect(safeArea.xMin, 0f, safeArea.width, safeArea.yMin));
            SetCollapsedFloor(3, new Rect(safeArea.xMin, safeArea.yMax, safeArea.width, arena.WorldSize.y - safeArea.yMax));
            if (neuralBoundary != null) neuralBoundary.SetBoundary(safeArea);
            if (boundaryVisual != null)
            {
                boundaryVisual.SetPositions(new[] { Floor(new Vector3(safeArea.xMin, 0f, safeArea.yMin)),
                    Floor(new Vector3(safeArea.xMin, 0f, safeArea.yMax)), Floor(new Vector3(safeArea.xMax, 0f, safeArea.yMax)),
                    Floor(new Vector3(safeArea.xMax, 0f, safeArea.yMin)) });
            }
            if (Time.time >= nextTimerUpdate)
            {
                nextTimerUpdate = Time.time + 1f;
                Owner?.SetMobilePattern(patternLabel, TimerHint(), 1.2f);
            }
            PlayerController player = PlayerController.Instance;
            if (player != null && Time.time >= nextBoundaryHit
                && (TimeRemaining <= 0f || !safeArea.Contains(new Vector2(player.transform.position.x, player.transform.position.z))))
            { nextBoundaryHit = Time.time + 1f; player.TakeDamage(TimeRemaining <= 0f ? 2f : 1f); }
            // The shrinking edge may overtake the boss too; gently move it into the playable interior.
            Vector3 target = Position;
            target.x = Mathf.Clamp(target.x, safeArea.xMin + footprint.x, safeArea.xMax - footprint.x);
            target.z = Mathf.Clamp(target.z, safeArea.yMin + footprint.y, safeArea.yMax - footprint.y);
            Vector3 step = Flat(target - Position);
            if (!attacking && step.sqrMagnitude > .001f) TryMove(Position, Vector3.ClampMagnitude(step, 4f * Time.deltaTime));
        }

        private void SetCollapsedFloor(int index, Rect area)
        {
            if (collapsedFloor[index] == null) return;
            collapsedFloor[index].transform.position = Floor(new Vector3(area.center.x, 0f, area.center.y));
            collapsedFloor[index].transform.localScale = new Vector3(area.width, 1f, area.height);
        }

        public bool CanCrossBoundary(Vector3 from, Vector3 to, Vector2 halfExtents)
        {
            if (!IsActive || !collapseStarted) return true;
            return CanTraverseSafeArea(safeArea, from, to, halfExtents);
        }

        public static bool CanTraverseSafeArea(Rect area, Vector3 from, Vector3 to, Vector2 halfExtents)
        {
            // Allow an overtaken player to escape inward; never trap them outside the boundary.
            float oldOverflow = BoundaryOverflow(area, from, halfExtents);
            float newOverflow = BoundaryOverflow(area, to, halfExtents);
            return newOverflow <= .001f || newOverflow <= oldOverflow + .00001f;
        }

        private static float BoundaryOverflow(Rect safeArea, Vector3 point, Vector2 extents)
        {
            return Mathf.Max(0f, safeArea.xMin + extents.x - point.x) + Mathf.Max(0f, point.x + extents.x - safeArea.xMax)
                + Mathf.Max(0f, safeArea.yMin + extents.y - point.z) + Mathf.Max(0f, point.z + extents.y - safeArea.yMax);
        }
    }
}
