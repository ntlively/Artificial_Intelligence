using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavPoint : MonoBehaviour
{
	public Vector3 position;
	public List<int> neighbors;
	public float distAvg;
	public int enemySpotted;	//Opposite type has been spotted here
	public int friendSpotted;	//Same type and self has been spotten here

	public NavPoint(Vector3 _position)
	{
		neighbors = new List<int>();
		position = _position;
		distAvg = 0;
		enemySpotted = 0;
		friendSpotted = 0;
	}

	public void addNeighbor(int index)
	{
		bool found = false;
		for(int i = 0; i < neighbors.Count; i++)
		{
			if(neighbors[i] == index)
			{
				found = true;
				break;
			}
		}
		if(!found)
			neighbors.Add(index);
	}

	public void setAvg(float _average)
	{
		distAvg = _average;
	}
}
