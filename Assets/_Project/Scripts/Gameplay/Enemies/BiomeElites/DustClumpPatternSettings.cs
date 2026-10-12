using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Dust Clump Pattern")]
    public sealed class DustClumpPatternSettings : MonsterPatternSettings
    {
        public const string DamageId = "dust-cloud";
        [Min(.1f)] public float triggerDistance = 4.5f;
        [Min(0)] public float spawnGraceSeconds = .75f;
        [Min(.1f)] public float windupSeconds = .9f;
        [Min(.1f)] public float launchOffset = 1.2f;
        [Tooltip("The full red circle and player ground-center damage share this radius.")]
        [Min(.1f)] public float cloudRadius = 1.1f;
        [Min(.1f)] public float cloudSpeed = 2f;
        [Min(.1f)] public float cloudLifetimeSeconds = 2f;
        [Min(.1f)] public float recoverySeconds = .8f;
        [Min(0)] public float rearmSeconds = 3.5f;
        public DustClumpPresentation presentation;

        public override string GetValidationError(MonsterDefinition definition)
        {
            foreach (float value in new[] { triggerDistance, windupSeconds, launchOffset, cloudRadius, cloudSpeed, cloudLifetimeSeconds, recoverySeconds })
                if (!MonsterBalanceNumbers.Positive(value)) return "Dust distances/times must be positive.";
            if (!MonsterBalanceNumbers.NonNegative(spawnGraceSeconds) || !MonsterBalanceNumbers.NonNegative(rearmSeconds)) return "Invalid dust cooldown.";
            if (definition.tier != MonsterTier.Elite || !definition.patternDamage.Exists(p => p != null && p.id == DamageId)) return "Elite tier and dust-cloud coefficient required.";
            return presentation != null ? presentation.GetValidationError() : "Dust presentation required.";
        }
        public override void Attach(EnemyController enemy)
        {
            var pattern = enemy.GetComponent<DustClumpElitePattern>() ?? enemy.gameObject.AddComponent<DustClumpElitePattern>();
            pattern.Initialize(enemy, this);
        }
    }
}
