using UnityEngine;
using System.Collections;

public class BeginBox : MonoBehaviour {

  public Vector3 targetScale;

  public float cycle;
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
  public bool canBeginPop = false;
  public bool ssFinished = false;

  public bool popping = false;
  public bool popped  = false;

  public bool beginningDead = false;
  public bool begunDead = false;

  public bool entered = false;
  public bool entering = false;
  public float enteredVal = 0;

  

  public float secondVal = 0;
  public float deadVal = 0;
  public float hovered = 0;
  public float jiggleVal = 0;

  public AudioClip enterClip;
  public AudioClip beginClip;
  public AudioClip secondClip;
  public AudioClip popClip;
  public AudioClip[] hitClipArray;
  public AudioClip holdClip;

  public AudioSource audioSource;
  public AudioSource holdSource;




	// Use this for initialization
	void Start () {

    GetComponent<Renderer>().enabled = false;
    GetComponent<Collider>().enabled = false;

    GetComponent<Stretch>().leftDrag.GetComponent<Renderer>().enabled = false;
    GetComponent<Stretch>().rightDrag.GetComponent<Renderer>().enabled = false;

    GetComponent<LineRenderer>().SetPosition( 0 , new Vector3( 0 , 0 , 0 ));
    GetComponent<LineRenderer>().SetPosition( 2 , new Vector3( 0 , 0 , 0 ));
    GetComponent<LineRenderer>().enabled = false;

    audioSource = transform.gameObject.AddComponent<AudioSource>();
    audioSource.spatialize = true;
    audioSource.loop = false;
    audioSource.volume = 1;
    //audio.pitch = .8f + .2f * Random.Range(0,.999f);

    transform.localScale = new Vector3( targetScale.x , targetScale.y , transform.localScale.z );
    //audio.Play();

    holdSource = transform.gameObject.AddComponent<AudioSource>();
    holdSource.clip = holdClip;
    holdSource.spatialize = true;
    holdSource.loop = true;
    holdSource.pitch = 2;
    holdSource.volume = 0;
    holdSource.Play();
  	
	}
	
	// Update is called once per frame
	void Update () {

    GetComponent<Stretch>().leftDrag.GetComponent<Renderer>().material.SetFloat("_Cycle" , cycle);
    GetComponent<Stretch>().rightDrag.GetComponent<Renderer>().material.SetFloat("_Cycle" , cycle);

    if(  GetComponent<Stretch>().leftDrag.GetComponent<HoverAndRelease>().releaseEvent == true ){
      audioSource.clip = hitClipArray[1];
      //audioSource.pitch = .5f;
      holdSource.volume = 0;
      audioSource.Play();
    }

    if(  GetComponent<Stretch>().rightDrag.GetComponent<HoverAndRelease>().releaseEvent == true ){
      audioSource.clip = hitClipArray[1];
      //audioSource.pitch = .5f;
      audioSource.Play();
      holdSource.volume = 0;
    }

    if(  GetComponent<HoverAndRelease>().releaseEvent == true ){
      jiggleVal += 2;
      audioSource.clip = hitClipArray[1];
      //audioSource.pitch = .5f;
      audioSource.Play();
      holdSource.volume = 0;
    }





    /*

      Making sure we grow with proper drag

    */
    if( GetComponent<Stretch>().leftDrag.GetComponent<MoveByController>().moving == true ){
      firstMovement = 1;
      holdSource.volume = .3f;
    }

    if( GetComponent<Stretch>().rightDrag.GetComponent<MoveByController>().moving == true ){
      firstMovement = 2;
      holdSource.volume = .3f;
    }
    if( GetComponent<MoveByController>().moving == true ){
      holdSource.volume = .3f;
    }

    holdSource.pitch = (.5f + cycle * .5f) / GetComponent<Stretch>().length;

    if( begun == false && firstMovement == 2 && GetComponent<Stretch>().rightDrag.GetComponent<MoveByController>().moving == false ){
      Begin();
    }

    if( begun == false && firstMovement == 1 && GetComponent<Stretch>().leftDrag.GetComponent<MoveByController>().moving == false ){
      Begin();
    }


    /*if( GetComponent<MoveByController>().moving == true ){
      moving = true;
    }

    if( GetComponent<MoveByController>().moving == true ){
      moving = true;
    }*/


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

    if( popping == true ){
      Popping();
    }

    if( entering == true ){
      Entering();
    }



    jiggleVal -= .04f;
    jiggleVal = Mathf.Clamp( jiggleVal , 0 , 1);
	
	}

  public void BeginEnter(){

    audioSource.pitch = .5f + cycle * .5f;
    audioSource.clip = enterClip;
    audioSource.Play();

    entering = true;
    targetScale = new Vector3( targetScale.x , targetScale.y , GetComponent<Stretch>().length);
    beginMag = (targetScale - transform.localScale).magnitude;

  
    GetComponent<Stretch>().leftDrag.GetComponent<Renderer>().enabled = true;
    GetComponent<Stretch>().rightDrag.GetComponent<Renderer>().enabled = true;
     GetComponent<Stretch>().leftDrag.GetComponent<Collider>().enabled = true;
    GetComponent<Stretch>().rightDrag.GetComponent<Collider>().enabled = true;


    GetComponent<Stretch>().leftDrag.GetComponent<Renderer>().material.SetFloat("_BeginVal", beginVal);
    GetComponent<Stretch>().rightDrag.GetComponent<Renderer>().material.SetFloat("_BeginVal", beginVal);
    GetComponent<Stretch>().setPosition();

    GetComponent<LineRenderer>().enabled = true;
   

  }

