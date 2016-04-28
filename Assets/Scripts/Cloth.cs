using UnityEngine;
using System.Collections;


using System.IO;
using System.Text;

public class Cloth : MonoBehaviour {


    // How the donut looks
    public Shader shader;
    public Shader debugShader;

    // How the donut feels
    public ComputeShader computeShader;


    public GameObject hand1;
    public GameObject hand2;
    public GameObject hand3;
    public GameObject hand4;


    public float trigger1;
    public float trigger2;
    public float trigger3;
    public float trigger4;

    public float tubeRadius = .6f;
    public float shellRadius = .8f;

    private float[] inValues;


    private ComputeBuffer _vertBuffer;
    private ComputeBuffer _ogBuffer;
    private ComputeBuffer _handBuffer;
    private ComputeBuffer _transBuffer;


    private const int threadX = 4;
    private const int threadY = 4;
    private const int threadZ = 4;

    private const int strideX = 4;
    private const int strideY = 4;
    private const int strideZ = 4;

    private int gridX { get { return threadX * strideX; } }
    private int gridY { get { return threadY * strideY; } }
    private int gridZ { get { return threadZ * strideZ; } }

    private int vertexCount { get { return gridX * gridY * gridZ; } }


    private int ribbonWidth = 64;
    private int ribbonLength { get { return (int)Mathf.Floor( (float)vertexCount / ribbonWidth ); } }
    

    private int _kernel;
    private Material material;
    private Material debugMaterial;

    private Vector3 p1;
    private Vector3 p2;

    //private bool objMade = false;
    private float[] transValues = new float[32];
    private float[] handValues;

    private float oTime = 0;

    struct Vert{
		public Vector3 position;
		public Vector3 oPosition;
		public float mass;
		public float o0;
		public float o1;
		public float o2;
		public float o3;
		public float o4;
		public float o5;
		public float o6;
		public float o7;
	}

	private int VertStructSize =  3 + 3 + 8 + 1;

    // Use this for initialization
    void Start () {

    	oTime = Time.time;
        handValues = new float[ 4 * AssignStructs.HandStructSize ];

        createBuffers();
        createMaterial();

        _kernel = computeShader.FindKernel("CSMain");


        Camera.main.gameObject.AddComponent<PostRenderEvent>();
        

        //TODO:
        //Figure out how to add this script to the main camera!
        Camera.onPostRender += Render;


    
    }

    void Update(){

     
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
      _ogBuffer.Release(); 
      _transBuffer.Release(); 
      DestroyImmediate( material );
      DestroyImmediate( debugMaterial );

    }

        //After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
    //this just draws the "mesh" as a set of points
    public void Render(Camera camera) {


        
        int numVertsTotal = (ribbonWidth-1) * 3 * 2 * (ribbonLength-1);

        material.SetPass(0);

        material.SetBuffer("buf_Points", _vertBuffer);
        material.SetBuffer("og_Points", _ogBuffer);

        material.SetInt( "_RibbonWidth"  , ribbonWidth  );
        material.SetInt( "_RibbonLength" , ribbonLength );
        material.SetInt( "_TotalVerts"   , vertexCount  );

        material.SetMatrix("worldMat", transform.localToWorldMatrix);
        material.SetMatrix("invWorldMat", transform.worldToLocalMatrix);

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

        return new Vector3( u , 1 , v );

    }

