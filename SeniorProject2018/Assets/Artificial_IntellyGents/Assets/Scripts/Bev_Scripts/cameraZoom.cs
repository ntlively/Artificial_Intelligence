using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraZoom : MonoBehaviour {



 	public float dragSpeed = 2;
    private Vector3 dragOrigin;
	Vector3 move;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		//If it is the mouse button set drag point
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) )
		{
			dragOrigin = Input.mousePosition;
			return;
		}
	
		//If the input is not a mouse button 0
		if (!Input.GetMouseButton(0) && !Input.GetMouseButtonDown(1)) return;
	
		//If it is a mouse button 0, move the camera
		Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);

		if(Input.GetMouseButton(0))
		{
			Debug.Log ("Rotate Up");
			move = new Vector3(pos.y * dragSpeed, pos.x * dragSpeed , 0);
		}
		else if (Input.GetMouseButton(1))
		{
			Debug.Log ("Rotate Right");
			move = new Vector3(0 , pos.y * dragSpeed, 0 );
		}
		
		transform.Rotate(move); 

		

    
		
	}
}
