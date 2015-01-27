using UnityEngine;
using System.Collections;

public class SplineBuilder : MonoBehaviour {
	static public float splinenumber;
	// Use this for initialization
	void Start () {
		name = (splinenumber + "point");


		transform.parent = GameObject.Find("SplineRoot").transform;
		CoasterBuilder.lastpoint =transform.position;

		splinenumber=splinenumber + 0.00015f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
