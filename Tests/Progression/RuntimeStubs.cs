using System;
namespace UnityEngine
{
    public enum HideFlags { HideAndDontSave }
    public class ScriptableObject { public HideFlags hideFlags; public static T CreateInstance<T>() where T:new()=>new T(); }
    public static class Resources { public static T Load<T>(string path) where T:class => null; }
    public static class Mathf { public static int Max(int a,int b)=>Math.Max(a,b); public static float Max(float a,float b)=>Math.Max(a,b); public static int Min(int a,int b)=>Math.Min(a,b); public static int Clamp(int v,int a,int b)=>Math.Clamp(v,a,b); public static int RoundToInt(float v)=>(int)Math.Round(v); }
    public static class Random { static System.Random random=new System.Random(1); public static int Range(int a,int b)=>random.Next(a,b); }
}
namespace Necrocis
{
    // Unity/audio/config seams only. The progression state machine and saved DTOs are production sources.
    public enum GameDifficulty { Normal,Hard }
    public enum BiomeType { None,Intestine,Liver,Stomach,Lung }
    public enum CharacterStatType { MaxHealth,MoveSpeed,AttackPower,Magic }
    public enum CharacterStatModifierMode { Flat,PercentAdd }
    public sealed class HealthDeltaRemainderData {}
    public sealed class MonsterVisitSaveData {}
    public sealed class BiomeElitePlan {}
    public static class SceneNames { public const string Hub="Hub"; }
    public class AudioManager { public static AudioManager Instance => null; public void PlaySFX(string s){} }
    public static class ProgressionPolicyBindings { public static Func<LevelProgressionConfig> ResolveActiveConfig; }
    public class LevelProgressionConfig:UnityEngine.ScriptableObject
    {
        public const string DefaultResourcePath="Balance/LevelProgressionConfig";
        public int MaxLevel=>30;public int JobSelectionLevel=>5;public int JobBasedChoiceStartLevel=>6;
        public int midgameItemChoiceLevel=15;public int Skill2UnlockLevel=>10;public int EnemyKillExp=>10;
        public int GetSkillUnlockLevel(int slot)=>slot<=1?5:10;
        public int GetRequiredExpForCurrentLevel(int level)=>30+(level-1)*20;
        public float GetExpGainMultiplier(int level,JobType job)=>1f;
    }
}
