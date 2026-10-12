using System.Collections.Generic;
using System.IO;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static class MonsterBalanceSetup
    {
        public const string Root = "Assets/_Project/Data/MonsterBalance";
        public const string CatalogPath = "Assets/_Project/Resources/Balance/MonsterBalanceCatalog.asset";
        public const string ProgressionPath = Root + "/MonsterProgressionProfile.asset";
        private const string DifficultyPath = "Assets/_Project/Resources/Balance/Difficulty/DifficultyBalanceCatalog.asset";

        [MenuItem("Necrocis/Balance/Open Monster Balance")]
        public static void Open()
        {
            MonsterBalanceCatalog catalog = EnsureAssets();
            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
        }

        public static MonsterBalanceCatalog EnsureAssets()
        {
            EnsureFolder(Root);
            EnsureFolder("Assets/_Project/Resources/Balance");
            string oldCatalogPath = Root + "/MonsterBalanceCatalog.asset";
            if (AssetDatabase.LoadAssetAtPath<MonsterBalanceCatalog>(CatalogPath) == null
                && AssetDatabase.LoadAssetAtPath<MonsterBalanceCatalog>(oldCatalogPath) != null)
            {
                string error = AssetDatabase.MoveAsset(oldCatalogPath, CatalogPath);
                if (!string.IsNullOrEmpty(error)) throw new System.InvalidOperationException(error);
            }
            EnsureFolder(Root + "/Definitions");
            var difficulties = AssetDatabase.LoadAssetAtPath<DifficultyBalanceCatalog>(DifficultyPath);
            if (difficulties == null) throw new System.InvalidOperationException("기존 난이도 카탈로그를 찾을 수 없습니다.");
            InitializeEliteRow(difficulties.normal);
            InitializeEliteRow(difficulties.hard);

            MonsterProgressionProfile progression = GetOrCreate<MonsterProgressionProfile>(ProgressionPath, out _);
            var definitions = new List<MonsterDefinition>();
            string[] ids = { "I-01", "I-02", "H-02", "H-03", "P-01", "P-03", "S-01", "S-03" };
            string[] names = { "가스낭", "굳은 잔여체", "염증 불씨", "숙취 잔재", "분진 뭉치", "꽃가루 침입자", "헬리코 나선충", "기름막 활주체" };
            for (int i = 0; i < ids.Length; i++)
            {
                MonsterDefinition definition = GetOrCreate<MonsterDefinition>(Root + "/Definitions/" + ids[i] + ".asset", out bool created);
                if (created)
                {
                    definition.monsterId = "elite." + ids[i].ToLowerInvariant();
                    definition.displayName = ids[i] + " " + names[i];
                    definition.tier = MonsterTier.Elite;
                    definition.designNote = "C-01 초기 데이터 템플릿. 체력 30 / 공격력 2 / 경험치 50은 튜닝 전 값이며, 아직 전투·스폰에 연결하지 않았습니다.";
                    EditorUtility.SetDirty(definition);
                }
                definitions.Add(definition);
            }
            MonsterBalanceCatalog catalog = GetOrCreate<MonsterBalanceCatalog>(CatalogPath, out bool catalogCreated);
            if (catalogCreated)
            {
                catalog.difficultyCatalog = difficulties;
                catalog.progression = progression;
                catalog.monsters = definitions;
                EditorUtility.SetDirty(catalog);
            }
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static void InitializeEliteRow(DifficultyBalanceProfile profile)
        {
            if (profile == null) throw new System.InvalidOperationException("Normal/Hard 프로필 연결을 확인하세요.");
            string path = AssetDatabase.GetAssetPath(profile);
            if (File.ReadAllText(path).Contains("\n  elites:")) return;
            EnemyDifficultyBalance source = profile.enemies ?? new EnemyDifficultyBalance();
            profile.elites = new EnemyDifficultyBalance
            {
                maxHealth = source.maxHealth, moveSpeed = source.moveSpeed,
                outgoingDamage = source.outgoingDamage, attackCooldown = source.attackCooldown,
                experienceReward = source.experienceReward
            };
            EditorUtility.SetDirty(profile);
        }

        private static T GetOrCreate<T>(string path, out bool created) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            created = asset == null;
            if (!created) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
