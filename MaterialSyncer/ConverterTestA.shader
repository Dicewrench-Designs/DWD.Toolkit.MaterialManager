//© Dicewrench Designs LLC 2025
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

//This is not a real shader.  It is here to unit test the 
//Material Converter is working...

Shader "DWD/TESTING/Converter Test A"
{
    Properties
    {
        [NoScaleOffset] _TextureA ("Texture A", 2D) = "white" {}
        _FloatA("Float A", float) = 3.0
        _IntA("Int A", int) = 1
        _VectorA("Vector A", Vector) = (1,2,3,4)
        _ColorA("Color A", Color) = (1,0,0,0)
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

            sampler2D _TextureA;
            float _FloatA;
            int _IntA;
            half4 _VectorA;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_TextureA, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
