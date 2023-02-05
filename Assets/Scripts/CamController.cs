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

    [SerializeField] private GameObject tmp;

    private Vector3 prevMousePos;

    // Start is called before the first frame update
    void Start()
    {
        prevMousePos = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -11)
        {
            tmp.SetActive(false);
        }
        else
        {
            tmp.SetActive(true);
        }
        if (Input.GetMouseButtonDown(2))
        {
            prevMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            transform.position -= new Vector3(0f,(Input.mousePosition - prevMousePos).y * camMoveSpeed * Camera.main.orthographicSize, 0f);
            transform.position = new Vector3(transform.position.x, Mathf.Min(transform.position.y, -90f), transform.position.z);
            prevMousePos = Input.mousePosition;
        }
        //Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - Input.mouseScrollDelta.y * camResizeSpeed, minViewPort, maxViewPort);
        //Debug.Log(Camera.main.orthographicSize);
    }
}
