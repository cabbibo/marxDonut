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

  public bool beginningSS = false;
  public bool begunSS = false;
  public bool canBeginSS = false;
  public bool willBeginSS = false;
  public bool ssFinished = false;

  public bool beginningDead = false;
  public bool begunDead = false;

  

  public float secondVal = 0;
  public float deadVal = 0;

  public AudioClip audioClip;
  private AudioSource audio;


	// Use this for initialization
	void Start () {

    GetComponent<Renderer>().enabled = false;
    GetComponent<Collider>().enabled = false;

    audio = transform.gameObject.AddComponent<AudioSource>();
    audio.clip = audioClip;
    audio.spatialize = true;
    audio.loop = false;
    audio.volume = 1;
    audio.pitch = .8f + .2f * Random.Range(0,.999f);

    transform.localScale = new Vector3( targetScale.x , targetScale.y , transform.localScale.z );
    //audio.Play();
  	
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


    if( canBeginSS == true && begunSS == false && GetComponent<MoveByController>().moving == true ){
      willBeginSS = true;
    }

    if( willBeginSS == true && GetComponent<MoveByController>().moving == false ){
      BeginSS();
    }

    if( beginning == true ){
      Beginning();
    }

    if( beginningSS == true ){
      BeginningSS();
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

    GetComponent<LineRenderer>().SetPosition( 0 , new Vector3( 0 , 0 , (1 - beginVal) * .6f ));
    GetComponent<LineRenderer>().SetPosition( 2 , new Vector3( 0 , 0 , (1 - beginVal) * -.6f ));

    if( beginVal > 1 && hasBegun == false ){
      beginVal = 1;
      FinishBegin();
    }

  }

  public void FinishBegin(){
    beginning= false;
    hasBegun = true;
    //canBeginSS = true;
    GetComponent<Collider>().enabled = true;

  }

  public void BeginSS(){
    begunSS = true;
    beginningSS = true;
  }

  public void BeginningSS(){

    secondVal += .01f;

    GetComponent<Stretch>().leftDrag.GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);
    GetComponent<Stretch>().rightDrag.GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);

    GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);

    if( secondVal > 1 && ssFinished == false  ){
      secondVal = 1;
      ssFinished = true;
      FinishSS();
    }
  }

  public void FinishSS(){

//    print("YA");

    GetComponent<Stretch>().leftDrag.GetComponent<Renderer>().enabled = false;
    GetComponent<Stretch>().leftDrag.GetComponent<MoveByController>().enabled = false;
    GetComponent<Stretch>().leftDrag.GetComponent<Wrench>().enabled = false;
    GetComponent<Stretch>().leftDrag.GetComponent<HitAndHoldPlay>().enabled = false;

    GetComponent<Stretch>().rightDrag.GetComponent<Renderer>().enabled = false;
    GetComponent<Stretch>().rightDrag.GetComponent<MoveByController>().enabled = false;
    GetComponent<Stretch>().rightDrag.GetComponent<Wrench>().enabled = false;
    GetComponent<Stretch>().rightDrag.GetComponent<HitAndHoldPlay>().enabled = false;
    


  }

  public void Begin(){
    
    targetScale = new Vector3( targetScale.x , targetScale.y , GetComponent<Stretch>().length);
    beginMag = (targetScale - transform.localScale).magnitude;

    GetComponent<Stretch>().setPosition();
    //GetComponent<Renderer>().enabled = true;

    beginning = true;
    begun = true;
    transform.localScale = new Vector3(0,0,0);

    audio.Play();

  }

}
