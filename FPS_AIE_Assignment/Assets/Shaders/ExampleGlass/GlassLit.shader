Shader "Custom/GlassLit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        GrabPass { "GrabScreenPass" }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _GrabPassTransparent;

        struct Input
        {
            //float2 uv_GrabPassTransparentTexture; //screen space?
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 objPos = mul(unity_WorldToObject, float4(IN.worldPos, 1));
            float4 uv_GPTTexture = ComputeGrabScreenPos(objPos); // uv_GrabPassTransparentTexture
            uv_GPTTexture.y *= -1;

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2Dproj (_GrabPassTransparent, UNITY_PROJ_COORD(uv_GPTTexture)) * _Color;
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
