Shader "Unlit/StepsDraw"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
		_TextureCoordinate("Coordinate", Vector) = (0, 0, 0, 0)
		_DrawColour("Draw Colour", Color) = (1, 0, 0, 0)
		_BrushSize("Brush Size", Range(0, 1000)) = 100
		_BrushStrength("Brush Strength", Range(0, 1)) = 1
    }
	
    SubShader
    {
        Tags {"RenderType" = "Opaque"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

			sampler2D 	_MainTex;
            float4 		_MainTex_ST;
			float4 		_TextureCoordinate;
			float4 		_DrawColour;
			float 		_BrushSize;
			float 		_BrushStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				float strength = pow(saturate(1 - distance(i.uv, _TextureCoordinate.xy)), 1024 / _BrushSize);
				fixed4 draw_colour = _DrawColour * (strength * _BrushStrength);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return saturate(col + draw_colour);
            }
            ENDCG
        }
    }
}
