using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NecrocisEditor
{
    public static class MonsterBalanceSmokeRunner
    {
        private static readonly List<UnityEngine.Object> temporary = new List<UnityEngine.Object>();
        private static readonly List<string> results = new List<string>();

        [MenuItem("Necrocis/Balance/Run C-01 Checks")]
        public static void Run()
        {
            results.Clear();
            try
            {
                Check("Formula, coefficient and cooldown applied once", CheckFormula);
                Check("Tier isolation and no hidden elite bonus", CheckTiers);
                Check("Exact stage, independent phase and immutable snapshot", CheckStageAndSnapshot);
                Check("Stationary / zero damage / disabled contact", CheckZeroValues);
                Check("Invalid and duplicate data fail explicitly", CheckInvalidData);
                Check("Finite result / reward overflow validation", CheckOverflow);
                Check("Final-only banker's rounding", CheckRounding);
                MonsterBalanceCatalog catalog = MonsterBalanceSetup.EnsureAssets();
                Check("Catalog assets / references / 8 elite IDs", () => CheckCatalog(catalog));
                Check("Serialized edit, Undo, Inspector and phase selection", () => CheckEditor(catalog));
                WriteReport(true, null);
                Debug.Log("[MonsterBalanceSmoke] PASS: " + results.Count + " checks");
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                WriteReport(false, exception.ToString());
                Debug.LogException(exception);
                if (Application.isBatchMode) EditorApplication.Exit(1);
                else throw;
            }
            finally
            {
                foreach (UnityEngine.Object value in temporary)
                    if (value != null) UnityEngine.Object.DestroyImmediate(value);
                temporary.Clear();
            }
        }

        private static T Temp<T>() where T : ScriptableObject
        {
            T value = ScriptableObject.CreateInstance<T>();
            temporary.Add(value);
            return value;
        }

        private static void Fixture(out MonsterDefinition definition, out DifficultyBalanceProfile difficulty, out MonsterProgressionProfile progression)
        {
            definition = Temp<MonsterDefinition>();
            definition.monsterId = "test.elite";
            definition.displayName = "검산용";
            definition.statSets[0].maxHealth = 100f;
            definition.statSets[0].attackPower = 8f;
            definition.statSets[0].moveSpeed = 2f;
            definition.reward.baseExperience = 40;
            definition.contact.damageCoefficient = 0.5f;
            difficulty = Temp<DifficultyBalanceProfile>();
            difficulty.difficulty = GameDifficulty.Hard;
            difficulty.elites = new EnemyDifficultyBalance
            { maxHealth = 1.2f, outgoingDamage = 1.25f, moveSpeed = 1.1f, experienceReward = 1.1f, attackCooldown = 0.8f };
            progression = Temp<MonsterProgressionProfile>();
        }

        private static MonsterBalanceSnapshot Resolve(MonsterDefinition d, DifficultyBalanceProfile p, MonsterProgressionProfile s, int stage = 2, string phase = "Default")
        {
            Require(MonsterBalanceResolver.TryResolve(d, phase, p, s, stage, out MonsterBalanceSnapshot result, out string error), error);
            return result;
        }

        private static void CheckFormula()
        {
            Fixture(out var d, out var p, out var s);
            string before = JsonUtility.ToJson(d) + JsonUtility.ToJson(p) + JsonUtility.ToJson(s);
            MonsterBalanceSnapshot value = Resolve(d, p, s);
            Equal(162f, value.MaxHealth); Equal(12f, value.AttackPower); Equal(2.2f, value.MoveSpeed);
            Require(value.Experience == 55, "XP must be 55");
            Equal(18f, value.GetPatternDamage(1.5f)); Equal(6f, value.ContactDamage);
            Equal(2.4f, value.GetRearmCooldown(3f));
            Require(before == JsonUtility.ToJson(d) + JsonUtility.ToJson(p) + JsonUtility.ToJson(s), "Resolver mutated source data.");
        }

        private static void CheckTiers()
        {
            Fixture(out var d, out var p, out var s);
            p.enemies.maxHealth = 2f; p.bosses.maxHealth = 3f;
            d.tier = MonsterTier.Normal; Equal(200f, Resolve(d,p,s,0).MaxHealth);
            d.tier = MonsterTier.Elite; Equal(120f, Resolve(d,p,s,0).MaxHealth);
            d.tier = MonsterTier.Boss; Equal(300f, Resolve(d,p,s,0).MaxHealth);
            p.difficulty = GameDifficulty.Normal;
            d.tier = MonsterTier.Elite;
            Require(Resolve(d,p,s,0).Difficulty == GameDifficulty.Normal, "Wrong explicit difficulty.");
        }

        private static void CheckStageAndSnapshot()
        {
            Fixture(out var d, out var p, out var s);
            MonsterBalanceSnapshot frozen = Resolve(d,p,s);
            d.statSets.Add(new MonsterStatSet { id = "Phase2", maxHealth = 200f, attackPower = 12f, moveSpeed = 0f });
            Equal(324f, Resolve(d,p,s,2,"Phase2").MaxHealth);
            Equal(162f, Resolve(d,p,s,2,"Default").MaxHealth);
            Equal(138f, Resolve(d,p,s,1).MaxHealth);
            d.statSets[0].maxHealth = 500f; p.elites.maxHealth = 4f; s.stages[2].maxHealth = 8f;
            Equal(162f, frozen.MaxHealth);
            Equal(18f, frozen.GetPatternDamage(1.5f));
            Equal(16000f, Resolve(d,p,s).MaxHealth);
        }

        private static void CheckZeroValues()
        {
            Fixture(out var d, out var p, out var s);
            d.statSets[0].moveSpeed = 0f; d.statSets[0].attackPower = 0f;
            d.reward.baseExperience = 0; d.contact.enabled = false;
            var value = Resolve(d,p,s);
            Equal(0f,value.MoveSpeed); Equal(0f,value.AttackPower); Equal(0f,value.ContactDamage);
            Require(value.Experience == 0, "Zero reward must stay zero.");
        }

        private static void CheckInvalidData()
        {
            Fixture(out var d, out var p, out var s);
            Reject(d,p,s,-1); Reject(d,p,s,5); Reject(d,p,s,2,"Missing");
            s.stages[4].stage = 2; Reject(d,p,s); s.stages = MonsterProgressionProfile.CreateRecommendedStages();
            d.statSets.Add(new MonsterStatSet()); Reject(d,p,s); d.statSets.RemoveAt(1);
            d.monsterId = ""; Reject(d,p,s); d.monsterId = "test.elite";
            d.tier = (MonsterTier)999; Reject(d,p,s); d.tier = MonsterTier.Elite;
            p.elites = null; Reject(d,p,s); p.elites = new EnemyDifficultyBalance();
            d.statSets[0].maxHealth = float.NaN; Reject(d,p,s); d.statSets[0].maxHealth = 100f;
            d.contact.damageCoefficient = -1f; Reject(d,p,s); d.contact.damageCoefficient = 0.5f;
            Require(!MonsterBalanceResolver.TryResolve(d,"Default",null,s,0,out _,out _),"Missing profile accepted.");
        }

        private static void CheckOverflow()
        {
            Fixture(out var d, out var p, out var s);
            d.statSets[0].maxHealth = float.MaxValue; p.elites.maxHealth = 2f;
            Reject(d,p,s);
            d.statSets[0].maxHealth = 100f; p.elites.maxHealth = 1f;
            d.reward.baseExperience = int.MaxValue; p.elites.experienceReward = 2f; Reject(d,p,s);
            d.reward.baseExperience = 40; p.elites.experienceReward = 1f;
            var value = Resolve(d,p,s);
            bool threw = false;
            try { value.GetPatternDamage(float.NaN); } catch (ArgumentOutOfRangeException) { threw = true; }
            Require(threw,"Non-finite damage coefficient accepted.");
        }

        private static void CheckRounding()
        {
            Fixture(out var d, out var p, out var s);
            d.reward.baseExperience = 5; p.elites.experienceReward = 0.5f;
            Require(Resolve(d,p,s,0).Experience == Mathf.RoundToInt(2.5f), "Rounding must match RoundToInt.");
            d.reward.baseExperience = 3; p.elites.experienceReward = 0.5f; s.stages[1].experience = 1.5f;
            Require(Resolve(d,p,s,1).Experience == 2, "Do not round 1.5 before applying progression.");
        }

        private static void CheckCatalog(MonsterBalanceCatalog catalog)
        {
            Require(catalog.GetValidationErrors().Count == 0, string.Join("; ",catalog.GetValidationErrors()));
            string[] expected = { "i-01","i-02","h-02","h-03","p-01","p-03","s-01","s-03" };
            foreach (string id in expected)
                Require(catalog.monsters.Any(m => m != null && m.monsterId == "elite."+id && m.tier == MonsterTier.Elite), "Missing "+id);
            Require(AssetDatabase.GetAssetPath(catalog.difficultyCatalog).Contains("/Balance/Difficulty/"), "Duplicate difficulty catalog.");

            var copy = Temp<MonsterBalanceCatalog>();
            copy.difficultyCatalog = catalog.difficultyCatalog; copy.progression = catalog.progression;
            copy.monsters = new List<MonsterDefinition> { catalog.monsters[0], catalog.monsters[0] };
            Require(copy.GetValidationErrors().Any(e=>e.Contains("중복")), "Duplicate IDs accepted.");
        }

        private static void CheckEditor(MonsterBalanceCatalog catalog)
        {
            Fixture(out var d, out var p, out var s);
            Undo.IncrementCurrentGroup();
            using (var serialized = new SerializedObject(d))
            {
                serialized.FindProperty("statSets").GetArrayElementAtIndex(0).FindPropertyRelative("maxHealth").floatValue = 80f;
                serialized.ApplyModifiedProperties();
            }
            Equal(129.6f,Resolve(d,p,s).MaxHealth);
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Equal(162f,Resolve(d,p,s).MaxHealth);
            Undo.ClearUndo(d);
            Undo.IncrementCurrentGroup();

            Editor inspector = Editor.CreateEditor(catalog);
            try
            {
                VisualElement tree = inspector.CreateInspectorGUI();
                Require(tree.Q<DropdownField>("monster-selector") != null, "Missing selector.");
                Require(tree.Q<SliderInt>("stage-selector") != null, "Missing stage preview.");
                Require(tree.Q("balance-results").childCount >= 8, "Missing calculated read-only rows.");
            }
            finally { UnityEngine.Object.DestroyImmediate(inspector); }

            d.statSets.Add(new MonsterStatSet { id = "Phase2", maxHealth = 200f });
            var previewCatalog = Temp<MonsterBalanceCatalog>();
            previewCatalog.monsters = new List<MonsterDefinition> { d };
            previewCatalog.progression = s;
            previewCatalog.difficultyCatalog = Temp<DifficultyBalanceCatalog>();
            previewCatalog.difficultyCatalog.normal = Temp<DifficultyBalanceProfile>();
            previewCatalog.difficultyCatalog.hard = p;
            inspector = Editor.CreateEditor(previewCatalog);
            try
            {
                VisualElement tree = inspector.CreateInspectorGUI();
                tree.Q<DropdownField>("statset-selector").value = "Phase2";
                tree.Q<EnumField>("difficulty-selector").value = GameDifficulty.Hard;
                Require(tree.Q<DropdownField>("statset-selector").value == "Phase2", "Difficulty switch lost selected phase.");
            }
            finally { UnityEngine.Object.DestroyImmediate(inspector); }
        }

        private static void Reject(MonsterDefinition d, DifficultyBalanceProfile p, MonsterProgressionProfile s, int stage = 2, string phase = "Default")
        {
            Require(!MonsterBalanceResolver.TryResolve(d,phase,p,s,stage,out _,out string error) && !string.IsNullOrEmpty(error), "Invalid input must fail explicitly.");
        }

        private static void Check(string name, Action action) { action(); results.Add("PASS " + name); }
        private static void Equal(float expected, float actual) { Require(Mathf.Abs(expected-actual) < 0.002f, $"Expected {expected}, got {actual}"); }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

        private static void WriteReport(bool success, string error)
        {
            string directory = Path.Combine(Application.dataPath,"../Logs");
            Directory.CreateDirectory(directory);
            File.WriteAllLines(Path.Combine(directory,"MonsterBalance-C01-results.txt"),
                new[] { "Monster Balance C-01", "Success: " + success }.Concat(results)
                .Concat(error == null ? Array.Empty<string>() : new[] { error }));
        }
    }
}
