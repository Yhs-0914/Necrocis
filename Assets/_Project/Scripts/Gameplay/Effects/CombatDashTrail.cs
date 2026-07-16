using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class CombatDashTrail : MonoBehaviour
    {
        private const string GhostPoolName = "CombatVfx.DashGhost";
        private const float SpawnInterval = 0.035f;

        private SpriteRenderer sourceRenderer;
        private Vector3 dashDirection;
        private float emitUntil;
        private float nextEmitAt;

        public void Begin(SpriteRenderer source, Vector3 direction, float duration)
        {
            if (source == null || source.sprite == null)
            {
                return;
            }

            sourceRenderer = source;
            dashDirection = direction;
            dashDirection.y = 0f;
            if (dashDirection.sqrMagnitude > 0.0001f)
            {
                dashDirection.Normalize();
            }

            emitUntil = Time.unscaledTime + Mathf.Max(0.05f, duration);
            nextEmitAt = Time.unscaledTime;
            enabled = true;
            SpawnGhost();
        }

        private void Update()
        {
            if (sourceRenderer == null || Time.unscaledTime >= emitUntil)
            {
                enabled = false;
                return;
            }

            if (Time.unscaledTime >= nextEmitAt)
            {
                SpawnGhost();
            }
        }

        private void SpawnGhost()
        {
            nextEmitAt = Time.unscaledTime + SpawnInterval;
            if (sourceRenderer == null || !sourceRenderer.enabled || sourceRenderer.sprite == null)
            {
                return;
            }

            GameObject ghostObject = RuntimePool.Acquire(GhostPoolName, CombatDashGhost.CreateObject);
            if (ghostObject == null || !ghostObject.TryGetComponent(out CombatDashGhost ghost))
            {
                RuntimePool.Release(ghostObject);
                return;
            }

            ghost.Show(sourceRenderer, -dashDirection * 0.45f + Vector3.up * 0.12f, 0.19f);
        }
    }

    [DisallowMultipleComponent]
    internal sealed class CombatDashGhost : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Vector3 drift;
        private Color startColor;
        private float duration;
        private float elapsed;

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("DashAfterImageFx");
            root.SetActive(false);
            root.AddComponent<SpriteRenderer>();
            root.AddComponent<CombatDashGhost>();
            return root;
        }

        public void Show(SpriteRenderer source, Vector3 worldDrift, float lifeTime)
        {
            if (source == null)
            {
                RuntimePool.Release(gameObject);
                return;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            transform.SetParent(null, false);
            transform.position = source.transform.position;
            transform.rotation = source.transform.rotation;
            transform.localScale = source.transform.lossyScale;
            gameObject.layer = source.gameObject.layer;

            spriteRenderer.sprite = source.sprite;
            spriteRenderer.flipX = source.flipX;
            spriteRenderer.flipY = source.flipY;
            spriteRenderer.sortingLayerID = source.sortingLayerID;
            spriteRenderer.sortingOrder = source.sortingOrder - 1;
            spriteRenderer.sharedMaterial = source.sharedMaterial;

            startColor = Color.Lerp(source.color, new Color(0.85f, 0.12f, 0.32f, 1f), 0.58f);
            startColor.a = 0.38f;
            spriteRenderer.color = startColor;
            spriteRenderer.enabled = true;

            drift = worldDrift;
            duration = Mathf.Max(0.05f, lifeTime);
            elapsed = 0f;
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position += drift * Time.unscaledDeltaTime;
            transform.localScale *= 1f + Time.unscaledDeltaTime * 0.5f;

            Color color = startColor;
            color.a *= Mathf.Pow(1f - t, 1.4f);
            spriteRenderer.color = color;
            if (t >= 1f)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void OnDisable()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
            elapsed = 0f;
        }
    }
}
