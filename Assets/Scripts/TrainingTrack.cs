using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingTrack : MonoBehaviour
{
    // Start is called before the first frame update

    public List<CarAgent> carList;
    void Start()
    {
        carList = new List<CarAgent>();
    }
}
