using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class CombatHitFlash : MonoBehaviour
    {
        private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>(4);
        private readonly List<Color> originalColors = new List<Color>(4);
        private float restoreAt;
        private bool flashing;

        public void Flash(Color flashColor, float duration)
        {
            if (!flashing)
            {
                CacheOriginalColors();
            }

            restoreAt = Mathf.Max(restoreAt, Time.unscaledTime + Mathf.Max(0.02f, duration));
            flashing = true;
            enabled = true;

            for (int i = 0; i < renderers.Count; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Color original = i < originalColors.Count ? originalColors[i] : renderer.color;
                Color applied = Color.Lerp(original, flashColor, 0.82f);
                applied.a = original.a;
                renderer.color = applied;
            }
        }

        private void Update()
        {
            if (flashing && Time.unscaledTime >= restoreAt)
            {
                Restore();
            }
        }

        private void OnDisable()
        {
            Restore();
        }

        private void CacheOriginalColors()
        {
            renderers.Clear();
            GetComponentsInChildren(true, renderers);
            originalColors.Clear();
            for (int i = 0; i < renderers.Count; i++)
            {
                originalColors.Add(renderers[i] != null ? renderers[i].color : Color.white);
            }
        }

        private void Restore()
        {
            int count = Mathf.Min(renderers.Count, originalColors.Count);
            for (int i = 0; i < count; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = originalColors[i];
                }
            }

            flashing = false;
            restoreAt = 0f;
            enabled = false;
        }
    }
}
