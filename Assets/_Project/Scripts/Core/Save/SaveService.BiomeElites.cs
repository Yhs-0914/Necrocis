using System;
using System.Collections.Generic;

namespace Necrocis
{
    public static partial class SaveService
    {
        private static int visitSceneHandle = int.MinValue;
        private static MonsterVisitContext loadedVisit;
        private static BiomeType resumeVisitBiome;
        public static string ActiveRunId => HasActiveSession ? activeRun.saveId : null;
        public static int RunMonsterStage => HasActiveSession ? Math.Min(4, activeRun.bosses?.DefeatedCount ?? 0) : 0;

        // Called once per loaded biome scene, before that scene spawns any enemy.
        public static MonsterVisitContext EnterLoadedBiome(BiomeType biome, int sceneHandle)
        {
            if (visitSceneHandle == sceneHandle && loadedVisit.Biome == biome && loadedVisit.RunId == ActiveRunId)
                return loadedVisit;
            if (!HasActiveSession)
            {
                loadedVisit = new MonsterVisitContext(null, Guid.NewGuid().ToString("N"), biome, GameDifficulty.Normal, 0);
                visitSceneHandle = sceneHandle;
                return loadedVisit;
            }
            MonsterVisitSaveData visit = activeRun.world.monsterVisit;
            bool resume = resumeVisitBiome == biome && visit != null && visit.biome == biome
                && !string.IsNullOrEmpty(visit.visitId) && visit.stageAtEntry >= 0 && visit.stageAtEntry <= 4;
            if (!resume)
                visit = new MonsterVisitSaveData { visitId = Guid.NewGuid().ToString("N"), biome = biome, stageAtEntry = RunMonsterStage };
            activeRun.world.monsterVisit = visit;
            resumeVisitBiome = BiomeType.None;
            loadedVisit = new MonsterVisitContext(activeRun.saveId, visit.visitId, biome, activeRun.difficulty, visit.stageAtEntry);
            visitSceneHandle = sceneHandle;
            if (!TryWriteRun(activeRun, out string error)) UnityEngine.Debug.LogError("[MonsterVisit] " + error);
            return loadedVisit;
        }

        private static void ResetLoadedVisit(bool prepareResume = false)
        {
            visitSceneHandle = int.MinValue;
            loadedVisit = default;
            resumeVisitBiome = prepareResume && HasActiveSession ? activeRun.checkpoint?.biome ?? BiomeType.None : BiomeType.None;
        }

        public static BiomeElitePlan GetBiomeElitePlan(BiomeType biome)
        {
            return HasActiveSession ? activeRun.world.biomeElitePlans.Find(p => p != null && p.biome == biome) : null;
        }

        public static bool TryStoreBiomeElitePlan(BiomeElitePlan plan, out string error)
        {
            error = null;
            if (!HasActiveSession) { error = "활성 런이 없습니다."; return false; }
            if (GetBiomeElitePlan(plan.biome) != null) { error = "이미 저장된 바이옴 배치를 덮어쓸 수 없습니다."; return false; }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (BiomeElitePlan existing in activeRun.world.biomeElitePlans)
                foreach (BiomeElitePlacement point in existing.placements) ids.Add(point.spawnId);
            foreach (BiomeElitePlacement point in plan.placements)
                if (point == null || string.IsNullOrWhiteSpace(point.spawnId) || string.IsNullOrWhiteSpace(point.monsterId) || !ids.Add(point.spawnId))
                { error = "바이옴 엘리트 배치 ID가 비었거나 중복되었습니다."; return false; }
            activeRun.world.biomeElitePlans.Add(plan);
            if (TryWriteRun(activeRun, out error)) return true;
            activeRun.world.biomeElitePlans.Remove(plan);
            return false;
        }

        public static bool IsBiomeEliteDefeated(string spawnId) => HasActiveSession && activeRun.world.defeatedBiomeEliteIds.Contains(spawnId);

        public static bool TryRecordBiomeEliteDefeat(string runId, string spawnId, out bool newlyRecorded, out string error)
        {
            newlyRecorded = false; error = null;
            if (!HasActiveSession || activeRun.saveId != runId) { error = "다른 런의 바이옴 엘리트입니다."; return false; }
            if (IsBiomeEliteDefeated(spawnId)) return true;
            bool known = activeRun.world.biomeElitePlans.Exists(p => p.placements.Exists(e => e.spawnId == spawnId));
            if (!known) { error = "등록되지 않은 바이옴 엘리트 스폰 ID입니다."; return false; }
            activeRun.world.defeatedBiomeEliteIds.Add(spawnId);
            if (!TryWriteRun(activeRun, out error)) { activeRun.world.defeatedBiomeEliteIds.Remove(spawnId); return false; }
            newlyRecorded = true;
            return true;
        }
    }
}
