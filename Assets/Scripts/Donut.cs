using UnityEngine;
using System.Collections;


using System.IO;
using System.Text;

public class Donut : MonoBehaviour {


    // How the donut looks
    public Shader shader;

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


    private int ribbonWidth = 1024;
    private int ribbonLength { get { return (int)Mathf.Floor( (float)vertexCount / ribbonWidth ); } }
    

    private int _kernel;
    private Material material;

    private Vector3 p1;
    private Vector3 p2;

    private bool objMade = false;
    private float[] transValues = new float[32];
    private float[] handValues;


    // Use this for initialization
    void Start () {



        
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

    }

    private int getID( int id  ){

        int b = (int)Mathf.Floor( id / 6 );
        int tri  = id % 6;
        int row = (int)Mathf.Floor( b / ribbonWidth );
        int col = (b) % ribbonWidth;

        int rowU = (row + 1) % ribbonLength;
        int colU = (col + 1) % ribbonWidth;

        int rDoID = row * ribbonWidth;
        int rUpID = rowU * ribbonWidth;

        int cDoID = col;
        int cUpID = colU;

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

        return fID;

    }

    static void WriteToFile(string s, string filename)
    {
        var sr = File.CreateText(filename);
        sr.WriteLine(s);
        sr.Close();
    }


    void createOBJ(){

        if( objMade == false ){
            objMade = true;

            _vertBuffer.GetData( inValues );

            print( inValues[0] );

            int numVertsTotal = ribbonWidth * 3 * 2 * (ribbonLength);

            
            AssignStructs.MeshInfo mesh = new AssignStructs.MeshInfo();

            Vector3[] verts = new Vector3[ ribbonLength * ribbonWidth ];
            Vector3[] norms = new Vector3[ ribbonLength * ribbonWidth ];
            Vector2[] uvs   = new Vector2[ ribbonLength * ribbonWidth ];
            int[] triangles = new int[numVertsTotal];

            for( int i = 0; i < ribbonWidth * ribbonLength; i++ ){
                
                int baseID = i * AssignStructs.VertC4StructSize;

                Vector3 v = new Vector3( inValues[baseID+0] , inValues[baseID+1], inValues[baseID+2]);
                Vector3 n = new Vector3( inValues[baseID+6] , inValues[baseID+7], inValues[baseID+8]);
                Vector2 u = new Vector2( inValues[baseID+9] , inValues[baseID+10]);

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
            mesh.name = "DONTUTEST";


        

            //string objString = makeString( mesh , transform );
            //print( objString );
            //WriteToFile( objString , "DONUT_TEST.obj");

            ObjExporter.MeshToFile( mesh );

        }




    }
 
    //Remember to release buffers and destroy the material when play has been stopped.
    void ReleaseBuffer(){

      _vertBuffer.Release(); 
      _ogBuffer.Release(); 
      _transBuffer.Release(); 
      DestroyImmediate( material );

    }

        //After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
    //this just draws the "mesh" as a set of points
    public void Render(Camera camera) {


        
        int numVertsTotal = ribbonWidth * 3 * 2 * (ribbonLength);

        material.SetPass(0);

        material.SetBuffer("buf_Points", _vertBuffer);
        material.SetBuffer("og_Points", _ogBuffer);

        material.SetInt( "_RibbonWidth"  , ribbonWidth  );
        material.SetInt( "_RibbonLength" , ribbonLength );
        material.SetInt( "_TotalVerts"   , vertexCount  );

        material.SetMatrix("worldMat", transform.localToWorldMatrix);
        material.SetMatrix("invWorldMat", transform.worldToLocalMatrix);

        Graphics.DrawProcedural(MeshTopology.Triangles, numVertsTotal);


    }

    private Vector3 getVertPosition( float uvX , float uvY  ){

        float u = uvY * 2.0f * Mathf.PI;
        float v = uvX * 2.0f * Mathf.PI;

        float largeMovement = Mathf.Sin( uvY * 10.0f ) * .3f;
        float smallMovement = Mathf.Sin( uvY * 100.0f )  * ( uvY * uvY * .03f);
        float tubeRad = .2f; //tubeRadius * Mathf.Pow( uvY - .01f , .3f)  * ( 1.0f + largeMovement + smallMovement ) ;
        float slideRad = .8f;//shellRadius / 2.0f + uvY;

        float xV = (slideRad + tubeRad * Mathf.Cos(v)) * Mathf.Cos(u) ;
        float zV = (slideRad + tubeRad * Mathf.Cos(v)) * Mathf.Sin(u) ;
        float yV = (tubeRad) * Mathf.Sin(v) + tubeRad;

        //print( xV );
        return new Vector3( xV , yV , zV );

    }

    private void createBuffers() {

      _vertBuffer = new ComputeBuffer( vertexCount ,  AssignStructs.VertC4StructSize * sizeof(float));
      _ogBuffer = new ComputeBuffer( vertexCount ,  3 * sizeof(float));
      _transBuffer = new ComputeBuffer( 32 ,  sizeof(float));
      _handBuffer = new ComputeBuffer( 4 , AssignStructs.HandStructSize * sizeof(float));
      
      inValues = new float[ AssignStructs.VertC4StructSize * vertexCount];
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

            AssignStructs.VertC4 vert = new AssignStructs.VertC4();


            vert.pos = fVec * .9f;
            vert.vel = new Vector3( 0 , 0 , 0 );
            vert.nor = new Vector3( 0 , 1 , 0 );
            vert.uv  = new Vector2( uvX , uvY );
            vert.ribbonID = 0;
            vert.life = -1;
            vert.debug = new Vector3( 0 , 1 , 0 );
            vert.row   = row; 
            vert.col   = col; 

            vert.lID = convertToID( col - 1 , row + 0 );
            vert.rID = convertToID( col + 1 , row + 0 );
            vert.uID = convertToID( col + 0 , row + 1 );
            vert.dID = convertToID( col + 0 , row - 1 );

            AssignStructs.AssignVertC4Struct( inValues , index , out index , vert );

          }
        }
      }

      _vertBuffer.SetData(inValues);
      _ogBuffer.SetData(ogValues);

    }

    private float convertToID( float col , float row ){

        float id;

        if( col >= ribbonWidth ){ col -= ribbonWidth; }
        if( col < 0 ){ col += ribbonWidth; }

        if( row >= ribbonLength ){ row -= ribbonLength; }
        if( row < 0 ){ row += ribbonLength; }

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


        computeShader.SetInt( "_NumberHands", 4 );

        computeShader.SetFloat( "_DeltaTime"    , Time.deltaTime );
        computeShader.SetFloat( "_Time"         , Time.time      );

        computeShader.SetInt( "_RibbonWidth"   , ribbonWidth     );
        computeShader.SetInt( "_RibbonLength"  , ribbonLength    );


        computeShader.SetBuffer( _kernel , "transBuffer"  , _transBuffer    );
        computeShader.SetBuffer( _kernel , "vertBuffer"   , _vertBuffer     );
        computeShader.SetBuffer( _kernel , "ogBuffer"     , _ogBuffer       );
        computeShader.SetBuffer( _kernel , "handBuffer"   , _handBuffer     );

        computeShader.Dispatch(_kernel, strideX , strideY , strideZ );


    }
}
