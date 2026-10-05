using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class PollenInvaderConnectionRunner
    {
        private static IEnumerator FollowupLifecycleChecks()
        {
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "turn stun first");
            var first = pattern.FirstWavePellets[1]; enemy.StatusEffects.ApplyStun(2);
            Move(first.transform.position + first.Direction * 2.6f); health.ResetHealth(); float hp = health.CurrentHealth;
            yield return Wait(() => pattern.FirstVolley.WaveDamageApplications == 1, 2, "first survives turn stun"); Equal(3, hp - health.CurrentHealth, "first shot damage remains");
            yield return Wait(() => pattern.Phase == PollenInvaderPhase.Ready, 2, "cancelled B recovery");
            Require(pattern.CycleVolleyCount == 1 && pattern.SecondVolley == null && !enemy.IsPatternPositionLocked && !enemy.IsPatternFacingLocked, "turn stun cancels only unlaunched wave");
            Pass("STUN B turn: first projectile survives, unlaunched second cancelled, selected final recovery and locks cleaned");
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.CycleVolleyCount == 2, 4, "late stun second");
            enemy.StatusEffects.ApplyStun(3); var second = pattern.SecondWavePellets[1]; var at = second.transform.position;
            Move(origin - Vector3.right * 3); yield return new WaitForSeconds(.3f);
            Require(second != null && Vector3.Distance(at, second.transform.position) > .8f && pattern.CycleVolleyCount == 2, "second continues through late stun");
            yield return Wait(() => enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 3, "late stun cleanup");
            Require(!enemy.IsPatternPositionLocked, "late stun not stuck"); Pass("STUN B after release: both existing waves keep their lifetime/movement and recover without another volley");
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "second wall first");
            var blocked = biome.WorldToGrid(pattern.LaunchPosition); biome.AddRuntimeBlockedCells(new[] { blocked });
            try
            {
                yield return new WaitForSeconds(.95f); Require(pattern.CycleVolleyCount == 1 && pattern.SecondVolley == null, "wall prevents second");
            }
            finally { biome.RemoveRuntimeBlockedCells(new[] { blocked }); }
            Pass("WALL B: new obstruction during rotation blocks second spawn, no hidden release");
            int oldRate = Application.targetFrameRate, oldSync = QualitySettings.vSyncCount;
            try
            {
                QualitySettings.vSyncCount = 0; Application.targetFrameRate = 15;
                Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "15FPS first"); Move(origin - Vector3.right * 3);
                float maxDt = 0;
                while (pattern.CycleVolleyCount < 2) { maxDt = Mathf.Max(maxDt, Time.deltaTime); Require(Time.time - pattern.ReleasedAt < 1, "15FPS second timing"); yield return null; }
                float gap = pattern.SecondReleasedAt - pattern.ReleasedAt;
                Require(maxDt > .045f && gap >= .79f && gap < .94f && pattern.ActivePellets.Length == 6, "15FPS sequential six");
                Pass($"FRAME B 15FPS: actual dt{maxDt:F3}s, two waves separated by {gap:F3}s, exactly3+3");
            }
            finally { Application.targetFrameRate = oldRate; QualitySettings.vSyncCount = oldSync; }
            Set(settings, "followup.rearmSeconds", 0); Set(settings, "followup.recoverySeconds", .1f); Set(settings, "followup.rotationSeconds", .3f); Set(settings, "pelletLifetimeSeconds", .6f);
            Spawn(); Move(origin + Vector3.right * 4); float until = Time.time + 7;
            yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "zero rearm first"); var budget = pattern.CycleHitBudget;
            while (pattern.VolleyCount < 4)
            {
                Require(Time.time < until && Object.FindObjectsByType<PollenPellet>(FindObjectsSortMode.None).Length <= 6, "six maximum across cycles");
                Require(enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount <= 6, "owned upper bound6"); yield return null;
            }
            Require(!ReferenceEquals(budget, pattern.CycleHitBudget), "fresh damage budget per cycle"); Restore(); enemy.ReleaseToPool();
            Pass("ZERO REARM B: two complete cycles, <=6 simultaneous pellets, new budget per cycle, no accumulation");
            foreach (float targetAngle in BattleAngles) foreach (bool afterSecond in new[] { false, true })
            {
                Spawn(); Move(origin + Aim(targetAngle) * 4); yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "B death first"); Move(origin - Aim(targetAngle) * 3);
                if (afterSecond) yield return Wait(() => pattern.CycleVolleyCount == 2, 2, "B death second");
                else yield return new WaitForSeconds(.4f);
                var facing = enemy.PatternFacing; var bank = settings.presentation.directionalPresentation.Capture(); var expected = enemy.Config.deathSprites.Select(f => bank.Resolve(f, facing)).ToArray();
                var body = Body(); var life = enemy.GetComponent<EnemyPatternLifetime>(); uint generation = enemy.SpawnGeneration;
                enemy.TakeDamage(10000); Require(life.OwnedObjectCount == 0 && !enemy.IsPatternPositionLocked, "B death instant cleanup");
                Move(origin); health.ResetHealth(); hp = health.CurrentHealth; var seen = new HashSet<Sprite>(); until = Time.time + 2;
                while (enemy.gameObject.activeSelf) { Require(Time.time < until && body.flipX == bank.Flip(facing), "B death facing"); seen.Add(body.sprite); yield return null; }
                Require(expected.All(seen.Contains), "B uses six existing directional deaths"); Equal(hp, health.CurrentHealth, "no postdeath wave2");
                Spawn(); Require(enemy.SpawnGeneration != generation && pattern.CycleHitBudget == null && pattern.SecondVolley == null && pattern.SecondWavePellets.Length == 0, "B pool clean");
                Pass($"DEATH B {targetAngle:0}deg/{(afterSecond ? "second" : "rotation")}: correct six-frame death, both waves/locks cleared, clean pool budget");
            }
            var saved = origin;
            foreach (bool afterSecond in new[] { false, true })
            {
                if (enemy != null) enemy.ReleaseToPool(); origin = fieldOrigin; field.enabled = true;
                Move(origin + Vector3.right * 4); field.Refresh(player.transform.position); enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<PollenInvaderElitePattern>();
                yield return Wait(() => pattern.CycleVolleyCount >= (afterSecond ? 2 : 1), 4, "B field phase");
                var life = enemy.GetComponent<EnemyPatternLifetime>(); var point = field.Plan.placements.Single(); uint generation = enemy.SpawnGeneration;
                Move(biome.GridToWorldWithHeight(point.x < 150 ? 280 : 15, point.y < 150 ? 280 : 15)); field.Refresh(player.transform.position); yield return null;
                Require(life.OwnedObjectCount == 0 && !life.IsCurrent(generation) && !SaveService.IsBiomeEliteDefeated(point.spawnId), "B field unload not a kill");
                Move(origin + Vector3.right * 5); field.Refresh(player.transform.position); enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<PollenInvaderElitePattern>();
                Require(pattern.CycleHitBudget == null && pattern.CycleVolleyCount == 0 && !enemy.IsPatternPositionLocked, "B field return clean"); field.enabled = false; enemy = null;
                Pass(afterSecond ? "FIELD B second: actual chunk unload clears both waves, no defeat, fresh return" : "FIELD B rotation: actual chunk unload cancels pending second, no defeat, fresh return");
            }
            origin = saved;
        }

        private static IEnumerator FollowupBalanceChecks()
        {
            if (enemy != null && enemy.gameObject.activeSelf) enemy.ReleaseToPool(); origin = fieldOrigin; field.enabled = true;
            Move(origin + Vector3.right * 5); field.Refresh(player.transform.position); enemy = field.Spawners.Single().ActiveEnemy;
            var point = field.Plan.placements.Single(); string spawnId = point.spawnId; int xp = 0; Action<int> observe = n => xp += n; var callback = LevelUpManager.OnLevelUp;
            LevelUpManager.OnLevelUp = null; LevelUpManager.OnExpGained += observe;
            try { enemy.TakeDamage(10000); enemy.TakeDamage(10000); enemy.GrantExp(); }
            finally { LevelUpManager.OnLevelUp = callback; LevelUpManager.OnExpGained -= observe; }
            Equal(50, xp, "B XP once"); Require(SaveService.IsBiomeEliteDefeated(spawnId), "B field defeat saved");
            yield return Wait(() => !enemy.gameObject.activeSelf, 2, "B field death");
            Require(SaveService.TrySaveActiveRun(out string error), error); string run = SaveService.ActiveRunId, visit = biome.MonsterVisit.VisitId;
            SaveService.UseStorageRootForTests(storage); Require(SaveService.TryContinue(GameDifficulty.Normal, out error), error);
            Require(SaveService.TryRestorePendingSession(out var resume, out error) && resume == BiomeType.Lung, error);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null; BindBattleScene();
            Require(SaveService.ActiveRunId == run && biome.MonsterVisit.VisitId == visit && SaveService.IsBiomeEliteDefeated(spawnId), "B disk continue");
            field.Configure(biome, temporary, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Move(biome.GridToWorldWithHeight(point.x, point.y)); field.Refresh(player.transform.position); Require(field.Spawners.Count == 0, "B defeated point not respawned"); field.enabled = false;
            Pass("SAVE B: actual dedicated XP50 once, defeat persists through disk save/continue and scene reload without respawn");
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath);
            foreach (var difficulty in new[] { GameDifficulty.Normal, GameDifficulty.Hard })
            {
                SaveService.MarkFinalBossDefeated(); Require(SaveService.TryBeginNewGame(difficulty, out error), error); Require(SaveService.TryRestorePendingSession(out _, out error), error);
                GameManager.Instance.EnterBiome(BiomeType.Lung); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null; BindBattleScene();
                foreach (int stage in new[] { 0, 4 })
                {
                    Require(biome.MonsterVisit.Stage == stage && biome.MonsterVisit.Difficulty == difficulty, "B actual difficulty/visit stage");
                    origin = FindPatch(0); Spawn(); enemy.ApplyBalancePhase("Default", true);
                    var source = definition.statSets[0]; var multi = catalog.difficultyCatalog.Get(difficulty).elites; Require(catalog.progression.TryGetStage(stage, out var progression), "stage source");
                    Equal(CharacterStats.ToHealthUnits(source.maxHealth * multi.maxHealth * progression.maxHealth), enemy.Stats.MaxHealth, "B HP once");
                    Equal(source.moveSpeed * multi.moveSpeed, enemy.Stats.MoveSpeed, "B move once"); enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
                    float raw = source.attackPower * multi.outgoingDamage * progression.attackPower * definition.patternDamage.Single(p => p.id == PollenInvaderPatternSettings.FollowupDamageId).coefficient;
                    Equal(raw, enemy.CreatePatternDamage(PollenInvaderPatternSettings.FollowupDamageId, 0).Amount, "B damage once");
                    Move(origin + Vector3.right * 4); yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "B stage first"); Move(origin - Vector3.right * 3);
                    yield return Wait(() => pattern.CycleVolleyCount == 2, 2, "B stage second"); yield return new WaitForSeconds(.5f);
                    var second = pattern.SecondWavePellets[1]; Move(second.transform.position + second.Direction * 1.6f); health.ResetHealth(); float hp = health.CurrentHealth;
                    yield return Wait(() => pattern.SecondVolley.WaveDamageApplications == 1, 1, "B stage actual second hit");
                    Equal(CharacterStats.ToHealthUnits(raw), hp - health.CurrentHealth, "B actual second damage"); Require(pattern.FirstVolley.WaveDamageApplications == 0, "stage second-only hit");
                    Move(origin - Vector3.right * 3); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Ready, 2, "B stage recovery");
                    Require(Mathf.Abs(pattern.NextReadyTime - Time.time - settings.followup.rearmSeconds * multi.attackCooldown) < .07f, "B cooldown multiplier once");
                    Equal(.8f, pattern.RotationDuration, "stage does not shorten rotation"); Equal(.16f, pattern.PelletRadius, "stage radius unchanged");
                    xp = 0; enemy.SuppressExperienceReward = false; callback = LevelUpManager.OnLevelUp; LevelUpManager.OnLevelUp = null; LevelUpManager.OnExpGained += observe;
                    try { enemy.TakeDamage(10000); enemy.TakeDamage(10000); enemy.GrantExp(); }
                    finally { LevelUpManager.OnLevelUp = callback; LevelUpManager.OnExpGained -= observe; }
                    int expectedXp = (int)Math.Round((double)definition.reward.baseExperience * multi.experienceReward * progression.experience, MidpointRounding.ToEven);
                    Equal(expectedXp, xp, "B reward once"); yield return Wait(() => !enemy.gameObject.activeSelf, 2, "B stage death");
                    Pass($"BALANCE B {difficulty}/stage{stage}: second actual damage{CharacterStats.ToHealthUnits(raw)}, XP{xp} once, HP/move/rearm multipliers once and stable cue/radius");
                    if (stage == 4) continue;
                    foreach (var relic in new[] { BiomeType.Intestine, BiomeType.Liver, BiomeType.Stomach, BiomeType.Lung }) GameManager.Instance.CollectRelic(relic);
                    Require(biome.MonsterVisit.Stage == 0, "B visit remains frozen"); Spawn(); Require(enemy.Balance.Current.Stage == 0, "B respawn same frozen visit"); enemy.ReleaseToPool();
                    GameManager.Instance.ReturnToHub(); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_HUB);
                    GameManager.Instance.EnterBiome(BiomeType.Lung); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null; BindBattleScene();
                }
            }
        }
    }
}
