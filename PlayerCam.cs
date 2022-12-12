using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform cameraRotation;
    public Camera camera;

    private float xRotation;
    private float yRotation;

    private float fov;
    private float desiredFov;
    private float lastDesiredFov;

    private Coroutine fovCoroutine;
    private Coroutine tiltCoroutine;

    // Start is called before the first frame update
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        // Horizontal rotation, due to unity handling rotation in 3 dimensions x, y, z
        yRotation += mouseX;

        // Vertical rotation clamped to between -90 and 90 degrees
        // this stops confusion from the user being able to look so far up that
        // they are rotated upside down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90);

        // Rotate camera and orientation

        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        cameraRotation.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    public void DoFOV(float endValue)
    {
        if (fovCoroutine != null) StopCoroutine(fovCoroutine);
        fovCoroutine = StartCoroutine(lerpFOV(camera, endValue, 0.25f));
    }

    public void DoTilt(float endValue)
    {
        if (tiltCoroutine != null) StopCoroutine(tiltCoroutine);
        tiltCoroutine = StartCoroutine(lerpZTilt(cameraRotation, endValue, 1f));
    }

    private IEnumerator lerpFOV(Camera targetCamera, float toFov, float duration)
    {
        // smoothly lerp FOV to desired value
        float counter = 0f;

        float fromFov = targetCamera.fieldOfView;
        
        while (counter < duration)
        {
            counter += Time.deltaTime;
            
            float FOVTime = counter / duration;
            
            targetCamera.fieldOfView = Mathf.Lerp(fromFov, toFov, FOVTime);

            yield return null;
        }
    }

    private IEnumerator lerpZTilt(Transform cameraRotation, float toAngle, float duration)
    {
        // smoothly lerp ZTilt to desired value
        float counter = 0f;

        float fromZTilt = cameraRotation.rotation.z;
        
        while (counter < duration)
        {
            counter += Time.deltaTime;
            
            float difference = counter / duration;

            cameraRotation.rotation = Quaternion.Euler(0, 0, 1f);

            yield return null;
        }
    
    }
}
