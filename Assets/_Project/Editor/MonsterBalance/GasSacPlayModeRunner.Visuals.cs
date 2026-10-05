using System.Collections;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class GasSacPlayModeRunner
    {
        private static IEnumerator FacingAndAreaChecks()
        {
            Spawn(); enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
            var body = enemy.transform.Find("Visual").GetComponent<SpriteRenderer>();
            MovePlayer(3.5f); yield return null;
            Require(body.flipX, "gas sac mirrors toward a player on the right while ready");
            MovePlayer(-3.5f); yield return null;
            Require(!body.flipX, "gas sac mirrors toward a player on the left while ready");
            MovePlayer(2.3f);
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 3, "right-facing A tell");
            Require(body.flipX, "A commits facing right");
            ElitePresentationChecks.RedArea(pattern.TelegraphObject, Vector2.one * settings.burstRadius * 2);
            yield return new WaitForSeconds(.15f);
            ElitePresentationChecks.RedArea(pattern.TelegraphObject, Vector2.one * settings.burstRadius * 2);
            Pass("A danger background is translucent red and covers the full 3.25m radius from the beginning; no collider added");
            MovePlayer(-3.5f); yield return null;
            Require(body.flipX, "crossing behind during A tell cannot turn the committed attack");
            yield return Wait(() => pattern.Phase == GasSacPhase.Recovery, 3, "A facing recovery");
            Require(body.flipX, "A keeps the same mirrored side through recovery");
            yield return Wait(() => pattern.Phase == GasSacPhase.Ready, 3, "A facing ready"); yield return null;
            Require(!body.flipX, "A turns toward the opposite player after recovery");
            Spawn();
            Require(enemy.transform.Find("Visual").GetComponent<SpriteRenderer>().flipX, "pool reuse refreshes facing from current player position");
            Pass("gas sac switches both ways while ready, locks facing through A, and resets correctly on reuse");
        }
    }
}
