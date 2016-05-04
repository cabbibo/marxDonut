﻿using UnityEngine;
using System.Collections;


using System.IO;
using System.Text;

public class Cloth : MonoBehaviour {


    // How the donut looks
    public Shader shader;
    public Shader debugShader;

    // How the donut feels
    public ComputeShader constraintPass;
    public ComputeShader normalPass;
    public ComputeShader forcePass;

    public Transform finalTransform;
    public GameObject[] Shapes;

    public Texture2D normalMap;
    public Cubemap cubeMap;

    public float clothSize = 1;
    public float startingHeight = 1;

    private float[] inValues;
    private float[] shapeValues;


    private ComputeBuffer _vertBuffer;

    private ComputeBuffer _upLinkBuffer;
    private ComputeBuffer _rightLinkBuffer;
    private ComputeBuffer _diagonalDownLinkBuffer;
    private ComputeBuffer _diagonalUpLinkBuffer;

    private ComputeBuffer _shapeBuffer;


    private const int threadX = 8;
    private const int threadY = 8;
    private const int threadZ = 8;

    private const int strideX = 8;
    private const int strideY = 8;
    private const int strideZ = 8;

    private int gridX { get { return threadX * strideX; } }
    private int gridY { get { return threadY * strideY; } }
    private int gridZ { get { return threadZ * strideZ; } }

    private int vertexCount { get { return gridX * gridY * gridZ; } }




    private int ribbonWidth = 512;
    private int ribbonLength { get { return (int)Mathf.Floor( (float)vertexCount / ribbonWidth ); } }
    

    private int _kernelforce;
    private int _kernelconstraint;
    private int _kernelnormal;


    private Material material;
    private Material debugMaterial;

    private Vector3 p1;
    private Vector3 p2;

    private float oTime = 0;
    private bool objMade = false;

  struct Vert{
		public Vector3 pos;
		public Vector3 oPos;
		public Vector3 ogPos;
		public Vector3 norm;
		public Vector2 uv;
		public float mass;
		public float[] ids;
		public Vector3 debug;
	};

	struct Link{
		public float id1;
		public float id2;
		public float distance;
		public float stiffness;
	}

	struct Shape{
		public Matrix4x4 mat;
		public float shape;
	}

	private int VertStructSize =  3 + 3 + 3 + 3 + 2 + 1 + 8 + 3;
	private int LinkStructSize =  1 + 1 + 1 + 1;
	private int ShapeStructSize = 16 + 1;

    // Use this for initialization
    void Start () {

    	oTime = Time.time;
        shapeValues = new float[ Shapes.Length * ShapeStructSize ];

        createBuffers();
        createMaterial();

        _kernelforce = forcePass.FindKernel("CSMain");
        _kernelnormal = normalPass.FindKernel("CSMain");
        _kernelconstraint = constraintPass.FindKernel("CSMain");


        Camera.main.gameObject.AddComponent<PostRenderEvent>();
        

        //TODO:
        //Figure out how to add this script to the main camera!
        Camera.onPostRender += Render;


    
    }

    void Update(){
    	//print(Input.GetKey ("a"));
        if( Input.GetKey(KeyCode.Space)){
            createOBJ();
        }
        Dispatch();
    }



    //When this GameObject is disabled we must release the buffers or else Unity complains.
    private void OnDisable(){
        Camera.onPostRender -= Render;
        ReleaseBuffer();
    }

      //For some reason I made this method to create a material from the attached shader.
    private void createMaterial(){

      material = new Material( shader );
      debugMaterial = new Material( debugShader );

    }

 
 
    //Remember to release buffers and destroy the material when play has been stopped.
    void ReleaseBuffer(){

      _vertBuffer.Release(); 

      _upLinkBuffer.Release(); 
      _rightLinkBuffer.Release(); 
      _diagonalUpLinkBuffer.Release(); 
      _diagonalDownLinkBuffer.Release(); 

    
      DestroyImmediate( material );
      DestroyImmediate( debugMaterial );

    }

