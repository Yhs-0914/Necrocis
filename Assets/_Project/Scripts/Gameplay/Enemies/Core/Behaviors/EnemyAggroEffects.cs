using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // A debris lifetime owns a source token; a handle also identifies one enemy spawn and registration.
    internal readonly struct EnemyAggroEffectHandle
    {
        internal readonly EnemyController Owner;
        internal readonly object Source;
        internal readonly uint Generation;
        internal readonly uint Registration;

        internal EnemyAggroEffectHandle(EnemyController owner, object source, uint generation, uint registration)
        {
            Owner = owner;
            Source = source;
            Generation = generation;
            Registration = registration;
        }

        internal bool IsCurrent => Owner != null && Owner.IsAggroEffectCurrent(this);
        internal void Release()
        {
            if (Owner != null) Owner.RemoveAggroEffect(this);
        }
    }

    public partial class EnemyController
    {
        private readonly struct AggroSourceEffect
        {
            internal readonly float SpeedMultiplier;
            internal readonly float ChaseMultiplier;
            internal readonly uint Registration;

            internal AggroSourceEffect(float speed, float chase, uint registration)
            {
                SpeedMultiplier = speed;
                ChaseMultiplier = chase;
                Registration = registration;
            }
        }

        private readonly Dictionary<object, AggroSourceEffect> aggroSources = new Dictionary<object, AggroSourceEffect>();
        private object aggroModifierSource = new object();
        private EnemySpawnRuleConfig aggroRuntimeConfig;
        private float aggroBaseChaseRadius;
        private uint aggroGeneration;
        private uint aggroRegistration;
        private uint aggroRevision;

        // Keep the existing public movement wrappers isolated from debris-owned sources.
        private readonly object legacyChaseSource = new object();
        private readonly object legacySpeedSource = new object();
        private EnemyAggroEffectHandle legacyChaseHandle;
        private EnemyAggroEffectHandle legacySpeedHandle;

        internal bool TryAddAggroEffect(object source, uint generation, float speedMultiplier,
            float chaseMultiplier, out EnemyAggroEffectHandle handle)
        {
            return TryRegisterAggroEffect(source, generation, speedMultiplier, chaseMultiplier, true, out handle);
        }

        private bool TryRegisterAggroEffect(object source, uint generation, float speedMultiplier,
            float chaseMultiplier, bool forceChase, out EnemyAggroEffectHandle handle)
        {
            handle = default;
            if (source == null || generation != SpawnGeneration || !isActiveAndEnabled || IsDead
                || config == null || stats == null || !IsAggroMultiplierValid(speedMultiplier)
                || !IsAggroMultiplierValid(chaseMultiplier))
                return false;

            if (aggroRuntimeConfig != null && (aggroGeneration != generation || aggroRuntimeConfig != config))
                ResetAggroEffects();

            // A reset can synchronously notify stat subscribers, including pooling the actor.
            if (generation != SpawnGeneration || !isActiveAndEnabled || IsDead || config == null || stats == null)
                return false;

            if (aggroSources.TryGetValue(source, out AggroSourceEffect existing))
            {
                handle = new EnemyAggroEffectHandle(this, source, generation, existing.Registration);
                return true;
            }

            if (aggroSources.Count == 0)
            {
                aggroRuntimeConfig = config;
                aggroBaseChaseRadius = config.chaseRadius;
                aggroGeneration = generation;
            }

            uint registration = ++aggroRegistration;
            aggroSources.Add(source, new AggroSourceEffect(speedMultiplier, chaseMultiplier, registration));
            aggroRevision++;
            handle = new EnemyAggroEffectHandle(this, source, generation, registration);
            RefreshAggroEffects();
            if (!IsAggroEffectCurrent(handle)) return false;

            if (forceChase)
            {
                attackTimer = 0f;
                ChangeState(EnemyChaseState.Instance);
            }
            return IsAggroEffectCurrent(handle);
        }

        internal bool IsAggroEffectCurrent(EnemyAggroEffectHandle handle)
        {
            return handle.Owner == this && handle.Source != null && handle.Generation == SpawnGeneration
                && handle.Generation == aggroGeneration
                && aggroSources.TryGetValue(handle.Source, out AggroSourceEffect effect)
                && effect.Registration == handle.Registration;
        }

        internal void RemoveAggroEffect(EnemyAggroEffectHandle handle)
        {
            if (!IsAggroEffectCurrent(handle)) return;
            aggroSources.Remove(handle.Source);
            aggroRevision++;
            if (aggroSources.Count == 0) ResetAggroEffects();
            else RefreshAggroEffects();
        }

        private void RefreshAggroEffects()
        {
            if (aggroRuntimeConfig == null || aggroRuntimeConfig != config || aggroGeneration != SpawnGeneration)
                return;

            float speedMultiplier = 1f;
            float chaseMultiplier = 1f;
            foreach (AggroSourceEffect effect in aggroSources.Values)
            {
                speedMultiplier = Mathf.Max(speedMultiplier, effect.SpeedMultiplier);
                chaseMultiplier = Mathf.Max(chaseMultiplier, effect.ChaseMultiplier);
            }

            uint revision = aggroRevision;
            uint generation = SpawnGeneration;
            CharacterStats targetStats = stats;
            object modifierSource = aggroModifierSource;
            aggroRuntimeConfig.chaseRadius = aggroBaseChaseRadius * chaseMultiplier;
            targetStats.RemoveModifiersFromSource(modifierSource);
            // StatChanged callbacks may remove the source or return/reconfigure this pooled actor.
            if (revision != aggroRevision || generation != SpawnGeneration || targetStats != stats
                || modifierSource != aggroModifierSource)
                return;

            if (speedMultiplier > 1f)
                targetStats.AddModifier(CharacterStatType.MoveSpeed, speedMultiplier - 1f,
                    CharacterStatModifierMode.PercentMultiply, modifierSource);
        }

        // Called before pool configuration/release and on direct disable. Safe to call repeatedly.
        private void ResetAggroEffects()
        {
            CharacterStats previousStats = stats;
            object previousModifierSource = aggroModifierSource;
            EnemySpawnRuleConfig previousConfig = aggroRuntimeConfig;
            float previousRadius = aggroBaseChaseRadius;

            aggroSources.Clear();
            aggroRuntimeConfig = null;
            aggroBaseChaseRadius = 0f;
            aggroGeneration = 0;
            aggroRevision++;
            aggroModifierSource = new object();
            legacyChaseHandle = default;
            legacySpeedHandle = default;
            if (previousConfig != null) previousConfig.chaseRadius = previousRadius;
            previousStats?.RemoveModifiersFromSource(previousModifierSource);
        }

        private static bool IsAggroMultiplierValid(float value)
        {
            return value >= 1f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private void ApplyLegacyAggroBoost(float percentBoost)
        {
            if (legacyChaseHandle.IsCurrent) return;
            TryRegisterAggroEffect(legacyChaseSource, SpawnGeneration, 1f, 1f + percentBoost, false,
                out legacyChaseHandle);
        }

        private void RemoveLegacyAggroBoost()
        {
            EnemyAggroEffectHandle handle = legacyChaseHandle;
            legacyChaseHandle = default;
            handle.Release();
        }

        private void ForceLegacyAggroChase()
        {
            if (!TryRegisterAggroEffect(legacySpeedSource, SpawnGeneration, 3f, 1f, false,
                out legacySpeedHandle)) return;
            attackTimer = 0f;
            ChangeState(EnemyChaseState.Instance);
        }

        private void RemoveLegacyAggroEffects()
        {
            EnemyAggroEffectHandle chase = legacyChaseHandle;
            EnemyAggroEffectHandle speed = legacySpeedHandle;
            legacyChaseHandle = default;
            legacySpeedHandle = default;
            chase.Release();
            speed.Release();
        }
    }
}
