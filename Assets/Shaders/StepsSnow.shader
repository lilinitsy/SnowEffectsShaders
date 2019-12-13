// This is going to use distance-based tessellation method
Shader "Custom/StepsSnow"
{
    Properties
    {
		_Tess("Tesselation",  Range(1, 32)) = 4
		_DisplacementMap("Displacement Texture", 2D) = "black" {}
		_Displacement("Displacement", Range(0, 1.0)) = 0.15
        _SnowColour("Snow Colour", Color) = (1, 1, 1, 1)
        _SnowTexture("Snow Albedo (RGB)", 2D) = "white" {}
		_GroundColour("Ground Colour", Color) = (1, 1, 1, 1)
		_GroundColour2("Ground Colour 2", Color) = (1, 1, 1, 1)
        _GroundTexture("Ground Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags {"RenderType"="Opaque"}
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:disp tessellate:tessDistance

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.6

		half 		_Glossiness;
        half 		_Metallic;
        float4 		_GroundColour;
		float4		_GroundColour2;
		float4 		_SnowColour;
		float		_Displacement;
		float 		_Tess;
        sampler2D 	_GroundTexture;
		sampler2D 	_SnowTexture;
		sampler2D 	_DisplacementMap;

		//////////////////////
		/// COPIED FROM DISPLACEMENT TESSELATION EXAMPLE
		//////////////////////
		#include "Tessellation.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
		};


		float4 tessDistance(appdata v0, appdata v1, appdata v2)
		{
			float minDist = 10.0;
			float maxDist = 25.0;
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
		}


		// VERTEX OFFSET IS DONE HERE
		void disp(inout appdata v)
		{
			float d = tex2Dlod(_DisplacementMap, float4(v.texcoord.xy,0,0)).r * _Displacement;
			if(d > 0.1)
			{
				v.vertex.xyz -= v.normal * d;
			}

			else
			{
				v.vertex.xyz += v.normal * d;
			}
			// It's something here to get the snow to appear, but not sure what
			// maybe look at percentage away from brush size max?
			// Will have to have _BrushSize or something too then
			// +v.normal*d = raise, so manipulate d somehow or lerp it...
			v.vertex.xyz += v.normal * _Displacement;
		}

		//////////////////////
		/// END COPY
		//////////////////////

        struct Input
        {
            float2 uv_GroundTexture;
			float2 uv_SnowTexture;
			float2 uv_DisplacementMap;
        };


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
			float displacement_val = tex2Dlod(_DisplacementMap, float4(IN.uv_DisplacementMap, 0, 0)).r;
			float4 c;
			if(displacement_val > 0)
			{
				c = lerp(tex2D(_SnowTexture, IN.uv_SnowTexture) * _SnowColour, tex2D(_GroundTexture, IN.uv_GroundTexture) * _GroundColour, displacement_val);
			}

			else
			{
				c = lerp(tex2D(_SnowTexture, IN.uv_SnowTexture) * _SnowColour, tex2D(_GroundTexture, IN.uv_GroundTexture) * _GroundColour2, displacement_val);
			}
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
