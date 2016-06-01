using UnityEngine;
using System.Collections;

public class MoveByController : MonoBehaviour {


  public Transform ogTransform;
  public bool moving;
  public bool maintainVelocity;

  private bool inside;
  private Vector3 oPos;
  private Vector3[] posArray = new Vector3[3];
  private Vector3 vel;

  private Quaternion relQuat;
  private Vector3 relPos;

  private GameObject insideGO;
  private GameObject secondInsideGO;
  public GameObject movingController;


  Collider colInside;

	void OnEnable(){

    EventManager.OnTriggerDown += OnTriggerDown;
    EventManager.OnTriggerUp += OnTriggerUp;
    EventManager.StayTrigger += StayTrigger;
    inside = false;
    moving = false;

    //posArray = new Vector3[10];
  }

	// Update is called once per frame
	void Update () {
    if( moving == true ){
      for( int i  = 2; i > 0; i --){
        posArray[i] = posArray[i-1];
      }
      posArray[0] = insideGO.transform.position;
     
     // vel = oPos - pos;
      transform.position = insideGO.transform.position;
      transform.rotation = insideGO.transform.rotation * relQuat;

      transform.position = transform.position - ( insideGO.transform.rotation* relPos);
      //transform.rotation = transform.rotation * relQuat;
    }
	
	}

  void OnTriggerDown(GameObject o){

    

    if( inside == true  ){

      if( insideGO == o.GetComponent<controllerInfo>().interactionTip ){

        //if( o.GetInstanceID() == insideGO.transform.parent.GetInstanceID() ){
        //transform.SetParent(o.transform);
        moving = true;

        relPos = insideGO.transform.position - transform.position;

        relQuat = Quaternion.Inverse(insideGO.transform.rotation) * transform.rotation;
        relPos = Quaternion.Inverse(insideGO.transform.rotation) * relPos;

        GetComponent<Rigidbody>().isKinematic = true;
      //}

        movingController = o;



      }else if( secondInsideGO == o.GetComponent<controllerInfo>().interactionTip  ){

        GameObject tmp = insideGO;
        insideGO = secondInsideGO;
        secondInsideGO = tmp;


      //if( o.GetInstanceID() == insideGO.transform.parent.GetInstanceID() ){
      //transform.SetParent(o.transform);
      moving = true;

      relPos = insideGO.transform.position - transform.position;

      relQuat = Quaternion.Inverse(insideGO.transform.rotation) * transform.rotation;
      relPos = Quaternion.Inverse(insideGO.transform.rotation) * relPos;

      GetComponent<Rigidbody>().isKinematic = true;
    //}

      movingController = o;

      }


    
    }
  }

  void OnTriggerUp(GameObject o){
   //transform.SetParent(ogTransform);
    

    if( maintainVelocity == true && moving == true   ){

      for( int i = 0; i<2; i++){
        vel += ( posArray[i] - posArray[i+1] );
      }
      vel /= 3;
      print( vel );
      GetComponent<Rigidbody>().velocity = vel * 200.0f;
      GetComponent<Rigidbody>().isKinematic = false; //= vel * 120.0f;

    }

    if( insideGO == o.GetComponent<controllerInfo>().interactionTip ){
      moving = false;
    }

    if( secondInsideGO != null && insideGO == o.GetComponent<controllerInfo>().interactionTip ){
      GameObject tmp = insideGO;
      insideGO = secondInsideGO;
      secondInsideGO = tmp;
    }
    
    
  }


  void StayTrigger(GameObject o){
//    print("ff");
  }


  void onCollisionEnter(){
    print( "check" );
  }



  void OnTriggerEnter(Collider Other){

    if( Other.tag == "Hand" ){ 
  
      inside = true; 
      //print( Other.gameObject );

     //if( moving == false ){
     //  insideGO = Other.gameObject;
     //}else{
        

      if( moving == false ){
        if( insideGO == null ){
          insideGO = Other.gameObject;
        }
      }

      if( insideGO != Other.gameObject ){
        secondInsideGO = Other.gameObject;
      }
      //}
      //print( insideGO );
    }
  }

  void OnTriggerExit(Collider Other){
    if( Other.tag == "Hand" ){ 
      
      if( Other.gameObject == insideGO && secondInsideGO == null){
        inside = false;
        insideGO = null;
      }

      if( Other.gameObject == secondInsideGO ){
        secondInsideGO = null;
      }
    }
  }

}
