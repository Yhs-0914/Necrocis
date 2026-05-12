using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// 엘리트 몹 스포너.
    /// 1단계: 일반 적 각 5마리 처치 → 경고 자막 → 엘리트가 킬 기반으로 지속 소환
    /// 이후: 7마리마다 엘리트 1마리 + 미니 경고 "면역 반응 강화 중..."
    /// </summary>
    public class EliteSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float spawnDistanceMin = 8f;
        [SerializeField] private float spawnDistanceMax = 12f;
        [SerializeField] private int eliteSpawnEveryNKills = 7;

        private readonly List<EliteEntry> eliteConfigs = new List<EliteEntry>();
        private readonly Dictionary<string, int> killCounts = new Dictionary<string, int>();
        private readonly HashSet<string> unlockedElites = new HashSet<string>();
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

        // 지속 소환
        private bool elitePhaseActive;
        private int totalKillsSincePhase;
        private int lastEliteSpawnKills;
        private bool warningShown;

        // 자막 대기열
        private readonly Queue<System.Action> warningQueue = new Queue<System.Action>();

        // 폰트
        private Font pixelFont;

        // 엘리트별 해금 자막
        private readonly Dictionary<string, string> eliteUnlockMessages = new Dictionary<string, string>
        {
            { "Granuloma", "면역 경고: 대식세포 집결\n— 육아종 형성 개시!" },
            { "Antibody", "적응 면역 활성화\n— 항체 투입 승인!" }
        };

        private struct EliteEntry
        {
            public EnemySpawnRuleConfig config;
            public string triggerEnemyName;
            public int triggerKillCount;
        }

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

            eliteConfigs.Add(new EliteEntry
            {
                config = config,
                triggerEnemyName = config.killTriggerEnemyName,
                triggerKillCount = config.killTriggerCount > 0 ? config.killTriggerCount : 5
            });
        }

        public void ClearConfigs()
        {
            eliteConfigs.Clear();
            killCounts.Clear();
            unlockedElites.Clear();
            activeEliteConfigs.Clear();
            elitePhaseActive = false;
            totalKillsSincePhase = 0;
            lastEliteSpawnKills = 0;
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
            if (string.IsNullOrEmpty(enemyName)) return;

            if (!killCounts.ContainsKey(enemyName))
                killCounts[enemyName] = 0;

            killCounts[enemyName]++;

            // 엘리트 단계 활성화 후: N마리마다 엘리트 소환 + 미니 경고
            if (elitePhaseActive && activeEliteConfigs.Count > 0)
            {
                totalKillsSincePhase++;
                if (totalKillsSincePhase - lastEliteSpawnKills >= eliteSpawnEveryNKills)
                {
                    lastEliteSpawnKills = totalKillsSincePhase;
                    SpawnRandomElite();
                    ShowMiniWarning("면역 반응 강화 중...");
                }
            }

            // 해금 조건 확인
            foreach (EliteEntry entry in eliteConfigs)
            {
                if (unlockedElites.Contains(entry.config.name)) continue;
                if (entry.triggerEnemyName != enemyName) continue;
                if (killCounts[enemyName] < entry.triggerKillCount) continue;

                unlockedElites.Add(entry.config.name);
                activeEliteConfigs.Add(entry.config);

                // 엘리트별 고유 해금 메시지
                string message;
                if (!eliteUnlockMessages.TryGetValue(entry.config.name, out message))
                    message = $"새로운 위협 감지!\n{entry.config.name} 출현!";

                bool isFirst = !warningShown;
                warningShown = true;

                if (!showingWarning)
                {
                    ShowWarning(message, () =>
                    {
                        if (isFirst)
                        {
                            elitePhaseActive = true;
                            totalKillsSincePhase = 0;
                            lastEliteSpawnKills = 0;
                        }
                        SpawnRandomElite();
                    });
                }
                else
                {
                    string msg = message;
                    warningQueue.Enqueue(() =>
                    {
                        ShowWarning(msg, () =>
                        {
                            if (!elitePhaseActive)
                            {
                                elitePhaseActive = true;
                                totalKillsSincePhase = 0;
                                lastEliteSpawnKills = 0;
                            }
                            SpawnRandomElite();
                        });
                    });
                }
            }
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
        // 미니 경고 (7마리마다)
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
