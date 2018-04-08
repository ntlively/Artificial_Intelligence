using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour {


 	public float dragSpeed = 2;
    private Vector3 dragOrigin;
	Vector3 move;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 6.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 6.0f;

        this.transform.Translate(x, 0, 0);
        this.transform.Translate(0, 0, z);


		if (Input.GetKey(KeyCode.Q))
		{
			Debug.Log("Going up");
			transform.Translate(new Vector3(0, 6.0f*Time.deltaTime ,0));
		}
		else if (Input.GetKey(KeyCode.E))
		{
			Debug.Log("Going down");
			transform.Translate(new Vector3(0, -6.0f*Time.deltaTime ,0));
		}


		if (Input.GetMouseButtonDown(0))
		{
			dragOrigin = Input.mousePosition;
			return;
		}
	
		//If the input is not a mouse button 0
		if (!Input.GetMouseButton(0)) return;
	
		//If it is a mouse button 0, move the camera
		Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);

		Debug.Log ("Rotate Up");
		move = new Vector3(pos.y * dragSpeed, pos.x * dragSpeed , 0);
				
		transform.Rotate(move); 
     
	}
}
