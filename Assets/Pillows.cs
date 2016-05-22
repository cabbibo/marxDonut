using UnityEngine;
using System.Collections;

public class Pillows : MonoBehaviour {

  public int NumShapes;
  public GameObject[] Shapes;

  public GameObject Pillow;
  public GameObject Node;

  // How the donut looks
  public Shader shader;
  public Shader debugShader;

  // How the donut feels
  public ComputeShader forcePass;

  public bool allBlocksStarted;
  public bool allBlocksSS;

  public Texture2D normalMap;
  public Cubemap cubeMap;

  private PillowFort PF;

  public ComputeBuffer _vertBuffer;
  public ComputeBuffer _shapeBuffer;

  private Material material;
  private Material debugMaterial;

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

  private float[] inValues;
  private float[] shapeValues;

  struct Shape{
    public Matrix4x4 mat;
    public float shape;
  }

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

  private int VertStructSize =  3 + 3 + 3 + 3 + 2 + 1 + 8 + 3;
  private int ShapeStructSize = 16 + 1;

  private float oTime = 0;

  // Use this for initialization
  void Start () {

    PF = GetComponent<PillowFort>();
    
    createShapes();

    shapeValues = new float[ Shapes.Length * ShapeStructSize ];
    createBuffers();
    createMaterial();

    _kernelforce = forcePass.FindKernel("CSMain");

    forcePass.SetInt( "_Reset"    , 0 );
    forcePass.SetInt( "_Ended"   , 0 );


    //Dispatch();
    //Camera.onPostRender += Render;

  
  }
  
  public void dropCloth(){
    //forcePass.SetInt( "_Reset"    , 1 );
    //Dispatch();
    //forcePass.SetInt( "_Reset"    , 0 );

    for( int i = 0;  i < NumShapes; i++ ){ 
      Shapes[i].GetComponent<BeginBox>().canBeginSS = true;
    }

  }

  public void setCycle(){
    if( material ){
      material.SetFloat( "_Cycle" , PF.cycle );
      for( int i = 0;  i < NumShapes; i++ ){ 
        Shapes[i].GetComponent<Renderer>().material.SetFloat("_Cycle" , PF.cycle );
      }
    }
  }


  // Update is called once per frame
  public void update () {

    allBlocksStarted = true;
    for( int i = 0; i < Shapes.Length; i++ ){

      if( Shapes[i].GetComponent<BeginBox>().begun == false ){ allBlocksStarted = false; }

    }

    allBlocksSS = true;
    for( int i = 0; i < Shapes.Length; i++ ){

      if( Shapes[i].GetComponent<BeginBox>().begunSS == false ){ allBlocksSS = false; }

    }

    assignShapeBuffer();
    /*if( started > 0 ){
      Dispatch();
    }*/

  
  }

  //When this GameObject is disabled we must release the buffers or else Unity complains.
    private void OnDisable(){
       // Camera.onPostRender -= Render;
        ReleaseBuffer();
    }


      //For some reason I made this method to create a material from the attached shader.
    private void createMaterial(){

      material = new Material( shader );
      debugMaterial = new Material( debugShader );

    }


    private Vector3 getVertPosition( float uvX , float uvY  ){

        float u = (uvY -.5f);
        float v = (uvX -.5f);

        return new Vector3( u * 1, 0 , v * 1 );

    }

          //After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
  //this just draws the "mesh" as a set of points
  public void Render(Camera camera) {


    if( PF.clothDropped == true ){ 
      
      int numVertsTotal = (ribbonWidth-1) * 3 * 2 * (ribbonLength-1);

      material.SetPass(0);

      material.SetBuffer("buf_Points", _vertBuffer);
      material.SetBuffer("shapeBuffer", _shapeBuffer);

      material.SetFloat( "_FullEnd" , PF.fullEnd );
   
      material.SetInt( "_RibbonWidth"  , ribbonWidth  );
      material.SetInt( "_RibbonLength" , ribbonLength );
      material.SetInt( "_TotalVerts"   , vertexCount  );
      material.SetInt( "_NumShapes"   , Shapes.Length  );
      material.SetTexture( "_NormalMap" , normalMap);
      material.SetTexture( "_CubeMap"  , cubeMap );

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

    }

  }


    //Remember to release buffers and destroy the material when play has been stopped.
    void ReleaseBuffer(){

      _vertBuffer.Release(); 
      _shapeBuffer.Release(); 
    
      DestroyImmediate( material );
      DestroyImmediate( debugMaterial );

    }


    private void createBuffers() {

      _shapeBuffer = new ComputeBuffer( Shapes.Length , ShapeStructSize * sizeof(float) );      
      _vertBuffer  = new ComputeBuffer( vertexCount ,  VertStructSize * sizeof(float));


      float lRight = 1 / (float)ribbonWidth;
      float lUp = 1 / (float)ribbonLength;


      Vector2 n = new Vector2( lRight , lUp );

      float lDia = n.magnitude;
      
      inValues = new float[ VertStructSize * vertexCount];

      // Used for assigning to our buffer;
      int index = 0;
      int indexOG = 0;
      int li1= 0;
      int li2= 0;
      int li3= 0;
      int li4= 0;


      /*        // second rite up here
       u   dU   x  . r
       . .           // third rite down here
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

            float uvX = col / ribbonWidth;
            float uvY = row / ribbonLength;

            Vector3 fVec = getVertPosition( uvX , uvY );


            Vert vert = new Vert();


            vert.pos = fVec * 1.000001f;

            vert.oPos = fVec- new Vector3( 0 , 0 , 0 );
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
    

    }



  void createShapes(){

    Shapes = new GameObject[NumShapes];
    for( int i = 0;  i < NumShapes; i++ ){ 

      Shapes[i] = Instantiate( Pillow , Random.insideUnitSphere * 1.0f + new Vector3( 0 , 1.0f , 0 ) , Random.rotation) as GameObject;
      Shapes[i].GetComponent<Stretch>().node = Node;

      //print( "noadsassinged");

      Vector3 v = Random.insideUnitSphere;
      Shapes[i].GetComponent<BeginBox>().targetScale = new Vector3( .2f , .2f , Random.Range( .05f , .1f ) );

      Shapes[i].GetComponent<Renderer>().material.SetTexture("_CubeMap" , cubeMap );

    }

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

        
     

        forcePass.SetFloat( "_DeltaTime"    , Time.time - PF.oTime );
        forcePass.SetFloat( "_Time"         , Time.time      );


        forcePass.SetInt( "_RibbonWidth"   , ribbonWidth     );
        forcePass.SetInt( "_RibbonLength"  , ribbonLength    );
        forcePass.SetInt( "_NumShapes"     , Shapes.Length   );
        forcePass.SetInt( "_NumberHands"   , PF.handBufferInfo.GetComponent<HandBuffer>().numberHands );

        forcePass.SetBuffer( _kernelforce , "vertBuffer"   , _vertBuffer );
        forcePass.SetBuffer( _kernelforce , "shapeBuffer"   , _shapeBuffer );
        forcePass.SetBuffer( _kernelforce , "transformBuffer"   , PF.fortCloth._transformBuffer );
        forcePass.SetBuffer( _kernelforce , "handBuffer"        , PF.handBufferInfo.GetComponent<HandBuffer>()._handBuffer );

        forcePass.Dispatch( _kernelforce , strideX , strideY  , strideZ );


  }

}
