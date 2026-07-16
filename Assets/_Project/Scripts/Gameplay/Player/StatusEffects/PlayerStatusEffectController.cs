using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class PlayerStatusEffectController : MonoBehaviour
    {
        private struct MoveSpeedSlow
        {
            public float ratio;
            public float endTime;
        }

        private struct AttackPowerReduction
        {
            public float ratio;
            public float endTime;
        }

        private readonly List<MoveSpeedSlow> moveSpeedSlows = new List<MoveSpeedSlow>();
        private readonly List<AttackPowerReduction> attackPowerReductions = new List<AttackPowerReduction>();
        private readonly List<Coroutine> damageOverTimeRoutines = new List<Coroutine>();
        private Coroutine statusRoutine;

        private PlayerStats playerStats;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
        }

        public void ApplyMoveSpeedSlow(float slowRatio, float duration)
        {
            if (slowRatio <= 0f || duration <= 0f)
            {
                return;
            }

            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }

            if (playerStats == null)
            {
                return;
            }

            moveSpeedSlows.Add(new MoveSpeedSlow
            {
                ratio = Mathf.Clamp01(slowRatio),
                endTime = Time.time + duration
            });

            RefreshTemporaryModifiers();

            EnsureStatusRoutine();
        }

        public void ApplyAttackPowerReduction(float reductionRatio, float duration)
        {
            if (reductionRatio <= 0f || duration <= 0f)
            {
                return;
            }

            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }

            if (playerStats == null)
            {
                return;
            }

            attackPowerReductions.Add(new AttackPowerReduction
            {
                ratio = Mathf.Clamp01(reductionRatio),
                endTime = Time.time + duration
            });

            RefreshTemporaryModifiers();
            EnsureStatusRoutine();
        }

        public void ApplyDamageOverTime(float duration, float tickInterval, float tickDamage)
        {
            if (duration <= 0f || tickInterval <= 0f || tickDamage <= 0f)
            {
                return;
            }

            Coroutine routine = null;
            routine = StartCoroutine(DamageOverTimeRoutine(
                Mathf.Max(0.01f, duration),
                Mathf.Max(0.01f, tickInterval),
                tickDamage,
                () => damageOverTimeRoutines.Remove(routine)));
            damageOverTimeRoutines.Add(routine);
        }

        public void CleanseTemporaryDebuffs()
        {
            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }

            if (statusRoutine != null)
            {
                StopCoroutine(statusRoutine);
                statusRoutine = null;
            }

            moveSpeedSlows.Clear();
            attackPowerReductions.Clear();
            StopDamageOverTimeRoutines();
            playerStats?.RemoveModifiersFromSource(this);
        }

        private void EnsureStatusRoutine()
        {
            if (statusRoutine == null)
            {
                statusRoutine = StartCoroutine(StatusRoutine());
            }
        }

        private IEnumerator StatusRoutine()
        {
            while (moveSpeedSlows.Count > 0 || attackPowerReductions.Count > 0)
            {
                float nextEndTime = float.PositiveInfinity;
                for (int i = 0; i < moveSpeedSlows.Count; i++)
                {
                    nextEndTime = Mathf.Min(nextEndTime, moveSpeedSlows[i].endTime);
                }

                for (int i = 0; i < attackPowerReductions.Count; i++)
                {
                    nextEndTime = Mathf.Min(nextEndTime, attackPowerReductions[i].endTime);
                }

                float waitTime = Mathf.Max(0.02f, nextEndTime - Time.time);
                yield return new WaitForSeconds(waitTime);
                RefreshTemporaryModifiers();
            }

            statusRoutine = null;
        }

        private IEnumerator DamageOverTimeRoutine(float duration, float tickInterval, float tickDamage, System.Action onComplete)
        {
            PlayerController player = GetComponent<PlayerController>();
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float waitTime = Mathf.Min(tickInterval, duration - elapsed);
                yield return new WaitForSeconds(waitTime);
                elapsed += waitTime;

                if (!isActiveAndEnabled)
                {
                    break;
                }

                if (player == null)
                {
                    player = GetComponent<PlayerController>();
                }

                player?.TakeDamage(tickDamage);
            }

            onComplete?.Invoke();
        }

        private void StopDamageOverTimeRoutines()
        {
            for (int i = 0; i < damageOverTimeRoutines.Count; i++)
            {
                if (damageOverTimeRoutines[i] != null)
                {
                    StopCoroutine(damageOverTimeRoutines[i]);
                }
            }

            damageOverTimeRoutines.Clear();
        }

        private void RefreshTemporaryModifiers()
        {
            float now = Time.time;
            for (int i = moveSpeedSlows.Count - 1; i >= 0; i--)
            {
                if (moveSpeedSlows[i].endTime <= now)
                {
                    moveSpeedSlows.RemoveAt(i);
                }
            }

            for (int i = attackPowerReductions.Count - 1; i >= 0; i--)
            {
                if (attackPowerReductions[i].endTime <= now)
                {
                    attackPowerReductions.RemoveAt(i);
                }
            }

            if (playerStats == null)
            {
                return;
            }

            playerStats.RemoveModifiersFromSource(this);

            float strongestSlow = 0f;
            for (int i = 0; i < moveSpeedSlows.Count; i++)
            {
                strongestSlow = Mathf.Max(strongestSlow, moveSpeedSlows[i].ratio);
            }

            float strongestAttackReduction = 0f;
            for (int i = 0; i < attackPowerReductions.Count; i++)
            {
                strongestAttackReduction = Mathf.Max(strongestAttackReduction, attackPowerReductions[i].ratio);
            }

            if (strongestSlow > 0f)
            {
                playerStats.ApplyModifier(new CharacterStatModifier(
                    CharacterStatType.MoveSpeed,
                    -strongestSlow,
                    CharacterStatModifierMode.PercentAdd,
                    this));
            }

            if (strongestAttackReduction > 0f)
            {
                playerStats.ApplyModifier(new CharacterStatModifier(
                    CharacterStatType.AttackPower,
                    -strongestAttackReduction,
                    CharacterStatModifierMode.PercentAdd,
                    this));
            }
        }

        private void OnDisable()
        {
            if (playerStats != null)
            {
                playerStats.RemoveModifiersFromSource(this);
            }

            moveSpeedSlows.Clear();
            attackPowerReductions.Clear();
            StopDamageOverTimeRoutines();
            statusRoutine = null;
        }
    }
}
