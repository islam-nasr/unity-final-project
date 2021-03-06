﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMouseFollow : MonoBehaviour
{
    public float mouseSensitivity = 150f;
    public Transform playerBody;
    
    float xRotation = 0F;
    // Start is called before the first frame update
    void Start()
    {
        //start the mouse at the center
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
        //playerBody.Rotate(Vector3.right * mouseY);

        
    }
}
