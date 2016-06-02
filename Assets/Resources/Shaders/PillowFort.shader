Shader "Custom/PillowFort" {



    SubShader{
//        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Cull off
        Pass{

           // Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
 
            CGPROGRAM
            #pragma target 5.0
 
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
            #include "Chunks/uvNormalMap.cginc"
            #include "Chunks/noise.cginc"

            uniform sampler2D _NormalMap;
            uniform samplerCUBE _CubeMap;

            uniform float _StartTime;
            uniform float _FullEnd;
            uniform int _NumShapes;
            uniform int _NumberHands;

            uniform float _Cycle;
            uniform float _Special;
            uniform float _ClothDown;
            uniform int _Large;
 

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

            struct Hand{
  float active;
  float3 pos;
  float3 vel;
  float3 aVel;
  float  triggerVal;
  float  thumbVal;
  float  sideVal;
  float2 thumbPos;
};

            
            struct Pos {
                float3 pos;
            };

            struct Shape{
	float4x4 mat;
	float shape;
    float active;
    float hovered;
    float jiggleVal;
};

            StructuredBuffer<Vert> buf_Points;
            StructuredBuffer<Shape> shapeBuffer;
            StructuredBuffer<Hand > handBuffer;

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
                float closest   : TEXCOORD6;
                float life 			: TEXCOORD5;
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

           float sdBox( float3 p, float3 b , float3 s ){
						  float3 d = abs(p) - b;
						  float x = max( d.x / s.x , 0 );
						  float y = max( d.y / s.y , 0 );
						  float z = max( d.z / s.z , 0 );
						  return min(max(x,max(y,z)),0.0) +
						         length(max(d/s,0.0));
						}

float boxDistance( float3 p , float4x4 m ){
    float3 s = float3(  length( float3( m[0][0] , m[0][1] , m[0][2] ) ),
    					length( float3( m[1][0] , m[1][1] , m[1][2] ) ),
    					length( float3( m[2][0] , m[2][1] , m[2][2] ) ) );
		//p *= s;
    float4 q = (mul( m , float4( p.x , p.y , p.z , 1. )));




    return sdBox( q.xyz , float3( .55 , .55 ,  .55) , s);

}

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (uint id : SV_VertexID){

                varyings o;

                // from getRibbonID 
                uint fID = getID( id );
                Vert v = buf_Points[fID];

                

                o.worldPos = v.pos;//mul( worldMat , float4( v.pos , 1.) ).xyz;

                if( _Large == 1 ){
                    o.worldPos /= 5;
                }

                o.eye = _WorldSpaceCameraPos - o.worldPos;

                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

                float closestHand = 1000;

                for( int i =0; i< _NumberHands; i++){
                    Hand h = handBuffer[i];
                    float3 dif = h.pos - v.pos;
                    if( length( dif) < closestHand ){
                        closestHand = length( dif );
                    }
                }
                o.closest = closestHand;


                o.debug = v.debug;//n * .5 + .5;
                //o.debug = v.norm * .5 + .5;
                o.life = v.life;
                o.uv = v.uv;
                o.nor = -v.norm;

            
                return o;

            }
 
            //Pixel function returns a solid color for each point.
            float4 frag (varyings i) : COLOR {


                float3 fNorm = uvNormalMap( _NormalMap , i.worldPos ,  i.uv  * float2( 1 , 1), i.nor * .5 + .5, 10.1 * (_Cycle + .3) , 3 * _Cycle+.2);

                float3 col = fNorm * .5 + .5;//i.debug;

                float3 fRefl = reflect( -normalize(i.eye) , fNorm );
                float3 cubeCol = texCUBE(_CubeMap,fRefl ).rgb;

                col =  col * cubeCol * 2;

                col = float3( 1 , .7 , .5 ) * cubeCol  * 2;

                float f = 100000;
                int closest = 0;

                float distVal =1000;  

                float dVal = .3 * noise( float3(200 * i.uv.x , 200 * i.uv.y ,0) );
                    dVal += .6 * noise( float3(40 * i.uv.x , 40 * i.uv.y , 0 ) );
                    dVal += noise( float3(80 * i.uv.x , 80 * i.uv.y , 0 ) );

                if( dVal > _ClothDown * 10 ){ discard;}
                if( _Special == 1. ){

                    col =  2 *  cubeCol * (fNorm * .5 +.5);

                  
                    if( dVal > i.closest * i.closest * i.closest * 10){ discard; }

                    distVal = i.closest * i.closest * i.closest * 10 - dVal;


                }


                float3 p = i.worldPos;

                if( _Large == 1 ){ p *= 5; }

                for( int j = 0; j < _NumShapes; j++ ){
       

  								float l = boxDistance( p , shapeBuffer[j].mat );

  								///col.x = l * .1;
                                if( l < f ){
                                    f = l;
                                    closest = j;
                                }
  								



  							}



  							if( f  < 0 ){ col = i.nor; }else{ 
  								col = col * (1 / (f * 6. + .1 ));

                                if( shapeBuffer[closest].hovered > 0 ){
                                    float3 fullCol = float3( .5 , .8 , 1.6 );
                                    float3 noCol = float3( 1.5 , 1 , .6 );

                                    col = lerp( col , col * lerp( fullCol , noCol , _Cycle ), clamp(1 - f * 2, 0,1));
                                }
  							}// / max( .5 , f / 5);} //float3( .1 / f , 0 , 0 );}
  							//col /= max(0.04 , f* 2);

  							float match = (1-abs(dot( -normalize(i.eye) , fNorm )));

                            float l = max(min( i.worldPos.y, 1 ),-1);
  							col = lerp( float3( 2. , 1.6 , .9) * match  + col * (1-match), col , max( 0 , l ) );

                             if( _Special == 1. ){

                                col =   2 *  cubeCol * (fNorm * .5 +.5) * ((1 / (f * 6. + .1 ))+ .5);
                                if( distVal  < .3 ){ col = 3 *  cubeCol * (fNorm * .5 +.5);}
                            }

  							//col += float3( 0 , 0 , .5 );

								col = pow(col,  2.2); 

                                col *= ( 1 - _FullEnd ); 

                                float3 noMoonCol = col;
                                float3 fullMoonCol = (col * float3( .3 , 2 , 4 ));

                                col = lerp( fullMoonCol , noMoonCol , _Cycle );

                fixed4 color;
               // col = float3( _Cycle , _Cycle , _Cycle );
               // col = float3( 1 , 1 , 1 );
	            	color = fixed4( col , 1. );
	            	return color;
	           

            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
	
}