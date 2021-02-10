using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAgentFollow : MonoBehaviour
{
    public Transform CarAgentTransform;
    public float distance;
    public float height;
    public float rotationDamping;
    public float heightDamping;
    public float zoomRatio;
    public float defaultFOV;
    private float rotation_vector;

    // Start is called before the first frame update
    void FixedUpdate()
    {
        Vector3 local_velocity = CarAgentTransform.InverseTransformDirection(CarAgentTransform.GetComponent<Rigidbody>().velocity);
        if (local_velocity.z < -0.5f)
        {
            rotation_vector = CarAgentTransform.eulerAngles.y + 100;
        }
        else
        {
            rotation_vector = CarAgentTransform.eulerAngles.y;
        }

        float accelaration = CarAgentTransform.GetComponent<Rigidbody>().velocity.magnitude;
        Camera.main.fieldOfView = defaultFOV + accelaration * zoomRatio * Time.deltaTime;
    }
    void LateUpdate()
    {
        float wantedAngle = rotation_vector;
        float wantedHeight = CarAgentTransform.position.y + height;
        float myAngle = transform.eulerAngles.y;
        float myHeight = transform.position.y;

        myAngle = Mathf.LerpAngle(myAngle, wantedAngle, rotationDamping * Time.deltaTime);
        myHeight = Mathf.LerpAngle(myHeight, wantedHeight, heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0, myAngle, 0);

        transform.position = CarAgentTransform.position;
        transform.position -= currentRotation * Vector3.forward * distance;

        Vector3 temp = transform.position;
        temp.y = myHeight;
        transform.position = temp;
        transform.LookAt(CarAgentTransform);


    }

}
