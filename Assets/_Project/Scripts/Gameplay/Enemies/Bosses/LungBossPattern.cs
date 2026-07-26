using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [System.Serializable]
    public class LungBossPatternSettings
    {
        [Header("Phase 1")]
        public float phase1BrotherHealth = 40f;
        public float phase1AttackDamage = 2f;
        public float phase1MoveSpeed = 1f;
        public float windGustMoveSpeed = 3f;
        public float windGustInterval = 5f;
        public float windGustDuration = 1.25f;
        public float brotherSpacing = 3.2f;

        [Header("Phase 1 Gas")]
        public float phase1GasRange = 7f;
        public float phase1GasCooldown = 3.5f;
        public float phase1GasKnockback = 1f;

        [Header("Phase 2")]
        public float phase2MaxHealth = 100f;
        public float phase2AttackDamage = 4f;
        public float phase2MoveSpeed = 1.5f;
        public float phase2GasRange = 10f;
        public float phase2GasCooldown = 4f;
        public float phase2GasKnockback = 1.2f;

        [Header("Dive")]
        public float diveCooldown = 8f;
        public float diveWindup = 0.55f;
        public float diveTravelTime = 0.65f;
        public float diveArcHeight = 4f;
        public float diveLandingRadius = 1.45f;
        public float diveLandingDamage = 3f;
        public float diveKnockback = 1.4f;

        [Header("Gas Projectile")]
        public float gasProjectileSpeed = 8f;
        public float gasProjectileRadius = 0.7f;
        public float gasProjectileScale = 1f;
        public float gasWindup = 0.25f;
        public float gasProjectileHeightOffset = 2f;

        [Header("Contact")]
        public float phase1ContactDamage = 1f;
        public float phase2ContactDamage = 2f;
        public float contactRadius = 1.05f;
        public float contactCooldown = 1.1f;

        [Header("Movement")]
        public float phase1PreferredDistance = 5.2f;
        public float phase2PreferredDistance = 4.2f;
        public float roamRadius = 10f;

        [Header("Temporary Visuals")]
        public Color gasColor = new Color(0.72f, 0.95f, 0.9f, 0.72f);
        public Color windColor = new Color(0.78f, 0.9f, 1f, 0.34f);
        public Color diveColor = new Color(0.5f, 0.85f, 1f, 0.42f);

        [Header("Debug")]
        public bool startInPhase2ForDebug = false;
        public bool useFastPatternCooldownsForDebug = false;
        public float fastPatternCooldown = 1f;
    }

    [DisallowMultipleComponent]
    public class LungBossPattern : MonoBehaviour, IBossPatternTempSpriteOwner
    {
        private enum BossPhase
        {
            Phase1,
            Transition,
            Phase2,
            Defeated
        }

        [Header("Phase 1")]
        [SerializeField] private float phase1BrotherHealth = 40f;
        [SerializeField] private float phase1AttackDamage = 2f;
        [SerializeField] private float phase1MoveSpeed = 1f;
        [SerializeField] private float windGustMoveSpeed = 3f;
        [SerializeField] private float windGustInterval = 5f;
        [SerializeField] private float windGustDuration = 1.25f;
        [SerializeField] private float brotherSpacing = 3.2f;

        [Header("Phase 1 Gas")]
        [SerializeField] private float phase1GasRange = 7f;
        [SerializeField] private float phase1GasCooldown = 3.5f;
        [SerializeField] private float phase1GasKnockback = 1f;

        [Header("Phase 2")]
        [SerializeField] private float phase2MaxHealth = 100f;
        [SerializeField] private float phase2AttackDamage = 4f;
        [SerializeField] private float phase2MoveSpeed = 1.5f;
        [SerializeField] private float phase2GasRange = 10f;
        [SerializeField] private float phase2GasCooldown = 4f;
        [SerializeField] private float phase2GasKnockback = 1.2f;

        [Header("Dive")]
        [SerializeField] private float diveCooldown = 8f;
        [SerializeField] private float diveWindup = 0.55f;
        [SerializeField] private float diveTravelTime = 0.65f;
        [SerializeField] private float diveArcHeight = 4f;
        [SerializeField] private float diveLandingRadius = 1.45f;
        [SerializeField] private float diveLandingDamage = 3f;
        [SerializeField] private float diveKnockback = 1.4f;

        [Header("Gas Projectile")]
        [SerializeField] private float gasProjectileSpeed = 8f;
        [SerializeField] private float gasProjectileRadius = 0.7f;
        [SerializeField] private float gasProjectileScale = 1f;
        [SerializeField] private float gasWindup = 0.25f;
        [SerializeField] private float gasProjectileHeightOffset = 2f;

        [Header("Contact")]
        [SerializeField] private float phase1ContactDamage = 1f;
        [SerializeField] private float phase2ContactDamage = 2f;
        [SerializeField] private float contactRadius = 1.05f;
        [SerializeField] private float contactCooldown = 1.1f;

        [Header("Movement")]
        [SerializeField] private float phase1PreferredDistance = 5.2f;
        [SerializeField] private float phase2PreferredDistance = 4.2f;
        [SerializeField] private float roamRadius = 10f;

        [Header("Temporary Visuals")]
        [SerializeField] private Color gasColor = new Color(0.72f, 0.95f, 0.9f, 0.72f);
        [SerializeField] private Color windColor = new Color(0.78f, 0.9f, 1f, 0.34f);
        [SerializeField] private Color diveColor = new Color(0.5f, 0.85f, 1f, 0.42f);

        [Header("Debug")]
        [SerializeField] private bool startInPhase2ForDebug = false;
        [SerializeField] private bool useFastPatternCooldownsForDebug = false;
        [SerializeField] private float fastPatternCooldown = 1f;

        private static Sprite gasSprite;
        private static Sprite ringSprite;

        private readonly EnemyController[] brothers = new EnemyController[2];
        private readonly CharacterStats[] brotherStats = new CharacterStats[2];
        private readonly SpriteRenderer[] brotherRenderers = new SpriteRenderer[2];
        private readonly bool[] gasRunning = new bool[2];
        private readonly float[] nextGasTime = new float[2];
        private readonly float[] nextContactTime = new float[2];

        private Transform encounterParent;
        private Vector3 anchorPosition;
        private Vector3 baseScale = Vector3.one;
        private BossPhase phase;
        private EnemyController survivor;
        private float nextWindGustTime;
        private float windGustEndTime;
        private float nextPhase2GasTime;
        private float nextDiveTime;
        private bool phase2ActionRunning;
        private bool encounterDefeated;
        private bool encounterActive;
        private Vector3 lastDefeatedPosition;
        private readonly List<GameObject> activeTempObjects = new List<GameObject>();

        public bool IsEncounterDefeated => encounterDefeated;
        public Vector3 LastDefeatedPosition => lastDefeatedPosition;
        public string CurrentPhaseName => phase.ToString();

        public void Initialize(EnemyController controller, Vector3 anchor, Transform parent, LungBossPatternSettings settings = null)
        {
            ApplySettings(settings);

            encounterParent = parent != null ? parent : transform.parent;
            anchorPosition = anchor;
            baseScale = transform.localScale;
            phase = startInPhase2ForDebug ? BossPhase.Phase2 : BossPhase.Phase1;
            encounterDefeated = false;
            encounterActive = false;
            phase2ActionRunning = false;
            lastDefeatedPosition = anchor;
            nextWindGustTime = Time.time + 1.2f;
            windGustEndTime = float.NegativeInfinity;
            nextGasTime[0] = Time.time + 0.8f;
            nextGasTime[1] = Time.time + 2.2f;
            nextContactTime[0] = Time.time + 1f;
            nextContactTime[1] = Time.time + 1f;
            nextPhase2GasTime = Time.time + 1.2f;
            nextDiveTime = Time.time + 3f;

            brothers[0] = controller;
            SpawnSibling();

            PositionBrothers();
            ConfigureBrother(0);
            ConfigureBrother(1);

            if (phase == BossPhase.Phase2)
            {
                survivor = brothers[0];
                EnterPhase2(survivor);
            }

            enabled = true;
        }

        private void ApplySettings(LungBossPatternSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            phase1BrotherHealth = settings.phase1BrotherHealth;
            phase1AttackDamage = settings.phase1AttackDamage;
            phase1MoveSpeed = settings.phase1MoveSpeed;
            windGustMoveSpeed = settings.windGustMoveSpeed;
            windGustInterval = settings.windGustInterval;
            windGustDuration = settings.windGustDuration;
            brotherSpacing = settings.brotherSpacing;
            phase1GasRange = settings.phase1GasRange;
            phase1GasCooldown = settings.phase1GasCooldown;
            phase1GasKnockback = settings.phase1GasKnockback;
            phase2MaxHealth = settings.phase2MaxHealth;
            phase2AttackDamage = settings.phase2AttackDamage;
            phase2MoveSpeed = settings.phase2MoveSpeed;
            phase2GasRange = settings.phase2GasRange;
            phase2GasCooldown = settings.phase2GasCooldown;
            phase2GasKnockback = settings.phase2GasKnockback;
            diveCooldown = settings.diveCooldown;
            diveWindup = settings.diveWindup;
            diveTravelTime = settings.diveTravelTime;
            diveArcHeight = settings.diveArcHeight;
            diveLandingRadius = settings.diveLandingRadius;
            diveLandingDamage = settings.diveLandingDamage;
            diveKnockback = settings.diveKnockback;
            gasProjectileSpeed = settings.gasProjectileSpeed;
            gasProjectileRadius = settings.gasProjectileRadius;
            gasProjectileScale = settings.gasProjectileScale;
            gasWindup = settings.gasWindup;
            gasProjectileHeightOffset = settings.gasProjectileHeightOffset;
            phase1ContactDamage = settings.phase1ContactDamage;
            phase2ContactDamage = settings.phase2ContactDamage;
            contactRadius = settings.contactRadius;
            contactCooldown = settings.contactCooldown;
            phase1PreferredDistance = settings.phase1PreferredDistance;
            phase2PreferredDistance = settings.phase2PreferredDistance;
            roamRadius = settings.roamRadius;
            gasColor = settings.gasColor;
            windColor = settings.windColor;
            diveColor = settings.diveColor;
            startInPhase2ForDebug = settings.startInPhase2ForDebug;
            useFastPatternCooldownsForDebug = settings.useFastPatternCooldownsForDebug;
            fastPatternCooldown = settings.fastPatternCooldown;
        }

        private void SpawnSibling()
        {
            if (brothers[0] == null || brothers[0].Config == null)
            {
                return;
            }

            Vector3 spawnPosition = GetGroundedPosition(anchorPosition + Vector3.right * Mathf.Max(1f, brotherSpacing) * 0.5f);
            int poolArchetypeId = EnemyController.GetPoolArchetypeId(brothers[0].Config);
            brothers[1] = EnemyController.Acquire(encounterParent, $"{brothers[0].Config.name}_Brother", poolArchetypeId);
            brothers[1].Configure(null, brothers[0].Config, spawnPosition, spawnPosition);
            brothers[1].transform.SetParent(encounterParent, true);
            brothers[1].SetIgnoreMidBossArenaRestriction(true);
        }

        private void PositionBrothers()
        {
            float halfSpacing = Mathf.Max(1f, brotherSpacing) * 0.5f;
            if (brothers[0] != null)
            {
                brothers[0].transform.position = GetGroundedPosition(anchorPosition + Vector3.left * halfSpacing);
            }

            if (brothers[1] != null)
            {
                brothers[1].transform.position = GetGroundedPosition(anchorPosition + Vector3.right * halfSpacing);
            }
        }

        private void ConfigureBrother(int index)
        {
            EnemyController brother = GetBrother(index);
            if (brother == null)
            {
                return;
            }

            brother.SetAiSuppressed(true);
            brother.SetIgnoreMidBossArenaRestriction(true);
            brother.Defeated -= HandleBrotherDefeated;
            brother.Defeated += HandleBrotherDefeated;

            brotherStats[index] = brother.Stats;
            if (brotherStats[index] != null)
            {
                brotherStats[index].SetBaseStat(CharacterStatType.MaxHealth, phase1BrotherHealth, true);
                brotherStats[index].SetBaseStat(CharacterStatType.AttackPower, phase1AttackDamage);
                brotherStats[index].SetBaseStat(CharacterStatType.MoveSpeed, phase1MoveSpeed);
            }

            brotherRenderers[index] = brother.GetComponentInChildren<SpriteRenderer>();
            if (brotherRenderers[index] != null)
            {
                brotherRenderers[index].color = Color.white;
            }
        }

        public void DisposeEncounter()
        {
            StopAllCoroutines();
            encounterActive = false;
            CleanupPatternObjects();
            UnbindBrothers();
            enabled = false;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            encounterActive = false;
            CleanupPatternObjects();
            UnbindBrothers();
        }

        public void SetEncounterActive(bool active)
        {
            if (encounterActive == active)
            {
                return;
            }

            encounterActive = active;
            StopAllCoroutines();
            CleanupPatternObjects();

            for (int i = 0; i < gasRunning.Length; i++)
            {
                gasRunning[i] = false;
            }

            phase2ActionRunning = false;

            if (active)
            {
                AudioManager.Instance?.PlaySFX("BossRoar");
                nextWindGustTime = Time.time + 1.2f;
                windGustEndTime = float.NegativeInfinity;
                nextGasTime[0] = Time.time + 0.8f;
                nextGasTime[1] = Time.time + 2.2f;
                nextContactTime[0] = Time.time + 1f;
                nextContactTime[1] = Time.time + 1f;
                nextPhase2GasTime = Time.time + 1.2f;
                nextDiveTime = Time.time + 3f;
            }
        }

        public void RecenterEncounter()
        {
            PositionBrothers();
        }

        public void ForEachEncounterBoss(System.Action<EnemyController> action)
        {
            if (action == null)
            {
                return;
            }

            for (int i = 0; i < brothers.Length; i++)
            {
                if (brothers[i] != null)
                {
                    action(brothers[i]);
                }
            }
        }

        private void UnbindBrothers()
        {
            for (int i = 0; i < brothers.Length; i++)
            {
                EnemyController brother = brothers[i];
                if (brother == null)
                {
                    continue;
                }

                brother.Defeated -= HandleBrotherDefeated;
                if (!brother.IsDead)
                {
                    brother.SetAiSuppressed(false);
                }
            }
        }

        private void Update()
        {
            if (!encounterActive)
            {
                return;
            }

            if (encounterDefeated)
            {
                return;
            }

            EvaluatePhaseState();
            if (encounterDefeated || PlayerController.Instance == null)
            {
                return;
            }

            if (phase == BossPhase.Phase1)
            {
                UpdateWindGust();
                for (int i = 0; i < brothers.Length; i++)
                {
                    EnemyController brother = GetBrother(i);
                    if (brother == null || brother.IsDead)
                    {
                        continue;
                    }

                    TryRunPhase1Gas(i, brother);
                    TryApplyContactDamage(i, brother, phase1ContactDamage);
                }
            }
            else if (phase == BossPhase.Phase2 && survivor != null && !survivor.IsDead)
            {
                TryApplyContactDamage(GetBrotherIndex(survivor), survivor, phase2ContactDamage);
                if (!phase2ActionRunning)
                {
                    if (Time.time >= nextDiveTime)
                    {
                        StartCoroutine(DiveRoutine(survivor));
                    }
                    else if (Time.time >= nextPhase2GasTime)
                    {
                        StartCoroutine(DoubleGasRoutine(survivor));
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (!encounterActive || encounterDefeated || PlayerController.Instance == null)
            {
                return;
            }

            if (phase == BossPhase.Phase1)
            {
                for (int i = 0; i < brothers.Length; i++)
                {
                    EnemyController brother = GetBrother(i);
                    if (brother == null || brother.IsDead || gasRunning[i])
                    {
                        continue;
                    }

                    MoveBrotherPhase1(i, brother);
                }
            }
            else if (phase == BossPhase.Phase2 && survivor != null && !survivor.IsDead && !phase2ActionRunning)
            {
                MoveSurvivorPhase2(survivor);
            }
        }

        private void EvaluatePhaseState()
        {
            bool firstAlive = IsAlive(brothers[0]);
            bool secondAlive = IsAlive(brothers[1]);

            if (!firstAlive && !secondAlive)
            {
                encounterDefeated = true;
                phase = BossPhase.Defeated;
                return;
            }

            if (phase == BossPhase.Phase1 && firstAlive != secondAlive)
            {
                EnterPhase2(firstAlive ? brothers[0] : brothers[1]);
            }
            else if (phase == BossPhase.Phase2 && survivor != null && survivor.IsDead)
            {
                encounterDefeated = true;
                phase = BossPhase.Defeated;
            }
        }

        private void EnterPhase2(EnemyController newSurvivor)
        {
            StopAllCoroutines();
            CleanupPatternObjects();
            for (int i = 0; i < gasRunning.Length; i++)
            {
                gasRunning[i] = false;
            }

            survivor = newSurvivor;
            phase = BossPhase.Phase2;
            AudioManager.Instance?.PlaySFX("LungPhase2");
            phase2ActionRunning = false;
            nextPhase2GasTime = Time.time + 1f;
            nextDiveTime = Time.time + 2.5f;

            int survivorIndex = GetBrotherIndex(survivor);
            if (survivorIndex >= 0 && brotherStats[survivorIndex] != null)
            {
                brotherStats[survivorIndex].SetBaseStat(CharacterStatType.MaxHealth, phase2MaxHealth, true);
                brotherStats[survivorIndex].SetBaseStat(CharacterStatType.AttackPower, phase2AttackDamage);
                brotherStats[survivorIndex].SetBaseStat(CharacterStatType.MoveSpeed, phase2MoveSpeed);
            }

            SpriteRenderer renderer = survivor != null ? survivor.GetComponentInChildren<SpriteRenderer>() : null;
            if (renderer != null)
            {
                renderer.color = Color.white;
            }

            if (survivor != null)
            {
                Vector3 center = survivor.transform.position;
                center.y = GetGroundHeight(center) + 0.08f;
                GameObject ring = CreateTempSpriteObject("LungBoss_EnrageWind", GetRingSprite(), windColor, center, 1.2f, 4900);
                StartCoroutine(FadeAndDestroy(ring, 0.75f, windColor, 4f));
            }
        }

        private void HandleBrotherDefeated(EnemyController defeated)
        {
            if (defeated != null)
            {
                lastDefeatedPosition = defeated.transform.position;
            }

            EvaluatePhaseState();
            if (encounterDefeated)
            {
                StopAllCoroutines();
                phase2ActionRunning = false;
                for (int i = 0; i < gasRunning.Length; i++)
                {
                    gasRunning[i] = false;
                }

                CleanupPatternObjects();
            }
        }

        private void UpdateWindGust()
        {
            if (Time.time < nextWindGustTime)
            {
                return;
            }

            windGustEndTime = Time.time + Mathf.Max(0.1f, windGustDuration);
            nextWindGustTime = Time.time + Mathf.Max(0.2f, windGustInterval);
            AudioManager.Instance?.PlaySFX("LungHighSpeed");
            Vector3 center = anchorPosition;
            center.y = GetGroundHeight(center) + 0.08f;
            GameObject wind = CreateTempSpriteObject("LungBoss_WindGust", GetRingSprite(), windColor, center, 2.4f, 2400);
            StartCoroutine(FadeAndDestroy(wind, Mathf.Max(0.1f, windGustDuration), windColor, 7f));
        }

        private void TryRunPhase1Gas(int index, EnemyController shooter)
        {
            if (gasRunning[index] || Time.time < nextGasTime[index] || !IsPlayerWithinRange(shooter.transform.position, phase1GasRange))
            {
                return;
            }

            gasRunning[index] = true;
            nextGasTime[index] = Time.time + GetPhase1GasCooldown();
            StartCoroutine(GasShotRoutine(
                shooter,
                GetDirectionToPlayer(shooter.transform.position),
                phase1GasRange,
                phase1AttackDamage,
                phase1GasKnockback,
                () => gasRunning[index] = false));
        }

        private IEnumerator DiveRoutine(EnemyController actor)
        {
            phase2ActionRunning = true;
            nextDiveTime = Time.time + GetDiveCooldown();
            AudioManager.Instance?.PlaySFX("LungAccelerate");

            float windup = Mathf.Max(0f, diveWindup);
            float elapsed = 0f;
            Vector3 actorBaseScale = actor.transform.localScale;
            while (elapsed < windup)
            {
                elapsed += Time.deltaTime;
                float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.08f;
                actor.transform.localScale = actorBaseScale * pulse;
                yield return null;
            }

            Vector3 start = actor.transform.position;
            Vector3 target = PlayerController.Instance != null ? PlayerController.Instance.transform.position : anchorPosition;
            target.y = GetGroundHeight(target);
            elapsed = 0f;
            float travelTime = Mathf.Max(0.05f, diveTravelTime);
            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / travelTime);
                Vector3 position = Vector3.Lerp(start, target, t);
                position.y = Mathf.Lerp(start.y, target.y, t) + Mathf.Sin(t * Mathf.PI) * diveArcHeight;
                actor.transform.position = position;
                yield return null;
            }

            actor.transform.position = GetGroundedPosition(target);
            actor.transform.localScale = actorBaseScale;

            Vector3 landing = actor.transform.position;
            landing.y = GetGroundHeight(landing) + 0.08f;
            GameObject ring = CreateTempSpriteObject("LungBoss_DiveLanding", GetRingSprite(), diveColor, landing, diveLandingRadius * 1.4f, 4900);
            StartCoroutine(FadeAndDestroy(ring, 0.35f, diveColor, diveLandingRadius * 2.4f));

            PlayerController player = PlayerController.Instance;
            if (IsPlayerWithinRange(landing, diveLandingRadius))
            {
                player.TakeDamage(diveLandingDamage);
                ApplyPlayerKnockback(player, landing, diveKnockback);
            }

            phase2ActionRunning = false;
        }

        private IEnumerator DoubleGasRoutine(EnemyController shooter)
        {
            phase2ActionRunning = true;
            nextPhase2GasTime = Time.time + GetPhase2GasCooldown();

            Vector3 forward = GetDirectionToPlayer(shooter.transform.position);
            yield return GasWindup(shooter);

            AudioManager.Instance?.PlaySFX("LungPhase2Skill");
            GameObject first = CreateGasProjectile(shooter.transform.position, forward);
            GameObject second = CreateGasProjectile(shooter.transform.position, -forward);
            yield return MoveGasProjectiles(
                first,
                second,
                shooter.transform.position,
                forward,
                phase2GasRange,
                phase2AttackDamage,
                phase2GasKnockback);

            phase2ActionRunning = false;
        }

        private IEnumerator GasShotRoutine(
            EnemyController shooter,
            Vector3 direction,
            float range,
            float damage,
            float knockback,
            System.Action onComplete)
        {
            yield return GasWindup(shooter);
            GameObject projectile = CreateGasProjectile(shooter.transform.position, direction);
            yield return MoveGasProjectiles(projectile, null, shooter.transform.position, direction, range, damage, knockback);
            onComplete?.Invoke();
        }

        private IEnumerator GasWindup(EnemyController shooter)
        {
            if (shooter == null)
            {
                yield break;
            }

            Vector3 startScale = shooter.transform.localScale;
            shooter.PlayAttackAnimationOnly();
            float elapsed = 0f;
            float duration = Mathf.Max(0f, gasWindup);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                shooter.transform.localScale = new Vector3(startScale.x * (1f + t * 0.08f), startScale.y * (1f - t * 0.05f), startScale.z);
                yield return null;
            }

            shooter.transform.localScale = startScale;
        }

        private IEnumerator MoveGasProjectiles(
            GameObject first,
            GameObject second,
            Vector3 origin,
            Vector3 direction,
            float range,
            float damage,
            float knockback)
        {
            float distance = 0f;
            float maxRange = Mathf.Max(0.1f, range);
            float speed = Mathf.Max(0.1f, gasProjectileSpeed);
            bool hitPlayer = false;

            while (distance < maxRange && (first != null || second != null))
            {
                float step = speed * Time.deltaTime;
                distance += step;

                MoveGasProjectile(first, direction, step);
                MoveGasProjectile(second, -direction, step);

                PlayerController player = PlayerController.Instance;
                if (!hitPlayer && player != null)
                {
                    if (IsPlayerNearObject(player, first) || IsPlayerNearObject(player, second))
                    {
                        hitPlayer = true;
                        player.TakeDamage(damage);
                        ApplyPlayerKnockback(player, origin, knockback);
                    }
                }

                yield return null;
            }

            if (first != null)
            {
                ReleaseTempSprite(first);
            }

            if (second != null)
            {
                ReleaseTempSprite(second);
            }
        }

        private void MoveBrotherPhase1(int index, EnemyController brother)
        {
            Vector3 direction = GetPhase1MoveDirection(index, brother.transform.position);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float speed = IsWindGustActive() ? windGustMoveSpeed : phase1MoveSpeed;
            brother.MoveByExternalPattern(direction.normalized * Mathf.Max(0f, speed) * Time.deltaTime);
        }

        private void MoveSurvivorPhase2(EnemyController actor)
        {
            Vector3 direction = GetPhase2MoveDirection(actor.transform.position);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            actor.MoveByExternalPattern(direction.normalized * Mathf.Max(0f, phase2MoveSpeed) * Time.deltaTime);
        }

        private Vector3 GetPhase1MoveDirection(int index, Vector3 position)
        {
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return Vector3.zero;
            }

            Vector3 fromAnchor = position - anchorPosition;
            fromAnchor.y = 0f;
            if (fromAnchor.magnitude > roamRadius)
            {
                Vector3 toAnchor = anchorPosition - position;
                toAnchor.y = 0f;
                return toAnchor;
            }

            Vector3 toPlayer = player.transform.position - position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            if (distance <= 0.0001f)
            {
                return Vector3.zero;
            }

            if (distance > phase1GasRange * 0.95f)
            {
                return toPlayer;
            }

            if (distance < phase1PreferredDistance * 0.7f)
            {
                return -toPlayer;
            }

            Vector3 tangent = Vector3.Cross(Vector3.up, toPlayer);
            return index == 0 ? tangent : -tangent;
        }

        private Vector3 GetPhase2MoveDirection(Vector3 position)
        {
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return Vector3.zero;
            }

            Vector3 toPlayer = player.transform.position - position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            if (distance <= 0.0001f)
            {
                return Vector3.zero;
            }

            if (distance > phase2GasRange * 0.9f)
            {
                return toPlayer;
            }

            if (distance < phase2PreferredDistance * 0.7f)
            {
                return -toPlayer;
            }

            return Vector3.zero;
        }

        private void TryApplyContactDamage(int index, EnemyController actor, float damage)
        {
            if (index < 0 || index >= nextContactTime.Length || Time.time < nextContactTime[index])
            {
                return;
            }

            PlayerController player = PlayerController.Instance;
            if (player == null || !IsPlayerWithinRange(actor.transform.position, contactRadius))
            {
                return;
            }

            nextContactTime[index] = Time.time + Mathf.Max(0.1f, contactCooldown);
            player.TakeDamage(damage);
        }

        private GameObject CreateGasProjectile(Vector3 shooterPosition, Vector3 direction)
        {
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Vector3 position = shooterPosition + direction * 0.65f;
            position.y = GetGroundHeight(position) + Mathf.Max(0f, gasProjectileHeightOffset);
            return CreateTempSpriteObject("LungBoss_GasShot", GetGasSprite(), gasColor, position, gasProjectileScale, 5000);
        }

        private void MoveGasProjectile(GameObject projectile, Vector3 direction, float step)
        {
            if (projectile == null)
            {
                return;
            }

            direction.y = 0f;
            projectile.transform.position += direction.normalized * step;
        }

        private bool IsPlayerNearObject(PlayerController player, GameObject obj)
        {
            if (player == null || obj == null)
            {
                return false;
            }

            Vector3 toPlayer = player.transform.position - obj.transform.position;
            toPlayer.y = 0f;
            return toPlayer.sqrMagnitude <= gasProjectileRadius * gasProjectileRadius;
        }

        private bool IsPlayerWithinRange(Vector3 center, float range)
        {
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return false;
            }

            Vector3 toPlayer = player.transform.position - center;
            toPlayer.y = 0f;
            float safeRange = Mathf.Max(0.1f, range);
            return toPlayer.sqrMagnitude <= safeRange * safeRange;
        }

        private Vector3 GetDirectionToPlayer(Vector3 from)
        {
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return Vector3.forward;
            }

            Vector3 direction = player.transform.position - from;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private bool IsWindGustActive()
        {
            return Time.time < windGustEndTime;
        }

        private float GetPhase1GasCooldown()
        {
            return useFastPatternCooldownsForDebug
                ? Mathf.Max(0.05f, fastPatternCooldown)
                : Mathf.Max(0.05f, phase1GasCooldown);
        }

        private float GetPhase2GasCooldown()
        {
            return useFastPatternCooldownsForDebug
                ? Mathf.Max(0.05f, fastPatternCooldown)
                : Mathf.Max(0.05f, phase2GasCooldown);
        }

        private float GetDiveCooldown()
        {
            return useFastPatternCooldownsForDebug
                ? Mathf.Max(0.05f, fastPatternCooldown)
                : Mathf.Max(0.05f, diveCooldown);
        }

        private EnemyController GetBrother(int index)
        {
            if (index < 0 || index >= brothers.Length)
            {
                return null;
            }

            return brothers[index];
        }

        private int GetBrotherIndex(EnemyController brother)
        {
            for (int i = 0; i < brothers.Length; i++)
            {
                if (brothers[i] == brother)
                {
                    return i;
                }
            }

            return 0;
        }

        private static bool IsAlive(EnemyController enemy)
        {
            return enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsDead;
        }

        private Vector3 GetGroundedPosition(Vector3 position)
        {
            position.y = GetGroundHeight(position);
            return position;
        }

        private float GetGroundHeight(Vector3 position)
        {
            BiomeManager biome = BiomeManager.Active;
            return biome != null ? biome.GetGroundHeight(position) : position.y;
        }

        private static void ApplyPlayerKnockback(PlayerController player, Vector3 sourcePosition, float distance)
        {
            if (player == null || distance <= 0f)
            {
                return;
            }

            Vector3 direction = player.transform.position - sourcePosition;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.forward;
            }

            Vector3 displacement = direction.normalized * distance;
            player.TryMoveByWorld(displacement);
        }

        private IEnumerator FadeAndDestroy(GameObject obj, float duration, Color startColor, float endScale)
        {
            if (obj == null)
            {
                yield break;
            }

            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            Vector3 startScale = obj.transform.localScale;
            Vector3 finalScale = Vector3.one * Mathf.Max(0.01f, endScale);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                obj.transform.localScale = Vector3.Lerp(startScale, finalScale, t);
                if (renderer != null)
                {
                    Color color = startColor;
                    color.a = Mathf.Lerp(startColor.a, 0f, t);
                    renderer.color = color;
                }

                yield return null;
            }

            ReleaseTempSprite(obj);
        }

        private GameObject CreateTempSpriteObject(string name, Sprite sprite, Color color, Vector3 position, float scale, int sortingOrder)
        {
            return BossPatternVisualPool.Acquire(name, sprite, color, position, scale, sortingOrder, this, activeTempObjects);
        }

        public void ReleaseTempSprite(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            activeTempObjects.Remove(obj);
            BossPatternVisualPool.Release(obj);
        }

        private void CleanupPatternObjects()
        {
            for (int i = 0; i < activeTempObjects.Count; i++)
            {
                if (activeTempObjects[i] != null)
                {
                    BossPatternVisualPool.Release(activeTempObjects[i]);
                }
            }

            activeTempObjects.Clear();
        }

        private static Sprite GetGasSprite()
        {
            if (gasSprite == null)
            {
                gasSprite = CreateCircleSprite("TempLungGasSprite", 48, 0.86f, true);
            }

            return gasSprite;
        }

        private static Sprite GetRingSprite()
        {
            if (ringSprite == null)
            {
                ringSprite = CreateCircleSprite("TempLungWindRingSprite", 64, 0.94f, false);
            }

            return ringSprite;
        }

        private static Sprite CreateCircleSprite(string name, int size, float radiusRatio, bool filled)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = TextureFilterMode(filled);
            float center = (size - 1) * 0.5f;
            float radius = center * radiusRatio;
            float innerRadius = radius * 0.7f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = 0f;

                    if (filled && distance <= radius)
                    {
                        alpha = Mathf.Clamp01(1f - distance / radius * 0.55f);
                    }
                    else if (!filled && distance <= radius && distance >= innerRadius)
                    {
                        float midpoint = (innerRadius + radius) * 0.5f;
                        alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - midpoint) / Mathf.Max(0.01f, radius - innerRadius));
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = name;
            return sprite;
        }

        private static FilterMode TextureFilterMode(bool filled)
        {
            return filled ? FilterMode.Bilinear : FilterMode.Bilinear;
        }
    }
}
