using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector2 rotation;
    public float mousespeed = 5;
    public float movementspeed = 20f;

    private void Start()
    {
        rotation = transform.eulerAngles;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            rotation.y += Input.GetAxis("Mouse X");
            rotation.x += -Input.GetAxis("Mouse Y");
            transform.eulerAngles = (Vector2)rotation * mousespeed;
        }

        float xDir = Input.GetAxis("Horizontal");
        float zDir = Input.GetAxis("Vertical");

        float yDir = 0;
        if (Input.GetKey("q"))
        {
            yDir = 1;
        } else if (Input.GetKey("e"))
        {
            yDir = -1;
        }

        Vector3 moveDir = transform.right * xDir + transform.forward * zDir + transform.up * yDir;
        transform.position += moveDir * (movementspeed * Time.deltaTime);
    }
}