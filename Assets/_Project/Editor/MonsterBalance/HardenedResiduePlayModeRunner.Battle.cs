using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class HardenedResiduePlayModeRunner
    {
        private static readonly float[] ResidueAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
        public static void RunDirectionBattle() => Start(false, false, false, 1);
        public static void RunTerrainChecks() => Start(false, false, false, 2);

        private static void BeginResidueD3()
        {
            field.enabled = false;
            if (enemy != null) enemy.ReleaseToPool();
            enemy = null;
            // New chunks must not introduce unrelated enemies into this single-species combat audit.
            var rules = (System.Collections.IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(biome);
            rules.Clear();
            int released = 0;
            foreach (var spawner in Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None))
            { spawner.ReleaseSpawnedEnemies(); spawner.enabled = false; }
            foreach (var other in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                if (other.gameObject.activeInHierarchy) { other.ReleaseToPool(); released++; }
            // Exercise the real movement/attack methods without live keyboard input changing test positions.
            Invoke(InputManager.Instance, "SetActionsEnabled", false);
            results.Add("INFO isolated runtime normal-spawn rules and keyboard input; released other actors=" + released);
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
        }

        private static bool TryFindResiduePatch(int level, out Vector3 patch)
        {
            for (int z = 7; z < biome.MapHeight - 7; z++) for (int x = 7; x < biome.MapWidth - 7; x++)
            {
                if (!biome.IsWalkable(x, z) || biome.GetHeightLevel(x, z) != level) continue;
                bool clear = true;
                for (int dx = -6; dx <= 6 && clear; dx++) for (int dz = -6; dz <= 6 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == level;
                if (!clear) continue;
                Vector3 candidate = biome.GridToWorldWithHeight(x, z);
                foreach (float angle in ResidueAngles)
                {
                    Vector3 center = candidate + Aim(angle) * settings.placementDistance;
                    if (!ResiduePlacementSafety.CanPlace(center, Vector3.forward, settings.footprint)
                        || !ResiduePlacementSafety.CanPlace(center, Vector3.left, settings.footprint)) { clear = false; break; }
                }
                if (clear) { patch = candidate; return true; }
            }
            patch = default; return false;
        }

        private static IEnumerator LandDirection(float angle)
        {
            Spawn(); MovePlayerTo(origin + Aim(angle) * 2.6f); health.ResetHealth();
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery && pattern.ActiveRubble != null, 4, "D-3 landing " + angle);
        }

        private static Bounds SolidSpriteBounds(Sprite sprite)
        {
            var texture = new Texture2D(2, 2);
            try
            {
                Require(ImageConversion.LoadImage(texture, File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture))), "read approved sprite alpha");
                Color32[] pixels = texture.GetPixels32(); Rect r = sprite.rect;
                int left = (int)r.width, bottom = (int)r.height, right = 0, top = 0;
                for (int y = 0; y < (int)r.height; y++) for (int x = 0; x < (int)r.width; x++)
                    if (pixels[((int)r.y + y) * texture.width + (int)r.x + x].a >= 128)
                    { left = Mathf.Min(left, x); bottom = Mathf.Min(bottom, y); right = Mathf.Max(right, x + 1); top = Mathf.Max(top, y + 1); }
                Require(right > left && top > bottom, "visible sprite alpha exists");
                Vector2 min = (new Vector2(left, bottom) - sprite.pivot) / sprite.pixelsPerUnit;
                Vector2 max = (new Vector2(right, top) - sprite.pivot) / sprite.pixelsPerUnit;
                return new Bounds((min + max) * .5f, max - min);
            }
            finally { Object.DestroyImmediate(texture); }
        }

        private static IEnumerator ResidueBattleChecks()
        {
            BeginResidueD3();
            Require(TryFindResiduePatch(0, out origin), "wide real-map battle patch");
            results.Add("INFO real map battle origin=" + origin + "; only movement/attack-path probes temporarily extend prop lifetime");
            var art = settings.presentation; var directions = art.directionalPresentation.Capture();
            Bounds chipBounds = SolidSpriteBounds(art.airborneChip);
            Bounds sideBounds = SolidSpriteBounds(art.rubble), verticalBounds = SolidSpriteBounds(art.verticalRubble);
            foreach (float angle in ResidueAngles)
            {
                Vector3 aim = Aim(angle); Spawn(); MovePlayerTo(origin + aim * 2.6f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 4, "eight-direction tell");
                EnemyFacing facing = enemy.PatternFacing;
                Vector3 center = pattern.LandingCenter, stopped = enemy.transform.position;
                bool vertical = facing == EnemyFacing.Front || facing == EnemyFacing.Back;
                Equal(0, (center - (origin + aim * settings.placementDistance)).magnitude, "diagonal landing center retains exact aim");
                ElitePresentationChecks.RedArea(pattern.TelegraphObject,
                    vertical ? new Vector2(settings.footprint.y, settings.footprint.x) : settings.footprint);
                enemy.SetPatternFacing(-aim); MovePlayerTo(center); float hp = health.CurrentHealth;
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Dropping, 3, "eight-direction release");
                var slab = pattern.ActiveRubble; var visual = slab.transform.Find("CrustSlab").GetComponent<SpriteRenderer>();
                Require(enemy.PatternFacing == facing && enemy.IsPatternFacingLocked, "committed diagonal facing");
                Equal(0, (pattern.LaunchPosition - enemy.GetPatternVisualOrigin()).magnitude, "chip starts at selected body origin");
                Require(visual.sprite == art.airborneChip && visual.GetComponentsInChildren<LineRenderer>().Length == 0,
                    "isolated chip has no projectile floor UI");
                float minHeight = float.PositiveInfinity, maxHeight = float.NegativeInfinity, previousT = -1;
                Vector3 end = slab.LandingVisualPosition, flight = end - pattern.LaunchPosition;
                while (pattern.Phase == HardenedResiduePhase.Dropping)
                {
                    Vector3 delta = visual.transform.position - pattern.LaunchPosition;
                    float t = Vector3.Dot(delta, flight) / flight.sqrMagnitude;
                    Require(t >= previousT - .001f && t <= 1.001f && Vector3.Cross(delta, flight).magnitude < .002f,
                        "continuous flight without sideways jump or reversal");
                    previousT = t;
                    float ground = biome.GetGroundHeight(visual.transform.position);
                    foreach (float x in new[] { chipBounds.min.x, chipBounds.max.x }) foreach (float y in new[] { chipBounds.min.y, chipBounds.max.y })
                    {
                        float h = visual.transform.TransformPoint(new Vector3(x, y, 0)).y - ground;
                        minHeight = Mathf.Min(minHeight, h); maxHeight = Mathf.Max(maxHeight, h);
                        Require(h >= -.003f, "visible chip pixels never penetrate the floor");
                    }
                    yield return null;
                }
                Require(pattern.Phase == HardenedResiduePhase.Recovery && pattern.LandingCount == 1 && slab.IsBlocking, "one landed prop");
                Equal(0, (enemy.transform.position - stopped).magnitude, "owner stays still during lift/drop");
                Equal(0, (visual.transform.position - end).magnitude, "chip endpoint and grounded prop pivot join");
                Equal(biome.GetGroundHeight(center), visual.transform.position.y, "landed pivot lies on actual ground");
                Require(visual.sprite == (vertical ? art.verticalRubble : art.rubble), "four-direction body selects matching prop axis");
                Bounds groundedBounds = vertical ? verticalBounds : sideBounds;
                float opaqueBase = visual.transform.TransformPoint(new Vector3(groundedBounds.center.x, groundedBounds.min.y, 0)).y;
                Require(Mathf.Abs(opaqueBase - biome.GetGroundHeight(center)) < .02f, "visible landed base touches ground within a pixel margin");
                Equal(3, hp - health.CurrentHealth, "central landing damage once");
                yield return new WaitForSeconds(.22f); Equal(3, hp - health.CurrentHealth, "landed obstacle has no contact damage");
                var collider = slab.GetComponent<BoxCollider>();
                Equal(vertical ? .7f : 2.2f, collider.bounds.size.x, "actual world collider x follows tell");
                Equal(vertical ? 2.2f : .7f, collider.bounds.size.z, "actual world collider z follows tell");
                if (Mathf.Abs(aim.z) > .1f)
                    Require(aim.z > 0 ? visual.sortingOrder < DirectionBody.sortingOrder : visual.sortingOrder > DirectionBody.sortingOrder,
                        "diagonal/front/back occlusion agrees with ground order");
                enemy.TakeDamage(10000);
                Require(!slab.IsBlocking && !slab.gameObject.activeSelf && ResidueRubble.ActiveBlockCount == 0, "death opens path immediately");
                var seen = new HashSet<Sprite>(); var body = DirectionBody; float until = Time.time + 2;
                while (enemy.gameObject.activeSelf && Time.time < until) { seen.Add(body.sprite); yield return null; }
                Require(!enemy.gameObject.activeSelf && art.deathFrames.All(f => seen.Contains(directions.Resolve(f, facing))), "six selected-direction death frames before pool return");
                Pass($"CYCLE {angle:0}deg/{facing}: fixed tell/axis, joined visible chip flight {minHeight:F3}..{maxHeight:F3}m, grounded visible base, correct occlusion, damage 3 once, immediate unblock + 6-frame death");
            }

            foreach (float angle in new[] { 0f, 90f, 45f, 135f, 225f, 315f }) foreach (bool dash in new[] { false, true })
            {
                Spawn(); MovePlayerTo(origin + Aim(angle) * 2.6f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 4, "actual dodge tell");
                Vector3 center = pattern.LandingCenter, escape = pattern.LandingDirection;
                MovePlayerTo(center); float hp = health.CurrentHealth;
                if (dash)
                {
                    Face(escape); player.StartCoroutine((IEnumerator)Invoke(player, "DashCoroutine"));
                    yield return new WaitForSeconds(.3f);
                }
                else yield return WalkTo(center + escape * 1.45f, .7f);
                Require(Vector3.Dot(player.transform.position - center, escape) > 1.1f, "native dodge leaves marked area");
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 2, "dodged impact");
                Require(!player.IsDashInvincible, "dodge result is measured after dash invincibility ends");
                Equal(hp, health.CurrentHealth, "actual dodge avoids landing damage");
                Pass($"DODGE {angle:0}deg: actual {(dash ? "dash" : "walking")} exits tell; invincibility ended before impact, damage 0");
            }

            Set(settings, "rubbleLifetime", 10);
            foreach (float angle in new[] { 0f, 90f })
            {
                yield return LandDirection(angle); var slab = pattern.ActiveRubble;
                Vector3 center = slab.transform.position, normal = pattern.LandingDirection;
                Vector3 along = Vector3.Cross(Vector3.up, normal);
                Vector2 feet = player.GetComponent<ProceduralTerrainMotor>().TerrainHalfExtents;
                float normalFeet = Mathf.Abs(normal.x) * feet.x + Mathf.Abs(normal.z) * feet.y;
                float alongFeet = Mathf.Abs(along.x) * feet.x + Mathf.Abs(along.z) * feet.y;
                float distance = .35f + normalFeet + .25f, detour = 1.1f + alongFeet + .3f;
                MovePlayerTo(center + normal * distance);
                yield return WalkFor(-normal, .3f);
                Require(Vector3.Dot(player.transform.position - center, normal) >= .35f + normalFeet - .04f, "actual walking respects footprint plus feet");
                MovePlayerTo(center + normal * distance); Face(-normal);
                player.StartCoroutine((IEnumerator)Invoke(player, "DashCoroutine")); yield return new WaitForSeconds(.35f);
                Require(slab.IsBlocking && Vector3.Dot(player.transform.position - center, normal) >= .35f + normalFeet - .04f, "actual dash cannot tunnel through slab");
                MovePlayerTo(center); yield return WalkTo(center + normal * distance, 1);
                foreach (float sign in new[] { -1f, 1f })
                {
                    MovePlayerTo(center + normal * distance);
                    yield return WalkTo(center + normal * distance + along * (sign * detour), 1.5f);
                    yield return WalkTo(center - normal * distance + along * (sign * detour), 1.5f);
                    Require(slab.IsBlocking, "native detour finishes before obstacle expires");
                }
                float hp = health.CurrentHealth;
                slab.Release(); MovePlayerTo(center + normal * distance);
                yield return WalkTo(center - normal * distance, 1.5f);
                Equal(hp, health.CurrentHealth, "obstacle movement adds no damage");
                Pass($"MOVEMENT axis={angle:0}: walking/dash blocked, overlap escape, both side detours, immediate direct-route restoration; actual physics frames");
            }

            foreach (float angle in ResidueAngles)
            {
                Vector3 aim = Aim(angle); yield return LandDirection(angle); var slab = pattern.ActiveRubble;
                enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
                Vector3 center = pattern.LandingCenter; MovePlayerTo(center + aim * 3); Face(-aim);
                float hp = enemy.Stats.CurrentHealth;
                FireBasic(center + aim * 2, -aim, 2);
                yield return Wait(() => enemy.Stats.CurrentHealth < hp, 2, "basic projectile continues behind prop");
                Require(slab == null || !slab.IsBlocking, "basic projectile breaks prop");
                Equal(2, hp - enemy.Stats.CurrentHealth, "basic projectile not consumed by prop");
                Pass($"BASIC {angle:0}deg: real bullet prefab breaks rubble, continues to owner, damage 2 once");
            }
            foreach (float angle in new[] { 0f, 90f, 180f, 270f })
            {
                Vector3 aim = Aim(angle); yield return LandDirection(angle); var slab = pattern.ActiveRubble;
                enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
                Vector3 center = pattern.LandingCenter; MovePlayerTo(center + aim * 1.3f); Face(-aim); Physics.SyncTransforms();
                Invoke(player.GetComponent<PlayerAttack>(), "MeleeAttack");
                Require(!slab.IsBlocking, "native Q melee destroys directional rubble");
                yield return LandDirection(angle); slab = pattern.ActiveRubble;
                enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true); center = pattern.LandingCenter;
                MovePlayerTo(center + aim * 3); float hp = enemy.Stats.CurrentHealth;
                var shot = new GameObject("I02_D3_SkillProjectile"); projectiles.Add(shot);
                shot.transform.position = center + aim * 2 + Vector3.up * .6f;
                shot.AddComponent<SkillProjectile>().Launch(-aim, 2, 8, 2, ~0, true, default);
                yield return Wait(() => enemy.Stats.CurrentHealth < hp, 2, "skill continues behind prop");
                Require(slab == null || !slab.IsBlocking, "skill breaks directional rubble"); Equal(2, hp - enemy.Stats.CurrentHealth, "skill damage reaches owner");
                Pass($"ATTACK {angle:0}deg: native melee + skill projectile break the prop; skill continues through it");
            }
            if (enemy != null) enemy.ReleaseToPool(); RestoreAssets();
            foreach (float angle in new[] { 0f, 90f })
            {
                yield return LandDirection(angle); var slab = pattern.ActiveRubble; MovePlayerTo(origin + Aim(angle) * 6);
                yield return Wait(() => slab == null, 3, "default two-second obstacle expires");
                Require(ResidueRubble.ActiveBlockCount == 0, "expiry leaves no movement block");
                Pass($"EXPIRY axis={angle:0}: default lifetime removes visual/collider and restores walking queries");
            }
            foreach (HardenedResiduePhase phase in new[] { HardenedResiduePhase.Windup, HardenedResiduePhase.Dropping, HardenedResiduePhase.Recovery })
            {
                Spawn(); MovePlayerTo(origin + Aim(135) * 2.6f);
                yield return Wait(() => pattern.Phase == phase, 4, "pool release phase " + phase);
                var life = enemy.GetComponent<EnemyPatternLifetime>(); var tell = pattern.TelegraphObject; var prop = pattern.ActiveRubble;
                enemy.ReleaseToPool();
                Require(life.OwnedObjectCount == 0 && ResidueRubble.ActiveBlockCount == 0
                    && (tell == null || !tell.activeSelf) && (prop == null || !prop.gameObject.activeSelf), "release immediately deactivates all owned objects");
                Spawn(); yield return null;
                Require(pattern.LandingCount == 0 && pattern.ActiveRubble == null && !enemy.IsPatternFacingLocked, "fresh generation has no delayed landing or direction lock");
                Pass("POOL " + phase + ": immediate tell/prop cleanup and clean spawn generation");
            }
            Spawn(); enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, enemy.Balance.Current.MoveSpeed);
            Vector3 before = enemy.transform.position; MovePlayerTo(origin + Aim(45) * 4.2f);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 5, "default-speed diagonal approach");
            Require(Vector3.Distance(before, enemy.transform.position) > .5f, "default enemy speed closes actual distance");
            Vector3 stationary = enemy.transform.position; yield return new WaitForSeconds(.3f);
            Equal(0, Vector3.Distance(stationary, enemy.transform.position), "committed windup stops normal movement");
            Pass("APPROACH: default-speed diagonal chase enters attack range and stops for the committed tell");
        }
    }
}
