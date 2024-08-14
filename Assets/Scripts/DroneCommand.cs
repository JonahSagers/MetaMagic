using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.Samples.Gestures.DebugTools;
using TMPro;

public class DroneCommand : MonoBehaviour
{
    public UdpSocket socket;
    public GameObject leader;
    public LineRenderer lineLeft;
    public GameObject trackballLeft;
    public LineRenderer lineRight;
    public GameObject trackballRight;
    public LineRenderer trackManualLeft;
    public LineRenderer trackManualRight;
    public Transform leftRoot;
    public Transform rightRoot;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject leftHandMesh;
    public GameObject rightHandMesh;
    public Vector3 leftManualPos;
    public Vector3 rightManualPos;
    public GameObject hitbox;
    public CurlDetection leftCurls;
    public CurlDetection rightCurls;
    public GameObject camPos;
    public Camera cam;
    //public Camera droneCam;
    public GameObject rig;
    private bool gestureLatch;
    public bool leftFist;
    public bool rightFist;
    public bool leftPoint;
    public bool rightPoint;
    public bool leftSnap;
    public bool rightSnap;
    public bool leftOpen;
    public bool rightOpen;
    public bool leftManual;
    public bool rightManual;
    public Material redMat;
    public Material blueMat;
    public Material manualMat;
    public Material autoMat;
    public Material defaultMat;
    public LayerMask obstacles;
    public string mode;
    public Image blackout;
    private float packetCooldown;
    // Start is called before the first frame update
    void Start()
    {
        mode = "ground";
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Leader")){
            leader = drone;
        }
        Application.targetFrameRate = 72;
        //SwapMode();
        //SwapMode();
    }
    // Update is called once per frame
    void Update()
    {
        // if((leftOpen && rightFist) || (leftFist && rightOpen)){
        //     float handDist = Vector3.Distance(leftHand.transform.position,rightHand.transform.position);
        //     if(handDist < 0.12f && !gestureLatch){
        //         foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Leader")){
        //             drone.GetComponent<DroneMove>().signal = true;
        //         }
        //         gestureLatch = true;
        //     } else if(handDist > 0.15f){
        //         gestureLatch = false;
        //     }
        // }
        if(leftManual){
            trackManualLeft.SetPosition(0, leftManualPos + Vector3.ClampMagnitude(leftRoot.position - (leftManualPos + camPos.transform.position), 0.4f) + camPos.transform.position);
            trackManualLeft.SetPosition(1, leftManualPos + camPos.transform.position);
            leftHandMesh.GetComponent<Renderer>().material = manualMat;
            packetCooldown -= Time.deltaTime;
            Vector3 vel = Vector3.ClampMagnitude((leftRoot.position - (leftManualPos + camPos.transform.position)) * 1.5f, 0.6f);
            if(packetCooldown <= 0){
                socket.SendData("Velocity " + vel.x + " " + vel.y + " " + vel.z);
                packetCooldown = 0.05f;
            }
            if(leader.GetComponent<DroneMove>().flying == true){
                leader.GetComponent<Rigidbody>().velocity += vel;
            }
        } else {
            leftHandMesh.GetComponent<Renderer>().material = autoMat;
        }
        if(rightManual){
            trackManualRight.SetPosition(0, rightManualPos + Vector3.ClampMagnitude(rightRoot.position - (rightManualPos + camPos.transform.position), 0.4f) + camPos.transform.position);
            trackManualRight.SetPosition(1, rightManualPos + camPos.transform.position);
            rightHandMesh.GetComponent<Renderer>().material = manualMat;
            if(leader.GetComponent<DroneMove>().flying == true){
                leader.GetComponent<Rigidbody>().velocity += Vector3.ClampMagnitude((rightRoot.position - (rightManualPos + camPos.transform.position)) * 1.5f, 0.6f);
            }
        } else {
            rightHandMesh.GetComponent<Renderer>().material = autoMat;
        }
        if(leftOpen && rightOpen){
            float handDist = Vector3.Distance(leftHand.transform.position,rightHand.transform.position);
            blackout.color = new Color(0,0,0,2.2f - (handDist * 10));
            if(handDist < 0.15f && !gestureLatch){
                SwapMode();
                gestureLatch = true;
            } else if(handDist > 0.17f){
                gestureLatch = false;
            }
        } else {
            blackout.color = new Color(0,0,0,0);
        }
        if(leftPoint){
            trackballLeft.transform.position = leftRoot.position + leftRoot.forward * (leftCurls.thumbSpread * 15 + 3);
            if(Physics.OverlapSphere(trackballLeft.transform.position, 3, obstacles).Length > 0){
                trackballLeft.GetComponent<Renderer>().material = redMat;
                lineLeft.endColor = Color.red;
            } else {
                lineLeft.endColor = Color.blue;
                trackballLeft.GetComponent<Renderer>().material = blueMat;
            }
            lineLeft.SetPosition(1, trackballLeft.transform.position);
            lineLeft.SetPosition(0, leftRoot.position);
        }
        if(rightPoint){
            trackballRight.transform.position = rightRoot.position + rightRoot.forward * (rightCurls.thumbSpread * 15 + 3);
            if(Physics.OverlapSphere(trackballRight.transform.position, 3, obstacles).Length > 0){
                trackballRight.GetComponent<Renderer>().material = redMat;
                lineRight.endColor = Color.red;
            } else {
                lineRight.endColor = Color.blue;
                trackballRight.GetComponent<Renderer>().material = blueMat;
            }
            lineRight.SetPosition(1, trackballRight.transform.position);
            lineRight.SetPosition(0, rightRoot.position);
        }
    }
    public void SwapMode()
    {
        //Modes:
        //"ground" Stand on the ground and control the swarm
        //"fpv" Control the leader drone's velocity directly
        //"spectator" Follow the leader drone but retain ground control.  NOTE: this is my favorite mode, but it's not default because of motion sickness
        if(mode == "ground"){
            mode = "spectator";
            rig.transform.parent = leader.transform;
            rig.transform.localPosition = -cam.gameObject.transform.position;
            rig.transform.eulerAngles = new Vector3(0,90,0);
        // } else if(mode == "spectator"){
        //     mode = "fpv";
        //     GameObject leader = null;
        //     foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Leader")){
        //         rig.transform.parent = drone.transform;
        //         leader = drone;
        //     }
        //     droneCam = leader.transform.GetChild(0).GetComponent<Camera>();
        //     droneCam.enabled = true;
        //     cam.enabled = false;
        //     rig.transform.localPosition = new Vector3(0,-1f,0);
        } else {
            mode = "ground";
            rig.transform.parent = null;
            rig.transform.position = hitbox.transform.position;
            rig.transform.eulerAngles = new Vector3(0,90,0);
        }
    }
    // void LateUpdate()
    // {
    //     if(droneCam != null && droneCam.enabled){
    //         droneCam.gameObject.transform.rotation = camPos.transform.rotation;
    //     }
    // }
    public void TakeOffCommand()
    {
        //currently there is no difference between takeoff and land, but they're separate functions just in case
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Leader")){
            drone.GetComponent<DroneMove>().takeOffSignal = true;
        }
        socket.SendData("TakeOff");
    }
    public void LandCommand()
    {
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Leader")){
            drone.GetComponent<DroneMove>().landSignal = true;
        }
        socket.SendData("Land");
    }
    public void LeftManual()
    {
        if(leftManual == false){
            leftManual = true;
            leader.GetComponent<DroneMove>().waypoints.Clear();
            leftManualPos = leftRoot.position - camPos.transform.position;
            trackManualLeft.enabled = true;
        }
    }
    public void LeftManualEnd()
    {
        leftManual = false;
        trackManualLeft.enabled = false;
        // if(!rightManual){
        //     socket.SendData("Stop");
        // }
    }
    public void RightManual()
    {
        if(rightManual == false){
            rightManual = true;
            leader.GetComponent<DroneMove>().waypoints.Clear();
            rightManualPos = rightRoot.position - camPos.transform.position;
            trackManualRight.enabled = true;
        }
    }
    public void RightManualEnd()
    {
        rightManual = false;
        trackManualRight.enabled = false;
        // if(!leftManual){
        //     socket.SendData("Stop");
        // }
    }
    public void LeftFist()
    {
        leftFist = true;
        LeftManualEnd();
    }
    public void LeftFistEnd()
    {
        leftFist = false;
    }
    public void RightFist()
    {
        rightFist = true;
        RightManualEnd();
    }
    public void RightFistEnd()
    {
        rightFist = false;
    }
    public void LeftPoint()
    {
        if(leftManual == false){
            leftPoint = true;
            lineLeft.enabled = true;
        }
    }
    public void LeftPointEnd()
    {
        if(leftManual == false){
            leftPoint = false;
            lineLeft.enabled = false;
            if(Physics.OverlapSphere(trackballLeft.transform.position, 3, obstacles).Length == 0){
                foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Leader")){
                    drone.GetComponent<DroneMove>().waypoints.Add(trackballLeft.transform.position);
                }
            }
            socket.SendData("GoTo " + trackballLeft.transform.position.x + " " + trackballLeft.transform.position.y + " " + trackballLeft.transform.position.z);
        }
    }
    public void RightPoint()
    {
        if(rightManual == false){
            rightPoint = true;
            lineRight.enabled = true;
        }
    }
    public void RightPointEnd()
    {
        if(rightManual == false){
            rightPoint = false;
            lineRight.enabled = false;
            if(Physics.OverlapSphere(trackballRight.transform.position, 3, obstacles).Length == 0){
                foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Leader")){
                    drone.GetComponent<DroneMove>().waypoints.Add(trackballRight.transform.position);
                }
            }
            socket.SendData("GoTo " + trackballRight.transform.position.x + " " + trackballRight.transform.position.y + " " + trackballRight.transform.position.z);
        }
    }
    public void LeftOpen()
    {
        leftOpen = true;
        LeftManualEnd();
    }
    public void LeftOpenEnd()
    {
        leftOpen = false;
    }
    public void RightOpen()
    {
        rightOpen = true;
        RightManualEnd();
    }
    public void RightOpenEnd()
    {
        rightOpen = false;
    }
}
