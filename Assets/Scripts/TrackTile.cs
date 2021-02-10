using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTile : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
            // Hedefler
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                transform.position + Vector3.up * 2, 
                transform.position + Vector3.up * 2 + transform.forward * 3
                );
        
    }
}
