using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;

public class ObjExporter {
 
    public static string MeshToString( AssignStructs.MeshInfo m ) {
      
        StringBuilder sb = new StringBuilder();
 
        sb.Append("g ").Append( m.name ).Append("\n");
        foreach(Vector3 v in m.vertices) {
            sb.Append(string.Format("v {0} {1} {2}\n",v.x,v.y,v.z));
        }
        sb.Append("\n");
        foreach(Vector3 v in m.normals) {
            sb.Append(string.Format("vn {0} {1} {2}\n",v.x,v.y,v.z));
        }
        sb.Append("\n");
        foreach(Vector2 v in m.uvs) {
            sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
        }

           
        for (int i=0;i<m.triangles.Length;i+=3) {
            sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
                m.triangles[i]+1, m.triangles[i+1]+1, m.triangles[i+2]+1));
        }
        
        return sb.ToString();
    }
 
    public static void MeshToFile(AssignStructs.MeshInfo m ) {

    	string filename = m.name + ".obj";

        using (StreamWriter sw = new StreamWriter(filename)) 
        {
            sw.Write(MeshToString(m));
        }
    }
}