using System;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    // Work with Unity's serialized objects rather than depending on YAML field order.
    public static class BiomeEnemySetup
    {
        private sealed class Entry
        {
            public string id, name, biome;
            public bool ranged;
            public float health, speed, range, cooldown;
            public int experience;
            public Entry(string id, string name, string biome, bool ranged, float health, float speed, float range, float cooldown, int experience)
            { this.id = id; this.name = name; this.biome = biome; this.ranged = ranged; this.health = health; this.speed = speed; this.range = range; this.cooldown = cooldown; this.experience = experience; }
        }
        private static readonly Entry[] Roster =
        {
            new Entry("01_stomach_gulp", "StomachGulp", "Stomach", true, 4, .8f, 7, 1.5f, 15),
            new Entry("02_stomach_mucus_snail", "StomachMucusSnail", "Stomach", false, 6, .8f, 1.5f, 1.1f, 10),
            new Entry("03_stomach_overfed_sac", "StomachOverfedSac", "Stomach", true, 6, .8f, 6.5f, 1.8f, 15),
            new Entry("04_intestine_foldworm", "IntestineFoldworm", "Intestine", false, 6, 1.1f, 1.5f, 1, 10),
            new Entry("05_intestine_villus_hand", "IntestineVillusHand", "Intestine", false, 6, .8f, 1.8f, 1.2f, 10),
            new Entry("06_intestine_knotted_eel", "IntestineKnottedEel", "Intestine", false, 8, .9f, 1.7f, 1.2f, 12),
            new Entry("07_liver_suture_cell", "LiverSutureCell", "Liver", true, 4, .8f, 7, 1.5f, 15),
            new Entry("08_liver_clot", "LiverClot", "Liver", false, 8, .8f, 1.5f, 1.2f, 12),
            new Entry("09_liver_coagulation_core", "LiverCoagulationCore", "Liver", true, 6, .8f, 6.5f, 1.8f, 15),
            new Entry("10_lung_breath_bubble", "LungBreathBubble", "Lung", true, 4, .8f, 7, 1.5f, 15),
            new Entry("11_lung_cilia_brush", "LungCiliaBrush", "Lung", false, 6, 1.1f, 1.7f, 1, 10),
            new Entry("12_lung_overinflated_alveoli", "LungOverinflatedAlveoli", "Lung", true, 6, .8f, 6.5f, 1.8f, 15)
        };

        [MenuItem("Necrocis/Balance/Configure Biome Normal Enemies")]
        public static void Run()
        {
            const string root = "Assets/_Project/Data/BiomeConfigs/";
            var intestine = AssetDatabase.LoadAssetAtPath<EnemySpawnConfig>(root + "IntestineEnemySpawnConfig.asset");
            if (intestine == null) throw new InvalidOperationException("장 스폰 설정이 없습니다.");
            var templates = intestine.GetEnemySpawnRules().ToDictionary(r => r.name, StringComparer.Ordinal);
            if (!templates.ContainsKey("NKCell") || !templates.ContainsKey("BCell"))
                throw new InvalidOperationException("기존 NKCell/BCell 원본을 보존해야 합니다.");
            var prepared = new Dictionary<EnemySpawnConfig, List<EnemySpawnRuleConfig>>();
            foreach (string biome in new[] { "Stomach", "Intestine", "Liver", "Lung" })
            {
                var asset = AssetDatabase.LoadAssetAtPath<EnemySpawnConfig>(root + biome + "EnemySpawnConfig.asset");
                if (asset == null) throw new InvalidOperationException(biome + " 스폰 설정이 없습니다.");
                prepared.Add(asset, asset.GetEnemySpawnRules().Where(r => !Roster.Any(e => e.name == r.name)).ToList());
            }
            for (int index = 0; index < Roster.Length; index++)
            {
                Entry entry = Roster[index];
                var asset = AssetDatabase.LoadAssetAtPath<EnemySpawnConfig>(root + entry.biome + "EnemySpawnConfig.asset");
                MonsterDefinition existing = asset.GetEnemySpawnRules().FirstOrDefault(r => r.name == entry.name)?.monsterDefinition;
                var rule = templates[entry.ranged ? "BCell" : "NKCell"].CreateRuntimeCopy();
                rule.monsterDefinition = existing; // Preserve tuning; never inherit the template's definition.
                rule.name = entry.name; rule.density = entry.biome == "Intestine" ? .02f : .05f;
                rule.minDistance = 6; rule.poissonSalt = 1601 + index; rule.maxAlive = 2;
                rule.activationRadius = 55; rule.respawnCooldown = 6; rule.spawnRadius = 2;
                rule.moveSpeed = entry.speed; rule.maxHealth = entry.health; rule.attackDamage = 1;
                rule.attackRange = entry.range; rule.attackCooldown = entry.cooldown; rule.expReward = entry.experience;
                rule.isRanged = entry.ranged; rule.isElite = false; rule.chargesAtPlayer = false;
                rule.splitsOnDeath = false; rule.leavesDebrisOnDeath = false;
                rule.chaseRadius = 10; rule.leashRadius = 14; rule.scale = Vector3.one;
                rule.colliderSize = new Vector3(.85f, 1.2f, .85f); rule.colliderCenter = new Vector3(0, .6f, 0);
                rule.expandColliderOnAttack = false; rule.animationSpeed = .16f;
                rule.attackAnimationSpeed = .14f; rule.deathAnimationSpeed = .16f;
                rule.attackSpritesUp = rule.attackSpritesDown = Array.Empty<Sprite>();
                rule.idleSpritesUp = rule.idleSpritesDown = rule.moveSpritesUp = rule.moveSpritesDown = Array.Empty<Sprite>();
                rule.projectileSpeed = 6; rule.projectileLifeTime = 3; rule.projectileScale = Vector3.one * .3f;
                rule.idleSprites = LoadFrames(entry.id, "Idle"); rule.moveSprites = LoadFrames(entry.id, "Move");
                rule.attackSprites = LoadFrames(entry.id, "Attack"); rule.deathSprites = LoadFrames(entry.id, "Death");
                prepared[asset].Add(rule);
            }
            // Resolve every required template and sprite before changing an asset.
            foreach (var pair in prepared)
            {
                Undo.RecordObject(pair.Key, "Configure biome normal enemies");
                pair.Key.enemySpawnRules = pair.Value;
                EditorUtility.SetDirty(pair.Key);
            }
            MonsterBalanceMigration.Run();
            AssetDatabase.SaveAssets();
            Debug.Log("[BiomeEnemySetup] 12 normal species updated; existing rules, definitions and tuning preserved.");
        }

        private static Sprite[] LoadFrames(string id, string action)
        {
            var result = new Sprite[4];
            for (int frame = 0; frame < result.Length; frame++)
            {
                string path = "Assets/_Project/Art/Images/Enemies/BiomeExclusive/" + id + "/" + action + "_" + frame + ".png";
                result[frame] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (result[frame] == null) throw new InvalidOperationException("스프라이트를 찾을 수 없습니다: " + path);
            }
            return result;
        }
    }
}
