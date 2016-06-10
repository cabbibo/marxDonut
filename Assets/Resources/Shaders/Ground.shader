// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/Gound" {
 Properties {
  

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
      
 
      


      uniform int _NumberSteps;
      uniform int _Fade;
      uniform float  _IntersectionPrecision;
      uniform float _MaxTraceDistance;
      uniform float _ClothDown;
      uniform float _Disappear;

      uniform float _Cycle;

      uniform int _NumberHands;
      uniform float3 _Hand1;
      uniform float3 _Hand2;

      uniform float _Trigger1;
      uniform float _Trigger2;

      uniform sampler2D _Audio;
      
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


      float2 map( in float3 pos ){
        
        float2 res;
        float2 lineF;
        float2 sphere;

        float c = _Cycle * 1.2 - .2;

			//res = float2( sdSphere( pos , .4 ) , 0.6 );
        float3 modVal = float3( .3 + _ClothDown * .2 +  c , .3 + _ClothDown * .2 +  c , .3+ _ClothDown * .2 +  c );
        //pos -= float3( 0 , .2 , 0.);
//
        //pos += .1 * float3( sin( pos.x * 10. ) , sin( pos.y * 10. ) , sin( pos.z * 10. ));
        //pos += .1 * float3( sin( pos.x * 10. ) , sin( pos.y * 10. ) , sin( pos.z * 10. ));
        //pos += .1 * float3( sin( pos.x * 10. ) , sin( pos.y * 10. ) , sin( pos.z * 10. ));
        int3 test;
        float bSize = .2 - _ClothDown * .2 + c * 1.9;
        float3 boxSize = float3( bSize , bSize , bSize );
        float2 res2 = float2( sdBox( modit(pos , modVal) - modVal / 2. , boxSize ) , 0.4 );
       
        float nVal = 0;
        nVal += .3 * (noise( pos - float3( 0 , _Time.y / 6, 0 ) )+1);
        nVal += (.1 +.1* _ClothDown) * (noise( pos * 10 - float3( 0 , _Time.y / 10 , 0 ) )+1);
        nVal += .2 * length( pos.xz );

        res2.x += nVal * c * 1.5;
    
          res2.x = max( res2.x , -sdSphere( pos - _Hand1 , .2 + .3 * _Trigger1 ) + nVal * .3);
          res2.x = max( res2.x , -sdSphere( pos - _Hand2 , .2 + .3 * _Trigger2 ) + nVal * .3);
    
       	//res = smoothU( res , res2 , 0.1 );
    		//res = float2( length( pos - float3( 0., -.8 ,0) ) - 1., 0.1 );
    		//res = smoothU( res , float2( length( pos - float3( .3 , .2 , -.2) ) - .1, 0.1 ) , .05 );
    		//res = smoothU( res , float2( length( pos - float3( -.4 , .2 , .4) ) - .1, 0.1 ) , .05 );
    		//res = smoothU( res , float2( length( pos - float3( 0.3 , .2 , -.3) ) - .1, 0.1 ) , .05 );

  	    return res2; 
  	 
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
            
       
        float ip = _IntersectionPrecision + .8 * _ClothDown;
        float h = ip * 2;
        float t = 0.0;
        float res = -1.0;
        float id = -1.0;
        
        for( int i=0; i< int(float(_NumberSteps) * (1-_ClothDown) + 1) ; i++ ){
            
            if( h < ip|| t > _MaxTraceDistance ) break;
    
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

        float3 aCol = float3( 0,0,0);

        if( res.y > -0.5 ){
        
        	float3 pos = ro + rd * res.x;
        	float3 norm = calcNormal( pos );
        
          float3 fRefl = reflect( -rd , norm );
          float3 cubeCol = texCUBE(_CubeMap,fRefl ).rgb;
        	col = float3( 1. , .8 , .4 );//norm * .5 + .5;
          col *= cubeCol * 2;
          col /= (pow( res.x ,.5 ) * 3.);
        
          //if( col > float3( 1 , 1, 1) ){ col /= 2; }
          col = min( float3( 2 , 1.5,1), col);

          //aCol = tex2D( _Audio , float2( abs(dot(norm,normalize(float3(1,1,1)))) * .5,0 ) ).xyz;
        	//col = float3( 1. , 0. , 0. );
        	
        }

      //col= float3( 2. , 1.5 , 1. ) * .5;


      float noiseCutoff = noise( float3( i.uv.x , i.uv.y , 0. ) * ( 8  + 8 *_Cycle ));
      noiseCutoff += noise( float3( i.uv.x , i.uv.y , 0. ) * (17 +30 *_Cycle)  );
      noiseCutoff += noise( float3( i.uv.x , i.uv.y , 0. ) * (23 + 100 * _Cycle ));
      noiseCutoff /= 3;

      if( noiseCutoff < _Disappear ){discard;}

        if( _Fade == 1 ){
          float l = length( i.uv - float2( .5 ,.5 ) )*2;

          col = lerp( col , float3( 0 ,0,0) , l ); 
        }


         col = min( float3( 1 , 1 , 1 ) , col );

        float3 noMoonCol = col;
        float3 fullMoonCol = (col * float3( .3 , 2 , 4 ));

        col = lerp( fullMoonCol , noMoonCol , _Cycle );
         //gamma correction
        col = pow(col,  2.2);  
        float v = pow(length( col  ) , .5);//length( col );
        float3 col2 = float3( v , v, v );//float3( length( col ) , length( col ) , length( col ) ) * .5;

        col = lerp( col2 , col , _ClothDown );

        //col += aCol;

        col = clamp( col , float3( 0,0,0) , float3( 2, 2,2));

        col = col * .5f;


        fixed4 color;
        color = fixed4( col , 1. );
        return color;

      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}