using System.IO;
using Necrocis;
using UnityEngine;
using Object=UnityEngine.Object;
namespace NecrocisEditor
{
    public static partial class OilFilmConnectionRunner
    {
        private static void CheckGpuTerrainClip(bool wall,string name)
        {
            var film=enemy.transform.Find("OilFilmOriginalMembrane").GetComponent<MeshRenderer>();Require(film.enabled,"GPU membrane active");
            int layer=film.gameObject.layer;var go=new GameObject("S03_ClipProbeCamera");var camera=go.AddComponent<Camera>();camera.CopyFrom(DontStarveCamera.GetActiveCamera());camera.enabled=false;
            var rt=RenderTexture.GetTemporary(512,512,24,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;var image=new Texture2D(512,512,TextureFormat.RGBA32,false);
            try
            {
                film.gameObject.layer=31;camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.allowHDR=false;camera.allowMSAA=false;
                camera.orthographicSize=3;camera.aspect=1;camera.transform.rotation=DontStarveCamera.GetActiveCamera().transform.rotation;camera.transform.position=pattern.GroundOrigin-camera.transform.forward*10;
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();
                float Alpha(Vector3 p){p.y=pattern.Footprint.Origin.y+.055f;var v=camera.WorldToViewportPoint(p);return image.GetPixel(Mathf.Clamp(Mathf.RoundToInt(v.x*511),0,511),Mathf.Clamp(Mathf.RoundToInt(v.y*511),0,511)).a;}
                var g=pattern.Footprint;var hidden=g.Origin+g.Forward*(g.RadiusAtAngle(.5f)+.18f);
                var visible=wall?g.Point(.95f,.65f):g.Origin+g.Forward*(g.RadiusAtAngle(.5f)-.18f);
                float inside=Alpha(visible),outside=Alpha(hidden);Require(inside>.2f&&outside<.01f,$"actual GPU mask: visible={inside}, hidden={outside}");
                File.WriteAllBytes("Logs/"+Label+"-"+name+".png",image.EncodeToPNG());Pass("GPU CLIP "+name+": approved texture visible before edge, alpha0 past blocked/other-height boundary, no squeezing");
            }
            finally{film.gameObject.layer=layer;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);Object.Destroy(image);Object.Destroy(go);}
        }
    }
}
