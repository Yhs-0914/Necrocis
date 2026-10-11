using System;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static class MergeIntegrationSetup
    {
        public const string FinalBossPath = "Assets/_Project/Resources/Balance/FinalBoss/FinalBossBalanceConfig.asset";

        [MenuItem("Necrocis/Balance/Open Final Boss Balance")]
        public static void OpenFinalBossBalance()
        {
            EnsureFinalBossBalance();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<FinalBossBalanceConfig>(FinalBossPath);
        }

        [MenuItem("Necrocis/Balance/Complete Main Integration")]
        public static void Run()
        {
            MonsterBalanceMigration.Run();
            EnsureFinalBossBalance();
            AssetDatabase.SaveAssets();
            Debug.Log("[MergeIntegration] All biome enemies use MonsterDefinition; final boss balance is independent.");
        }

        private static void EnsureFinalBossBalance()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/Balance/FinalBoss"))
                AssetDatabase.CreateFolder("Assets/_Project/Resources/Balance", "FinalBoss");
            var asset = AssetDatabase.LoadAssetAtPath<FinalBossBalanceConfig>(FinalBossPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<FinalBossBalanceConfig>();
                AssetDatabase.CreateAsset(asset, FinalBossPath);
            }
            string error = asset.GetValidationError();
            if (error != null) throw new InvalidOperationException(error);
        }
    }
}
