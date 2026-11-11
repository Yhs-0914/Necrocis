using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public sealed class WorldRecoveryPickup : MonoBehaviour
    {
        private static readonly HashSet<WorldRewardSaveData> active = new HashSet<WorldRewardSaveData>();
        private WorldRewardSaveData reward;

        public static void Spawn(WorldRewardSaveData state)
        {
            if (state == null || state.collected || active.Contains(state)) return;
            var biome = BiomeManager.Active;
            if (biome == null || biome.BiomeType != state.biome) return;
            Vector2Int grid = biome.WorldToGrid(new Vector3(state.x, state.y, state.z));
            bool valid = biome.IsValidPosition(grid.x, grid.y) && biome.IsWalkable(grid.x, grid.y);
            if (!valid)
            {
                for (int radius = 1; radius <= 4 && !valid; radius++)
                    for (int x = -radius; x <= radius && !valid; x++)
                        for (int y = -radius; y <= radius && !valid; y++)
                        {
                            var candidate = new Vector2Int(grid.x + x, grid.y + y);
                            if (!biome.IsValidPosition(candidate.x, candidate.y) || !biome.IsWalkable(candidate.x, candidate.y)) continue;
                            Vector3 position = biome.GridToWorld(candidate.x, candidate.y);
                            state.x = position.x; state.z = position.z;
                            grid = candidate;
                            valid = true;
                        }
            }
            if (!valid) return;
            var go = new GameObject("Recovery_" + state.id);
            go.transform.SetParent(biome.transform, true);
            go.transform.position = new Vector3(state.x, biome.GetGroundHeight(grid.x, grid.y), state.z);
            var pickup = go.AddComponent<WorldRecoveryPickup>();
            pickup.reward = state;
            active.Add(state);
            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = .8f;
            trigger.center = Vector3.up * .55f;
            var visual = new GameObject("Heart");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = Vector3.up * .65f;
            var renderer = visual.AddComponent<SpriteRenderer>();
            var hud = FindFirstObjectByType<PlayerHeartUI>();
            renderer.sprite = hud != null && hud.FullHeartSprite != null ? hud.FullHeartSprite : TextureSpriteCache.GetCircleSprite();
            renderer.color = hud != null && hud.FullHeartSprite != null ? Color.white : new Color(1f, .15f, .25f);
            renderer.sortingOrder = 1000;
            visual.transform.localScale = Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(renderer.sprite, .9f);
            visual.AddComponent<Billboard>().SetUpdateMode(Billboard.UpdateMode.Once);
        }

        private void OnTriggerStay(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            var stats = player != null ? player.Stats : null;
            if (reward == null || reward.collected || stats == null || stats.IsDead
                || stats.CurrentHealth >= stats.MaxHealth) return;
            stats.Heal(CampaignRewardRules.RecoveryHealth);
            SaveService.CompleteWorldReward(reward);
            AudioManager.Instance?.PlaySFX("ItemPickup");
            Destroy(gameObject);
        }

        private void OnDestroy() { if (reward != null) active.Remove(reward); }
    }
}
