using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class SkillCooldownUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private Button clickButton;
    [SerializeField] private CanvasGroup interactionBlocker;
    [SerializeField] private Text keyText;
    [SerializeField] private Text cooldownText;
    [SerializeField] private TMP_Text keyTMPText;
    [SerializeField] private TMP_Text cooldownTMPText;

    [Header("Visual Settings")]
    [SerializeField] private Color readyIconColor = Color.white;
    [SerializeField] private Color coolingIconColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color lockedIconColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color cooldownOverlayColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private bool useUnscaledTime;

    [Header("Options")]
    [SerializeField] private string keyLabel = "E";
    [SerializeField] private bool showRemainingSeconds = true;
    [SerializeField] private bool blockClickWhileCooling = true;
    [SerializeField] [Min(0)] private int remainingTimeDecimalPlaces = 1;

    private Coroutine cooldownCoroutine;
    private float cooldownDuration;
    private float remainingCooldown;
    private float normalizedProgress;
    private bool cachedButtonInteractable = true;
    private bool hasCachedButtonInteractable;
    private bool cachedCanvasGroupBlocksRaycasts = true;
    private bool hasCachedCanvasGroupBlocksRaycasts;

    public bool IsCoolingDown { get; private set; }
    public bool IsUnlocked { get; private set; } = true;
    public float RemainingCooldown => remainingCooldown;
    public float CooldownDuration => cooldownDuration;
    public float NormalizedProgress => normalizedProgress;
    public event Action<float> CooldownProgressChanged;
    public event Action<bool> CooldownStateChanged;

    private void Awake()
    {
        ValidateReferences();
        ApplyFillDefaults();
        ApplyIdleVisual();
        ApplyKeyLabel();
    }

    private void OnValidate()
    {
        ApplyFillDefaults();
        ApplyKeyLabel();
    }

    private void OnDisable()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        SetButtonInteractableDuringCooldown(isCoolingDown: false);
    }

    public void SetKeyLabel(string label)
    {
        keyLabel = label;
        ApplyKeyLabel();
    }

    public void StartCooldown(float seconds)
    {
        StartCooldown(seconds, seconds);
    }

    public void StartCooldown(float remainingSeconds, float totalDurationSeconds)
    {
        if (!IsUnlocked)
        {
            ForceReady();
            return;
        }

        float safeRemain = Mathf.Max(0f, remainingSeconds);
        float safeTotal = Mathf.Max(safeRemain, totalDurationSeconds);

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        if (safeRemain <= 0f)
        {
            ForceReady();
            return;
        }

        cooldownCoroutine = StartCoroutine(CooldownRoutine(safeRemain, safeTotal));
    }

    public void SetIconSprite(Sprite sprite, bool syncOverlaySprite = true)
    {
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
        }

        if (syncOverlaySprite && cooldownFillImage != null)
        {
            cooldownFillImage.sprite = sprite;
        }
    }

    public void ForceReady()
    {
        IsCoolingDown = false;
        cooldownDuration = 0f;
        remainingCooldown = 0f;
        normalizedProgress = 0f;
        SetFillAmount(0f);
        SetCooldownText(string.Empty);
        SetButtonInteractableDuringCooldown(isCoolingDown: !IsUnlocked);
        ApplyIdleVisual();
        CooldownStateChanged?.Invoke(false);
        CooldownProgressChanged?.Invoke(0f);
    }

    public void SetUnlocked(bool unlocked)
    {
        if (IsUnlocked == unlocked)
        {
            return;
        }

        IsUnlocked = unlocked;
        ForceReady();
    }

    private IEnumerator CooldownRoutine(float remainingDuration, float totalDuration)
    {
        IsCoolingDown = true;
        cooldownDuration = Mathf.Max(0.0001f, totalDuration);
        remainingCooldown = Mathf.Max(0f, remainingDuration);
        ApplyCoolingVisual();
        CooldownStateChanged?.Invoke(true);
        SetButtonInteractableDuringCooldown(isCoolingDown: true);
        UpdateVisualByRemainingTime(remainingCooldown);

        while (remainingCooldown > 0f)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            remainingCooldown = Mathf.Max(0f, remainingCooldown - deltaTime);
            UpdateVisualByRemainingTime(remainingCooldown);
            yield return null;
        }

        if (remainingCooldown <= 0f)
        {
            cooldownCoroutine = null;
            ForceReady();
        }
    }

    private void ValidateReferences()
    {
        if (iconImage == null)
        {
            Debug.LogWarning($"[{nameof(SkillCooldownUI)}] Icon Image is not assigned on {name}.", this);
        }

        if (cooldownFillImage == null)
        {
            Debug.LogWarning($"[{nameof(SkillCooldownUI)}] Cooldown Fill Image is not assigned on {name}.", this);
        }
    }

    private void ApplyFillDefaults()
    {
        if (cooldownFillImage == null)
        {
            return;
        }

        if (iconImage != null && cooldownFillImage.sprite == null)
        {
            cooldownFillImage.sprite = iconImage.sprite;
        }

        cooldownFillImage.type = Image.Type.Filled;
        cooldownFillImage.fillMethod = Image.FillMethod.Vertical;
        cooldownFillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        cooldownFillImage.raycastTarget = false;
        cooldownFillImage.color = cooldownOverlayColor;
        cooldownFillImage.fillAmount = 0f;
    }

    private void ApplyReadyVisual()
    {
        if (iconImage != null)
        {
            iconImage.color = readyIconColor;
        }
    }

    private void ApplyCoolingVisual()
    {
        if (iconImage != null)
        {
            iconImage.color = coolingIconColor;
        }
    }

    private void ApplyLockedVisual()
    {
        if (iconImage != null)
        {
            iconImage.color = lockedIconColor;
        }
    }

    private void ApplyIdleVisual()
    {
        if (IsUnlocked)
        {
            ApplyReadyVisual();
            return;
        }

        ApplyLockedVisual();
    }

    private void ApplyKeyLabel()
    {
        if (keyText != null)
        {
            keyText.text = keyLabel;
        }

        if (keyTMPText != null)
        {
            keyTMPText.text = keyLabel;
        }
    }

    private void UpdateRemainingText(float seconds)
    {
        if (!showRemainingSeconds)
        {
            SetCooldownText(string.Empty);
            return;
        }

        int decimals = Mathf.Clamp(remainingTimeDecimalPlaces, 0, 3);
        string format = $"F{decimals}";
        SetCooldownText(seconds.ToString(format));
    }

    private void SetCooldownText(string value)
    {
        if (cooldownText != null)
        {
            cooldownText.text = value;
        }

        if (cooldownTMPText != null)
        {
            cooldownTMPText.text = value;
        }
    }

    private void SetFillAmount(float value)
    {
        float clamped = Mathf.Clamp01(value);
        normalizedProgress = clamped;

        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = clamped;
        }

        CooldownProgressChanged?.Invoke(clamped);
    }

    private void UpdateVisualByRemainingTime(float remaining)
    {
        float progress = cooldownDuration <= 0f ? 1f : 1f - Mathf.Clamp01(remaining / cooldownDuration);
        SetFillAmount(progress);
        UpdateRemainingText(remaining);
    }

    private void SetButtonInteractableDuringCooldown(bool isCoolingDown)
    {
        if (!blockClickWhileCooling)
        {
            return;
        }

        if (isCoolingDown)
        {
            if (clickButton != null)
            {
                if (!hasCachedButtonInteractable)
                {
                    cachedButtonInteractable = clickButton.interactable;
                    hasCachedButtonInteractable = true;
                }

                clickButton.interactable = false;
            }

            if (interactionBlocker != null)
            {
                if (!hasCachedCanvasGroupBlocksRaycasts)
                {
                    cachedCanvasGroupBlocksRaycasts = interactionBlocker.blocksRaycasts;
                    hasCachedCanvasGroupBlocksRaycasts = true;
                }

                interactionBlocker.blocksRaycasts = false;
            }

            return;
        }

        if (clickButton != null && hasCachedButtonInteractable)
        {
            clickButton.interactable = cachedButtonInteractable;
            hasCachedButtonInteractable = false;
        }

        if (interactionBlocker != null && hasCachedCanvasGroupBlocksRaycasts)
        {
            interactionBlocker.blocksRaycasts = cachedCanvasGroupBlocksRaycasts;
            hasCachedCanvasGroupBlocksRaycasts = false;
        }
    }
}
