Shader "Custom/DarkIsInvisible" {
    Properties {
        _Color ("Main Color", Color) = (1,0.2028302,0.2797437,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }
 
    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="False" "RenderType"="Transparent"}
        LOD 200
 
    CGPROGRAM
    #pragma surface surf AlphaLambert noambient alpha finalcolor:setalpha
 
    sampler2D _MainTex;
    fixed4 _Color;
 
    struct Input {
        float2 uv_MainTex;
    };
   
    float lightAlpha;
    half4 LightingAlphaLambert (SurfaceOutput s, half3 lightDir, half atten) {
        half4 c;
        c.rgb = _LightColor0.rgb * atten;
        c.a = s.Alpha;
        lightAlpha = max(_LightColor0.r, max(_LightColor0.g, _LightColor0.b)) * atten;
       
        return c;
    }
   
    void setalpha(Input IN, SurfaceOutput o, inout fixed4 color)
    {
        color.a = lightAlpha;
        return;
    }
 
    void surf (Input IN, inout SurfaceOutput o) {
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        o.Albedo = c.rgb;
        o.Alpha = min(o.Alpha, c.a);
    }
    ENDCG
    }
 
    Fallback "Transparent/VertexLit"
}