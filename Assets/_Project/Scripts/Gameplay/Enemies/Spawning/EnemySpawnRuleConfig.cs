using UnityEngine;
using System.Collections.Generic;
namespace Necrocis
{
    [System.Serializable]
    public class EnemySpawnRuleConfig
    {
        public EnemySpawnRuleConfig CreateRuntimeCopy() => (EnemySpawnRuleConfig)MemberwiseClone();

        [Tooltip("체력·공격력·이동속도·경험치·접촉 피해의 원본입니다. 배치/AI 값은 이 스폰 규칙에 둡니다.")]
        public MonsterDefinition monsterDefinition;
        public string name = "Enemy";
        [Tooltip("Player receiving regions only; movement and contact retain colliderSize/center.")]
        public EnemyReceivingProfile receivingProfile;

        [Header("Poisson")]
        public float density = 0.0025f;
        public float minDistance = 8f;
        public int poissonSalt = 400;

        [Tooltip("비워두면 모든 지역에서 허용")]
        public List<int> allowedRegions = new List<int>();

        [Header("Spawner")]
        public int maxAlive = 1;
        public float activationRadius = 20f;
        public float respawnCooldown = 8f;
        public float spawnRadius = 1.5f;

        [Header("Movement")]
        public float moveSpeed = 1.5f;
        public float stoppingDistance = 0.1f;
        public float wanderRadius = 4f;
        public float chaseRadius = 6f;
        public float leashRadius = 8f;
        public Vector2 idleDelayRange = new Vector2(0.5f, 1.5f);

        [Header("Combat")]
        public float maxHealth = 30f;
        public float attackDamage = 1f;
        public float attackRange = 1.5f;
        public float attackCooldown = 1f;
        public int expReward = 10;

        [Header("Contact Damage")]
        public bool enableContactDamage = true;
        [Min(0f)] public float contactDamage = 1f;
        [Min(0f)] public float contactKnockbackDistance = 0.45f;

        [Header("Additional Stats")]
        public List<CharacterStatValue> additionalBaseStats = new List<CharacterStatValue>();

        [Header("Separation")]
        public float separationDistance = 1.1f;
        public float separationStrength = 1f;

        [Header("Visual")]
        public float heightOffset = 0f;
        public Vector3 scale = Vector3.one;
        public int sortingOrder = 1000;
        public bool useBillboard = true;
        public bool useYSort = true;
        public float animationSpeed = 0.15f;

        [Header("Physics")]
        public bool addCollider = true;
        public bool isTrigger = false;
        public Vector3 colliderSize = new Vector3(0.7f, 1.1f, 0.7f);
        public Vector3 colliderCenter = new Vector3(0f, 0.55f, 0f);

        [Header("Sprites - Idle / Move")]
        public Sprite[] idleSprites;           // 기본 방향 / 좌우는 flipX로 처리
        public Sprite[] idleSpritesUp;         // 상방 대기
        public Sprite[] idleSpritesDown;       // 하방 대기
        public Sprite[] moveSprites;           // 기본 방향 / 좌우는 flipX로 처리
        public Sprite[] moveSpritesUp;         // 상방 이동
        public Sprite[] moveSpritesDown;       // 하방 이동

        [Header("Sprites - Attack")]
        public Sprite[] attackSprites;         // 기본 공격 (좌우는 flipX로 처리)
        public Sprite[] attackSpritesUp;       // 상방 공격 (NK세포 등 방향별 공격용)
        public Sprite[] attackSpritesDown;     // 하방 공격
        public float attackAnimationSpeed = 0.12f;

        [Header("Ranged Attack (원거리 공격)")]
        public bool isRanged = false;
        public float projectileSpeed = 8f;
        public float projectileLifeTime = 3f;
        public Sprite projectileSprite;
        public Vector3 projectileScale = new Vector3(0.4f, 0.4f, 0.4f);
        public float projectileSpawnOffset = 0.5f;

        [Header("Attack Collider (대식세포 등 공격 시 콜라이더 확장)")]
        public bool expandColliderOnAttack = false;
        public Vector3 attackColliderSize = new Vector3(2f, 2f, 2f);
        public Vector3 attackColliderCenter = new Vector3(0f, 0.55f, 0f);

        [Header("Sprites - Death")]
        public Sprite[] deathSprites;
        public float deathAnimationSpeed = 0.15f;

        [Header("Elite")]
        public bool isElite = false;
        public Color tintColor = Color.white;

        [Tooltip("이 엘리트를 소환하기 위해 잡아야 하는 일반 적 이름")]
        public string killTriggerEnemyName = "";
        [Tooltip("소환에 필요한 킬 수")]
        public int killTriggerCount = 10;

        [Header("Elite - Split on Death (육아종)")]
        public bool splitsOnDeath = false;
        public int splitCount = 2;
        public string splitEnemyName = "";

        [Header("Elite - Split VFX (분열 이펙트)")]
        public Sprite[] splitVfxSprites;
        public float splitVfxScale = 3f;
        public float splitVfxSpeed = 0.08f;
        public float splitVfxDuration = 0.6f;

        [Header("Elite - Charge (항체)")]
        public bool chargesAtPlayer = false;
        public float chargeSpeed = 6f;
        public float chargeAccelTime = 0.3f;

        [Header("Elite - Aggro Debris (항체 잔해)")]
        public bool leavesDebrisOnDeath = false;
        public float debrisDuration = 5f;
        public float debrisAggroRadius = 8f;

        [Header("Elite - Debris VFX (충격파 이펙트)")]
        public Sprite[] debrisVfxSprites;
        public float debrisVfxScale = 15f;
        public float debrisVfxSpeed = 0.12f;
    }

}
