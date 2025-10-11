Shader "Slider/Segmented" {
    Properties {
        [Header(Main Settings)]
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Progress("Progress", Range(0,1)) = 1

        [Header(HP Settings)]
        _MaxHP("Max HP", Float) = 1000

        [Header(Segment Small Line Settings)]
        _StepSmall("Small Line Step", Float) = 100
        _LineColorSmall("Small Line Color", Color) = (1,1,1,0.5)
        _LineThicknessSmall("Small Line Thickness", Float) = 5
        _LineLengthSmall("Small Line Length", Range(0.01,1)) = 0.2

        [Header(Segment Large Line Settings)]
        _StepLarge("Large Line Step", Float) = 500
        _LineColorLarge("Large Line Color", Color) = (1,1,0,1)
        _LineThicknessLarge("Large Line Thickness", Float) = 10
        _LineLengthLarge("Large Line Length", Range(0.01,1)) = 0.4

        [Header(Vertical Anchor)]
        _AnchorFromTop("Anchor From Top (0=bottom,1=top)", Range(0,1)) = 0
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize; // x = 1/width, y = 1/height, z = width, w = height
    float4 _MainTex_ST; // x = offset.x, y = offset.y, z = scale.x, w = scale.y
    float _Progress;
    float _MaxHP;
    float _StepSmall;
    float _StepLarge;
    float4 _LineColorSmall;
    float4 _LineColorLarge;
    float _LineThicknessSmall;
    float _LineThicknessLarge;
    float _LineLengthSmall;
    float _LineLengthLarge;
    float _AnchorFromTop;

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

    float4 frag(v2f i):COLOR{
        float2 uv = i.uv;
        float4 baseColor = tex2D(_MainTex, uv);

        // progress clip (horizontal)
        if(uv.x > _Progress)
        {
            baseColor.a = 0;
            return baseColor;
        }

        // compute usable tex width
        float texWidth = _MainTex_TexelSize.z;

        float uvThicknessLarge = _LineThicknessLarge / texWidth;
        float uvThicknessSmall = _LineThicknessSmall / texWidth;

        // vertical coordinate, allow anchoring from top or bottom
        float yCoord = (_AnchorFromTop > 0.5) ? (1.0 - uv.y) : uv.y;

        float hpAtUV = uv.x * _MaxHP;
        float largeLineDist = abs(fmod(hpAtUV, _StepLarge));
        float largeLineAlpha = step(largeLineDist, uvThicknessLarge * _MaxHP);

        if (yCoord >= _LineLengthLarge) largeLineAlpha = 0.0;

        // maxHP가 largeLineStep으로 나누어떨어지는지 판별
        float epsilon = 0.001;
        bool isDivisible = abs(fmod(_MaxHP, _StepLarge)) < epsilon;

        // 마지막 위치에서 큰 선 표시 여부 결정 (yCoord 기준)
        if (isDivisible && abs(hpAtUV - _MaxHP) < (uvThicknessLarge * _MaxHP)) {
            largeLineAlpha = 1.0;
        }else if (hpAtUV < (_StepLarge * 0.5)) 
        {
            largeLineAlpha = 0.0;
        }

        float smallLineDist = abs(fmod(hpAtUV, _StepSmall));
        float smallLineAlpha = step(smallLineDist, uvThicknessSmall * _MaxHP) * (1 - largeLineAlpha);

        if (yCoord >= _LineLengthSmall) smallLineAlpha = 0.0;

        float lineAlpha = max(largeLineAlpha, smallLineAlpha);

        // 각각의 선 색상 적용
        float4 largeLineColor = _LineColorLarge;
        float4 smallLineColor = _LineColorSmall;

        // 텍스처 알파값을 선에도 적용
        largeLineColor.a *= baseColor.a;
        smallLineColor.a *= baseColor.a;

        // 최종 컬러 합성
        float4 finalColor = baseColor;
        if (largeLineAlpha > 0.0)
            finalColor = lerp(finalColor, largeLineColor, largeLineAlpha * largeLineColor.a);
        else if (smallLineAlpha > 0.0)
            finalColor = lerp(finalColor, smallLineColor, smallLineAlpha * smallLineColor.a);

        return finalColor;
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
