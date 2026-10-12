using System;
using System.Collections;
using Necrocis;
using ProceduralMap;
using UnityEngine;
using Object=UnityEngine.Object;
namespace NecrocisEditor
{
    public static partial class OilFilmConnectionRunner
    {
        private static Vector3 FindPatch(int level,int clearance=7)
        {
            var map=biome.GetComponent<MapGenerator>();
            for(int z=clearance+2;z<biome.MapHeight-clearance-2;z++)for(int x=clearance+2;x<biome.MapWidth-clearance-2;x++)
            {
                if(biome.GetHeightLevel(x,z)!=level||map.IsCellReservedForBossArena(x,z))continue;bool clear=true;
                for(int dx=-clearance;dx<=clearance&&clear;dx++)for(int dz=-clearance;dz<=clearance&&clear;dz++)clear=biome.IsWalkable(x+dx,z+dz)&&biome.GetHeightLevel(x+dx,z+dz)==level&&!map.IsCellReservedForBossArena(x+dx,z+dz);
                if(clear)return biome.GridToWorldWithHeight(x,z);
            }
            throw new InvalidOperationException("No natural Stomach patch at level "+level);
        }
        private static void CheckTerrainMesh()
        {
            var g=pattern.Footprint;Require(g!=null,"terrain footprint");
            for(int i=0;i<=64;i++)foreach(float r in new[]{.25f,.5f,.75f,.98f})
            {
                Vector3 p=g.Point(i/64f,r);Require(GasSacOrb.HasClearPath(g.Origin,p),"warning sample crosses wall/height");
                Equal(biome.GetGroundHeight(g.Origin),biome.GetGroundHeight(p),"warning stays on source height");
            }
            if(pattern.TelegraphObject!=null)CheckWarning();
        }
        private static IEnumerator TerrainChecks()
        {
            var saved=origin;
            foreach(float direction in BattleAngles)
            {
                Spawn();Move(origin+Aim(direction)*2);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"late wall tell");var f=pattern.LockedDirection;
                var cell=biome.WorldToGrid(origin+f*1.2f);biome.AddRuntimeBlockedCells(new[]{cell});GameObject cube=null;
                try
                {
                    yield return null;yield return new WaitForEndOfFrame();Require(!pattern.Footprint.Contains(origin+f*2),"late wall clips shared footprint");CheckTerrainMesh();Require(enemy.GetComponent<OilFilmVisual>().IsSurfaceVisible,"warning membrane also clips without squeezing");
                    if(direction==0){cube=DebugWall(cell);Capture("wall-warning");}
                    Move(origin+f*2.1f);health.ResetHealth();yield return Wait(()=>pattern.ImpactCount==1,3,"clipped impact");yield return new WaitForEndOfFrame();CheckSurface();CheckTerrainMesh();Equal(100,health.CurrentHealth,"wall protects");
                    if(direction==0){Capture("wall-impact"); CheckGpuTerrainClip(true,"wall-clip-mask");}float radius=pattern.Footprint.RadiusAtAngle(.5f);biome.RemoveRuntimeBlockedCells(new[]{cell});yield return null;
                    Equal(radius,pattern.Footprint.RadiusAtAngle(.5f),"removing wall never expands committed area");
                    Pass($"LATE WALL {direction}: warning/visible membrane/damage clipped, no through-wall hit or later expansion");
                }
                finally{biome.RemoveRuntimeBlockedCells(new[]{cell});if(cube!=null)Object.Destroy(cube);}
                if(direction%90!=0)continue;
                Spawn();f=OilFilmElitePattern.FacingDirection((EnemyFacing)(Mathf.RoundToInt(direction/90)*2%8),DontStarveCamera.GetActiveCamera());
                var wall=biome.WorldToGrid(origin+f*2);biome.AddRuntimeBlockedCells(new[]{wall});
                try
                {
                    Move(origin+f*1);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"static wall tell");Require(pattern.Footprint.TerrainClipped,"initial clipping");CheckTerrainMesh();
                    Move(origin+f*2.3f);yield return Wait(()=>pattern.ImpactCount==1,3,"static wall impact");Equal(100,health.CurrentHealth,"static shadow protects");
                    Pass($"STATIC WALL {direction}: initial red area stops at blocked tile, damage shadow respected");
                }
                finally{biome.RemoveRuntimeBlockedCells(new[]{wall});}
            }
            foreach(int level in new[]{0,1})foreach(float direction in new[]{0f,90f,270f})
            {
                origin=FindPatch(level,6);Spawn();Move(origin+Aim(direction)*1.6f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"height tell");CheckTerrainMesh();
                Equal(biome.GetGroundHeight(origin)+.035f,pattern.TelegraphObject.transform.position.y,"local warning height");
                yield return Wait(()=>pattern.ImpactCount==1,3,"height impact");yield return new WaitForEndOfFrame();CheckSurface();Equal(3,100-health.CurrentHealth,"height damage");
                if(level==1&&direction==270)Capture("high-ground");Pass($"HEIGHT {level}/{direction}: warning and membrane grounded at natural height, actual damage3");
            }
            bool found=false;Vector2Int boundary=default;var map=biome.GetComponent<MapGenerator>();
            for(int z=4;z<biome.MapHeight-4&&!found;z++)for(int x=6;x<biome.MapWidth-7&&!found;x++)
            {
                if(biome.GetHeightLevel(x,z)!=0||biome.GetHeightLevel(x+1,z)!=1)continue;bool clear=true;
                for(int dx=-1;dx<=2&&clear;dx++)clear=biome.IsWalkable(x+dx,z)&&biome.GetHeightLevel(x+dx,z)==(dx<=0?0:1)&&!map.IsCellReservedForBossArena(x+dx,z);
                if(clear){boundary=new Vector2Int(x,z);found=true;}
            }
            Require(found,"natural low/high boundary");
            foreach(bool up in new[]{true,false})
            {
                var aim=up?Vector3.right:Vector3.left;origin=biome.GridToWorldWithHeight(boundary.x+(up?-1:2),boundary.y);Spawn();Move(origin+aim*1);
                yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"ledge tell");Require(pattern.Footprint.RadiusAtAngle(.5f)<2,"ledge clips range");CheckTerrainMesh();
                Capture(up?"ledge-up-warning":"ledge-down-warning");Move(origin+aim*2.2f);health.ResetHealth();yield return Wait(()=>pattern.ImpactCount==1,3,"ledge impact");yield return new WaitForEndOfFrame();CheckSurface();CheckTerrainMesh();Equal(100,health.CurrentHealth,"no cross-height damage");
                CheckGpuTerrainClip(false,up?"ledge-up-mask":"ledge-down-mask");Capture(up?"ledge-up-impact":"ledge-down-impact");Pass("LEDGE "+(up?"up":"down")+": actual step clips all three representations; other height stays unharmed");
            }
            origin=saved;
        }
        private static GameObject DebugWall(Vector2Int cell)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="S03_A3_TestWall";go.GetComponent<Collider>().enabled=false;
            go.transform.position=biome.GridToWorldWithHeight(cell.x,cell.y)+Vector3.up*.5f;go.transform.localScale=new Vector3(biome.TileSize*.98f,1,biome.TileSize*.98f);
            go.GetComponent<Renderer>().material.color=new Color(.3f,.29f,.34f);return go;
        }
    }
}
