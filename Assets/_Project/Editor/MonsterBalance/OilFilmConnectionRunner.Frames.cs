using System.Collections;
using Necrocis;
using UnityEngine;
namespace NecrocisEditor
{
    public static partial class OilFilmConnectionRunner
    {
        private static IEnumerator FrameChecks()
        {
            int oldRate=Application.targetFrameRate,oldSync=QualitySettings.vSyncCount;
            try
            {
                foreach(int fps in new[]{60,15})foreach(float direction in new[]{0f,45f,90f,270f})foreach(bool inside in new[]{true,false})
                {
                    Application.targetFrameRate=fps;QualitySettings.vSyncCount=0;Spawn();Move(origin+Aim(direction)*1.6f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"fps tell");
                    var forward=pattern.LockedDirection;Move(origin+forward*(inside?2.475f:2.525f));health.ResetHealth();float maxDt=0;bool fullPose=false;
                    while(pattern.Phase!=OilFilmPhase.Recovery)
                    {
                        yield return new WaitForEndOfFrame();maxDt=Mathf.Max(maxDt,Time.deltaTime);
                        if(pattern.Phase==OilFilmPhase.Release&&enemy.GetComponent<OilFilmVisual>().CurrentProfile.index==10){CheckSurface();fullPose=true;}
                    }
                    Require(fullPose,"full impact pose rendered at frame rate");Equal(inside?3:0,100-health.CurrentHealth,"fps boundary damage");Equal(1,pattern.ImpactCount,"one impact at low fps");
                    if(fps==15)Require(maxDt>.045f,"actual low fps measured");
                    Move(origin+Vector3.forward*4);Pass($"FRAME {fps}fps/{direction}/{(inside?"inside":"outside")}: damage{(inside?3:0)}, full impact pose and shared edge, dt={maxDt:F3}");
                }
            }
            finally{Application.targetFrameRate=oldRate;QualitySettings.vSyncCount=oldSync;}
        }
    }
}
