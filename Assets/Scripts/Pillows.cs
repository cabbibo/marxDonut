using UnityEngine;
using System.Collections;

public class Pillows : MonoBehaviour {

  public int NumShapes;
  public GameObject[] Shapes;

  public GameObject Pillow;
  public GameObject Node;

  // How the donut looks
  public Shader shader;

  // How the donut feels
  public ComputeShader forcePass;

  public bool allBlocksStarted;
  public bool allBlocksSS;
  public bool allPillowsPopped;
  public int pillowsPopped = 0;

  public Texture2D normalMap;
  public Cubemap cubeMap;

  private PillowFort PF;

  public ComputeBuffer _vertBuffer;
  public ComputeBuffer _shapeBuffer;
  public ComputeBuffer _inverseShapeBuffer;
  public ComputeBuffer _startedBuffer;

  private Material material;

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


  private int ribbonWidth { get { return (int)Mathf.Floor( Mathf.Pow( (float)vertexCount / (6 * NumShapes)  , .5f ) ); }}
  private int ribbonLength { get { return (int)Mathf.Floor( Mathf.Pow( (float)vertexCount / (6 * NumShapes)  , .5f ) ); }} //{ get { return (int)Mathf.Floor( (float)vertexCount / ribbonWidth ); } }
  

  private int _kernelforce;

  private float[] inValues;
  private float[] shapeValues;
  private float[] inverseShapeValues;
  private float[] startedVals;

  private float[] shapesActive;

  
 

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

    print( "RibbonWidth" );
    print( ribbonWidth );
    
    createShapes();

    shapeValues = new float[ Shapes.Length * ShapeStructSize ];
    inverseShapeValues = new float[ Shapes.Length * ShapeStructSize ];
    shapesActive = new float[ Shapes.Length ];

    createBuffers();
    createMaterial();

    _kernelforce = forcePass.FindKernel("CSMain");

    forcePass.SetInt( "_Reset"    , 0 );
    forcePass.SetInt( "_Ended"   , 0 );


    startedVals = new float[ Shapes.Length ];

