using UnityEngine;
using UnityEngine.Events;

namespace Necrocis
{
    /// <summary>
    /// 재단 시스템 - 4개 부산물 바침 → 대뇌 맵 이동
    /// </summary>
    public class Altar : MonoBehaviour
    {
        [Header("재단 설정")]
#if UNITY_EDITOR
        [Tooltip("편집기에서 범위를 표시하는 선의 반경입니다. 실제 진입 감지는 Trigger Collider 크기를 사용합니다.")]
        [SerializeField] private float interactionRadius = 3f;
#endif
        [SerializeField] private bool isActivated = false;

        [Header("부산물 슬롯 (시각용)")]
        [SerializeField] private Transform[] relicSlots = new Transform[4];
        [SerializeField] private GameObject[] relicVisuals = new GameObject[4];

        [Header("시각 효과")]
        [SerializeField] private SpriteRenderer altarRenderer;
        [SerializeField] private Color inactiveColor = Color.gray;
        [SerializeField] private Color readyColor = Color.yellow;
        [SerializeField] private Color activatedColor = Color.magenta;
        [SerializeField] private ParticleSystem activationEffect;

        [Header("이벤트")]
        public UnityEvent OnAltarActivated;
        public UnityEvent OnFinalBossEnter;

        private System.IDisposable relicSubscription;
        public bool IsActivated => isActivated;

        private void Start()
        {
            if (altarRenderer == null)
                altarRenderer = GetComponent<SpriteRenderer>();
            UpdateRelicDisplay();
            UpdateVisuals();

            relicSubscription = WorldProgressBindings.Relics.SubscribeAllCollected(OnAllRelicsCollected);
        }

        private void OnDestroy()
        {
            relicSubscription?.Dispose();
            relicSubscription = null;
        }

        /// <summary>
        /// 모든 부산물 수집 시 호출
        /// </summary>
        private void OnAllRelicsCollected()
        {
            Debug.Log("[Altar] 모든 부산물 수집됨! 재단 활성화 준비");
            UpdateVisuals();
        }

        /// <summary>
        /// 재단 상호작용 시도
        /// </summary>
        public void TryInteract(GameObject player)
        {
            if (!WorldProgressBindings.Relics.IsAvailable) return;

            if (WorldProgressBindings.Relics.HasAll)
            {
                if (!isActivated)
                    ActivateAltar();
                EnterFinalBoss();
            }
            else
            {
                // 부산물 부족
                int collected = WorldProgressBindings.Relics.Count;
                Debug.Log($"[Altar] 부산물이 부족합니다. ({collected}/4)");
                ShowMissingRelics();
            }
        }

        /// <summary>
        /// 재단 활성화 (부산물 바침)
        /// </summary>
        private void ActivateAltar()
        {
            isActivated = true;

            Debug.Log("[Altar] 재단 활성화! 대뇌 맵 진입 가능");

            // 부산물 시각 효과
            ShowAllRelics();

            // 이펙트 재생
            if (activationEffect != null)
            {
                activationEffect.Play();
            }

            UpdateVisuals();
            OnAltarActivated?.Invoke();
        }

        /// <summary>
        /// 최종 보스(대뇌) 맵 진입
        /// </summary>
        private void EnterFinalBoss()
        {
            if (WorldProgressBindings.Travel.LoadFinalBoss())
            {
                AudioManager.Instance?.PlaySFX("PortalEnter");
                OnFinalBossEnter?.Invoke();
            }
        }

        /// <summary>
        /// 부족한 부산물 표시
        /// </summary>
        private void ShowMissingRelics()
        {
            if (!WorldProgressBindings.Relics.IsAvailable) return;

            string missing = "부족한 부산물: ";

            if (!WorldProgressBindings.Relics.HasRelic(BiomeType.Intestine))
                missing += "장, ";
            if (!WorldProgressBindings.Relics.HasRelic(BiomeType.Liver))
                missing += "간, ";
            if (!WorldProgressBindings.Relics.HasRelic(BiomeType.Stomach))
                missing += "위, ";
            if (!WorldProgressBindings.Relics.HasRelic(BiomeType.Lung))
                missing += "폐, ";

            Debug.Log($"[Altar] {missing.TrimEnd(',', ' ')}");
        }

        /// <summary>
        /// 모든 부산물 시각 표시
        /// </summary>
        private void ShowAllRelics()
        {
            for (int i = 0; i < relicVisuals.Length; i++)
            {
                if (relicVisuals[i] != null)
                {
                    relicVisuals[i].SetActive(true);
                }
            }
        }

        /// <summary>
        /// 시각 효과 업데이트
        /// </summary>
        private void UpdateVisuals()
        {
            if (altarRenderer == null) return;

            if (isActivated)
            {
                altarRenderer.color = activatedColor;
            }
            else if (WorldProgressBindings.Relics.IsAvailable && WorldProgressBindings.Relics.HasAll)
            {
                altarRenderer.color = readyColor;
            }
            else
            {
                altarRenderer.color = inactiveColor;
            }
        }

        /// <summary>
        /// 수집된 부산물 개별 표시
        /// </summary>
        public void UpdateRelicDisplay()
        {
            if (!WorldProgressBindings.Relics.IsAvailable) return;

            BiomeType[] biomes = { BiomeType.Intestine, BiomeType.Liver, BiomeType.Stomach, BiomeType.Lung };

            for (int i = 0; i < biomes.Length && i < relicVisuals.Length; i++)
            {
                if (relicVisuals[i] != null)
                {
                    relicVisuals[i].SetActive(WorldProgressBindings.Relics.HasRelic(biomes[i]));
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                TryInteract(other.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                TryInteract(other.gameObject);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 상호작용 범위
            Gizmos.color = isActivated ? Color.magenta : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);

            // 부산물 슬롯 위치
            Gizmos.color = Color.red;
            foreach (var slot in relicSlots)
            {
                if (slot != null)
                {
                    Gizmos.DrawWireCube(slot.position, Vector3.one * 0.5f);
                }
            }
        }
#endif
    }
}
