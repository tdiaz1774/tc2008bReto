using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] List<GameObject> cameras;
    List<GameObject> carCameras;
    List<List<GameObject>> setsOfCameras;
    Dictionary<string, int> keyToMovement;
    int currentCamIndex;
    int currentCamSetIndex;
    int movement;

    void Start()
    {
        setsOfCameras = new List<List<GameObject>>();
        carCameras = new List<GameObject>();

        keyToMovement = new Dictionary<string, int>{
            {"PreviousCam", -1},
            {"NextCam", 1}
        };

        setsOfCameras.Add(cameras);

        currentCamIndex = 0;
        currentCamSetIndex = 0;
        SetCameras();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("PreviousCam") != Input.GetButtonDown("NextCam")) {
            movement = Input.GetButtonDown("PreviousCam") ? keyToMovement["PreviousCam"] : keyToMovement["NextCam"];

            if (currentCamIndex + movement < setsOfCameras[currentCamSetIndex].Count && currentCamIndex + movement >= 0)
                currentCamIndex += movement;
            else if (currentCamIndex + movement >= setsOfCameras[currentCamSetIndex].Count)
                currentCamIndex = 0;
            else
                currentCamIndex = setsOfCameras[currentCamSetIndex].Count - 1;

            SetCameras();
        }

        if (Input.GetButtonDown("PreviousCamSet") != Input.GetButtonDown("NextCamSet")) {
            movement = Input.GetButtonDown("PreviousCamSet") ? keyToMovement["PreviousCam"] : keyToMovement["NextCam"];
            SetCameras(true);

            if (currentCamSetIndex + movement < setsOfCameras.Count && currentCamSetIndex + movement >= 0)
                currentCamSetIndex += movement;
            else if (currentCamSetIndex + movement >= setsOfCameras.Count)
                currentCamSetIndex = 0;
            else
                currentCamSetIndex = setsOfCameras.Count - 1;
            
            currentCamIndex = 0;
            SetCameras();
        }

        if (setsOfCameras[currentCamSetIndex][currentCamIndex].tag == "CarFollowingCameraDisabled")
            SetCameras();
    }

    void SetCameras(bool changeSet = false)
    {
        while (setsOfCameras[currentCamSetIndex][currentCamIndex].tag == "CarFollowingCameraDisabled") {
            setsOfCameras[currentCamSetIndex].Remove(setsOfCameras[currentCamSetIndex][currentCamIndex]);

            if (currentCamIndex + 1 < setsOfCameras[currentCamSetIndex].Count)
                currentCamIndex += movement;
            else
                currentCamIndex = 0;
            
            if (setsOfCameras[currentCamSetIndex].Count == 0) {
                setsOfCameras.Remove(setsOfCameras[currentCamSetIndex]);
                currentCamSetIndex = 0;
            }
        }

        foreach(GameObject camera in setsOfCameras[currentCamSetIndex]) {
            if (camera == setsOfCameras[currentCamSetIndex][currentCamIndex] && !changeSet)
                camera.SetActive(true);
            else
                camera.SetActive(false);
        }
    }

    public void GetCarCameras()
    {
        GameObject[] carCamerasArr = GameObject.FindGameObjectsWithTag("CarFollowingCamera");

        foreach(GameObject carCamera in carCamerasArr)
        {
            carCamera.SetActive(false);
            carCameras.Add(carCamera);
        }

        setsOfCameras.Add(carCameras);
    }
}
