using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.Samples.Gestures.DebugTools;
using UnityEngine;

public class GambitCard : MonoBehaviour
{
    public DeckManager deckManager;
    public Collider hitbox;
    [Header("Left Hand")]
    public Collider thumb;
    [Header("Right Hand")]
    public Collider index;
    public CurlDetection curlDetection;
    public float indexSpread;
    public Collider middle;
    public bool isHeld;
    public Vector3 anchorPoint;
    public Rigidbody rb;
    public List<Vector3> velocities;
    public LayerMask homingLayers;
    // Start is called before the first frame update
    void Start()
    {
        deckManager = transform.parent.GetComponent<DeckManager>();
        thumb = deckManager.thumb;
        index = deckManager.index;
        middle = deckManager.middle;
        curlDetection = GameObject.Find("Right Hand Detection").GetComponent<CurlDetection>();
        rb.maxAngularVelocity = 1000;
    }

    // Update is called once per frame
    void Update()
    {
        indexSpread = curlDetection.indexSpread;
    }

    public IEnumerator Held()
    {
        while(transform.localPosition.x + transform.localPosition.z < 0.05f){
            Vector3 lastPos = transform.position;
            transform.position = thumb.transform.position + anchorPoint;
            transform.localPosition = new Vector3(transform.localPosition.x, -0.0075f, transform.localPosition.z);
            transform.localEulerAngles = new Vector3(0, (transform.localPosition.x + transform.localPosition.z) * 1000, 0);
            velocities.Add(transform.position - lastPos);
            if(velocities.Count > 10){
                velocities.RemoveAt(0);
            }
            yield return 0;
        }
        StartCoroutine(SoftToss());
    }

    public IEnumerator SoftToss()
    {
        transform.parent = null;
        //rb.useGravity = true;
        foreach(Vector3 vel in velocities){
            rb.velocity += vel * 50;
        }
        rb.angularVelocity = new Vector3(0, (transform.localEulerAngles.y / Mathf.Abs(transform.localEulerAngles.y)) * rb.velocity.magnitude * 3, 0);
        velocities.Clear();
        deckManager.Draw();
        while(deckManager.gesture != "twopoint"){
            yield return 0;
        }
        float elapsed = 0.2f;
        rb.angularDrag = 4;
        while(elapsed <= 1.4f){
            transform.position = Vector3.Lerp(transform.position, Vector3.Lerp(index.transform.position, middle.transform.position, 0.5f), Mathf.Pow(elapsed - 0.1f, 2) -0.4f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(middle.transform.eulerAngles), elapsed * elapsed);
            elapsed += Mathf.Min(Time.deltaTime * 2, 1.4f - Time.deltaTime);
            yield return 0;
        }
        transform.parent = middle.transform;
        rb.angularVelocity = Vector3.zero;
        rb.angularDrag = 0;
        rb.velocity = Vector3.zero;
        StartCoroutine(Primed());
    }

    public IEnumerator Primed()
    {
        Vector3 lastPos = transform.position;
        yield return 0;
        velocities.Add(transform.position - lastPos);
        bool throwing = false;
        while((!throwing || velocities[velocities.Count - 1].magnitude > 0.04f) && indexSpread < 0.5f){
            velocities.Add(transform.position - lastPos);
            if(velocities[velocities.Count - 1].magnitude > 0.06f){
                throwing = true;
            }
            if(velocities.Count > 10){
                velocities.RemoveAt(0);
            }
            lastPos = transform.position;
            yield return 0;
        }
        transform.parent = null;
        foreach(Vector3 vel in velocities){
            rb.velocity += vel * 60;
        }
        rb.drag = 1.2f;
        rb.angularVelocity = new Vector3(0, rb.velocity.magnitude * 3, 0);
        StartCoroutine(SeekTarget());
    }

    public IEnumerator SeekTarget()
    {
        while(true){
            if(rb.velocity.magnitude < 9.81f){
                rb.velocity += new Vector3(0,-Mathf.Max(0, 9.81f - rb.velocity.magnitude * 2),0) * Time.deltaTime;
            }
            if(Physics.OverlapSphere(transform.position, 5, homingLayers) != null){
                Debug.Log("Gottem");
            }
            yield return 0;
        }
    }

    void OnTriggerEnter(Collider hit)
    {
        if(hit == thumb && isHeld == false){
            Vector3 contactPoint = -hitbox.ClosestPointOnBounds(thumb.transform.position);
            anchorPoint = contactPoint + transform.position;
            isHeld = true;
            StartCoroutine(Held());
        }
    }
}
