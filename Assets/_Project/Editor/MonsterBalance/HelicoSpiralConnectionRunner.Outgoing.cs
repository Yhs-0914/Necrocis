using System.Collections;
using System.Linq;
using Necrocis;
using UnityEngine;
using Object=UnityEngine.Object;
namespace NecrocisEditor
{
    public static partial class HelicoSpiralConnectionRunner
    {
        private static IEnumerator OutgoingChecks()
        {
            Set(settings,"spawnGraceSeconds",100);float shotRadius=0;
            foreach(float direction in BattleAngles)
            {
                Spawn();enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth,200,true);Move(origin-Aim(direction)*4);Face(Aim(direction));Physics.SyncTransforms();
                typeof(PlayerAttack).GetMethod("RangedAttack",Private).Invoke(player.GetComponent<PlayerAttack>(),null);
                var shot=Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Single(p=>p.gameObject.activeSelf);
                shotRadius=(float)typeof(Projectile).GetMethod("GetScaledHitCheckRadius",Private).Invoke(shot,null);
                yield return Wait(()=>enemy.Stats.CurrentHealth<200,1.5f,"native W hits new receiving box");
                Require(shotRadius>PlayerAttackGeometry.Padding,"native projectile radius includes padding");
                foreach(var p in Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None))p.gameObject.SetActive(false);
                yield return new WaitForFixedUpdate();yield return null;
                Pass($"PLAYER W {direction:0}: actual native projectile hits S-01 receiving box, no height miss; radius including padding={shotRadius:F3}m");
            }
            foreach(float direction in new[]{0f,45f})foreach(float edge in new[]{-.03f,.03f})
            {
                Spawn();enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth,200,true);var aim=Aim(direction);var side=Vector3.Cross(aim,Vector3.up);
                var bounds=CombatHitGeometry.EnemyBodyBounds(enemy);
                float reach=Mathf.Abs(side.x)*bounds.extents.x+Mathf.Abs(side.z)*bounds.extents.z;
                Move(origin-aim*4+side*(reach+shotRadius+edge));Face(aim);Physics.SyncTransforms();
                typeof(PlayerAttack).GetMethod("RangedAttack",Private).Invoke(player.GetComponent<PlayerAttack>(),null);
                var shot=Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Single(p=>p.gameObject.activeSelf);
                var launch=shot.transform.position;var actualDirection=(Vector3)typeof(Projectile).GetField("moveDirection",Private).GetValue(shot);
                string lastDamage="none";
                System.Action<EnemyController,float> observe=(e,amount)=>lastDamage=$"amount={amount}, shot={shot.transform.position}, enemy={e.GetComponent<Rigidbody>().position}, hits={typeof(Projectile).GetField("currentHitCount",Private).GetValue(shot)}, radius={typeof(Projectile).GetMethod("GetScaledHitCheckRadius",Private).Invoke(shot,null)}";
                enemy.DamageTaken+=observe;
                try{yield return new WaitForSeconds(.65f);}
                finally{enemy.DamageTaken-=observe;}
                Require(edge<0?enemy.Stats.CurrentHealth<200:enemy.Stats.CurrentHealth==200,$"native W edge {direction}/{edge}: HP={enemy.Stats.CurrentHealth}, reach={reach}, radius={shotRadius}, hero={player.transform.position}, launch={launch}, aim={actualDirection}, {lastDamage}");
                foreach(var p in Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None))p.gameObject.SetActive(false);
                yield return new WaitForFixedUpdate();yield return null;
                Pass($"PLAYER W EDGE {direction:0}/{edge:+.00;-.00}: actual shot {(edge<0?"hits":"misses")} generous body edge, including diagonal rounded corner");
            }
            Restore();
        }
    }
}
