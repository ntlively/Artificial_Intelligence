using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DisplayKalmanGrid : MonoBehaviour
{
    //Create UI Accessible Developement Variables
    [Header("Grid Display")]
    [Range(0.0f, 1.0f)]
    public float gridOpacity = 0.5f;
    public bool displayTimesCaught = true;
    public bool displayVisited = true;
    public bool displayProbabilityDistribution = true;
    public bool displayPredictedPosition = true;

    //Regular Variables, not exposed to UI
    public List<NavCheckPoint> gridList = new List<NavCheckPoint>();
    private LayerMask obstacleMask;

    //Awake
    void Awake()
    {
        obstacleMask = 1 << 10;

        //Map of the ground floor
        Vector3 fillPoints = new Vector3(-21.0f, 0.0f, 21.0f);
        int row = 0;

        //Start from the left portion of the map and iterate through
        //creating weighted points that are on the navmesh.
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 1600; j++)
            {
                fillPoints[0] = fillPoints[0] + 1.0f;
                row++;

                NavMeshHit hit;
                //check if point is on navmesh
                NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();

                Mesh mesh = new Mesh();
                mesh.name = "ExportedNavMesh";
                mesh.vertices = triangulatedNavMesh.vertices;
                mesh.triangles = triangulatedNavMesh.indices;


                if (NavMesh.SamplePosition(fillPoints, out hit, 0.71f, NavMesh.AllAreas))
                {
                    //create object
                    NavCheckPoint temp = new NavCheckPoint(fillPoints, hit.position);
                    temp.navPosition[1] += 0.5f;
                    //add to list
                    gridList.Add(temp);
                }

                if (row == 40)
                {
                    fillPoints[0] = -21.0f;
                    fillPoints[2] = fillPoints[2] - 1.0f;
                    row = 0;

                }
            }

            fillPoints[0] = -21.0f;
            fillPoints[1] = fillPoints[1] + 1.0f;
            fillPoints[2] = 21.0f;

        }

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void OnDrawGizmosSelected()
    {

        Vector3 fillPoints = new Vector3(-21.0f, 1.0f, 20.0f);
        Vector3 reachPoint = new Vector3(0.0f, 0.0f, 0.0f);
        int row = 0;

        //Display entire influence map using cubes
        for (int k = 0; k < gridList.Count; k++)
        {
            //If the point is on the second level display as blue
            // if (gridList[k].position[1] == 4.5f)
            // {
            // 	Gizmos.color = Color.blue;
            // 	//Adjust cubes so that the top right corner of the cube is the center of the cube.	
            // }
            // else
            // {
            // 	//Lower level of influence map.
            // 	Gizmos.color = Color.green;
            // }

            //Opacity value is visit time ratio
            float opacityValue;
            if (displayVisited)
                opacityValue = 1.0f;
            else
                opacityValue = 0.0f;

            //Full color
            Gizmos.color = new Color(0.5f, 0.0f, 0.5f, gridOpacity * opacityValue);

            fillPoints = gridList[k].position;
            fillPoints[1] += 0.05f;

            Gizmos.DrawCube(fillPoints, new Vector3(1.0f, 1.0f, 1.0f));
            //Gizmos.DrawSphere(fillPoints, 0.5f);
        }


        //Displays the current reachable waypoints from pervious waypoint.
        // for(int i = 0; i < reachablePoints.Count; i++)
        // {
        // 	float value = (Time.time - reachablePoints[i].visitTime)/Time.time;
        // 	Gizmos.color = new Color(1.0f,value,value,0.25f);
        // 	reachPoint[0] = reachablePoints[i].position[0];
        // 	reachPoint[1] = reachablePoints[i].position[1];
        // 	reachPoint[2] = reachablePoints[i].position[2];
        // 	Gizmos.DrawCube(reachPoint, new Vector3(1.0f, 1.0f, 1.0f));
        // }

        //Displays the selected waypoint the AI is headed to.
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(nextWaypoint, 0.5f);

    }
}
