using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 분열 VFX 페이드아웃 후 자동 제거
    /// </summary>
    public class SplitVfxFadeOut : MonoBehaviour
    {
        private SpriteRenderer target;
        private float fadeDuration;
        private float elapsed;

        public void Init(SpriteRenderer renderer, float duration)
        {
            target = renderer;
            fadeDuration = duration;
            elapsed = 0f;
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            Color c = target.color;
            c.a = Mathf.Lerp(0.9f, 0f, t);
            target.color = c;

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
