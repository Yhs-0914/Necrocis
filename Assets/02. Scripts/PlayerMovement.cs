using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    // 상태 변수들
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    // [중요] 공격 스크립트가 갖다 쓸 변수 (외부 공개)
    public Vector3 lastMoveDirection { get; private set; }

    [Header("Health State")]
    private float currentHealth;

    // UI 업데이트용 이벤트
    public static event Action<float, float> OnHealthChanged;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 회전 고정 (쓰러짐 방지)
        rb.freezeRotation = true;
        // 회전 제약 명시적 적용 (X, Z축 회전 잠금)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY; // Y축 이동도 필요없으면 잠금(선택)

        lastMoveDirection = Vector3.forward;

        // [통합] 스탯 매니저에서 최대 체력 가져오기
        float maxHP = PlayerStats.Instance.GetHealth();
        currentHealth = maxHP;

        OnHealthChanged?.Invoke(currentHealth, maxHP);
    }

    // Input System: Send Messages 방식 (PlayerInput 컴포넌트 필요)
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // [중요] 이동 입력이 있을 때만 방향 갱신 (멈췄을 때 마지막 방향 기억)
        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        }
    }

    void OnDash(InputValue value)
    {
        if (value.isPressed && !isDashing && dashCooldownTimer <= 0)
        {
            StartDash();
        }
    }

    void Update()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        // 스프라이트 좌우 반전
        if (moveInput.x < 0) spriteRenderer.flipX = true;
        else if (moveInput.x > 0) spriteRenderer.flipX = false;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                rb.linearVelocity = Vector3.zero; // 대쉬 끝
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = new Vector3(
                dashDirection.x * dashSpeed,
                rb.linearVelocity.y,
                dashDirection.z * dashSpeed
            );
        }
        else
        {
            // [통합] 스탯 매니저에서 현재 이동속도 가져오기!
            float currentSpeed = PlayerStats.Instance.GetSpeed();

            Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
            rb.MovePosition(rb.position + move * currentSpeed * Time.fixedDeltaTime);
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (moveInput != Vector2.zero)
        {
            dashDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        }
        else
        {
            dashDirection = spriteRenderer.flipX ? Vector3.left : Vector3.right;
        }
    }

    // 데미지 처리
    public void TakeDamage(float damage)
    {
        // [통합] 방어력 적용 (데미지 감소 공식 예시: 데미지 - 방어력)
        float defense = PlayerStats.Instance.GetDefense();
        float finalDamage = Mathf.Max(1, damage - defense); // 최소 1 데미지

        currentHealth -= finalDamage;

        float maxHP = PlayerStats.Instance.GetHealth();
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHP);

        OnHealthChanged?.Invoke(currentHealth, maxHP);

        Debug.Log($"플레이어 피격! 데미지: {finalDamage} (방어력: {defense}) / 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("플레이어 사망");
        // 게임 오버 로직 추가 가능
        Time.timeScale = 0f;
    }
}