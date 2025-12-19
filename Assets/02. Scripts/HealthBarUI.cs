using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBarUI : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        // PlayerMovement로 변경
        PlayerMovement.OnHealthChanged += UpdateHealthBar;
    }

    void OnDisable()
    {
        PlayerMovement.OnHealthChanged -= UpdateHealthBar;
    }

    // int -> float으로 변경 (PlayerStats가 float을 반환하므로)
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
    }
}