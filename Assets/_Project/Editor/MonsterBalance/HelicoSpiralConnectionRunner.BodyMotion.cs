using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class HelicoSpiralConnectionRunner
    {
        [Serializable] private sealed class WholeBodyFrame { public string file,phase; public float time,progress,degrees; }
        [Serializable] private sealed class WholeBodyClip
        {
            public float direction,step,sweepSeconds;
            public int width,height;
            public string sourceSprite,sourceAsset;
            public bool originalSpriteUnchanged,noAdditionalTailRenderer,scaleUnchanged,headPivotStable,motionOnly;
            public WholeBodyFrame[] frames;
        }
        private static float BodySwingAngle(float seconds,out string state,out float progress)
        {
            if(seconds<.2f){state="Original";progress=0;return 0;}
            seconds-=.2f;
            if(seconds<.9f){state="Windup";progress=seconds/.9f;return 24*Mathf.SmoothStep(0,1,Mathf.Min(1,seconds/.65f));}
            seconds-=.9f;
            if(seconds<.18f){state="Sweep";progress=seconds/.18f;return Mathf.Lerp(24,-32,Mathf.SmoothStep(0,1,progress));}
            seconds-=.18f;state="Recovery";progress=Mathf.Clamp01(seconds/1.25f);
            if(seconds<.3f)return Mathf.Lerp(-32,5,Mathf.SmoothStep(0,1,seconds/.3f));
            return Mathf.Lerp(5,0,Mathf.SmoothStep(0,1,(seconds-.3f)/.45f));
        }
        // Visual approval fixture. Uses the existing complete idle sprite; no
        // replacement flag geometry, no new art, and no combat/telegraph claims.
        private static IEnumerator RecordOriginalBodyMotion()
        {
            Spawn();pattern.EndSpawn();
            enemy.SetAiSuppressed(true);enemy.SetPatternPositionLocked(true);enemy.PatternOwnsContact=true;
            enemy.ReleasePatternFacing();enemy.SetPatternFacing(Aim(angle),false);enemy.CommitPatternFacing(Aim(angle),false);
            typeof(HelicoSpiralElitePattern).GetField("bodyDirection",Private).SetValue(pattern,Aim(angle));
            enemy.SetPatternFrame(settings.presentation.idleFrames[0]);
            var oldTail=enemy.GetComponent<HelicoTailSweepVisual>();oldTail.Hide();oldTail.enabled=false;
            Move(origin+Aim(angle+90)*2.8f);health.GrantTemporaryInvincibility(10);
            yield return new WaitForSeconds(.8f);
            var camera=DontStarveCamera.GetActiveCamera();DontStarveCamera.Instance.enabled=false;camera.orthographicSize=2.8f;
            camera.transform.position=origin-Aim(angle)*.2f+Vector3.up*.45f-camera.transform.forward*18;
            var body=Body();var grounding=enemy.GetComponent<HelicoGroundedVisual>();
            typeof(HelicoGroundedVisual).GetMethod("LateUpdate",Private).Invoke(grounding,null);grounding.enabled=false;
            var billboard=body.GetComponent<Billboard>();if(billboard!=null)billboard.enabled=false;
            var original=body.sprite;Vector3 originalScale=body.transform.localScale,originalWorldScale=body.transform.lossyScale,basePosition=body.transform.position;
            Color originalTint=body.color;
            Quaternion baseRotation=body.transform.rotation;
            Vector2 localHead=settings.presentation.directionalPresentation.Capture().Origin(enemy.PatternFacing);
            Vector3 pivot=body.transform.TransformPoint(new Vector3(localHead.x,localHead.y,0));
            Vector2 screenPivot=camera.WorldToViewportPoint(pivot);
            float ground=biome.GetGroundHeight(origin),start=Time.time;
            string folder="output/art/HelicoSpiral/TailSweep/OriginalBody/"+Mathf.RoundToInt(angle).ToString("000");Directory.CreateDirectory(folder);
            var samples=new List<WholeBodyFrame>();float prior=Time.captureDeltaTime;
            Time.captureDeltaTime=1f/60;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            try
            {
                for(int i=0;i<168;i++)
                {
                    yield return new WaitForEndOfFrame();
                    float seconds=i/60f;float degrees=BodySwingAngle(seconds,out string state,out float progress)*(body.flipX?-1:1);
                    Quaternion turn=Quaternion.AngleAxis(degrees,camera.transform.forward);
                    body.transform.rotation=turn*baseRotation;body.transform.position=pivot+turn*(basePosition-pivot);
                    // Preserve screen-space placement while keeping the billboard
                    // above terrain. This never resizes or deforms the artwork.
                    float lowest=float.PositiveInfinity;var b=original.bounds;
                    foreach(float x in new[]{b.min.x,b.max.x})foreach(float y in new[]{b.min.y,b.max.y})lowest=Mathf.Min(lowest,body.transform.TransformPoint(new Vector3(x,y,0)).y);
                    if(lowest<ground+.025f)body.transform.position+=camera.transform.forward*((ground+.025f-lowest)/camera.transform.forward.y);
                    Require(body.sprite==original&&body.enabled,"same original whole sprite throughout motion");
                    Equal(0,Vector3.Distance(originalScale,body.transform.localScale),"no body/tail scaling");
                    Equal(0,Vector3.Distance(originalWorldScale,body.transform.lossyScale),"no inherited scale change");
                    Require(body.color==originalTint,"original sprite tint preserved");
                    Equal(0,Vector2.Distance(screenPivot,camera.WorldToViewportPoint(body.transform.TransformPoint(new Vector3(localHead.x,localHead.y,0)))),"head pivot stays fixed on screen");
                    Require(!oldTail.IsShowing&&body.GetComponentsInChildren<LineRenderer>().Length==0,"no generated or clipped tail");
                    if(i%2!=0)continue;
                    const int width=960,height=540;var target=RenderTexture.GetTemporary(width,height,24,RenderTextureFormat.ARGB32);
                    var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
                    string file=samples.Count.ToString("D3")+".png";
                    try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(folder,file),texture.EncodeToPNG());}
                    finally{camera.targetTexture=oldTarget;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(target);UnityEngine.Object.Destroy(texture);}
                    samples.Add(new WholeBodyFrame{file=file,phase=state,time=seconds,progress=progress,degrees=degrees});
                }
            }
            finally{Time.captureDeltaTime=prior;body.transform.localScale=originalScale;body.transform.SetPositionAndRotation(basePosition,baseRotation);}
            File.WriteAllText(Path.Combine(folder,"clip.json"),JsonUtility.ToJson(new WholeBodyClip{
                direction=angle,step=1f/30,sweepSeconds=.18f,width=960,height=540,sourceSprite=original.name,sourceAsset=AssetDatabase.GetAssetPath(original),
                originalSpriteUnchanged=true,noAdditionalTailRenderer=true,scaleUnchanged=true,headPivotStable=true,motionOnly=true,frames=samples.ToArray()},true));
        }
    }
}
