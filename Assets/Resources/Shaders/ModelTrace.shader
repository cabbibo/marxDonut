﻿
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/ModelTrace" {
 Properties {
  _MainTex ("Albedo (RGB)", 2D) = "white" {}

    _NumberSteps( "Number Steps", Int ) = 20
    _MaxTraceDistance( "Max Trace Distance" , Float ) = 10.0
    _IntersectionPrecision( "Intersection Precision" , Float ) = 0.0001
    _CubeMap( "Cube Map" , Cube )  = "defaulttexture" {}
    _Fade( "Fade", Int ) = 1



  }
  
  SubShader {
    //Tags { "RenderType"="Transparent" "Queue" = "Transparent" }

    Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
    LOD 200

    Cull Off

    Pass {
      //Blend SrcAlpha OneMinusSrcAlpha // Alpha blending


      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag
      // Use shader model 3.0 target, to get nicer looking lighting
      #pragma target 3.0

      #include "UnityCG.cginc"
      #include "Chunks/noise.cginc"
      
 
      
			uniform sampler2D _MainTex;

      uniform int _NumberSteps;
      uniform int _Fade;
      uniform float  _IntersectionPrecision;
      uniform float _MaxTraceDistance;
      uniform float _BeginVal;
      uniform float _SecondVal;
      uniform float _Cycle;
      uniform float _Hovered;

      uniform float _TriggerDown;
      

      uniform float3 _Hand1;
      uniform float3 _Hand2;
      
      uniform samplerCUBE _CubeMap;


      struct VertexIn
      {
         float4 position  : POSITION; 
         float3 normal    : NORMAL; 
         float4 texcoord  : TEXCOORD0; 
         float4 tangent   : TANGENT;
      };

      struct VertexOut {
          float4 pos    : POSITION; 
          float3 normal : NORMAL; 
          float4 uv     : TEXCOORD0; 
          float3 ro     : TEXCOORD2;

          //float3 rd     : TEXCOORD3;
          float3 camPos : TEXCOORD4;
      };
        

      float sdBox( float3 p, float3 b ){

        float3 d = abs(p) - b;

        return min(max(d.x,max(d.y,d.z)),0.0) +
               length(max(d,0.0));

      }

      float2 opU( float2 d1 , float2 d2 ){
      	if( d1.x < d2.x ){
      		return d1;
      	}else{
      		return d2;
      	}; //min( d1 , d2 );
      }

      float sdSphere( float3 p, float s ){
        return length(p)-s;
      }

      float sdCapsule( float3 p, float3 a, float3 b, float r )
      {
          float3 pa = p - a, ba = b - a;
          float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
          return length( pa - ba*h ) - r;
      }

      float2 smoothU( float2 d1, float2 d2, float k)
      {
          float a = d1.x;
          float b = d2.x;
          float h = clamp(0.5+0.5*(b-a)/k, 0.0, 1.0);
          return float2( lerp(b, a, h) - k*h*(1.0-h), lerp(d2.y, d1.y, pow(h, 2.0)));
      }

      
      float3 modit(float3 x, float3 m) {
			    float3 r = x%m;
			    return r<0 ? r+m : r;
			}
float opS( float d1, float d2 ) 
{ 
    return max(-d1,d2); 
} 


      float2 map( in float3 pos ){
        
        float2 res;
        float2 lineF;
        float2 sphere;

        float3 modVal = float3( .03 , .03 , .03 );//, .3+ _ClothDown * .2 +  c );


        float n = .8 * noise( pos * 100 + float3( 0 , 0 , _Time.y) );
        n += .4 * noise( pos * 200 + float3( 0 , 0 , 1.5 * _Time.y) );
        n += noise( pos * 50 + float3( 0 , 0 , 3.5 * _Time.y) );
        res = float2( sdSphere( modit( pos , modVal ) - modVal * .5 , .006 ) - n * .01  , 1 );
        //if( pos.z < .02 ){ res.x -= noise( pos * 10 ) * -10 * (pos.z - .02) * pos.y; }
        //res.x -= pos.y;
        //res.y = 1;

	
        float2 thing2 = float2( sdCapsule( pos , float3( 0 ,0 ,0 ) , float3( 0 , -.01 , .15 ) , .05 ) , 2);
        thing2.x += n * .03;
        thing2.x -= _TriggerDown * .02;

        res = opU( res , thing2);
        //res.x += noise( pos * 100 ) * .03;

        res = opU( float2( sdSphere( pos , .03 ) , 3 ) , res );

        res.x += _TriggerDown * .03;
        

        //res.x = smoothU( res , float2( sdSphere( pos , .03 ) , 100 ) , .0 );
        //res.x = lerp( res.x , sdBox( pos , float3( .2 , .2 , .2 ) ) , 1 - _BeginVal);
//
//
        //res.x -= ((.1 * _BeginVal) + _Hovered* .04)* noise( pos * 10 );
        //res.y = 1;
//
        //res.x += _SecondVal;

       
       //res.x = n - 1.5;
       //res.x /= 100;
       //res.y = 1;
        //res.x += .1 + noise( pos * 3 );
        //res.x /= 10;

  	    return res; 
  	 
  	  }

      float3 calcNormal( in float3 pos ){

      	float3 eps = float3( 0.001, 0.0, 0.0 );
      	float3 nor = float3(
    	    map(pos+eps.xyy).x - map(pos-eps.xyy).x,
    	    map(pos+eps.yxy).x - map(pos-eps.yxy).x,
    	    map(pos+eps.yyx).x - map(pos-eps.yyx).x );
      	return normalize(nor);

      }
              
         

      float2 calcIntersection( in float3 ro , in float3 rd ){     
            
               
        float h =  _IntersectionPrecision * 2;
        float t = 0.0;
        float res = -1.0;
        float id = -1.0;
        
        for( int i=0; i< _NumberSteps; i++ ){
            
          if( h < _IntersectionPrecision || t > _MaxTraceDistance ) break;
  
          float3 pos = ro + rd*t;
          float2 m = map( pos );
          
          h = m.x;
          t += h;
          id = m.y;
            
        }
    
    
        if( t <  _MaxTraceDistance ){ res = t; }
        if( t >  _MaxTraceDistance ){ id = -1.0; }
        
        return float2( res , id );
          
      
      }
            
    

      VertexOut vert(VertexIn v) {
        
        VertexOut o;

        o.normal = v.normal;
        
        o.uv = v.texcoord;
  
        // Getting the position for actual position
        o.pos = mul( UNITY_MATRIX_MVP , v.position );
     
        float3 mPos = mul( unity_ObjectToWorld , v.position );

        o.ro = mPos;// v.position;
        o.camPos = _WorldSpaceCameraPos;//mul( _World2Object , float4( _WorldSpaceCameraPos  , 1. )); 

        o.ro = v.position;
        o.camPos = mul( unity_WorldToObject , float4( _WorldSpaceCameraPos  , 1. )); 

        return o;

      }


     // Fragment Shader
      fixed4 frag(VertexOut i) : COLOR {

        float3 ro = i.ro;
        float3 rd = normalize(ro - i.camPos);

        float3 col = float3( 0.0 , 0.0 , 0.0 );
    		float2 res = calcIntersection( ro , rd );
    		
    		col= float3( 0. , 0. , 0. );

    		if( res.y > -0.5 ){

    			float3 pos = ro + rd * res.x;
    			float3 norm = calcNormal( pos );


          float3 fRefl = reflect( -rd , norm );
          float3 cubeCol = texCUBE(_CubeMap,fRefl ).rgb;
    			float3 col1 = fRefl * .5 + .5;
          float3 col2 = cubeCol * 2 * float3( 1 , .8 , .4 );
         //col = lerp( col1 , col2 , res.y - .4 );

         if( res.y == 1 ){ col = length( cubeCol )* float3(1,1,1) * .3;}
         if( res.y == 2 ){ col = length( cubeCol )* float3(1,1,1) * .8;}
         if( res.y == 3 ){ col = cubeCol;}
    			//col = float3( 1. , 0. , 0. );
    			
    		}else{

    			//float3 fRefl = reflect( -rd , i.normal );
          //float3 cubeCol = texCUBE(_CubeMap,fRefl ).rgb;
    			//col = cubeCol;
          discard;
    		}


    		//col = float3( 1. , 1. , 1. );

        //gamma correction
        col = pow(col,  2.2);  
        /*   float3 noMoonCol = col;
                                float3 fullMoonCol = (col * float3( .3 , 2 , 4 ));

                                col = lerp( fullMoonCol , noMoonCol , _Cycle );
        float3 col2 = float3( length( col ) , length( col ) , length( col ) ) * .5;

        col = lerp( col2 , col , _BeginVal );
// Or (cheaper, but assuming gamma of 2.0 rather than 2.2)  
   ///return float4( sqrt( finalCol ), pixelAlpha );  

   col *= _Hovered * 4 + 1;*/



        //col = lerp( col , float3( 1 , 0 , 0 ) , _SecondVal );
        fixed4 color;
        color = fixed4( col , 1. );
        return color;

      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}
