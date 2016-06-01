using UnityEngine;
using System.Collections;

public class Stretch : MonoBehaviour {

  // TODO:

  public GameObject leftDrag;
  public GameObject rightDrag;

  public GameObject node;

  public float length;

  private Vector3 upVec;

  private Vector3 lScale;

  // saving old transforms for delta
  public Transform oLDrag;
  public Transform oRDrag;


	// Use this for initialization
	void Start () {

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

    leftDrag.GetComponent<Wrench>().BasisObject = gameObject;
    rightDrag.GetComponent<Wrench>().BasisObject = gameObject;



   //Rigidbody rbl = leftDrag.GetComponent<Rigidbody>(); 
   /// rbl.isKinematic = true;
   //Rigidbody rbr = rightDrag.GetComponent<Rigidbody>(); 
   /// rbr.isKinematic = true;
	
    leftDrag.GetComponent<SphereCollider>().isTrigger = true;
    rightDrag.GetComponent<SphereCollider>().isTrigger = true;

    upVec = transform.localToWorldMatrix.MultiplyVector( new Vector3(0 , 1 , 0) ).normalized;


	}


  float getLength(){

    Vector3 p = leftDrag.transform.position - rightDrag.transform.position;
    Vector3 dif = p;
 
    return dif.magnitude * .83333f;



  }

  public void setPosition(){

    leftDrag.transform.parent = null;
    rightDrag.transform.parent = null;

    Vector3 p = leftDrag.transform.position - rightDrag.transform.position;
    Vector3 dif = p;
    p *= .5f;
    p += rightDrag.transform.position;
    transform.position = p;

    transform.LookAt( leftDrag.transform , upVec );

    length = getLength();


    transform.localScale = new Vector3( transform.localScale.x , transform.localScale.y , length );

    leftDrag.transform.localScale = lScale;
    rightDrag.transform.localScale = lScale;

  }


	
	// Update is called once per frame
	void Update () {

    if( GetComponent<BeginBox>().ssFinished == false ){

    length = getLength();

    if( leftDrag.GetComponent<MoveByController>().moving == true ){
      SteamVR_TrackedObject tObj = leftDrag.GetComponent<MoveByController>().movingController.GetComponent<SteamVR_TrackedObject>();
      var device = SteamVR_Controller.Input((int)tObj.index);
      device.TriggerHapticPulse(300);
    }

    if( rightDrag.GetComponent<MoveByController>().moving == true ){
      SteamVR_TrackedObject tObj = rightDrag.GetComponent<MoveByController>().movingController.GetComponent<SteamVR_TrackedObject>();
      var device = SteamVR_Controller.Input((int)tObj.index);
      device.TriggerHapticPulse(300);
    }
	
    //if( GetComponent<BeginBox>().hasBegun == true ){
      if( GetComponent<MoveByController>().moving == false ){

        setPosition();
        GetComponent<LineRenderer>().enabled = true;

      }else{
        

        leftDrag.transform.position = transform.localToWorldMatrix.MultiplyPoint( new Vector3( 0 , 0 ,.6f) );
        rightDrag.transform.position = transform.localToWorldMatrix.MultiplyPoint( new Vector3( 0 , 0 ,-.6f) );

       //leftDrag.transform.parent = transform;
       //rightDrag.transform.parent = transform;

        upVec = transform.localToWorldMatrix.MultiplyVector( new Vector3(0 , 1 , 0) ).normalized;

        // leftDrag.transform.localScale = Vector3.zero; //new Vector3( 1 , 1 , 1 );//Vector3.Scale( lScale , new Vector3(1 / transform.localScale.x , 1 / transform.localScale.y , 1 / transform.localScale.z));
        //rightDrag.transform.localScale = Vector3.zero; //new Vector3( 1 , 1 , 1 );//Vector3.Scale( lScale , new Vector3(1 / transform.localScale.x , 1 / transform.localScale.y , 1 / transform.localScale.z));
        //rightDrag.transform.localScale = rightDrag.transform.lossyScale; //Vector3.Scale( lScale , rightDrag.transform.lossyScale );//new Vector3(1 / transform.localScale.x , 1 / transform.localScale.y , 1 / transform.localScale.z);
        GetComponent<LineRenderer>().enabled = false;


      } 

    }
   // }


	}
}