    //Dispatch();
    Camera.onPostRender += Render;

  
  }

  public void Restart(){

    forcePass.SetInt( "_Reset"    , 0 );
    forcePass.SetInt( "_Ended"   , 0 );
  
    for( int i = 0;  i < NumShapes; i++ ){ 
      Shapes[i].GetComponent<BeginBox>().Restart();
      shapesActive[i] = 1;
    }


  }
  
  public void dropCloth(){
    for( int i = 0;  i < NumShapes; i++ ){ 
      //Shapes[i].GetComponent<BeginBox>().canBeginSS = true;
      shapesActive[i] = 1;
    }

  }

  public void fullClothDropped(){

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

  public void onClothDisappear(){
    for( int i = 0; i < Shapes.Length; i++ ){
      shapesActive[i] = 0;
      Shapes[i].GetComponent<BeginBox>().canBeginPop = true;
     // Shapes[i].transform.position = new Vector3( 100000 , 0 , 0 );
     // Shapes[i].GetComponent<Stretch>().leftDrag.transform.position = new Vector3( 100000 , 0 , 0 );
     // Shapes[i].GetComponent<Stretch>().rightDrag.transform.position = new Vector3( 100000 , 0 , 0 );
    }

  }


  private void CheckForRandomEnter(){
    for( int i = 0; i < Shapes.Length; i++ ){

      if( Shapes[i].GetComponent<BeginBox>().entering == false && Shapes[i].GetComponent<BeginBox>().entered == false){

        float f = Random.Range(0.0f,1.0f);
        if( f < .01 ){
          Shapes[i].GetComponent<BeginBox>().BeginEnter();
        }
      }
    }
  }

  // Update is called once per frame
  public void update () {


    if( PF.fadedIn == true ){
      CheckForRandomEnter();
    }

    allBlocksStarted = true;
    allBlocksSS = true;
    pillowsPopped = 0;
    allPillowsPopped = true;

    for( int i = 0; i < Shapes.Length; i++ ){

      if( Shapes[i].GetComponent<BeginBox>().begun == false ){ allBlocksStarted = false; }
      if( Shapes[i].GetComponent<BeginBox>().begunSS == false ){ allBlocksSS = false; }
      if( Shapes[i].GetComponent<BeginBox>().popped == false ){ allPillowsPopped = false; }else{
        pillowsPopped ++;
      }

      startedVals[i] = Shapes[i].GetComponent<BeginBox>().beginVal + Shapes[i].GetComponent<BeginBox>().secondVal;

    }

    _startedBuffer.SetData(startedVals);

    assignShapeBuffer();

    Dispatch();
    /*if( started > 0 ){
      Dispatch();
    }*/

  
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


  void createShapes(){

    Shapes = new GameObject[NumShapes];
    for( int i = 0;  i < NumShapes; i++ ){ 

      Shapes[i] = Instantiate( Pillow , Random.insideUnitSphere * 1.0f + new Vector3( 0 , 1.0f , 0 ) , Random.rotation) as GameObject;
      Shapes[i].GetComponent<Stretch>().node = Node;

      //print( "noadsassinged");

      Vector3 v = Random.insideUnitSphere;
      Shapes[i].GetComponent<BeginBox>().targetScale = new Vector3( .2f , .2f , Random.Range( .05f , .1f ) );

      Shapes[i].GetComponent<Renderer>().material.SetTexture("_CubeMap" , cubeMap );
      Shapes[i].GetComponent<Renderer>().enabled = false;

    }

  }



          //After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
  //this just draws the "mesh" as a set of points
  public void Render(Camera camera) {


    //if( PF.clothDropped == true ){ 
      
      int numVertsTotal = (ribbonWidth-1) * 3 * 2 * (ribbonLength-1) * 6 * NumShapes;

      material.SetPass(0);

      material.SetBuffer("buf_Points", _vertBuffer);
      //forcePass.SetBuffer( _kernelforce , "shapeBuffer"   , _inverseShapeBuffer );
      material.SetBuffer("shapeBuffer", _inverseShapeBuffer );

      material.SetFloat( "_FullEnd" , PF.fullEnd );
   
      material.SetInt( "_RibbonWidth"  , ribbonWidth  );
      material.SetInt( "_RibbonLength" , ribbonLength );
      material.SetInt( "_TotalVerts"   , vertexCount  );
      material.SetInt( "_NumShapes"   , Shapes.Length  );
      material.SetTexture( "_NormalMap" , normalMap);
      material.SetTexture( "_CubeMap"  , cubeMap );

      

      material.SetBuffer("startedBuffer", _startedBuffer );

      material.SetMatrix("worldMat", transform.localToWorldMatrix);
      material.SetMatrix("invWorldMat", transform.worldToLocalMatrix);

      Graphics.DrawProcedural(MeshTopology.Triangles, numVertsTotal);

    //}

  }


    //Remember to release buffers and destroy the material when play has been stopped.
    void ReleaseBuffer(){

      _vertBuffer.Release(); 
      _startedBuffer.Release(); 
      _shapeBuffer.Release(); 
      _inverseShapeBuffer.Release(); 
    
      DestroyImmediate( material );

    }



    private Vector3 getVertPosition( float uvX , float uvY  , int side ){

        float u = (uvY -.5f)* 2;
        float v = (uvX -.5f)* 2;

        Vector2 uv = new Vector2( u , v );
        Vector3 n;
        if( side == 0 ){
          n = new Vector3( 1 , u * 1 , v * 1 );
        }else if( side == 1 ){
          n = new Vector3( -1 , -u*1 , - v*1  );
        }else if( side == 2 ){
          n = new Vector3(  -u*1 , 1  , v*1  );
        }else if( side == 3 ){
          n = new Vector3(  u*1 , -1  , -v*1  );
        }else if( side == 4 ){
          n = new Vector3(  -v*1 , -u * 1 , 1 );
        }else if( side == 5 ){
          n = new Vector3(  v*1 , u * 1 , -1  );
        }else{
          n = new Vector3( 0 , 0 , 0);
          print("ISAAC YOU ARE SO DUMB");
        }
        n /= 2;

        return n;



    }

    private void createBuffers() {

      _shapeBuffer = new ComputeBuffer( Shapes.Length , ShapeStructSize * sizeof(float) );      
      _inverseShapeBuffer = new ComputeBuffer( Shapes.Length , ShapeStructSize * sizeof(float) );      
      _vertBuffer  = new ComputeBuffer( vertexCount ,  VertStructSize * sizeof(float));
      _startedBuffer  = new ComputeBuffer( NumShapes ,   sizeof(float));


      float lRight = 1 / (float)ribbonWidth;
      float lUp = 1 / (float)ribbonLength;


      Vector2 n = new Vector2( lRight , lUp );

      float lDia = n.magnitude;
      
      inValues = new float[ VertStructSize * vertexCount];

      // Used for assigning to our buffer;
      int index = 0;


      for (int z = 0; z < gridZ; z++) {
        for (int y = 0; y < gridY; y++) {
          for (int x = 0; x < gridX; x++) {

            int id = x + y * gridX + z * gridX * gridY; 

            // Gets which box we are in
            int box = (int)Mathf.Floor( (float)id / (float)( ribbonWidth * ribbonLength * 6));
            int inBoxID = id % ( ribbonWidth * ribbonLength * 6);
            int inSideID = inBoxID % (ribbonWidth * ribbonLength);
            int side = (int)Mathf.Floor( (float)inBoxID / (float)( ribbonWidth * ribbonLength));

            
            float col = (float)(inSideID % ribbonWidth );
            float row = Mathf.Floor( ((float)inSideID + 0.01f) / ribbonWidth);

            float uvX = col / (ribbonWidth-1);
            float uvY = row / (ribbonLength-1);


             if( inBoxID == 31 * 31 ){
              print( "BOX : " );
              print( (float)box );
              print( uvX );
              print( uvY );
              print( side );

            }

            Vector3 fVec = getVertPosition( uvX , uvY , side );


            Vert vert = new Vert();


            vert.pos = fVec * 1.000001f;

            vert.oPos = fVec;
            vert.ogPos = fVec ;
            vert.norm = fVec.normalized;//new Vector3( 0 , 1 , 0 );
            vert.uv = new Vector2( uvX , uvY );

            vert.ids = new float[8];
            vert.ids[0] = convertToID( col + 1 , row + 0 , box , side );
            vert.ids[1] = convertToID( col + 1 , row - 1 , box , side );
            vert.ids[2] = convertToID( col + 0 , row - 1 , box , side );
            vert.ids[3] = convertToID( col - 1 , row - 1 , box , side );
            vert.ids[4] = convertToID( col - 1 , row - 0 , box , side );
            vert.ids[5] = convertToID( col - 1 , row + 1 , box , side );
            vert.ids[6] = convertToID( col - 0 , row + 1 , box , side );
            vert.ids[7] = convertToID( col + 1 , row + 1 , box , side );

            vert.debug = new Vector3(0,1,0);

            inValues[index++] = vert.pos.x;
            inValues[index++] = vert.pos.y;
            inValues[index++] = vert.pos.z;

            // using velocity on this one!
            inValues[index++] = 0;
            inValues[index++] = 0;
            inValues[index++] = 0;

            inValues[index++] = vert.ogPos.x;
            inValues[index++] = vert.ogPos.y;
            inValues[index++] = vert.ogPos.z;

            inValues[index++] = vert.norm.x;
            inValues[index++] = vert.norm.y;
            inValues[index++] = vert.norm.z;

            inValues[index++] = vert.uv.x;
            inValues[index++] = vert.uv.y;


           
            inValues[index++] = (float)box;

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






  private float convertToID( float col , float row , int box , int side ){

      float id;

      if( col >= ribbonWidth ){ return -1; }
      if( col < 0 ){ return -1; }

      if( row >= ribbonLength ){ return -1; }
      if( row < 0 ){ return -1; }

      id = row * ribbonWidth + col + side * ribbonWidth * ribbonLength + box * ribbonWidth * ribbonLength * 6;

      return id;

  }


	
  private void assignShapeBuffer(){

    int index = 0;

    for( int i = 0; i < Shapes.Length; i++ ){
      GameObject go = Shapes[i];
      for( int j = 0; j < 16; j++ ){
        int x = j % 4;
        int y = (int) Mathf.Floor(j / 4);
        inverseShapeValues[index] = go.transform.localToWorldMatrix[x,y];
        shapeValues[index++] = go.transform.worldToLocalMatrix[x,y];
        
      }

      // TODO:
      // Make different for different shapes
      inverseShapeValues[index] = shapesActive[i];
      shapeValues[index++] = shapesActive[i];
      

    }

    _shapeBuffer.SetData(shapeValues);
    _inverseShapeBuffer.SetData(inverseShapeValues);

  }


   private void Dispatch() {

        forcePass.SetFloat( "_DeltaTime"    , Time.time - PF.oTime );
        forcePass.SetFloat( "_Time"         , Time.time      );
        forcePass.SetFloat( "_StartedCloth" , PF.clothDown );


        forcePass.SetInt( "_RibbonWidth"   , ribbonWidth     );
        forcePass.SetInt( "_RibbonLength"  , ribbonLength    );
        forcePass.SetInt( "_NumShapes"     , Shapes.Length   );
        forcePass.SetInt( "_NumberHands"   , PF.handBufferInfo.GetComponent<HandBuffer>().numberHands );

        forcePass.SetBuffer( _kernelforce , "vertBuffer"   , _vertBuffer );
        forcePass.SetBuffer( _kernelforce , "startedBuffer"   , _startedBuffer );

        forcePass.SetBuffer( _kernelforce , "shapeBuffer"   , _inverseShapeBuffer );
        forcePass.SetBuffer( _kernelforce , "transformBuffer"   , PF.fortCloth._transformBuffer );
        forcePass.SetBuffer( _kernelforce , "handBuffer"        , PF.handBufferInfo.GetComponent<HandBuffer>()._handBuffer );

        forcePass.Dispatch( _kernelforce , strideX , strideY  , strideZ );


  }

}
