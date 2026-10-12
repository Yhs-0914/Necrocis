using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class HardenedResiduePlayModeRunner
    {
        private static IEnumerator Checks()
        {
            yield return SetupField();
            if (stage3Mode == 1) { yield return ResidueBattleChecks(); yield break; }
            if (stage3Mode == 2) { yield return ResidueTerrainChecks(); yield break; }
            if (directionChecks) { yield return DirectionChecks(); yield break; }
            if (preview)
            {
                MovePlayer(2.6f);
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 4, "preview lift");
                if (deathPreview)
                {
                    enemy.SuppressExperienceReward = true; enemy.TakeDamage(10000);
                    yield return new WaitForSeconds(settings.presentation.deathFrameSeconds * 2.1f);
                }
                else yield return new WaitForSeconds(settings.windupSeconds * .5f);
                yield break;
            }
            Pass("isolated Intestine field attaches I-02 at zero kills with authored -2m player ground offset; production registration is preserved");
            field.enabled = false; enemy = null;
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);

            Spawn(); MovePlayer(3.5f); yield return null;
            var facingBody = enemy.transform.Find("Visual").GetComponent<SpriteRenderer>();
            Require(facingBody.flipX, "residue faces right while ready");
            MovePlayer(-3.5f); yield return null;
            Require(!facingBody.flipX, "residue faces left while ready");
            MovePlayer(-2.6f);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 3, "left residue tell");
            Require(!facingBody.flipX && pattern.LandingCenter.x < origin.x, "left mirrored attack locks a left landing point");
            ElitePresentationChecks.RedArea(pattern.TelegraphObject, settings.footprint);
            MovePlayer(2.6f); yield return null;
            Require(!facingBody.flipX, "residue does not turn in the middle of preparation");
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 3, "left residue recovery");
            Require(!facingBody.flipX, "residue keeps left-facing recovery");
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Ready, 2, "residue facing ready"); yield return null;
            Require(facingBody.flipX, "residue turns right after recovery");
            Pass("residue mirrors both directions, locks facing through the committed attack, then turns after recovery; full translucent red landing background");

            Spawn(); MovePlayer(2.6f); health.ResetHealth();
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 3, "first tell");
            Require(enemy.transform.Find("Visual").GetComponent<SpriteRenderer>().flipX, "left-authored body faces the player to its right when committing the attack");
            Vector3 lockedCenter = pattern.LandingCenter, lockedActor = enemy.transform.position;
            var boundary = pattern.TelegraphObject.transform.Find("LandingBoundary").GetComponent<LineRenderer>();
            Equal(settings.footprint.x, boundary.GetPosition(1).x - boundary.GetPosition(0).x, "displayed landing width");
            Equal(settings.footprint.y, boundary.GetPosition(2).z - boundary.GetPosition(1).z, "displayed landing depth");
            ElitePresentationChecks.RedArea(pattern.TelegraphObject, settings.footprint);
            MovePlayerTo(lockedCenter); float hp = health.CurrentHealth;
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 3, "first impact");
            Equal(3, hp - health.CurrentHealth, "attack 2 × landing coefficient 1.5 = one 3 HP hit");
            Equal(0, Vector3.Distance(lockedActor, enemy.transform.position), "no self locomotion during attack");
            Equal(0, Vector3.Distance(lockedCenter, pattern.LandingCenter), "landing target stays locked");
            Require(pattern.LandingCount == 1 && pattern.TelegraphObject == null && pattern.ActiveRubble.IsBlocking, "one visible slab after one landing");
            Require(pattern.ActiveRubble.GetComponent<EnemyController>() == null, "rubble is not an enemy or auto-aim target");
            health.ResetHealth(); enemy.TakeDamage(5); yield return new WaitForSeconds(.2f);
            Equal(60, health.CurrentHealth, "no lingering or contact damage from rubble");
            Require(enemy.Balance.ContactEnabled, "body contact remains enabled; rubble itself has no contact damage");
            Pass("fixed .8s crack/lift → landing damage 3 once → 1s vulnerable recovery; visible rectangle matches footprint, no lingering damage");

            var slab = pattern.ActiveRubble; Vector3 center = slab.transform.position;
            yield return Wait(() => slab == null, 3, "natural two-second expiry");
            Require(ResidueRubble.ActiveBlockCount == 0 && ResidueRubble.CanTraverse(center + Vector3.forward * 1.4f,
                center - Vector3.forward * 1.4f, player.GetComponent<ProceduralTerrainMotor>().TerrainHalfExtents), "expiry leaves a clear direct route");
            Pass("default slab expires after 2s and unregisters its movement blocking");

            foreach (bool widthEdge in new[] { true, false }) foreach (float margin in new[] { -.02f, .02f })
            {
                Spawn(); MovePlayer(2.6f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 3, "landing boundary tell");
                Vector3 axis = widthEdge ? Vector3.right : Vector3.forward;
                float half = (widthEdge ? settings.footprint.x : settings.footprint.y) * .5f;
                MovePlayerTo(pattern.LandingCenter + axis * (half + margin)); hp = health.CurrentHealth;
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 2, "landing boundary hit");
                Equal(margin < 0 ? 3 : 0, hp - health.CurrentHealth, "landing rectangle edge matches actual damage");
            }
            Spawn(); MovePlayer(2.6f); health.ResetHealth();
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 3, "walk dodge tell");
            MovePlayerTo(pattern.LandingCenter); hp = health.CurrentHealth;
            yield return WalkTo(pattern.LandingCenter + Vector3.forward * 1.2f, 1);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 2, "walk dodge landing");
            Equal(hp, health.CurrentHealth, "walking out during .8s tell avoids landing damage");
            Pass("landing width/depth edges: .02m inside hits once, .02m outside misses; actual walking during tell avoids damage");

            // Move over real physics frames, as gameplay does. MovePosition is not synchronous within one editor coroutine tick.
            Set(settings, "rubbleLifetime", 6f);
            yield return Land(); slab = pattern.ActiveRubble; center = slab.transform.position;
            MovePlayerTo(center);
            yield return WalkTo(center + Vector3.forward * 1.4f, 2);
            Require(slab.IsBlocking, "player can escape an overlapping landing while slab is still active");
            MovePlayerTo(center + Vector3.forward * 1.4f);
            yield return WalkFor(Vector3.back, .35f);
            Require(player.transform.position.z > center.z + .7f, "walking cannot cross the slab");
            MovePlayerTo(center + Vector3.forward * 1.4f);
            yield return WalkTo(center + Vector3.forward * 1.4f + Vector3.right * 2.2f, 2);
            yield return WalkTo(center - Vector3.forward * 1.4f + Vector3.right * 2.2f, 2);
            Require(player.transform.position.z < center.z - 1f, "detour reaches the other side");
            Require(slab.IsBlocking, "detour succeeds while obstacle is still active");
            slab.Release(); MovePlayerTo(center + Vector3.forward * 1.4f);
            yield return WalkTo(center - Vector3.forward * 1.4f, 2);
            Require(ResidueRubble.ActiveBlockCount == 0, "removed slab leaves a clear walking route");
            Pass("actual physics-frame walking: blocks direct crossing, permits escape from underneath, permits side detour, removal restores direct walking");

            yield return Land(); center = pattern.LandingCenter;
            MovePlayerTo(center + Vector3.forward * 1.4f); Face(Vector3.back);
            player.StartCoroutine((IEnumerator)Invoke(player, "DashCoroutine"));
            yield return new WaitForSeconds(.4f);
            Require(player.transform.position.z > center.z + .7f && pattern.ActiveRubble != null, "actual dash cannot tunnel across rubble");
            Pass("actual player dash uses the same movement blocker and cannot pass through the slab");

            // Long enough to inspect native attack paths; these temporary settings are restored before the real reward check.
            Set(settings, "rubbleLifetime", 4f);
            yield return Land(); slab = pattern.ActiveRubble; center = pattern.LandingCenter;
            enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            MovePlayerTo(center + Vector3.right * 1.3f); Face(Vector3.left); Physics.SyncTransforms();
            int xp = 0; Action<int> observe = value => xp += value; LevelUpManager.OnExpGained += observe;
            try { Invoke(player.GetComponent<PlayerAttack>(), "MeleeAttack"); }
            finally { LevelUpManager.OnExpGained -= observe; }
            Require(slab.RemainingHits == 0 && !slab.IsBlocking && ResidueRubble.ActiveBlockCount == 0, "actual basic melee destroys rubble in one hit immediately");
            Equal(0, xp, "destroying the prop gives no XP");
            Pass("actual Q melee destroys the slab in one hit; movement unblocks immediately and no prop XP is granted");

            yield return Land(); enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            slab = pattern.ActiveRubble; center = pattern.LandingCenter; MovePlayerTo(center + Vector3.right * 3);
            hp = enemy.Stats.CurrentHealth; FireBasic(center + Vector3.right * 2, Vector3.left, 2);
            yield return Wait(() => enemy.Stats.CurrentHealth < hp, 3, "basic projectile hits the owner behind rubble");
            Require(slab == null || !slab.IsBlocking, "basic projectile breaks rubble");
            Equal(2, hp - enemy.Stats.CurrentHealth, "same basic projectile continues to damage owner behind it");
            Pass("actual basic projectile breaks the prop and continues through to hit the enemy behind it");

            SetHits(2); yield return Land(); enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            slab = pattern.ActiveRubble; center = pattern.LandingCenter; MovePlayerTo(center + Vector3.right * 3);
            hp = enemy.Stats.CurrentHealth; FireBasic(center + Vector3.right * 2, Vector3.left, 2);
            yield return Wait(() => enemy.Stats.CurrentHealth < hp, 3, "two-hit prop first projectile");
            Require(slab != null && slab.IsBlocking && slab.RemainingHits == 1, "sweep/overlap/trigger duplicates count as one attack");
            hp = enemy.Stats.CurrentHealth; FireBasic(center + Vector3.right * 2, Vector3.left, 2);
            yield return Wait(() => enemy.Stats.CurrentHealth < hp, 3, "two-hit prop second projectile");
            Require(slab == null || !slab.IsBlocking, "different attack destroys the two-hit prop");
            Pass("Inspector 2-hit setting: one projectile counts once across all collision routes; a second projectile breaks it and also passes through");
            SetHits(1);

            yield return Land(); enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            slab = pattern.ActiveRubble; center = pattern.LandingCenter; MovePlayerTo(center + Vector3.right * 3); hp = enemy.Stats.CurrentHealth;
            var shotObject = new GameObject("I02_SkillProjectile_Test"); projectiles.Add(shotObject);
            shotObject.transform.position = center + Vector3.right * 2 + Vector3.up * .6f;
            shotObject.AddComponent<SkillProjectile>().Launch(Vector3.left, 2, 8, 2, ~0, true, default);
            yield return Wait(() => enemy.Stats.CurrentHealth < hp, 3, "skill projectile hits owner behind slab");
            Require(slab == null || !slab.IsBlocking, "skill projectile breaks prop without being consumed by it");
            Equal(2, hp - enemy.Stats.CurrentHealth, "skill projectile still hits real enemy");

            yield return Land(); enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            slab = pattern.ActiveRubble; center = pattern.LandingCenter; MovePlayerTo(center + Vector3.right * 3); Physics.SyncTransforms();
            Invoke(player.GetComponent<PlayerAttack>(), "FireBeam", Vector3.left, 2f, 8f);
            Require(!slab.IsBlocking, "native beam destroys prop");
            yield return Land(); enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            slab = pattern.ActiveRubble; center = pattern.LandingCenter; MovePlayerTo(center); Face(Vector3.left);
            Invoke(player.GetComponent<PlayerClassSkillController>(), "ExecuteMageSkill1");
            Require(!slab.IsBlocking, "native damage area skill destroys prop");
            Pass("skill projectile passes through and damages the real enemy; native beam and Mage E area damage also break the prop");

            Spawn(); MovePlayer(8);
            Vector3 candidate = origin + Vector3.right * settings.placementDistance;
            Require(ResiduePlacementSafety.CanPlace(candidate, Vector3.forward, settings.footprint), "safe open placement before corridor test");
            Require(!ResiduePlacementSafety.CanPlace(biome.GetPlayerSpawnPosition(), Vector3.forward, settings.footprint), "entrance exclusion");
            var blocked = new List<Vector2Int>(); Vector2Int cell = biome.WorldToGrid(candidate);
            for (int x = cell.x - 4; x <= cell.x + 4; x++) foreach (int z in new[] { cell.y - 2, cell.y + 2 })
                if (biome.IsWalkable(x, z)) blocked.Add(new Vector2Int(x, z));
            biome.AddRuntimeBlockedCells(blocked);
            try
            {
                Require(!ResiduePlacementSafety.CanPlace(candidate, Vector3.forward, settings.footprint), "narrow corridor rejected");
                MovePlayer(2.6f); yield return new WaitForSeconds(1);
                Require(pattern.Phase == HardenedResiduePhase.Ready && pattern.LandingCount == 0 && pattern.RejectedPlacements > 0,
                    "unsafe attack creates no tell/slab and cannot seal the only passage");
            }
            finally { biome.RemoveRuntimeBlockedCells(blocked); }
            Pass("placement safety rejects entrance and a narrow corridor; safe player-width routes around the slab are required");

            Spawn(); MovePlayer(2.6f);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 3, "death cancellation tell");
            var tell = pattern.TelegraphObject; var cancelled = pattern;
            enemy.TakeDamage(10000); yield return new WaitForSeconds(1);
            Require(tell == null && cancelled.LandingCount == 0 && ResidueRubble.ActiveBlockCount == 0, "death cancels queued landing");
            yield return Land(); slab = pattern.ActiveRubble;
            enemy.TakeDamage(10000);
            Require(!slab.IsBlocking && ResidueRubble.ActiveBlockCount == 0, "death immediately unblocks landed rubble");
            yield return null;
            Require(slab == null && enemy.IsDeathAnimPlaying, "prop removed while owner death animation continues");
            Spawn(); yield return new WaitForSeconds(1);
            Require(enemy.gameObject.activeSelf && !enemy.IsDead && pattern.Phase == HardenedResiduePhase.Ready && ResidueRubble.ActiveBlockCount == 0,
                "pool reuse has no old landing, block or death callback");
            Pass("death before/after landing and immediate pool reuse clear warnings/blockers without truncating the dedicated death animation");

            Spawn(); MovePlayer(2.6f);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 3, "stun cancellation tell");
            enemy.StatusEffects.ApplyStun(1f); yield return new WaitForSeconds(.9f);
            Require(pattern.LandingCount == 0 && pattern.TelegraphObject == null && ResidueRubble.ActiveBlockCount == 0, "stun cancels prepared attack without a new prop");
            Pass("stun during preparation cancels the landing and ground warning");

            var frozen = pattern;
            Set(settings, "windupSeconds", .4f); Set(settings, "recoverySeconds", .3f); Set(settings, "rearmSeconds", .6f);
            Set(settings, "footprint.x", 1.6f); Set(settings, "rubbleLifetime", .7f); Set(settings, "spawnGraceSeconds", 0);
            Set(definition, "statSets.Array.data[0].attackPower", 4);
            int index = definition.patternDamage.FindIndex(p => p.id == HardenedResiduePatternSettings.DamageId);
            Set(definition, $"patternDamage.Array.data[{index}].coefficient", 2);
            var profile = DifficultyBalanceService.GetProfile(GameDifficulty.Normal);
            Set(profile, "elites.outgoingDamage", 1.5f); Set(profile, "elites.attackCooldown", .5f);
            Equal(.8f, frozen.WindupDuration, "existing actor tell remains frozen"); Equal(2.2f, frozen.Footprint.x, "existing footprint remains frozen");
            Spawn(); MovePlayer(2.6f); health.ResetHealth();
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 1, "edited tell");
            MovePlayerTo(pattern.LandingCenter); hp = health.CurrentHealth;
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 2, "edited landing");
            Equal(12, hp - health.CurrentHealth, "actual edited damage: 4 × 1.5 × 2 = 12 once");
            Equal(1.6f, pattern.ActiveRubble.Footprint.x, "Inspector changes actual blocker size");
            Equal(.4f, pattern.WindupDuration, "difficulty does not multiply tell");
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Ready, 1, "edited recovery finishes");
            Require(pattern.NextReadyTime - Time.time > .2f && pattern.NextReadyTime - Time.time <= .301f, "rearm .6 × .5 once");
            Require(!pattern.TryBeginAttack(), "no duplicate cycle while prop/rearm remains");
            yield return Wait(() => pattern.ActiveRubble == null, 1, "edited prop expiry");
            Pass("Serialized Inspector edits reach new actors: damage 12, .4s tell, 1.6m width, .3s recovery/rearm, .7s prop; old actor unchanged");

            Set(settings, "recoverySeconds", .1f); Set(settings, "rearmSeconds", 0); Set(settings, "rubbleLifetime", 2);
            yield return Land();
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Ready, 1, "zero-rearm recovery");
            yield return new WaitForSeconds(.2f);
            Require(pattern.ActiveRubble != null && pattern.LandingCount == 1 && !pattern.TryBeginAttack()
                && enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 1, "one prop maximum even with zero rearm");
            Pass("zero rearm and .1s recovery cannot stack another landing or rubble while the previous prop remains");

            if (enemy != null) enemy.ReleaseToPool(); enemy = null; RestoreAssets();
            MovePlayer(8); field.enabled = true; field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<HardenedResidueElitePattern>();
            MovePlayer(2.6f);
            yield return Wait(() => pattern.ActiveRubble != null && pattern.ActiveRubble.IsBlocking, 4, "real field prop before unload");
            slab = pattern.ActiveRubble; var unloadedActor = enemy; uint unloadedGeneration = enemy.SpawnGeneration;
            MovePlayerTo(biome.GridToWorldWithHeight(point.x < 150 ? 280 : 15, point.y < 150 ? 280 : 15));
            UpdateChunks(); field.Refresh(player.transform.position); yield return null;
            Require(slab == null && ResidueRubble.ActiveBlockCount == 0, "actual chunk unload removes slab and blocking immediately");
            Require(!unloadedActor.gameObject.activeSelf || unloadedActor.SpawnGeneration != unloadedGeneration, "unload ends the old spawn generation");
            Require(!SaveService.IsBiomeEliteDefeated(point.spawnId), "unload is not a kill");
            MovePlayer(8); UpdateChunks(); field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<HardenedResidueElitePattern>();
            Equal(30, enemy.Stats.MaxHealth, "unbeaten point returns with default health");
            Pass("actual chunk unload/reload clears slab and collision without recording a kill; an unbeaten I-02 returns cleanly");
            MovePlayer(2.6f);
            yield return Wait(() => pattern.ActiveRubble != null && pattern.ActiveRubble.IsBlocking, 4, "real field prop before death");
            slab = pattern.ActiveRubble; xp = 0; observe = value => xp += value;
            Action onLevelUp = LevelUpManager.OnLevelUp;
            LevelUpManager.OnExpGained += observe; LevelUpManager.OnLevelUp = null;
            try { enemy.TakeDamage(10000); enemy.TakeDamage(10000); enemy.GrantExp(); }
            finally { LevelUpManager.OnExpGained -= observe; LevelUpManager.OnLevelUp = onLevelUp; }
            Equal(50, xp, "real field death reward once"); Require(SaveService.IsBiomeEliteDefeated(point.spawnId), "kill saved before corpse finishes");
            Require(!slab.IsBlocking && ResidueRubble.ActiveBlockCount == 0, "actual map death opens path immediately");
            var actor = enemy; var seen = new HashSet<Sprite>(); float until = Time.time + 3;
            var renderer = actor.transform.Find("Visual").GetComponent<SpriteRenderer>();
            while (actor.gameObject.activeSelf)
            {
                Require(Time.time < until, "death must finish"); seen.Add(renderer.sprite); field.Refresh(player.transform.position); yield return null;
            }
            Require(settings.presentation.deathFrames.All(seen.Contains), "all six death frames visible before pool return");
            field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0 && ResidueRubble.ActiveBlockCount == 0, "defeated point does not respawn or leave blockers");
            Pass("real field death: blocker removed instantly, XP 50 once, saved defeat, all 6 death frames, no respawn");
        }

        private static void FireBasic(Vector3 position, Vector3 direction, float damage)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/bullet.prefab");
            Require(prefab != null, "real basic bullet prefab");
            GameObject shot = Object.Instantiate(prefab); projectiles.Add(shot);
            position.y = biome.GetGroundHeight(position) + .6f; shot.transform.position = position;
            shot.GetComponent<Projectile>().Launch(direction, damage, ~0, 10);
        }

        private static IEnumerator WalkTo(Vector3 destination, float timeout)
        {
            float until = Time.time + timeout;
            while (true)
            {
                Vector3 delta = destination - player.transform.position; delta.y = 0;
                if (delta.magnitude < .08f) yield break;
                Require(Time.time < until, "walking timed out: " + player.transform.position + " → " + destination);
                player.TryMoveByWorld(delta.normalized * Mathf.Min(delta.magnitude, player.MoveSpeed * Time.fixedDeltaTime));
                yield return new WaitForFixedUpdate();
            }
        }

        private static IEnumerator WalkFor(Vector3 direction, float seconds)
        {
            float until = Time.time + seconds;
            while (Time.time < until)
            { player.TryMoveByWorld(direction * player.MoveSpeed * Time.fixedDeltaTime); yield return new WaitForFixedUpdate(); }
        }
    }
}
