using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackList : MonoBehaviour
{
    // Start is called before the first frame update
    public Dictionary<string, GameObject> roadList;
    void Awake()
    {
        roadList = new Dictionary<string, GameObject>();

        foreach (Transform child in transform)
        {
            roadList.Add(child.gameObject.name, child.gameObject);
        }
    }
}
