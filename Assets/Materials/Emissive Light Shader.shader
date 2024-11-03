Shader "Unlit/Emissive Light Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _InnerRadius ("InnerRadius", Float) = 0.0
        _OuterRadius ("OuterRadius", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            float4 _MainTex_ST;

            float _InnerRadius;
            float _OuterRadius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // sample the texture

                float2 uvCentralized = i.uv;
                uvCentralized -= 0.5;
                uvCentralized *= 2.0;

                float distance = 1 - length(uvCentralized);
                // fixed4 col = tex2D(_MainTex, i.uv);
                float4 col = float4(distance, distance, distance, 1);

                return distance.xxxx;
            }
            ENDCG
        }
    }
}
