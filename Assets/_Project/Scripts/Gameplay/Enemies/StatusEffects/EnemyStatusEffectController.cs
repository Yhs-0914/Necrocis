using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class EnemyStatusEffectController : MonoBehaviour
    {
        private struct MoveSpeedSlow
        {
            public float ratio;
            public float endTime;
        }

        [SerializeField] private bool enableDebugLogs = true;

        private EnemyController enemy;

        private float stunEndTime;
        private float vulnerabilityEndTime;
        private float damageTakenIncreaseRatio;

        private Coroutine poisonCoroutine;
        private float poisonEndTime;
        private float poisonTickInterval;
        private float poisonTickDamage;

        private Coroutine bleedCoroutine;
        private float bleedEndTime;
        private float bleedTickInterval;
        private float bleedTickDamage;

        private float freezeVisualEndTime;
        private readonly List<MoveSpeedSlow> moveSpeedSlows = new List<MoveSpeedSlow>();
        private Coroutine slowRoutine;
        private EnemyStatusVisualOverlay statusVisualOverlay;

        public bool IsStunned => Time.time < stunEndTime;
        public bool IsPoisoned => Time.time < poisonEndTime;
        public bool IsBleeding => Time.time < bleedEndTime;
        public bool IsFrozenVisualActive => Time.time < freezeVisualEndTime;

        public void Initialize(EnemyController owner)
        {
            enemy = owner;
            ResolveStatusOverlay();
        }

        public void ResetEffects()
        {
            stunEndTime = 0f;
            vulnerabilityEndTime = 0f;
            damageTakenIncreaseRatio = 0f;

            poisonEndTime = 0f;
            poisonTickInterval = 0f;
            poisonTickDamage = 0f;

            bleedEndTime = 0f;
            bleedTickInterval = 0f;
            bleedTickDamage = 0f;

            freezeVisualEndTime = 0f;
            moveSpeedSlows.Clear();

            if (poisonCoroutine != null)
            {
                StopCoroutine(poisonCoroutine);
                poisonCoroutine = null;
            }

            if (bleedCoroutine != null)
            {
                StopCoroutine(bleedCoroutine);
                bleedCoroutine = null;
            }

            if (slowRoutine != null)
            {
                StopCoroutine(slowRoutine);
                slowRoutine = null;
            }

            if (enemy != null && enemy.Stats != null)
            {
                enemy.Stats.RemoveModifiersFromSource(this);
            }

            statusVisualOverlay?.Refresh(false, false, false);
        }

        public float GetIncomingDamageMultiplier()
        {
            if (damageTakenIncreaseRatio > 0f && Time.time >= vulnerabilityEndTime)
            {
                damageTakenIncreaseRatio = 0f;
                vulnerabilityEndTime = 0f;
            }

            return 1f + Mathf.Max(0f, damageTakenIncreaseRatio);
        }

        public void ApplyStun(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            float newEndTime = Time.time + duration;
            if (newEndTime > stunEndTime)
            {
                stunEndTime = newEndTime;
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[EnemyStatus] Stun applied to {EnemyName} for {duration:0.##}s");
            }
        }

        public void ApplyDamageTakenIncrease(float increaseRatio, float duration)
        {
            if (increaseRatio <= 0f || duration <= 0f)
            {
                return;
            }

            damageTakenIncreaseRatio = Mathf.Max(damageTakenIncreaseRatio, increaseRatio);

            float newEndTime = Time.time + duration;
            if (newEndTime > vulnerabilityEndTime)
            {
                vulnerabilityEndTime = newEndTime;
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[EnemyStatus] Damage taken increased on {EnemyName} by {increaseRatio * 100f:0.#}% for {duration:0.##}s");
            }
        }

        public void ApplyBleed(float duration, float tickInterval, float tickDamage)
        {
            if (duration <= 0f || tickInterval <= 0f || tickDamage <= 0f)
            {
                return;
            }

            bleedTickInterval = tickInterval;
            bleedTickDamage = tickDamage;
            bleedEndTime = Mathf.Max(bleedEndTime, Time.time + duration);

            if (bleedCoroutine == null)
            {
                bleedCoroutine = StartCoroutine(BleedRoutine());
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[EnemyStatus] Bleed applied to {EnemyName} | {duration:0.#}s / {tickDamage:0.#}dmg per {tickInterval:0.#}s");
            }
        }

        public void ApplyPoison(float duration, float tickInterval, float tickDamage)
        {
            if (duration <= 0f || tickInterval <= 0f || tickDamage <= 0f)
            {
                return;
            }

            poisonTickInterval = tickInterval;
            poisonTickDamage = tickDamage;
            poisonEndTime = Mathf.Max(poisonEndTime, Time.time + duration);

            if (poisonCoroutine == null)
            {
                poisonCoroutine = StartCoroutine(PoisonRoutine());
            }

            if (enableDebugLogs)
            {
                Debug.Log($"Poison applied to {EnemyName} for {duration:0.##}s");
            }
        }

        public void ApplyMoveSpeedSlow(float slowRatio, float duration)
        {
            if (slowRatio <= 0f || duration <= 0f || enemy == null || enemy.Stats == null)
            {
                return;
            }

            moveSpeedSlows.Add(new MoveSpeedSlow
            {
                ratio = Mathf.Clamp01(slowRatio),
                endTime = Time.time + duration
            });
            freezeVisualEndTime = Mathf.Max(freezeVisualEndTime, Time.time + duration);

            RefreshMoveSpeedSlow();

            if (slowRoutine == null)
            {
                slowRoutine = StartCoroutine(SlowRoutine());
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[EnemyStatus] Slow applied to {EnemyName} for {duration:0.##}s ({slowRatio * 100f:0.#}%)");
            }
        }

        private IEnumerator PoisonRoutine()
        {
            while (Time.time < poisonEndTime)
            {
                float interval = Mathf.Max(0.05f, poisonTickInterval);
                yield return new WaitForSeconds(interval);

                if (enemy == null || enemy.IsDead)
                {
                    break;
                }

                enemy.TakeDamage(poisonTickDamage);
            }

            poisonCoroutine = null;
            poisonEndTime = 0f;
        }

        private IEnumerator BleedRoutine()
        {
            while (Time.time < bleedEndTime)
            {
                float interval = Mathf.Max(0.05f, bleedTickInterval);
                yield return new WaitForSeconds(interval);

                if (enemy == null || enemy.IsDead)
                {
                    break;
                }

                enemy.TakeDamage(bleedTickDamage);
            }

            bleedCoroutine = null;
            bleedEndTime = 0f;
        }

        private IEnumerator SlowRoutine()
        {
            while (moveSpeedSlows.Count > 0)
            {
                float nextEndTime = float.PositiveInfinity;
                for (int i = 0; i < moveSpeedSlows.Count; i++)
                {
                    nextEndTime = Mathf.Min(nextEndTime, moveSpeedSlows[i].endTime);
                }

                float waitTime = Mathf.Max(0.02f, nextEndTime - Time.time);
                yield return new WaitForSeconds(waitTime);
                RefreshMoveSpeedSlow();
            }

            slowRoutine = null;
        }

        private void RefreshMoveSpeedSlow()
        {
            float now = Time.time;
            for (int i = moveSpeedSlows.Count - 1; i >= 0; i--)
            {
                if (moveSpeedSlows[i].endTime <= now)
                {
                    moveSpeedSlows.RemoveAt(i);
                }
            }

            if (enemy == null || enemy.Stats == null)
            {
                return;
            }

            enemy.Stats.RemoveModifiersFromSource(this);

            float strongestSlow = 0f;
            for (int i = 0; i < moveSpeedSlows.Count; i++)
            {
                strongestSlow = Mathf.Max(strongestSlow, moveSpeedSlows[i].ratio);
            }

            if (strongestSlow > 0f)
            {
                enemy.Stats.AddModifier(
                    CharacterStatType.MoveSpeed,
                    -strongestSlow,
                    CharacterStatModifierMode.PercentAdd,
                    this);
            }
        }

        private void Update()
        {
            ResolveStatusOverlay();
            statusVisualOverlay?.Refresh(IsPoisoned, IsFrozenVisualActive, IsBleeding);
        }

        private void OnDisable()
        {
            ResetEffects();
        }

        private void ResolveStatusOverlay()
        {
            if (statusVisualOverlay != null || enemy == null)
            {
                return;
            }

            statusVisualOverlay = enemy.GetComponent<EnemyStatusVisualOverlay>();
            if (statusVisualOverlay == null)
            {
                statusVisualOverlay = enemy.gameObject.AddComponent<EnemyStatusVisualOverlay>();
            }

            statusVisualOverlay.Initialize(enemy);
        }

        private string EnemyName => enemy != null ? enemy.gameObject.name : gameObject.name;
    }

    [DisallowMultipleComponent]
    public class EnemyStatusVisualOverlay : MonoBehaviour
    {
        private EnemyController owner;
        private SpriteRenderer sourceRenderer;
        private SpriteRenderer poisonOverlay;
        private SpriteRenderer freezeOverlay;
        private SpriteRenderer bleedOverlay;

        public void Initialize(EnemyController enemy)
        {
            owner = enemy;
            ResolveSourceRenderer();
            EnsureOverlayRenderers();
        }

        public void Refresh(bool poisoned, bool frozen, bool bleeding)
        {
            ResolveSourceRenderer();
            EnsureOverlayRenderers();

            if (sourceRenderer == null)
            {
                return;
            }

            poisonOverlay.enabled = poisoned;
            freezeOverlay.enabled = frozen;
            bleedOverlay.enabled = bleeding;
        }

        private void LateUpdate()
        {
            if (sourceRenderer == null)
            {
                ResolveSourceRenderer();
                EnsureOverlayRenderers();
                return;
            }

            SyncOverlay(poisonOverlay, new Color(0.22f, 0.95f, 0.3f, 0.56f), 0.01f);
            SyncOverlay(freezeOverlay, new Color(0.62f, 0.86f, 1f, 1f), 0.1f);
            SyncOverlay(bleedOverlay, new Color(1f, 0.12f, 0.16f, 0.56f), 0.03f);
        }

        private void OnDisable()
        {
            if (poisonOverlay != null) poisonOverlay.enabled = false;
            if (freezeOverlay != null) freezeOverlay.enabled = false;
            if (bleedOverlay != null) bleedOverlay.enabled = false;
        }

        private void ResolveSourceRenderer()
        {
            if (sourceRenderer != null)
            {
                return;
            }

            if (owner == null)
            {
                owner = GetComponent<EnemyController>();
            }

            if (owner == null)
            {
                return;
            }

            SpriteRenderer[] renderers = owner.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer candidate = renderers[i];
                if (candidate == null || candidate.gameObject.name.Contains("Overlay"))
                {
                    continue;
                }

                sourceRenderer = candidate;
                break;
            }
        }

        private void EnsureOverlayRenderers()
        {
            if (sourceRenderer == null)
            {
                return;
            }

            if (poisonOverlay == null)
            {
                poisonOverlay = CreateOverlayRenderer("PoisonOverlay");
            }

            if (freezeOverlay == null)
            {
                freezeOverlay = CreateOverlayRenderer("FreezeOverlay");
            }

            if (bleedOverlay == null)
            {
                bleedOverlay = CreateOverlayRenderer("BleedOverlay");
            }
        }

        private SpriteRenderer CreateOverlayRenderer(string name)
        {
            GameObject overlayObject = new GameObject(name);
            overlayObject.transform.SetParent(sourceRenderer.transform, false);
            overlayObject.transform.localPosition = Vector3.zero;
            overlayObject.transform.localRotation = Quaternion.identity;
            overlayObject.transform.localScale = Vector3.one;

            SpriteRenderer renderer = overlayObject.AddComponent<SpriteRenderer>();
            renderer.enabled = false;
            renderer.sortingLayerID = sourceRenderer.sortingLayerID;
            renderer.sortingOrder = sourceRenderer.sortingOrder + 1;
            return renderer;
        }

        private void SyncOverlay(SpriteRenderer overlay, Color color, float scaleOffset)
        {
            if (overlay == null || sourceRenderer == null)
            {
                return;
            }

            overlay.sprite = sourceRenderer.sprite;
            overlay.flipX = sourceRenderer.flipX;
            overlay.color = color;
            overlay.sortingLayerID = sourceRenderer.sortingLayerID;
            overlay.sortingOrder = sourceRenderer.sortingOrder + 1;
            overlay.transform.localScale = Vector3.one * (1f + scaleOffset);
        }
    }
}
