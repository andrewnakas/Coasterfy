using UnityEngine;
using System.Collections;
/*
 * Coasterfy
 * This script takes project tango motion tracking data and procedually generates and rigs up a rollercoaster using a spline collider
 * Ideally this would automaticly scale with the size of motion being drawn
 * One would be able to easily draw and design a rollercoaster in a room or bring the tango device to a rollercoaster and map that
 * It would be sweet if there was a platform to easily map, export and upload the worlds rollercoasters to a database that would allow people to ride every rollercoaster....
* Also another simple place for this to go would be able to load adf files and build a coaster from that.
 */
public class CoasterBuilder : MonoBehaviour {
	public bool trackbuild; 
	public bool building; 
	public float splinetimer;
	public static float buildtimer; 
	public Transform tracker;
	public GameObject camera;
	public GameObject cart;
	public GameObject splineroot;
	public Transform splinepoint;
	public Vector3 lookPos;
	public static Vector3 lastpoint;
	Vector3 coasterstart;
	public float traveldistance; 
	public int rideswitchint;
	// Use this for initialization
	void Start () {
		trackbuild = true; 
		building = true;

	
	}
	
	// Update is called once per frame
	void Update () {

		Debug.Log (lookPos);
		if (  building == true){

			buildtimer+=Time.deltaTime;


		}

		
	

		//Track builder
		if (trackbuild == true){
			 coasterstart = transform.position;
		
			trackbuild = false;


		}
		traveldistance = ( Mathf.Abs(coasterstart.y) + Mathf.Abs(coasterstart.x ) + Mathf.Abs(coasterstart.z)) -( Mathf.Abs(transform.position.z) + Mathf.Abs(transform.position.y ) + Mathf.Abs(transform.position.x));

		lookPos =  transform.position-lastpoint ;
		if ( Mathf.Abs(traveldistance) > 1 && building == true){

			Instantiate(tracker,transform.position,Quaternion.LookRotation(lookPos)) ;

			trackbuild = true; 

		}



		// Spline builder

		splinetimer+=Time.deltaTime;

		if (splinetimer > 1f && building == true){
			//Quaternion.LookRotation

			Instantiate(splinepoint,transform.position, Quaternion.LookRotation(lookPos));
			//lastpoint = 
			splinetimer= 0;

		}

		//Debug.Log (( Mathf.Abs(coasterstart.y) + Mathf.Abs(coasterstart.x ) + Mathf.Abs(coasterstart.z)) -( Mathf.Abs(transform.position.z) + Mathf.Abs(transform.position.y ) + Mathf.Abs(transform.position.x)) );
		//Debug.Log (  Mathf.Abs(coasterstart.magnitude) - Mathf.Abs(transform.position.magnitude) );
	
	}
	//this is the ride switch that hooks up to the gui to turn on and off the coaster from riding back to tango.
	public void RideSwitch(){

		rideswitchint++;

			if (rideswitchint  %2==0)
		{
			Application.LoadLevel("Coasterfy");

		
		} else {
			building = false;
			cart.SetActive (true);
			camera.SetActive (false);

		}






	}
}
