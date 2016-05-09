// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/Title" {
 Properties {
  


    _Scale( "Scale" , Vector ) = ( 1.5 , .2 , 2 , 0 )
    _TitleTexture( "TitleTexture" , 2D ) = "white" {}





  }
  
  SubShader {
    Tags { "RenderType"="Transparent" "Queue" = "Transparent" }

    //Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
    LOD 200

    Pass {
      //Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

      CULL OFF
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      // Use shader model 3.0 target, to get nicer looking lighting
      #pragma target 3.0

      #include "UnityCG.cginc"
      #include "Chunks/noise.cginc"
      
 
      
      uniform sampler2D _TitleTexture;
  
      uniform float _StartTime;

      uniform int _Hit;
      uniform float _HitTime;

      uniform int _Logo;
      uniform float _LogoTime;

      uniform int _Fade;
      uniform float _FadeTime;




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
          float3 centerP : TEXCOORD3;

          //float3 rd     : TEXCOORD3;
          float3 camPos : TEXCOORD4;
      };
        

float3 hsv(float h, float s, float v)
{
  return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
    h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
}

      VertexOut vert(VertexIn v) {
        
        VertexOut o;

        o.normal = v.normal;
        
        o.uv = v.texcoord;
  
        // Getting the position for actual position
        o.pos = mul( UNITY_MATRIX_MVP , v.position );
     
        float3 mPos = mul( unity_ObjectToWorld , v.position );
        o.centerP = mul( unity_ObjectToWorld , float4( 0. , 0. , 0. , 1. ) ).xyz;

        o.ro = v.position;
        o.camPos = mul( unity_WorldToObject , float4( _WorldSpaceCameraPos  , 1. )); 

        return o;

      }


     // Fragment Shader
      fixed4 frag(VertexOut i) : COLOR {

      	if( _Logo < 0 ){ discard; }

        float3 ro = i.ro;
        float3 rd = normalize(ro - i.camPos);

       // ro -= i.centerP;
       // rd += i.centerP;

        float3 col = float3( 0.0 , 0.0 , 0.0 );

    		col= float3( 0. , 0. , 0. );

    		float2 w = i.uv.xy;
    		float hit = 0;

        for( int i = 30 - clamp( int( 50.6 * (_Time.y - _LogoTime) *  (_Time.y - _LogoTime) ) , 0 , 30 ); i < 31; i++){

          float3 pos = ro + rd * float(i) * .003;

          //if( pos.z > -s ){ break; }

          float2 w = (pos.xy* float2( 1.1 , .21 )+float2( .5 , .5 ));// * float2( _Scale.x , _Scale.y ) * .6 +.5 * float2( _Scale.x * .5 , _Scale.y);

          float val = 2-length(tex2D(_TitleTexture , w).xyz);

          float noiseVal = 2 * noise( pos *  20.0 * float3( 1.1 , .21 , 1 ) + float3( 0 , 0 , _Time.y));
          noiseVal += 1 * noise( pos *  50.0 * float3( 1.1 , .21 , 1 ) + float3( 0 , 0 , 1.4 * _Time.y));
          noiseVal += .5 * noise( pos *  100.0 * float3( 1.1 , .21 , 1 ) + float3( 0 , 0 , 1.4 * _Time.y));

          float total = val;// + val * noiseVal;// val +  noiseVal * val;

          if( total > .8 ){
          	hit = 1;
            col += lerp( float3( 1,1,1) , hsv( noiseVal ,1,1), float(i)/30);

            break;
          }
        }
        //col /= 5.0;
        //col = normalize( col );
        if( hit < .5 ){
        	discard;
        }



        

     
    		//col = float3( 1. , 1. , 1. );
       // float m = col.x * col.y * col.z + .3;// length( col );
       // col = lerp( col , float3( m  , m , m) , _Learning );

            float fadeTime = clamp( (_Time.y - _FadeTime)* .5 , 0 , 1 );

            col = lerp( col , float3( 0 , 0 , 0 ) , fadeTime   );
            fixed4 color;
            color = fixed4( col , 1 );
            return color;
      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}