using System;
using System.Collections.Generic;

namespace Necrocis
{
    // Simple, repeatable rewards keep three distinct offers available even late in a run.
    public static class PlayerStatItemCatalog
    {
        public sealed class Definition
        {
            public string Id { get; }
            public string Name { get; }
            public string Description { get; }
            public CharacterStatType Stat { get; }
            public float Amount { get; }
            public CharacterStatModifierMode Mode { get; }

            public Definition(string id, string name, string description, CharacterStatType stat, float amount,
                CharacterStatModifierMode mode = CharacterStatModifierMode.Flat)
            {
                Id = id; Name = name; Description = description; Stat = stat; Amount = amount; Mode = mode;
            }
        }

        public static IReadOnlyList<Definition> Items { get; } = Array.AsReadOnly(new[]
        {
            new Definition("dense_muscle", "응축 근육", "공격력 +1 (중복 획득 시 누적)", CharacterStatType.AttackPower, 1f),
            new Definition("hardened_muscle", "경화 근육", "공격력 +2 (중복 획득 시 누적)", CharacterStatType.AttackPower, 2f),
            new Definition("vital_cell", "생명 세포", "최대 체력 +1 (반 칸), 획득 시 체력 +1 회복 (중복 누적)", CharacterStatType.MaxHealth, 1f),
            new Definition("vital_organ", "생명 장기", "최대 체력 +2 (한 칸), 획득 시 체력 +2 회복 (중복 누적)", CharacterStatType.MaxHealth, 2f),
            new Definition("swift_cell", "신속 세포", "공격속도 +5% (중복 획득 시 합산)", CharacterStatType.AttackSpeed, 0.05f, CharacterStatModifierMode.PercentAdd),
            new Definition("elastic_fiber", "탄성 섬유", "이동속도 +0.5 (중복 획득 시 누적)", CharacterStatType.MoveSpeed, 0.5f)
        });

        public static bool TryGet(string id, out Definition definition)
        {
            foreach (var item in Items)
                if (string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase)) { definition = item; return true; }
            definition = null;
            return false;
        }

        public static bool IsRepeatable(string id) => TryGet(id, out _);
    }
}
