using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class AgentData
{
    public List<Vector3> positions;
}

public class CarData : AgentData
{
    public CarData() {}
    public CarData(string param) {
        positions = new List<Vector3>();
        directions = new List<Vector3>();
    }
    public List<Vector3> directions;
}

public class TrafficLightData
{
    public List<int> status;
    
}

public class AgentController : MonoBehaviour
{
    string serverUrl = "http://localhost:8585";
    string sendConfigEndpoint = "/init";
    string updateEndpoint = "/update";
    string getAgentsEndpoint = "/getAgents";
    string getObstaclesEndpoint = "/getObstacles";
    string getTrafficLightEndpoint = "/getTrafficLights";
    string getDestinationEndpoint = "/getDestinations";

    AgentData destinationData;
    CarData carData;
    TrafficLightData trafficLightData;
    GameObject[] agents;
    GameObject[] trafficLights;

    List<Vector3> destinationPositions;
    List<Vector3> oldPositions;
    List<Vector3> newPositions;
    List<Vector3> directions;
    List<int> status;

    public int NAgents;
    public GameObject[] carPrefabs;
    public float timeToUpdate = 5.0f, timer, dt;
    public float carRotationSpeed = 5.0f;
    bool hold = false;


    void Start()
    {
        destinationPositions = new List<Vector3>();
        oldPositions = new List<Vector3>();
        newPositions = new List<Vector3>();
        directions = new List<Vector3>();
        status = new List<int>();

        agents = new GameObject[NAgents];

        timer = timeToUpdate;

        for(int i = 0; i < NAgents; i++)
            agents[i] = Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length-1)], Vector3.zero, Quaternion.identity);
            
        StartCoroutine(SendConfiguration()); 
        StartCoroutine(GetCarCamerasRequest());
    }

    IEnumerator SendConfiguration()
    {
        WWWForm form = new WWWForm();

        form.AddField("numAgents", NAgents.ToString());

        UnityWebRequest www = UnityWebRequest.Post(serverUrl + sendConfigEndpoint, form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            StartCoroutine(GetCarData());
            StartCoroutine(GetDestinationData());
        }
    }

    IEnumerator GetCarCamerasRequest()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("CarFollowingCamera").Length >= agents.Length);
        this.gameObject.GetComponent<CameraManager>().GetCarCameras();
    }

    private void Update() 
    {
        float t = timer/timeToUpdate;
        UnityWebRequest www;
        // Smooth out the transition at start and end
        dt = t * t * ( 3f - 2f*t);

        if (timer >= timeToUpdate)
        {
            timer = 0;
            hold = true;
            StartCoroutine(UpdateSimulation());
        }

        if (!hold)
        {
            UnityWebRequest mywww;
            trafficLights = GameObject.FindGameObjectsWithTag("TrafficLight");

            for (int s = 0; s < agents.Length; s++)
            {
                if (destinationPositions.Contains(oldPositions[s]) && agents[s].active) {
                    agents[s].SetActive(false);
                } else {
                    Vector3 interpolated = Vector3.Lerp(oldPositions[s], newPositions[s], dt);

                    agents[s].transform.localPosition = interpolated;
                    
                    Vector3 dir = oldPositions[s] - newPositions[s];
                    agents[s].transform.rotation = Quaternion.Slerp(agents[s].transform.rotation, Quaternion.LookRotation(directions[s]), carRotationSpeed*Time.deltaTime);
                }
            }

            for (int s = 0; s < trafficLights.Length; s++) {
                for (int o = 0; o < 3; o++) {
                    if (o == status[s])
                        trafficLights[s].transform.GetChild(o).gameObject.SetActive(true);
                    else
                        trafficLights[s].transform.GetChild(o).gameObject.SetActive(false);
                }
            }
            // Move time from the last frame
            timer += Time.deltaTime;
        }
    }

    IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + updateEndpoint);
        
        // StartCoroutine(GetBoxData("UPDATE"));
        // StartCoroutine(GetTrashcanData());
        
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            StartCoroutine(GetCarData());
            StartCoroutine(GetTrafficLightData());
            StartCoroutine(GetTrafficLightData());
        }
    }

    IEnumerator GetCarData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getAgentsEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            carData = JsonUtility.FromJson<CarData>(www.downloadHandler.text);

            oldPositions = new List<Vector3>(newPositions);
            newPositions.Clear();
            directions.Clear();

            foreach(Vector3 v in carData.positions)
                newPositions.Add(v);
            
            foreach(Vector3 v in carData.directions)
                directions.Add(v);

            hold = false;
        }
    }

    IEnumerator GetTrafficLightData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getTrafficLightEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);

        else 
        {
            trafficLightData = JsonUtility.FromJson<TrafficLightData>(www.downloadHandler.text);

            status.Clear();
            
            foreach(int s in trafficLightData.status)
                status.Add(s);
        }
    }

    IEnumerator GetDestinationData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getDestinationEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);

        else 
        {
            destinationData = JsonUtility.FromJson<AgentData>(www.downloadHandler.text);
            
            destinationPositions.Clear();

            foreach (Vector3 pos in destinationData.positions) {
                destinationPositions.Add(pos);
            }
        }
    }
}
    