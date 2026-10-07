Shader "Necrocis/FinalBossPhaseThree"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False" }
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
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };
            sampler2D _MainTex;
            fixed4 _Color;
            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 source = tex2D(_MainTex, input.uv);
                float3 key = source.rgb;
                #ifndef UNITY_COLORSPACE_GAMMA
                    key = LinearToGammaSpace(key);
                #endif
                // Preserve the original art. Only its dark, neutral-purple backdrop is masked.
                // Warm neural tissue, cyan synapses and bright highlights remain visible.
                float warm = smoothstep(.015, .045, key.r - max(key.g, key.b));
                float cyan = smoothstep(.012, .05, min(key.g, key.b) - key.r);
                float bright = smoothstep(.32, .55, max(key.r, max(key.g, key.b)));
                source.a *= max(warm, max(cyan, bright));
                clip(source.a - .01);
                return source * input.color;
            }
            ENDCG
        }
    }
}