        //After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
    //this just draws the "mesh" as a set of points
    public void Render(Camera camera) {


        
        int numVertsTotal = (ribbonWidth-1) * 3 * 2 * (ribbonLength-1);

        material.SetPass(0);

        material.SetBuffer("buf_Points", _vertBuffer);
     
        material.SetInt( "_RibbonWidth"  , ribbonWidth  );
        material.SetInt( "_RibbonLength" , ribbonLength );
        material.SetInt( "_TotalVerts"   , vertexCount  );
        material.SetTexture( "_NormalMap" , normalMap);
        material.SetTexture( "_CubeMap"  , cubeMap );

        material.SetMatrix("worldMat", finalTransform.localToWorldMatrix);
        material.SetMatrix("invWorldMat", finalTransform.worldToLocalMatrix);

        Graphics.DrawProcedural(MeshTopology.Triangles, numVertsTotal);

        debugMaterial.SetPass(0);

        debugMaterial.SetBuffer("buf_Points", _vertBuffer);

        debugMaterial.SetInt( "_RibbonWidth"  , ribbonWidth  );
        debugMaterial.SetInt( "_RibbonLength" , ribbonLength );
        debugMaterial.SetInt( "_TotalVerts"   , vertexCount  );

        debugMaterial.SetMatrix("worldMat", transform.localToWorldMatrix);
        debugMaterial.SetMatrix("invWorldMat", transform.worldToLocalMatrix);

        //Graphics.DrawProcedural(MeshTopology.Lines, 16 * ribbonLength * ribbonWidth );


    }

    private Vector3 getVertPosition( float uvX , float uvY  ){

        float u = (uvY -.5f);
        float v = (uvX -.5f);

        return new Vector3( u * clothSize, startingHeight , v * clothSize );

    }

    private void createBuffers() {

      _shapeBuffer = new ComputeBuffer( Shapes.Length , ShapeStructSize * sizeof(float) );

      _vertBuffer  = new ComputeBuffer( vertexCount ,  VertStructSize * sizeof(float));
      
      _upLinkBuffer 			      = new ComputeBuffer( vertexCount / 2 , LinkStructSize * sizeof(float));
      _rightLinkBuffer 			    = new ComputeBuffer( vertexCount / 2 , LinkStructSize * sizeof(float));
      _diagonalDownLinkBuffer 	= new ComputeBuffer( vertexCount / 2 , LinkStructSize * sizeof(float));
      _diagonalUpLinkBuffer 	  = new ComputeBuffer( vertexCount / 2 , LinkStructSize * sizeof(float));

      float lRight = clothSize / (float)ribbonWidth;
      float lUp = clothSize / (float)ribbonLength;


      Vector2 n = new Vector2( lRight , lUp );

      float lDia = n.magnitude;
      
      inValues = new float[ VertStructSize * vertexCount];

      float[] upLinkValues = new float[ LinkStructSize * vertexCount / 2 ];
      float[] rightLinkValues = new float[ LinkStructSize * vertexCount / 2 ];
      float[] diagonalDownLinkValues = new float[ LinkStructSize * vertexCount / 2 ];
      float[] diagonalUpLinkValues = new float[ LinkStructSize * vertexCount / 2 ];

      // Used for assigning to our buffer;
      int index = 0;
      int indexOG = 0;
      int li1= 0;
      int li2= 0;
      int li3= 0;
      int li4= 0;


      /*        // second rite up here
			 u	 dU   x  . r
			 . .					 // third rite down here
			 x  . r        x  . r
				 . 
				   dD

      */

      for (int z = 0; z < gridZ; z++) {
        for (int y = 0; y < gridY; y++) {
          for (int x = 0; x < gridX; x++) {

            int id = x + y * gridX + z * gridX * gridY; 
            
            float col = (float)(id % ribbonWidth );
            float row = Mathf.Floor( ((float)id +0.01f) / ribbonWidth);

            if( row % 2 == 0 ){

            	upLinkValues[li1++] = id;
            	upLinkValues[li1++] = convertToID( col + 0 , row + 1 );
            	upLinkValues[li1++] = lUp;
            	upLinkValues[li1++] = 1;


            	// Because of the way the right links
            	// are made, we need to alternate them,
            	// and flip flop them back and forth
            	// so they are not writing to the same
            	// positions during the same path!
            	float id1 , id2;

            	if( col % 2 == 0 ){
            		id1 = id;
            		id2 = convertToID( col + 1 , row + 0 );
            	}else{
            		id1 = convertToID( col + 0 , row + 1 );
            		id2 = convertToID( col + 1 , row + 1 );
            	}

            	rightLinkValues[li2++] = id1;
            	rightLinkValues[li2++] = id2;
            	rightLinkValues[li2++] = lRight;
            	rightLinkValues[li2++] = 1;


            	diagonalDownLinkValues[li3++] = id;
            	diagonalDownLinkValues[li3++] = convertToID( col - 1 , row - 1 );
            	diagonalDownLinkValues[li3++] = lDia;
            	diagonalDownLinkValues[li3++] = 1;

            	diagonalUpLinkValues[li4++] = id;
            	diagonalUpLinkValues[li4++] = convertToID( col + 1 , row + 1 );
            	diagonalUpLinkValues[li4++] = lDia;
            	diagonalUpLinkValues[li4++] = 1;

            }


            float uvX = col / ribbonWidth;
            float uvY = row / ribbonLength;

            Vector3 fVec = getVertPosition( uvX , uvY );


            Vert vert = new Vert();


            vert.pos = fVec * 1.000001f;

            vert.oPos = fVec- new Vector3( 0 , -.11f , 0 );
            vert.ogPos = fVec ;
            vert.norm = new Vector3( 0 , 1 , 0 );
            vert.uv = new Vector2( uvX , uvY );

            vert.mass = 0.3f;
            if( col == 0 || col == ribbonWidth || row == 0 || row == ribbonLength ){
            	vert.mass = 2.0f;
            }
            vert.ids = new float[8];
            vert.ids[0] = convertToID( col + 1 , row + 0 );
            vert.ids[1] = convertToID( col + 1 , row - 1 );
            vert.ids[2] = convertToID( col + 0 , row - 1 );
            vert.ids[3] = convertToID( col - 1 , row - 1 );
            vert.ids[4] = convertToID( col - 1 , row - 0 );
            vert.ids[5] = convertToID( col - 1 , row + 1 );
            vert.ids[6] = convertToID( col - 0 , row + 1 );
            vert.ids[7] = convertToID( col + 1 , row + 1 );

            vert.debug = new Vector3(0,1,0);

            inValues[index++] = vert.pos.x;
            inValues[index++] = vert.pos.y;
            inValues[index++] = vert.pos.z;

            inValues[index++] = vert.oPos.x;
            inValues[index++] = vert.oPos.y;
            inValues[index++] = vert.oPos.z;

            inValues[index++] = vert.ogPos.x;
            inValues[index++] = vert.ogPos.y;
            inValues[index++] = vert.ogPos.z;

            inValues[index++] = vert.norm.x;
            inValues[index++] = vert.norm.y;
            inValues[index++] = vert.norm.z;

            inValues[index++] = vert.uv.x;
            inValues[index++] = vert.uv.y;

            inValues[index++] = 0;

            inValues[index++] = vert.ids[0];
            inValues[index++] = vert.ids[1];
            inValues[index++] = vert.ids[2];
            inValues[index++] = vert.ids[3];
            inValues[index++] = vert.ids[4];
            inValues[index++] = vert.ids[5];
            inValues[index++] = vert.ids[6];
            inValues[index++] = vert.ids[7];

            inValues[index++] = vert.debug.x;
            inValues[index++] = vert.debug.y;
            inValues[index++] = vert.debug.z;

          }
        }
      }

      _vertBuffer.SetData(inValues);

      _upLinkBuffer.SetData(upLinkValues);
      _rightLinkBuffer.SetData(rightLinkValues);
      _diagonalUpLinkBuffer.SetData(diagonalUpLinkValues);
      _diagonalDownLinkBuffer.SetData(diagonalDownLinkValues);
    

    }

