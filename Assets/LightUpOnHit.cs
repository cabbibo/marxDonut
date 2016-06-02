using UnityEngine;
using System.Collections;

public class LightUpOnHit : MonoBehaviour {

  public GameObject Model;

  public controllerInfo ci;

	// Use this for initialization
	void Start () {

  
	
	}
	
	// Update is called once per frame
	void Update () {
    GetComponent<Renderer>().material.SetFloat("_TriggerDown" , ci.triggerVal );
    
    Model.GetComponent<Renderer>().material.SetFloat("_TriggerDown" , ci.triggerVal );
	
	}
 

}
