using System;
using System.Collections;
using Necrocis;
using ProceduralMap;
using UnityEngine;
namespace NecrocisEditor
{
    public static partial class HelicoSpiralConnectionRunner
    {
        private static Vector3 FindPatch(int level,int clearance=7)
        {
            var map=biome.GetComponent<MapGenerator>();
            for(int z=clearance+2;z<biome.MapHeight-clearance-2;z++)for(int x=clearance+2;x<biome.MapWidth-clearance-2;x++)
            {
                if(biome.GetHeightLevel(x,z)!=level||map.IsCellReservedForBossArena(x,z))continue;bool clear=true;
                for(int dx=-clearance;dx<=clearance&&clear;dx++)for(int dz=-clearance;dz<=clearance&&clear;dz++)
                    clear=biome.IsWalkable(x+dx,z+dz)&&biome.GetHeightLevel(x+dx,z+dz)==level&&!map.IsCellReservedForBossArena(x+dx,z+dz);
                if(clear)return biome.GridToWorldWithHeight(x,z);
            }
            throw new InvalidOperationException("No natural Stomach patch at level "+level);
        }
        private static IEnumerator TerrainChecks()
        {
            var saved=origin;
            foreach(float direction in BattleAngles)
            {
                var aim=Aim(direction);Spawn();Move(origin+aim*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,2,"late obstacle tell");
                var cell=biome.WorldToGrid(origin+aim*2.8f);biome.AddRuntimeBlockedCells(new[]{cell});
                try
                {
                    Move(origin+aim*4);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"late obstacle stop");
                    Require(pattern.WallStopped&&pattern.Travelled<3&&pattern.TelegraphObject==null,"late obstacle stops and clears");
                    var delta=enemy.GetComponent<Rigidbody>().position-pattern.DashStart;delta.y=0;
                    Equal(0,Vector3.Cross(delta,aim).magnitude,"no diagonal wall sliding");Equal(100,health.CurrentHealth,"blocked target unharmed");
                    CheckStoppedOutsideCell(cell);
                    Pass($"WALL {direction:0}: late blocked tile stops full capsule outside cell, no slide and no through-wall damage");
                }
                finally{biome.RemoveRuntimeBlockedCells(new[]{cell});}
                Spawn();Move(origin+aim*1.3f);biome.AddRuntimeBlockedCells(new[]{cell});
                try
                {
                    yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,3,"forecast obstacle tell");
                    Require(pattern.PlannedDistance>0&&pattern.PlannedDistance<3.5f,"forecast clips to obstacle");float length=pattern.PlannedDistance;
                    Equal(0,Vector3.Distance(pattern.TelegraphObject.transform.position-pattern.DashStart-Vector3.up*.055f,aim*(length*.5f)),"clipped warning center");
                    Move(origin+Aim(direction+90)*3);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"forecast ends");
                    Equal(length,pattern.Travelled,"stops at forecast endpoint");CheckStoppedOutsideCell(cell);
                    Pass($"FORECAST {direction:0}: red footprint shortened to actual safe distance{length:F2}m before windup, exact endpoint");
                }
                finally{biome.RemoveRuntimeBlockedCells(new[]{cell});}
            }
            foreach(float direction in new[]{45f,135f,225f,315f})foreach(float edge in new[]{-.025f,.025f})
            {
                var aim=Aim(direction);var normal=Vector3.Cross(aim,Vector3.up);var blocked=biome.WorldToGrid(saved+Vector3.right*3);
                var center=biome.GridToWorldWithHeight(blocked.x,blocked.y);var corner=center+new Vector3(Mathf.Sign(normal.x),0,Mathf.Sign(normal.z))*(biome.TileSize*.5f);
                var tangent=corner+normal*(settings.bodyWidth*.5f+edge);origin=tangent-aim*1.5f;
                Spawn();Move(origin+aim*1.3f);biome.AddRuntimeBlockedCells(new[]{blocked});
                try
                {
                    yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,3,"corner forecast");float length=pattern.PlannedDistance;
                    Require(edge<0?length<3.5f:Mathf.Abs(length-3.5f)<.01f,"rounded corner prediction");
                    Move(origin+normal*3);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"corner charge");
                    Equal(length,pattern.Travelled,"corner forecast matches movement");CheckStoppedOutsideCell(blocked);
                    Pass($"CORNER {direction:0}/{edge:+.000;-.000}: full capsule {(edge<0?"stops":"passes")} at rounded corner, no square over-expansion");
                }
                finally{biome.RemoveRuntimeBlockedCells(new[]{blocked});}
            }
            origin=saved;
            foreach(int level in new[]{0,1})foreach(float direction in new[]{0f,45f,270f})
            {
                origin=FindPatch(level,6);Spawn();Move(origin+Aim(direction)*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,2,"height warning");
                Equal(biome.GetGroundHeight(origin)+.055f,pattern.TelegraphObject.transform.position.y,"height warning plane");
                Move(origin+Aim(direction+90)*3);yield return Wait(()=>pattern.DashCount==1,2,"height dash");
                while(pattern.Phase==HelicoSpiralPhase.Dash)
                {
                    yield return new WaitForEndOfFrame();CheckGround();Equal(level*biome.HeightStep,enemy.GetComponent<Rigidbody>().position.y,"body follows natural ground");
                }
                Equal(3.5f,pattern.Travelled,"height full dash");
                Pass($"HEIGHT {level}/{direction:0}: natural ground{level*biome.HeightStep:F2}m, full dash, footprint and body underside stay grounded");
            }
            yield return LedgeChecks();origin=saved;
        }
        private static void CheckStoppedOutsideCell(Vector2Int cell)
        {
            var p=enemy.GetComponent<Rigidbody>().position;var b=new Bounds(biome.GridToWorld(cell.x,cell.y),new Vector3(biome.TileSize,1,biome.TileSize));
            float squared=CombatHitGeometry.SegmentRectDistanceSquared(CombatHitGeometry.Flat(p-pattern.LockedDirection*pattern.BodyHalfLength),CombatHitGeometry.Flat(p+pattern.LockedDirection*pattern.BodyHalfLength),b);
            Require(squared>=pattern.BodyRadius*pattern.BodyRadius-.001f,"whole body stays outside blocked cell");
        }
        private static IEnumerator LedgeChecks()
        {
            bool found=false;Vector2Int boundary=default;
            var map=biome.GetComponent<MapGenerator>();
            for(int z=4;z<biome.MapHeight-4&&!found;z++)for(int x=6;x<biome.MapWidth-7&&!found;x++)
            {
                if(biome.GetHeightLevel(x,z)!=0||biome.GetHeightLevel(x+1,z)!=1)continue;bool clear=true;
                for(int dx=-5;dx<=6&&clear;dx++)for(int dz=-2;dz<=2&&clear;dz++)
                    clear=biome.IsWalkable(x+dx,z+dz)&&biome.GetHeightLevel(x+dx,z+dz)==(dx<=0?0:1)&&!map.IsCellReservedForBossArena(x+dx,z+dz);
                if(clear){boundary=new Vector2Int(x,z);found=true;}
            }
            Require(found,"natural Stomach low/high boundary");
            foreach(bool up in new[]{true,false})
            {
                var aim=up?Vector3.right:Vector3.left;origin=biome.GridToWorldWithHeight(boundary.x+(up?-2:3),boundary.y);
                Spawn();Move(origin+aim*1.3f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,3,"ledge approach");
                Require(pattern.PlannedDistance<3.5f,"ledge clips forecast");float length=pattern.PlannedDistance;
                Move(biome.GridToWorldWithHeight(boundary.x+(up?4:-3),boundary.y));health.ResetHealth();
                yield return Wait(()=>pattern.DashCount==1,2,"ledge release");
                while(pattern.Phase==HelicoSpiralPhase.Dash){yield return new WaitForEndOfFrame();CheckGround();}
                Equal(length,pattern.Travelled,"ledge matches forecast");var at=biome.WorldToGrid(enemy.GetComponent<Rigidbody>().position);
                Require(biome.GetHeightLevel(at.x,at.y)==(up?0:1),"cannot cross height by dash");Equal(100,health.CurrentHealth,"no cross-height hit");
                Pass(up?"LEDGE up: real higher terrain clips red forecast and stops dash before rising, target unharmed":"LEDGE down: real lower terrain clips red forecast and stops at edge, no hovering/cross-height hit");
            }
        }
    }
}
