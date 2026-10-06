Shader "Hidden/PSXPixelization"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off
        Cull Off

        // Pass 0: Downscale + Color Reduction
        Pass
        {
            Name "PSX_Downscale"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragDown

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _RenderScale;
            int _ColorDepth;

            float4 FragDown(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);

                // PSX color depth reduction
                if (_ColorDepth < 32)
                {
                    float levels = pow(2.0, (float)_ColorDepth) - 1.0;
                    color.rgb = floor(color.rgb * levels + 0.5) / levels;
                }

                return color;
            }
            ENDHLSL
        }

        // Pass 1: Upscale (point filtered)
        Pass
        {
            Name "PSX_Upscale"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragUp

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 FragUp(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);
            }
            ENDHLSL
        }
    }
}
