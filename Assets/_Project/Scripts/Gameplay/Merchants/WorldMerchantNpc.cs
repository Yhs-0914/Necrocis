using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public sealed class WorldMerchantNpc : MonoBehaviour
    {
        public const string IdleResourcePath = "Merchants/CellMerchantIdle";
        private static Sprite[] cachedFrames;
        private static readonly List<WorldMerchantNpc> activeMerchants = new List<WorldMerchantNpc>();
        private static readonly float[] FrameDurations = { .9f, .18f, .12f, .25f };
        private SpriteRenderer visual;
        private int frame;
        private float elapsed;
        internal static IReadOnlyList<WorldMerchantNpc> ActiveMerchants => activeMerchants;
        public Vector2Int Cell { get; private set; }
        public int FrameIndex => frame;
        public bool IsVisible => visual != null && visual.enabled;
        public Vector3 PromptWorldPosition => visual != null && visual.sprite != null
            ? visual.transform.TransformPoint(new Vector3(visual.sprite.bounds.center.x, visual.sprite.bounds.max.y, 0))
            : transform.position + Vector3.up * 2.2f;

        public static WorldMerchantNpc Create(Transform parent, Vector2Int cell, Vector3 ground)
        {
            if (cachedFrames == null || cachedFrames.Length == 0)
            {
                cachedFrames = Resources.LoadAll<Sprite>(IdleResourcePath);
                Array.Sort(cachedFrames, (a, b) => string.CompareOrdinal(a.name, b.name));
            }
            if (cachedFrames.Length != 4)
            { Debug.LogError("[Merchant] 상인 대기 스프라이트 4프레임을 찾을 수 없습니다."); return null; }
            var root = new GameObject("CellMerchant"); root.transform.SetParent(parent, false);
            root.transform.position = ground;
            var merchant = root.AddComponent<WorldMerchantNpc>(); merchant.Cell = cell;
            var artwork = new GameObject("Visual"); artwork.transform.SetParent(root.transform, false);
            merchant.visual = artwork.AddComponent<SpriteRenderer>();
            merchant.visual.sprite = cachedFrames[0]; merchant.visual.sortingOrder = 260;
            artwork.transform.localScale = Vector3.one * (2.2f / cachedFrames[0].bounds.size.y);
            var billboard = artwork.AddComponent<Billboard>();
            billboard.ResetBaseLocalPosition(Vector3.down * .5f); // Cancel Billboard's default lift; sprite feet are the pivot.
            billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);
            return merchant;
        }

        public void SetVisible(bool visible) { if (visual != null) visual.enabled = visible; }

        private void OnEnable() => activeMerchants.Add(this);
        private void OnDisable() => activeMerchants.Remove(this);

        private void Update()
        {
            if (!IsVisible || Time.deltaTime <= 0 || cachedFrames == null) return;
            elapsed += Time.deltaTime;
            while (elapsed >= FrameDurations[frame])
            {
                elapsed -= FrameDurations[frame]; frame = (frame + 1) % cachedFrames.Length;
                visual.sprite = cachedFrames[frame];
            }
        }
    }
}
