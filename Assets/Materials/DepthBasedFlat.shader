Shader "Unlit/DepthBasedFlat"
{
    Properties
    {
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _Tint("Color Tint", Color) = (1,1,1,1) 
        PixelSnap("Pixel snap", Float) = 0
        _RendererColor("RendererColor", Color) = (1,1,1,1)
        _Flip("Flip", Vector) = (1,1,1,1)
        _AlphaTex("External Alpha", 2D) = "white" {}
        _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+1"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        LOD 100

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;   
            
            float4 _ColorTint;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col*= _ColorTint;

                return _ColorTint;
            }
            ENDCG
        }
    }
}
