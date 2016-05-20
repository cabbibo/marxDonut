using UnityEngine;
using System.Collections;

public class BeginBox : MonoBehaviour {

  public Vector3 targetScale;


  public bool beginning = false;
  public bool begun = false;
  public bool hasBegun = false;

  public int firstMovement = 0;
  public float beginMag;
  public float beginVal = 0;


	// Use this for initialization
	void Start () {

    GetComponent<Renderer>().enabled = false;
    GetComponent<Collider>().enabled = false;

	
	}
	
	// Update is called once per frame
	void Update () {

    if( GetComponent<Stretch>().leftDrag.GetComponent<MoveByController>().moving == true ){
      firstMovement = 1;
    }
    if( GetComponent<Stretch>().rightDrag.GetComponent<MoveByController>().moving == true ){
      firstMovement = 2;
    }

    if( begun == false && firstMovement == 2 && GetComponent<Stretch>().rightDrag.GetComponent<MoveByController>().moving == false ){
      Begin();
    }

    if( begun == false && firstMovement == 1 && GetComponent<Stretch>().leftDrag.GetComponent<MoveByController>().moving == false ){
      Begin();
    }

    if( beginning == true ){
      Beginning();
    }
	
	}

  private void Beginning(){

    //print("ssss");

    transform.localScale = (targetScale - transform.localScale) * .04f + transform.localScale;
    Vector3 m = (targetScale - transform.localScale);
    beginVal += .02f;

    //GetComponent<Renderer>().material.setFloat("_BeginVal", beginVal);
    GetComponent<Stretch>().leftDrag.GetComponent<Renderer>().material.SetFloat("_BeginVal", beginVal);
    GetComponent<Stretch>().rightDrag.GetComponent<Renderer>().material.SetFloat("_BeginVal", beginVal);


    if( beginVal > 1 && hasBegun == false ){
      beginVal = 1;
      FinishBegin();
    }

  }

  public void FinishBegin(){
    beginning= false;
    hasBegun = true;
    GetComponent<Collider>().enabled = true;

  }

  public void Begin(){
    
    targetScale = new Vector3( targetScale.x , targetScale.y , GetComponent<Stretch>().length);
    beginMag = (targetScale - transform.localScale).magnitude;

    GetComponent<Stretch>().setPosition();
    GetComponent<Renderer>().enabled = true;

    beginning = true;
    begun = true;
    transform.localScale = new Vector3(0,0,0);

  }

}
