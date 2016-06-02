Shader "Custom/Pillows" {

 Properties {
  

    _NumberSteps( "Number Steps", Int ) = 20
    _MaxTraceDistance( "Max Trace Distance" , Float ) = 10.0
    _IntersectionPrecision( "Intersection Precision" , Float ) = 0.0001
    _CubeMap( "Cube Map" , Cube )  = "defaulttexture" {}
    _Fade( "Fade", Int ) = 1



  }


    SubShader{
        //Tags { "RenderType"="Opaque" "Queue" = "Opaque" }
        Cull off
        Pass{

            //Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
 
            CGPROGRAM
            #pragma target 5.0
 
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
            #include "Chunks/uvNormalMap.cginc"


            uniform sampler2D _NormalMap;
            uniform samplerCUBE _CubeMap;

            uniform float _StartTime;
            uniform float _FadeTime;
            uniform float _RealTime;

            uniform int _NumberSteps;
            uniform int _NumberHands;
			uniform int _Fade;

            uniform sampler2D _Audio;
            uniform float _Special;

            uniform int _Large;

 

            struct Vert{
							float3 pos;
							float3 oPos;
							float3 ogPos;
							float3 norm;
							float2 uv;
							float boxID;
							float ids[8];
							float3 debug;
						};

						struct Shape{
							float4x4 mat;
							float shape;
                            float active;
                            float hovered;
                            float jiggleVal;
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

            
            StructuredBuffer<Vert> buf_Points;
            StructuredBuffer<Shape> shapeBuffer;
            StructuredBuffer<Hand> handBuffer;
            


            uniform float4x4 worldMat;

            uniform int _RibbonWidth;
            uniform int _RibbonLength;
            uniform int _TotalVerts;

            uniform float _Cycle;
            uniform float _ClothDown;
 
            //A simple input struct for our pixel shader step containing a position.
            struct varyings {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 nor      : TEXCOORD0;
                float started   : TEXCOORD7;
                float3 eye      : TEXCOORD2;
                float3 debug    : TEXCOORD3;
                float2 uv       : TEXCOORD4;
                float secondVal : TEXCOORD6;

                float life 			: TEXCOORD5;
            };

			float hash( float n )
			{
			    return frac(sin(n)*43758.5453);
			}
 

            uint getID( uint id  ){

                uint base = floor( id / 6 );
                uint tri  = id % 6;
                uint box = floor( base / ((_RibbonWidth-1) * ( _RibbonLength-1) * 6) );
                uint inBoxID = base  % ( (_RibbonWidth-1) * (_RibbonLength-1) * 6 );
                uint face = floor( inBoxID/ ((_RibbonWidth-1) * ( _RibbonLength-1)) );
                uint inFaceID = base % ( (_RibbonWidth-1) * (_RibbonLength-1));
          
                uint row = floor( inFaceID / ( _RibbonWidth -1 ) );
                uint col = (inFaceID) % ( _RibbonWidth - 1 );

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

                fID = fID + box  * _RibbonLength * _RibbonWidth * 6  + face * _RibbonWidth * _RibbonLength;

                return fID;

            }
           

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (uint id : SV_VertexID){

                varyings o;

                // from getRibbonID 
                uint fID = getID( id );
                Vert v = buf_Points[fID];

                uint base = floor( id / 3 );
                uint tri  = id % 3;

                float3 rPos = float3( hash( float( base * 100 ) + .16123 ) , hash( float( base * 511 ) + .163 ) , hash( float( base * 1526 ) ) + .16123 );
                float3 rDir = float3( hash( float( base * 60012 ) + .9523 ) , hash( float( base * 6000012 ) + .953 ) , hash( float( base * 6151136 ) ) + .8612 );
                rDir = normalize( rDir - float3( .5 , .1 , .5 ));
                float rSize = hash( float( base * 105153) + 512 * hash( float( base * 8522)));
                rSize *= ( .01 + (.5 -  abs( _Cycle-.5)) * .2 );
                rSize += .05 * (.5 - abs( _Cycle-.5));


                

                float3 triPos = (rPos-float3( .5 , .5 , .5 )) * 20;

                float closest = 1000000000;

                float3 closeVec;
                float3 closeHand;

                for( int i = 0; i< _NumberHands; i++ ){
                    Hand h = handBuffer[i];

                    float3 dif = triPos - h.pos;

                    if( length( dif ) < closest ){
                        closest = length( dif );
                        closeVec = dif;
                        closeHand = h.pos;
                    }

                    
                }

                if( closest < .7 ){
                    rDir = lerp(  rDir , normalize( closeVec ) , clamp( (.7 -  length( closeVec )) * 5 , 0 , 1)) ;
                } 

                if( closest < .5 ){
                    triPos = closeHand + normalize( closeVec ) * .5;
                }

                float3 dir = rDir;//_WorldSpaceCameraPos - rPos;

                float3 x = cross( dir , float3( 0 , 1 , 0 ) );
                float3 y = cross( x , dir );
                x = cross( y , dir );

                x = normalize( x );
                y = normalize( y );



                if( tri == 0 ){ triPos += x * rSize * .66; }
                if( tri == 1 ){ triPos += -x * rSize * .66; }
                if( tri == 2 ){ triPos += y* rSize; }




                float3 l = v.pos;
                float3 r = v.pos;
                float3 u = v.pos;
                float3 d = v.pos;

                if( v.ids[0] >= 0 ){ r = buf_Points[ v.ids[0] ].pos; }
                if( v.ids[2] >= 0 ){ d = buf_Points[ v.ids[2] ].pos; }
                if( v.ids[4] >= 0 ){ l = buf_Points[ v.ids[4] ].pos; }
                if( v.ids[6] >= 0 ){ u = buf_Points[ v.ids[6] ].pos; }

                o.uv = v.uv;
                o.nor = normalize(cross( l - r , u - d )); //v.norm;

             
                float3 dif =   - v.pos;

                Shape s = shapeBuffer[ int( v.boxID )];
                o.started = clamp( s.active , 0 , 1 );

                o.worldPos = lerp( triPos , v.pos , o.started + s.jiggleVal * .001 ); ///mul( worldMat , float4( v.pos , 1.) ).xyz;

                //if( _Large == 1 ){ o.worldPos *= 5; }
                o.eye = _WorldSpaceCameraPos - o.worldPos;

                float3 aCol = tex2Dlod( _Audio , float4( length( v.uv - .5 ) * .1 , 0 ,0,0)).xyz;
                //float3 aCol = tex2D( _Audio , float2(dot( normalize(cross( l - r , u - d )) , _WorldSpaceCameraPos - v.pos)* .5 +v.uv.x * .3 , 0)).xyz;

               // o.worldPos += o.nor * length( aCol ) * .1;


                //o.worldPos = lerp( triPos , v.pos , _StartedVal ); ///mul( worldMat , float4( v.pos , 1.) ).xyz;


                o.eye = _WorldSpaceCameraPos - o.worldPos;

                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));



                o.secondVal = clamp( s.active - 1 , 0 , 1 );


                o.debug = float3( s.hovered , s.shape , s.jiggleVal);//n * .5 + .5;
                //o.debug = v.norm * .5 + .5;
                o.life = v.boxID;
                

            
                return o;

            }
 
            //Pixel function returns a solid color for each point.
            float4 frag (varyings i) : COLOR {

            	float3 col = float3( 0.0 , 0.0 , 0.0 );			    		
			    		col= float3( 0. , 0. , 0. );

			     float3 fNorm = uvNormalMap( _NormalMap , i.worldPos ,  i.uv  * float2( 1 , 1), i.nor * .5 + .5, 10.1 * (_Cycle + .3) , 3 * _Cycle+.2);

                //float3 col = fNorm * .5 + .5;//i.debug;

                float3 fRefl = reflect( -normalize(i.eye) , fNorm );
                float3 cubeCol = texCUBE(_CubeMap,fRefl ).rgb;

                col =  col * cubeCol * 2;

                col = float3( 1 , .7 , .5 ) * cubeCol  * 2;

                col = lerp( col , col * col , i.secondVal );

                
                
                float3 noMoonCol = col;
                float3 fullMoonCol = (col * float3( .1 , 2 , 2));
                col = lerp( fullMoonCol , noMoonCol , _Cycle );

                col = lerp( float3( col.x , col.x , col.x ) , col , i.started );

                col *= 1 + 2 * i.debug.x;

                if( _Special == 1. ){
                    col = cubeCol *2 *(fNorm * .5 + .5);
                }

                float aboveVal = clamp( -i.eye.y * 4 + .5 , 0 , 1 );
                float3 belowCol = float3( length( col ), length( col) , length( col )) * .4;
                col = lerp(  belowCol , col , aboveVal * .8 + .2 );
                col = lerp( col , col * (fNorm * .5 + .5) , clamp( aboveVal - .3 , 0 , 1 ) * clamp((i.started - _ClothDown),0,1) );

                col /= .1 + length( i.worldPos ) * length( i.worldPos ) * .5;

                //col = aCol;

             
                
                //col = i.debug;
			        //col = i.nor * .5 + .5;

            		//float3 col = float3( i.uv.x , i.uv.y , 1);

                return float4( col , 1 );

            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
	
}