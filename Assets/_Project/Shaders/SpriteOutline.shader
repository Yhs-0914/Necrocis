Shader "Necrocis/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0.86,0.88,0.9,1)
        _OutlineSize ("Outline Size", Float) = 2
        _OutlineExpand ("Outline Expand", Float) = 0
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
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _OutlineExpand;

            v2f vert(appdata_t input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                float2 direction = input.vertex.xy;
                float lengthValue = max(length(direction), 0.0001);
                input.vertex.xy += direction / lengthValue * _OutlineExpand;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float centerAlpha = tex2D(_MainTex, input.texcoord).a;
                float2 stepSize = _MainTex_TexelSize.xy * max(_OutlineSize, 1);

                float outlineAlpha = 0;
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, input.texcoord + float2(stepSize.x, 0)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, input.texcoord + float2(-stepSize.x, 0)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, input.texcoord + float2(0, stepSize.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, input.texcoord + float2(0, -stepSize.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, input.texcoord + float2(stepSize.x, stepSize.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, input.texcoord + float2(stepSize.x, -stepSize.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, input.texcoord + float2(-stepSize.x, stepSize.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, input.texcoord + float2(-stepSize.x, -stepSize.y)).a);

                float alpha = step(0.05, outlineAlpha) * (1 - step(0.05, centerAlpha)) * _OutlineColor.a * input.color.a;
                return fixed4(_OutlineColor.rgb, alpha);
            }
            ENDCG
        }
    }
}
