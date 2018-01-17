using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointClass : MonoBehaviour {

	[SerializeField]
	protected float debugDrawRadius = 0.5F;
	
	// Use this for initialization
	public virtual void OnDrawGizmos () {
		if(this.tag == "Waypoint")
		{
			Gizmos.color = Color.red;
		}
		else if (this.tag == "Finish")
		{
			Gizmos.color = Color.blue;
		}
		
		Gizmos.DrawWireSphere(transform.position,debugDrawRadius);
	}
}
