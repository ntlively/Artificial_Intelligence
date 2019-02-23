using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DisplayGrid : MonoBehaviour
{
    //Create UI Accessible Developement Variables
    [Header("Grid Display")]
    [Range(0.0f, 1.0f)]
    public float gridOpacity = 0.5f;
    [Range(0.0f, 10.0f)]
    public float distanceWeight = 10;

    //public float cubeXSize = 1.0f - (targetDist * Mathf.Cos(targetAngle));
    //public float cubeZSize = 1.0f - (targetDist * Mathf.Sin(targetAngle));

    public bool displayGrid = true;

    private Vector3 targetLocation = new Vector3(0.0f, 0.0f, 0.0f);
    //Regular Variables, not exposed to UI
    private List<NavCheckPoint> gridList = new List<NavCheckPoint>();
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

    public void setTargetLocation(Vector3 target) {
        targetLocation = target;
    }

    public virtual void OnDrawGizmosSelected()
    {

        Vector3 fillPoints = new Vector3(-21.0f, 1.0f, 20.0f);


        //Display entire influence map using cubes
        for (int k = 0; k < gridList.Count; k++)
        {
 
            float targetDist = Vector3.Distance(gridList[k].position, targetLocation);
            //float targetAngle = Vector3.Angle(gridList[k].position, targetLocation);

            //Opacity value is visit time ratio
            float opacityValue;

            if (displayGrid)
                opacityValue = Mathf.Clamp(1.0f - targetDist/distanceWeight, 0.0f, 1.0f);
            else
                opacityValue = 0.0f;

            //Set the color of the squares
            Gizmos.color = new Color(1.0f-opacityValue, 0.0f+opacityValue, 0.5f, gridOpacity * opacityValue);

            fillPoints = gridList[k].position;
            fillPoints[1] += 0.05f;

            Gizmos.DrawCube(fillPoints, new Vector3(1.0f, opacityValue+1.0f, 1.0f));
        }

    }
}
