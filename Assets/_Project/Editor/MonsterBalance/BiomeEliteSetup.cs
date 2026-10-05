using System;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static class BiomeEliteSetup
    {
        public const string Root = "Assets/_Project/Data/BiomeElites";

        [MenuItem("Necrocis/Balance/Set Up Biome Elite Placement (C-03)")]
        public static void Run()
        {
            if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets/_Project/Data", "BiomeElites");
            MonsterBalanceCatalog catalog = MonsterBalanceSetup.EnsureAssets();
            foreach (string name in new[] { "Intestine", "Liver", "Lung", "Stomach" })
            {
                string path = Root + "/" + name + "BiomeEliteSpawnConfig.asset";
                var config = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(path);
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>();
                    // No unimplemented species in shipped maps. A/B and MAP stages populate this list.
                    AssetDatabase.CreateAsset(config, path);
                }
                EditorUtility.SetDirty(config);
                var biome = AssetDatabase.LoadAssetAtPath<BiomeConfig>("Assets/_Project/Data/BiomeConfigs/" + name + "BiomeConfig.asset");
                if (biome == null) throw new InvalidOperationException(name + " biome config missing");
                if (biome.biomeEliteSpawnConfig == null) { biome.biomeEliteSpawnConfig = config; EditorUtility.SetDirty(biome); }
                if (!catalog.biomeEliteSpawns.Contains(biome.biomeEliteSpawnConfig))
                { catalog.biomeEliteSpawns.Add(biome.biomeEliteSpawnConfig); EditorUtility.SetDirty(catalog); }
            }
            AssetDatabase.SaveAssets();
            Selection.activeObject = catalog;
            Debug.Log("[BiomeElite-C03] Four independent biome placement configs connected; legacy kill spawner unchanged.");
        }
    }
}
