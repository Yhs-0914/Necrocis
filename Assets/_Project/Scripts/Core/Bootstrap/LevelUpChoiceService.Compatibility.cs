namespace Necrocis
{
    public static partial class LevelUpChoiceService
    {
        public static int? ApplyChoice(PlayerStats player,PlayerItemManager items,LevelProgressionConfig config,CharacterStatType stat,string source)
        {
            if (player==null) return null;
            return ApplyChoiceTo(player,items!=null && items.ContainsItem(PlayerItemIds.BioGambleId),config,stat,source,LevelUpManager.RecordResolvedModifier);
        }
    }
}
