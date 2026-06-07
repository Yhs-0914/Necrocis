using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Necrocis
{
    public class PlayerController : MonoBehaviour
    {
        public static event System.Action OnPlayerDied;

        private static PlayerController instance;

        public static PlayerController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PlayerController>();
                }

                return instance;
            }
            private set => instance = value;
        }

        private static readonly Quaternion FixedPlayerRotation = Quaternion.identity;

        [Header("Sprite Renderer")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Idle Sprites")]
        [SerializeField] private Sprite[] idleSprites;

        [Header("Walk Sprites By Direction")]
        [SerializeField] private Sprite[] walkDownSprites;
        [SerializeField] private Sprite[] walkUpSprites;
        [SerializeField] private Sprite[] walkLeftSprites;
        [SerializeField] private Sprite[] walkRightSprites;

        [Header("Warrior Sprites")]
        [SerializeField] private Sprite[] warriorIdleSprites;
        [SerializeField] private Sprite[] warriorWalkDownSprites;
        [SerializeField] private Sprite[] warriorWalkUpSprites;
        [SerializeField] private Sprite[] warriorWalkLeftSprites;
        [SerializeField] private Sprite[] warriorWalkRightSprites;

        [Header("Mage Sprites")]
        [SerializeField] private Sprite[] mageIdleSprites;
        [SerializeField] private Sprite[] mageWalkDownSprites;
        [SerializeField] private Sprite[] mageWalkUpSprites;
        [SerializeField] private Sprite[] mageWalkLeftSprites;
        [SerializeField] private Sprite[] mageWalkRightSprites;

        [Header("Archer Sprites")]
        [SerializeField] private Sprite[] archerIdleSprites;
        [SerializeField] private Sprite[] archerWalkDownSprites;
        [SerializeField] private Sprite[] archerWalkUpSprites;
        [SerializeField] private Sprite[] archerWalkLeftSprites;
        [SerializeField] private Sprite[] archerWalkRightSprites;

        [Header("근접공격 스프라이트 (8방향)")]
        [SerializeField] private Sprite[] meleeDown;
        [SerializeField] private Sprite[] meleeUp;
        [SerializeField] private Sprite[] meleeLeft;
        [SerializeField] private Sprite[] meleeRight;
        [SerializeField] private Sprite[] meleeDownLeft;
        [SerializeField] private Sprite[] meleeDownRight;
        [SerializeField] private Sprite[] meleeUpLeft;
        [SerializeField] private Sprite[] meleeUpRight;

        [Header("원거리공격 스프라이트 (8방향)")]
        [SerializeField] private Sprite[] rangedDown;
        [SerializeField] private Sprite[] rangedUp;
        [SerializeField] private Sprite[] rangedLeft;
        [SerializeField] private Sprite[] rangedRight;
        [SerializeField] private Sprite[] rangedDownLeft;
        [SerializeField] private Sprite[] rangedDownRight;
        [SerializeField] private Sprite[] rangedUpLeft;
        [SerializeField] private Sprite[] rangedUpRight;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.8f;
        [SerializeField] private bool invincibleDuringDash = true;

        [Header("Animation Settings")]
        [SerializeField] private float idleFrameRate = 4f;
        [SerializeField] private float walkFrameRate = 8f;
        [SerializeField] private float attackAnimDuration = 0.3f;
        [SerializeField] private float attackFrameRate = 12f;
        [SerializeField] private Sprite[] deathSprites;
        [SerializeField] private float deathFrameRate = 8f;

        [Header("Position Lock")]
        [SerializeField] private bool lockYPosition = false;
        [SerializeField] private float lockedY = -2f;
        [SerializeField] private float groundOffsetY = -2f;
        [SerializeField] private bool useDynamicGroundHeight = true;

        // 4諛⑺뼢 ?닿굅??(?ㅽ봽?쇱씠???좊땲硫붿씠??諛?怨듦꺽 諛⑺뼢??
        public enum Direction { Down, Up, Left, Right }
        private Direction currentDirection = Direction.Up;      // ?꾩옱 諛붾씪蹂대뒗 諛⑺뼢
        private Vector3 lastMoveDirection = Vector3.forward;
        private bool isMoving = false;

        // 대시 상태
        private bool isDashing = false;
        private float lastDashTime = float.NegativeInfinity;
        private Vector3 dashVelocity;

        // [Sound] 발소리 타이머
        [Header("Sound")]
        [SerializeField] private float footstepInterval = 0.35f;
        private float nextFootstepTime;

        // 공격 애니메이션 상태
        private bool isPlayingAttackAnim = false;
        private float attackAnimEndTime = 0f;                       // ?대룞 以??щ?

        // ?ㅽ봽?쇱씠???좊땲硫붿씠???곹깭
        private Sprite[] currentAnimation;   // ?꾩옱 ?ъ깮 以묒씤 ?ㅽ봽?쇱씠??諛곗뿴
        private int currentFrame = 0;        // ?꾩옱 ?꾨젅???몃뜳??
        private float frameTimer = 0f;       // ?꾨젅???꾪솚 ??대㉧
        private float currentFrameRate;      // ?꾩옱 ?꾨젅???띾룄

        // ?대룞 諛?臾쇰━
        private Vector3 movement;                  // ?대룞 踰≫꽣
        private Rigidbody rb;                      // 臾쇰━ 而댄룷?뚰듃 (?덉쑝硫??ъ슜)
        private CharacterController characterController; // CharacterController (?덉쑝硫??곗꽑 ?ъ슜)
        private Collider cachedHitCollider;
        private Health cachedHealth;
        private PlayerStats playerStats;           // ?ㅽ꺈 而댄룷?뚰듃 李몄“
        private bool playerStatsConfigured;        // 湲곕낯 ?ㅽ꺈 ?ㅼ젙 ?꾨즺 ?щ?
        private bool playerStatsEventsBound;       // HP 蹂寃??대깽??援щ룆 ?щ?
        private bool deathHandled;                 // ?щ쭩 泥섎━ ?꾨즺 ?щ? (以묐났 諛⑹?)
        private bool isPlayingDeathAnimation;
        private Coroutine deathRoutine;

        // ?몃? ?묎렐???꾨줈?쇳떚 (PlayerStats媛 ?놁쑝硫??덉쟾??湲곕낯媛?諛섑솚)
        public PlayerStats Stats => playerStats;
        public CharacterStats RuntimeStats => playerStats != null ? playerStats.RuntimeStats : null;
        public float MoveSpeed => playerStats != null ? playerStats.MoveSpeed : 0f;
        public float CurrentHealth => playerStats != null ? playerStats.CurrentHealth : 0f;
        public float MaxHealth => playerStats != null ? playerStats.MaxHealth : 0f;
        public float AttackPower => playerStats != null ? playerStats.AttackPower : 0f;
        public float AttackSpeed => playerStats != null ? playerStats.AttackSpeed : 0f;
        public float AttackRange => playerStats != null ? playerStats.AttackRange : 0f;
        public float Magic => playerStats != null ? playerStats.Magic : 0f;
        public float SkillCooldownReduction => playerStats != null ? playerStats.SkillCooldownReduction : 0f;
        public bool IsDead => playerStats != null && playerStats.IsDead;
        public bool IsMoving => isMoving;
        public Collider HitCollider
        {
            get
            {
                if (cachedHitCollider == null)
                {
                    cachedHitCollider = GetComponent<Collider>();
                    if (cachedHitCollider == null)
                    {
                        cachedHitCollider = GetComponentInChildren<Collider>();
                    }
                }

                return cachedHitCollider;
            }
        }

        public Health HealthComponent
        {
            get
            {
                if (cachedHealth == null)
                {
                    cachedHealth = GetComponent<Health>();
                }

                return cachedHealth;
            }
        }
        // ?좊땲???앸챸二쇨린: 李몄“瑜?罹먯떆?섍퀬 湲곕낯 ?곹깭瑜?珥덇린?뷀빀?덈떎.

        private void Awake()
        {
            // ?깃????⑦꽩
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ?ㅽ봽?쇱씠???뚮뜑??李얘린
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

            // ?ㅽ봽?쇱씠??湲곕낯 ?ㅼ젙
            spriteRenderer.color = Color.white;

            Billboard billboard = spriteRenderer.GetComponent<Billboard>();
            if (billboard == null)
            {
                billboard = spriteRenderer.gameObject.AddComponent<Billboard>();
            }
            billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);

            SpriteYSort ySort = spriteRenderer.GetComponent<SpriteYSort>();
            if (ySort == null)
            {
                ySort = spriteRenderer.gameObject.AddComponent<SpriteYSort>();
            }
            ySort.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
            ySort.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);

            // 臾쇰━ 而댄룷?뚰듃 ?뺤씤
            rb = GetComponent<Rigidbody>();
            characterController = GetComponent<CharacterController>();
            EnsurePlayerStats();
            EnsureClassSkillController();
            EnsureDeathScreen();
            lastMoveDirection = DirectionToVector(currentDirection);
            ApplyLockedRotation();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
            if (instance == this)
            {
                instance = null;
            }
        }
        // ?좊땲???앸챸二쇨린: Awake ?댄썑 珥덇린 ?고????ㅼ젙???섑뻾?⑸땲??

        private void Start()
        {
            // ?쒓렇 ?ㅼ젙
            gameObject.tag = "Player";

            // Y ?꾩튂 媛뺤젣 (諛붾떏 ??
            Vector3 pos = transform.position;
            pos.y = 0f;
            transform.position = pos;
            ApplyJobVisual(LevelUpManager.GetCurrentJob());

            // 珥덇린 ?좊땲硫붿씠??(?湲?
            SetAnimation(idleSprites, idleFrameRate);
            ApplyLockedRotation();

            Debug.Log($"[Player] 시작 위치: {transform.position}");
        }
        // ?좊땲???앸챸二쇨린: 留??꾨젅??寃뚯엫?뚮젅??濡쒖쭅???ㅽ뻾?⑸땲??

        private void Update()
        {
            SyncDeathState();
            HandleInput();
            UpdateAnimation();
            ApplyLockedRotation();
            // [Sound] 이동 중 발소리
            if (isMoving && !isDashing && Time.time >= nextFootstepTime)
            {
                nextFootstepTime = Time.time + footstepInterval;
                AudioManager.Instance?.PlaySFX("PlayerFootstep");
            }
        }
        // ?좊땲???앸챸二쇨린: 臾쇰━ ?ㅽ뀦 湲곕컲 濡쒖쭅???ㅽ뻾?⑸땲??

        private void FixedUpdate()
        {
            SyncDeathState();
            Move();
            ApplyLockedY();
            ApplyLockedRotation();
        }

        /// <summary>
        /// ?낅젰 泥섎━
        /// </summary>
        private void HandleInput()
        {
            // ?ъ빱???녾굅??寃뚯엫 ?쒖옉 吏곹썑硫??낅젰 臾댁떆
            if (!Application.isFocused || Time.timeSinceLevelLoad < 0.5f)
            {
                movement = Vector3.zero;
                isMoving = false;
                return;
            }

            if (IsControlBlocked())
            {
                StopMotion();
                return;
            }

            // InputManager 湲곕컲 ?낅젰
            var input = InputManager.Instance;

            Vector2 moveInput = input.MoveAction.ReadValue<Vector2>();
            movement = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            isMoving = movement.sqrMagnitude > 0.01f;
            if (isMoving)
            {
                lastMoveDirection = movement;
            }

            // 諛⑺뼢 寃곗젙 (留덉?留??낅젰 諛⑺뼢 ?좎?)
            // 대시 입력 (Shift)
            if (input.DashAction.WasPressedThisFrame() && !isDashing && Time.time >= lastDashTime + dashCooldown)
            {
                StartCoroutine(DashCoroutine());
            }

            if (isMoving)
            {
                UpdateDirection(moveInput.x, moveInput.y);
            }

            // ?좊땲硫붿씠??蹂寃?
            UpdateAnimationState();
        }

        /// <summary>
        /// 諛⑺뼢 ?낅뜲?댄듃
        /// </summary>
        private void UpdateDirection(float h, float v)
        {
            // ?섏쭅 ?곗꽑
            if (Mathf.Abs(v) >= Mathf.Abs(h))
            {
                currentDirection = v > 0 ? Direction.Up : Direction.Down;
            }
            else
            {
                currentDirection = h > 0 ? Direction.Right : Direction.Left;
            }
        }

        /// <summary>
        /// ?좊땲硫붿씠???곹깭 ?낅뜲?댄듃
        /// </summary>
        private void UpdateAnimationState()
        {
            if (isPlayingAttackAnim) return;
            Sprite[] newAnimation;
            float newFrameRate;

            if (!isMoving)
            {
                // ?湲??좊땲硫붿씠??(?섎굹留??ъ슜)
                newFrameRate = idleFrameRate;
                newAnimation = idleSprites;
            }
            else
            {
                // ?대룞 ?좊땲硫붿씠??
                newFrameRate = walkFrameRate;
                switch (currentDirection)
                {
                    case Direction.Down:
                        newAnimation = walkDownSprites;
                        break;
                    case Direction.Up:
                        newAnimation = walkUpSprites;
                        break;
                    case Direction.Left:
                        newAnimation = walkLeftSprites;
                        break;
                    case Direction.Right:
                        newAnimation = walkRightSprites;
                        break;
                    default:
                        newAnimation = walkDownSprites;
                        break;
                }
            }

            // ?좊땲硫붿씠??蹂寃???由ъ뀑
            if (newAnimation != currentAnimation)
            {
                SetAnimation(newAnimation, newFrameRate);
            }
        }

        /// <summary>
        /// ?좊땲硫붿씠???ㅼ젙
        /// </summary>
        private void SetAnimation(Sprite[] sprites, float frameRate)
        {
            currentAnimation = sprites;
            currentFrameRate = frameRate;
            currentFrame = 0;
            frameTimer = 0f;

            // 泥??꾨젅??利됱떆 ?곸슜
            if (currentAnimation != null && currentAnimation.Length > 0)
            {
                spriteRenderer.sprite = currentAnimation[0];
            }
        }

        /// <summary>
        /// ?좊땲硫붿씠???꾨젅???낅뜲?댄듃
        /// </summary>
        private void UpdateAnimation()
        {
            if (isPlayingAttackAnim && Time.time >= attackAnimEndTime)
            {
                isPlayingAttackAnim = false;
                UpdateAnimationState();
                return;
            }

            if (isPlayingDeathAnimation)
            {
                return;
            }

            if (currentAnimation == null || currentAnimation.Length == 0) return;

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / currentFrameRate;

            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                currentFrame = (currentFrame + 1) % currentAnimation.Length;

                if (spriteRenderer != null && currentFrame < currentAnimation.Length)
                {
                    spriteRenderer.sprite = currentAnimation[currentFrame];
                }
            }
        }

        /// <summary>
        /// ?대룞 泥섎━
        /// </summary>
        private void Move()
        {
            if (IsControlBlocked())
            {
                StopMotion();
                return;
            }

            if (isDashing)
            {
                TryMoveWithHeight(dashVelocity * Time.fixedDeltaTime);
                return;
            }

            if (!isMoving)
            {
                if (rb != null)
                {
                    // ?대룞 ???????띾룄 ?쒓굅 (?쒕━?꾪듃 諛⑹?)
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
        // TryMoveWithHeight: ?묒뾽???쒕룄?섍퀬 ?깃났 ?щ?瑜?諛섑솚?⑸땲??

        // ?믪씠 湲곕컲 ?대룞 ?쒖빟 泥섎━
        // BiomeManager媛 ?덉쑝硫?CanMove()濡??대룞 媛???щ? ?뺤씤
        // ?媛곸꽑 ?대룞??遺덇??섎㈃ X/Z 異?媛쒕퀎濡??쒕룄 (踰??щ씪?대뵫 ?④낵)
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

            // ?媛곸꽑 ?대룞 遺덇? ????異뺣퀎 遺꾨━ ?대룞 ?쒕룄
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

        public bool TryMoveByWorld(Vector3 displacement)
        {
            displacement.y = 0f;
            float distance = displacement.magnitude;
            if (distance <= 0.000001f)
            {
                return true;
            }

            BiomeManager biome = BiomeManager.Active;
            float maxStep = biome != null
                ? Mathf.Max(0.05f, biome.TileSize * 0.45f)
                : distance;
            int stepCount = Mathf.Max(1, Mathf.CeilToInt(distance / maxStep));
            Vector3 step = displacement / stepCount;
            bool movedAny = false;

            for (int i = 0; i < stepCount; i++)
            {
                if (!TryMoveWithHeight(step))
                {
                    break;
                }

                movedAny = true;
            }

            return movedAny;
        }
        // ApplyMove: 蹂寃??ы빆???고???媛앹껜??諛섏쁺?⑸땲??

        // ?ㅼ젣 ?대룞 ?곸슜: CharacterController > Rigidbody > Transform ?곗꽑?쒖쐞
        private void ApplyMove(Vector3 moveVector)
        {
            if (characterController != null)
            {
                characterController.Move(moveVector);
            }
            else if (rb != null)
            {
                rb.MovePosition(rb.position + moveVector);
            }
            else
            {
                transform.position += moveVector;
            }
        }

        private bool IsControlBlocked()
        {
            return deathHandled || IsDead;
        }

        private void SyncDeathState()
        {
            if (!deathHandled && IsDead)
            {
                HandleDeath();
            }
        }

        private void StopMotion()
        {
            movement = Vector3.zero;
            isMoving = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// ?ㅽ룿 ?꾩튂濡??대룞
        /// </summary>
        public void SpawnAt(Vector3 position)
        {
            transform.position = position;

            if (rb != null)
            {
                rb.position = position;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (characterController != null)
            {
                characterController.enabled = false;
                transform.position = position;
                characterController.enabled = true;
            }

            ApplyLockedY();
            ApplyLockedRotation();
        }

        public void ReviveForRespawn()
        {
            deathHandled = false;
            movement = Vector3.zero;
            isMoving = false;
            isPlayingAttackAnim = false;
            isPlayingDeathAnimation = false;
            if (deathRoutine != null)
            {
                StopCoroutine(deathRoutine);
                deathRoutine = null;
            }

            EnsurePlayerStats();

            Health health = GetComponent<Health>();
            if (health != null)
                health.ResetHealth();
            else
                playerStats.RuntimeStats.ResetHealthToMax();

            PlayerAttack attack = GetComponent<PlayerAttack>();
            if (attack != null)
                attack.enabled = true;

            PlayerClassSkillController classSkillController = GetComponent<PlayerClassSkillController>();
            if (classSkillController != null)
                classSkillController.enabled = true;

            enabled = true;
            ApplyJobVisual(LevelUpManager.GetCurrentJob());
            SetAnimation(idleSprites, idleFrameRate);
            ApplyLockedRotation();
        }
        // LockY: ??而댄룷?뚰듃???듭떖 濡쒖쭅???ㅽ뻾?⑸땲??

        public void LockY(float y)
        {
            lockYPosition = true;
            lockedY = y;
            groundOffsetY = y;
            ApplyLockedY();
        }
        // UnlockY: ??而댄룷?뚰듃???듭떖 濡쒖쭅???ㅽ뻾?⑸땲??

        public void UnlockY()
        {
            lockYPosition = false;
        }
        // ApplyLockedY: 蹂寃??ы빆???고???媛앹껜??諛섏쁺?⑸땲??

        private void ApplyLockedY()
        {
            if (!lockYPosition) return;

            float desiredY = lockedY;
            BiomeManager biome = BiomeManager.Active;
            if (useDynamicGroundHeight && biome != null)
            {
                desiredY = biome.GetGroundHeight(transform.position) + groundOffsetY;
            }

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
        // ApplyLockedRotation: 蹂寃??ы빆???고???媛앹껜??諛섏쁺?⑸땲??

        private void ApplyLockedRotation()
        {
            if (rb != null)
            {
                rb.rotation = FixedPlayerRotation;
                rb.angularVelocity = Vector3.zero;
            }

            transform.rotation = FixedPlayerRotation;
        }

        /// <summary>
        /// ?꾩옱 諛⑺뼢 媛?몄삤湲?
        /// </summary>
        public Direction GetCurrentDirection()
        {
            return currentDirection;
        }

        public Vector3 GetLogicalFacingDirection()
        {
            if (movement.sqrMagnitude > 0.0001f)
            {
                return movement.normalized;
            }

            if (lastMoveDirection.sqrMagnitude > 0.0001f)
            {
                return lastMoveDirection.normalized;
            }

            return DirectionToVector(currentDirection);
        }

        public static Vector3 DirectionToVector(Direction direction)
        {
            return direction switch
            {
                Direction.Up => Vector3.forward,
                Direction.Down => Vector3.back,
                Direction.Left => Vector3.left,
                Direction.Right => Vector3.right,
                _ => Vector3.forward
            };
        }
        // RefreshBaseStats: 蹂寃??ы빆???고???媛앹껜??諛섏쁺?⑸땲??

        public void RefreshBaseStats(bool resetCurrentHealth = false)
        {
            EnsurePlayerStats();
            playerStats.ResetBaseStats(resetCurrentHealth);
            playerStatsConfigured = true;
        }
        // TakeDamage: ??而댄룷?뚰듃???듭떖 濡쒖쭅???ㅽ뻾?⑸땲??

        public void TakeDamage(float damage)
        {
            if (deathHandled) return;
            if (isDashing && invincibleDuringDash) return;

            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            else
            {
                float finalDamage = Mathf.Max(0f, damage);
                PlayerItemCombatEffects itemEffects = GetComponent<PlayerItemCombatEffects>();
                if (itemEffects != null)
                {
                    finalDamage *= itemEffects.GetIncomingDamageMultiplier();
                }

                playerStats?.TakeDamage(finalDamage);
            }
        }
        // Heal: ??而댄룷?뚰듃???듭떖 濡쒖쭅???ㅽ뻾?⑸땲??

        public void Heal(float amount)
        {
            EnsurePlayerStats();
            playerStats.Heal(amount);
        }
        // AddStatModifier: ?곹깭 ?먮뒗 而щ젆?섏쓣 媛깆떊?⑸땲??

        public void AddStatModifier(CharacterStatModifier modifier)
        {
            EnsurePlayerStats();
            playerStats.ApplyModifier(modifier);
        }
        // AddStatModifiers: ?곹깭 ?먮뒗 而щ젆?섏쓣 媛깆떊?⑸땲??

        public void AddStatModifiers(IEnumerable<CharacterStatModifierData> modifiers, object source)
        {
            EnsurePlayerStats();
            playerStats.ApplyModifiers(modifiers, source);
        }
        // ApplyOrReplaceStatModifiers: 蹂寃??ы빆???고???媛앹껜??諛섏쁺?⑸땲??

        public void ApplyOrReplaceStatModifiers(IEnumerable<CharacterStatModifierData> modifiers, object source)
        {
            EnsurePlayerStats();
            playerStats.ApplyOrReplaceSourceModifiers(modifiers, source);
        }
        // RemoveStatModifiersFromSource: ?곹깭 ?먮뒗 而щ젆?섏쓣 媛깆떊?⑸땲??

        public int RemoveStatModifiersFromSource(object source)
        {
            EnsurePlayerStats();
            return playerStats.RemoveModifiersFromSource(source);
        }
        // FaceDirection: ??而댄룷?뚰듃???듭떖 濡쒖쭅???ㅽ뻾?⑸땲??

        public void PlayAttackAnimation(bool isMelee)
        {
            Sprite[] sprites = isMelee ? GetMeleeSprites() : GetRangedSprites();
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"[PlayerController] 공격 스프라이트 미할당 - isMelee:{isMelee} dir:{lastMoveDirection}");
                return;
            }

            float duration = Mathf.Max(0.05f, attackAnimDuration);
            SetAnimation(sprites, attackFrameRate > 0f ? attackFrameRate : 12f);
            isPlayingAttackAnim = true;
            attackAnimEndTime = Time.time + duration;
            Debug.Log($"[PlayerController] 공격 애니 시작: sprites={sprites.Length} s[0]={sprites[0]?.name ?? "NULL"} duration={duration} frameRate={attackFrameRate}");
        }

        private Sprite[] GetMeleeSprites()
        {
            float x = lastMoveDirection.x;
            float z = lastMoveDirection.z;
            float absX = Mathf.Abs(x);
            float absZ = Mathf.Abs(z);
            bool diagX = absX > 0.3f;
            bool diagZ = absZ > 0.3f;

            if (diagX && diagZ)
            {
                if (x < 0 && z > 0) return meleeUpLeft;
                if (x > 0 && z > 0) return meleeUpRight;
                if (x < 0 && z < 0) return meleeDownLeft;
                return meleeDownRight;
            }
            if (absZ >= absX)
                return z > 0 ? meleeUp : meleeDown;
            return x > 0 ? meleeRight : meleeLeft;
        }

        private Sprite[] GetRangedSprites()
        {
            float x = lastMoveDirection.x;
            float z = lastMoveDirection.z;
            float absX = Mathf.Abs(x);
            float absZ = Mathf.Abs(z);
            bool diagX = absX > 0.3f;
            bool diagZ = absZ > 0.3f;

            if (diagX && diagZ)
            {
                if (x < 0 && z > 0) return rangedUpLeft;
                if (x > 0 && z > 0) return rangedUpRight;
                if (x < 0 && z < 0) return rangedDownLeft;
                return rangedDownRight;
            }
            if (absZ >= absX)
                return z > 0 ? rangedUp : rangedDown;
            return x > 0 ? rangedRight : rangedLeft;
        }

        private IEnumerator DashCoroutine()
        {
            isDashing = true;
            lastDashTime = Time.time;
            AudioManager.Instance?.PlaySFX("PlayerDash"); // [Sound] 대시

            Vector3 dir = lastMoveDirection.sqrMagnitude > 0.001f
                ? lastMoveDirection.normalized
                : DirectionToVector(currentDirection);
            dir.y = 0f;
            dashVelocity = dir * dashSpeed;

            yield return new WaitForSeconds(dashDuration);

            isDashing = false;
            dashVelocity = Vector3.zero;
        }

        public void FaceDirection(Direction direction)
        {
            currentDirection = direction;
            lastMoveDirection = DirectionToVector(direction);
            UpdateAnimationState();
        }
        // EnsurePlayerStats: ??而댄룷?뚰듃???듭떖 濡쒖쭅???ㅽ뻾?⑸땲??

        // PlayerStats 而댄룷?뚰듃 蹂댁옣: ?놁쑝硫??앹꽦, 誘몄꽕?뺤씠硫?湲곕낯媛믪쑝濡?珥덇린?? ?대깽??援щ룆
        private void EnsurePlayerStats()
        {
            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
                if (playerStats == null)
                {
                    playerStats = gameObject.AddComponent<PlayerStats>();
                }
            }

            if (!playerStatsConfigured)
            {
                playerStats.EnsureInitialized();
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
            {
                gameObject.AddComponent<PlayerClassSkillController>();
            }
        }

        private PlayerDeathScreen EnsureDeathScreen()
        {
            PlayerDeathScreen deathScreen = GetComponent<PlayerDeathScreen>();
            if (deathScreen == null)
            {
                deathScreen = gameObject.AddComponent<PlayerDeathScreen>();
            }

            return deathScreen;
        }

        private void OnEnable()
        {
            LevelUpManager.OnJobChanged += HandleJobChanged;
        }

        private void OnDisable()
        {
            LevelUpManager.OnJobChanged -= HandleJobChanged;
        }

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (!deathHandled) return;

            deathHandled = false;
            movement = Vector3.zero;
            isMoving = false;
            isDashing = false;
            dashVelocity = Vector3.zero;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (characterController != null)
                characterController.enabled = true;

            enabled = true;

            PlayerAttack attack = GetComponent<PlayerAttack>();
            if (attack != null) attack.enabled = true;

            PlayerClassSkillController classSkill = GetComponent<PlayerClassSkillController>();
            if (classSkill != null) classSkill.enabled = true;

            Health health = GetComponent<Health>();
            if (health != null) health.ResetHealth();
            else if (playerStats != null) playerStats.RuntimeStats?.ResetHealthToMax();
        }

        private void HandleJobChanged(JobType job)
        {
            ApplyJobVisual(job);
        }

        private void ApplyJobVisual(JobType job)
        {
            switch (job)
            {
                case JobType.Warrior:
                    idleSprites      = warriorIdleSprites;
                    walkDownSprites  = warriorWalkDownSprites;
                    walkUpSprites    = warriorWalkUpSprites;
                    walkLeftSprites  = warriorWalkLeftSprites;
                    walkRightSprites = warriorWalkRightSprites;
                    break;
                case JobType.Mage:
                    idleSprites      = mageIdleSprites;
                    walkDownSprites  = mageWalkDownSprites;
                    walkUpSprites    = mageWalkUpSprites;
                    walkLeftSprites  = mageWalkLeftSprites;
                    walkRightSprites = mageWalkRightSprites;
                    break;
                case JobType.Archer:
                    idleSprites      = archerIdleSprites;
                    walkDownSprites  = archerWalkDownSprites;
                    walkUpSprites    = archerWalkUpSprites;
                    walkLeftSprites  = archerWalkLeftSprites;
                    walkRightSprites = archerWalkRightSprites;
                    break;
                default:
                    return;
            }

            SetAnimation(idleSprites, idleFrameRate);
        }

        // HP 蹂寃?肄쒕갚: ?곕?吏/?뚮났 濡쒓렇 異쒕젰 + HP 0?대㈃ ?щ쭩 泥섎━
        private void HandlePlayerHealthChanged(CharacterStats _, CharacterHealthChangedEventArgs args)
        {
            if (args.CurrentValue <= 0f && args.PreviousValue > 0f)
            {
                if (TryReviveFromSplitRegeneration(args.MaxValue))
                {
                    return;
                }
            }

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
            {
                HandleDeath();
            }
        }

        private bool TryReviveFromSplitRegeneration(float maxHealth)
        {
            if (playerStats != null && playerStats.CurrentHealth > 0f)
            {
                return true;
            }

            PlayerItemCombatEffects itemEffects = GetComponent<PlayerItemCombatEffects>();
            if (itemEffects == null || playerStats == null)
            {
                return false;
            }

            if (!itemEffects.TryConsumeSplitRegeneration(0f, maxHealth, out float reviveHealth))
            {
                return false;
            }

            playerStats.RuntimeStats.RestoreHealth(reviveHealth);
            Health health = GetComponent<Health>();
            health?.GrantTemporaryInvincibility(0.5f);
            Debug.Log($"[Player] 분열 재생 발동 | HP {playerStats.CurrentHealth}/{playerStats.MaxHealth}");
            return true;
        }
        // Die: ??而댄룷?뚰듃???듭떖 濡쒖쭅???ㅽ뻾?⑸땲??

        public void HandleDeath()
        {
            if (deathHandled)
            {
                return;
            }

            Die();
        }

        // ?щ쭩 泥섎━: ?대룞/怨듦꺽 鍮꾪솢?깊솕, ?湲??좊땲硫붿씠???꾪솚
        private void Die()
        {
            deathHandled = true;
            StopMotion();
            isDashing = false;
            dashVelocity = Vector3.zero;
            AudioManager.Instance?.PlayPlayerSfx(PlayerSoundId.Death);
            AudioManager.Instance?.StopBgm();

            PlayerAttack attack = GetComponent<PlayerAttack>();
            if (attack != null)
                attack.enabled = false;

            PlayerClassSkillController classSkillController = GetComponent<PlayerClassSkillController>();
            if (classSkillController != null)
                classSkillController.enabled = false;

            if (deathRoutine != null)
                StopCoroutine(deathRoutine);
            deathRoutine = StartCoroutine(PlayDeathThenShowGameOver());

            Debug.Log("[Player] HP媛 0???섏뼱 ?щ쭩?덉뒿?덈떎.");
        }

        private IEnumerator PlayDeathThenShowGameOver()
        {
            Sprite[] deathFrames = ResolveDeathAnimationFrames();
            if (deathFrames.Length > 0)
            {
                isPlayingDeathAnimation = true;
                float frameDuration = 1f / Mathf.Max(1f, deathFrameRate);
                for (int i = 0; i < deathFrames.Length; i++)
                {
                    if (spriteRenderer != null && deathFrames[i] != null)
                    {
                        spriteRenderer.sprite = deathFrames[i];
                    }

                    yield return new WaitForSeconds(frameDuration);
                }
            }
            else
            {
                SetAnimation(idleSprites, idleFrameRate);
            }

            isPlayingDeathAnimation = false;
            EnsureDeathScreen().ShowDeath();
            OnPlayerDied?.Invoke();
            deathRoutine = null;
        }

        private Sprite[] ResolveDeathAnimationFrames()
        {
            if (deathSprites == null || deathSprites.Length == 0)
            {
                return System.Array.Empty<Sprite>();
            }

            Sprite selectedSheet = SelectDirectionalDeathSheet();
            if (TryCreateFramesFromDeathSheet(selectedSheet, out Sprite[] sheetFrames))
            {
                return sheetFrames;
            }

            List<Sprite> frames = new List<Sprite>(deathSprites.Length);
            for (int i = 0; i < deathSprites.Length; i++)
            {
                if (deathSprites[i] != null)
                {
                    frames.Add(deathSprites[i]);
                }
            }

            return frames.ToArray();
        }

        private Sprite SelectDirectionalDeathSheet()
        {
            if (deathSprites.Length >= 2
                && IsHorizontalDeathSheet(deathSprites[0])
                && IsHorizontalDeathSheet(deathSprites[1]))
            {
                Vector3 facing = GetLogicalFacingDirection();
                return facing.z > 0.1f ? deathSprites[1] : deathSprites[0];
            }

            return deathSprites[0];
        }

        private static bool TryCreateFramesFromDeathSheet(Sprite sheetSprite, out Sprite[] frames)
        {
            frames = System.Array.Empty<Sprite>();
            if (!IsHorizontalDeathSheet(sheetSprite) || sheetSprite.texture == null)
            {
                return false;
            }

            Rect sheetRect = sheetSprite.rect;
            int frameSize = Mathf.RoundToInt(sheetRect.height);
            int frameCount = Mathf.RoundToInt(sheetRect.width) / frameSize;
            if (frameCount <= 1)
            {
                return false;
            }

            frames = new Sprite[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                Rect frameRect = new Rect(
                    sheetRect.x + frameSize * i,
                    sheetRect.y,
                    frameSize,
                    frameSize);

                frames[i] = Sprite.Create(
                    sheetSprite.texture,
                    frameRect,
                    new Vector2(0.5f, 0.5f),
                    sheetSprite.pixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect);
                frames[i].name = $"{sheetSprite.name}_{i}";
            }

            return true;
        }

        private static bool IsHorizontalDeathSheet(Sprite sprite)
        {
            if (sprite == null)
            {
                return false;
            }

            int width = Mathf.RoundToInt(sprite.rect.width);
            int height = Mathf.RoundToInt(sprite.rect.height);
            return height > 0 && width >= height * 2 && width % height == 0;
        }
    }
}
