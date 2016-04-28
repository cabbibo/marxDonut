Shader "Custom/ClothProper" {



    SubShader{
//        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Cull off
        Pass{

            Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
 
            CGPROGRAM
            #pragma target 5.0
 
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
 

            struct Vert {

			  float3 pos;
			  float3 oPos;
			  float mass;
			  float o0;
			  float o1;
			  float o2;
			  float o3;
			  float o4;
			  float o5;
			  float o6;
			  float o7;

			};
            
            struct Pos {
                float3 pos;
            };

            StructuredBuffer<Vert> buf_Points;
            StructuredBuffer<Pos> og_Points;

            uniform float4x4 worldMat;

            uniform int _RibbonWidth;
            uniform int _RibbonLength;
            uniform int _TotalVerts;
 
            //A simple input struct for our pixel shader step containing a position.
            struct varyings {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 nor      : TEXCOORD0;
                float3 eye      : TEXCOORD2;
                float3 debug    : TEXCOORD3;
                float2 uv       : TEXCOORD4;
            };

            uint getID( uint id  ){

                uint base = floor( id / 6 );
                uint tri  = id % 6;
                uint row = floor( base / ( _RibbonWidth -1 ) );
                uint col = (base) % ( _RibbonWidth - 1 );

                uint rowU = (row + 1);// % _RibbonLength;
                uint colU = (col + 1);// % _RibbonWidth;

                uint rDoID = row * _RibbonWidth;
                uint rUpID = rowU * _RibbonWidth;

                uint cDoID = col;
                uint cUpID = colU;

                uint fID = 0;

                if( tri == 0 ){
                    fID = rDoID + cDoID;
                }else if( tri == 1 ){
                    fID = rUpID + cDoID;
                }else if( tri == 2 ){
                    fID = rUpID + cUpID;
                }else if( tri == 3 ){
                    fID = rDoID + cDoID;
                }else if( tri == 4 ){
                    fID = rUpID + cUpID;
                }else if( tri == 5 ){
                    fID = rDoID + cUpID;
                }else{
                    fID = 0;
                }

                return fID;

            }
           

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (uint id : SV_VertexID){

                varyings o;

                // from getRibbonID 
                uint fID = getID( id );
                Vert v = buf_Points[fID];
                Pos og = og_Points[fID];

                float3 dif =   - v.pos;

                o.worldPos = v.pos; //mul( worldMat , float4( v.pos , 1.) ).xyz;

                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

                float3 n = float3( 0 , 0 , 0 );

                float3 l = v.pos , u = v.pos , d = v.pos  , r = v.pos;

                if( v.o0 > -1 ){ r = buf_Points[v.o0].pos; }
                if( v.o2 > -1 ){ d = buf_Points[v.o2].pos; }
                if( v.o4 > -1 ){ l = buf_Points[v.o4].pos; }
                if( v.o6 > -1 ){ u = buf_Points[v.o6].pos; }

                n = cross( normalize(l - r) , normalize( u-d));

                n = normalize( n );

                o.debug = n * .5 + .5;
                //o.debug = float3( v.mass , 0 , 0 );

            
                return o;

            }
 
            //Pixel function returns a solid color for each point.
            float4 frag (varyings i) : COLOR {

                float3 col = i.debug;

                return float4( col , 1.);

            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
	
}