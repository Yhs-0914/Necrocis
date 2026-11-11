using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>Stomach acid timing. Visuals and damage are connected separately.</summary>
    [DisallowMultipleComponent]
    public sealed class StomachAcidCycle : MonoBehaviour
    {
        public enum Phase { Charging, Warning, Rising, Flooded, Draining }

        [Header("Gauge")]
        [SerializeField, Min(1f)] private float maximumGauge = 100f;
        [SerializeField, Min(0f)] private float gaugePerSecond = 5f;

        [Header("Cycle durations (seconds)")]
        [SerializeField, Min(0.1f)] private float warningDuration = 5f;
        [SerializeField, Min(0.1f)] private float riseDuration = 3f;
        [SerializeField, Min(0.1f)] private float floodDuration = 10f;
        [SerializeField, Min(0.1f)] private float drainDuration = 3f;

        [Header("Events")]
        [SerializeField] private UnityEvent<float> onGaugeChanged = new UnityEvent<float>();
        [SerializeField] private UnityEvent<float> onAcidLevelChanged = new UnityEvent<float>();
        [SerializeField] private UnityEvent onWarningStarted = new UnityEvent();
        [SerializeField] private UnityEvent onFloodStarted = new UnityEvent();
        [SerializeField] private UnityEvent onCycleCompleted = new UnityEvent();

        [Header("Runtime (read during Play mode)")]
        [SerializeField] private Phase currentPhase;
        [SerializeField] private float currentGauge;
        [SerializeField] private float acidLevel;
        private float phaseElapsed;
        private StomachAcidView hud;
        private ProceduralMap.MapGenerator floodMap;
        [Header("Lava / Acid Damage")]
        [SerializeField, Min(0f)] private float damagePerTick = 1f;
        [SerializeField, Min(0.1f)] private float damageInterval = 1f;
        private float nextDamageTime;
        [Header("Enemy Flood Damage (fraction of max HP per flood)")]
        [SerializeField, Range(0f, 1f)] private float normalEnemyFloodDamage = 0.3f;
        [SerializeField, Range(0f, 1f)] private float eliteEnemyFloodDamage = 0.1f;
        private readonly System.Collections.Generic.List<EnemyController> floodTargets =
            new System.Collections.Generic.List<EnemyController>();

        private void ApplyEnemyFloodDamage(float exposureSeconds)
        {
            if (floodMap == null || !floodMap.IsReady || exposureSeconds <= 0f) return;
            floodTargets.Clear();
            floodTargets.AddRange(EnemyController.ActiveEnemyControllers);
            foreach (EnemyController enemy in floodTargets)
            {
                if (enemy == null || !enemy.isActiveAndEnabled || enemy.IsDead ||
                    enemy.IsBossEncounter || enemy.Stats == null ||
                    enemy.gameObject.scene != floodMap.gameObject.scene) continue;
                if (!floodMap.IsPlayerInAcid(enemy.transform.position, true)) continue;
                float fraction = enemy.IsElite ? eliteEnemyFloodDamage : normalEnemyFloodDamage;
                enemy.TakeFloodDamage(enemy.Stats.MaxHealth * fraction *
                    exposureSeconds / Mathf.Max(0.1f, floodDuration), HealthEffectSource.ForOwner(this, "enemy-flood"));
            }
        }

        public Phase CurrentPhase => currentPhase;
        public float NormalizedGauge => currentGauge / Mathf.Max(1f, maximumGauge);
        public float AcidLevel => acidLevel;

        public float PhaseSecondsRemaining => Mathf.Max(0f, currentPhase switch
        {
            Phase.Warning => warningDuration - phaseElapsed,
            Phase.Rising => riseDuration - phaseElapsed,
            Phase.Flooded => floodDuration - phaseElapsed,
            Phase.Draining => drainDuration - phaseElapsed,
            _ => gaugePerSecond > 0f ? (maximumGauge - currentGauge) / gaugePerSecond : 0f
        });

        private void Start()
        {
            hud = new StomachAcidView(this);
        }

        private void OnEnable() => ResetCycle();

        private void OnDisable()
        {
            ResetCycle();
            hud?.Hide();
        }

        private void LateUpdate()
        {
            hud?.Refresh();
            if (floodMap == null)
            {
                foreach (var map in FindObjectsByType<ProceduralMap.MapGenerator>(FindObjectsSortMode.None))
                {
                    if (map.gameObject.scene != gameObject.scene) continue;
                    floodMap = map;
                    break;
                }
            }
            if (floodMap != null) floodMap.SetAcidFloodLevel(acidLevel);
            ApplyAcidDamage();
        }

        private void ApplyAcidDamage()
        {
            if (Time.deltaTime <= 0f || floodMap == null || !floodMap.IsReady ||
                damagePerTick <= 0f || Time.time < nextDamageTime) return;

            PlayerController player = PlayerController.Instance;
            if (player == null || !player.isActiveAndEnabled || player.IsDead || player.IsDashInvincible) return;
            ProceduralTerrainMotor motor = player.GetComponent<ProceduralTerrainMotor>();
            if (motor != null && motor.IsTraversing) return;
            Health health = player.HealthComponent;
            if (health == null || health.IsDead || health.IsInvincible) return;
            if (!floodMap.IsPlayerInAcid(player.transform.position, acidLevel > 0f)) return;

            nextDamageTime = Time.time + Mathf.Max(0.1f, damageInterval);
            health.TakeDamage(new EnemyDamageRequest(Mathf.Max(0f, damagePerTick * DifficultyBalanceService.GetIncomingDamageMultiplier(null)),
                null, HealthEffectSource.ForOwner(this, "player-acid")));
        }

        private void OnDestroy() => hud?.Dispose();

        private void Update()
        {
            if (Time.deltaTime <= 0f) return;

            if (currentPhase == Phase.Charging)
            {
                AddGauge(gaugePerSecond * Time.deltaTime);
                return;
            }

            if (currentPhase == Phase.Flooded)
                ApplyEnemyFloodDamage(Mathf.Min(Time.deltaTime, Mathf.Max(0f, floodDuration - phaseElapsed)));
            phaseElapsed += Time.deltaTime;
            switch (currentPhase)
            {
                case Phase.Warning:
                    if (phaseElapsed >= warningDuration) EnterPhase(Phase.Rising);
                    break;
                case Phase.Rising:
                    SetAcidLevel(phaseElapsed / Mathf.Max(0.1f, riseDuration));
                    if (phaseElapsed >= riseDuration)
                    {
                        EnterPhase(Phase.Flooded);
                        onFloodStarted.Invoke();
                    }
                    break;
                case Phase.Flooded:
                    if (phaseElapsed >= floodDuration) EnterPhase(Phase.Draining);
                    break;
                case Phase.Draining:
                    SetAcidLevel(1f - phaseElapsed / Mathf.Max(0.1f, drainDuration));
                    if (phaseElapsed >= drainDuration)
                    {
                        ResetCycle();
                        onCycleCompleted.Invoke();
                    }
                    break;
            }
        }

        /// <summary>Shared entry point for future kills, hits and environmental puddles.</summary>
        public void AddGauge(float amount)
        {
            if (!isActiveAndEnabled || currentPhase != Phase.Charging ||
                amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount)) return;

            currentGauge = Mathf.Min(currentGauge + amount, Mathf.Max(1f, maximumGauge));
            bool full = NormalizedGauge >= 1f;
            if (full) EnterPhase(Phase.Warning);
            onGaugeChanged.Invoke(NormalizedGauge);
            if (full) onWarningStarted.Invoke();
        }

        [ContextMenu("Acid/Fill Gauge (Play Mode)")]
        private void FillGauge()
        {
            if (Application.isPlaying) AddGauge(maximumGauge);
        }

        [ContextMenu("Acid/Reset Cycle")]
        public void ResetCycle()
        {
            EnterPhase(Phase.Charging);
            currentGauge = 0f;
            acidLevel = 0f;
            if (floodMap != null) floodMap.SetAcidFloodLevel(0f);
            onGaugeChanged.Invoke(0f);
            onAcidLevelChanged.Invoke(0f);
        }

        private void EnterPhase(Phase phase)
        {
            currentPhase = phase;
            phaseElapsed = 0f;
        }

        private void SetAcidLevel(float value)
        {
            acidLevel = Mathf.Clamp01(value);
            onAcidLevelChanged.Invoke(acidLevel);
        }
    }
}

