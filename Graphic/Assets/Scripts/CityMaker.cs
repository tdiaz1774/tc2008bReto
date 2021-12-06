using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CityMaker : MonoBehaviour
{
    [SerializeField] TextAsset layout;
    [SerializeField] GameObject roadPrefab;
    [SerializeField] GameObject buildingPrefab;
    [SerializeField] GameObject semaphorePrefab;
    [SerializeField] float minBuildingSize = 1f;
    [SerializeField] float maxBuildingSize = 2f;
    [SerializeField] int tileSize;
    float offset;
    int angle;
    int len;
    int[] range;
    char[] rightLeft;
    char[] upDown;

    // Start is called before the first frame update
    void Start()
    {
        range = new int[2]{-1,1};
        rightLeft = new char[2]{'<','>'};
        upDown = new char[2]{'^','v'};
        MakeTiles(layout.text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MakeTiles(string tiles)
    {
        int x = 0;
        // Mesa has y 0 at the bottom
        // To draw from the top, find the rows of the file
        // and move down
        // Remove the last enter, and one more to start at 0
        int y = tiles.Split('\n').Length - 1;
        len = y;
        Debug.Log(y);

        Vector3 position;
        GameObject tile;

        for (int i=0; i<tiles.Length; i++) {
            if (tiles[i] == '>' || tiles[i] == '<') {
                position = new Vector3(x * tileSize, 0, y * tileSize);
                tile = Instantiate(roadPrefab, position, Quaternion.identity);
                tile.transform.parent = transform;
                x += 1;
            } else if (tiles[i] == 'v' || tiles[i] == '^') {
                position = new Vector3(x * tileSize, 0, y * tileSize);
                tile = Instantiate(roadPrefab, position, Quaternion.Euler(0, 90, 0));
                tile.transform.parent = transform;
                x += 1;
            } else if (tiles[i] == 's') {
                foreach (int j in range) {
                    foreach (char dir in rightLeft) {
                        if ((i+j < tiles.Length && i+j >= 0 && Mathf.Floor((i+j)/len) == Mathf.Floor(i/len) && tiles[i+j] == dir) && (i+2*j < tiles.Length && i+2*j >= 0 && Mathf.Floor((i+2*j)/len) == Mathf.Floor(i/len) && tiles[i+2*j] == dir)) {
                            angle = dir == '>' ? 90 : 270;
                            offset = dir == '<' ? 0.5f : -0.5f;
                        }
                    }
                }

                position = new Vector3(x * tileSize, 0, y * tileSize);
                tile = Instantiate(roadPrefab, position, Quaternion.identity);
                tile.transform.parent = transform;

                position = new Vector3(position.x, position.y, position.z+offset);

                tile = Instantiate(semaphorePrefab, position, Quaternion.Euler(0, angle, 0));
                tile.tag = "TrafficLight";
                tile.transform.parent = transform;
                x += 1;
            } else if (tiles[i] == 'S') {
                foreach (int j in range) {
                    foreach (char dir in upDown) {
                        if (i+(len+2)*j < tiles.Length && i+(len+2)*j >= 0 && tiles[i+(len+2)*j] == dir) {
                            Debug.Log("Light at " + i.ToString() + " has direction " + dir);
                            angle = dir == '^' ? 0 : 180;
                            offset = dir == 'v' ? -0.5f : 0.5f;
                        }
                    }
                }

                position = new Vector3(x * tileSize, 0, y * tileSize);
                tile = Instantiate(roadPrefab, position, Quaternion.Euler(0, 90, 0));
                tile.transform.parent = transform;

                position = new Vector3(position.x+offset, position.y, position.z);

                tile = Instantiate(semaphorePrefab, position, Quaternion.Euler(0, angle, 0));
                tile.tag = "TrafficLight";
                tile.transform.parent = transform;
                x += 1;
            } else if (tiles[i] == 'D') {
                position = new Vector3(x * tileSize, 0, y * tileSize);
                tile = Instantiate(buildingPrefab, position, Quaternion.Euler(0, 90, 0));
                tile.GetComponent<Renderer>().materials[0].color = Color.red;
                tile.transform.parent = transform;
                x += 1;
            } else if (tiles[i] == '#') {
                position = new Vector3(x * tileSize, 0, y * tileSize);
                tile = Instantiate(buildingPrefab, position, Quaternion.identity);
                tile.transform.localScale = new Vector3(1, Random.Range(minBuildingSize, maxBuildingSize), 1);
                tile.transform.parent = transform;
                x += 1;
            } else if (tiles[i] == '\n') {
                x = 0;
                y -= 1;
            }
        }

    }
}
