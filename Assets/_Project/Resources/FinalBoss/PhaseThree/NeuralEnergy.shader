Shader "Necrocis/NeuralEnergy"
{
    Properties { _Color ("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            fixed4 _Color;
            Output vert(Input v)
            {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color * _Color; return o;
            }
            fixed4 frag(Output i) : SV_Target
            {
                float edge = saturate(1.0 - abs(i.uv.y * 2.0 - 1.0));
                float core = pow(edge, 8.0);
                return fixed4(i.color.rgb + core * .22, i.color.a * (edge * edge * .65 + core * .35));
            }
            ENDCG
        }
    }
}
