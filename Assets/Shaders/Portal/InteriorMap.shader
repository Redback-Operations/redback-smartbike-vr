Shader "Volume/InteriorMappingCubeVF_ObjectSpace"
{
    Properties
    {
        _Cube ("Interior Cubemap", Cube) = "" {}
        _Rotation ("Rotation (Degrees)", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+2" }
        LOD 100
        
        Cull Front
        Zwrite Off
        
        Blend SrcAlpha One
        
        Stencil
        {
            Ref 2
            Comp Equal
        }


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            samplerCUBE _Cube;
            float _Rotation;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 objPos : TEXCOORD0; // Object space position
            };

            v2f vert(appdata v)
            {
                v2f o;
                // Transform to clip space.
                o.pos = UnityObjectToClipPos(v.vertex);
                // Pass the object space vertex position.
                o.objPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Convert rotation angle from degrees to radians.
                float rad = _Rotation * 0.0174533;
                float s = sin(rad);
                float c = cos(rad);

                // Apply a rotation around the Y axis to the object-space position.
                float3 rotatedPos;
                rotatedPos.x = c * i.objPos.x + s * i.objPos.z;
                rotatedPos.y = i.objPos.y;
                rotatedPos.z = -s * i.objPos.x + c * i.objPos.z;
                
                // Use texCUBElod to sample the cubemap with the unnormalized (rotated) position.
                // The fourth component (LOD) is set to 0.
                fixed4 cubeColor = texCUBElod(_Cube, float4(rotatedPos, 0.0));
                return cubeColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
