Shader "Necrocis/OilFilmTerrainClip"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _ClipOrigin;
                float4 _ClipAxes;
                float _ClipArc;
                float _Reach[65];
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 positionWS:TEXCOORD1; float4 color:COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS=TransformObjectToWorld(input.positionOS);
                output.positionCS=TransformWorldToHClip(output.positionWS);
                output.uv=input.uv;output.color=input.color*_Color;
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                float2 p=input.positionWS.xz-_ClipOrigin.xy;
                float angle=atan2(dot(p,_ClipAxes.zw),dot(p,_ClipAxes.xy));
                clip(_ClipArc*.5-abs(angle)+.00001);
                float slot=saturate(angle/_ClipArc+.5)*64;
                int lo=min(63,(int)floor(slot));
                float reach=lerp(_Reach[lo],_Reach[lo+1],slot-lo);
                clip(reach-length(p)+.00001);
                half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv)*input.color;
                return color;
            }
            ENDHLSL
        }
    }
}
