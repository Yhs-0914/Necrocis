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
        [Serializable] private sealed class TailMotionFrame { public string file, phase; public float time, progress; }
        [Serializable] private sealed class TailMotionClip { public float direction, step, sweepSeconds; public int width,height; public TailMotionFrame[] frames; }
        private static IEnumerator RecordTailMotion()
        {
            string folder="output/art/HelicoSpiral/TailSweep/AirArc/"+Mathf.RoundToInt(angle).ToString("000");
            Directory.CreateDirectory(folder);var samples=new List<TailMotionFrame>();
            Move(enemy.GetComponent<Rigidbody>().position+Aim(angle+90)*2.25f);health.GrantTemporaryInvincibility(8);
            var camera=DontStarveCamera.GetActiveCamera();DontStarveCamera.Instance.enabled=false;camera.orthographicSize=2.6f;
            camera.transform.position=enemy.GetComponent<Rigidbody>().position-pattern.LockedDirection*.45f+Vector3.up*.5f-camera.transform.forward*18;
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            float prior=Time.captureDeltaTime;Time.captureDeltaTime=1f/60;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
            float began=Time.time;int width=0,height=0;
            try
            {
                for(int i=0;i<144;i++)
                {
                    yield return new WaitForEndOfFrame();
                    if(pattern.TailActionVisible)CheckTailGround();
                    if(i%2!=0)continue;
                    width=960;height=540;
                    var target=RenderTexture.GetTemporary(width,height,24,RenderTextureFormat.ARGB32);
                    var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
                    var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
                    string file=samples.Count.ToString("D3")+".png";
                    try
                    {
                        camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                        texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
                        File.WriteAllBytes(Path.Combine(folder,file),texture.EncodeToPNG());
                    }
                    finally{camera.targetTexture=oldTarget;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(target);UnityEngine.Object.Destroy(texture);}
                    samples.Add(new TailMotionFrame{file=file,time=Time.time-began,phase=pattern.Phase.ToString(),progress=pattern.TailProgress});
                }
            }
            finally{Time.captureDeltaTime=prior;}
            File.WriteAllText(Path.Combine(folder,"clip.json"),JsonUtility.ToJson(new TailMotionClip{direction=angle,step=1f/30,sweepSeconds=pattern.TailSweepDuration,width=width,height=height,frames=samples.ToArray()},true));
        }
    }
}
