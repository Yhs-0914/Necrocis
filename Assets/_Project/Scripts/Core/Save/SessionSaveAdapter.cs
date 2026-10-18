using System;
using System.Collections.Generic;

namespace Necrocis
{
    // Runtime capture/apply only. Source resolution and session/persistence decisions belong to callers.
    internal sealed class SessionSaveAdapter
    {
        internal readonly struct SourceBindings
        {
            public PlayerController Player { get; }
            public PlayerStats Stats { get; }
            public PlayerItemManager Items { get; }
            public PlayerItemCombatEffects ItemEffects { get; }
            public PlayerDamageCoordinator DamageCoordinator { get; }
            public GameManager GameManager { get; }
            public string SceneName { get; }

            public SourceBindings(PlayerController player, PlayerStats stats, PlayerItemManager items,
                GameManager gameManager, string sceneName, PlayerItemCombatEffects itemEffects = null,
                PlayerDamageCoordinator damageCoordinator = null)
            {
                Player = player;
                Stats = stats;
                Items = items;
                ItemEffects = itemEffects;
                DamageCoordinator = damageCoordinator;
                GameManager = gameManager;
                SceneName = sceneName;
            }
        }

        private readonly SourceBindings source;

        internal SessionSaveAdapter(SourceBindings source)
        {
            this.source = source;
        }

        internal void Capture(RunSaveData run)
        {
            run.player ??= new PlayerProgressSaveData();
            LevelUpManager.CaptureProgress(run.player);

            if (source.Stats != null)
            {
                run.player.currentHealth = source.Stats.CurrentHealth;
                run.player.healthDeltas = source.Stats.RuntimeStats.CapturePersistentHealthDeltas();
            }

            if (source.Items != null)
            {
                run.player.items = source.Items.CaptureSavedItems();
            }

            source.GameManager?.CaptureToSave(run);

            string sceneName = source.SceneName;
            if (!string.Equals(sceneName, SceneLoader.SCENE_MAIN_MENU, StringComparison.Ordinal))
            {
                run.checkpoint ??= new ResumeCheckpointSaveData();
                run.checkpoint.sceneName = sceneName;
                run.checkpoint.biome = source.GameManager?.CurrentBiome ?? BiomeType.None;
                run.checkpoint.wasInBossRoom = source.GameManager != null
                                                 && source.GameManager.CurrentState == GameState.InBossRoom;
                // The final arena currently has no encounter/checkpoint state. Resume safely at its hub portal.
                if (string.Equals(sceneName, SceneLoader.SCENE_HUB, StringComparison.Ordinal)
                    || string.Equals(sceneName, SceneLoader.SCENE_FINAL_BOSS, StringComparison.Ordinal))
                {
                    run.checkpoint.sceneName = SceneLoader.SCENE_HUB;
                    run.checkpoint.biome = BiomeType.None;
                    run.checkpoint.wasInBossRoom = false;
                }
            }
        }

        internal bool TryRestore(RunSaveData run, out string error)
        {
            error = string.Empty;
            PlayerController player = source.Player;
            PlayerStats playerStats = source.Stats;
            PlayerItemManager itemManager = source.Items;
            if (player == null || playerStats == null || itemManager == null)
            {
                error = "플레이어 저장 데이터를 복원할 런타임 컴포넌트가 준비되지 않았습니다.";
                return false;
            }

            PlayerProgressSaveData progress = run.player ?? new PlayerProgressSaveData();
            // Start a new recipient/source life before any reset or item restoration.
            if (source.DamageCoordinator != null) source.DamageCoordinator.BeginLifetime();
            else playerStats.BeginHealthLifetime();
            LevelUpManager.RestoreProgress(progress);

            playerStats.ResetBaseStats(true);
            playerStats.ResetStats();
            IReadOnlyList<SavedStatModifierData> modifiers = progress.levelUpModifiers;
            if (modifiers != null)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    SavedStatModifierData modifier = modifiers[i];
                    if (modifier == null)
                    {
                        continue;
                    }

                    playerStats.ApplyModifier(new CharacterStatModifier(
                        modifier.statType,
                        modifier.value,
                        modifier.mode,
                        LevelUpManager.RestoredModifierSource));
                }
            }

            itemManager.RestoreSavedItems(progress.items);
            playerStats.RuntimeStats.SetCurrentHealth(
                progress.currentHealth < 0f ? playerStats.MaxHealth : progress.currentHealth);
            player.ReviveRuntimeStateAfterLoad();
            playerStats.RuntimeStats.RestorePersistentHealthDeltas(progress.healthDeltas,
                key => playerStats.ResolvePersistentHealthEffectSource(key)
                    ?? source.ItemEffects?.ResolvePersistentItemHealthEffectSource(key));

            source.GameManager?.RestoreFromSave(run);
            return true;
        }
    }
}
