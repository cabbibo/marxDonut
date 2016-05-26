using UnityEngine;
using System.Collections;

public class Wrench : MonoBehaviour {

  private Quaternion oRotation;
  public GameObject BasisObject;


  private Vector3 v1;
  private Vector3 v2;
  private Vector3 v3;

  public float deltaX;
  public float deltaY;

	// Use this for initialization
	void Start () {
	 oRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {

   // v1 = transform.TransformPoint( new Vector3(0,0,0));
    v2 = oRotation * new Vector3(1,0,0);
    v3 = transform.rotation * new Vector3(1,0,0);

    v1 = Vector3.Cross( v2 , v3 );

    deltaX = Vector3.Dot( v1 , BasisObject.transform.TransformDirection( new Vector3(1 , 0 , 0 )));
    deltaY = Vector3.Dot( v1 , BasisObject.transform.TransformDirection( new Vector3(0 , 1 , 0 )));


    float newX = BasisObject.transform.localScale.x;
    float newY = BasisObject.transform.localScale.y;

    if( GetComponent<MoveByController>().moving == true){
      newX += deltaY*.1f;
      newY += deltaX*.1f;

      if( newX < .03f ){ newX = .03f; }
      if( newY < .03f ){ newY = .03f; }
    }

    BasisObject.transform.localScale = new Vector3( newX , newY , BasisObject.transform.localScale.z );
    oRotation = transform.rotation;
	
	}
}
