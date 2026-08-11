Shader "Necrocis/BossArenaFog"
{
    Properties
    {
        [PerRendererData] _MainTex ("Fog Texture", 2D) = "white" {}
        _SecondaryColor ("Wisp Color", Color) = (0.55, 0.75, 0.65, 1)
        _FogTiling ("World Tiling", Vector) = (3, 1, 0, 0)
        _PrimarySpeed ("Primary Speed", Vector) = (0.018, 0.009, 0, 0)
        _SecondarySpeed ("Secondary Speed", Vector) = (-0.012, 0.016, 0, 0)
        _DistortionStrength ("Distortion", Range(0, 0.3)) = 0.035
        _EdgeSoftness ("Interior Edge Softness", Range(0.001, 0.45)) = 0.16
        _Density ("Density", Range(0, 2)) = 1.1
        _BaseOpacity ("Interior Base Opacity", Range(0, 1)) = 0
        _InteriorMode ("Interior Mode", Range(0, 1)) = 0
        _CoreDarkness ("Core Darkness", Range(0, 1)) = 0.82
        _WispBrightness ("Wisp Brightness", Range(0, 2)) = 0.75
        _Seed ("Layer Seed", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _SecondaryColor;
            float4 _FogTiling;
            float4 _PrimarySpeed;
            float4 _SecondarySpeed;
            float _DistortionStrength;
            float _EdgeSoftness;
            float _Density;
            float _BaseOpacity;
            float _InteriorMode;
            float _CoreDarkness;
            float _WispBrightness;
            float _Seed;

            float MirrorRepeat(float value)
            {
                float repeated = frac(value * 0.5) * 2.0;
                return 1.0 - abs(repeated - 1.0);
            }

            float2 MirrorRepeat(float2 value)
            {
                return float2(MirrorRepeat(value.x), MirrorRepeat(value.y));
            }

            float FogLuminance(fixed3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            fixed4 SampleWall(float2 uv)
            {
                return tex2D(_MainTex, float2(MirrorRepeat(uv.x), saturate(uv.y)));
            }

            float SampleInterior(float2 uv)
            {
                fixed3 sampleColor = tex2D(_MainTex, MirrorRepeat(uv)).rgb;
                return smoothstep(0.70, 0.985, FogLuminance(sampleColor));
            }

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color;
                return output;
            }

            fixed4 RenderWall(v2f input, float timeValue)
            {
                float alongTiling = max(1.0, _FogTiling.x);
                float seedOffset = _Seed * 0.173;
                float along = input.texcoord.x * alongTiling;

                float distortionSample = SampleWall(float2(
                    along * 0.41 - _SecondarySpeed.x * timeValue * 0.7 + seedOffset,
                    input.texcoord.y)).a;
                float crossDistortion = (distortionSample * 2.0 - 1.0) * _DistortionStrength;

                fixed4 primary = SampleWall(float2(
                    along + _PrimarySpeed.x * timeValue + seedOffset,
                    input.texcoord.y + crossDistortion));
                fixed4 counterFlow = SampleWall(float2(
                    along * 0.68 + _SecondarySpeed.x * timeValue - seedOffset * 0.61,
                    1.0 - input.texcoord.y - crossDistortion * 0.8));

                float primaryAlpha = primary.a;
                float counterAlpha = counterFlow.a * 0.72;
                float fogAlpha = saturate(max(primaryAlpha, counterAlpha) * _Density);
                float denseCore = smoothstep(0.46, 0.94, fogAlpha);
                float translucentRim = smoothstep(0.04, 0.42, fogAlpha)
                    * (1.0 - smoothstep(0.48, 0.94, fogAlpha));
                float textureHighlight = saturate(max(FogLuminance(primary.rgb), FogLuminance(counterFlow.rgb)) - 0.16);
                float accentAmount = saturate(
                    translucentRim * _WispBrightness
                    + textureHighlight * _WispBrightness * 0.32);

                fixed3 darkCoreColor = input.color.rgb
                    * lerp(0.78, 0.34, denseCore * saturate(_CoreDarkness));
                darkCoreColor += _SecondaryColor.rgb * denseCore * 0.035;
                fixed3 accentColor = _SecondaryColor.rgb * (0.42 + accentAmount * 0.38);
                fixed3 fogColor = lerp(darkCoreColor, accentColor, accentAmount * 0.72);
                return fixed4(fogColor, fogAlpha * input.color.a);
            }

            fixed4 RenderInterior(v2f input, float timeValue)
            {
                float2 tiling = max(_FogTiling.xy, float2(0.5, 0.5));
                float2 seedOffset = float2(_Seed * 0.173, _Seed * 0.317);
                float2 distortionUv = input.texcoord * tiling * 0.43
                    + _SecondarySpeed.xy * timeValue * 0.55
                    + seedOffset;
                float2 distortion = (float2(
                    SampleInterior(distortionUv),
                    SampleInterior(distortionUv.yx + float2(0.37, 0.61))) * 2.0 - 1.0)
                    * _DistortionStrength;

                float primary = SampleInterior(
                    input.texcoord * tiling + _PrimarySpeed.xy * timeValue + seedOffset + distortion);
                float secondary = SampleInterior(
                    input.texcoord * tiling * 0.71 + _SecondarySpeed.xy * timeValue
                    - seedOffset * 0.63 - distortion * 0.75);
                float detail = SampleInterior(
                    input.texcoord * tiling * 1.67
                    - (_PrimarySpeed.xy + _SecondarySpeed.xy) * timeValue * 0.38
                    + seedOffset.yx * 1.41);

                float layeredFog = saturate(primary * 0.58 + secondary * 0.45 + detail * 0.24 - 0.20);
                layeredFog = saturate(smoothstep(0.04, 0.94, layeredFog) * _Density);
                float2 edgeDistance = min(input.texcoord, 1.0 - input.texcoord);
                float edgeMask = smoothstep(
                    0.0,
                    max(0.001, _EdgeSoftness),
                    min(edgeDistance.x, edgeDistance.y));
                float fogOpacity = lerp(layeredFog, 1.0, saturate(_BaseOpacity));
                float alpha = saturate(fogOpacity * edgeMask) * input.color.a;
                fixed3 color = lerp(input.color.rgb * 0.52, _SecondaryColor.rgb * 0.55, secondary * 0.3);
                return fixed4(color, alpha);
            }

            fixed4 frag(v2f input) : SV_Target
            {
                if (_InteriorMode > 0.5)
                {
                    return RenderInterior(input, _Time.y);
                }

                return RenderWall(input, _Time.y);
            }
            ENDCG
        }
    }
}
