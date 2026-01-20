Shader "UI/SimpleGlitch_UI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _GlitchIntensity ("Glitch Intensity", Range(0,1)) = 0.1
        _BlockScale ("Block Scale", Range(1,50)) = 10
        _NoiseSpeed ("Noise Speed", Range(1,10)) = 10
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            float _GlitchIntensity;
            float _BlockScale;
            float _NoiseSpeed;

            // ------------------------------
            // ã^éóóêêîê∂ê¨
            // ------------------------------
            float Random(float2 seeds)
            {
                return frac(sin(dot(seeds, float2(12.9898, 78.233))) * 43758.5453);
            }

            float BlockNoise(float2 seeds)
            {
                return Random(floor(seeds));
            }

            float SignedNoise(float2 seeds)
            {
                return -1.0 + 2.0 * BlockNoise(seeds);
            }

            // ------------------------------
            // Vertex
            // ------------------------------
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                o.worldPosition = v.vertex;
                return o;
            }

            // ------------------------------
            // Fragment
            // ------------------------------
            fixed4 frag (v2f i) : SV_Target
            {
                // UIÉ}ÉXÉNëŒâû
                #ifdef UNITY_UI_CLIP_RECT
                if (UnityGet2DClipping(i.worldPosition.xy, _ClipRect) == 0)
                {
                    discard;
                }
                #endif

                float2 uv = i.uv;

                float noise = BlockNoise(uv.y * _BlockScale);
                noise += Random(uv.x) * 0.3;

                float randomValue = SignedNoise(float2(uv.y, _Time.y * _NoiseSpeed));

                uv.x += randomValue
                        * sin(sin(_GlitchIntensity) * 0.5)
                        * sin(-sin(noise) * 0.2)
                        * frac(_Time.y);

                fixed4 col;
                col.r = tex2D(_MainTex, uv + float2(0.006, 0)).r;
                col.g = tex2D(_MainTex, uv).g;
                col.b = tex2D(_MainTex, uv - float2(0.008, 0)).b;
                col.a = tex2D(_MainTex, uv).a;

                return col * i.color;
            }
            ENDCG
        }
    }
}