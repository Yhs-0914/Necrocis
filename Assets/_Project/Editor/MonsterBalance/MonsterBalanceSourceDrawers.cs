using System.Collections.Generic;
using Necrocis;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NecrocisEditor
{
    [CustomPropertyDrawer(typeof(EnemySpawnRuleConfig))]
    public sealed class EnemySpawnRuleBalanceDrawer : PropertyDrawer
    {
        private static readonly HashSet<string> Legacy = new HashSet<string>
        {
            "moveSpeed", "maxHealth", "attackDamage", "expReward",
            "enableContactDamage", "contactDamage", "contactKnockbackDistance"
        };

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new Foldout { text = property.displayName, value = property.isExpanded };
            void Rebuild()
            {
                root.Clear();
                bool bound = property.FindPropertyRelative("monsterDefinition").objectReferenceValue != null;
                if (bound) root.Add(new HelpBox("기본 능력치/보상/접촉/피해 계수는 Monster Definition에서 조절합니다. Additional Base Stats에 HP·공격력·이동속도를 중복 등록하면 생성 오류가 발생합니다.", HelpBoxMessageType.Info));
                var hidden = bound ? new HashSet<string>(Legacy) : new HashSet<string>();
                var definition = property.FindPropertyRelative("monsterDefinition").objectReferenceValue as MonsterDefinition;
                if (definition != null && (definition.pattern is GasSacPatternSettings || definition.pattern is HardenedResiduePatternSettings
                    || definition.pattern is InflammationEmberPatternSettings || definition.pattern is HangoverRemnantPatternSettings || definition.pattern is DustClumpPatternSettings
                    || definition.pattern is PollenInvaderPatternSettings || definition.pattern is HelicoSpiralPatternSettings
                    || definition.pattern is OilFilmPatternSettings))
                {
                    hidden.UnionWith(new[] { "attackRange", "attackCooldown", "isRanged", "projectileSpeed", "projectileLifeTime",
                        "projectileSprite", "projectileScale", "projectileSpawnOffset", "expandColliderOnAttack", "attackColliderSize",
                        "attackColliderCenter", "idleSprites", "idleSpritesUp", "idleSpritesDown", "moveSprites", "moveSpritesUp",
                        "moveSpritesDown", "attackSprites", "attackSpritesUp", "attackSpritesDown", "attackAnimationSpeed", "animationSpeed", "deathSprites", "deathAnimationSpeed",
                        "chargesAtPlayer", "chargeSpeed", "chargeAccelTime", "stoppingDistance", "wanderRadius", "idleDelayRange", "separationDistance", "separationStrength" });
                    root.Add(new HelpBox("전용 패턴의 시간·범위·프레임은 Monster Definition → Pattern에서 조절합니다. 기본 FSM 공격 설정은 사용하지 않습니다.", HelpBoxMessageType.Info));
                }
                if (property.serializedObject.targetObject is BiomeEliteSpawnConfig)
                {
                    hidden.UnionWith(new[] { "density", "minDistance", "allowedRegions", "maxAlive", "activationRadius",
                        "respawnCooldown", "spawnRadius", "leashRadius", "isElite", "killTriggerEnemyName", "killTriggerCount" });
                    root.Add(new HelpBox("바이옴 엘리트: 배치 수·간격·활성화·복귀 범위는 상위 맵 배치 설정에서 조절합니다. 처치 수 조건은 사용하지 않습니다.", HelpBoxMessageType.Info));
                }
                if (definition != null && definition.pattern is HelicoSpiralPatternSettings)
                    root.Add(new HelpBox("나선충 Collider Size/Center는 플레이어 공격을 받는 몸체입니다. 플레이어에게 주는 접촉·돌진 판정은 Pattern의 몸통 폭/반길이를 사용합니다.", HelpBoxMessageType.Info));
                AddChildren(root, property, hidden);
            }
            Rebuild();
            root.TrackPropertyValue(property.FindPropertyRelative("monsterDefinition"), _ => Rebuild());
            return root;
        }

        internal static void AddChildren(VisualElement root, SerializedProperty parent, HashSet<string> hidden)
        {
            SerializedProperty child = parent.Copy();
            SerializedProperty end = parent.GetEndProperty();
            if (!child.NextVisible(true)) return;
            do
            {
                if (SerializedProperty.EqualContents(child, end)) break;
                if (hidden == null || !hidden.Contains(child.name)) root.Add(new PropertyField(child.Copy()));
            } while (child.NextVisible(false));
        }
    }

    [CustomPropertyDrawer(typeof(MidBossDefinition))]
    public sealed class MidBossBalanceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new Foldout { text = property.displayName, value = true };
            void Rebuild()
            {
                root.Clear();
                var hidden = new HashSet<string> { "intestinePattern", "liverPattern", "stomachPattern", "lungPattern" };
                var type = (MidBossPatternType)property.FindPropertyRelative("patternType").enumValueIndex;
                switch (type)
                {
                    case MidBossPatternType.Intestine: hidden.Remove("intestinePattern"); break;
                    case MidBossPatternType.Liver: hidden.Remove("liverPattern"); break;
                    case MidBossPatternType.Stomach: hidden.Remove("stomachPattern"); break;
                    case MidBossPatternType.Lung: hidden.Remove("lungPattern"); break;
                }
                var reference = property.FindPropertyRelative("bossRule.monsterDefinition");
                if (reference != null && reference.objectReferenceValue != null)
                {
                    hidden.UnionWith(new[] { "overrideStats", "maxHealthMultiplier", "attackDamageMultiplier", "moveSpeedMultiplier", "minimumMaxHealth" });
                    root.Add(new HelpBox("페이즈 능력치는 Boss Rule → Monster Definition의 Default/Phase2가 원본입니다. 패턴 설정에는 시간·범위·이동 동작을 둡니다.", HelpBoxMessageType.Info));
                }
                EnemySpawnRuleBalanceDrawer.AddChildren(root, property, hidden);
            }
            Rebuild();
            root.TrackPropertyValue(property.FindPropertyRelative("patternType"), _ => Rebuild());
            var definition = property.FindPropertyRelative("bossRule.monsterDefinition");
            if (definition != null) root.TrackPropertyValue(definition, _ => Rebuild());
            return root;
        }
    }
}
