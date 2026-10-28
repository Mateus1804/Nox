Shader "Nox/Superficie mineral"
{
    Properties
    {
        _Color ("Cor base", Color) = (.3,.4,.4,1)
        _Glossiness ("Suavidade", Range(0,1)) = .08
        _EmissionColor ("Emissão", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color, _EmissionColor;
        half _Glossiness;
        struct Input { float3 worldPos; };
        float hash(float3 p) { return frac(sin(dot(p,float3(12.9898,78.233,37.719)))*43758.5453); }
        float noise(float3 p)
        {
            float3 i=floor(p), f=frac(p); f=f*f*(3-2*f);
            return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),
                lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+float3(1,1,1)),f.x),f.y),f.z);
        }
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float broad=noise(IN.worldPos*.65), fine=noise(IN.worldPos*4.5);
            // Variação mineral em coordenadas de mundo: escala consistente em plataformas grandes.
            o.Albedo=_Color.rgb*(.84+.22*broad+.07*fine);
            o.Metallic=0; o.Smoothness=_Glossiness; o.Occlusion=1;
            o.Emission=_EmissionColor.rgb; o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Standard"
}
