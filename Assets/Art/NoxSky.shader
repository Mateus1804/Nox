Shader "Nox/Ceu espiritual"
{
    Properties
    {
        _Top ("Zênite", Color) = (0.035,0.09,0.18,1)
        _Horizon ("Horizonte", Color) = (0.38,0.63,0.65,1)
        _Bottom ("Névoa inferior", Color) = (0.15,0.27,0.33,1)
        _SunColor ("Luz distante", Color) = (1,0.79,0.48,1)
        _SunDirection ("Direção da luz", Vector) = (0.4,0.45,0.7,0)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float3 ray : TEXCOORD0; };
            float4 _Top, _Horizon, _Bottom, _SunColor, _SunDirection;
            v2f vert(appdata v)
            {
                v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.ray = v.vertex.xyz; return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float3 ray = normalize(i.ray);
                float elevation = saturate(ray.y);
                float3 sky = lerp(_Horizon.rgb, _Top.rgb, pow(elevation, 0.65));
                sky = lerp(sky, _Bottom.rgb, saturate(-ray.y * 3));
                float sun = dot(ray, normalize(_SunDirection.xyz));
                sky += _SunColor.rgb * (smoothstep(0.9984, 0.9993, sun) * 0.8 + pow(saturate(sun), 48) * 0.09);
                return fixed4(sky, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
