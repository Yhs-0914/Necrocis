using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Necrocis
{
    /// <summary>
    /// 플레이어 이동 + 방향별 스프라이트 애니메이션
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }
        private static readonly Quaternion FixedPlayerRotation = Quaternion.identity;

        [Header("기본 스탯")]
        [FormerlySerializedAs("moveSpeed")]
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float baseMaxHealth = 150f;
        [SerializeField] private float baseAttackPower = 10f;

        [Header("스프라이트 렌더러")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("대기 애니메이션")]
        [SerializeField] private Sprite[] idleSprites;

        [Header("이동 애니메이션 (방향별)")]
        [SerializeField] private Sprite[] walkDownSprites;
        [SerializeField] private Sprite[] walkUpSprites;
        [SerializeField] private Sprite[] walkLeftSprites;
        [SerializeField] private Sprite[] walkRightSprites;

        [Header("애니메이션 설정")]
        [SerializeField] private float idleFrameRate = 4f;
        [SerializeField] private float walkFrameRate = 8f;

        [Header("위치 고정")]
        [SerializeField] private bool lockYPosition = false;
        [SerializeField] private float lockedY = -2f;
        [SerializeField] private float groundOffsetY = -2f;
        [SerializeField] private bool useDynamicGroundHeight = true;

        // ── 직업별 스프라이트 ──────────────────────────────────────
        [System.Serializable]
        public class JobSpriteSet
        {
            public Sprite[] idleSprites;
            public Sprite[] walkDownSprites;
            public Sprite[] walkUpSprites;
            public Sprite[] walkLeftSprites;
            public Sprite[] walkRightSprites;
        }

        [Header("직업별 스프라이트")]
        [SerializeField] private JobSpriteSet warriorSprites;
        [SerializeField] private JobSpriteSet mageSprites;
        [SerializeField] private JobSpriteSet archerSprites;

        private JobSpriteSet GetCurrentSet()
        {
            switch (LevelUpManager.GetCurrentJob())
            {
                case JobType.Warrior: return warriorSprites;
                case JobType.Mage:    return mageSprites;
                case JobType.Archer:  return archerSprites;
                default:              return null;
            }
        }
        // ──────────────────────────────────────────────────────────

        // 방향
        public enum Direction { Down, Up, Left, Right }
        private Direction currentDirection = Direction.Up;
        private Vector3 lastMoveDirection = Vector3.forward;
        private bool isMoving = false;

        // 애니메이션
        private Sprite[] currentAnimation;
        private int currentFrame = 0;
        private float frameTimer = 0f;
        private float currentFrameRate;

        // 이동
        private Vector3 movement;
        private Rigidbody rb;
        private CharacterController characterController;
        private PlayerStats playerStats;
        private bool playerStatsConfigured;
        private bool playerStatsEventsBound;
        private bool deathHandled;

        // 플레이 모드 진입 시 이전 세션의 static 참조를 초기화 (MissingReferenceException 방지)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        public PlayerStats Stats => playerStats;
        public CharacterStats RuntimeStats => playerStats != null ? playerStats.RuntimeStats : null;
        public float MoveSpeed => playerStats != null ? playerStats.MoveSpeed : 0f;
        public float CurrentHealth => playerStats != null ? playerStats.CurrentHealth : 0f;
        public float MaxHealth => playerStats != null ? playerStats.MaxHealth : 0f;
        public float AttackPower => playerStats != null ? playerStats.AttackPower : 0f;
        public bool IsDead => playerStats != null && playerStats.IsDead;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    GameObject spriteObj = new GameObject("Sprite");
                    spriteObj.transform.SetParent(transform);
                    spriteObj.transform.localPosition = Vector3.zero;
                    spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
                }
            }

            spriteRenderer.color = Color.white;

            Billboard billboard = spriteRenderer.GetComponent<Billboard>();
            if (billboard == null)
                billboard = spriteRenderer.gameObject.AddComponent<Billboard>();
            billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);

            SpriteYSort ySort = spriteRenderer.GetComponent<SpriteYSort>();
            if (ySort == null)
                ySort = spriteRenderer.gameObject.AddComponent<SpriteYSort>();
            ySort.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
            ySort.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);

            rb = GetComponent<Rigidbody>();
            characterController = GetComponent<CharacterController>();
            EnsurePlayerStats();
            EnsureClassSkillController();
            lastMoveDirection = DirectionToVector(currentDirection);
            ApplyLockedRotation();
        }

        private void Start()
        {
            gameObject.tag = "Player";

            Vector3 pos = transform.position;
            pos.y = 0f;
            transform.position = pos;

            SetAnimation(idleSprites, idleFrameRate);
            ApplyLockedRotation();

            Debug.Log($"[Player] 시작 위치: {transform.position}");
        }

        private void Update()
        {
            HandleInput();
            UpdateAnimation();
            ApplyLockedRotation();
        }

        private void FixedUpdate()
        {
            Move();
            ApplyLockedY();
            ApplyLockedRotation();
        }

        private void HandleInput()
        {
            if (!Application.isFocused || Time.timeSinceLevelLoad < 0.5f)
            {
                movement = Vector3.zero;
                isMoving = false;
                return;
            }

            if (deathHandled)
            {
                movement = Vector3.zero;
                isMoving = false;
                return;
            }

            var input = InputManager.Instance;

            Vector2 moveInput = input.MoveAction.ReadValue<Vector2>();
            movement = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            isMoving = movement.sqrMagnitude > 0.01f;
            if (isMoving)
                lastMoveDirection = movement;

            if (isMoving)
                UpdateDirection(moveInput.x, moveInput.y);

            UpdateAnimationState();
        }

        private void UpdateDirection(float h, float v)
        {
            if (Mathf.Abs(v) >= Mathf.Abs(h))
                currentDirection = v > 0 ? Direction.Up : Direction.Down;
            else
                currentDirection = h > 0 ? Direction.Right : Direction.Left;
        }

        private void UpdateAnimationState()
        {
            JobSpriteSet set = GetCurrentSet();

            Sprite[] newAnimation;
            float newFrameRate;

            if (!isMoving)
            {
                newFrameRate = idleFrameRate;
                newAnimation = (set?.idleSprites?.Length > 0) ? set.idleSprites : idleSprites;
            }
            else
            {
                newFrameRate = walkFrameRate;
                switch (currentDirection)
                {
                    case Direction.Down:
                        newAnimation = (set?.walkDownSprites?.Length > 0) ? set.walkDownSprites : walkDownSprites;
                        break;
                    case Direction.Up:
                        newAnimation = (set?.walkUpSprites?.Length > 0) ? set.walkUpSprites : walkUpSprites;
                        break;
                    case Direction.Left:
                        newAnimation = (set?.walkLeftSprites?.Length > 0) ? set.walkLeftSprites : walkLeftSprites;
                        break;
                    case Direction.Right:
                        newAnimation = (set?.walkRightSprites?.Length > 0) ? set.walkRightSprites : walkRightSprites;
                        break;
                    default:
                        newAnimation = (set?.walkDownSprites?.Length > 0) ? set.walkDownSprites : walkDownSprites;
                        break;
                }
            }

            if (newAnimation != currentAnimation)
                SetAnimation(newAnimation, newFrameRate);
        }

        private void SetAnimation(Sprite[] sprites, float frameRate)
        {
            currentAnimation = sprites;
            currentFrameRate = frameRate;
            currentFrame = 0;
            frameTimer = 0f;

            if (currentAnimation != null && currentAnimation.Length > 0)
                spriteRenderer.sprite = currentAnimation[0];
        }

        private void UpdateAnimation()
        {
            if (currentAnimation == null || currentAnimation.Length == 0) return;

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / currentFrameRate;

            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                currentFrame = (currentFrame + 1) % currentAnimation.Length;

                if (spriteRenderer != null && currentFrame < currentAnimation.Length)
                    spriteRenderer.sprite = currentAnimation[currentFrame];
            }
        }

        private void Move()
        {
            if (!isMoving)
            {
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                return;
            }

            Vector3 moveVector = movement * MoveSpeed * Time.fixedDeltaTime;
            bool moved = TryMoveWithHeight(moveVector);

            if (!moved && rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private bool TryMoveWithHeight(Vector3 moveVector)
        {
            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                ApplyMove(moveVector);
                return true;
            }

            Vector3 currentPos = transform.position;
            Vector3 targetPos = currentPos + moveVector;
            if (biome.CanMove(currentPos, targetPos))
            {
                ApplyMove(moveVector);
                return true;
            }

            Vector3 moveX = new Vector3(moveVector.x, 0f, 0f);
            Vector3 moveZ = new Vector3(0f, 0f, moveVector.z);

            if (Mathf.Abs(moveVector.x) >= Mathf.Abs(moveVector.z))
            {
                if (moveX.sqrMagnitude > 0f && biome.CanMove(currentPos, currentPos + moveX))
                {
                    ApplyMove(moveX);
                    return true;
                }
                if (moveZ.sqrMagnitude > 0f && biome.CanMove(currentPos, currentPos + moveZ))
                {
                    ApplyMove(moveZ);
                    return true;
                }
                return false;
            }

            if (moveZ.sqrMagnitude > 0f && biome.CanMove(currentPos, currentPos + moveZ))
            {
                ApplyMove(moveZ);
                return true;
            }
            if (moveX.sqrMagnitude > 0f && biome.CanMove(currentPos, currentPos + moveX))
            {
                ApplyMove(moveX);
                return true;
            }
            return false;
        }

        private void ApplyMove(Vector3 moveVector)
        {
            if (characterController != null)
                characterController.Move(moveVector);
            else if (rb != null)
                rb.MovePosition(rb.position + moveVector);
            else
                transform.position += moveVector;
        }

        public void SpawnAt(Vector3 position)
        {
            transform.position = position;

            if (characterController != null)
            {
                characterController.enabled = false;
                transform.position = position;
                characterController.enabled = true;
            }

            ApplyLockedY();
            ApplyLockedRotation();
        }

        public void LockY(float y)
        {
            lockYPosition = true;
            lockedY = y;
            groundOffsetY = y;
            ApplyLockedY();
        }

        public void UnlockY()
        {
            lockYPosition = false;
        }

        private void ApplyLockedY()
        {
            if (!lockYPosition) return;

            float desiredY = lockedY;
            BiomeManager biome = BiomeManager.Active;
            if (useDynamicGroundHeight && biome != null)
                desiredY = biome.GetGroundHeight(transform.position) + groundOffsetY;

            if (characterController != null)
            {
                Vector3 pos = transform.position;
                pos.y = desiredY;
                transform.position = pos;
                return;
            }

            if (rb != null)
            {
                Vector3 pos = rb.position;
                pos.y = desiredY;
                rb.position = pos;

                Vector3 vel = rb.linearVelocity;
                vel.y = 0f;
                rb.linearVelocity = vel;
                return;
            }

            Vector3 fallback = transform.position;
            fallback.y = desiredY;
            transform.position = fallback;
        }

        private void ApplyLockedRotation()
        {
            if (rb != null)
            {
                rb.rotation = FixedPlayerRotation;
                rb.angularVelocity = Vector3.zero;
            }
            transform.rotation = FixedPlayerRotation;
        }

        public Direction GetCurrentDirection() => currentDirection;

        public Vector3 GetLogicalFacingDirection()
        {
            if (movement.sqrMagnitude > 0.0001f)
                return movement.normalized;
            if (lastMoveDirection.sqrMagnitude > 0.0001f)
                return lastMoveDirection.normalized;
            return DirectionToVector(currentDirection);
        }

        public static Vector3 DirectionToVector(Direction direction)
        {
            return direction switch
            {
                Direction.Up    => Vector3.forward,
                Direction.Down  => Vector3.back,
                Direction.Left  => Vector3.left,
                Direction.Right => Vector3.right,
                _               => Vector3.forward
            };
        }

        public void FaceDirection(Direction direction)
        {
            currentDirection = direction;
            lastMoveDirection = DirectionToVector(direction);
            UpdateAnimationState();
        }

        public void RefreshBaseStats(bool resetCurrentHealth = false)
        {
            EnsurePlayerStats();
            playerStats.ConfigureBaseStats(baseMoveSpeed, baseMaxHealth, baseAttackPower, resetCurrentHealth);
            playerStatsConfigured = true;
        }

        public void TakeDamage(float damage)
        {
            if (deathHandled) return;

            Health health = GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);
            else
                playerStats?.TakeDamage(damage);
        }

        public void Heal(float amount)
        {
            EnsurePlayerStats();
            playerStats.Heal(amount);
        }

        public void AddStatModifier(CharacterStatModifier modifier)
        {
            EnsurePlayerStats();
            playerStats.ApplyModifier(modifier);
        }

        public void AddStatModifiers(IEnumerable<CharacterStatModifierData> modifiers, object source)
        {
            EnsurePlayerStats();
            playerStats.ApplyModifiers(modifiers, source);
        }

        public void ApplyOrReplaceStatModifiers(IEnumerable<CharacterStatModifierData> modifiers, object source)
        {
            EnsurePlayerStats();
            playerStats.ApplyOrReplaceSourceModifiers(modifiers, source);
        }

        public int RemoveStatModifiersFromSource(object source)
        {
            EnsurePlayerStats();
            return playerStats.RemoveModifiersFromSource(source);
        }

        private void EnsurePlayerStats()
        {
            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
                if (playerStats == null)
                    playerStats = gameObject.AddComponent<PlayerStats>();
            }

            if (!playerStatsConfigured)
            {
                playerStats.ConfigureBaseStats(baseMoveSpeed, baseMaxHealth, baseAttackPower, true);
                playerStatsConfigured = true;
            }

            if (!playerStatsEventsBound)
            {
                playerStats.HealthChanged += HandlePlayerHealthChanged;
                playerStatsEventsBound = true;
            }
        }

        private void EnsureClassSkillController()
        {
            if (GetComponent<PlayerClassSkillController>() == null)
                gameObject.AddComponent<PlayerClassSkillController>();
        }

        private void HandlePlayerHealthChanged(CharacterStats _, CharacterHealthChangedEventArgs args)
        {
            if (args.CurrentValue < args.PreviousValue)
            {
                float damageTaken = args.PreviousValue - args.CurrentValue;
                Debug.Log($"[Player] 피해 {damageTaken} 받음 | HP {args.CurrentValue}/{args.MaxValue}");
            }
            else if (args.CurrentValue > args.PreviousValue)
            {
                float healed = args.CurrentValue - args.PreviousValue;
                Debug.Log($"[Player] 회복 {healed} | HP {args.CurrentValue}/{args.MaxValue}");
            }

            if (!deathHandled && args.CurrentValue <= 0f)
                Die();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Die()
        {
            deathHandled = true;
            movement = Vector3.zero;
            isMoving = false;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            SetAnimation(idleSprites, idleFrameRate);

            PlayerAttack attack = GetComponent<PlayerAttack>();
            if (attack != null)
                attack.enabled = false;

            PlayerClassSkillController classSkillController = GetComponent<PlayerClassSkillController>();
            if (classSkillController != null)
                classSkillController.enabled = false;

            enabled = false;
            Debug.Log("[Player] HP가 0이 되어 사망했습니다.");
        }
    }
}
