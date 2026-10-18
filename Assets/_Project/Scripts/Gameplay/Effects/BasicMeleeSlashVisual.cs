using UnityEngine;
using UnityEngine.SceneManagement;

namespace Necrocis
{
    // One Q visual lifetime. The pool activates a node before Show, so OnEnable must not
    // start fading or expose the previous shot's sprite/alpha.
    public sealed class BasicMeleeSlashVisual : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Material defaultMaterial;
        private Behaviour owner;
        private Scene birthScene;
        private float began, duration;
        private bool showing;

        public static GameObject CreateObject()
        {
            var node = new GameObject("BasicMeleeSlashVisual");
            node.SetActive(false);
            var effect = node.AddComponent<BasicMeleeSlashVisual>();
            effect.spriteRenderer = node.AddComponent<SpriteRenderer>();
            effect.defaultMaterial = effect.spriteRenderer.sharedMaterial;
            effect.spriteRenderer.enabled = false;
            return node;
        }

        public void Show(Behaviour source, Sprite sprite, Vector3 position, Quaternion rotation,
            float scale, int sortingOrder, float lifetime)
        {
            owner = source;
            birthScene = SceneManager.GetActiveScene();
            began = Time.time;
            duration = Mathf.Max(.02f, lifetime);
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = Vector3.one * scale;
            spriteRenderer.sprite = sprite;
            spriteRenderer.sharedMaterial = defaultMaterial;
            spriteRenderer.color = Color.white;
            spriteRenderer.flipX = spriteRenderer.flipY = false;
            spriteRenderer.sortingLayerID = 0;
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.enabled = true;
            showing = true;
        }

        private void OnEnable()
        {
            showing = false;
            if (spriteRenderer != null) spriteRenderer.enabled = false;
        }

        private void Update()
        {
            if (!showing) return;
            // A disabled attack component may still finish its visual, like the old coroutine.
            // Inactive/destroyed owners and an unloaded origin scene cannot leave orphan effects.
            float elapsed = Time.time - began;
            if (owner == null || !owner.gameObject.activeInHierarchy || !birthScene.IsValid()
                || !birthScene.isLoaded || elapsed >= duration)
            {
                RuntimePool.Release(gameObject);
                return;
            }
            spriteRenderer.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, elapsed / duration));
        }

        private void OnDisable()
        {
            showing = false;
            owner = null;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
                spriteRenderer.sprite = null;
                spriteRenderer.color = Color.white;
            }
        }
    }
}
