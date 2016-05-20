Shader "Custom/LuneTitle" {
	// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

 Properties {
  


    _Scale( "Scale" , Vector ) = ( 1.5 , .2 , 2 , 0 )
    _TitleTexture( "TitleTexture" , 2D ) = "white" {}





  }
  
  SubShader {
    Tags { "RenderType"="Transparent" "Queue" = "Transparent" }

    //Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
    LOD 200

    Pass {
      Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

      //CULL OFF
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      // Use shader model 3.0 target, to get nicer looking lighting
      #pragma target 3.0

      #include "UnityCG.cginc"
      #include "Chunks/noise.cginc"
      
 
      
      uniform sampler2D _TitleTexture;
  
      uniform float _Cycle;
      uniform float _ClothDown;
      uniform float _EndingVal;
      uniform float _FullEnd;




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


        float3 ro = i.ro;
        float3 rd = normalize(ro - i.camPos);

       // ro -= i.centerP;
       // rd += i.centerP;

        float3 col = float3( 0.0 , 0.0 , 0.0 );

    		col= float3( 0. , 0. , 0. );

    		float2 w = i.uv.xy;
    		float hit = 0;

        for( int j = 0; j < 31; j++){

          float3 pos = ro + rd * float(j) * .003;

          //if( pos.z > -s ){ break; }

          float2 w = (pos.xy+float2( .5 , .5 ));// * float2( _Scale.x , _Scale.y ) * .6 +.5 * float2( _Scale.x * .5 , _Scale.y);

          float val = tex2D(_TitleTexture , w ).w;

          float noiseVal = 2 * noise( pos *  20.0 * float3( 1.1 , .21 , 1 ) + float3( 0 , 0 , _Time.y));
          noiseVal += 1 * noise( pos *  50.0 * float3( 1.1 , .21 , 1 ) + float3( 0 , 0 , 1.4 * _Time.y));
          noiseVal += .5 * noise( pos *  100.0 * float3( 1.1 , .21 , 1 ) + float3( 0 , 0 , 1.4 * _Time.y));

          float total = val;// + val * noiseVal;// val +  noiseVal * val;

          if( total > .8 ){
          	hit = 1;
            col += lerp(  hsv( noiseVal ,1,0) , hsv( noiseVal ,.5,1), float(j)/30);
            break;
          }
        }
        //col /= 5.0;
        //col = normalize( col );
        if( hit < .5 ){
        	discard;
        }

        col = lerp( col , float3( 0 , 0 , 0 ) , _ClothDown - _EndingVal + _FullEnd  );
        fixed4 color;
        color = fixed4( col , 1 );
        return color;
      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}