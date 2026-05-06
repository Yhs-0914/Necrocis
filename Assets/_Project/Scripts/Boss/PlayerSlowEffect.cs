using System.Collections;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 플레이어에게 임시 이동속도 감소를 적용하는 헬퍼 컴포넌트.
    /// AddComponent 후 Apply()를 호출하면 duration 이후 자동 제거.
    /// </summary>
    public class PlayerSlowEffect : MonoBehaviour
    {
        public static void Apply(PlayerController pc, float slowRatio, float duration)
        {
            if (pc == null) return;
            PlayerSlowEffect effect = pc.gameObject.AddComponent<PlayerSlowEffect>();
            effect.StartCoroutine(effect.Run(pc, slowRatio, duration));
        }

        private IEnumerator Run(PlayerController pc, float slowRatio, float duration)
        {
            CharacterStatModifier mod = new CharacterStatModifier(
                CharacterStatType.MoveSpeed, -slowRatio,
                CharacterStatModifierMode.PercentAdd, this);
            pc.AddStatModifier(mod);

            yield return new WaitForSeconds(duration);

            if (pc != null)
                pc.RemoveStatModifiersFromSource(this);

            Destroy(this);
        }
    }
}
