Shader "Slider/DefaultSlider"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Progress("Progress", Range(0,1)) = 1
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    sampler2D _MainTex;
    float _Progress;

    struct v2f{
        float4 pos:SV_POSITION;
        float2 uv:TEXCOORD;
    };

    v2f vert(appdata_base i){
        v2f o;
        o.pos = UnityObjectToClipPos(i.vertex);
        o.uv = i.texcoord.xy;
        return o;
    }
    
    float4 frag (v2f i) : Color {
        float2 uv = i.uv;
        float4 baseColor = tex2D(_MainTex, uv);

        // 프로그레스에 따라 슬라이더 영역만 표시
        if(uv.x > _Progress)
        {
            baseColor.a = 0;
            return baseColor;
        }
        return baseColor;
    }

    ENDCG

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Pass{
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
    FallBack "Diffuse"
}
