using UnityEngine;
using System.Collections;

public class Lune : MonoBehaviour {

  public float clothDown;
  public float endingVal;
  public float fullEnd; 
  public float cycle;

  //public texture2D


  public GameObject moon;
  public GameObject title;

	// Use this for initialization
	void Start () {

    moon = transform.Find("Moon").gameObject; //Instantiate( Moon );
    title = transform.Find("Title").gameObject;

    
	
	}
	
	// Update is called once per frame
	void Update () {

//    print( cycle );
    moon.GetComponent<Renderer>().material.SetFloat( "_Cycle" , cycle);
    title.GetComponent<Renderer>().material.SetFloat( "_Cycle" , cycle);

    moon.GetComponent<Renderer>().material.SetFloat( "_ClothDown" , clothDown);
    title.GetComponent<Renderer>().material.SetFloat( "_ClothDown" , clothDown);

    moon.GetComponent<Renderer>().material.SetFloat( "_FullEnd" , fullEnd);
    title.GetComponent<Renderer>().material.SetFloat( "_FullEnd" , fullEnd);

    moon.GetComponent<Renderer>().material.SetFloat( "_EndingVal" , endingVal);
    title.GetComponent<Renderer>().material.SetFloat( "_EndingVal" , endingVal);

    moon.transform.rotation = Quaternion.Euler(0 , 180 * cycle - 90, 0); //new Quaternion( 0 , 1 , 0  , 90 * cycle);

	
	}
}
