using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class PollenInvaderConnectionRunner
    {
        private static IEnumerator DirectionDeathChecks()
        {
            foreach (float direction in BattleAngles)
            {
                yield return DeathCase(direction, "windup");
                yield return DeathCase(direction, "closing");
            }
            foreach (float direction in new[] { 45f, 135f, 225f, 315f }) yield return DeathCase(direction, "release");
            yield return DeathCase(270, "ready");
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.VolleyCount == 1, 3, "live pool release");
            var life = enemy.GetComponent<EnemyPatternLifetime>(); var pellets = pattern.ActivePellets.ToArray(); uint gen = enemy.SpawnGeneration;
            enemy.ReleaseToPool(); Require(life.OwnedObjectCount == 0 && pellets.All(p => p == null || !p.gameObject.activeSelf), "pool clears all flying pellets synchronously");
            Spawn(); Require(enemy.SpawnGeneration != gen && pattern.LastVolley == null && pattern.VolleyCount == 0, "pool volley counter clean");
            Pass("POOL: manual return during flight clears all three and reused generation starts without stale volley");
        }
        private static IEnumerator DeathCase(float direction, string phase)
        {
            Spawn(); Move(origin + Aim(direction) * 4);
            if (phase == "ready") { yield return null; yield return null; }
            else
            {
                yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "death tell");
                if (phase != "windup")
                {
                    Move(origin - Aim(direction) * 3); yield return Wait(() => pattern.VolleyCount == 1, 2, "death volley");
                    if (phase == "closing") yield return new WaitForSeconds(.4f);
                }
            }
            var facing = enemy.PatternFacing; var bank = settings.presentation.directionalPresentation.Capture(); var frames = enemy.Config.deathSprites.Select(f => bank.Resolve(f, facing)).ToArray();
            var body = Body(); var life = enemy.GetComponent<EnemyPatternLifetime>(); uint gen = enemy.SpawnGeneration;
            enemy.TakeDamage(10000); Require(life.OwnedObjectCount == 0 && !enemy.IsPatternPositionLocked, "death ownership and lock cleared");
            Move(origin); health.ResetHealth(); float hp = health.CurrentHealth; var seen = new HashSet<Sprite>(); float until = Time.time + 2;
            while (enemy.gameObject.activeSelf)
            {
                Require(Time.time < until && body.flipX == bank.Flip(facing), "death finish and direction flip"); seen.Add(body.sprite); yield return null;
            }
            Require(frames.All(seen.Contains), "six correct directional death frames"); Equal(hp, health.CurrentHealth, "no corpse or late projectile hit");
            Spawn(); Require(enemy.SpawnGeneration != gen && pattern.LastVolley == null && pattern.VolleyCount == 0 && !enemy.IsPatternPositionLocked, "death pool generation fresh");
            Pass($"DEATH {direction:0}deg/{phase}: six correct {facing} frames, immediate threats/lock cleanup, no late damage and clean reuse");
        }
        private static IEnumerator FieldUnloadChecks()
        {
            var saved = origin;
            foreach (bool flying in new[] { false, true })
            {
                if (enemy != null) enemy.ReleaseToPool(); origin = fieldOrigin; field.enabled = true;
                Move(origin + Vector3.right * 4); field.Refresh(player.transform.position);
                enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<PollenInvaderElitePattern>();
                yield return Wait(() => flying ? pattern.VolleyCount == 1 : pattern.Phase == PollenInvaderPhase.Windup, 3, "field attack");
                var life = enemy.GetComponent<EnemyPatternLifetime>(); uint gen = enemy.SpawnGeneration; var point = field.Plan.placements.Single();
                if (flying) Require(life.OwnedObjectCount == 3, "field owns all flying projectiles");
                Move(biome.GridToWorldWithHeight(point.x < 150 ? 280 : 15, point.y < 150 ? 280 : 15)); field.Refresh(player.transform.position); yield return null;
                Require(!life.IsCurrent(gen) && life.OwnedObjectCount == 0 && !SaveService.IsBiomeEliteDefeated(point.spawnId), "unload not a kill");
                Move(origin + Vector3.right * 5); field.Refresh(player.transform.position);
                enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<PollenInvaderElitePattern>();
                Require(!enemy.IsPatternPositionLocked && pattern.VolleyCount == 0 && pattern.LastVolley == null, "fresh field return");
                field.enabled = false; enemy = null;
                Pass(flying ? "FIELD flight: actual chunk unload clears all pellets without defeat and returns cleanly" : "FIELD tell: actual chunk unload cancels tell without defeat and returns cleanly");
            }
            origin = saved;
        }
    }
}
