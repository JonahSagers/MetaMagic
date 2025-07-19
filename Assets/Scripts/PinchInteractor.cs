using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands.Samples.Gestures.DebugTools;

public class PinchInteractor : MonoBehaviour
{
    public Transform indexPos;
    public Transform thumbPos;
    public Transform wrist;
    public Vector3 rotationOffset;

    public CurlDetection handData;
    private Coroutine dragRoutine;

    public LineRenderer tether;
    public Transform hovered;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = (indexPos.position + thumbPos.position) / 2;
        transform.rotation = wrist.rotation;
        transform.Rotate(rotationOffset);
        Debug.DrawLine(transform.position, transform.position + transform.forward);

        if (handData.indexPinch > 0.5f && dragRoutine == null){
            dragRoutine = StartCoroutine(DragItem(hovered));
        }
    }

    public IEnumerator DragItem(Transform target)
    {
        float previousAngle = wrist.eulerAngles.z;
        float angleChange = 0;
        float distance = Vector3.Distance(transform.position, target.position);
        Rigidbody rb = target.GetComponent<Rigidbody>();
        float ogDrag = rb.drag;
        rb.useGravity = false;
        rb.drag = 5;
        tether.enabled = true;
        while (handData.indexPinch > 0.5f)
        {
            float angle = wrist.eulerAngles.z;
            float diff = Mathf.DeltaAngle(previousAngle, angle);
            previousAngle = angle;
            angleChange += diff;

            Debug.Log(angleChange);
            rb.velocity += ((transform.position + transform.forward * distance) - target.position) * Time.deltaTime * 20;
            tether.SetPosition(0, transform.position);
            tether.SetPosition(1, target.transform.position);

            if (angleChange > 50){
                distance -= Time.deltaTime / 2;
            }
            else if (angleChange < -50){
                distance += Time.deltaTime / 2;
            }
            yield return 0;
        }
        rb.drag = ogDrag;
        rb.useGravity = true;
        tether.enabled = false;
        dragRoutine = null;
    }
}
