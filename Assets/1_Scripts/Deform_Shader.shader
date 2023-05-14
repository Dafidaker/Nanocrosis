Shader "Unlit/Deform"
{
    Properties
    {
        _Color("Color", Color ) = (1, 1, 1, 1)
        _MainTex("Texture", 2D) = "white" {}
        _speed("Speed", float) = 0
        _frequency("Frequency", float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            float4 _Color;
            sampler2D _MainTex, _DissolveTexture;
            float4 _MainTex_ST;
            float _speed , _frequency;

            struct VertexData
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float3 normals : NORMAL;
            };


            struct VertexToFragment
            {
                float4 position : SV_POSITION;
                float3 localPosition : TEXCOORD0;
                float3 normals : NORMAL;
                float2 uv: TEXCOORD1;
                float2 uvDetail: TEXCOORD2;
            };

            
            VertexToFragment vert (VertexData vertData) 
            {
                VertexToFragment v2f;

                //moves the shape
                vertData.position.x += sin((_Time.y * _speed) + (vertData.position.y * _frequency)) * 0.1;
                
                v2f.localPosition = vertData.position.xyz;
                v2f.normals = vertData.normals;
                v2f.position = UnityObjectToClipPos(vertData.position);
                v2f.uv = vertData.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return v2f;
            }

            //RETURNS A COLOR
            float4 frag (VertexToFragment v2f) : SV_TARGET
            {
                float4 color = tex2D(_MainTex, v2f.uv) * _Color;
                return color;
            }
            
            ENDCG
        }
    }
}

