Shader "Custom/LineColourURP"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (RGB)", 2D) = "white" {}
        _LineColor ("Line Color", Color) = (1, 1, 0, 1)
        _UseMask ("Use Mask", Float) = 0.0
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "Queue" = "Geometry" 
        }
        LOD 100

        Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                float4 _LineColor;
            CBUFFER_END

            TEXTURE2D (_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D (_MaskTex);
            SAMPLER(sampler_MaskTex);
            float _UseMask;

            struct VertexInput
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            VertexOutput vert(VertexInput i)
            {
                VertexOutput o;
                o.position = TransformObjectToHClip(i.position.xyz);
                o.uv = i.uv;
                return o;
            }

            float4 frag(VertexOutput i) : SV_Target
            {
                float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                float4 finalColor;

                if (_UseMask == 1.0)
                {
                    finalColor = lerp(baseTex, _LineColor, maskTex.r);
                }
                else
                {
                   finalColor = baseTex;
                }

                return float4(finalColor.rgb, baseTex.a);
            }

            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
