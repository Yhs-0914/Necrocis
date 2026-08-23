using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// 엘리트 몹 스포너.
    /// 등록된 일반 적을 합산해 10마리 처치할 때마다 엘리트 1마리를 소환한다.
    /// 첫 소환은 전체 경고, 이후 소환은 미니 경고를 표시한다.
    /// </summary>
    public class EliteSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float spawnDistanceMin = 8f;
        [SerializeField] private float spawnDistanceMax = 12f;
        [SerializeField, Min(1)] private int eliteSpawnEveryNKills = 10;

        private readonly HashSet<string> normalEnemyNames = new HashSet<string>();
        private readonly List<EnemySpawnRuleConfig> activeEliteConfigs = new List<EnemySpawnRuleConfig>();

        // 경고 UI (풀 자막)
        private Canvas warningCanvas;
        private Text warningText;
        private CanvasGroup canvasGroup;
        private float warningTimer;
        private float warningDuration = 3f;
        private bool showingWarning;
        private System.Action onWarningComplete;

        // 미니 경고 UI (상단 작은 텍스트)
        private Canvas miniCanvas;
        private Text miniText;
        private CanvasGroup miniCanvasGroup;
        private float miniTimer;
        private float miniDuration = 1.5f;
        private bool showingMini;

        // 일반몹 합산 처치 기반 지속 소환
        private int totalNormalKills;
        private bool warningShown;

        // 자막 대기열
        private readonly Queue<System.Action> warningQueue = new Queue<System.Action>();

        // 폰트
        private Font pixelFont;

        public static EliteSpawner Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void RegisterEliteConfig(EnemySpawnRuleConfig config)
        {
            if (config == null || !config.isElite) return;

            if (!activeEliteConfigs.Contains(config))
                activeEliteConfigs.Add(config);
        }

        public void ConfigureKillInterval(int normalKillsPerElite)
        {
            eliteSpawnEveryNKills = Mathf.Max(1, normalKillsPerElite);
        }

        public void RegisterNormalEnemyConfig(EnemySpawnRuleConfig config)
        {
            if (config == null || config.isElite || string.IsNullOrEmpty(config.name)) return;

            normalEnemyNames.Add(config.name);
        }

        public void ClearConfigs()
        {
            normalEnemyNames.Clear();
            activeEliteConfigs.Clear();
            totalNormalKills = 0;
            warningShown = false;
            showingWarning = false;
            showingMini = false;
            warningQueue.Clear();
            if (warningCanvas != null)
                warningCanvas.gameObject.SetActive(false);
            if (miniCanvas != null)
                miniCanvas.gameObject.SetActive(false);
        }

        public void NotifyEnemyKilled(string enemyName)
        {
            if (string.IsNullOrEmpty(enemyName)
                || !normalEnemyNames.Contains(enemyName)
                || activeEliteConfigs.Count == 0)
            {
                return;
            }

            totalNormalKills++;
            int killsPerElite = Mathf.Max(1, eliteSpawnEveryNKills);
            if (totalNormalKills % killsPerElite != 0) return;

            QueueEliteSpawn();
        }

        private void QueueEliteSpawn()
        {
            if (!warningShown)
            {
                warningShown = true;
                ShowWarning("면역 경고\n엘리트 적 출현!", SpawnRandomElite);
                return;
            }

            System.Action spawnAction = () =>
            {
                SpawnRandomElite();
                ShowMiniWarning("면역 반응 강화 중...");
            };

            if (showingWarning)
                warningQueue.Enqueue(spawnAction);
            else
                spawnAction();
        }

        private void Update()
        {
            // 풀 자막 애니메이션
            if (showingWarning)
            {
                UpdateWarningAnimation();
            }

            // 미니 경고 애니메이션
            if (showingMini)
            {
                UpdateMiniWarning();
            }

            // 대기열 처리
            if (!showingWarning && warningQueue.Count > 0)
            {
                warningQueue.Dequeue().Invoke();
            }
        }

        // ─────────────────────────────────
        // 풀 자막 (해금 시)
        // ─────────────────────────────────

        private void UpdateWarningAnimation()
        {
            warningTimer += Time.deltaTime;

            if (warningTimer < 0.5f)
            {
                float t = warningTimer / 0.5f;
                canvasGroup.alpha = t;
                float scale = Mathf.Lerp(2f, 1f, t);
                warningText.transform.localScale = new Vector3(scale, scale, 1f);
            }
            else if (warningTimer < 2f)
            {
                canvasGroup.alpha = 1f;
                float pulse = 1f + Mathf.Sin(warningTimer * 6f) * 0.05f;
                warningText.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else if (warningTimer < warningDuration)
            {
                float t = (warningTimer - 2f) / 1f;
                canvasGroup.alpha = 1f - t;
            }
            else
            {
                showingWarning = false;
                warningCanvas.gameObject.SetActive(false);
                onWarningComplete?.Invoke();
                onWarningComplete = null;
            }
        }

        private void ShowWarning(string message, System.Action onComplete)
        {
            EnsureWarningUI();

            warningText.text = message;
            warningCanvas.gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            warningTimer = 0f;
            showingWarning = true;
            onWarningComplete = onComplete;
        }

        // ─────────────────────────────────
        // 미니 경고 (첫 소환 이후)
        // ─────────────────────────────────

        private void ShowMiniWarning(string message)
        {
            EnsureMiniWarningUI();

            miniText.text = message;
            miniCanvas.gameObject.SetActive(true);
            miniCanvasGroup.alpha = 0f;
            miniTimer = 0f;
            showingMini = true;
        }

        private void UpdateMiniWarning()
        {
            miniTimer += Time.deltaTime;

            // 페이드 인 (0~0.2초)
            if (miniTimer < 0.2f)
            {
                miniCanvasGroup.alpha = miniTimer / 0.2f;
            }
            // 유지 (0.2~1초)
            else if (miniTimer < 1f)
            {
                miniCanvasGroup.alpha = 1f;
            }
            // 페이드 아웃 (1~1.5초)
            else if (miniTimer < miniDuration)
            {
                float t = (miniTimer - 1f) / 0.5f;
                miniCanvasGroup.alpha = 1f - t;
            }
            else
            {
                showingMini = false;
                miniCanvas.gameObject.SetActive(false);
            }
        }

        // ─────────────────────────────────
        // 소환
        // ─────────────────────────────────

        private void SpawnRandomElite()
        {
            if (activeEliteConfigs.Count == 0) return;
            if (PlayerController.Instance == null) return;

            EnemySpawnRuleConfig config = activeEliteConfigs[Random.Range(0, activeEliteConfigs.Count)];
            Transform player = PlayerController.Instance.transform;
            BiomeManager biome = BiomeManager.Active;
            Vector3 spawnPos = FindSpawnPosition(player.position, biome, config);

            Transform spawnParent = transform;
            int poolId = EnemyController.GetPoolArchetypeId(config);
            EnemyController elite = EnemyController.Acquire(spawnParent, $"{config.name}_Elite", poolId);
            elite.Configure(null, config, spawnPos, spawnPos);
        }

        // ─────────────────────────────────
        // UI 생성
        // ─────────────────────────────────

        private void LoadFont()
        {
            if (pixelFont != null) return;

            pixelFont = Resources.Load<Font>("PFStardust");
            if (pixelFont == null)
            {
                Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();
                foreach (Font f in allFonts)
                {
                    if (f.name.Contains("PFStardust") || f.name.Contains("스타더스트"))
                    {
                        pixelFont = f;
                        break;
                    }
                }
            }
        }

        private void EnsureWarningUI()
        {
            if (warningCanvas != null) return;
            LoadFont();

            GameObject canvasObj = new GameObject("EliteWarningCanvas");
            canvasObj.transform.SetParent(transform, false);

            warningCanvas = canvasObj.AddComponent<Canvas>();
            warningCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            warningCanvas.sortingOrder = 9999;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGroup = canvasObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // 배경
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.6f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // 텍스트
            GameObject textObj = new GameObject("WarningText");
            textObj.transform.SetParent(canvasObj.transform, false);

            warningText = textObj.AddComponent<Text>();
            warningText.text = "";
            warningText.fontSize = 64;
            warningText.alignment = TextAnchor.MiddleCenter;
            warningText.color = new Color(1f, 0.3f, 0.3f, 1f);
            warningText.horizontalOverflow = HorizontalWrapMode.Overflow;
            warningText.verticalOverflow = VerticalWrapMode.Overflow;

            if (pixelFont != null)
                warningText.font = pixelFont;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 1f);
            outline.effectDistance = new Vector2(3f, -3f);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(1200f, 300f);
            textRect.anchoredPosition = Vector2.zero;

            canvasObj.SetActive(false);
        }

        private void EnsureMiniWarningUI()
        {
            if (miniCanvas != null) return;
            LoadFont();

            GameObject canvasObj = new GameObject("EliteMiniWarningCanvas");
            canvasObj.transform.SetParent(transform, false);

            miniCanvas = canvasObj.AddComponent<Canvas>();
            miniCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            miniCanvas.sortingOrder = 9998;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            miniCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
            miniCanvasGroup.alpha = 0f;
            miniCanvasGroup.blocksRaycasts = false;
            miniCanvasGroup.interactable = false;

            // 텍스트 (상단)
            GameObject textObj = new GameObject("MiniText");
            textObj.transform.SetParent(canvasObj.transform, false);

            miniText = textObj.AddComponent<Text>();
            miniText.text = "";
            miniText.fontSize = 32;
            miniText.alignment = TextAnchor.MiddleCenter;
            miniText.color = new Color(1f, 0.6f, 0.6f, 0.9f); // 연한 빨간
            miniText.horizontalOverflow = HorizontalWrapMode.Overflow;
            miniText.verticalOverflow = VerticalWrapMode.Overflow;

            if (pixelFont != null)
                miniText.font = pixelFont;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 1f);
            textRect.anchorMax = new Vector2(0.5f, 1f);
            textRect.sizeDelta = new Vector2(800f, 60f);
            textRect.anchoredPosition = new Vector2(0f, -80f); // 상단에서 80px 아래

            canvasObj.SetActive(false);
        }

        private Vector3 FindSpawnPosition(Vector3 playerPos, BiomeManager biome, EnemySpawnRuleConfig config)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(spawnDistanceMin, spawnDistanceMax);
                Vector3 candidate = playerPos + new Vector3(offset.x, 0f, offset.y);

                if (biome != null)
                {
                    Vector2Int grid = biome.WorldToGrid(candidate);
                    if (!biome.IsValidPosition(grid.x, grid.y) || !biome.IsWalkable(grid.x, grid.y))
                        continue;

                    candidate.y = biome.GetGroundHeight(candidate) + config.heightOffset;
                }

                return candidate;
            }

            Vector3 fallback = playerPos + new Vector3(spawnDistanceMin, 0f, 0f);
            if (biome != null)
                fallback.y = biome.GetGroundHeight(fallback) + config.heightOffset;
            return fallback;
        }
    }
}
