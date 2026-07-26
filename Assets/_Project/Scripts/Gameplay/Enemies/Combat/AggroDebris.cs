using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 항체 엘리트 사망 시 생성되는 잔해.
    /// 1) 충격파 VFX가 화면 전체로 퍼짐
    /// 2) 주변 적이 강제로 플레이어에게 돌진 (chaseRadius 50% 증가 + 강제 Chase)
    /// 3) 지속 시간 후 효과 해제
    /// </summary>
    public class AggroDebris : MonoBehaviour
    {
        private const float AggroScanInterval = 0.2f;

        private float duration;
        private float aggroRadius;
        private float elapsed;
        private bool active;

        // 잔해 스프라이트
        private SpriteRenderer debrisRenderer;

        // 충격파 VFX
        private GameObject vfxObject;
        private SpriteRenderer vfxRenderer;
        private SpriteFrameAnimator vfxAnimator;
        private float vfxScale;
        private bool vfxFinished;

        // 영향받는 적 추적
        private readonly HashSet<EnemyController> boostedEnemies = new HashSet<EnemyController>();
        private float aggroRadiusSqr;
        private float nextAggroScanTime;

        public void Configure(float duration, float aggroRadius, Sprite debrisSprite, int sortingOrder,
                              Sprite[] vfxSprites, float vfxTargetScale, float vfxSpeed)
        {
            this.duration = duration;
            this.aggroRadius = aggroRadius;
            this.elapsed = 0f;
            this.active = true;
            this.vfxScale = vfxTargetScale;
            this.vfxFinished = false;
            this.aggroRadiusSqr = aggroRadius * aggroRadius;

            // ── 잔해 본체 (사망 스프라이트 마지막 프레임) ──
            debrisRenderer = GetComponent<SpriteRenderer>();
            if (debrisRenderer == null)
                debrisRenderer = gameObject.AddComponent<SpriteRenderer>();

            debrisRenderer.sprite = debrisSprite;
            debrisRenderer.sortingOrder = sortingOrder;
            debrisRenderer.color = new Color(0.6f, 0.2f, 0.8f, 0.7f);

            Billboard bb = GetComponent<Billboard>();
            if (bb == null)
                bb = gameObject.AddComponent<Billboard>();
            bb.enabled = true;

            // ── 충격파 VFX (자식 오브젝트) ──
            if (vfxSprites != null && vfxSprites.Length > 0)
            {
                SpawnShockwaveVFX(vfxSprites, vfxSpeed, sortingOrder + 100);
            }
            else
            {
                vfxFinished = true;
            }

            // 즉시 어그로 적용
            ForceAggroNearbyEnemies();
            nextAggroScanTime = Time.time + AggroScanInterval;
        }

        private void SpawnShockwaveVFX(Sprite[] frames, float speed, int sortingOrder)
        {
            vfxObject = new GameObject("ShockwaveVFX");
            vfxObject.transform.SetParent(transform, false);
            vfxObject.transform.localPosition = new Vector3(0f, 1f, 0f);
            vfxObject.transform.localScale = Vector3.one;

            vfxRenderer = vfxObject.AddComponent<SpriteRenderer>();
            vfxRenderer.sortingOrder = sortingOrder;
            vfxRenderer.color = new Color(0.7f, 0.3f, 1f, 0.9f); // 보라색 충격파
            vfxRenderer.sprite = frames[0];

            Billboard vfxBb = vfxObject.AddComponent<Billboard>();
            vfxBb.enabled = true;

            vfxAnimator = vfxObject.AddComponent<SpriteFrameAnimator>();
            vfxAnimator.enabled = true;

            // 반복 재생 (2회) 후 종료
            int loopCount = 0;
            System.Action onLoop = null;
            onLoop = () =>
            {
                loopCount++;
                if (loopCount >= 2)
                {
                    vfxFinished = true;
                    if (vfxObject != null) vfxObject.SetActive(false);
                }
                else
                {
                    vfxAnimator.PlayOneShot(frames, speed, onLoop);
                }
            };
            vfxAnimator.PlayOneShot(frames, speed, onLoop);
        }

        private void Update()
        {
            if (!active) return;

            elapsed += Time.deltaTime;

            // 충격파 VFX 확대 애니메이션 (0.5초 동안 1 → vfxScale)
            if (!vfxFinished && vfxObject != null)
            {
                float expandTime = 0.5f;
                float t = Mathf.Clamp01(elapsed / expandTime);
                float currentScale = Mathf.Lerp(1f, vfxScale, t);
                vfxObject.transform.localScale = new Vector3(currentScale, currentScale, currentScale);

                // VFX 페이드
                if (vfxRenderer != null)
                {
                    float vfxAlpha = Mathf.Lerp(0.9f, 0.3f, t);
                    vfxRenderer.color = new Color(0.7f, 0.3f, 1f, vfxAlpha);
                }
            }

            // 잔해 페이드 아웃 (마지막 3초)
            float remaining = duration - elapsed;
            if (remaining < 3f && debrisRenderer != null)
            {
                float alpha = Mathf.Clamp01(remaining / 3f) * 0.7f;
                Color c = debrisRenderer.color;
                c.a = alpha;
                debrisRenderer.color = c;
            }

            if (elapsed >= duration)
            {
                active = false;
                Destroy(gameObject);
                return;
            }

            // 새로 범위에 들어온 적만 주기적으로 확인한다.
            if (Time.time >= nextAggroScanTime)
            {
                nextAggroScanTime = Time.time + AggroScanInterval;
                ForceAggroNearbyEnemies();
            }
        }

        /// <summary>
        /// 범위 내 적을 강제로 플레이어에게 돌진시킨다
        /// </summary>
        private void ForceAggroNearbyEnemies()
        {
            IReadOnlyList<EnemyController> enemies = EnemyController.ActiveEnemyControllers;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead) continue;
                if (enemy.Config == null) continue;
                if (enemy.IsElite) continue; // 다른 엘리트는 제외

                Vector3 delta = enemy.transform.position - transform.position;
                delta.y = 0f;
                if (delta.sqrMagnitude > aggroRadiusSqr) continue;

                if (!boostedEnemies.Contains(enemy))
                {
                    boostedEnemies.Add(enemy);
                    enemy.ApplyAggroBoost(1.0f);  // chaseRadius 2배 증가
                    enemy.ForceChasePlayer();     // 이동속도 3배 + 즉시 플레이어에게 돌진
                }
            }
        }

        private void OnDestroy()
        {
            foreach (EnemyController enemy in boostedEnemies)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.RemoveAllAggroEffects();
                }
            }
            boostedEnemies.Clear();

            if (vfxObject != null)
                Destroy(vfxObject);
        }
    }
}
