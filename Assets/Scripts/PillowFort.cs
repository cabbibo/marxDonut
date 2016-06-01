using UnityEngine;
using System.Collections;
using System;


/*


TODO:

//- each time bar is released for long time, plays own long note. 
//- when cloth is dropped, convert to pillows that can be dragged but not skewed
//- exploded pillows are the stars in the world. 
//- they explode at the end, making the cloth be able to fully drop
//- twist using x and y matching to scale x and y

// - Glitch when restarts of flash of fade?
//- Need the second version of the pillows to be much more fantastical / rewarding
- Cycle added to everything from nodes to pillows to cloth
//- Audio
//- release note when setting object down

- Something special on full / new moons!
  - ( cutout pattern on cloth?)
  - EVERYTHING BECOMES AUDIO REACTIVE!

//- limit jiggle val
//- subtle higher
//- move by controller
//- make it so that nodes stay visible and figure out neccesary scaling!!!
//- date +1 every loop

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
    public AudioClip clothDropClip;
    public AudioClip clothDisappearClip;

    public float cycle;

    public float lunarCycle;

    private AudioSource startSource;
    private AudioSource clothSource;
    private AudioSource lastSource;
    private AudioSource clipPlayer;

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

    public float special = 0;

    public float moonAge;

    // Use this for initialization
    void Start () {
      

   

      var dt = System.DateTime.Now;

      int m = dt.Month;
      int d = dt.Day;
      int y = dt.Year;
     //int d = System.DateTime.Now.getDay();
     //int y = System.DateTime.Now.getYear();
      //print( "MOON AGE");
      //print( m );
      //print( d );
      //print( y );

      moonAge = MoonAge( d , m , y );

      cycle = moonAge / 29;
      cycle = 1 - Mathf.Sin( cycle * Mathf.PI);
      print( moonAge );
      print( cycle );

      oTime = Time.time;

      fortCloth = GetComponent<FortCloth>();
      pillows = GetComponent<Pillows>();

      startSource = transform.gameObject.AddComponent<AudioSource>();
      startSource.clip = startLoop;
      startSource.spatialize = true;
      startSource.loop = true;
      startSource.volume = 0;
      startSource.Play();
    

      clothSource = transform.gameObject.AddComponent<AudioSource>();
      clothSource.clip = clothLoop;
      clothSource.spatialize = true;
      clothSource.loop = true;
      clothSource.volume = 0;
      clothSource.Play();
    

      lastSource = transform.gameObject.AddComponent<AudioSource>();
      lastSource.clip = lastLoop;
      lastSource.spatialize = true;
      lastSource.loop = true;
      lastSource.volume = 0;
      lastSource.Play();

      clipPlayer = transform.gameObject.AddComponent<AudioSource>();
      clipPlayer.spatialize = false;
      clipPlayer.loop = false;
      
      clipPlayer.volume = 1;

  
  
      setCycle();


    }

    void setCycle( ){

      //cycle = 1;///Mathf.Abs( Mathf.Sin( Time.time * .003f ));

      if( cycle < 0.05 || cycle > .95 ){
        special = 1;
      }else{
        special = 0;
      }

      lastSource.pitch = .5f + cycle * .5f;
      clipPlayer.pitch = .5f + cycle * .5f;
      clothSource.pitch = .5f + cycle * .5f;
      startSource.pitch = .5f + cycle * .5f ;
      
      lune.GetComponent<Lune>().cycle = cycle;
      lune.GetComponent<Lune>().moonAge = moonAge;
      floor.GetComponent<Renderer>().material.SetFloat("_Cycle",cycle);
      
  
      floor.GetComponent<Renderer>().material.SetFloat( "_Cycle" , cycle );
      floor.GetComponent<Renderer>().material.SetFloat( "_Cycle" , cycle );

      pillows.setCycle();
      fortCloth.setCycle();

    }

    void dropCloth(){

      clothDropped = true;
      oTime = Time.time;
      started = 1;


      clipPlayer.clip = clothDropClip;
      clipPlayer.Play();

      fortCloth.dropCloth();
      pillows.dropCloth();
      
    }



    void FixedUpdate(){

      pillows.update();
      fortCloth.update();

      if( pillows.allBlocksStarted == true && clothDropped == false ){ dropCloth(); }
      //if( pillows.allPillowsPopped == true && ended == false ){ beginEnd(); }

      

      fadeIn += .001f;
      //fadeIn += .1f;
      //print( fadeIn );
      if( fadeIn > 1 ){ 
        fadeIn = 1; 
        fadedIn = true;
      }

      if( clothDropped == true ){
        clothDown += .0006f;
        //clothDown += .1f;



        if( clothDown > 1 ){ 


          
          clothDown = 1; 
          if( fullDropped == false ){
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
      startSource.volume = Mathf.Min( fadeIn , 1 - clothDown);

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
        disappearVal += .002f;
        if( disappearVal > 1 ){ disappearVal = 1;}


        clothSource.volume = 1 - disappearVal;
        lastSource.volume = disappearVal;

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

      if( fadedIn == false ){
        disappearVal = 1 - fadeIn;
        fullEnd = 1 - fadeIn;
      }

      lune.GetComponent<Lune>().fadeIn = 1-disappearVal;
      //moon.GetComponent<Renderer>().material.SetFloat( "_EndingVal" , endingVal);
      //title.GetComponent<Renderer>().material.SetFloat( "_EndingVal" , endingVal);
    
    }

    // Using  http://www.codeproject.com/Articles/100174/Calculate-and-Draw-Moon-Phase
    // And https://en.wikipedia.org/wiki/Lunar_phase
    private int julian(int day, int month, int year)
    {
      int newMonth;
      int newYear; 
      int k1, k2, k3; 
      int julianDate;
      
      newMonth = month + 9;
      newYear = year - (int)((12 - month) / 10);
      
      if (newMonth >= 12){ newMonth = newMonth - 12; }

      k1 = (int)(365.25 * (newYear + 4712));
      k2 = (int)(30.6001 * newMonth + 0.5);
      k3 = (int)((int)((newYear / 100) + 49) * 0.75) - 38;
      
      julianDate = k1 + k2 + day + 59;
      
      if (julianDate > 2299160){
        julianDate = julianDate - k3;
      }

      return julianDate;
    }

    private float MoonAge(int d, int m, int y)
    {      

      float finalAge = 0;
      float moonCycle = 0;
      int j = julian(d, m, y);
      moonCycle  = ((float)j + 4.867f) / 29.53059f;
      moonCycle  = moonCycle  - Mathf.Floor(moonCycle);
      if(moonCycle < 0.5f){
        finalAge = moonCycle * 29.53059f + 29.53059f / 2;
      }else{
        finalAge = moonCycle * 29.53059f - 29.53059f / 2;
      }
      // Moon's finalAge in days
      finalAge = Mathf.Floor(finalAge) + 1;
      return finalAge;
    }

    void Update(){
      setCycle();
      lune.GetComponent<Lune>().clothDown = clothDown;
      lune.GetComponent<Lune>().endingVal = endingVal * endingVal;
      lune.GetComponent<Lune>().fullEnd = fullEnd * fullEnd * fullEnd * fullEnd;
      floor.GetComponent<Renderer>().material.SetFloat("_ClothDown",clothDown);
      floor.GetComponent<Renderer>().material.SetFloat("_Disappear",disappearVal);
    }

    private void releaseCloth(){
     
      ending = 1;
      fortCloth.forcePass.SetInt( "_Ended"   , 1 );

      lune.GetComponent<Lune>().moon.GetComponent<Renderer>().enabled = true;
      lune.GetComponent<Lune>().title.GetComponent<Renderer>().enabled = true;

      clipPlayer.clip = clothDisappearClip;
      clipPlayer.Play();
      pillows.onClothDisappear();
      
    }

    void Restart(){

      started = -1;
      ending = -1;

      fadedIn = false;
      oTime = Time.time;;

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


      moonAge ++; //= MoonAge( d , m , y );

      cycle = moonAge / 29;
      if( cycle > 1 ){ cycle -= 1; }
      cycle = 1 - Mathf.Sin( cycle * Mathf.PI);

      print( cycle );
      setCycle();


      pillows.Restart();
      fortCloth.Restart();


    }


    private void triggerFinalEnd(){

      finalEndTriggered = true;

    }



    
 
 
    


    

    
    

    
    

    



 

    
}