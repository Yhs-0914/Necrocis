using System;
using System.Collections;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NecrocisEditor
{
    public static partial class HelicoSpiralConnectionRunner
    {
        private static void SetTailEnabled(bool value)
        {
            if(!backups.ContainsKey(settings))backups.Add(settings,EditorJsonUtility.ToJson(settings));
            using var so=new SerializedObject(settings);so.FindProperty("tailSweepEnabled").boolValue=value;so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static IEnumerator BeginTail(float direction)
        {
            Spawn();Move(origin+Aim(direction)*2.6f);
            yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,3,"tail dash windup");
            Move(origin+Aim(direction+90)*3);
            float until=Time.time+3;
            while(pattern.Phase!=HelicoSpiralPhase.TailWindup)
            {
                Require(Time.time<until,$"tail warning timeout: phase={pattern.Phase},wall={pattern.WallStopped},planned={pattern.PlannedDistance:R},travelled={pattern.Travelled:R},enabled={pattern.TailEnabled},stunned={enemy.StatusEffects.IsStunned}");
                yield return null;
            }
        }
        private static void CheckTailGround()
        {
            CheckGround();var g=pattern.TailGeometry;Require(g!=null,"tail geometry");
            if(!pattern.TailActionVisible)return;
            var v=enemy.GetComponent<HelicoTailSweepVisual>();Require(v.IsShowing&&!Body().enabled,"one body and two airborne flags");
            Equal(0,Vector3.Distance(v.VisibleTip,g.AirPoint(pattern.TailProgress,1)),"visible airborne tip matches trajectory");
            Require(v.VisibleTip.y-g.Origin.y>.55f,"tail tip stays above floor");
            foreach(int flag in new[]{0,1})
            {
                var line=enemy.GetComponentsInChildren<LineRenderer>().Single(l=>l.name=="Flag"+flag+"Layer0");
                for(int j=0;j<=HelicoTailSweepGeometry.CurveSegments;j++)
                {
                    float along=j/(float)HelicoTailSweepGeometry.CurveSegments;
                    Vector3 point=line.GetPosition(8+j),expected=g.AirPoint(pattern.TailProgress,along,flag);
                    Equal(0,Vector3.Distance(point,expected),"whole visible curved tail matches motion");
                    Equal(0,Vector2.Distance(CombatHitGeometry.Flat(point),CombatHitGeometry.Flat(g.CurvePoint(pattern.TailProgress,along,flag))),"vertical projection matches damage path");
                    Require(point.y-g.Origin.y>=.55f,"entire active tail stays airborne");
                    Require(g.Contains(point),"projected curve stays inside red warning");
                }
                Vector3 midpoint=g.AirPoint(pattern.TailProgress,.5f,flag);
                Require(Vector3.Distance(midpoint,Vector3.Lerp(g.AirPoint(pattern.TailProgress,0,flag),g.AirPoint(pattern.TailProgress,1,flag),.5f))>.20f,"broad curved shape, never a straight spoke");
            }
        }
        private static IEnumerator TailChecks()
        {
            if(preview)
            {
                if(phase=="body-motion"){yield return RecordOriginalBodyMotion();yield break;}
                yield return BeginTail(angle);
                if(phase=="motion"){yield return RecordTailMotion();yield break;}
                if(phase=="tell"){yield return new WaitForSeconds(.45f);yield break;}
                Move(enemy.GetComponent<Rigidbody>().position+Aim(angle+90)*2.25f);health.GrantTemporaryInvincibility(8);
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.TailSweep,2,"preview sweep");
                float target=phase=="start"?.05f:phase=="end"?.87f:.45f;
                yield return Wait(()=>pattern.TailProgress>=target,1,"preview action fraction");
                yield return new WaitForEndOfFrame();CheckTailGround();yield break;
            }
            Require(settings.tailSweepEnabled&&definition.GetValidationError()==null,"valid enabled tail source");
            var art=settings.presentation;Equal(130,art.groundings.Length,"130 sprite groundings");Equal(15,art.tailBodies.Length,"15 split body meshes");
            var catalog=Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath);var editor=Editor.CreateEditor(catalog);
            try
            {
                typeof(MonsterBalanceCatalogEditor).GetField("monsterIndex",Private).SetValue(editor,catalog.monsters.IndexOf(definition));
                var paths=editor.CreateInspectorGUI().Query<PropertyField>().ToList().Select(f=>f.bindingPath).ToArray();
                foreach(string key in new[]{"tailSweepEnabled","tailReach","tailArcDegrees","tailWidth","tailWindupSeconds","tailRecoverySeconds"})Require(paths.Contains(key),"Inspector "+key);
            }
            finally{UnityEngine.Object.DestroyImmediate(editor);}
            Pass("SOURCE: tail3/3/2,40 sprites,130 groundings,15 active body meshes; editable B fields, unchanged HP30/XP50");
            foreach(float direction in BattleAngles)
            {
                yield return BeginTail(direction);var g=pattern.TailGeometry;var facing=enemy.PatternFacing;
                Require(Vector3.Angle(g.Rear,-Aim(direction))<.01f,"rear direction");
                Equal(.75f,Vector3.Distance(CombatHitGeometry.Flat(g.Origin),CombatHitGeometry.Flat(enemy.GetComponent<Rigidbody>().position)),"body-derived rear root");
                var mesh=pattern.TelegraphObject.GetComponent<MeshFilter>().sharedMesh;
                foreach(var vertex in mesh.vertices)Require(g.Contains(pattern.TelegraphObject.transform.TransformPoint(vertex)),"red boundary inside shared footprint");
                Move(g.Origin+g.Rear*.8f);health.ResetHealth();float began=Time.time;
                while(pattern.Phase==HelicoSpiralPhase.TailWindup){Equal(100,health.CurrentHealth,"no warning damage");yield return new WaitForEndOfFrame();CheckTailGround();}
                Require(Time.time-began>.8f,"readable tail warning");
                while(pattern.Phase==HelicoSpiralPhase.TailSweep){Require(enemy.PatternFacing==facing&&enemy.IsPatternFacingLocked,"tail facing locked");yield return new WaitForEndOfFrame();CheckTailGround();}
                Equal(97,health.CurrentHealth,"rear hit damage3");Equal(1,pattern.HitAttempts,"cycle single attempt");Equal(1,pattern.TailHitAttempts,"one tail attempt");
                Require(pattern.TailCompleted&&pattern.TelegraphObject==null&&Body().enabled&&!enemy.GetComponent<HelicoTailSweepVisual>().IsShowing,"recovery removes area and restores sprite");
                Move(enemy.transform.position+Aim(direction+90)*3);
                yield return new WaitForSeconds(1.0f);Require(pattern.Phase==HelicoSpiralPhase.Recovery,"long final opening");
                Pass($"ALIGN/HIT {direction}: full airborne curves project inside warning; curved sweep damage; rear damage3 once; locked view and final recovery");
            }
            if(phase=="alignment"){enemy.ReleaseToPool();yield break;}
            foreach(float direction in BattleAngles)foreach(bool inside in new[]{true,false})
            {
                yield return BeginTail(direction);var g=pattern.TailGeometry;
                Move(g.Origin+g.Rear*(g.Reach+g.Radius+(inside?-.03f:.03f)));health.ResetHealth();
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"outer edge result");
                Equal(inside?97:100,health.CurrentHealth,"outer rim fair edge");Pass($"RADIAL EDGE {direction} {(inside?"inside":"outside")}: +/-0.03m of displayed rim gives {(inside?3:0)} damage");
            }
            foreach(float direction in BattleAngles)foreach(int side in new[]{-1,1})foreach(bool inside in new[]{true,false})
            {
                yield return BeginTail(direction);var g=pattern.TailGeometry;
                Vector3 ray=g.Direction(side<0?0:1),normal=Vector3.Cross(Vector3.up,ray)*side;
                Move(g.Origin+ray*.85f+normal*(g.Radius+(inside?-.03f:.03f)));health.ResetHealth();
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"angular edge result");
                Equal(inside?97:100,health.CurrentHealth,"angular edge result");Pass($"ANGLE EDGE {direction}/{side}/{inside}: +/-0.03m of side boundary gives {(inside?3:0)} damage");
            }
            foreach(float direction in new[]{0f,45f,90f})
            {
                Spawn();Move(origin+Aim(direction)*1.5f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.TailWindup,3,"dash-hit combo");
                Equal(1,pattern.HitAttempts,"dash consumed budget");var g=pattern.TailGeometry;Move(g.Origin+g.Rear*.8f);health.ResetHealth();
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"shared hit budget");Equal(100,health.CurrentHealth,"no second damage after resetting immunity");
                Equal(0,pattern.TailHitAttempts,"no second tail attempt");Pass($"SHARED BUDGET {direction}: dash hit prevents tail hit even after immunity reset");
            }
            foreach(var state in new[]{HelicoSpiralPhase.TailWindup,HelicoSpiralPhase.TailSweep})
            {
                yield return BeginTail(0);if(state==HelicoSpiralPhase.TailSweep)yield return Wait(()=>pattern.Phase==state,2,"stun active");
                enemy.StatusEffects.ApplyStun(1);yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();
                Require(pattern.TelegraphObject==null&&!enemy.GetComponent<HelicoTailSweepVisual>().IsShowing&&!pattern.TailCompleted,"tail stun cancels");Equal(100,health.CurrentHealth,"cancel no damage");
                Pass("STUN "+state+": area/action cleared, basic recovery");
                yield return BeginTail(90);if(state==HelicoSpiralPhase.TailSweep)yield return Wait(()=>pattern.Phase==state,2,"death active");
                enemy.TakeDamage(10000);yield return new WaitForEndOfFrame();
                Require(pattern.TelegraphObject==null&&!enemy.GetComponent<HelicoTailSweepVisual>().IsShowing&&Body().enabled,"death restores original art and clears attack");
                Equal(100,health.CurrentHealth,"death no damage");Pass("DEATH "+state+": common directional death visible, no late tail damage");
                yield return BeginTail(45);if(state==HelicoSpiralPhase.TailSweep)yield return Wait(()=>pattern.Phase==state,2,"pool active");
                enemy.ReleaseToPool();yield return null;Require(pattern.Phase==HelicoSpiralPhase.Inactive&&pattern.TelegraphObject==null&&!enemy.GetComponent<HelicoTailSweepVisual>().IsShowing,"pool clears tail");
                Pass("POOL "+state+": coroutine, area and ground flags cleaned");
            }
            foreach(float direction in new[]{0f,90f,225f})
            {
                yield return BeginTail(direction);var g=pattern.TailGeometry;
                Move(enemy.GetComponent<Rigidbody>().position+Aim(direction)*1.35f);health.ResetHealth();yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"head safe");Equal(100,health.CurrentHealth,"head side safe");Pass($"HEAD SAFE {direction}: front remains outside tail damage");
                yield return BeginTail(direction);g=pattern.TailGeometry;
                yield return Wait(()=>pattern.TailProgress>=.7f,2,"late entry time");Move(g.Origin+g.Direction(.15f)*.85f);health.ResetHealth();
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,1,"late entry recovery");yield return new WaitForSeconds(.35f);Equal(100,health.CurrentHealth,"passed sweep no lingering damage");Pass($"TIMING {direction}: entry behind already-passed tail and during recovery is safe");
                yield return BeginTail(direction);g=pattern.TailGeometry;
                Move(g.Origin+g.Direction(0)*.85f);health.GrantTemporaryInvincibility(2);yield return Wait(()=>pattern.HitAttempts==1,2,"invincible attempt");
                Equal(100,health.CurrentHealth,"invincible no damage");health.ResetHealth();Move(g.Origin+g.Direction(.85f)*.85f);
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,1,"invincible reentry");Equal(100,health.CurrentHealth,"invincible attempt consumes cycle budget");Pass($"INVULNERABILITY {direction}: blocked first hit is not retried after invulnerability reset");
            }
            Restore();SetTailEnabled(false);Spawn();Move(origin+Vector3.right*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,3,"A disabled source");Move(origin+Vector3.forward*3);
            yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"A recovery");Equal(3.5f,pattern.Travelled,"A full dash");Require(!pattern.TailEnabled&&pattern.TailGeometry==null,"B disabled exact A path");
            yield return new WaitForSeconds(.95f);Require(pattern.Phase==HelicoSpiralPhase.Ready,"A0.9 recovery");Pass("B OFF: original3.5m dash +0.9s recovery, no tail action/area/damage");
            Restore();Set(settings,"tailReach",1.5f);Set(settings,"tailArcDegrees",90);Set(settings,"tailWidth",.12f);Set(settings,"tailWindupSeconds",1.1f);
            yield return BeginTail(22.5f);Equal(1.5f,pattern.TailGeometry.Reach,"Inspector length used");Equal(90,pattern.TailGeometry.Arc,"Inspector arc used");Equal(.06f,pattern.TailGeometry.Radius,"Inspector width used");Equal(1.1f,pattern.TailWindupDuration,"Inspector time used");
            Move(pattern.TailGeometry.Origin+pattern.TailGeometry.Rear*1.4f);health.ResetHealth();
            yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,3,"changed length hit");Equal(97,health.CurrentHealth,"Inspector extended range applies real damage");Pass("INSPECTOR: SerializedObject length1.5/angle90/width0.12/tell1.1 consumed by next spawn and real damage");
            Restore();
            foreach(float direction in new[]{0f,45f})
            {
                Spawn();Move(origin+Aim(direction)*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,3,"tail wall tell");
                var cell=biome.WorldToGrid(origin+Aim(direction)*2);biome.AddRuntimeBlockedCells(new[]{cell});
                try {Move(origin+Aim(direction+90)*3);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,3,"wall recovery");Require(pattern.WallStopped&&pattern.TailGeometry==null&&!pattern.TailCompleted,"wall skips tail");Equal(100,health.CurrentHealth,"wall cancellation safe");}
                finally {biome.RemoveRuntimeBlockedCells(new[]{cell});}
                Pass($"WALL {direction}: shortened dash skips tail and uses basic recovery");
            }
            foreach(int fps in new[]{60,15})
            {
                QualitySettings.vSyncCount=0;Application.targetFrameRate=fps;yield return BeginTail(45);var g=pattern.TailGeometry;
                Move(g.Origin+g.Direction(.63f)*1.02f);health.ResetHealth();float maxDelta=0;
                while(pattern.Phase!=HelicoSpiralPhase.Recovery){maxDelta=Mathf.Max(maxDelta,Time.deltaTime);yield return new WaitForEndOfFrame();CheckTailGround();}
                Equal(97,health.CurrentHealth,"continuous sweep hits once at frame rate");Equal(1,pattern.HitAttempts,"single low fps hit");
                Pass($"FPS {fps}: continuous sweep hit3 once, visible ground tip follows geometry; max dt={maxDelta:0.000}");
            }
            Application.targetFrameRate=oldFrameRate;QualitySettings.vSyncCount=oldVsync;
            backups.Add(definition,EditorJsonUtility.ToJson(definition));
            using(var so=new SerializedObject(definition)){int index=definition.patternDamage.FindIndex(p=>p.id==HelicoSpiralPatternSettings.TailDamageId);so.FindProperty("patternDamage").GetArrayElementAtIndex(index).FindPropertyRelative("coefficient").floatValue=2;so.ApplyModifiedPropertiesWithoutUndo();}
            yield return BeginTail(0);Move(pattern.TailGeometry.Origin+pattern.TailGeometry.Rear*.8f);health.ResetHealth();yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"coefficient result");Equal(96,health.CurrentHealth,"Inspector coefficient2 x attack2 =4");Pass("DAMAGE INSPECTOR: separate helico-tail coefficient2 gives real4 damage, base attack unchanged");
            Restore();enemy.ReleaseToPool();Require(definition.GetValidationError()==null,"source restored");
        }
    }
}
