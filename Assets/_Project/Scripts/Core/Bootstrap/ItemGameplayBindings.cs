namespace Necrocis
{
    public static class ItemGameplayBindings
    {
        public static void Register()
        {
            ItemGroundPlacementBindings.Resolve=ItemGroundPlacementPolicy.Resolve;
            ItemInventoryBindings.SlotOverride=()=>DifficultyBalanceService.ActiveProfile?.items?.maxSlotsOverride??0;
            ItemInventoryBindings.DescriptionPolicy=()=>{
                var config=LevelUpManager.Config;
                return new ItemDescriptionPolicy(config!=null && config.bioGambleEnabled,config!=null?config.bioGambleMinDelta:0,config!=null?config.bioGambleMaxDelta:0);
            };
            ItemInventoryBindings.StatRecipient=owner=>owner!=null?owner.GetComponent<PlayerStats>():null;
            ItemInventoryBindings.ApplyImplementation=(item,recipient)=>{
                if(recipient==null || recipient is PlayerStats)item.ApplyTo(recipient as PlayerStats);else item.ApplyToRecipient(recipient);
            };
            ItemInventoryBindings.RemoveImplementation=(item,recipient)=>{
                if(recipient==null || recipient is PlayerStats)item.RemoveFrom(recipient as PlayerStats);else item.RemoveFromRecipient(recipient);
            };
            ProgressionPolicyBindings.ResolveActiveConfig=DifficultyBalanceService.GetProgressionConfig;
            ProgressionPolicyBindings.BuildPlayerBaseStats=()=>{
                var values=LevelUpManager.Config.BuildPlayerBaseStats();
                for(int i=0;i<values.Length;i++){var value=values[i];value.value*=DifficultyBalanceService.GetPlayerBaseStatMultiplier(value.statType);values[i]=value;}
                return values;
            };
            GrowthChoiceBindings.ResolveRecipient=()=>PlayerStats.Instance??UnityEngine.Object.FindFirstObjectByType<PlayerStats>();
            GrowthChoiceBindings.OwnsBioGamble=recipient=>{
                var stats=recipient as PlayerStats;var items=PlayerItemManager.Instance??(stats!=null?stats.GetComponent<PlayerItemManager>():null);
                return items!=null && items.ContainsItem(PlayerItemIds.BioGambleId);
            };
            GrowthChoiceBindings.AnyPlayerOwnsBioGamble=()=>{
                var items=PlayerItemManager.Instance??UnityEngine.Object.FindFirstObjectByType<PlayerItemManager>();
                return items!=null && items.ContainsItem(PlayerItemIds.BioGambleId);
            };
            EnemyDefeatBindings.PlayerKillReaction=enemy=>{
                var player=PlayerController.Instance;
                if(player!=null)player.GetComponent<PlayerItemCombatEffects>()?.NotifyEnemyDefeatedByPlayer(enemy);
            };
        }
    }
}
