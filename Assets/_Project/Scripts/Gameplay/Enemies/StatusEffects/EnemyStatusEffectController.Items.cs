using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    internal static class RetiredItemHealthSources
    {
        private static readonly HashSet<HealthEffectSource> retired = new HashSet<HealthEffectSource>();
        internal static void Add(HealthEffectSource source) => retired.Add(source);
        internal static bool Contains(HealthEffectSource source) => retired.Contains(source);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => retired.Clear();
    }
    public partial class EnemyStatusEffectController
    {
        public struct ItemDotRecord
        {
            public HealthEffectSource Source;
            public float Until, Interval, Damage, NextTick;
            public bool Transferred, Extended;
            public int Skipped;
        }
        private readonly Dictionary<HealthEffectSource, ItemDotRecord> poisonRecords = new Dictionary<HealthEffectSource, ItemDotRecord>();
        private readonly Dictionary<HealthEffectSource, ItemDotRecord> bleedRecords = new Dictionary<HealthEffectSource, ItemDotRecord>();
        private bool bleedExtended;
        private int bleedSkippedTicks;
        public bool IsSlowed => moveSpeedSlows.Exists(slow => slow.endTime > Time.time);
        public bool PoisonCanTransfer
        {
            get { foreach (var record in poisonRecords.Values) if (record.Until > Time.time && !record.Transferred) return true; return false; }
        }
        private bool HasLiveRecords(Dictionary<HealthEffectSource, ItemDotRecord> records)
        { foreach (var record in records.Values) if (record.Until > Time.time && !RetiredItemHealthSources.Contains(record.Source)) return true; return false; }
        public bool HasSlowExcluding(HealthEffectSource source) => moveSpeedSlows.Exists(slow => slow.endTime > Time.time && (!slow.hasSource || slow.source != source));
        private void RegisterDot(bool poison, float duration, float interval, float damage, HealthEffectSource source)
        {
            var records = poison ? poisonRecords : bleedRecords;
            // November 11 balance: keep one strongest effect per status, across item sources.
            // Retain source metadata so removing an item can still retire its active damage.
            foreach (var key in new List<HealthEffectSource>(records.Keys))
                if (key != source) records.Remove(key);
            var current = poison ? poisonSource : bleedSource;
            bool active = poison ? hasPoisonSource : hasBleedSource;
            if (active && records.TryGetValue(current, out var primary))
            {
                primary.NextTick = poison ? poisonNextTickTime : bleedNextTickTime;
                primary.Until = poison ? poisonEndTime : bleedEndTime; if (!poison) primary.Skipped = bleedSkippedTicks; records[current] = primary;
            }
            if (!poison && (!IsBleeding || bleedSource != source)) bleedExtended = false;
            if (records.TryGetValue(source, out var existing) && existing.Until > Time.time)
            {
                existing.Until = Mathf.Max(existing.Until, Time.time + duration);
                if (damage / interval >= existing.Damage / Mathf.Max(.05f, existing.Interval)) { existing.Damage = damage; existing.Interval = interval; }
                records[source] = existing;
            }
            else records[source] = new ItemDotRecord { Source = source, Until = Time.time + duration, Interval = interval, Damage = damage, NextTick = Time.time + interval };
        }
        private void Update()
        {
            if (enemy == null || enemy.IsDead || Time.deltaTime <= 0) return;
            TickSecondary(poisonRecords, hasPoisonSource, poisonSource);
            TickSecondary(bleedRecords, hasBleedSource, bleedSource);
            RefreshStatusVisuals();
        }
        private void TickSecondary(Dictionary<HealthEffectSource, ItemDotRecord> records, bool hasPrimary, HealthEffectSource primary)
        {
            if (records.Count == 0 || hasPrimary && records.Count == 1) return;
            uint generation = enemy.SpawnGeneration;
            foreach (var record in new List<ItemDotRecord>(records.Values))
            {
                if (hasPrimary && record.Source == primary) continue;
                if (RetiredItemHealthSources.Contains(record.Source)) { records.Remove(record.Source); continue; }
                if (record.NextTick <= Time.time + .0001f && record.NextTick <= record.Until + .0001f)
                {
                    var next = record; next.NextTick += Mathf.Max(.05f, record.Interval);
                    if (next.Skipped > 0) { next.Skipped--; records[record.Source] = next; continue; }
                    records[record.Source] = next; enemy.TakeDamage(record.Damage, record.Source);
                    if (enemy == null || enemy.SpawnGeneration != generation || enemy.IsDead) return;
                }
                else if (record.Until < Time.time) records.Remove(record.Source);
            }
        }
        public void TransferPoisonTo(EnemyStatusEffectController target, HealthEffectSource source)
        {
            if (target == null) return;
            ItemDotRecord best = default;
            foreach (var record in poisonRecords.Values)
                if (record.Until > Time.time && !record.Transferred && record.Damage > best.Damage) best = record;
            if (best.Damage <= 0) return;
            float next = hasPoisonSource && best.Source == poisonSource ? poisonNextTickTime : best.NextTick;
            target.ApplyPoison(best.Until - Time.time, best.Interval, best.Damage, source);
            if (target.poisonRecords.TryGetValue(source, out var transferred))
            { transferred.Transferred = true; transferred.NextTick = next; target.poisonRecords[source] = transferred; }
            if (target.poisonCoroutine != null) target.StopCoroutine(target.poisonCoroutine);
            target.poisonNextTickTime = next; target.poisonWaitPending = true;
            target.poisonCoroutine = target.StartCoroutine(target.ResumeDot(new EnemyStatusRuntimeSnapshot.Dot { Pending = true, NextRemaining = Mathf.Max(0, next - Time.time) }, true));
        }
        private bool TryGetBleed(out ItemDotRecord chosen)
        {
            chosen = default;
            if (hasBleedSource && bleedRecords.TryGetValue(bleedSource, out chosen) && chosen.Until > Time.time) return true;
            chosen = default;
            foreach (var record in bleedRecords.Values) if (record.Until > Time.time && record.Damage > chosen.Damage) chosen = record;
            return chosen.Damage > 0;
        }
        public bool TryExtendBleed(float seconds)
        {
            if (!TryGetBleed(out var record) || record.Extended) return false;
            record.Extended = true; record.Until += seconds; bleedRecords[record.Source] = record;
            if (hasBleedSource && record.Source == bleedSource) { bleedEndTime = record.Until; bleedExtended = true; }
            return true;
        }
        public void ConsumeBleedTick()
        {
            if (enemy == null || enemy.IsDead || !TryGetBleed(out var record)) return;
            record.Skipped++; bleedRecords[record.Source] = record;
            if (hasBleedSource && record.Source == bleedSource) bleedSkippedTicks++;
            enemy.TakeDamage(record.Damage, record.Source);
        }
        public void ConsumeSlow(float seconds)
        {
            for (int i = 0; i < moveSpeedSlows.Count; i++)
            { var slow = moveSpeedSlows[i]; slow.endTime = Mathf.Max(Time.time, slow.endTime - seconds); moveSpeedSlows[i] = slow; }
            RefreshMoveSpeedSlow();
            if (slowRoutine != null) StopCoroutine(slowRoutine);
            slowRoutine = moveSpeedSlows.Count > 0 ? StartCoroutine(SlowRoutine()) : null;
            freezeVisualEndTime = 0;
            foreach (var slow in moveSpeedSlows) freezeVisualEndTime = Mathf.Max(freezeVisualEndTime, slow.endTime);
            RefreshStatusVisuals();
        }
        public void RemoveItemSource(HealthEffectSource source)
        {
            poisonRecords.Remove(source); bleedRecords.Remove(source);
            if (hasPoisonSource && poisonSource == source)
            {
                if (poisonCoroutine != null) StopCoroutine(poisonCoroutine);
                poisonCoroutine = null; EndPoisonSource(); poisonEndTime = 0;
            }
            if (hasBleedSource && bleedSource == source)
            {
                if (bleedCoroutine != null) StopCoroutine(bleedCoroutine);
                bleedCoroutine = null; EndBleedSource(); bleedEndTime = 0;
            }
            moveSpeedSlows.RemoveAll(slow => slow.hasSource && slow.source == source);
            RefreshMoveSpeedSlow(); freezeVisualEndTime = 0;
            foreach (var slow in moveSpeedSlows) freezeVisualEndTime = Mathf.Max(freezeVisualEndTime, slow.endTime);
            enemy?.Stats?.RemoveHealthEffectSource(source); RefreshStatusVisuals();
        }
        private ItemDotRecord[] CaptureRecords(Dictionary<HealthEffectSource, ItemDotRecord> records)
        {
            var result = new List<ItemDotRecord>();
            foreach (var entry in records.Values)
            { var value = entry; value.Until -= Time.time; value.NextTick -= Time.time; if (value.Until > 0) result.Add(value); }
            return result.ToArray();
        }
        private void RestoreRecords(Dictionary<HealthEffectSource, ItemDotRecord> target, ItemDotRecord[] saved)
        {
            target.Clear(); if (saved == null) return;
            foreach (var entry in saved) { var value = entry; value.Until += Time.time; value.NextTick += Time.time; if (!RetiredItemHealthSources.Contains(value.Source)) target[value.Source] = value; }
        }
    }
}
