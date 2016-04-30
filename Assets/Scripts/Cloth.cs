using UnityEngine;
using System.Collections;


using System.IO;
using System.Text;

public class Cloth : MonoBehaviour {


    // How the donut looks
    public Shader shader;
    public Shader debugShader;

    // How the donut feels
    public ComputeShader constraintPass;
    public ComputeShader collisionPass;
    public ComputeShader forcePass;

    public GameObject[] Shapes;

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


    private const int threadX = 6;
    private const int threadY = 6;
    private const int threadZ = 6;

    private const int strideX = 6;
    private const int strideY = 6;
    private const int strideZ = 6;

    private int gridX { get { return threadX * strideX; } }
    private int gridY { get { return threadY * strideY; } }
    private int gridZ { get { return threadZ * strideZ; } }

    private int vertexCount { get { return gridX * gridY * gridZ; } }




    private int ribbonWidth = 216;
    private int ribbonLength { get { return (int)Mathf.Floor( (float)vertexCount / ribbonWidth ); } }
    

    private int _kernelforce;
    private int _kernelcollision;
    private int _kernelconstraint;


    private Material material;
    private Material debugMaterial;

    private Vector3 p1;
    private Vector3 p2;

    private float oTime = 0;

    struct Vert{
		public Vector3 pos;
		public Vector3 oPos;
		public Vector3 ogPos;
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

	private int VertStructSize =  3 + 3 + 3 + 1 + 8 + 3;
	private int LinkStructSize =  1 + 1 + 1 + 1;
	private int ShapeStructSize = 16 + 1;

    // Use this for initialization
    void Start () {

    	oTime = Time.time;
        shapeValues = new float[ Shapes.Length * ShapeStructSize ];

        createBuffers();
        createMaterial();

        _kernelforce = forcePass.FindKernel("CSMain");
        _kernelcollision = collisionPass.FindKernel("CSMain");
        _kernelconstraint = constraintPass.FindKernel("CSMain");


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

        return new Vector3( u * clothSize, startingHeight , v * clothSize );

    }

    private void createBuffers() {

      _shapeBuffer = new ComputeBuffer( Shapes.Length , ShapeStructSize * sizeof(float) );

      _vertBuffer = new ComputeBuffer( vertexCount ,  VertStructSize * sizeof(float));
      
      _upLinkBuffer = new ComputeBuffer( vertexCount / 2  , LinkStructSize * sizeof(float));
      _rightLinkBuffer = new ComputeBuffer( vertexCount / 2  , LinkStructSize * sizeof(float));
      _diagonalDownLinkBuffer = new ComputeBuffer( vertexCount / 2  , LinkStructSize * sizeof(float));
      _diagonalUpLinkBuffer = new ComputeBuffer( vertexCount / 2 , LinkStructSize * sizeof(float));

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


      for (int z = 0; z < gridZ; z++) {
        for (int y = 0; y < gridY; y++) {
          for (int x = 0; x < gridX; x++) {

            int id = x + y * gridX + z * gridX * gridY; 
            
            float col = (float)(id % ribbonWidth );
            float row = Mathf.Floor( ((float)id) / ribbonWidth);

            if( row % 2 == 0 ){

            	upLinkValues[li1++] = id;
            	upLinkValues[li1++] = convertToID( col + 0 , row + 1 );
            	upLinkValues[li1++] = lUp;
            	upLinkValues[li1++] = 1;

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

            vert.oPos = fVec;
            vert.ogPos = fVec;

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

            inValues[index++] = vert.mass;

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

	}

    
}