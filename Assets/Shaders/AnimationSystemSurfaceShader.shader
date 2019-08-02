Shader "Custom/Animation System Surface Shader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _RendererIndex("Renderer Index", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
    

        #pragma surface surf Standard fullforwardshadows  vertex:vert
        #include "CustomVertexData.cginc"

        #pragma target 4.5                 


        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

    
        UNITY_INSTANCING_BUFFER_START(CustomRendererProps)

            UNITY_DEFINE_INSTANCED_PROP(int, _RendererIndex)

        UNITY_INSTANCING_BUFFER_END(CustomRendererProps)                                 



        struct appdata_extend
        {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            float4 texcoord2 : TEXCOORD2;
            float4 texcoord3 : TEXCOORD3;
            float4 texcoord4 : TEXCOORD4;
            float4 texcoord5 : TEXCOORD5;
            float4 texcoord6 : TEXCOORD6;
            float4 texcoord7 : TEXCOORD7;
            fixed4 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };


        struct CustomRenderData
        {
            int boneIndex1;
            int boneIndex2;
            int boneIndex3;
            int boneIndex4;

            float boneWeight1;
            float boneWeight2;
            float boneWeight3;
            float boneWeight4;

            float4 vertexPos;
            float3 vertexNormal;
            int renderIndex;
        };




#if SHADER_API_D3D11 || SHADER_API_METAL || SHADER_API_VULKAN
        StructuredBuffer<float4x4> matrixBuffer;
#endif

        void CalculatePositions(CustomRenderData rendererData, out float4 objectSpacePosition, out float3 objectSpaceNormal)
        {           

#if SHADER_API_D3D11 || SHADER_API_METAL || SHADER_API_VULKAN
         
            int index = rendererData.renderIndex;   
            int startIdx = index * 256;

            float4x4 mat = matrixBuffer[startIdx + rendererData.boneIndex1] * rendererData.boneWeight1 +
                matrixBuffer[startIdx + rendererData.boneIndex2] * rendererData.boneWeight2 +
                matrixBuffer[startIdx + rendererData.boneIndex3] * rendererData.boneWeight3 +
                matrixBuffer[startIdx + rendererData.boneIndex4] * rendererData.boneWeight4;

            mat = mul(unity_WorldToObject, mat);           

            objectSpacePosition = mul(mat, rendererData.vertexPos);
            objectSpaceNormal = mul(mat, float4(rendererData.vertexNormal, 0)).xyz;

#else
            objectSpacePosition = rendererData.vertexPos;
            objectSpaceNormal = rendererData.vertexNormal;
#endif
        }

        void vert(inout appdata_extend v, out Input o)
        {

            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(Input, o);

            CustomRenderData renderData;

            renderData.boneIndex1 = v.texcoord6.x;
            renderData.boneIndex2 = v.texcoord6.y;
            renderData.boneIndex3 = v.texcoord6.z;
            renderData.boneIndex4 = v.texcoord6.w;

            renderData.boneWeight1 = v.texcoord7.x;
            renderData.boneWeight2 = v.texcoord7.y;
            renderData.boneWeight3 = v.texcoord7.z;
            renderData.boneWeight4 = v.texcoord7.w;

            renderData.vertexPos = v.vertex;
            renderData.vertexNormal = v.normal;

            renderData.renderIndex = UNITY_ACCESS_INSTANCED_PROP(CustomRendererProps, _RendererIndex);

            CalculatePositions(renderData, v.vertex, v.normal);

        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
