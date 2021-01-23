Shader "Unlit/TextureBlendShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SubTex ("SubTexture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SubTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 mainTexture = tex2D(_MainTex, i.uv);
                fixed4 subTexture = tex2D(_SubTex, i.uv);
                //fixed4 col = mainTexture * 0.5 + subTexture * 0.5;
                fixed4 col = mainTexture * mainTexture.a + subTexture * (1.0f - mainTexture.a);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return mainTexture;
                //return subTexture;
                //return subTexture;
                return col;
            }
            ENDCG
        }
    }
}