    private float convertToID( float col , float row ){

        float id;

        if( col >= ribbonWidth ){ return -10; }
        if( col < 0 ){ return -10; }

        if( row >= ribbonLength ){ return -10; }
        if( row < 0 ){ return -10; }

        id = row * ribbonWidth + col;

        return id;

    }

    private void doConstraint( float v , int offset , ComputeBuffer b ){

    	// Which link in compute are we doing
    	constraintPass.SetInt("_Offset" , offset );
    	constraintPass.SetFloat("_Multiplier" , v );
    	constraintPass.SetBuffer( _kernelconstraint , "linkBuffer"   , b     );

    	//TODO: only need to dispatch for 1/9th of the buffer size!
    	constraintPass.Dispatch( _kernelconstraint , strideX / 2 , strideY  , strideZ );

    } 
    
    private void assignShapeBuffer(){

    	int index = 0;

    	for( int i = 0; i < Shapes.Length; i++ ){
    		GameObject go = Shapes[i];
		    for( int j = 0; j < 16; j++ ){
		      int x = j % 4;
		      int y = (int) Mathf.Floor(j / 4);
		      shapeValues[index++] = go.transform.worldToLocalMatrix[x,y];
		    }

		    // TODO:
		    // Make different for different shapes
		    shapeValues[index++] = 1;

    	}

    	_shapeBuffer.SetData(shapeValues);

    }

