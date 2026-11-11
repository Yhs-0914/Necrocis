using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // Active five-choice policy. Legacy four-choice callers keep their existing adapter.
    public static partial class LevelUpChoiceService
    {
        public static List<CharacterStatType> BuildChoices()
        {
            var values=new List<CharacterStatType>{CharacterStatType.AttackPower,CharacterStatType.AttackRange,CharacterStatType.AttackSpeed,CharacterStatType.Magic,CharacterStatType.MoveSpeed,CharacterStatType.MaxHealth};
            for(int i=values.Count-1;i>0;i--){int j=Random.Range(0,i+1);(values[i],values[j])=(values[j],values[i]);}
            values.RemoveAt(5);return values;
        }
        public static int? ApplyChoiceTo(IPlayerGrowthRecipient player,bool ownsBioGamble,LevelProgressionConfig config,CharacterStatType stat,string source, System.Action<CharacterStatType,float,CharacterStatModifierMode> recordResolved)
        {
            if(player==null || !player.IsAvailable)return null;
            if(config!=null && config.bioGambleEnabled && ownsBioGamble)
            {
                if (!config.TryGetLevelUpStatValue(stat, out var growth)) return null;
                int delta = config.RollGrowthMultiplier();
                float value = config.GetRuntimeModifierValue(growth) * delta;
                player.ApplyModifier(new CharacterStatModifier(stat, value, growth.mode, source));
                recordResolved(stat, value, growth.mode);
                if (growth.healWhenPositive && stat == CharacterStatType.MaxHealth && value > 0f) player.Heal(value);
                return delta;
            }
            if(config==null || !config.TryGetLevelUpStatValue(stat,out var setting))
            {Debug.LogWarning($"[LevelUpStackChoiceUI] Missing level-up stat config for {stat}.");return null;}
            float modifier=config.GetRuntimeModifierValue(setting);
            player.ApplyModifier(new CharacterStatModifier(stat,modifier,setting.mode,source));recordResolved(stat,modifier,setting.mode);
            if(setting.healWhenPositive && stat==CharacterStatType.MaxHealth && setting.mode==CharacterStatModifierMode.Flat && setting.value>0f)player.Heal(setting.value);
            return null;
        }
        public static LevelUpStatChoice ToLegacyChoice(CharacterStatType stat) => stat switch
        {
            CharacterStatType.AttackPower=>LevelUpStatChoice.AttackPowerUp,
            CharacterStatType.AttackRange=>LevelUpStatChoice.AttackSpeedRangeUp,
            CharacterStatType.AttackSpeed=>LevelUpStatChoice.AttackSpeedRangeUp,
            CharacterStatType.Magic=>LevelUpStatChoice.MagicUp,
            CharacterStatType.MoveSpeed=>LevelUpStatChoice.SpeedUp,
            _=>LevelUpStatChoice.HealthUp
        };
        public static void RecordSelection(CharacterStatType stat) => LevelUpManager.RecordSelection(ToLegacyChoice(stat));
    }
}
