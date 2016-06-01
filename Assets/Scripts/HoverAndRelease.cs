using UnityEngine;
using System.Collections;

public class HoverAndRelease : MonoBehaviour {

  public float hovered;
  public float release;
  public bool releaseEvent;

  public bool m = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
    releaseEvent = false;
    release -= .1f;
    if( release < 0 ){ release = 0; }

    if( GetComponent<MoveByController>().moving == true && m == false ){
      m = true;
    }else if( GetComponent<MoveByController>().moving == false && m == true ){
      m = false;
      release = 1;
      releaseEvent = true;
    }
	
	}

  void OnTriggerEnter(Collider c ){
    if( c.tag == "Hand"){ 
      hovered = 1;
      GetComponent<Renderer>().material.SetFloat( "_Hovered" , hovered );
    }
  }
  void OnTriggerExit(Collider c ){

    // make sure we don't accidentally release when moving
    if( c.tag == "Hand" && GetComponent<MoveByController>().moving == false ){ 
      hovered = 0;
      GetComponent<Renderer>().material.SetFloat( "_Hovered" , hovered );

    }
  }
}
