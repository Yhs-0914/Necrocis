Shader "Necrocis/BossArenaFog"
{
    Properties
    {
        [PerRendererData] _MainTex ("Fog Texture", 2D) = "white" {}
        [PerRendererData] _Tint ("Renderer Tint", Color) = (1, 1, 1, 1)
        _SecondaryColor ("Vein Color", Color) = (0.55, 0.75, 0.65, 1)
        _FogTiling ("World Tiling", Vector) = (3, 1, 0, 0)
        _PrimarySpeed ("Primary Speed", Vector) = (0.018, 0.009, 0, 0)
        _SecondarySpeed ("Secondary Speed", Vector) = (-0.012, 0.016, 0, 0)
        _DistortionStrength ("Distortion", Range(0, 0.3)) = 0.035
        _EdgeSoftness ("Interior Edge Softness", Range(0.001, 0.45)) = 0.16
        _Density ("Density", Range(0, 2)) = 1.1
        _BaseOpacity ("Interior Base Opacity", Range(0, 1)) = 0
        _InteriorMode ("Interior Mode", Range(0, 1)) = 0
        _CoreDarkness ("Core Darkness", Range(0, 1)) = 0.82
        _WispBrightness ("Vein Brightness", Range(0, 2)) = 0.75
        _Seed ("Layer Seed", Float) = 0
        _SealAmount ("Barrier Seal", Range(0, 1)) = 0
        _RevealAmount ("Interior Reveal", Range(0, 1)) = 0
        _FlowBoost ("Transition Flow Boost", Range(0, 2)) = 0
        _PixelDensity ("Pixel Density", Range(16, 256)) = 96
        _AnimationFps ("Animation FPS", Range(4, 30)) = 12
        _AspectRatio ("Interior Aspect Ratio", Float) = 1
        _ApproachAmount ("Player Approach", Range(0, 1)) = 0
        _SideSeal ("Side Seal N/S/E/W", Vector) = (0, 0, 0, 0)
        _SideApproach ("Side Approach N/S/E/W", Vector) = (0.18, 0.18, 0.18, 0.18)
        _UseSideState ("Use Perimeter Side State", Range(0, 1)) = 0
        _OrganProfile ("Organ Motion Profile", Range(0, 3)) = 0
        _MotionIntensity ("Motion Intensity", Range(0, 2)) = 1
        _GroundContact ("Ground Contact Layer", Range(0, 1)) = 0
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
                float4 sideWeights : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 sideWeights : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Tint;
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
            float _SealAmount;
            float _RevealAmount;
            float _FlowBoost;
            float _PixelDensity;
            float _AnimationFps;
            float _AspectRatio;
            float _ApproachAmount;
            float4 _SideSeal;
            float4 _SideApproach;
            float _UseSideState;
            float _OrganProfile;
            float _MotionIntensity;
            float _GroundContact;

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

            float Smooth01(float value)
            {
                value = saturate(value);
                return value * value * (3.0 - 2.0 * value);
            }

            float2 PixelateUv(float2 uv)
            {
                float density = max(16.0, _PixelDensity);
                return (floor(uv * density) + 0.5) / density;
            }

            float QuantizedTime(float timeValue)
            {
                float fps = max(4.0, _AnimationFps);
                return floor(timeValue * fps) / fps;
            }

            float SampleWallDensity(float2 uv)
            {
                float2 sampleUv = PixelateUv(float2(MirrorRepeat(uv.x), saturate(uv.y)));
                fixed4 sampleColor = tex2D(_MainTex, sampleUv);
                float alphaWeight = sqrt(sqrt(saturate(sampleColor.a)));
                float density = FogLuminance(sampleColor.rgb) * alphaWeight;
                return smoothstep(0.005, 0.18, density);
            }

            float SampleInteriorDensity(float2 uv)
            {
                fixed4 sampleColor = tex2D(_MainTex, MirrorRepeat(PixelateUv(uv)));
                float alphaWeight = sqrt(sqrt(saturate(sampleColor.a)));
                float density = FogLuminance(sampleColor.rgb) * alphaWeight;
                return smoothstep(0.005, 0.22, density);
            }

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color * _Tint;
                output.sideWeights = input.sideWeights;
                return output;
            }

            fixed4 RenderWall(v2f input, float timeValue)
            {
                float2 fogUv = input.texcoord;
                float alongTiling = max(1.0, _FogTiling.x);
                float seedOffset = _Seed * 0.173;
                float flowTime = QuantizedTime(timeValue) * (1.0 + saturate(_FlowBoost) * 1.65);
                float along = fogUv.x * alongTiling;

                float sideWeightTotal = max(0.001, dot(input.sideWeights, float4(1.0, 1.0, 1.0, 1.0)));
                float sideSeal = dot(input.sideWeights, _SideSeal) / sideWeightTotal;
                float sideApproach = dot(input.sideWeights, _SideApproach) / sideWeightTotal;
                float sealSource = lerp(_SealAmount, sideSeal, saturate(_UseSideState));
                float approachSource = lerp(_ApproachAmount, sideApproach, saturate(_UseSideState));

                float intensity = saturate(_MotionIntensity * 0.5) * 2.0;
                float intestine = 1.0 - step(0.5, abs(_OrganProfile - 0.0));
                float liver = 1.0 - step(0.5, abs(_OrganProfile - 1.0));
                float stomach = 1.0 - step(0.5, abs(_OrganProfile - 2.0));
                float lung = 1.0 - step(0.5, abs(_OrganProfile - 3.0));
                float peristalsis = sin(along * 0.72 - flowTime * 1.65)
                    + sin(along * 0.31 + flowTime * 0.54) * 0.42;
                float clotBeat = pow(saturate(0.5 + 0.5 * sin(flowTime * 2.15)), 4.0);
                float acidBoil = sin(along * 1.83 + flowTime * 2.35)
                    * sin(along * 0.47 - flowTime * 0.91);
                float breath = 0.5 + 0.5 * sin(flowTime * 1.12);
                fogUv.y += peristalsis * intestine * 0.035 * intensity;
                fogUv.y += acidBoil * stomach * 0.045 * intensity;
                fogUv.y = (fogUv.y - 0.5)
                    * (1.0 - lung * breath * 0.13 * intensity)
                    + 0.5;
                flowTime *= 1.0 + intestine * 0.18 + stomach * 0.36 - lung * 0.24;
                along = fogUv.x * alongTiling;

                float distortionSample = SampleWallDensity(float2(
                    along * 0.39 - _SecondarySpeed.x * flowTime * 0.72 + seedOffset,
                    fogUv.y));
                float crossDistortion = (distortionSample * 2.0 - 1.0) * _DistortionStrength;

                float primary = SampleWallDensity(float2(
                    along + _PrimarySpeed.x * flowTime + seedOffset,
                    fogUv.y + crossDistortion));
                float counterFlow = SampleWallDensity(float2(
                    along * 0.67 + _SecondarySpeed.x * flowTime - seedOffset * 0.61,
                    1.0 - fogUv.y - crossDistortion * 0.82));
                float filamentDetail = SampleWallDensity(float2(
                    along * 1.43 - (_PrimarySpeed.x + _SecondarySpeed.x) * flowTime * 0.48
                        + seedOffset * 1.37,
                    fogUv.y * 0.91 + crossDistortion * 0.46));

                float organicDensity = saturate(
                    max(primary, counterFlow * 0.78)
                    + min(primary, counterFlow) * 0.24
                    + filamentDetail * 0.13);
                float sparseOrganDensity = saturate(
                    primary * 0.82
                    + counterFlow * 0.14
                    + filamentDetail * 0.06);
                float preserveOpenShapes = saturate(stomach * 0.72 + lung * 0.56);
                organicDensity = lerp(organicDensity, sparseOrganDensity, preserveOpenShapes);
                organicDensity = saturate(organicDensity * _Density);

                float seal = Smooth01(sealSource);
                float approach = Smooth01(approachSource) * (1.0 - seal);
                // The idle barrier must read before the player steps into it.
                // Keep enough body coverage at seal=0, then grow into the dense lock state.
                float idleCoverageThreshold = lerp(0.20, 0.055, approach);
                float coverageThreshold = lerp(idleCoverageThreshold, 0.035, seal);
                float body = smoothstep(
                    coverageThreshold - 0.075,
                    coverageThreshold + 0.145,
                    organicDensity);
                float persistentWisp = smoothstep(0.28, 0.72, organicDensity) * (0.38 + seal * 0.16);
                float verticalEdge = smoothstep(0.0, 0.16, fogUv.y)
                    * (1.0 - smoothstep(0.84, 1.0, fogUv.y));
                float horizontalCardEdge = smoothstep(0.0, 0.18, fogUv.x)
                    * (1.0 - smoothstep(0.82, 1.0, fogUv.x));
                verticalEdge *= lerp(horizontalCardEdge, 1.0, saturate(_UseSideState));
                float approachBody = smoothstep(0.18, 0.72, organicDensity)
                    * approach
                    * (0.54 + 0.08 * sin(flowTime * 2.4 + along));
                // The source texture has wisps on both outer edges and a dark hollow
                // through its center. Fill that hollow with a noisy translucent body
                // so it reads as one fog bank instead of two parallel stripes.
                float centerDistance = abs(fogUv.y - 0.5 + crossDistortion * 0.035);
                float centerProfile = 1.0 - smoothstep(0.20, 0.49, centerDistance);
                float centerMass = centerProfile * saturate(
                    0.04
                    + organicDensity * 0.72
                    + approach * 0.20
                    + seal * 0.36
                    + liver * clotBeat * 0.14 * intensity);
                float groundTurbulence = sin(along * 0.91 + flowTime * 0.63)
                    * (organicDensity - 0.5)
                    * saturate(_GroundContact)
                    * 0.32
                    * intensity;
                float groundSpread = lerp(1.0, 0.78 + groundTurbulence, saturate(_GroundContact));
                float fogAlpha = saturate(
                    max(max(max(body, persistentWisp), approachBody), centerMass)
                    * verticalEdge
                    * groundSpread);

                float transitionVein = smoothstep(
                    coverageThreshold - 0.12,
                    coverageThreshold - 0.015,
                    organicDensity)
                    * (1.0 - smoothstep(
                        coverageThreshold + 0.04,
                        coverageThreshold + 0.19,
                        organicDensity));
                float textureVein = smoothstep(0.36, 0.94, filamentDetail) * 0.44;
                float transitionPulse = sin(saturate(sealSource) * UNITY_PI);
                float accentAmount = saturate(
                    transitionVein * (0.72 + transitionPulse * 0.48)
                    + textureVein * _WispBrightness
                    + approachBody * 0.28);
                float denseCore = smoothstep(0.38, 0.92, fogAlpha);

                fixed3 darkCoreColor = input.color.rgb
                    * lerp(0.92, 0.58, denseCore * saturate(_CoreDarkness));
                darkCoreColor += _SecondaryColor.rgb * (0.055 + denseCore * 0.085);
                fixed3 veinColor = _SecondaryColor.rgb * (0.34 + accentAmount * 0.48);
                fixed3 fogColor = lerp(darkCoreColor, veinColor, accentAmount * 0.74);
                fogColor = lerp(
                    fogColor,
                    _SecondaryColor.rgb * 0.72,
                    saturate(approachBody * 0.22));
                return fixed4(fogColor, fogAlpha * input.color.a);
            }

            fixed4 RenderInterior(v2f input, float timeValue)
            {
                float2 tiling = max(_FogTiling.xy, float2(0.5, 0.5));
                float2 seedOffset = float2(_Seed * 0.173, _Seed * 0.317);
                float flowTime = QuantizedTime(timeValue) * (1.0 + saturate(_FlowBoost) * 0.72);
                float2 distortionUv = input.texcoord * tiling * 0.43
                    + _SecondarySpeed.xy * flowTime * 0.55
                    + seedOffset;
                float2 distortion = (float2(
                    SampleInteriorDensity(distortionUv),
                    SampleInteriorDensity(distortionUv.yx + float2(0.37, 0.61))) * 2.0 - 1.0)
                    * _DistortionStrength;

                float primary = SampleInteriorDensity(
                    input.texcoord * tiling + _PrimarySpeed.xy * flowTime + seedOffset + distortion);
                float secondary = SampleInteriorDensity(
                    input.texcoord * tiling * 0.71 + _SecondarySpeed.xy * flowTime
                    - seedOffset * 0.63 - distortion * 0.75);
                float detail = SampleInteriorDensity(
                    input.texcoord * tiling * 1.67
                    - (_PrimarySpeed.xy + _SecondarySpeed.xy) * flowTime * 0.38
                    + seedOffset.yx * 1.41);

                float layeredFog = saturate(primary * 0.56 + secondary * 0.43 + detail * 0.22 - 0.17);
                layeredFog = saturate(smoothstep(0.04, 0.90, layeredFog) * _Density);

                float2 edgeDistance = min(input.texcoord, 1.0 - input.texcoord);
                float edgeMask = smoothstep(
                    0.0,
                    max(0.001, _EdgeSoftness),
                    min(edgeDistance.x, edgeDistance.y));

                float2 centered = (input.texcoord - 0.5) * 2.0;
                centered.x *= max(0.25, _AspectRatio);
                float radialDistance = length(centered);
                float reveal = Smooth01(_RevealAmount);
                float revealFront = lerp(-0.24, 1.62 * max(1.0, _AspectRatio), reveal);
                float noisyFront = revealFront + (secondary - 0.5) * 0.19 + (detail - 0.5) * 0.07;
                float revealMask = smoothstep(noisyFront - 0.11, noisyFront + 0.075, radialDistance);
                revealMask *= 1.0 - smoothstep(0.965, 1.0, reveal);

                float fogOpacity = lerp(layeredFog, 1.0, saturate(_BaseOpacity));
                float alpha = saturate(fogOpacity * edgeMask * revealMask) * input.color.a;
                float revealVein = (1.0 - smoothstep(0.035, 0.17, abs(radialDistance - noisyFront)))
                    * sin(saturate(_RevealAmount) * UNITY_PI);
                float posterizedShade = floor(saturate(secondary) * 4.0) / 3.0;
                fixed3 interiorColor = lerp(input.color.rgb * 0.42, _SecondaryColor.rgb * 0.48, posterizedShade * 0.34);
                interiorColor = lerp(interiorColor, _SecondaryColor.rgb * 0.82, revealVein * _WispBrightness * 0.56);
                return fixed4(interiorColor, alpha);
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
