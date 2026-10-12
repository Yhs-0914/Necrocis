using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Necrocis;
using UnityEngine;
namespace NecrocisEditor
{
    public static partial class OilFilmConnectionRunner
    {
        [Serializable] private sealed class MotionFrame { public string file, phase; public float time; public int sourceIndex, impacts; }
        [Serializable] private sealed class MotionDocument { public int angle, fps=20; public bool nativeUnityFrames=true; public List<MotionFrame> frames=new List<MotionFrame>(); }
        private static IEnumerator RecordMotion()
        {
            float old=Time.captureDeltaTime;Time.captureDeltaTime=1f/60;
            try
            {
                Set(settings,"spawnGraceSeconds",.3f);
                foreach(int direction in new[]{0,90,180,270})
                {
                    string folder="output/art/OilFilmGlider/Runtime/"+direction.ToString("000");Directory.CreateDirectory(folder);
                    angle=direction;Spawn();Move(origin+Aim(direction)*1.6f);var document=new MotionDocument{angle=direction};float began=Time.time;int n=0;bool moved=false;float doneAt=-1;
                    while(Time.time-began<4)
                    {
                        if(!moved&&pattern.Phase==OilFilmPhase.Windup){Move(origin+Aim(direction+100)*2.8f);moved=true;}
                        if(pattern.ImpactCount==1&&pattern.Phase==OilFilmPhase.Ready&&doneAt<0)doneAt=Time.time+.25f;
                        yield return new WaitForEndOfFrame();
                        if(n++%3==0)
                        {
                            string file=document.frames.Count.ToString("D3")+".png";CaptureFile(folder+"/"+file);
                            document.frames.Add(new MotionFrame{file=file,phase=pattern.Phase.ToString(),time=Time.time-began,sourceIndex=enemy.GetComponent<OilFilmVisual>().CurrentProfile.index,impacts=pattern.ImpactCount});
                        }
                        if(doneAt>=0&&Time.time>=doneAt)break;
                    }
                    Require(pattern.ImpactCount==1&&pattern.Phase==OilFilmPhase.Ready,"complete recorded cycle");
                    File.WriteAllText(folder+"/clip.json",JsonUtility.ToJson(document,true));Pass("MOTION "+direction+": native 20fps complete warning/impact/recovery, "+document.frames.Count+" frames");
                }
            }
            finally{Time.captureDeltaTime=old;Restore();}
        }
    }
}
