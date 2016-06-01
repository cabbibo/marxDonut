Shader "Custom/LineTraced" {
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
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
      
 
      


      uniform float _Cycle;     
      uniform float _ClothDown;
      uniform float _EndingVal;
      uniform float _FullEnd;

      uniform int _NumberSteps;
      uniform int _Fade;
      uniform float  _IntersectionPrecision;
      uniform float _MaxTraceDistance;

      uniform float3 _Hand1;
      uniform float3 _Hand2;

      uniform sampler2D _AudioMap;
      
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
        float3 mSize= float3( .1 , .1 , .1 );
        float3 mPos = modit( pos , mSize);
        mPos -= .5 * mSize;
        //res.x = sdSphere( mPos , .03 );
        res.x = .04 * (noise( pos * 40 + float3( 0 , -_Time.y, 0 ) ) + noise( pos * 10 ) + noise( pos * 3));

       /* res.x += .0 - (.3 * _Cycle) * noise( pos * ( 4 + _Cycle * 10 ) + float3(
        											_Time.y * ( .414 + .4  * _Cycle ) , 
        											_Time.y * (.264 + .2 * _Cycle ),
        										  _Time.y * ( .1124 + .3 * _Cycle) ));*/

        //res.x = sdSphere( pos , .45 );
        res.y = 1;
       
       //res.x = n - 1.5;
       //res.x /= 100;
       //res.y = 1;
        //res.x += .1 + noise( pos * 3 );
        //res.x /= 10;

  	    return res; 
  	 
  	  }

      float3 calcNormal( in float3 pos ){

      	float3 eps = float3( 0.01, 0.0, 0.0 );
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
        //o.normal = mul( unity_ObjectToWorld , float4( v.normal , 0 )).xyz;

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

        float matchV = dot( float3( 1 , 0 , 0 ) , i.normal );

    		if( res.y > -0.5 ){

    			float3 pos = ro + rd * res.x;
    			float3 norm = calcNormal( pos );

          float3 fRefl = reflect( -rd , norm );
          float3 cubeCol = texCUBE(_CubeMap,fRefl ).rgb;
    			col = cubeCol;//norm * .5 + .5;

          float m = dot( float3( 0 , 1 , 0 ) , norm );

    			col =  (float3( m , m , m )+1) * length( cubeCol );//col;//lerp( col , float3( length( col) , length( col ) , length( col)) , abs( sin(_Cycle * 2 * 3.14159)) );
         // col *= cubeCol * 2 * float3( .4 , .6 , .8 );
         col = col * col * col;
    			//col = float3( 1. , 0. , 0. );
    			
    		}else{

    			//float3 fRefl = reflect( -rd , i.normal );
          //float3 cubeCol = texCUBE(_CubeMap,fRefl ).rgb;
    			//col = cubeCol;
         // discard;
    		}




            fixed4 color;
            color = fixed4( col , 1. );
            return color;
      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}