  public void Entering(){

    enteredVal += .02f;

    GetComponent<LineRenderer>().SetPosition( 0 , new Vector3( 0 , 0 , (enteredVal) * .6f ));
    GetComponent<LineRenderer>().SetPosition( 2 , new Vector3( 0 , 0 , (enteredVal) * -.6f ));

    float l = .01f * enteredVal;
     GetComponent<LineRenderer>().SetWidth(l, l);

    if( enteredVal > 1 && entered == false ){

      enteredVal = 1;
      entered = true;
      entering = false;

      GetComponent<Stretch>().leftDrag.GetComponent<MoveByController>().enabled = true;
      GetComponent<Stretch>().leftDrag.GetComponent<Wrench>().enabled = true;
      GetComponent<Stretch>().leftDrag.GetComponent<HitAndHoldPlay>().enabled = true;

      GetComponent<Stretch>().rightDrag.GetComponent<MoveByController>().enabled = true;
      GetComponent<Stretch>().rightDrag.GetComponent<Wrench>().enabled = true;
      GetComponent<Stretch>().rightDrag.GetComponent<HitAndHoldPlay>().enabled = true;

          
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

     float l = .01f * (1 - beginVal);
     GetComponent<LineRenderer>().SetWidth(l, l);


    if( beginVal > 1 && hasBegun == false ){
      beginVal = 1;
      FinishBegin();
    }

  }

  public void FinishBegin(){
    beginning= false;
    hasBegun = true;
    GetComponent<LineRenderer>().enabled = false;

    //canBeginSS = true;
    GetComponent<Collider>().enabled = true;

  }

  public void BeginSS(){
    begunSS = true;
    beginningSS = true;
    willBeginSS = false;
    audioSource.pitch = .5f + cycle * .5f;
    audioSource.clip = secondClip;
    audioSource.Play();
  }

  public void BeginningSS(){

    secondVal += .01f;

    GetComponent<Stretch>().leftDrag.GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);
    GetComponent<Stretch>().rightDrag.GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);

    GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);

    if( secondVal > 1 && ssFinished == false  ){
      secondVal = 1;
      ssFinished = true;
      beginningSS = false;
      FinishSS();
    }
  }

  void OnTriggerEnter(Collider Other){

    if( Other.tag == "Hand"){ 

      hovered = 1;
      jiggleVal = 1;

      //print("TRIGGER WORKING");


      if( canBeginPop == true && popping == false ){
        popping = true;
        secondVal = 0;
        GetComponent<Collider>().enabled = false;

        audioSource.pitch = .5f + cycle * .5f;
        audioSource.clip = popClip;
        audioSource.Play();

      }else{

      //  int whichHit = Random.Range(0,hitClipArray.Length);
        //print( whichHit );
        audioSource.pitch = .5f + cycle * .5f;
        audioSource.clip = hitClipArray[0];
        audioSource.Play();

      }
    }

  }

  void OnTriggerExit(){
    hovered = 0;
  }

  public void Restart(){

    beginning = false;
    begun = false;
    hasBegun = false;

    firstMovement = 0;
    beginVal = 0;

    beginningSS = false;
    begunSS = false;
    canBeginSS = false;
    willBeginSS = false;
    canBeginPop = false;
    ssFinished = false;

    popping = false;
    popped  = false;

    beginningDead = false;
    begunDead = false;

    secondVal = 0;
    deadVal = 0;

    entered = false;
    entering = false;
    enteredVal = 0;

    GetComponent<Stretch>().leftDrag.GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);
    GetComponent<Stretch>().rightDrag.GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);

    GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);
    GetComponent<Collider>().enabled = false;
    GetComponent<LineRenderer>().enabled = false;




  }


  public void Popping(){

    if( beginVal < 0 && popped == false  ){
      beginVal = 0;
      popped = true;
    }

    if( popped == false ){
      beginVal -= .01f;
      //secondVal -= .01f;
    }

    GetComponent<Renderer>().material.SetFloat("_BeginVal", beginVal);

   
  }

  public void FinishSS(){

    GetComponent<Stretch>().leftDrag.GetComponent<Renderer>().enabled = false;
    GetComponent<Stretch>().leftDrag.GetComponent<Collider>().enabled = false;
    GetComponent<Stretch>().leftDrag.GetComponent<MoveByController>().enabled = false;
    GetComponent<Stretch>().leftDrag.GetComponent<Wrench>().enabled = false;
    GetComponent<Stretch>().leftDrag.GetComponent<HitAndHoldPlay>().enabled = false;

    GetComponent<Stretch>().rightDrag.GetComponent<Renderer>().enabled = false;
    GetComponent<Stretch>().rightDrag.GetComponent<Collider>().enabled = false;
    GetComponent<Stretch>().rightDrag.GetComponent<MoveByController>().enabled = false;
    GetComponent<Stretch>().rightDrag.GetComponent<Wrench>().enabled = false;
    GetComponent<Stretch>().rightDrag.GetComponent<HitAndHoldPlay>().enabled = false;


    GetComponent<LineRenderer>().enabled = false;
    
  }

  public void Begin(){
    
    targetScale = new Vector3( targetScale.x , targetScale.y , GetComponent<Stretch>().length);
    beginMag = (targetScale - transform.localScale).magnitude;

    GetComponent<Stretch>().setPosition();
    //GetComponent<Renderer>().enabled = true;

    beginning = true;
    begun = true;
    transform.localScale = new Vector3(0,0,0);

    audioSource.clip = beginClip;
    audioSource.Play();

  }

}
