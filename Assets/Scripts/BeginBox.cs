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

  public PillowFort PF;

  

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
  private Stretch s;



	// Use this for initialization
	void Start () {

    s = GetComponent<Stretch>();
    
    GetComponent<Renderer>().enabled = false;
    GetComponent<Collider>().enabled = false;

    s.leftDrag.GetComponent<Renderer>().enabled = false;
    s.rightDrag.GetComponent<Renderer>().enabled = false;

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

    s = GetComponent<Stretch>();

    s.leftDrag.GetComponent<Renderer>().material.SetFloat("_Cycle" , cycle);
    s.rightDrag.GetComponent<Renderer>().material.SetFloat("_Cycle" , cycle);

    if(  s.leftDrag.GetComponent<HoverAndRelease>().releaseEvent == true ){
      audioSource.clip = hitClipArray[1];
      //audioSource.pitch = .5f;
      holdSource.volume = 0;
      audioSource.Play();
    }

    if(  s.rightDrag.GetComponent<HoverAndRelease>().releaseEvent == true ){
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



    //makeSureInSpace(){}



    if( s.leftDrag.transform.position.x < PF.minMaxPlayArea.x &&
        s.rightDrag.transform.position.x < PF.minMaxPlayArea.x
      ){ 

      if(s.leftDrag.transform.position.x >= s.rightDrag.transform.position.x ){
        s.leftDrag.GetComponent<Rigidbody>().AddForce( Vector3.right * 1);
      }else{
        s.rightDrag.GetComponent<Rigidbody>().AddForce( Vector3.right * 1);
      }

     
    }

    if( s.leftDrag.transform.position.x > PF.minMaxPlayArea.y &&
        s.rightDrag.transform.position.x > PF.minMaxPlayArea.y
      ){ 

     

      if(s.leftDrag.transform.position.x <= s.rightDrag.transform.position.x ){
        s.leftDrag.GetComponent<Rigidbody>().AddForce( Vector3.left * 1);
      }else{
        s.rightDrag.GetComponent<Rigidbody>().AddForce( Vector3.left * 1);
      }
    }

    if( s.leftDrag.transform.position.z < PF.minMaxPlayArea.z &&
        s.rightDrag.transform.position.z < PF.minMaxPlayArea.z
      ){ 

     
      if(s.leftDrag.transform.position.z >= s.rightDrag.transform.position.z ){
        s.leftDrag.GetComponent<Rigidbody>().AddForce( Vector3.forward * 1);
      }else{
        s.rightDrag.GetComponent<Rigidbody>().AddForce( Vector3.forward * 1);
      }
    }

    if( s.leftDrag.transform.position.z > PF.minMaxPlayArea.w &&
        s.rightDrag.transform.position.z > PF.minMaxPlayArea.w
      ){ 

      if(s.leftDrag.transform.position.z <= s.rightDrag.transform.position.z ){
        s.leftDrag.GetComponent<Rigidbody>().AddForce( Vector3.forward * -1);
      }else{
        s.rightDrag.GetComponent<Rigidbody>().AddForce( Vector3.forward * -1);
      }
    }

    if( s.leftDrag.transform.position.y > 2.3f &&
        s.rightDrag.transform.position.y > 2.3f
      ){ 

      if(s.leftDrag.transform.position.y <= s.rightDrag.transform.position.y ){
        s.leftDrag.GetComponent<Rigidbody>().AddForce( Vector3.up * -1);
      }else{
        s.rightDrag.GetComponent<Rigidbody>().AddForce( Vector3.up * -1);
      }
    }

    if( s.leftDrag.transform.position.y < 0.2f &&
        s.rightDrag.transform.position.y < 0.2f
      ){ 

      if(s.leftDrag.transform.position.y >= s.rightDrag.transform.position.y ){
        s.leftDrag.GetComponent<Rigidbody>().AddForce( Vector3.up * 3);
      }else{
        s.rightDrag.GetComponent<Rigidbody>().AddForce( Vector3.up * 3);
      }
    }


    /*if( s.leftDrag.transform.position.x > PF.minMaxPlayArea.y ){ 
      s.leftDrag.GetComponent<Rigidbody>().AddForce( Vector3.left * .1f);
    }

    if( s.leftDrag.transform.position.z < PF.minMaxPlayArea.z ){ 
      s.leftDrag.GetComponent<Rigidbody>().AddForce( Vector3.forward * -.1f);
    }

    if( s.leftDrag.transform.position.z > PF.minMaxPlayArea.w ){ 
      s.leftDrag.GetComponent<Rigidbody>().AddForce( Vector3.forward * .1f);
    }*/




    /*

      Making sure we grow with proper drag

    */
    if( s.leftDrag.GetComponent<MoveByController>().moving == true ){
      firstMovement = 1;
      holdSource.volume = .3f;
    }

    if( s.rightDrag.GetComponent<MoveByController>().moving == true ){
      firstMovement = 2;
      holdSource.volume = .3f;
    }
    if( GetComponent<MoveByController>().moving == true ){
      holdSource.volume = .3f;
    }

    holdSource.pitch = (PF.cyclePitch) / s.length;

    if( begun == false && firstMovement == 2 && s.rightDrag.GetComponent<MoveByController>().moving == false ){
      Begin();
    }

    if( begun == false && firstMovement == 1 && s.leftDrag.GetComponent<MoveByController>().moving == false ){
      Begin();
    }


    if( GetComponent<MoveByController>().inside == true && GetComponent<Collider>().enabled == true ){
      SteamVR_TrackedObject tObj =  GetComponent<MoveByController>().insideGO.transform.parent.transform.gameObject.GetComponent<SteamVR_TrackedObject>();
      var device = SteamVR_Controller.Input((int)tObj.index);
      short size = (short)(100 * ( 1 + secondVal * 6 + beginVal));
      if( secondVal < .5 ){
          device.TriggerHapticPulse(300);
        }else{
          device.TriggerHapticPulse(600);
        }

      if(  GetComponent<MoveByController>().secondInsideGO != null){
        SteamVR_TrackedObject tObj2 =  GetComponent<MoveByController>().secondInsideGO.transform.parent.transform.gameObject.GetComponent<SteamVR_TrackedObject>();
        var device2 = SteamVR_Controller.Input((int)tObj2.index);

        if( secondVal < .5 ){
          device2.TriggerHapticPulse(300);
        }else{
          device2.TriggerHapticPulse(600);
        }

      }
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

    

    float basePitch = PF.cyclePitch;
    float octave = Mathf.Floor( Random.Range(0,.99f) * 2 ) + 1;
    audioSource.pitch = octave;
    audioSource.clip = enterClip;
    audioSource.Play();

    entering = true;
    targetScale = new Vector3( targetScale.x , targetScale.y , s.length);
    beginMag = (targetScale - transform.localScale).magnitude;

  
    s.leftDrag.GetComponent<Renderer>().enabled = true;
    s.rightDrag.GetComponent<Renderer>().enabled = true;
     s.leftDrag.GetComponent<Collider>().enabled = true;
    s.rightDrag.GetComponent<Collider>().enabled = true;


    s.leftDrag.GetComponent<Renderer>().material.SetFloat("_BeginVal", beginVal);
    s.rightDrag.GetComponent<Renderer>().material.SetFloat("_BeginVal", beginVal);
    s.setPosition();

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

      s.leftDrag.GetComponent<MoveByController>().enabled = true;
      s.leftDrag.GetComponent<Wrench>().enabled = true;
      s.leftDrag.GetComponent<HitAndHoldPlay>().enabled = true;

      s.rightDrag.GetComponent<MoveByController>().enabled = true;
      s.rightDrag.GetComponent<Wrench>().enabled = true;
      s.rightDrag.GetComponent<HitAndHoldPlay>().enabled = true;

          
    }

  }

  private void Beginning(){

    //print("ssss");

    transform.localScale = (targetScale - transform.localScale) * .04f + transform.localScale;
    Vector3 m = (targetScale - transform.localScale);
    beginVal += .02f;

    if( beginVal == .04f ){ 

      print("PALS");

      float basePitch = PF.cyclePitch;
      float octave = Mathf.Pow( 2, Mathf.Floor( s.length * 2 ));
      audioSource.pitch = octave;
      audioSource.clip = beginClip;
      audioSource.Play();

    }


    //GetComponent<Renderer>().material.setFloat("_BeginVal", beginVal);
    s.leftDrag.GetComponent<Renderer>().material.SetFloat("_BeginVal", beginVal);
    s.rightDrag.GetComponent<Renderer>().material.SetFloat("_BeginVal", beginVal);

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

    
  }

  public void BeginningSS(){

    secondVal += .01f;
    if( secondVal == .04f ){ 

      float basePitch = PF.cyclePitch;
    float octave = Mathf.Pow( 2, Mathf.Floor( s.length * 5 ));
    audioSource.pitch = octave;
    audioSource.clip = secondClip;
    audioSource.Play();

    }

    s.leftDrag.GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);
    s.rightDrag.GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);

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

        audioSource.pitch = PF.cyclePitch;
        audioSource.clip = popClip;
        audioSource.Play();

      }else{

      //  int whichHit = Random.Range(0,hitClipArray.Length);
        //print( whichHit );
        float basePitch = PF.cyclePitch;
        float octave = Mathf.Pow( 2, Mathf.Floor( s.length * 3 ));
        audioSource.pitch = basePitch * octave;
        audioSource.clip = hitClipArray[2];
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

    s.leftDrag.GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);
    s.rightDrag.GetComponent<Renderer>().material.SetFloat("_SecondVal", secondVal);

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

    s.leftDrag.GetComponent<Renderer>().enabled = false;
    s.leftDrag.GetComponent<Collider>().enabled = false;
    s.leftDrag.GetComponent<MoveByController>().enabled = false;
    s.leftDrag.GetComponent<Wrench>().enabled = false;
    s.leftDrag.GetComponent<HitAndHoldPlay>().enabled = false;

    s.rightDrag.GetComponent<Renderer>().enabled = false;
    s.rightDrag.GetComponent<Collider>().enabled = false;
    s.rightDrag.GetComponent<MoveByController>().enabled = false;
    s.rightDrag.GetComponent<Wrench>().enabled = false;
    s.rightDrag.GetComponent<HitAndHoldPlay>().enabled = false;


    GetComponent<LineRenderer>().enabled = false;
    
  }

  public void Begin(){
    
    targetScale = new Vector3( targetScale.x , targetScale.y , s.length);
    beginMag = (targetScale - transform.localScale).magnitude;

    s.setPosition();
    //GetComponent<Renderer>().enabled = true;

    beginning = true;
    begun = true;
    transform.localScale = new Vector3(0,0,0);

    //audioSource.clip = beginClip;
    //audioSource.Play();

  }

}
