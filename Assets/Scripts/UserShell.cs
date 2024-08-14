using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserShell : MonoBehaviour
{
    public DroneCommand playerBrain;
    public GameObject playerPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(playerBrain.mode == "ground"){
            transform.position = playerPos.transform.position;
        }
    }
}
