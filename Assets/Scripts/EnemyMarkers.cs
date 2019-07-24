using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMarkers : MonoBehaviour
{
    public GameObject markerPrefab;
    public int numOfOthers;

    private string _ID;
    private Dictionary<PlaneControl, GameObject> otherShips;

    // Start is called before the first frame update
    void Start()
    {
        _ID = transform.root.GetComponent<PlaneControl>()._ID;
        numOfOthers = 0;
        otherShips = new Dictionary<PlaneControl, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        List<PlaneControl> playerList = new List<PlaneControl>();
        GameObject[] playerArray = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in playerArray) {
            if (go != null && go.GetComponent<PlaneControl>() != null) {
                playerList.Add(go.GetComponent<PlaneControl>());
            }
        }
        //Debug.Log(playerList.Count);
        if (playerList.Count - 1 > numOfOthers) {
            foreach (PlaneControl p in playerList) {
                if (p._ID == _ID) {
                    continue;
                }
                GameObject marker = Instantiate(markerPrefab, transform.position, transform.rotation);
                marker.transform.parent = this.transform;
                marker.GetComponent<SingleMarker>().target = p.transform.gameObject;
                otherShips.Add(p, marker);
                numOfOthers += 1;
            }
        }
        foreach (PlaneControl p in otherShips.Keys) {
            if (p == null) {
                playerList.Remove(p);
                Destroy(otherShips[p]);
                otherShips.Remove(p);
                numOfOthers -= 1;
            }
        }
    }
}
