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
    public static partial class GasSacPlayModeRunner
    {
        private static IEnumerator DeathChecks()
        {
            GasSacPresentation art = settings.presentation;
            Require(art.deathFrames.Length == 6 && art.deathFrames.Distinct().Count() == 6, "six unique dedicated death sprites");
            foreach (Sprite frame in art.deathFrames)
            {
                Require(frame != art.deflated && frame != art.idle && !art.inflationFrames.Contains(frame), "death never reuses a recovery/idle frame");
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(frame));
                Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled
                    && importer.textureCompression == TextureImporterCompression.Uncompressed, "death pixel import settings");
                Equal(art.deathFrames[0].pixelsPerUnit, frame.pixelsPerUnit, "consistent death-sheet PPU");
                Equal(Mathf.Round(frame.pivot.x), frame.pivot.x, "integer death x pivot");
                Equal(Mathf.Round(frame.pivot.y), frame.pivot.y, "integer corpse ground pivot");
            }
            Require(art.deathFrames[0].bounds.size.x <= art.idle.bounds.size.x + .01f
                && art.deathFrames[0].bounds.size.y <= art.idle.bounds.size.y + .01f, "death starts inside approved idle world-size envelope");
            Require(art.deathFrames[5].bounds.size.y < art.deathFrames[0].bounds.size.y * .5f, "final corpse is visibly flattened");
            if (!preview) Pass("6 dedicated transparent death poses, Point/no mip/uncompressed, integer foot anchors, matched initial size and flattened last frame");

            MovePlayer(5.2f); health.ResetHealth();
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 4, "body windup");
            Require(pattern.TelegraphObject == null && enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0,
                "B has no ground UI during windup");
            yield return Wait(() => pattern.ActiveOrb != null, 3, "shot before death");
            var shot = pattern.ActiveOrb;
            if (preview)
            {
                enemy.SuppressExperienceReward = true;
                enemy.TakeDamage(10000);
                yield return new WaitForSeconds(art.deathFrameSeconds * 2.1f);
                Require(enemy.IsDeathAnimPlaying && enemy.gameObject.activeInHierarchy, "death preview retained by real field");
                yield break;
            }

            float before = health.CurrentHealth;
            int xp = 0; Action<int> record = value => xp += value;
            Action onLevelUp = LevelUpManager.OnLevelUp;
            LevelUpManager.OnExpGained += record;
            LevelUpManager.OnLevelUp = null; // Test-only: observe XP without a stat-selection UI pausing the animation.
            try { enemy.TakeDamage(10000); enemy.TakeDamage(10000); enemy.GrantExp(); }
            finally { LevelUpManager.OnExpGained -= record; LevelUpManager.OnLevelUp = onLevelUp; }
            Equal(50, xp, "field kill rewards exactly 50 XP once, before animation completion");
            Require(SaveService.IsBiomeEliteDefeated(point.spawnId), "kill saved immediately, before animation completion");
            Require(enemy.IsDead && enemy.IsDeathAnimPlaying && enemy.gameObject.activeInHierarchy, "logical death precedes visual completion");
            Require(!enemy.GetComponent<BoxCollider>().enabled && !pattern.enabled && pattern.Phase == GasSacPhase.Inactive,
                "collider and attacks end immediately");
            Require(enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, "death releases shot ownership immediately");
            var actor = enemy; var renderer = actor.transform.Find("Visual").GetComponent<SpriteRenderer>();
            var seen = new HashSet<Sprite>();
            float start = Time.time, until = start + 3;
            while (actor.gameObject.activeSelf)
            {
                Require(Time.time < until, "death animation must finish and return to pool");
                seen.Add(renderer.sprite);
                field.Refresh(player.transform.position); // The actual map refresh must not truncate the corpse animation.
                yield return null;
            }
            Require(art.deathFrames.All(seen.Contains), "real map displays all six death frames before pooling");
            Require(Time.time - start >= art.deathFrames.Length * art.deathFrameSeconds - .03f, "death is not cut short by field refresh");
            Require(shot == null, "flying orb removed at death");
            Equal(before, health.CurrentHealth, "corpse and cancelled shot cannot hit player");
            field.Refresh(player.transform.position);
            Require(!field.Spawners.Any(s => s.Placement.spawnId == point.spawnId), "finished corpse spawner cleaned");
            Pass("real map death: 50 XP once + saved kill immediately; no collision/damage; all 6 frames run for 0.84s before pooling; no respawn");

            // A second actual map point verifies that leaving a chunk can interrupt the visual death safely.
            BiomeElitePlacement second = field.Plan.placements[1];
            origin = biome.GridToWorldWithHeight(second.x, second.y); MovePlayer(5.2f);
            typeof(BiomeManager).GetMethod("UpdateChunks", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(biome, null);
            field.Refresh(player.transform.position);
            enemy = field.Spawners.Single(s => s.Placement.spawnId == second.spawnId).ActiveEnemy;
            enemy.SuppressExperienceReward = true; enemy.TakeDamage(10000);
            uint oldGeneration = enemy.SpawnGeneration; actor = enemy;
            Require(enemy.IsDeathAnimPlaying, "second map corpse starts its animation");
            MovePlayerTo(biome.GridToWorldWithHeight(second.x < 150 ? 280 : 15, second.y < 150 ? 280 : 15));
            typeof(BiomeManager).GetMethod("UpdateChunks", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(biome, null);
            field.Refresh(player.transform.position); yield return null;
            Require(!actor.gameObject.activeSelf || actor.SpawnGeneration != oldGeneration, "unload releases dying actor immediately");
            Require(!field.Spawners.Any(s => s.Placement.spawnId == second.spawnId), "unloaded corpse node removed");
            Require(SaveService.IsBiomeEliteDefeated(second.spawnId), "unload does not undo saved death");
            Pass("actual chunk unload during death removes the corpse immediately while retaining the saved defeat");

            field.enabled = false; enemy = null;
            origin = biome.GridToWorldWithHeight(point.x, point.y);
            SpawnOrbTest(); MovePlayer(2.3f);
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 3, "A death interruption");
            var ring = pattern.TelegraphObject;
            enemy.SuppressExperienceReward = true; enemy.TakeDamage(10000);
            yield return null;
            Require(ring == null && pattern.BurstCount == 0 && enemy.IsDeathAnimPlaying, "A ring/attack removed but death visual continues");
            Pass("A telegraph interrupted by death: ring disappears, no burst, dedicated death animation starts");

            actor = enemy; oldGeneration = actor.SpawnGeneration;
            SpawnOrbTest(); yield return new WaitForSeconds(1.1f);
            Require(enemy.gameObject.activeSelf && !enemy.IsDead && !enemy.IsDeathAnimPlaying, "pool reuse cannot inherit old death completion");
            Require(actor != enemy || enemy.SpawnGeneration != oldGeneration, "same object reused only with a new generation");
            Require(enemy.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite == art.idle, "reused actor restores V2 idle");
            Require(enemy.GetComponent<BoxCollider>().enabled, "reused live actor restores collision");
            Pass("release in mid-death + immediate pool reuse restores idle/collision and cannot receive an old completion callback");

            float frozenInterval = enemy.Config.deathAnimationSpeed;
            Set(art, "deathFrameSeconds", .06f);
            Equal(.14f, frozenInterval, "existing actor keeps original death timing");
            Equal(.14f, enemy.Config.deathAnimationSpeed, "Inspector edit cannot mutate current actor");
            SpawnOrbTest(); Equal(.06f, enemy.Config.deathAnimationSpeed, "new actor uses Inspector death timing");
            Require(!ReferenceEquals(enemy.Config.deathSprites, art.deathFrames), "spawn owns a death-frame snapshot");
            enemy.SuppressExperienceReward = true; start = Time.time; enemy.TakeDamage(10000);
            yield return Wait(() => !enemy.gameObject.activeSelf, 2, "edited death finish");
            Require(Time.time - start >= .33f && Time.time - start < .6f, "six edited frames finish in approximately .36 seconds");
            Pass("Inspector death interval edit: old actor remains 0.14s/frame, next actor plays 0.06s/frame and finishes at about 0.36s");
        }
    }
}
