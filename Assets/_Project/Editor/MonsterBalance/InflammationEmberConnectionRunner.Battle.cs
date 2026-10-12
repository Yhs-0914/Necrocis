using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class InflammationEmberConnectionRunner
    {
        private static IEnumerator BattleChecks()
        {
            field.enabled = false;
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            SpawnBattle(); Move(3); health.ResetHealth();
            float firstHit = Time.time; enemy.TakeDamage(1); Vector3 locked = pattern.LockedDirection;
            while (Time.time - firstHit < .55f)
            {
                enemy.TakeDamage(1);
                if (Time.time - firstHit > .15f) Move(-3);
                Require(pattern.Phase == InflammationEmberPhase.Windup && pattern.LockedDirection == locked, "rapid hits retain first aim and preparation");
                Require(!Body().flipX, "committed right-facing pose does not turn with target");
                yield return new WaitForSeconds(.05f);
            }
            yield return Wait(() => pattern.ShotCount == 1, 1, "rapid-hit counter");
            Require(Time.time - firstHit < .85f && pattern.ActiveThorn.Direction.x > .99f, "rapid hits do not postpone .7s release or retarget");
            while (pattern.Phase == InflammationEmberPhase.Recovery)
            { enemy.TakeDamage(1); Require(!Body().flipX, "recovery keeps facing"); yield return new WaitForSeconds(.05f); }
            yield return null; Require(Body().flipX, "ready turns to left player after recovery");
            while (Time.time < pattern.NextReadyTime) { enemy.TakeDamage(1); yield return new WaitForSeconds(.1f); }
            yield return new WaitForSeconds(.15f);
            Require(pattern.ShotCount == 1 && pattern.Phase == InflammationEmberPhase.Ready, "cooldown damage does not queue a future counter");
            enemy.TakeDamage(1); yield return Wait(() => pattern.ShotCount == 2, 1, "fresh post-cooldown hit");
            Require(pattern.LockedDirection.x < -.99f, "new hit locks new side");
            Pass("rapid hits cannot reset .7s tell, accumulate shots or retarget; facing locks through recovery, then updates; cooldown hits do not queue, next fresh hit can counter");

            SpawnBattle(); Move(3); enemy.TakeDamage(0); enemy.TakeDamage(-1); yield return new WaitForSeconds(.2f);
            Require(pattern.ShotCount == 0 && pattern.Phase == InflammationEmberPhase.Ready, "non-damaging hits cannot trigger");
            enemy.StatusEffects.ApplyPoison(1.2f, .1f, 1);
            yield return Wait(() => pattern.Phase == InflammationEmberPhase.Windup, 1, "actual poison tick starts counter");
            firstHit = Time.time;
            yield return Wait(() => pattern.ShotCount == 1, 1, "poison counter");
            Require(Time.time - firstHit < .85f, "poison ticks cannot keep postponing release");
            yield return Wait(() => pattern.Phase == InflammationEmberPhase.Ready, 2, "poison recovery");
            Require(pattern.ShotCount == 1 && !enemy.StatusEffects.IsPoisoned, "finite DoT causes only one committed cycle");
            Pass("zero/negative damage does not trigger; real poison ticks trigger one counter without resetting or multiplying it");

            foreach (bool dash in new[] { false, true })
            {
                SpawnBattle(); Move(3); health.ResetHealth(); float hp = health.CurrentHealth; enemy.TakeDamage(1);
                if (dash)
                {
                    FacePlayer(Vector3.forward);
                    player.StartCoroutine((IEnumerator)typeof(PlayerController).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player, null));
                    yield return new WaitForSeconds(.4f);
                    Require(player.transform.position.z - origin.z > .8f, "native dash actually moved sideways");
                }
                else yield return WalkTo(origin + Vector3.right * 3 + Vector3.forward * 1.6f, 1);
                yield return Wait(() => pattern.ShotCount == 1, 1, "dodge counter launch");
                Require(pattern.LockedDirection.x > .99f, "dodged shot keeps original direction");
                yield return Wait(() => pattern.ActiveThorn == null, 3, "dodged thorn expires");
                Equal(hp, health.CurrentHealth, "side dodge avoids actual projectile");
                Pass(dash ? "actual player dash during tell avoids fixed-direction counter with zero damage" : "actual default-speed walking during tell avoids fixed-direction counter with zero damage");
            }

            SpawnBattle(); Move(3); enemy.TakeDamage(1);
            yield return Wait(() => pattern.Phase == InflammationEmberPhase.Recovery, 1, "retaliation recovery");
            Move(-1.1f); FacePlayer(Vector3.right); Physics.SyncTransforms(); float enemyHp = enemy.Stats.CurrentHealth;
            typeof(PlayerAttack).GetMethod("MeleeAttack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player.GetComponent<PlayerAttack>(), null);
            Require(enemy.Stats.CurrentHealth < enemyHp && pattern.ShotCount == 1, "native Q damages recovering enemy without adding a counter");
            Require(enemy.Balance.ContactEnabled, "body contact stays enabled during recovery");
            Pass("native Q can punish recovery; recovery hits do not add a shot and body contact remains enabled");

            SpawnBattle(); Move(3); enemy.StatusEffects.ApplyStun(.3f); enemy.TakeDamage(1); yield return new WaitForSeconds(.35f);
            Require(pattern.Phase == InflammationEmberPhase.Ready && pattern.ShotCount == 0, "stunned hit cannot schedule delayed counter");
            enemy.TakeDamage(1); Require(pattern.Phase == InflammationEmberPhase.Windup, "fresh hit after stun can arm");
            enemy.StatusEffects.ApplyStun(.5f); yield return new WaitForSeconds(.9f);
            Require(pattern.ShotCount == 0 && pattern.Phase == InflammationEmberPhase.Ready && !enemy.GetComponent<EnemyPatternLifetime>().IsCycleRunning,
                "stun during preparation cancels pending shot without a stuck cycle");
            Pass("hits during stun are ignored; stun during preparation cancels the counter and leaves no delayed shot or stuck cycle");

            SpawnBattle(); Move(3); enemy.TakeDamage(1); enemy.TakeDamage(10000);
            var cancelled = pattern; yield return new WaitForSeconds(.9f);
            Require(cancelled.ShotCount == 0 && cancelled.Phase == InflammationEmberPhase.Inactive, "death during tell cancels release");
            SpawnBattle(); Move(3); enemy.TakeDamage(1);
            yield return Wait(() => pattern.ActiveThorn != null, 1, "live thorn before death");
            var dyingShot = pattern.ActiveThorn; uint oldGeneration = enemy.SpawnGeneration;
            enemy.TakeDamage(10000);
            Require(!dyingShot.gameObject.activeSelf && enemy.IsDeathAnimPlaying, "death disables live thorn immediately while animation continues");
            SpawnBattle(); Move(-3); yield return null;
            Require(enemy.SpawnGeneration != oldGeneration && pattern.ShotCount == 0 && pattern.Phase == InflammationEmberPhase.Ready,
                "pooled actor starts without old event/cycle/shot");
            enemy.TakeDamage(1); yield return Wait(() => pattern.ShotCount == 1, 1, "reused single counter");
            Require(pattern.ActiveThorn.Direction.x < -.99f, "reused actor fires once toward new side");
            Pass("death during tell cancels; death during flight disables thorn immediately; immediate pool reuse has clean generation, event and aim");

            Set(settings, "recoverySeconds", .1f); Set(settings, "rearmSeconds", 0);
            SpawnBattle(); Move(3.8f); enemy.TakeDamage(1);
            yield return Wait(() => pattern.Phase == InflammationEmberPhase.Ready && pattern.ActiveThorn != null, 2, "zero-rearm live thorn");
            for (int i = 0; i < 5; i++) enemy.TakeDamage(1);
            Require(pattern.ShotCount == 1 && !enemy.GetComponent<EnemyPatternLifetime>().IsCycleRunning, "remaining thorn blocks another counter even with zero rearm");
            yield return Wait(() => pattern.ActiveThorn == null, 3, "previous thorn removed");
            enemy.TakeDamage(1); yield return Wait(() => pattern.ShotCount == 2, 1, "new hit after thorn removal");
            RestoreAssets();
            Pass("Inspector .1s recovery / zero rearm still permits only one live thorn; a fresh hit after cleanup can counter again");

            SpawnBattle(); Move(10); enemy.TakeDamage(1); yield return new WaitForSeconds(.8f);
            Require(pattern.ShotCount == 0 && pattern.Phase == InflammationEmberPhase.Ready, "out-of-range/offscreen hit cannot arm");
            SpawnBattle(); Move(3); enemy.TakeDamage(1); Move(20);
            yield return Wait(() => pattern.Phase == InflammationEmberPhase.Ready, 3, "leave-screen recovery");
            Require(pattern.ShotCount == 0, "leaving view during tell cancels fire"); Move(3); yield return new WaitForSeconds(.3f);
            Require(pattern.ShotCount == 0, "returning does not fire old reservation");
            Pass("out-of-range/offscreen hits and leaving view during tell cannot create surprise or delayed return shots");

            SpawnBattle(); Move(3);
            Vector2Int blocked = biome.WorldToGrid(origin + Vector3.right * 1.5f);
            biome.AddRuntimeBlockedCells(new[] { blocked });
            try { enemy.TakeDamage(1); yield return new WaitForSeconds(.8f); Require(pattern.ShotCount == 0 && pattern.Phase == InflammationEmberPhase.Ready, "blocked line cannot arm counter"); }
            finally { biome.RemoveRuntimeBlockedCells(new[] { blocked }); }
            Pass("blocked terrain between attacker and ember prevents reservation without weakening terrain rules");

            yield return ProjectileAudit();
            if (!directionalBattle) yield return FieldDeathChecks();
        }

        private static IEnumerator FieldDeathChecks()
        {
            if (enemy != null) enemy.ReleaseToPool(); enemy = null; RestoreAssets(); field.enabled = true;
            Spawn(); Move(3); enemy.TakeDamage(1);
            yield return Wait(() => pattern.ActiveThorn != null, 1, "field flight before chunk unload");
            var shot = pattern.ActiveThorn; var actor = enemy; var life = actor.GetComponent<EnemyPatternLifetime>(); uint generation = actor.SpawnGeneration;
            var point = field.Plan.placements[0];
            MoveTo(biome.GridToWorldWithHeight(point.x < 150 ? 280 : 15, point.y < 150 ? 280 : 15)); field.Refresh(player.transform.position); yield return null;
            Require(shot == null && !life.IsCurrent(generation) && !SaveService.IsBiomeEliteDefeated(point.spawnId), "unload cancels thorn without recording kill");
            Spawn(); Move(-3); enemy.TakeDamage(1);
            yield return Wait(() => pattern.ActiveThorn != null, 1, "field flight before real death");
            shot = pattern.ActiveThorn; actor = enemy; int xp = 0; Action<int> observe = amount => xp += amount; var levelUp = LevelUpManager.OnLevelUp;
            LevelUpManager.OnExpGained += observe; LevelUpManager.OnLevelUp = null;
            try { enemy.TakeDamage(10000); enemy.TakeDamage(10000); enemy.GrantExp(); }
            finally { LevelUpManager.OnExpGained -= observe; LevelUpManager.OnLevelUp = levelUp; }
            Equal(50, xp, "real field XP once");
            Require(SaveService.IsBiomeEliteDefeated(point.spawnId) && !shot.gameObject.activeSelf, "death recorded and live thorn disabled synchronously");
            var body = Body(); var seen = new HashSet<Sprite>(); bool facing = body.flipX; float deadline = Time.time + 3;
            while (actor.gameObject.activeSelf)
            {
                Require(Time.time < deadline, "dedicated death finishes"); seen.Add(body.sprite); Require(body.flipX == facing, "death keeps last side");
                field.Refresh(player.transform.position); yield return null;
            }
            Require(settings.presentation.deathFrames.All(seen.Contains), "all six dedicated death frames rendered");
            field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0 && Object.FindObjectsByType<EmberThorn>(FindObjectsSortMode.None).Length == 0, "dead field point and owned thorns cleared");
            MoveTo(biome.GridToWorldWithHeight(point.x < 150 ? 280 : 15, point.y < 150 ? 280 : 15)); field.Refresh(player.transform.position);
            Move(3); field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0, "defeated H-02 does not respawn after chunk round trip");
            Pass("real field unload clears flight without a kill; real death gives XP 50 once, saves defeat, displays all six death frames with retained facing, and stays dead after chunk round trip");
        }

        private static void SpawnBattle()
        {
            if (enemy != null && enemy.gameObject.activeSelf) enemy.ReleaseToPool();
            Move(5);
            var rule = InflammationEmberSetup.PreviewRule(definition);
            enemy = EnemyController.Acquire(null, "H02_BattleProbe", EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(null, rule, origin, origin); enemy.SuppressExperienceReward = true;
            enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 200, true);
            pattern = enemy.GetComponent<InflammationEmberElitePattern>();
        }
        private static SpriteRenderer Body() => enemy.transform.Find("Visual").GetComponent<SpriteRenderer>();
        private static void FacePlayer(Vector3 direction)
        {
            typeof(PlayerController).GetField("movement", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(player, Vector3.zero);
            typeof(PlayerController).GetField("lastMoveDirection", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(player, direction);
        }
        private static IEnumerator WalkTo(Vector3 destination, float timeout)
        {
            float until = Time.time + timeout;
            while (true)
            {
                Vector3 delta = destination - player.transform.position; delta.y = 0;
                if (delta.magnitude < .08f) yield break;
                Require(Time.time < until, "walk dodge deadline");
                player.TryMoveByWorld(delta.normalized * Mathf.Min(delta.magnitude, player.MoveSpeed * Time.fixedDeltaTime));
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
