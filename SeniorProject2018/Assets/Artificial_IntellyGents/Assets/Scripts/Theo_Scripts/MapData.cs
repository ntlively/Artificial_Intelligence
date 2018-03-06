using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData : MonoBehaviour
{
	public List<NavPoint> navPoints;

	public MapData()
	{
		navPoints = new List<NavPoint>();
	}

	public void triangulate()
	{
		UnityEngine.AI.NavMeshTriangulation navMesh;
		navMesh = UnityEngine.AI.NavMesh.CalculateTriangulation();
		Mesh mesh = new Mesh();
		mesh.vertices = navMesh.vertices;
		mesh.triangles = navMesh.indices;
		//navPoints = new NavPoint[mesh.vertices.Length];

		//Debug.Log("number of vertices:" + mesh.vertices.Length);
		for(int i = 0; i < mesh.vertices.Length; i++)
		{
			navPoints.Add(new NavPoint(mesh.vertices[i]));
		}


		//Debug.Log("number of nav points:" + navPoints.Count);
		for(int material = 0; material < mesh.subMeshCount; material++)
		{
			int[] triangles = mesh.GetTriangles(material);
			for(int i = 0; i < triangles.Length;i+=3)
			{
				//1st gets 2nd and 3rd
				navPoints[triangles[i]].addNeighbor(triangles[i+1]);
				navPoints[triangles[i]].addNeighbor(triangles[i+2]);

				//2nd gets 1st and 3rd
				navPoints[triangles[i+1]].addNeighbor(triangles[i]);
				navPoints[triangles[i+1]].addNeighbor(triangles[i+2]);

				//3rd gets 1st and 2nd
				navPoints[triangles[i+2]].addNeighbor(triangles[i]);
				navPoints[triangles[i+2]].addNeighbor(triangles[i+1]);
			}
		}

		for(int i = 0; i < navPoints.Count; i++)
		{
			float sum = 0.0f;
			for(int j = 0; j < navPoints[i].neighbors.Count; j++)
			{
				sum = sum + Vector3.Distance(navPoints[i].position,navPoints[navPoints[i].neighbors[j]].position);
			}
			navPoints[i].setAvg((sum/navPoints[i].neighbors.Count));
		}
	}
}
