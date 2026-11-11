// Pure-math harness only. This is NOT a substitute for Unity physics, coroutines,
// serialization or play testing. Compile the actual game math against these small API shims.
namespace UnityEngine
{
    public class Object { public HideFlags hideFlags; }
    public class MonoBehaviour : Object { }
    public class ScriptableObject : Object { public static T CreateInstance<T>() where T : new() => new T(); }
    public enum HideFlags { HideAndDontSave }
    public struct Vector2Int { public int x, y; public Vector2Int(int x, int y) { this.x = x; this.y = y; } }
    public class MinAttribute(float n) : Attribute { }
    public class HideInInspector : Attribute { }
    public class RangeAttribute(float min, float max) : Attribute { }
    public class HeaderAttribute(string s) : Attribute { }
    public class TooltipAttribute(string s) : Attribute { }
    public class TextAreaAttribute : Attribute { public TextAreaAttribute() { } public TextAreaAttribute(int min, int max) { } }
    public class CreateAssetMenuAttribute : Attribute { public string menuName, fileName; }
    public static class Random { private static readonly System.Random Rng = new(414); public static float value => (float)Rng.NextDouble(); public static int Range(int min, int max) => Rng.Next(min, max); }
    public static class Resources { public static T Load<T>(string path) where T : class => null; }
    public static class Mathf
    {
        public static float Max(float a, float b) => Math.Max(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static float Min(float a, float b) => Math.Min(a, b);
        public static int Min(int a, int b) => Math.Min(a, b);
        public static float Clamp(float x, float min, float max) => Math.Clamp(x, min, max);
        public static int Clamp(int x, int min, int max) => Math.Clamp(x, min, max);
        public static float Floor(float x) => MathF.Floor(x);
        public static int RoundToInt(float x) => (int)MathF.Round(x, MidpointRounding.ToEven);
        public static bool Approximately(float a, float b) => MathF.Abs(a - b) <= 1e-6f * MathF.Max(1, MathF.Max(MathF.Abs(a), MathF.Abs(b)));
    }
}
namespace Necrocis
{
    public static class SaveService { public static void RecordPlayerHealing(float amount) { } }
    public class MonsterPatternSettings { public string GetValidationError(MonsterDefinition d) => null; }
    public class BiomeConfig { }
    public class PlayerStats { public float AttackPower, AttackSpeed = 1, AttackRange = 1, Magic, SkillCooldownReduction; }
    public class AudioManager { public static AudioManager Instance => null; public void PlaySFX(string key) { } }
    public static class DifficultyBalanceService
    {
        public static DifficultyBalanceProfile ActiveProfile;
        public static LevelProgressionConfig Progression;
        public static LevelProgressionConfig GetProgressionConfig() => Progression;
    }
    public class SavedStatModifierData { public CharacterStatType statType; public CharacterStatModifierMode mode; public float value; }
    public class PlayerProgressSaveData
    {
        public int level = 1, experience;
        public JobType job;
        public List<LevelUpStatChoice> selectionHistory = new();
        public List<SavedStatModifierData> levelUpModifiers = new();
    }
}