    private void Dispatch() {

    	assignShapeBuffer();

		  forcePass.SetFloat( "_DeltaTime"    , Time.time - oTime );
		  forcePass.SetFloat( "_Time"         , Time.time      );

		  oTime = Time.time;

		  forcePass.SetInt( "_RibbonWidth"   , ribbonWidth     );
		  forcePass.SetInt( "_RibbonLength"  , ribbonLength    );
		  forcePass.SetInt( "_NumShapes"     , Shapes.Length   );

		  forcePass.SetBuffer( _kernelforce , "vertBuffer"   , _vertBuffer );
		  forcePass.SetBuffer( _kernelforce , "shapeBuffer"   , _shapeBuffer );
		  forcePass.Dispatch( _kernelforce , strideX , strideY  , strideZ );

			constraintPass.SetInt( "_RibbonWidth"   , ribbonWidth     );
      constraintPass.SetInt( "_RibbonLength"  , ribbonLength    );

			constraintPass.SetBuffer( _kernelconstraint , "vertBuffer"   , _vertBuffer     );

			doConstraint( 1 , 1 , _upLinkBuffer );
			doConstraint( 1 , 1 , _rightLinkBuffer );
			doConstraint( 1 , 1 , _diagonalDownLinkBuffer );
			doConstraint( 1 , 1 , _diagonalUpLinkBuffer );

			doConstraint( 1 , 0 , _upLinkBuffer );
			doConstraint( 1 , 0 , _rightLinkBuffer );
			doConstraint( 1 , 0 , _diagonalDownLinkBuffer );
			doConstraint( 1 , 0 , _diagonalUpLinkBuffer );


			//calculate our normals
			normalPass.SetBuffer( _kernelnormal , "vertBuffer"   , _vertBuffer );
			normalPass.Dispatch( _kernelnormal , strideX , strideY  , strideZ );
	}



	/*

		EXPORT INFO


	*/

	private int getID( int id  ){

	  int b = (int)Mathf.Floor( id / 6 );
	  int tri  = id % 6;
	  int row = (int)Mathf.Floor( b / (ribbonWidth-1) );
	  int col = (b) % (ribbonWidth -1);

	  int rowU = (row + 1);
	  int colU = (col + 1);

	  int rDoID = row * ribbonWidth;
	  int rUpID = rowU * ribbonWidth;

	  int cDoID = col;
	  int cUpID = colU;

	  if( row < 0 || row >= ribbonWidth || col < 0 || col >= ribbonWidth  ){
	  	print( "no1");
	  }

	  if( rowU < 0 || rowU >= ribbonWidth || colU < 0 || colU >= ribbonWidth  ){
	  	print( "no2");
	  }

	  int fID = 0;

	  if( tri == 0 ){
	      fID = rDoID + cDoID;
	  }else if( tri == 1 ){
	      fID = rUpID + cUpID;
	  }else if( tri == 2 ){
	      fID = rUpID + cDoID;
	  }else if( tri == 3 ){
	      fID = rDoID + cDoID;
	  }else if( tri == 4 ){
	      fID = rDoID + cUpID;
	  }else if( tri == 5 ){
	      fID = rUpID + cUpID;
	  }else{
	      fID = 0;
	  }


	  if( fID >= ribbonLength * ribbonWidth ){
	  	print("NOOOOO");
	  }

	  return fID;

	}



	void createOBJ(){

        if( objMade == false ){
            objMade = true;

            _vertBuffer.GetData( inValues );


            int numVertsTotal = (ribbonWidth-1) * 3 * 2 * (ribbonLength-1);

            
            ObjExporter.MeshInfo mesh = new ObjExporter.MeshInfo();

            Vector3[] verts = new Vector3[ ribbonLength * ribbonWidth ];
            Vector3[] norms = new Vector3[ ribbonLength * ribbonWidth ];
            Vector2[] uvs   = new Vector2[ ribbonLength * ribbonWidth ];
            int[] triangles = new int[numVertsTotal];

            for( int i = 0; i < ribbonWidth * ribbonLength; i++ ){
                
                int baseID = i * VertStructSize;

                Vector3 v = new Vector3( inValues[baseID+0] , inValues[baseID+1], inValues[baseID+2]);
                v = v * 1000;

                Vector3 n = new Vector3( inValues[baseID+10] , inValues[baseID+11], inValues[baseID+12]);//calculateVector( baseID//new Vector3( inValues[baseID+6] , inValues[baseID+7], inValues[baseID+8]);
                Vector2 u = new Vector2( inValues[baseID+13] , inValues[baseID+14]);

                verts[i] = v;
                norms[i] = n;
                uvs[i]   = u;

            }

            for( int i = 0; i < numVertsTotal; i++ ){
                triangles[i] = getID( i );
            }

            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.uvs = uvs;
            mesh.triangles = triangles;
            mesh.name = "ct2";


            ObjExporter.MeshToFile( mesh );

        }


    }
 

    
}