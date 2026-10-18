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
                int delta=Random.Range(config.bioGambleMinDelta,Mathf.Max(config.bioGambleMinDelta,config.bioGambleMaxDelta)+1);
                bool percent=stat==CharacterStatType.AttackRange || stat==CharacterStatType.AttackSpeed || stat==CharacterStatType.MoveSpeed;
                float value=percent?delta/100f:delta;
                var mode=percent?CharacterStatModifierMode.PercentAdd:CharacterStatModifierMode.Flat;
                player.ApplyModifier(new CharacterStatModifier(stat,value,mode,source));recordResolved(stat,value,mode);
                if(stat==CharacterStatType.MaxHealth && delta>0)player.Heal(delta);
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
