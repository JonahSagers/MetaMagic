using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMove : MonoBehaviour
{
    public Rigidbody rb;
    public float yaw;
    public bool isLeader;
    public Transform leader;
    public Vector3 targetOffset;
    public bool flying;
    public bool grounded;
    public bool landSignal;
    public bool takeOffSignal;
    private float propSpeed;
    private Coroutine action;
    public List<Vector3> waypoints;
    public List<Transform> nearby;
    public List<Vector3> nearbyObstacles;
    public Transform rotor1;
    public Transform rotor2;
    public Transform rotor3;
    public Transform rotor4;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        if(flying){
            if(isLeader){
                float yMove = Input.GetKey(KeyCode.LeftShift) ? -1 : 0;
                yMove += Input.GetKey(KeyCode.Space) ? 1 : 0;
                //yaw += Input.GetAxis("Mouse X") * Time.deltaTime * 2000;
                yaw += Input.GetKey(KeyCode.Q) ? -1 * Time.deltaTime * 50 : 0;
                yaw += Input.GetKey(KeyCode.E) ? 1 * Time.deltaTime * 50 : 0;
                yaw %= 360;
                float xMove = Input.GetAxis("Horizontal");
                float zMove = Input.GetAxis("Vertical");
                transform.rotation = Quaternion.Euler(0, yaw, 0);
                Vector3 move = xMove * transform.right + yMove * transform.up + zMove * transform.forward;
                rb.velocity += move * Time.deltaTime * 18;
                if(waypoints.Count > 0){
                    rb.velocity += Vector3.Normalize(waypoints[0] - transform.position) * Time.deltaTime * Mathf.Min(4 * Vector3.Distance(waypoints[0], transform.position), 18);
                    if(Vector3.Distance(waypoints[0], transform.position) < ((waypoints.Count > 1) ? 1 : 0.01f)){
                        waypoints.RemoveAt(0);
                    }
                    int i = 0;
                    while(i < nearbyObstacles.Count){
                        if(Vector3.Distance(nearbyObstacles[i], transform.position) < 2){
                            rb.velocity -= Vector3.Normalize(nearbyObstacles[i] - transform.position) * Time.deltaTime * 10 *  (15 - (5 * Vector3.Distance(nearbyObstacles[i], transform.position)));
                            i += 1;
                        } else {
                            nearbyObstacles.RemoveAt(i);
                        }
                    }
                }
                if(landSignal && action == null && isLeader){
                    landSignal = false;
                    foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
                        DroneMove droneScript = drone.GetComponent<DroneMove>();
                        droneScript.StartCoroutine(droneScript.Land());
                    }
                    Debug.Log("Landing");
                    action = StartCoroutine(Land());
                }
                //transform.localRotation = Quaternion.AngleAxis(yaw, Vector3.up);
            } else {
                if(leader != null){
                    //if(Vector3.Distance((leader.position + targetOffset), transform.position) > 3){
                        rb.velocity += Vector3.Normalize((leader.position + targetOffset) - transform.position) * Time.deltaTime * Mathf.Min(4 * Vector3.Distance((leader.position + targetOffset), transform.position), 30);
                    //}
                    foreach(Transform drone in nearby){
                        if(Vector3.Distance(drone.position, transform.position) < 2){
                            rb.velocity -= Vector3.Normalize(drone.position - transform.position) * Time.deltaTime * 15 * (15 - (5 * Vector3.Distance(drone.position, transform.position)));
                        }
                    }
                    int i = 0;
                    while(i < nearbyObstacles.Count){
                        if(Vector3.Distance(nearbyObstacles[i], transform.position) < 2){
                            rb.velocity -= Vector3.Normalize(nearbyObstacles[i] - transform.position) * Time.deltaTime * 10 *  (15 - (5 * Vector3.Distance(nearbyObstacles[i], transform.position)));
                            i += 1;
                        } else {
                            nearbyObstacles.RemoveAt(i);
                        }
                    }
                } else {
                    foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Leader")){
                        leader = drone.transform;
                    }
                }
            }
        } else {
            if(takeOffSignal && action == null && isLeader){
                takeOffSignal = false;
                foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
                    DroneMove droneScript = drone.GetComponent<DroneMove>();
                    droneScript.StartCoroutine(droneScript.TakeOff());
                }
                Debug.Log("Taking Off");
                action = StartCoroutine(TakeOff());
            }
        }
        rotor1.Rotate(Vector3.up, propSpeed * Time.deltaTime, Space.Self);
        rotor2.Rotate(Vector3.up, propSpeed * Time.deltaTime, Space.Self);
        rotor3.Rotate(Vector3.up, propSpeed * Time.deltaTime, Space.Self);
        rotor4.Rotate(Vector3.up, propSpeed * Time.deltaTime, Space.Self);
        transform.localRotation = Quaternion.Euler(rb.velocity.z, yaw, -rb.velocity.x);
        rb.velocity -= rb.velocity * 4 * Time.deltaTime;
    }
    public IEnumerator TakeOff()
    {
        grounded = false;
        while(propSpeed < 2000){
            propSpeed += Time.deltaTime * 2000;
            yield return 0;
        }
        propSpeed = 2000;
        float elapsed = 0;
        while(elapsed < 2){
            rb.velocity += Vector3.up * Time.deltaTime * 5;
            elapsed += Time.deltaTime;
            yield return 0;
        }
        landSignal = false;
        takeOffSignal = false;
        flying = true;
        action = null;
    }
    public IEnumerator Land()
    {
        flying = false;
        if(isLeader){
            yield return new WaitForSeconds(0);
        } else {
            float elapsed = 0;
            while(elapsed < 10){
                //this instance of leader.position is not affected by offset because here we *want* the drones to push each other out of the way.
                if(transform.position.y > leader.position.y){
                    rb.velocity += Vector3.down * Time.deltaTime * 4;
                } else {
                    rb.velocity += Vector3.up * Time.deltaTime * 4;
                }
                foreach(Transform drone in nearby){
                    Vector3 tempChange = new Vector3(0,0,0);
                    if(Vector3.Distance(drone.position, transform.position) < 2){
                        tempChange -= Vector3.Normalize(drone.position - transform.position) * Time.deltaTime * 15 * (15 - (5 * Vector3.Distance(drone.position, transform.position)));
                    }
                    rb.velocity += new Vector3(tempChange.x, 0, tempChange.z);
                }
                int i = 0;
                while(i < nearbyObstacles.Count){
                    if(Vector3.Distance(nearbyObstacles[i], transform.position) < 2){
                        rb.velocity -= Vector3.Normalize(nearbyObstacles[i] - transform.position) * Time.deltaTime * 10 *  (15 - (5 * Vector3.Distance(nearbyObstacles[i], transform.position)));
                        i += 1;
                    } else {
                        nearbyObstacles.RemoveAt(i);
                    }
                }
                elapsed += Time.deltaTime;
                yield return 0;
            }
        }
        while(!grounded){
            rb.velocity += Vector3.down * Time.deltaTime * 5;
            yield return 0;
        }
        while(propSpeed > 500){
            propSpeed -= Time.deltaTime * 2000;
            yield return 0;
        }
        propSpeed = 500;
        landSignal = false;
        takeOffSignal = false;
        action = null;
    }
    private void OnTriggerStay(Collider target)
    {
        if(target.gameObject.layer == 6 && !nearby.Contains(target.transform)){
            nearby.Add(target.transform);
        } else {
            if(target.gameObject.layer != 6){
                var collisionPoint = target.ClosestPoint(transform.position);
                nearbyObstacles.Clear();
                nearbyObstacles.Add(collisionPoint);
            }
        }
    }
    private void OnTriggerExit(Collider target)
    {
        if(nearby.Contains(target.transform)){
            nearby.Remove(target.transform);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if(!flying && collision.gameObject.layer == 7){
            grounded = true;
        } else {
            Debug.Log("Crash Detected" + collision.gameObject.layer);
        }
    }
}