namespace Necrocis
{
    internal sealed class StomachAcidView
    {
        private StomachAcidCycle cycle;
        private GameObject canvasObject;
        private Image fill;
        private Text title;
        private Text status;
        private Text warning;

        public StomachAcidView(StomachAcidCycle owner)
        {
            cycle = owner;
            canvasObject = new GameObject("Stomach Acid HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(owner.transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image panel = RuntimeUiFactory.CreateImage("Acid Panel", canvasObject.transform, new Color(0.08f, 0.025f, 0.045f, 0.92f));
            panel.raycastTarget = false;
            RuntimeUiFactory.SetRect(panel.rectTransform, new Vector2(1f, 1f), new Vector2(380f, 112f), new Vector2(-214f, -80f));
            Font font = GameUiTheme.LoadFont();
            Image accent = RuntimeUiFactory.CreateImage("Lava Accent", panel.transform, new Color(1f, 0.48f, 0.08f));
            accent.raycastTarget = false;
            RuntimeUiFactory.SetRect(accent.rectTransform, new Vector2(0f, 0.5f), new Vector2(4f, 112f), new Vector2(2f, 0f));
            title = RuntimeUiFactory.CreateText("Gauge Label", panel.transform, "", font, 22, Color.white);
            RuntimeUiFactory.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(348f, 30f), new Vector2(0f, -22f));

            Image track = RuntimeUiFactory.CreateImage("Gauge Track", panel.transform, new Color(0.22f, 0.12f, 0.13f));
            track.raycastTarget = false;
            RuntimeUiFactory.SetRect(track.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(348f, 18f), Vector2.zero);
            fill = RuntimeUiFactory.CreateImage("Gauge Fill", track.transform, new Color(0.75f, 0.9f, 0.18f));
            fill.raycastTarget = false;
            RuntimeUiFactory.Stretch(fill.rectTransform);
            status = RuntimeUiFactory.CreateText("Cycle Status", panel.transform, "", font, 18, Color.white);
            RuntimeUiFactory.SetRect(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(348f, 30f), new Vector2(0f, 22f));
            warning = RuntimeUiFactory.CreateText("Evacuation Warning", canvasObject.transform, "", font, 30, new Color(1f, 0.8f, 0.3f));
            warning.alignment = TextAnchor.MiddleCenter;
            RuntimeUiFactory.SetRect(warning.rectTransform, new Vector2(0.5f, 1f), new Vector2(700f, 50f), new Vector2(0f, -170f));
            Refresh();
        }

