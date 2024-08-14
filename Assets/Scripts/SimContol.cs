using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimContol : MonoBehaviour
{
    public Transform leader;
    public GameObject dronePre;
    public int droneCount;
    public int tolerance;
    public LineRenderer line;
    public List<GameObject> drones;
    public Dictionary<string, List<Vector3>> targetPositions = new Dictionary<string, List<Vector3>>{{"Cube",new List<Vector3>()},{"V",new List<Vector3>()}};
    // Start is called before the first frame update
    void Start()
    {
        targetPositions["Cube"].Clear();
        int i = 0;
        int x;
        int y;
        int z;
        while(targetPositions["Cube"].Count < droneCount){
            i += tolerance;
            x = i;
            y = i;
            z = -i;
            for(x = i; x >= -i; x-=tolerance){
                for(z = -i; z <= i; z+=tolerance){
                    targetPositions["Cube"].Add(new Vector3(x,y,z));
                }
            }
            for(y = i-tolerance; y > -i; y-=tolerance){
                z = -i;
                for(x = i; x > -i; x-=tolerance){
                    targetPositions["Cube"].Add(new Vector3(x,y,z));
                }
                for(z = -i; z < i; z+=tolerance){
                    targetPositions["Cube"].Add(new Vector3(x,y,z));
                }
                for(x = -i; x < i; x+=tolerance){
                    targetPositions["Cube"].Add(new Vector3(x,y,z));
                }
                for(z = i; z > -i; z-=tolerance){
                    targetPositions["Cube"].Add(new Vector3(x,y,z));
                }
            }
            for(x = -i; x <= i; x+=tolerance){
                for(z = -i; z <= i; z+=tolerance){
                    targetPositions["Cube"].Add(new Vector3(x,y,z));
                }
            }
        }
        targetPositions["V"].Clear();
        x = 0;
        z = 0;
        while(targetPositions["V"].Count < droneCount){
            x += tolerance;
            z += tolerance;
            targetPositions["V"].Add(new Vector3(x,0,z));
            targetPositions["V"].Add(new Vector3(-x,0,z));
        }
        for(int j = 0; j < droneCount; j++){
            GameObject drone = Instantiate(dronePre, new Vector3(j * -5 - 2,-4.290f,Random.Range(-1.0f,1.0f)), Quaternion.identity);
            drones.Add(drone);
        }
        SetFormation("Cube");
        SetFormation("V");
    }

    // Update is called once per frame
    void Update()
    {
        if(leader == null){
            foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Leader")){
                drone.GetComponent<DroneMove>().isLeader = true;
                leader = drone.transform;
                //transform.parent = leader;
                //transform.localPosition = new Vector3(0,1,-4);
            }
            if(leader == null){
                GameObject.FindGameObjectsWithTag("Drone")[0].tag = "Leader";
            }
        } else {
            DroneMove leaderScript = leader.GetComponent<DroneMove>();
            if(leaderScript.waypoints.Count > 0){
                line.positionCount = (leaderScript.waypoints.Count + 1);
                line.SetPosition(0, leader.transform.position + (Vector3.up * 0.06f));
                for(int i = 1; i < line.positionCount; i++){
                    line.SetPosition(i,leaderScript.waypoints[i-1]);
                }
            } else {
                line.positionCount = 2;
                line.SetPosition(0, leader.transform.position + (Vector3.up * 0.06f));
                line.SetPosition(1, leader.transform.position + (Vector3.up * 0.06f) + leader.GetComponent<Rigidbody>().velocity);
            }
            
        }
    }

    public void SetFormation(string formation)
    {
        int i = 0;
        foreach(GameObject drone in drones){
            drone.GetComponent<DroneMove>().targetOffset = targetPositions[formation][i];
            i++;
        }
    }
}
