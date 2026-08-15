using UnityEngine;
using ProceduralMap.Pooling;

namespace ProceduralMap
{
    /// <summary>용암 속에서 주기적으로 솟았다가 내려가는 시각 효과. 현재는 데미지를 주지 않는다.</summary>
    public sealed class LavaPopAnimator : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite smallSprite;
        [SerializeField] private Sprite largeSprite;
        [SerializeField, Min(0.05f)] private float riseDuration = 0.3f;
        [SerializeField, Min(0f)] private float topDuration = 0.2f;
        [SerializeField, Min(0.05f)] private float fallDuration = 0.3f;
        [SerializeField, Min(0f)] private float minWait = 1.2f;
        [SerializeField, Min(0f)] private float maxWait = 3f;
        [SerializeField, Min(0f)] private float jumpHeight = 0.8f;

        private Vector3 basePosition;
        private float timer;
        private float waitDuration;

        private void Awake()
        {
            if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            float motionDuration = riseDuration + topDuration + fallDuration;
            if (timer < waitDuration)
            {
                timer += Time.deltaTime;
                SetHeight(0f, smallSprite);
                return;
            }

            float motionTime = timer - waitDuration;
            float height01;
            if (motionTime < riseDuration) height01 = motionTime / riseDuration;
            else if (motionTime < riseDuration + topDuration) height01 = 1f;
            else height01 = 1f - (motionTime - riseDuration - topDuration) / fallDuration;
            height01 = Mathf.Clamp01(height01);
            SetHeight(Mathf.Sin(height01 * Mathf.PI * 0.5f), height01 > 0.5f ? largeSprite : smallSprite);

            timer += Time.deltaTime;
            if (motionTime >= motionDuration) ResetCycle();
        }

        private void SetHeight(float height01, Sprite sprite)
        {
            transform.position = basePosition + Vector3.up * (height01 * jumpHeight);
            if (spriteRenderer)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.enabled = height01 > 0.02f;
            }
        }

        private void ResetCycle()
        {
            timer = 0f;
            waitDuration = Random.Range(minWait, Mathf.Max(minWait, maxWait));
            SetHeight(0f, smallSprite);
        }

        public void OnTakenFromPool()
        {
            basePosition = transform.position;
            ResetCycle();
            // 청크가 열렸을 때 모두 숨어 있는 상태로 시작하지 않도록
            // 대기와 점프 구간 전체에서 서로 다른 시작 시점을 선택한다.
            float fullCycle = waitDuration + riseDuration + topDuration + fallDuration;
            timer = Random.Range(0f, fullCycle);
        }

        public void OnReturnedToPool()
        {
            transform.position = basePosition;
        }
    }
}
