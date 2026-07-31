using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// O키로 열고 닫는 스탯 확인 UI.
    /// 현재 스탯과 기본값 대비 증감을 표시.
    /// </summary>
    public class StatUI : MonoBehaviour
    {
        [Header("UI 설정")]
        [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.85f);  // 패널 배경색
        [SerializeField] private Color positiveColor = new Color(0.4f, 1f, 0.4f);  // 증가 표시 색상 (초록)
        [SerializeField] private Color negativeColor = new Color(1f, 0.4f, 0.4f);  // 감소 표시 색상 (빨강)

        private GameObject uiRoot;     // UI Canvas 루트
        private Text contentText;      // 모든 스탯 정보를 표시하는 단일 Text
        private bool isShowing;        // 현재 표시 중 여부
        private CharacterStats subscribedStats;
        private readonly System.Text.StringBuilder statTextBuilder = new System.Text.StringBuilder(384);
        private string positiveColorHex;
        private string negativeColorHex;

        // 표시할 스탯 순서 정의
        private static readonly CharacterStatType[] displayStats = new CharacterStatType[]
        {
            CharacterStatType.MaxHealth,
            CharacterStatType.AttackPower,
            CharacterStatType.MoveSpeed,
            CharacterStatType.AttackSpeed,
            CharacterStatType.AttackRange,
            CharacterStatType.Magic,
            CharacterStatType.SkillCooldownReduction
        };

        private void Start()
        {
            BuildUI();
            uiRoot.SetActive(false);
            TryBindStats();
        }

        private void OnEnable()
        {
            LevelUpManager.OnLevelUp += HandleProgressionChanged;
            LevelUpManager.OnJobSelect += HandleProgressionChanged;
            LevelUpManager.OnJobChanged += HandleJobChanged;
            TryBindStats();
        }

        private void OnDisable()
        {
            LevelUpManager.OnLevelUp -= HandleProgressionChanged;
            LevelUpManager.OnJobSelect -= HandleProgressionChanged;
            LevelUpManager.OnJobChanged -= HandleJobChanged;
            UnbindStats();
        }

        // O키로 창을 토글한다. 수치는 구독한 스탯/진행 이벤트에서 갱신한다.
        private void Update()
        {
            if (subscribedStats == null)
            {
                TryBindStats();
            }

            var input = InputManager.Instance;
            if (input == null) return;

            if (input.StatWindowAction.WasPressedThisFrame())
            {
                AudioManager.Instance?.PlaySFX("StatWindow");
                if (isShowing)
                    Hide();
                else
                    Show();
            }
        }

        private void Show()
        {
            TryBindStats();
            RefreshStats();
            uiRoot.SetActive(true);
            isShowing = true;
        }

        private void Hide()
        {
            uiRoot.SetActive(false);
            isShowing = false;
        }

        // 모든 스탯 정보를 StringBuilder로 조합하여 Text에 표시
        // 기본값 대비 증감분을 색상으로 표시 (초록: 증가, 빨강: 감소)
        private void RefreshStats()
        {
            CharacterStats stats = subscribedStats != null
                ? subscribedStats
                : PlayerStats.Instance?.RuntimeStats;
            if (stats == null) return;

            positiveColorHex ??= ColorUtility.ToHtmlStringRGB(positiveColor);
            negativeColorHex ??= ColorUtility.ToHtmlStringRGB(negativeColor);
            System.Text.StringBuilder sb = statTextBuilder;
            sb.Clear();

            // 레벨
            sb.AppendLine($"<color=#FFD933><b>스탯 정보</b></color>");
            sb.AppendLine($"Lv.{LevelUpManager.GetCurrentLevel()}");

            // 직업
            JobType job = LevelUpManager.GetCurrentJob();
            string jobName = job == JobType.None ? "없음" : GetJobName(job);
            sb.AppendLine($"<color=#99CCFF>직업: {jobName}</color>");
            sb.AppendLine();

            // HP (현재/최대 형태)
            float hp = stats.CurrentHealth;
            float maxHp = stats.MaxHealth;
            float hpBase = stats.GetBaseValue(CharacterStatType.MaxHealth);
            float hpDiff = maxHp - hpBase;
            sb.Append($"체력: {hp:F0}/{maxHp:F0}");
            if (Mathf.Abs(hpDiff) > 0.01f)
                sb.Append(hpDiff > 0 ? $" <color=#{positiveColorHex}>(+{hpDiff:F1})</color>" : $" <color=#{negativeColorHex}>({hpDiff:F1})</color>");
            sb.AppendLine();

            // 나머지 스탯
            for (int i = 1; i < displayStats.Length; i++)
            {
                CharacterStatType type = displayStats[i];
                float current = stats.GetValue(type);
                float baseVal = stats.GetBaseValue(type);
                float diff = current - baseVal;

                sb.Append($"{GetStatName(type)}: {current:F1}");
                if (Mathf.Abs(diff) > 0.01f)
                {
                    if (diff > 0)
                        sb.Append($" <color=#{positiveColorHex}>(+{diff:F1})</color>");
                    else
                        sb.Append($" <color=#{negativeColorHex}>({diff:F1})</color>");
                }
                sb.AppendLine();
            }

            contentText.text = sb.ToString();
        }

        private void TryBindStats()
        {
            CharacterStats currentStats = PlayerStats.Instance?.RuntimeStats;
            if (currentStats == subscribedStats)
            {
                return;
            }

            UnbindStats();
            subscribedStats = currentStats;
            if (subscribedStats == null)
            {
                return;
            }

            subscribedStats.StatChanged += HandleStatChanged;
            subscribedStats.HealthChanged += HandleHealthChanged;
        }

        private void UnbindStats()
        {
            if (subscribedStats == null)
            {
                return;
            }

            subscribedStats.StatChanged -= HandleStatChanged;
            subscribedStats.HealthChanged -= HandleHealthChanged;
            subscribedStats = null;
        }

        private void HandleStatChanged(CharacterStats _, CharacterStatChangedEventArgs __)
        {
            RefreshIfVisible();
        }

        private void HandleHealthChanged(CharacterStats _, CharacterHealthChangedEventArgs __)
        {
            RefreshIfVisible();
        }

        private void HandleProgressionChanged()
        {
            RefreshIfVisible();
        }

        private void HandleJobChanged(JobType _)
        {
            RefreshIfVisible();
        }

        private void RefreshIfVisible()
        {
            if (isShowing)
            {
                RefreshStats();
            }
        }

        // 스탯 타입 → 한글 이름 변환
        private static string GetStatName(CharacterStatType type)
        {
            return type switch
            {
                CharacterStatType.MaxHealth => "체력",
                CharacterStatType.AttackPower => "공격력",
                CharacterStatType.MoveSpeed => "이동속도",
                CharacterStatType.AttackSpeed => "공격속도",
                CharacterStatType.AttackRange => "공격 사거리",
                CharacterStatType.Magic => "마력",
                CharacterStatType.SkillCooldownReduction => "스킬 쿨타임 감소",
                _ => type.ToString()
            };
        }

        // 직업 타입 → 한글 이름 변환
        private static string GetJobName(JobType job)
        {
            return job switch
            {
                JobType.Warrior => "전사",
                JobType.Mage => "마법사",
                JobType.Archer => "궁수",
                _ => job.ToString()
            };
        }

        // ─────────────────────────────────
        // UI 자동 생성
        // ─────────────────────────────────

        private void BuildUI()
        {
            // Canvas
            GameObject canvasObj = new GameObject("StatUICanvas");
            canvasObj.transform.SetParent(transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            uiRoot = canvasObj;

            // 패널 (왼쪽 상단 고정, 내용에 맞게 크기 조절)
            GameObject panel = new GameObject("StatPanel", typeof(RectTransform));
            panel.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(10, -10);

            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = panelColor;

            ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // 텍스트 1개로 모든 정보 표시
            GameObject textObj = new GameObject("Content", typeof(RectTransform));
            textObj.transform.SetParent(panel.transform, false);

            contentText = textObj.AddComponent<Text>();
            contentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            contentText.fontSize = 22;
            contentText.color = Color.white;
            contentText.alignment = TextAnchor.UpperLeft;
            contentText.supportRichText = true;
            contentText.horizontalOverflow = HorizontalWrapMode.Overflow;
            contentText.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }
}
