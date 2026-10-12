using System;
using System.Collections.Generic;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static class MonsterBalanceMigration
    {
        [MenuItem("Necrocis/Balance/Migrate Existing Combat (C-02)")]
        public static void Run()
        {
            MonsterBalanceCatalog catalog = MonsterBalanceSetup.EnsureAssets();
            foreach (string guid in AssetDatabase.FindAssets("t:EnemySpawnConfig", new[] { "Assets/_Project" }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<EnemySpawnConfig>(AssetDatabase.GUIDToAssetPath(guid));
                string before = EditorJsonUtility.ToJson(asset);
                foreach (EnemySpawnRuleConfig rule in asset.GetEnemySpawnRules()) MigrateRule(catalog, rule);
                if (before != EditorJsonUtility.ToJson(asset)) EditorUtility.SetDirty(asset);
            }
            foreach (string guid in AssetDatabase.FindAssets("t:BossArenaConfig", new[] { "Assets/_Project" }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<BossArenaConfig>(AssetDatabase.GUIDToAssetPath(guid));
                string before = EditorJsonUtility.ToJson(asset);
                MigrateBoss(catalog, asset.midBossArena.boss);
                if (before != EditorJsonUtility.ToJson(asset)) EditorUtility.SetDirty(asset);
            }
            // Covers legacy embedded rules still used when split config references are absent.
            foreach (string guid in AssetDatabase.FindAssets("t:BiomeConfig", new[] { "Assets/_Project" }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<BiomeConfig>(AssetDatabase.GUIDToAssetPath(guid));
                string before = EditorJsonUtility.ToJson(asset);
                foreach (EnemySpawnRuleConfig rule in asset.GetEnemySpawnRules()) MigrateRule(catalog, rule);
                if (asset.GetMidBossArenaConfig()?.enabled == true) MigrateBoss(catalog, asset.GetMidBossArenaConfig().boss);
                if (before != EditorJsonUtility.ToJson(asset)) EditorUtility.SetDirty(asset);
            }
            List<string> errors = catalog.GetValidationErrors();
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("[MonsterBalance-C02] Migration PASS: " + catalog.monsters.Count + " definitions. Existing definitions preserved.");
        }

        private static MonsterDefinition Create(MonsterBalanceCatalog catalog, string id, string label,
            MonsterTier tier, float hp, float attack, float speed, int xp, bool contact, float contactDamage, float knockback)
        {
            string path = MonsterBalanceSetup.Root + "/Definitions/" + id + ".asset";
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(path);
            if (definition == null)
            {
                if (attack <= 0f && contact && contactDamage > 0f)
                    throw new InvalidOperationException(id + ": 공격력 0에서 접촉 피해 계수를 이관할 수 없습니다.");
                definition = ScriptableObject.CreateInstance<MonsterDefinition>();
                definition.monsterId = id;
                definition.displayName = label;
                definition.tier = tier;
                definition.statSets = new List<MonsterStatSet> { Set("Default", hp, attack, speed) };
                definition.reward.baseExperience = xp;
                definition.contact.enabled = contact;
                definition.contact.damageCoefficient = attack > 0f ? contactDamage / attack : 0f;
                definition.contact.knockbackDistance = knockback;
                definition.designNote = "C-02 기존 전투 이관. 원본 수정은 새 개체부터 적용. 재사용 대기·범위·예고 시간은 스폰 규칙/보스 패턴 설정에서 조절.";
                AssetDatabase.CreateAsset(definition, path);
            }
            if (!catalog.monsters.Contains(definition)) catalog.monsters.Add(definition);
            return definition;
        }

        private static MonsterStatSet Set(string id, float hp, float attack, float speed) =>
            new MonsterStatSet { id = id, maxHealth = hp, attackPower = attack, moveSpeed = speed };

        private static void Pattern(MonsterDefinition definition, string id, float rawDamage, float attack)
        {
            if (definition.patternDamage.Exists(p => p.id == id)) return;
            if (attack <= 0f && rawDamage > 0f) throw new InvalidOperationException(id + ": 공격력 0을 확인하세요.");
            definition.patternDamage.Add(new MonsterPatternDamage { id = id, coefficient = attack > 0f ? rawDamage / attack : 0f });
        }

        private static void MigrateRule(MonsterBalanceCatalog catalog, EnemySpawnRuleConfig rule)
        {
            if (rule == null || rule.monsterDefinition != null) return;
            rule.monsterDefinition = Create(catalog, (rule.isElite ? "legacy-elite." : "normal.") + rule.name.ToLowerInvariant(),
                rule.name, rule.isElite ? MonsterTier.Elite : MonsterTier.Normal,
                rule.maxHealth, rule.attackDamage, rule.moveSpeed, rule.expReward,
                rule.enableContactDamage, rule.contactDamage, rule.contactKnockbackDistance);
        }

        private static void MigrateBoss(MonsterBalanceCatalog catalog, MidBossDefinition boss)
        {
            if (boss?.bossRule == null || boss.bossRule.monsterDefinition != null) return;
            EnemySpawnRuleConfig rule = boss.bossRule;
            float hp = Mathf.Max(rule.maxHealth * (boss.overrideStats ? boss.maxHealthMultiplier : 1f), boss.minimumMaxHealth);
            float attack = rule.attackDamage * (boss.overrideStats ? boss.attackDamageMultiplier : 1f);
            float speed = rule.moveSpeed * (boss.overrideStats ? boss.moveSpeedMultiplier : 1f);
            float secondHp = hp, secondAttack = attack, secondSpeed = speed;
            // BuildBossRule previously used default contact (1 damage, 0.45 knockback).
            float contactDamage = 1f;
            switch (boss.patternType)
            {
                case MidBossPatternType.Intestine:
                    secondSpeed = speed * boss.intestinePattern.phase2MoveMultiplier;
                    speed *= boss.intestinePattern.phase1MoveMultiplier;
                    break;
                case MidBossPatternType.Liver:
                    attack = boss.liverPattern.phase1AttackDamage; speed = boss.liverPattern.phase1MoveSpeed;
                    secondAttack = boss.liverPattern.phase2AttackDamage; secondSpeed = boss.liverPattern.phase2MoveSpeed;
                    break;
                case MidBossPatternType.Stomach:
                    attack = boss.stomachPattern.phase1AttackDamage; speed = boss.stomachPattern.phase1MoveSpeed;
                    secondAttack = boss.stomachPattern.phase2AttackDamage; secondSpeed = boss.stomachPattern.phase2MoveSpeed;
                    break;
                case MidBossPatternType.Lung:
                    hp = boss.lungPattern.phase1BrotherHealth; attack = boss.lungPattern.phase1AttackDamage; speed = boss.lungPattern.phase1MoveSpeed;
                    secondHp = boss.lungPattern.phase2MaxHealth; secondAttack = boss.lungPattern.phase2AttackDamage; secondSpeed = boss.lungPattern.phase2MoveSpeed;
                    contactDamage = boss.lungPattern.phase1ContactDamage;
                    boss.lungPattern.windGustSpeedMultiplier = speed > 0f ? boss.lungPattern.windGustMoveSpeed / speed : 0f;
                    break;
            }
            MonsterDefinition definition = Create(catalog, "boss." + boss.patternType.ToString().ToLowerInvariant(),
                boss.displayName, MonsterTier.Boss, hp, attack, speed, rule.expReward, true, contactDamage, 0.45f);
            if (!definition.TryGetStatSet("Phase2", out _)) definition.statSets.Add(Set("Phase2", secondHp, secondAttack, secondSpeed));
            switch (boss.patternType)
            {
                case MidBossPatternType.Intestine:
                    Pattern(definition, "dung", boss.intestinePattern.dungCollisionDamage, attack);
                    Pattern(definition, "stomp", boss.intestinePattern.stompDamage, secondAttack);
                    var settings = boss.intestinePattern;
                    settings.parasiteDefinition = Create(catalog, "normal.intestine-parasite", "장 보스 소환 기생충", MonsterTier.Normal,
                        settings.parasiteMaxHealth, settings.parasiteAttackDamage, settings.parasiteMoveSpeed, 0, true, 1f, 0.45f);
                    break;
                case MidBossPatternType.Liver: Pattern(definition, "blood-bomb", boss.liverPattern.phase1AttackDamage, attack); break;
                case MidBossPatternType.Stomach:
                    Pattern(definition, "melee", boss.stomachPattern.phase1AttackDamage, attack);
                    Pattern(definition, "charge", boss.stomachPattern.chargeDamage, attack);
                    Pattern(definition, "acid", boss.stomachPattern.acidDamage, secondAttack);
                    Pattern(definition, "acid-dot", boss.stomachPattern.acidTickDamage, secondAttack);
                    Pattern(definition, "short-range", boss.stomachPattern.shortRangeDamage, secondAttack);
                    break;
                case MidBossPatternType.Lung:
                    Pattern(definition, "gas", boss.lungPattern.phase1AttackDamage, attack);
                    Pattern(definition, "dive", boss.lungPattern.diveLandingDamage, secondAttack);
                    break;
            }
            rule.monsterDefinition = definition;
            EditorUtility.SetDirty(definition);
        }
    }
}
