Shader "Insane Systems/Fog Of War"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MapSize("Map Size", Range(16, 512)) = 256
		_FogStrength("Fog Strength", Range(0, 1)) = 0.8
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		ZTest Always // для того, чтобы перекрывать вообще всё

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:fade

		#pragma target 2.0

		sampler2D _MainTex;
		float _MapSize;
		float _FogStrength;
		
		uniform float _Enabled;
        uniform sampler2D _FOWVisionRadiusesTexture;
        uniform sampler2D _FOWPositionsTexture;
        uniform float _ActualUnitsCount;
        uniform float _MaxUnits;
        
		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
			float3 screenPos;
		};

		fixed4 _Color;

		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		float random(in float2 st)
		{
			return frac(sin(dot(st.xy,
				float2(12.9898, 78.233)))
				* 43758.5453123);
		}

		float noise(in float2 st)
		{
			float2 i = floor(st);
			float2 f = frac(st);

			float a = random(i);
			float b = random(i + float2(1.0, 0.0));
			float c = random(i + float2(0.0, 1.0));
			float d = random(i + float2(1.0, 1.0));

			float2 u = f * f*(3.0 - 2.0*f);

			return lerp(a, b, u.x) +
				(c - a)* u.y * (1.0 - u.x) +
				(d - b) * u.x * u.y;
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float mapSize = _MapSize;

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            float alpha = _FogStrength;
			float3 pos = IN.worldPos;
			pos.y = 0;

			float2 worldUV = IN.worldPos.xz + _Time.y * 1; // IN.screenPos.xy / IN.screenPos.w;
			float xCoordUnscaled = noise(worldUV * 0.1);
			float alphaNoise = noise(worldUV * 0.3);
			
			if (_Enabled)
			{			
			    float2 textureResolution = float2(_MaxUnits, 1);
			    
                for (int i = 0; i < _ActualUnitsCount; i++)
                {
                    float2 unitPixelCenterPos = float2(i, 0) + 0.5; // 0.5 to sample center of pixel due we work in texel space
                    float3 unitPosition = tex2D(_FOWPositionsTexture, unitPixelCenterPos / textureResolution).rgb * 1024; //_Positions[i].xyz;
                    unitPosition.y = 0; // todo offset by unit height.
                    
                    float visionRadius = tex2D(_FOWVisionRadiusesTexture, unitPixelCenterPos / textureResolution).r * 512;
                    
                    float distanceToUnit = distance(unitPosition, pos);
                    
                    if (distanceToUnit < visionRadius)
                    {
                        float size = visionRadius - distanceToUnit;
                        
                        if (size < 1)
                            alpha = lerp(alpha, 0, size); // Insane Systems: previous alpha used because this sector can be already visible by other unit.
                        else 
                            alpha = 0;
                    }
                }
    
                alpha = clamp(alpha, 0, _FogStrength);
			}
			else
			{
			    alpha = 0;
			}
             
			c.a = alpha;
			c.rgb = xCoordUnscaled * 0.03; // brightness
			c.rgb = float3(0, 0, 0);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
