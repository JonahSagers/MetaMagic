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

        if (handData.indexPinch > 0.5f && dragRoutine == null)
        {
            dragRoutine = StartCoroutine(DragItem());
        }
    }

    public IEnumerator DragItem()
    {
        while (handData.indexPinch > 0.5f)
        {
            yield return 0;
        }
        dragRoutine = null;
    }
}
