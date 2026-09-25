Shader "Custom/RealisticWaterURP"
{
    Properties
    {
        [Header(Water Colors)]
        _ShallowColor ("Shallow Color", Color) = (0.25, 0.75, 0.85, 0.6)
        _DeepColor ("Deep Color", Color) = (0.02, 0.18, 0.38, 0.92)
        _DepthMaxDistance ("Depth Max Distance", Float) = 3.5

        [Header(Shore Foam)]
        _FoamColor ("Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _FoamDistance ("Foam Distance", Range(0.01, 2.0)) = 0.5
        _FoamNoiseScale ("Foam Noise Scale", Float) = 20.0
        _FoamSpeed ("Foam Speed", Float) = 0.8

        [Header(Waves and Lighting)]
        _WaveHeight ("Wave Vertex Height", Range(0.0, 0.5)) = 0.08
        _WaveFrequency ("Wave Frequency", Float) = 1.5
        _WaveSpeed ("Wave Speed", Float) = 1.2
        _NormalStrength ("Wave Normal Strength", Range(0.1, 5.0)) = 1.5
        _Smoothness ("Sun Reflection Smoothness", Range(0.5, 1.0)) = 0.96
        _FresnelPower ("Fresnel Power", Range(1.0, 8.0)) = 3.5

        [Header(Refraction)]
        _RefractionStrength ("Underwater Distortion", Range(0.0, 0.1)) = 0.03
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent-100" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector" = "True"
        }

        LOD 300
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float4 _FoamColor;
                float _DepthMaxDistance;
                float _FoamDistance;
                float _FoamNoiseScale;
                float _FoamSpeed;
                float _WaveHeight;
                float _WaveFrequency;
                float _WaveSpeed;
                float _NormalStrength;
                float _Smoothness;
                float _FresnelPower;
                float _RefractionStrength;
            CBUFFER_END

            // Procedural wave displacement function
            float CalculateWaveElevation(float2 pos, float time)
            {
                float w1 = sin(pos.x * _WaveFrequency + time * _WaveSpeed) * cos(pos.y * (_WaveFrequency * 0.8) + time * (_WaveSpeed * 0.9));
                float w2 = sin((pos.x + pos.y) * (_WaveFrequency * 1.5) + time * (_WaveSpeed * 1.3));
                float w3 = cos(pos.x * (_WaveFrequency * 2.2) - time * (_WaveSpeed * 0.7));
                return (w1 * 0.5 + w2 * 0.3 + w3 * 0.2) * _WaveHeight;
            }

            // Pseudo-random hash for procedural foam noise
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 uv)
            {
                float2 id = floor(uv);
                float2 f = frac(uv);
                float2 u = f * f * (3.0 - 2.0 * f);

                float bl = Hash21(id);
                float br = Hash21(id + float2(1.0, 0.0));
                float tl = Hash21(id + float2(0.0, 1.0));
                float tr = Hash21(id + float2(1.0, 1.0));

                return lerp(lerp(bl, br, u.x), lerp(tl, tr, u.x), u.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float time = _Time.y;

                // Animate vertex height
                float wave = CalculateWaveElevation(worldPos.xz, time);
                worldPos.y += wave;

                output.positionWS = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float time = _Time.y;

                // 1. Calculate Analytical Wave Normal using Finite Differences
                float eps = 0.15;
                float hC = CalculateWaveElevation(input.positionWS.xz, time);
                float hR = CalculateWaveElevation(input.positionWS.xz + float2(eps, 0.0), time);
                float hT = CalculateWaveElevation(input.positionWS.xz + float2(0.0, eps), time);

                float dHdX = (hR - hC) / eps;
                float dHdZ = (hT - hC) / eps;
                float3 waveNormalWS = normalize(float3(-dHdX * _NormalStrength, 1.0, -dHdZ * _NormalStrength));

                // 2. Refraction & Underwater Scene Sampling
                float2 distortedScreenUV = screenUV + waveNormalWS.xz * _RefractionStrength;

                // Sample Scene Depth
                float rawDepth = SampleSceneDepth(distortedScreenUV);
                float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float surfaceEyeDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                float waterDepth = max(0.0, sceneEyeDepth - surfaceEyeDepth);

                // If distortion samples above water geometry, fallback to undistorted
                if (waterDepth <= 0.0)
                {
                    distortedScreenUV = screenUV;
                    rawDepth = SampleSceneDepth(distortedScreenUV);
                    sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                    waterDepth = max(0.0, sceneEyeDepth - surfaceEyeDepth);
                }

                half3 underwaterColor = SampleSceneColor(distortedScreenUV);

                // 3. Depth-Based Color Absorption (Beer-Lambert Approximation)
                float depthFactor = saturate(waterDepth / max(0.01, _DepthMaxDistance));
                half4 waterBodyColor = lerp(_ShallowColor, _DeepColor, depthFactor);

                // Blend underwater scene with water body color
                half3 finalColor = lerp(underwaterColor, waterBodyColor.rgb, waterBodyColor.a);

                // 4. Shore Foam (Animated Noise at Contact Edges)
                float foamFactor = 1.0 - saturate(waterDepth / max(0.01, _FoamDistance));
                if (foamFactor > 0.01)
                {
                    float2 foamUV = input.positionWS.xz * _FoamNoiseScale + float2(time * _FoamSpeed, time * _FoamSpeed * 0.7);
                    float foamNoise = ValueNoise(foamUV);
                    foamNoise = (foamNoise + ValueNoise(foamUV * 2.0 - time * 0.4)) * 0.5;

                    float foamCutoff = step(0.45, foamNoise + foamFactor * 0.5);
                    float foamIntensity = foamFactor * foamCutoff;
                    finalColor = lerp(finalColor, _FoamColor.rgb, foamIntensity * _FoamColor.a);
                }

                // 5. Fresnel Effect (Glancing Angle Reflection)
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);
                float NdotV = saturate(dot(waveNormalWS, viewDirWS));
                float fresnel = pow(1.0 - NdotV, _FresnelPower);
                half3 skyReflectionColor = half3(0.65, 0.85, 1.0); // Subtle sky reflection
                finalColor = lerp(finalColor, skyReflectionColor, fresnel * 0.5);

                // 6. Directional Sun Light & Specular Sparkle
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 halfVector = normalize(lightDir + viewDirWS);
                float NdotH = saturate(dot(waveNormalWS, halfVector));
                float specular = pow(NdotH, _Smoothness * 256.0);
                half3 sunSpecular = mainLight.color * specular * _Smoothness;
                finalColor += sunSpecular;

                float finalAlpha = saturate(waterBodyColor.a + fresnel * 0.3 + foamFactor * 0.8);
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
