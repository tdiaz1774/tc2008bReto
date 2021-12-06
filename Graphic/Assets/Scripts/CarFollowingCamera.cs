using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarFollowingCamera : MonoBehaviour
{
    public GameObject cameraPrefab;
    [SerializeField] float smoothFactor = 0.5f;
    [SerializeField] float cameraHeight = 0.3f;
    [SerializeField] float cameraProximity = 0.8f;
    [SerializeField] float camSensitivity = 0.1f;
    private Vector3 lastMouse;
    private float camRotation;
    private int fullSpin;
    GameObject camera;
    Vector3 newPosition;

    // Start is called before the first frame update
    void Start()
    {
        camera = Instantiate(cameraPrefab, transform.position, Quaternion.identity);
        camera.tag = "CarFollowingCamera";
    }

    void LateUpdate()
    {
        if (camera.active) {
            lastMouse = Input.mousePosition - lastMouse;
            fullSpin = lastMouse.x >= 0 ? 360 : -360;
            lastMouse = new Vector3((lastMouse.x - Mathf.Floor(lastMouse.x/fullSpin)*fullSpin)*camSensitivity, 0, 0);

            camRotation += lastMouse.x;

            if (camRotation > 360) {
                camRotation = 360 - camRotation;
            } else if (camRotation < 0) {
                camRotation = 360 + camRotation;
            }

            lastMouse = Input.mousePosition;

            newPosition = transform.position + new Vector3(Mathf.Sin((transform.rotation.eulerAngles.y+camRotation)*Mathf.PI/180)*cameraProximity, cameraHeight, Mathf.Cos((transform.rotation.eulerAngles.y+camRotation)*Mathf.PI/180)*cameraProximity);
            camera.transform.position = Vector3.Slerp(camera.transform.position, newPosition, smoothFactor);
            camera.transform.LookAt(transform);
        }
    }

    // void OnEnable()
    // {
    //     lastMouse = Input.mousePosition;
    //     camRotation = 180f;
    // }

    void OnDisable()
    {
        camera.tag = "CarFollowingCameraDisabled";
    }
}
