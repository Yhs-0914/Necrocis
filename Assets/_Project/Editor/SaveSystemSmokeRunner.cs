using System;
using System.IO;
using System.Reflection;
using Necrocis;
using ProceduralMap;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static class SaveSystemSmokeRunner
    {
        public static void Run()
        {
            string storageRoot = Path.Combine(
                Path.GetTempPath(),
                $"necrocis-save-system-smoke-{Guid.NewGuid():N}");

            try
            {
                SaveService.UseStorageRootForTests(storageRoot);
                DifficultyBalanceProfile normalBalance =
                    DifficultyBalanceService.GetProfile(GameDifficulty.Normal);
                DifficultyBalanceProfile hardBalance =
                    DifficultyBalanceService.GetProfile(GameDifficulty.Hard);
                Require(normalBalance != null && hardBalance != null, "난이도별 Balance Profile을 읽지 못했습니다.");
                Require(normalBalance != hardBalance, "Normal과 Hard가 같은 Balance Profile을 공유합니다.");
                Require(
                    normalBalance.progression != null
                    && hardBalance.progression != null
                    && normalBalance.progression != hardBalance.progression,
                    "Normal과 Hard가 같은 Progression 에셋을 공유합니다.");

                Require(
                    SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error),
                    $"Normal 새 게임 생성 실패: {error}");
                Require(
                    DifficultyBalanceService.ActiveProfile == normalBalance,
                    "Normal 세션이 Normal Balance Profile을 선택하지 않았습니다.");
                Require(SaveService.HasContinueSave(GameDifficulty.Normal), "Normal 단일 슬롯이 생성되지 않았습니다.");

                int firstIntestineSeed = SaveService.GetOrCreateBiomeSeed(BiomeType.Intestine);
                int firstLiverSeed = SaveService.GetOrCreateBiomeSeed(BiomeType.Liver);
                int firstStomachSeed = SaveService.GetOrCreateBiomeSeed(BiomeType.Stomach);
                int firstLungSeed = SaveService.GetOrCreateBiomeSeed(BiomeType.Lung);
                Require(firstIntestineSeed != 0, "Normal 새 게임에 장 시드가 생성되지 않았습니다.");
                Require(firstLiverSeed != 0, "Normal 새 게임에 간 시드가 생성되지 않았습니다.");
                Require(firstStomachSeed != 0, "Normal 새 게임에 위 시드가 생성되지 않았습니다.");
                Require(firstLungSeed != 0, "Normal 새 게임에 폐 시드가 생성되지 않았습니다.");
                Require(
                    SaveService.GetOrCreateBiomeSeed(BiomeType.Intestine) == firstIntestineSeed,
                    "같은 Normal run에서 장 시드가 변경되었습니다.");

                SaveService.ResetStaticStateForTests();
                SaveService.UseStorageRootForTests(storageRoot);
                Require(
                    SaveService.TryContinue(GameDifficulty.Normal, out error),
                    $"시드 저장 검증용 Normal 계속하기 실패: {error}");
                Require(
                    SaveService.GetOrCreateBiomeSeed(BiomeType.Intestine) == firstIntestineSeed
                    && SaveService.GetOrCreateBiomeSeed(BiomeType.Liver) == firstLiverSeed
                    && SaveService.GetOrCreateBiomeSeed(BiomeType.Stomach) == firstStomachSeed
                    && SaveService.GetOrCreateBiomeSeed(BiomeType.Lung) == firstLungSeed,
                    "저장 후 계속하기에서 바이옴 시드가 유지되지 않았습니다.");
                VerifyProceduralMapUsesSavedSeed(firstIntestineSeed);

                SaveService.MarkBossDefeated(BiomeType.Intestine);
                Require(
                    SaveService.IsBossDefeated(BiomeType.Intestine),
                    "Normal run 보스 클리어 상태가 저장되지 않았습니다.");
                Require(
                    SaveService.IsBossDiscovered(BiomeType.Intestine),
                    "Profile 보스 컨셉아트 해금 상태가 저장되지 않았습니다.");

                SaveService.MarkFinalBossDefeated();
                Require(SaveService.IsHardUnlocked, "Normal 클리어 후 Hard가 해금되지 않았습니다.");
                Require(
                    SaveService.Profile.normalCampaignCompleted,
                    "Normal 캠페인 클리어 프로필 플래그가 기록되지 않았습니다.");

                Require(
                    SaveService.TryBeginNewGame(GameDifficulty.Normal, out error),
                    $"Normal 슬롯 덮어쓰기 실패: {error}");
                Require(
                    SaveService.GetOrCreateBiomeSeed(BiomeType.Intestine) != firstIntestineSeed
                    && SaveService.GetOrCreateBiomeSeed(BiomeType.Liver) != firstLiverSeed
                    && SaveService.GetOrCreateBiomeSeed(BiomeType.Stomach) != firstStomachSeed
                    && SaveService.GetOrCreateBiomeSeed(BiomeType.Lung) != firstLungSeed,
                    "Normal 새 게임이 이전 run의 바이옴 시드를 재사용했습니다.");
                Require(
                    !SaveService.IsBossDefeated(BiomeType.Intestine),
                    "Normal 새 게임이 기존 run 보스 진행도를 유지했습니다.");
                Require(
                    SaveService.IsBossDiscovered(BiomeType.Intestine),
                    "Normal 새 게임이 영구 컨셉아트 해금을 지웠습니다.");

                bool liverDiscoveryBeforeHardRun = SaveService.IsBossDiscovered(BiomeType.Liver);
                Require(
                    SaveService.TryBeginNewGame(GameDifficulty.Hard, out error),
                    $"Hard 새 게임 생성 실패: {error}");
                Require(
                    DifficultyBalanceService.ActiveProfile == hardBalance,
                    "Hard 세션이 Hard Balance Profile을 선택하지 않았습니다.");
                SaveService.MarkBossDefeated(BiomeType.Liver);
                Require(
                    SaveService.IsBossDiscovered(BiomeType.Liver) == liverDiscoveryBeforeHardRun,
                    "Hard run 보스 처치가 Profile 컨셉아트 해금에 섞였습니다.");
                Require(
                    SaveService.TryHandleHardDeath(out error),
                    $"Hard 사망 초기화 실패: {error}");
                Require(!SaveService.HasActiveSession, "Hard 사망 후 활성 세션 참조가 남아 있습니다.");
                Require(
                    !SaveService.HasContinueSave(GameDifficulty.Hard),
                    "Hard 사망 후 계속하기 슬롯이 남아 있습니다.");
                Require(
                    SaveService.HasContinueSave(GameDifficulty.Normal),
                    "Hard 사망이 독립된 Normal 슬롯까지 지웠습니다.");

                SaveService.ResetStaticStateForTests();
                SaveService.UseStorageRootForTests(storageRoot);
                Require(SaveService.IsHardUnlocked, "재시작 후 Hard 영구 해금이 복원되지 않았습니다.");
                Require(
                    SaveService.IsBossDiscovered(BiomeType.Intestine),
                    "재시작 후 영구 보스 컨셉아트 해금이 복원되지 않았습니다.");
                Require(
                    SaveService.HasContinueSave(GameDifficulty.Normal),
                    "재시작 후 Normal 단일 슬롯이 복원되지 않았습니다.");
                Require(
                    !SaveService.HasContinueSave(GameDifficulty.Hard),
                    "재시작 후 폐기된 Hard run이 되살아났습니다.");

                string saveDirectory = Path.Combine(storageRoot, "Saves");
                Require(File.Exists(Path.Combine(saveDirectory, "profile.json")), "profile.json이 생성되지 않았습니다.");
                Require(File.Exists(Path.Combine(saveDirectory, "normal.json")), "normal.json이 생성되지 않았습니다.");
                Require(File.Exists(Path.Combine(saveDirectory, "hard-run.json")), "hard-run.json이 생성되지 않았습니다.");

                File.WriteAllText(Path.Combine(saveDirectory, "profile.json"), "{ invalid-json");
                SaveService.ResetStaticStateForTests();
                SaveService.UseStorageRootForTests(storageRoot);
                Require(
                    SaveService.IsHardUnlocked,
                    "손상된 Profile 주 파일에서 backup Profile로 복구하지 못했습니다.");

                Debug.Log(
                    "[SaveSystemSmoke] PASS - profile, biome seeds, Normal slot, Hard unlock, Hard-death reset and backup recovery verified");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveSystemSmoke] FAIL - {exception}");
                EditorApplication.Exit(1);
            }
            finally
            {
                SaveService.ResetStaticStateForTests();
                if (Directory.Exists(storageRoot))
                {
                    Directory.Delete(storageRoot, true);
                }
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void VerifyProceduralMapUsesSavedSeed(int expectedSeed)
        {
            GameObject owner = new GameObject("Procedural Seed Smoke");
            owner.SetActive(false);
            BiomeConfig config = ScriptableObject.CreateInstance<BiomeConfig>();

            try
            {
                config.biomeType = BiomeType.Intestine;
                MapGenerator generator = owner.AddComponent<MapGenerator>();
                ProceduralBiomeBridge bridge = owner.AddComponent<ProceduralBiomeBridge>();
                bridge.Configure(config);
                MethodInfo awake = typeof(ProceduralBiomeBridge).GetMethod(
                    "Awake",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Require(awake != null, "ProceduralBiomeBridge.Awake를 찾지 못했습니다.");
                awake.Invoke(bridge, null);

                Require(
                    generator.RandomSeed == expectedSeed,
                    "절차 맵 생성기가 저장된 바이옴 시드를 사용하지 않았습니다.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
    }
}
