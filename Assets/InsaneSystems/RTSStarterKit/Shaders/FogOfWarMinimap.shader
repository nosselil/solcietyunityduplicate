// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt). Modified by Insane Systems

Shader "Insane Systems/Fog of War Minimap"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            uniform float _Enabled;
            uniform sampler2D _FOWVisionRadiusesTexture;
            uniform sampler2D _FOWMinimapPositionsTexture;
            uniform float _ActualUnitsCount;
            uniform float _MaxUnits;
            uniform float _MapSize;
            uniform float _MinimapSize;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                float alpha = 0.8;
			    float2 pos = IN.texcoord;
			    	
                if (_Enabled)
                {			
                    float2 textureResolution = float2(_MaxUnits, 1);
			    
                    for (int i = 0; i < _ActualUnitsCount; i++)
                    {
                        float2 unitPixelCenterPos = float2(i, 0) + 0.5; // 0.5 to sample center of pixel due we work in texel space
                        float2 unitIconPosition = tex2D(_FOWMinimapPositionsTexture, unitPixelCenterPos / textureResolution).rg * 1024 / _MapSize; //_Positions[i].xyz;
                        float visionRadius = tex2D(_FOWVisionRadiusesTexture, unitPixelCenterPos / textureResolution).r * 512;
                    
                        //float2 unitIconPosition = _Positions[i].xy / _MapSize; // old code
                        
                        float distanceToUnit = distance(unitIconPosition, pos) * _MinimapSize;
                        
                        if (distanceToUnit < visionRadius)
                        {
                            float size = visionRadius - distanceToUnit;
                            
                            if (size < 1)
                                alpha = lerp(alpha, 0, size); // Insane Systems: previous alpha used because this sector can be already visible by other unit.
                            else 
                                alpha = 0;
                        }
                    }
        
                    alpha = clamp(alpha, 0, 0.8);
                }
                else
                {
                    alpha = 0;
                }
                 
                color.a = alpha;
                color.rgb = float3(0, 0, 0);
			
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
