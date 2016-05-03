Shader "Custom/ClothDebug" {



    SubShader{
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Cull off
        Pass{

            Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
 
            CGPROGRAM
            #pragma target 5.0
 
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
 

           struct Vert{
						float3 pos;
						float3 oPos;
						float3 ogPos;
						float3 norm;
						float2 uv;
						float life;
						float ids[8];
						float3 debug;
					};

            
            struct Pos {
                float3 pos;
            };

            StructuredBuffer<Vert> buf_Points;

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

     
     		float3 getSecondPos( int id , float3 dPos ){
     			if( id < 0 ){ return dPos; }else{
     				return buf_Points[ id ].pos;
     			}
     		}

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (uint id : SV_VertexID){

                varyings o;




                // from getRibbonID 
                int fID = floor( id / 16 );
                //fID += 10;
                Vert v = buf_Points[fID];
 
 								o.debug = float3(v.life ,1,1 );

                if( (id % 2) == 0){
                	o.worldPos = v.pos;
                }else{
                	uint bID = floor( (id % 16)/2 );
                	o.worldPos = getSecondPos( v.ids[bID] , v.pos );

                }



                o.worldPos += float3( 0 , .0001 , 0 );

                //do.worldPos = mul( worldMat , float4( o.worldPos , 1.) ).xyz;
              

                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
               

            
                return o;

            }
 
            //Pixel function returns a solid color for each point.
            float4 frag (varyings i) : COLOR {

                float3 col = i.debug;

                return float4(  1 , 1 , 1 , 1-i.debug.x);

            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
	
}