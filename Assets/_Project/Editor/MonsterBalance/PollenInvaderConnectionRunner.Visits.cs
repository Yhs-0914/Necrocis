using System;
using System.Collections;
using System.Linq;
using Necrocis;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class PollenInvaderConnectionRunner
    {
        private static IEnumerator VisitChecks()
        {
            var point = field.Plan.placements.Single(); string spawnId = point.spawnId;
            Require(SaveService.TrySaveActiveRun(out string error), error); string run = SaveService.ActiveRunId, visit = biome.MonsterVisit.VisitId;
            SaveService.UseStorageRootForTests(storage); Require(SaveService.TryContinue(GameDifficulty.Normal, out error), error);
            Require(SaveService.TryRestorePendingSession(out BiomeType resume, out error) && resume == BiomeType.Lung, error);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null; BindBattleScene();
            Require(SaveService.ActiveRunId == run && biome.MonsterVisit.VisitId == visit && SaveService.IsBiomeEliteDefeated(spawnId), "disk continue preserves visit and kill");
            field.Configure(biome, temporary, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Move(biome.GridToWorldWithHeight(point.x, point.y)); field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0, "disk-loaded defeated field does not respawn"); field.enabled = false;
            Pass("SAVE: actual disk save/continue and scene reload preserve visit and defeated P-03; no respawn");
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath);
            foreach (var difficulty in new[] { GameDifficulty.Normal, GameDifficulty.Hard })
            {
                if (enemy != null && enemy.gameObject.activeSelf) enemy.ReleaseToPool();
                SaveService.MarkFinalBossDefeated(); Require(SaveService.TryBeginNewGame(difficulty, out error), error);
                Require(SaveService.TryRestorePendingSession(out _, out error), error);
                GameManager.Instance.EnterBiome(BiomeType.Lung); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null; BindBattleScene();
                foreach (int stage in new[] { 0, 1, 2, 3, 4 })
                {
                    Require(biome.MonsterVisit.Stage == stage && biome.MonsterVisit.Difficulty == difficulty, $"actual visit captures stage/difficulty: expected={difficulty}/{stage}, actual={biome.MonsterVisit.Difficulty}/{biome.MonsterVisit.Stage}");
                    origin = FindPatch(0); Spawn(); enemy.ApplyBalancePhase("Default", true);
                    var source = definition.statSets.Single(s => s.id == "Default"); var multi = catalog.difficultyCatalog.Get(difficulty).elites;
                    Require(catalog.progression.TryGetStage(stage, out var progression), "stage source exists");
                    Equal(CharacterStats.ToHealthUnits(source.maxHealth * multi.maxHealth * progression.maxHealth), enemy.Stats.MaxHealth, "HP multipliers once");
                    Equal(source.moveSpeed * multi.moveSpeed, enemy.Stats.MoveSpeed, "move speed excludes progression");
                    float raw = source.attackPower * multi.outgoingDamage * progression.attackPower * definition.patternDamage.Single(p => p.id == PollenInvaderPatternSettings.DamageId).coefficient;
                    Equal(raw, enemy.CreatePatternDamage(PollenInvaderPatternSettings.DamageId, 0).Amount, "damage multipliers once");
                    enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); Move(origin + Vector3.right * 3.6f);
                    yield return Wait(() => pattern.VolleyCount == 1, 3, "stage live pellets"); var pellet = pattern.ActivePellets[1];
                    Move(pellet.transform.position + pellet.Direction * 2.6f); health.ResetHealth(); float hp = health.CurrentHealth;
                    yield return Wait(() => pattern.LastVolley.HitAttempts == 1, 2, "stage actual damage"); Equal(CharacterStats.ToHealthUnits(raw), hp - health.CurrentHealth, "live pellets actual HP");
                    Move(origin + Vector3.left * 3);
                    Equal(.16f, pattern.PelletRadius, "stage does not expand danger area"); Equal(.85f, pattern.WindupDuration, "stage does not compress warning");
                    Equal(3.5f, pellet.Speed, "pellet speed has no progression multiplier"); Equal(30, pattern.SpreadHalfAngle, "fan stays fixed");
                    yield return Wait(() => pattern.Phase == PollenInvaderPhase.Ready, 4, "stage rearm");
                    Require(Mathf.Abs(pattern.NextReadyTime - Time.time - settings.rearmSeconds * multi.attackCooldown) < .06f, "one difficulty rearm multiplier");
                    int expectedXp = (int)Math.Round((double)definition.reward.baseExperience * multi.experienceReward * progression.experience, MidpointRounding.ToEven), xp = 0;
                    enemy.SuppressExperienceReward = false; Action<int> observe = amount => xp += amount; var levelUp = LevelUpManager.OnLevelUp;
                    LevelUpManager.OnLevelUp = null; LevelUpManager.OnExpGained += observe;
                    try { enemy.TakeDamage(10000); enemy.TakeDamage(10000); enemy.GrantExp(); }
                    finally { LevelUpManager.OnLevelUp = levelUp; LevelUpManager.OnExpGained -= observe; }
                    Equal(expectedXp, xp, "actual reward once");
                    yield return Wait(() => !enemy.gameObject.activeSelf, 2, "stage death");
                    Pass($"BALANCE {difficulty}/stage{stage}: actual HP, pellet={CharacterStats.ToHealthUnits(raw)}, XP={xp} once, movement/radius/tell unchanged, single rearm multiplier");
                    if (stage == 4) continue;
                    GameManager.Instance.CollectRelic(new[] { BiomeType.Intestine, BiomeType.Liver, BiomeType.Stomach, BiomeType.Lung }[stage]);
                    Require(biome.MonsterVisit.Stage == stage, "current visit stays frozen");
                    Spawn(); Require(enemy.Balance.Current.Stage == stage, "same visit respawn stays frozen"); enemy.ReleaseToPool();
                    GameManager.Instance.ReturnToHub(); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_HUB);
                    GameManager.Instance.EnterBiome(BiomeType.Lung); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null; BindBattleScene();
                }
            }
        }

        private static void BindBattleScene()
        {
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>(); field = biome.GetComponent<BiomeEliteField>();
            Require(biome.BiomeType == BiomeType.Lung, "visit fixture must load actual Lung scene");
            field.enabled = false; enemy = null; pattern = null;
            Require(biome.GetBiomeConfig().biomeEliteSpawnConfig.monsters.Count == productionLungCount, "production Lung unchanged");
            ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules", Private).GetValue(biome)).Clear();
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            typeof(InputManager).GetMethod("SetActionsEnabled", Private).Invoke(InputManager.Instance, new object[] { false });
            groundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position);
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
        }
    }
}
