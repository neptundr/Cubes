using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minCameraSize;
    
    private Camera camera;

    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A))
            transform.position = new Vector3(transform.position.x - movementSpeed, transform.position.y, transform.position.z);
        if (Input.GetKey(KeyCode.D))
            transform.position = new Vector3(transform.position.x + movementSpeed, transform.position.y, transform.position.z);
        if (Input.GetKey(KeyCode.S))
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - movementSpeed);
        if (Input.GetKey(KeyCode.W))
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + movementSpeed);
        if (Input.GetKey(KeyCode.E) && camera.orthographicSize - zoomSpeed > minCameraSize) camera.orthographicSize -= zoomSpeed;
        if (Input.GetKey(KeyCode.Q)) camera.orthographicSize += zoomSpeed;
    }
}
