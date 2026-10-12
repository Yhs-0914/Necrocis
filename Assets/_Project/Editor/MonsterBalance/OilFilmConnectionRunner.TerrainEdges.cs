using System.Collections;
using Necrocis;
using UnityEngine;
namespace NecrocisEditor
{
    public static partial class OilFilmConnectionRunner
    {
        private static IEnumerator TerrainEdgeChecks()
        {
            foreach(float direction in new[]{0f,90f,180f,270f})foreach(int sign in new[]{-1,1})
            {
                Spawn();Move(origin+Aim(direction)*1.6f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"corner tell");
                var f=pattern.LockedDirection;var side=pattern.Footprint.Right*sign;var cell=biome.WorldToGrid(origin+f+side);biome.AddRuntimeBlockedCells(new[]{cell});
                try
                {
                    yield return null;yield return new WaitForEndOfFrame();CheckTerrainMesh();var shadow=origin+(f+side).normalized*2.475f;
                    Require(!pattern.Footprint.Contains(shadow),"corner casts damage shadow");Move(shadow);health.ResetHealth();
                    yield return Wait(()=>pattern.ImpactCount==1,3,"corner impact");yield return new WaitForEndOfFrame();CheckSurface();Equal(100,health.CurrentHealth,"no diagonal corner damage");
                    Pass($"CORNER {direction}/side{sign}: diagonal obstacle shadow clips warning/visuals/damage, no tile-corner bypass");
                }
                finally{biome.RemoveRuntimeBlockedCells(new[]{cell});}
            }
            foreach(var cell in new[]{new Vector2Int(-1,10),new Vector2Int(biome.MapWidth,10),new Vector2Int(10,-1),new Vector2Int(10,biome.MapHeight)})
            {
                var invalid=biome.GridToWorld(cell.x,cell.y);var g=new OilFilmSector(invalid,Vector3.right,2.5f,110);
                Equal(0,g.MaximumReach,"invalid map origin has no footprint");Require(!g.Contains(invalid)&&!g.Contains(invalid+Vector3.right),"invalid origin cannot affect target");
                Pass("MAP BOUNDS "+cell+": invalid map origin produces no damage area");
            }
            Spawn();Move(origin+Vector3.right*20);yield return new WaitForSeconds(1);Require(pattern.ImpactCount==0&&pattern.TelegraphObject==null,"outside chase/leash cannot trigger");
            Pass("LEASH: distant player cannot trigger unseen/leashed attack");
        }
    }
}
