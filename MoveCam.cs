using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCam : MonoBehaviour
{

    public Transform cameraPosition;
    public Transform cameraRotation;

    private void Update()
    {
        transform.position = cameraPosition.position;    
        transform.rotation = cameraRotation.rotation;
    }
}