    private void createBuffers() {

      _vertBuffer = new ComputeBuffer( vertexCount ,  VertStructSize * sizeof(float));
      _ogBuffer = new ComputeBuffer( vertexCount ,  3 * sizeof(float));
      _transBuffer = new ComputeBuffer( 32 ,  sizeof(float));
      _handBuffer = new ComputeBuffer( 4 , AssignStructs.HandStructSize * sizeof(float));
      
      inValues = new float[ VertStructSize * vertexCount];
      float[] ogValues = new float[ 3         * vertexCount];

      // Used for assigning to our buffer;
      int index = 0;
      int indexOG = 0;


      for (int z = 0; z < gridZ; z++) {
        for (int y = 0; y < gridY; y++) {
          for (int x = 0; x < gridX; x++) {

            int id = x + y * gridX + z * gridX * gridY; 
            
            float col = (float)(id % ribbonWidth );
            float row = Mathf.Floor( ((float)id+.01f) / ribbonWidth);


            float uvX = col / ribbonWidth;
            float uvY = row / ribbonLength;

            Vector3 fVec = getVertPosition( uvX , uvY );


            //pos
            ogValues[indexOG++] = fVec.x;
            ogValues[indexOG++] = fVec.y;
            ogValues[indexOG++] = fVec.z;

            Vert vert = new Vert();


            vert.position = fVec * 1.000001f;

            vert.oPosition = fVec;

            vert.mass = 0.3f;
            vert.o0 = convertToID( col + 1 , row + 0 );
            vert.o1 = convertToID( col + 1 , row - 1 );
            vert.o2 = convertToID( col + 0 , row - 1 );
            vert.o3 = convertToID( col - 1 , row - 1 );
            vert.o4 = convertToID( col - 1 , row - 0 );
            vert.o5 = convertToID( col - 1 , row + 1 );
            vert.o6 = convertToID( col - 0 , row + 1 );
            vert.o7 = convertToID( col + 1 , row + 1 );

            inValues[index++] = vert.position.x;
            inValues[index++] = vert.position.y;
            inValues[index++] = vert.position.z;

            inValues[index++] = vert.oPosition.x;
            inValues[index++] = vert.oPosition.y;
            inValues[index++] = vert.oPosition.z;

            inValues[index++] = vert.mass;

            inValues[index++] = vert.o0;
            inValues[index++] = vert.o1;
            inValues[index++] = vert.o2;
            inValues[index++] = vert.o3;
            inValues[index++] = vert.o4;
            inValues[index++] = vert.o5;
            inValues[index++] = vert.o6;
            inValues[index++] = vert.o7;

          }
        }
      }

      _vertBuffer.SetData(inValues);
      _ogBuffer.SetData(ogValues);

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
    
    private void Dispatch() {



        AssignStructs.AssignTransBuffer( transform , transValues , _transBuffer );

        // Setting up hand buffers
        int index = 0;
        AssignStructs.AssignHandStruct( handValues , index , out index , hand1 , trigger1 );
        AssignStructs.AssignHandStruct( handValues , index , out index , hand2 , trigger2 );
        AssignStructs.AssignHandStruct( handValues , index , out index , hand3 , trigger3 );
        AssignStructs.AssignHandStruct( handValues , index , out index , hand4 , trigger4 );
        _handBuffer.SetData( handValues );


        //computeShader.SetInt( "_NumberHands", 4 );



        computeShader.SetFloat( "_DeltaTime"    , Time.time - oTime );
        computeShader.SetFloat( "_Time"         , Time.time      );

        oTime = Time.time;

        computeShader.SetInt( "_RibbonWidth"   , ribbonWidth     );
        computeShader.SetInt( "_RibbonLength"  , ribbonLength    );


        //computeShader.SetBuffer( _kernel , "transBuffer"  , _transBuffer    );
        computeShader.SetBuffer( _kernel , "vertBuffer"   , _vertBuffer     );
        //computeShader.SetBuffer( _kernel , "ogBuffer"     , _ogBuffer       );
        //computeShader.SetBuffer( _kernel , "handBuffer"   , _handBuffer     );


        /*
		
			TODO: Call proper amount of times 
			per frame, passing off extra time to next frame!

			elapsedTime = lastTime - currentTime
			lastTime = currentTime // reset lastTime
			  
			// add time that couldn't be used last frame
			elapsedTime += leftOverTime
			  
			// divide it up in chunks of 16 ms
			timesteps = floor(elapsedTime / 16)
			  
			// store time we couldn't use for the next frame.
			leftOverTime = elapsedTime - timesteps * 16

        */



		// Which link in compute are we doing
    	//computeShader.SetInt("_Iteration" , 0 );

    	//Switch which pairs we are doing
    	//computeShader.SetInt("_Offset" , 0 );

    	//TODO: only need to dispatch for half of the buffer size!
    	//computeShader.Dispatch( _kernel , strideX , strideY  , strideZ );

        //solver accuracy
		for( int k = 0; k < 4; k++ ){
        for( int i = 0; i < 4; i++ ){

        	// number of Links in solver Loop
        	for( int j = 0; j < 9; j++ ){

	        	// Which link in compute are we doing
	        	computeShader.SetInt("_Iteration" , j );
	        	computeShader.SetInt("_Total" , i );
	        	computeShader.SetInt("_WhichOne", k);

	        	//Switch which pairs we are doing
	        	computeShader.SetInt("_Offset" , i % 4 );

	        	//TODO: only need to dispatch for half of the buffer size!
	        	computeShader.Dispatch( _kernel , strideX , strideY  , strideZ );

        	}

        }
    	}

    }

    
}