        public void Refresh()
        {
            bool visible = cycle != null && cycle.isActiveAndEnabled;
            canvasObject.SetActive(visible);
            if (!visible) return;

            float gauge = Mathf.Clamp01(cycle.NormalizedGauge);
            fill.rectTransform.anchorMax = new Vector2(gauge, 1f);
            title.text = $"위액 게이지  {Mathf.FloorToInt(gauge * 100f)}%";
            int seconds = Mathf.CeilToInt(cycle.PhaseSecondsRemaining);
            status.text = cycle.CurrentPhase switch
            {
                StomachAcidCycle.Phase.Warning => $"범람 임박! 높은 층으로 이동 · {seconds}초",
                StomachAcidCycle.Phase.Rising => $"위액 차오르는 중 · {seconds}초",
                StomachAcidCycle.Phase.Flooded => $"범람 유지 · {seconds}초",
                StomachAcidCycle.Phase.Draining => $"위액 빠지는 중 · {seconds}초",
                _ => "위액이 모이고 있습니다"
            };
            fill.color = cycle.CurrentPhase == StomachAcidCycle.Phase.Charging
                ? new Color(1f, 0.55f, 0.08f)
                : new Color(1f, 0.38f, 0.15f);
            bool showWarning = cycle.CurrentPhase == StomachAcidCycle.Phase.Warning;
            warning.gameObject.SetActive(showWarning);
            if (showWarning)
            {
                warning.text = $"위액 범람까지 {seconds}초 · 2층 이상으로 대피하세요!";
                warning.color = new Color(1f, 0.8f, 0.3f, 0.75f + 0.25f * Mathf.Sin(Time.time * 6f));
            }
        }

        public void Hide()
        {
            if (canvasObject != null) canvasObject.SetActive(false);
        }

        public void Dispose()
        {
            if (canvasObject != null) Object.Destroy(canvasObject);
        }
    }
}
