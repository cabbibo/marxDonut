using UnityEngine;
using System.Collections;


/*


TODO:

//- each time bar is released for long time, plays own long note. 
//- when cloth is dropped, convert to pillows that can be dragged but not skewed
//- exploded pillows are the stars in the world. 
//- they explode at the end, making the cloth be able to fully drop
//- twist using x and y matching to scale x and y


*/

public class PillowFort : MonoBehaviour {

    public GameObject lune;
    public GameObject floor;

    public Pillows pillows;
    public FortCloth fortCloth;


    public GameObject handBufferInfo; 

    public AudioClip startLoop;
    public AudioClip clothLoop;
    public AudioClip lastLoop;

    public float cycle;

    private AudioSource startSource;
    private AudioSource clothSource;
    private AudioSource lastSource;

    private Vector3 p1;
    private Vector3 p2;

    public float oTime = 0;

    public int started = -1;
    public int ending = -1;

    public bool fadedIn = false;

    public bool clothDropped = false;
    public bool allBlocksStarted = false;
    public bool allPillowsPopped = false;
    public bool fullDropped = false;


    public bool finalEndTriggered = false;

    public float clothDown = 0;
    public int framesSinceDrop = 0;
    public float endingVal = 0;
    public float disappearVal = 0;
    public float fadeIn = 0;

    public float fullEnd = 0;

    // Use this for initialization
    void Start () {

      oTime = Time.time;

      fortCloth = GetComponent<FortCloth>();
      pillows = GetComponent<Pillows>();

      print("AS");
      print( pillows );

      startSource = transform.gameObject.AddComponent<AudioSource>();
      startSource.clip = startLoop;
      startSource.spatialize = true;
      startSource.loop = true;
      startSource.pitch = .1f + cycle * .9f ;
      startSource.volume = 0;
      startSource.Play();
    

      clothSource = transform.gameObject.AddComponent<AudioSource>();
      clothSource.clip = clothLoop;
      clothSource.spatialize = true;
      clothSource.loop = true;
      clothSource.pitch = .25f + cycle * .75f;
      clothSource.volume = 0;
      clothSource.Play();
    

      lastSource = transform.gameObject.AddComponent<AudioSource>();
      lastSource.clip = lastLoop;
      lastSource.spatialize = true;
      lastSource.loop = true;
      lastSource.pitch = .5f + cycle * .5f;
      lastSource.volume = 0;
      lastSource.Play();
  
      setCycle();


    }

    void setCycle( ){
      //float cycle = Mathf.Abs( Mathf.Sin( Time.time ));
      
      lune.GetComponent<Lune>().cycle = cycle;
      floor.GetComponent<Renderer>().material.SetFloat("_Cycle",cycle);
      pillows.setCycle();
      fortCloth.setCycle();

    }

    void dropCloth(){

      clothDropped = true;
      oTime = Time.time;
      started = 1;

      fortCloth.dropCloth();
      pillows.dropCloth();
      
    }



    void FixedUpdate(){

      pillows.update();
      fortCloth.update();

      if( pillows.allBlocksStarted == true && clothDropped == false ){ dropCloth(); }
      //if( pillows.allPillowsPopped == true && ended == false ){ beginEnd(); }

      if( fadedIn == false ){
        disappearVal = 1 - fadeIn;
        fullEnd = 1 - fadeIn;
      }

      fadeIn += .01f;
      //print( fadeIn );
      if( fadeIn > 1 ){ 
        fadeIn = 1; 
        fadedIn = true;
      }

      if( clothDropped == true ){
        clothDown += .0006f;
        if( clothDown > 1 ){ 


          clothDown = 1; 
          if( fullDropped == false ){
            print( "YAAA");
            pillows.fullClothDropped();
            fullDropped = true;
            lune.GetComponent<Lune>().moon.GetComponent<Renderer>().enabled = false;
            lune.GetComponent<Lune>().title.GetComponent<Renderer>().enabled = false;
          }
        }
        framesSinceDrop ++;

        
      }
      
      if( pillows.allBlocksSS == true && ending < 0 ){
        releaseCloth();
      }

      clothSource.volume = clothDown;
      startSource.volume = 1 - clothDown;

      if( ending > 0 ){

        endingVal += .001f;

        // tweens up to new value when another pillow popped!
        float currentVal = (float)pillows.pillowsPopped / (float)pillows.NumShapes;
        if( endingVal > currentVal){ endingVal = currentVal; }


        if( pillows.allPillowsPopped && finalEndTriggered == false ){ 
          triggerFinalEnd();
          endingVal = 1;
        }

        // used to make floor disappear
        disappearVal += .001f;
        if( disappearVal > 1 ){ disappearVal = 1;}


        clothSource.volume = 1 - endingVal;
        lastSource.volume = endingVal;

      }

      if( finalEndTriggered == true ){
        fullEnd += .001f;
        lastSource.volume = 1 - fullEnd;
        
        if( fullEnd > 1 ){ 
          fullEnd = 1;
          Restart();
        }

        
      } 

      oTime = Time.time;
    
    }

    void Update(){
      setCycle();
      lune.GetComponent<Lune>().clothDown = clothDown;
      lune.GetComponent<Lune>().endingVal = endingVal;
      lune.GetComponent<Lune>().fullEnd = fullEnd;
      floor.GetComponent<Renderer>().material.SetFloat("_ClothDown",clothDown);
      floor.GetComponent<Renderer>().material.SetFloat("_Disappear",disappearVal);
    }

    private void releaseCloth(){
     
      ending = 1;
      fortCloth.forcePass.SetInt( "_Ended"   , 1 );

      lune.GetComponent<Lune>().moon.GetComponent<Renderer>().enabled = true;
      lune.GetComponent<Lune>().title.GetComponent<Renderer>().enabled = true;

      pillows.onClothDisappear();
      
      
    }

    void Restart(){

      started = -1;
      ending = -1;

      fadedIn = false;

      clothDropped = false;
      allBlocksStarted = false;
      allPillowsPopped = false;
      fullDropped = false;


      finalEndTriggered = false;

      clothDown = 0;
      framesSinceDrop = 0;
      endingVal = 0;
      disappearVal = 0;
      fadeIn = 0;



      fullEnd = 0;

      pillows.Restart();
      fortCloth.Restart();

    }


    private void triggerFinalEnd(){

      finalEndTriggered = true;

    }



    
 
 
    


    

    
    

    
    

    



 

    
}