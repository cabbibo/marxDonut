using UnityEngine;
using System.Collections;

public class FortCloth : MonoBehaviour {

  // How the donut looks
  public Shader shader;
  public Shader debugShader;

  // How the donut feels
  public ComputeShader constraintPass;
  public ComputeShader normalPass;
  public ComputeShader forcePass;

  public Texture2D normalMap;
  public Cubemap cubeMap;

  public float clothSize = 1;
  public float startingHeight = 1;

  private PillowFort PF;

  private ComputeBuffer _vertBuffer;
  public ComputeBuffer _transformBuffer;

  private ComputeBuffer _upLinkBuffer;
  private ComputeBuffer _rightLinkBuffer;
  private ComputeBuffer _diagonalDownLinkBuffer;
  private ComputeBuffer _diagonalUpLinkBuffer;

  public Material material;
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
  private int _kernelconstraint;
  private int _kernelnormal;

  private float[] inValues;
  private float[] transformValues;

  private float oTime = 0;


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

  private int VertStructSize =  3 + 3 + 3 + 3 + 2 + 1 + 8 + 3;
  private int LinkStructSize =  1 + 1 + 1 + 1;


	// Use this for initialization
	void Start () {

    PF = GetComponent<PillowFort>();
    transformValues = new float[ 16 ];

    createBuffers();
    createMaterial();

    _kernelforce = forcePass.FindKernel("CSMain");
    _kernelnormal = normalPass.FindKernel("CSMain");
    _kernelconstraint = constraintPass.FindKernel("CSMain");

    forcePass.SetInt( "_Reset"    , 0 );
    forcePass.SetInt( "_Ended"   , 0 );

    Dispatch();
    
    Camera.onPostRender += Render;
	
	}
	
  public void Restart(){
  
    forcePass.SetInt( "_Reset"    , 1 );
    Dispatch();
    forcePass.SetInt( "_Reset"    , 0);
    

  }

  public void update(){

    if( PF.started > 0 ){
      Dispatch();
    }
  }


  public void setCycle(){
    if( material ){

      forcePass.SetFloat( "_Cycle" , PF.cycle );
      material.SetFloat( "_Cycle" , PF.cycle );

    }
  }

  public void dropCloth(){
    forcePass.SetInt( "_Reset"    , 1 );
    Dispatch();
    forcePass.SetInt( "_Reset"    , 0 );
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
      _transformBuffer.Release(); 

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


   // print( PF.clothDropped );
    if( PF.clothDropped == true ){ 

   
      //print(":s");
      
      int numVertsTotal = (ribbonWidth-1) * 3 * 2 * (ribbonLength-1);

      if( PF.special == 1){

        material.SetPass(0);

        material.SetInt("_Large" , 1 );

        material.SetBuffer("buf_Points", _vertBuffer);
        material.SetBuffer("shapeBuffer", PF.pillows._shapeBuffer);

        material.SetInt( "_NumberHands"   , PF.handBufferInfo.GetComponent<HandBuffer>().numberHands );
        material.SetBuffer( "handBuffer"  , PF.handBufferInfo.GetComponent<HandBuffer>()._handBuffer );

        material.SetFloat( "_FullEnd" , PF.fullEnd );
        material.SetFloat("_ClothDown" , PF.clothDown );
     
        material.SetInt( "_RibbonWidth"  , ribbonWidth  );
        material.SetInt( "_RibbonLength" , ribbonLength );
        material.SetInt( "_TotalVerts"   , vertexCount  );
        material.SetInt( "_NumShapes"   , PF.pillows.Shapes.Length  );

        material.SetFloat("_Special" , PF.special );
        material.SetTexture( "_NormalMap" , normalMap);
        material.SetTexture( "_CubeMap"  , cubeMap );

        material.SetMatrix("worldMat", transform.localToWorldMatrix);
        material.SetMatrix("invWorldMat", transform.worldToLocalMatrix);

        Graphics.DrawProcedural(MeshTopology.Triangles, numVertsTotal);

      }

      material.SetPass(0);
      material.SetInt("_Large" , 0 );
      material.SetBuffer("buf_Points", _vertBuffer);
      material.SetBuffer("shapeBuffer", PF.pillows._shapeBuffer);

      material.SetInt( "_NumberHands"   , PF.handBufferInfo.GetComponent<HandBuffer>().numberHands );
      material.SetBuffer( "handBuffer"  , PF.handBufferInfo.GetComponent<HandBuffer>()._handBuffer );

      material.SetFloat( "_FullEnd" , PF.fullEnd );
      material.SetFloat("_ClothDown" , PF.clothDown );
   
      material.SetInt( "_RibbonWidth"  , ribbonWidth  );
      material.SetInt( "_RibbonLength" , ribbonLength );
      material.SetInt( "_TotalVerts"   , vertexCount  );
      material.SetInt( "_NumShapes"   , PF.pillows.Shapes.Length  );

      material.SetFloat("_Special" , PF.special );
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


  private Vector3 getVertPosition( float uvX , float uvY  ){

        float u = (uvY -.5f);
        float v = (uvX -.5f);

        return new Vector3( u * clothSize, startingHeight , v * clothSize );

    }

  private void createBuffers() {
      _transformBuffer  = new ComputeBuffer( 1 ,  16);      
      _vertBuffer  = new ComputeBuffer( vertexCount ,  VertStructSize * sizeof(float));
      
      _upLinkBuffer             = new ComputeBuffer( vertexCount / 2 , LinkStructSize * sizeof(float));
      _rightLinkBuffer          = new ComputeBuffer( vertexCount / 2 , LinkStructSize * sizeof(float));
      _diagonalDownLinkBuffer   = new ComputeBuffer( vertexCount / 2 , LinkStructSize * sizeof(float));
      _diagonalUpLinkBuffer     = new ComputeBuffer( vertexCount / 2 , LinkStructSize * sizeof(float));

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
    

  private void assignTransform(){

    Matrix4x4 m = transform.worldToLocalMatrix;
    int index = 0;
    for( int j = 0; j < 16; j++ ){
      int x = j % 4;
      int y = (int) Mathf.Floor(j / 4);
      transformValues[index++] = m[x,y];
    }

    _transformBuffer.SetData(transformValues);



  }
  private void Dispatch() {

    
    assignTransform();

    forcePass.SetFloat( "_DeltaTime"    , Time.time - PF.oTime );
    forcePass.SetFloat( "_Time"         , Time.time      );

    ///oTime = Time.time;

    forcePass.SetInt( "_RibbonWidth"   , ribbonWidth     );
    forcePass.SetInt( "_RibbonLength"  , ribbonLength    );
//    print( PF.pillows);
    forcePass.SetInt( "_NumShapes"     , PF.pillows.Shapes.Length   );
    forcePass.SetInt( "_NumberHands"   , PF.handBufferInfo.GetComponent<HandBuffer>().numberHands );


    forcePass.SetBuffer( _kernelforce , "vertBuffer"      , _vertBuffer );
    forcePass.SetBuffer( _kernelforce , "transformBuffer" , _transformBuffer );

    if( PF.pillows._shapeBuffer != null ){
      forcePass.SetBuffer( _kernelforce , "shapeBuffer"     , PF.pillows._shapeBuffer );
    }
    forcePass.SetBuffer( _kernelforce , "handBuffer"      , PF.handBufferInfo.GetComponent<HandBuffer>()._handBuffer );

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

}
