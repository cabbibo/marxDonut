using UnityEngine;
using System.Collections;

public class LuneExport : MonoBehaviour {

	
  /*

    EXPORT INFO


  

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


  ObjExporter.MeshInfo createClothMesh(){

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
        //v = v * 1000;

        Vector3 n = new Vector3( inValues[baseID+10] , inValues[baseID+11], inValues[baseID+12]);//calculateVector( baseID//new Vector3( inValues[baseID+6] , inValues[baseID+7], inValues[baseID+8]);
        Vector2 u = new Vector2( inValues[baseID+13] , inValues[baseID+14]);

        verts[i] = v;
        norms[i] = n * -1;
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

    return mesh;

  }

  ObjExporter.MeshInfo createShapeMesh( int index , int baseID ,   GameObject Shape  ){

    
    ObjExporter.MeshInfo mesh = new ObjExporter.MeshInfo();
    Mesh m = Shape.GetComponent<MeshFilter>().mesh;

    Vector3[] verts = new Vector3[ m.vertices.Length];
    Vector3[] norms = new Vector3[ m.normals.Length ];
    Vector2[] uvs   = new Vector2[ m.uv.Length ];
    int[] triangles = new int[ m.triangles.Length ];

        Matrix4x4 mat = Shape.transform.localToWorldMatrix;

        for( int i = 0; i < m.vertices.Length; i++ ){

          Vector3 v = mat.MultiplyPoint( m.vertices[i] );
          verts[i] = v;

          Vector3 n = mat.MultiplyVector( m.normals[i] );
          norms[i] = n.normalized;

          uvs[i] = m.uv[i];

        }
  
        for( int i = 0; i < m.triangles.Length; i++ ){
          triangles[ i ] = m.triangles[i] + baseID;

        }

    mesh.vertices = verts;
    mesh.normals = norms;
    mesh.uvs = uvs;
    mesh.triangles = triangles;
    mesh.name = "Crystal_" + index;

    print( mesh.name );

    return mesh;

  }

  void createOBJ(){

        //if( objMade == false ){
         //   objMade = true;

            _vertBuffer.GetData( inValues );


            int numVertsTotal = (ribbonWidth-1) * 3 * 2 * (ribbonLength-1);

            
            ObjExporter.MeshInfo mesh = createClothMesh();

            ObjExporter.MeshInfo[] meshes = new ObjExporter.MeshInfo[ Shapes.Length + 1 ];
            meshes[0] = mesh;
            for( int i = 0; i < Shapes.Length; i++ ){

              int baseID = i * Shapes[i].GetComponent<MeshFilter>().mesh.vertices.Length + mesh.vertices.Length;
              meshes[i+1] = createShapeMesh( i , baseID , Shapes[i] );
            }


            ObjExporter.MeshesToFile( "JELLOPearl", meshes );

        //}


    }*/
}
