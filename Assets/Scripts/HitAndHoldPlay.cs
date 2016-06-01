using UnityEngine;
using System.Collections;

public class HitAndHoldPlay : MonoBehaviour {

  public AudioClip hitClip;
  public AudioClip holdClip;

  private AudioSource hitSource;
  private AudioSource holdSource;

	// Use this for initialization
	void Start () {

    holdSource = transform.gameObject.AddComponent<AudioSource>();
    holdSource.clip = holdClip;
    holdSource.spatialize = true;
    holdSource.loop = true;
    holdSource.pitch = 2;
    holdSource.volume = 0;
    holdSource.Play();
	

    hitSource = transform.gameObject.AddComponent<AudioSource>();
    hitSource.clip = hitClip;
    hitSource.spatialize = true;
    hitSource.loop = false;
    	}
	
	// Update is called once per frame
	void Update () {
    if( GetComponent<MoveByController>().moving == true ){
      //holdSource.volume = 1;
    }else{
      holdSource.volume = 0;
    }
	
	}

  void OnTriggerEnter(Collider c ){


    if( c.tag == "Hand"){ 
      hitSource.Play();
    }
  }
}
