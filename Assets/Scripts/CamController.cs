using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CamController : MonoBehaviour
{
    [SerializeField] private float minViewPort;
    [SerializeField] private float maxViewPort;
    [SerializeField] private float camResizeSpeed;

    [SerializeField] private float camMoveSpeed;

    private Vector3 prevMousePos;

    // Start is called before the first frame update
    void Start()
    {
        prevMousePos = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            prevMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            transform.position -= (Input.mousePosition - prevMousePos) * camMoveSpeed * Camera.main.orthographicSize;
            prevMousePos = Input.mousePosition;
        }
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - Input.mouseScrollDelta.y * camResizeSpeed, minViewPort, maxViewPort);
        Debug.Log(Camera.main.orthographicSize);
    }
}
