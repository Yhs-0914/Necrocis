using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Necrocis
{
    public class PlayerController : MonoBehaviour
    {
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

        [Header("Animation Settings")]
        [SerializeField] private float idleFrameRate = 4f;
        [SerializeField] private float walkFrameRate = 8f;
        [SerializeField] private float attackAnimDuration = 0.3f;
        [SerializeField] private float attackFrameRate = 12f;

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
        private PlayerStats playerStats;           // ?ㅽ꺈 而댄룷?뚰듃 李몄“
        private bool playerStatsConfigured;        // 湲곕낯 ?ㅽ꺈 ?ㅼ젙 ?꾨즺 ?щ?
        private bool playerStatsEventsBound;       // HP 蹂寃??대깽??援щ룆 ?щ?
        private bool deathHandled;                 // ?щ쭩 泥섎━ ?꾨즺 ?щ? (以묐났 諛⑹?)

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
            lastMoveDirection = DirectionToVector(currentDirection);
            ApplyLockedRotation();
        }

        private void OnDestroy()
        {
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

            Debug.Log($"[Player] ?쒖옉 ?꾩튂: {transform.position}");
        }
        // ?좊땲???앸챸二쇨린: 留??꾨젅??寃뚯엫?뚮젅??濡쒖쭅???ㅽ뻾?⑸땲??

        private void Update()
        {
            HandleInput();
            UpdateAnimation();
            ApplyLockedRotation();
        }
        // ?좊땲???앸챸二쇨린: 臾쇰━ ?ㅽ뀦 湲곕컲 濡쒖쭅???ㅽ뻾?⑸땲??

        private void FixedUpdate()
        {
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

            if (deathHandled)
            {
                movement = Vector3.zero;
                isMoving = false;
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

        /// <summary>
        /// ?ㅽ룿 ?꾩튂濡??대룞
        /// </summary>
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

            float duration = Mathf.Max(0.3f, attackAnimDuration);
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

        private void OnEnable()
        {
            LevelUpManager.OnJobChanged += HandleJobChanged;
        }

        private void OnDisable()
        {
            LevelUpManager.OnJobChanged -= HandleJobChanged;
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
            if (args.CurrentValue < args.PreviousValue)
            {
                float damageTaken = args.PreviousValue - args.CurrentValue;
                Debug.Log($"[Player] ?쇳빐 {damageTaken} 諛쏆쓬 | HP {args.CurrentValue}/{args.MaxValue}");
            }
            else if (args.CurrentValue > args.PreviousValue)
            {
                float healed = args.CurrentValue - args.PreviousValue;
                Debug.Log($"[Player] ?뚮났 {healed} | HP {args.CurrentValue}/{args.MaxValue}");
            }

            if (!deathHandled && args.CurrentValue <= 0f)
            {
                Die();
            }
        }
        // Die: ??而댄룷?뚰듃???듭떖 濡쒖쭅???ㅽ뻾?⑸땲??

        // ?щ쭩 泥섎━: ?대룞/怨듦꺽 鍮꾪솢?깊솕, ?湲??좊땲硫붿씠???꾪솚
        private void Die()
        {
            deathHandled = true;
            movement = Vector3.zero;
            isMoving = false;
            AudioManager.Instance?.PlayPlayerSfx(PlayerSoundId.Death);
            AudioManager.Instance?.StopBgm();

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
            Debug.Log("[Player] HP媛 0???섏뼱 ?щ쭩?덉뒿?덈떎.");
        }
    }
}
