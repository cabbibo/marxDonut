using UnityEngine;
using System.Collections;

public class Stretch : MonoBehaviour {

  // TODO:

  private GameObject leftDrag;
  private GameObject rightDrag;

  public GameObject node;

  private Vector3 upVec;

  private Vector3 lScale;

	// Use this for initialization
	void Start () {
    print( "Nodsmade");

    //Instantiate( Pillow , Random.insideUnitSphere * 1.5f + new Vector3( 0 , 1.5f , 0 ) , Random.rotation) as GameObject;

    leftDrag = Instantiate( node ) as GameObject;
    rightDrag = Instantiate( node  ) as GameObject;

    rightDrag.transform.position = transform.localToWorldMatrix.MultiplyPoint( new Vector3(  .6f , 0 , 0 ) ); 
    leftDrag.transform.position  = transform.localToWorldMatrix.MultiplyPoint( new Vector3( -.6f , 0 , 0 ) );

    leftDrag.GetComponent<Renderer>().material.color = new Color(0,0,0,1);
    rightDrag.GetComponent<Renderer>().material.color = new Color(0,0,0,1);

    leftDrag.transform.rotation = transform.rotation;
    rightDrag.transform.rotation = transform.rotation;

    lScale = new Vector3( .1f , .1f , .1f );
    leftDrag.transform.localScale = lScale;
    rightDrag.transform.localScale = lScale;

    leftDrag.AddComponent<MoveByController>();
    rightDrag.AddComponent<MoveByController>();

   //Rigidbody rbl = leftDrag.GetComponent<Rigidbody>(); 
   /// rbl.isKinematic = true;
   //Rigidbody rbr = rightDrag.GetComponent<Rigidbody>(); 
   /// rbr.isKinematic = true;
	
    leftDrag.GetComponent<SphereCollider>().isTrigger = true;
    rightDrag.GetComponent<SphereCollider>().isTrigger = true;

    upVec = transform.localToWorldMatrix.MultiplyVector( new Vector3(0 , 1 , 0) ).normalized;


	}


	
	// Update is called once per frame
	void Update () {
	
    if( GetComponent<MoveByController>().moving == false ){

      leftDrag.transform.parent = null;
      rightDrag.transform.parent = null;

      Vector3 p = leftDrag.transform.position - rightDrag.transform.position;
      Vector3 dif = p;
      p *= .5f;
      p += rightDrag.transform.position;
      transform.position = p;

      transform.LookAt( leftDrag.transform , upVec );

      transform.localScale = new Vector3( .2f , .2f , dif.magnitude * .83333f  );

      leftDrag.transform.localScale = lScale;
      rightDrag.transform.localScale = lScale;

    }else{

      leftDrag.transform.parent = transform;
      rightDrag.transform.parent = transform;

      upVec = transform.localToWorldMatrix.MultiplyVector( new Vector3(0 , 1 , 0) ).normalized;

      leftDrag.transform.localScale = Vector3.zero;// Vector3.Scale( lScale , transform.localScale);
      rightDrag.transform.localScale = Vector3.zero;// Vector3.Scale( lScale , transform.localScale); 


    } 


	}
}
