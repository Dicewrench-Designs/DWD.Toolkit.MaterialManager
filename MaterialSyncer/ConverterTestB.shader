//© Dicewrench Designs LLC 2025
//Last Owned by: Bllen White (allen@dicewrenchdesigns.com)

//This is not a real shader.  It is here to unit test the 
//Material Converter is working...

Shader "DWD/TESTING/Converter Test B"
{
    Properties
    {
        [NoScaleOffset] _TextureB ("Texture B", 2D) = "white" {}
        _FloatB("Float B", float) = 0.0
        _IntB("Int B", int) = 0
        _VectorB("Vector B", Vector) = (0,0,0,0)
        _ColorB("Color B", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

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

            sampler2D _TextureB;
            float _FloatB;
            int _IntB;
            half4 _VectorB;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_TextureB, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
