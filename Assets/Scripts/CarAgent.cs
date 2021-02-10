using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    public float speed = 10f;
    public float torque = 10f;
    public int id=1;
    public int score = 0;
    public bool resetOnCollision = true;
    private bool destroyAllCars = true;
    public bool brake = false;
    Vector3 startPosition;
    Quaternion startDirection;

    private Transform _track;

    public GameObject carRoad;
    private Dictionary<string, GameObject> roadList;
    List <GameObject> Track_Names;
    Stack<string> stacks = new Stack<string>();
    bool coordinateControl = false;
    List<float> list = new List<float>();

    public override void Initialize()
    {
        GetTrackIncrement();
    }

    private void Start()
    {
        this.transform.parent.gameObject.GetComponent<TrainingTrack>().carList.Add(this);

        startPosition = transform.localPosition;
        startDirection = transform.localRotation;
        stacks.Push("Start");
        Track_Names = new List<GameObject>();
        for (int i = 0; i < carRoad.GetComponent<TrackList>().roadList.Count; i++)
        {
            Track_Names.Add(carRoad.GetComponent<TrackList>().roadList.ElementAt(i).Value);
            //Debug.Log(Track_Names[i]);
        }

    }

    private void MoveCar(float horizontal, float vertical, float dt)
    {
        var v1 = ObserveRay(1.5f, .5f, 25f);
        var v2 = ObserveRay(1.5f, 0f, 0f);
        var v3 = ObserveRay(1.5f, -.5f, -25f);
        if (v1.hitdistance!=0)
        {
            list.Add(v1.hitdistance);
        }
        if (v2.hitdistance != 0)
        {
            list.Add(v2.hitdistance);
        }
        if (v3.hitdistance != 0)
        {
            list.Add(v3.hitdistance);
        }
        list.Sort();
        //Debug.Log("V1.distance = "+v1.hitdistance+"\n"+"V2.distance"+v2.hitdistance+"\n"+"V3.distance"+v3.hitdistance);
        if (v1.car == true || v2.car == true || v3.car == true)
        {
            //Debug.Log(v1.hitdistance + "\t" + v2.hitdistance + "\t" + v3.hitdistance + "\n" + list[0] + "\n");
            if (list[0]<2f)
            {
                float distance = (speed * 0) * vertical;
                //Debug.Log("Speed == " + (speed / 5));
                transform.Translate(distance * dt * Vector3.forward);
                //Debug.Log("AAAAAA" + distance * dt * Vector3.forward);
                float rotation = horizontal * (torque / 2f) * 90f;
                //Debug.Log("Torque == " + (torque / 2));
                transform.Rotate(0f, rotation * dt, 0f);
                list.Clear();
            }
            else
            {
                float distance = (list[0]/5) * vertical;
                //Debug.Log("Speed == " + (speed / 5));
                transform.Translate(distance * dt * Vector3.forward);
                //Debug.Log("KKKKKKKKK"+distance*dt*Vector3.forward);
                float rotation = horizontal * (torque / 2f) * 90f;
                //Debug.Log("Torque == " + (torque / 2));
                transform.Rotate(0f, rotation * dt, 0f);
                list.Clear();
            }
        }
        else
        {
            if (list.Count==0)
            {
                float distance = speed * vertical;
                //Debug.Log(distance);
                transform.Translate(distance * dt * Vector3.forward);
                //Debug.Log("Speed == " + speed);
                float rotation = horizontal * torque * 90f;
                transform.Rotate(0f, rotation * dt, 0f);
                //Debug.Log("Torque == " + torque);
                list.Clear();
            }
            else
            {
                float distance = (speed / 5) * list[0] * vertical;
                //Debug.Log(distance);
                transform.Translate(distance * dt * Vector3.forward);
                //Debug.Log("Speed == " + speed);
                float rotation = horizontal * torque * 90f;
                transform.Rotate(0f, rotation * dt, 0f);
                //Debug.Log("Torque == " + torque);
                list.Clear();
            }
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        float horizontal = vectorAction[0];
        float vertical = vectorAction[1];

        var lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        int reward = GetTrackIncrement();
        
        var moveVec = transform.position - lastPos;
        float angle = Vector3.Angle(moveVec, _track.forward);
        float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        AddReward(bonus + reward);

        score += reward;
    }

    public override void CollectObservations(VectorSensor vectorSensor)
    {
        float angle = Vector3.SignedAngle(_track.forward, transform.forward, Vector3.up);

        
        var v1 = ObserveRay(1.5f, .5f, 25f);
        var v2 = ObserveRay(1.5f, 0f, 0f);
        var v3 = ObserveRay(1.5f, -.5f, -25f);
        var v4 = ObserveRay(-1.5f, 0, 180f);
        vectorSensor.AddObservation(angle / 180f);
        vectorSensor.AddObservation(v1.value);
        vectorSensor.AddObservation(v2.value);
        vectorSensor.AddObservation(v3.value);
        vectorSensor.AddObservation(v4.value);

    }

    private returnvalues ObserveRay(float z, float x, float angle)
    {
        var rf = new returnvalues();
        var tf = transform;
        // Başlangıç Pozisyonu
        var raySource = tf.position + Vector3.up / 2f; 
        const float RAY_DIST = 5f;
        var position = raySource + tf.forward * z + tf.right * x;

        // Açısı
        var eulerAngle = Quaternion.Euler(0, angle, 0f);
        var dir = eulerAngle * tf.forward;
        // Hit kontrolü
        Physics.Raycast(position, dir, out var hit, RAY_DIST);
        //Debug.DrawRay(tf.position,hit.point,Color.blue);
        if(hit.collider)
        {
            if (hit.distance < 7f)
            {
                if (hit.collider.CompareTag("car") || hit.collider.CompareTag("wall"))
                {
                    rf.car = hit.collider.CompareTag("car");
                    rf.wall = hit.collider.CompareTag("wall");
                    rf.hitdistance = hit.distance;
                }
            }
        }

        if (hit.distance >= 0)
        {
            rf.value = hit.distance / RAY_DIST;
            return rf;
        }
        else
        {
            rf.value = -1f;
            return rf;
        }
        //return hit.distance >= 0 ? hit.distance / RAY_DIST : -1f;
    }

    private int GetTrackIncrement()
    {
        int reward = 0;
        var carCenter = transform.position + Vector3.up;

        
        if (Physics.Raycast(carCenter, Vector3.down, out var hit, 2f))
        {
            var newHit = hit.transform;
            
            if (_track != null && newHit != _track)
            {
                float angle = Vector3.Angle(_track.forward, newHit.position - _track.position);
                reward = (angle < 90f) ? 1 : -1;
            }

            _track = newHit;
        }

        return reward;
    }

    public override void OnEpisodeBegin()
    {

        if (resetOnCollision) // Başa döndürme
        {
            transform.localPosition = startPosition;
            transform.localRotation = startDirection;
        }
    }

    private void OnCollisionEnter(Collision other)  // Çarpışma olursa Reward -1 diğer durumlarda 1
    {
        List<CarAgent> carList = this.transform.parent.gameObject.GetComponent<TrainingTrack>().carList;
        if (destroyAllCars)
        {
            if (other.gameObject.CompareTag("car") || other.gameObject.CompareTag("wall")) // Duvara çarparsa
            {
                foreach (CarAgent car in carList)
                {
                    car.restartCarPos(-1f);
                    carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                }

            }
            /*else if (other.gameObject.CompareTag("wall")) // Duvara çarparsa
            {
                foreach (CarAgent car in carList)
                {
                   car.restartCarPos(-1f);

                }
            }*/
        }
        else
        {
            if (other.gameObject.CompareTag("car")) // Duvara çarparsa
            {
                //Debug.Log(id + " touched to " + other.gameObject.GetComponent<CarAgent>().id);
                //if (other.gameObject != null)
                //{
                //    other.gameObject.GetComponent<CarAgent>().OnEpisodeBegin();

                //}

                restartCarPos(-1f);
            }
            else if (other.gameObject.CompareTag("wall")) // Duvara çarparsa
            {

                restartCarPos(-1f);

            }
        }

        if (carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].activeSelf == true &&
            carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].activeSelf == true &&
            carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].activeSelf == true &&
            carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].activeSelf == true)
        {
            carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
            carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
            carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
            carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
            carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
            carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
            carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
            carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
        }


        if (other.gameObject.name.Equals("10"))
        {
            foreach (var car in carList)
            {
                if (car.gameObject.transform.position.x > -5.49 && car.gameObject.transform.position.x < 15.47 &&
                    car.gameObject.transform.position.z > 5.1 && car.gameObject.transform.position.z < 24.86)
                {
                    coordinateControl = true;
                }
            }
            if (coordinateControl)
            {
                coordinateControl = false;
            }
            else
            {
                if (carRoad.GetComponent<TrackList>().roadList["6"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["6"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                }
                if (carRoad.GetComponent<TrackList>().roadList["20"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["20"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                }
                /*if (carRoad.GetComponent<TrackList>().roadList["34"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["34"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                }*/
            }
            //Debug.Log("isActive = " + isActive + "!isActive = "+ !isActive);

            //Debug.Log(roadList.Count);
        }
        if (other.gameObject.name.Equals("19"))
        {
            foreach (var car in carList)
            {
                if (car.gameObject.transform.position.x > -5.49 && car.gameObject.transform.position.x < 15.47 &&
                    car.gameObject.transform.position.z > 5.1 && car.gameObject.transform.position.z < 24.86)
                {
                    coordinateControl = true;
                }
            }
            if (coordinateControl)
            {
                coordinateControl = false;
            }
            else
            {
                if (carRoad.GetComponent<TrackList>().roadList["20"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["20"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                }
                if (carRoad.GetComponent<TrackList>().roadList["34"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["34"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                }
                /*if (carRoad.GetComponent<TrackList>().roadList["43"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["43"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                }*/
            }

            //Debug.Log(roadList.Count);
        }
        if (other.gameObject.name.Equals("30"))
        {
            foreach (var car in carList)
            {
                if (car.gameObject.transform.position.x > -5.49 && car.gameObject.transform.position.x < 15.47 &&
                    car.gameObject.transform.position.z > 5.1 && car.gameObject.transform.position.z < 24.86)
                {
                    coordinateControl = true;
                }
            }
            if (coordinateControl)
            {
                coordinateControl = false;
            }
            else
            {
                if (carRoad.GetComponent<TrackList>().roadList["34"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["34"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                }
                if (carRoad.GetComponent<TrackList>().roadList["43"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["43"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                }
                /*if (carRoad.GetComponent<TrackList>().roadList["6"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["6"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                }*/
            }


            //Debug.Log(roadList.Count);
        }
        if (other.gameObject.name.Equals("44"))
        {
            foreach (var car in carList)
            {
                if (car.gameObject.transform.position.x > -5.49 && car.gameObject.transform.position.x < 15.47 &&
                    car.gameObject.transform.position.z > 5.1 && car.gameObject.transform.position.z < 24.86)
                {
                    coordinateControl = true;
                }
            }
            if (coordinateControl)
            {
                coordinateControl = false;
            }
            else
            {
                if (carRoad.GetComponent<TrackList>().roadList["43"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["43"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                }
                if (carRoad.GetComponent<TrackList>().roadList["6"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["6"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                }
                /*if (carRoad.GetComponent<TrackList>().roadList["20"].activeSelf == true)
                {
                    carRoad.GetComponent<TrackList>().roadList["20"].SetActive(false);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(true);
                }
                else
                {
                    carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                    carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                }*/
            }
        }



        if (other.gameObject.name.Equals("6"))//Completed
        {
            stacks.Push(other.gameObject.name);
            try
            {
                if (stacks.Pop().Equals("6"))
                {

                    if (stacks.Peek().Equals("43") || stacks.Peek().Equals("20") || stacks.Peek().Equals("SecretRoad2"))
                    {
                        foreach (CarAgent car in carList)
                        {
                            car.restartCarPos(-1f);

                        }
                        carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                        stacks.Clear();
                    }
                    else
                    {
                        stacks.Push("6");
                    }
                }
                else
                {
                    stacks.Push("6");
                }
            }
            catch (Exception)
            {
            }
        }
        if (other.gameObject.name.Equals("20"))//Completed
        {
            stacks.Push(other.gameObject.name);
            try
            {
                if (stacks.Pop().Equals("20"))
                {
                    if (stacks.Peek().Equals("6") || stacks.Peek().Equals("34") || stacks.Peek().Equals("SecretRoad3"))
                    {
                        foreach (CarAgent car in carList)
                        {
                            car.restartCarPos(-1f);

                        }
                        carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                        stacks.Clear();
                    }
                    else
                    {
                        stacks.Push("20");
                    }
                }
                else
                {
                    stacks.Push("20");
                }
            }
            catch (Exception)
            {
            }
        }
        if (other.gameObject.name.Equals("34"))//Completed
        {
            stacks.Push(other.gameObject.name);
            try
            {
                if (stacks.Pop().Equals("34"))
                {
                    if (stacks.Peek().Equals("20") || stacks.Peek().Equals("43") || stacks.Peek().Equals("SecretRoad4"))
                    {
                        foreach (CarAgent car in carList)
                        {
                            car.restartCarPos(-1f);

                        }
                        carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                        stacks.Clear();
                    }
                    else
                    {
                        stacks.Push("34");
                    }
                }
                else
                {
                    stacks.Push("34");
                }
            }
            catch (Exception)
            {
            }
        }
        if (other.gameObject.name.Equals("43"))
        {
            stacks.Push(other.gameObject.name);
            try
            {
                if (stacks.Pop().Equals("43"))
                {
                    if (stacks.Peek().Equals("34") || stacks.Peek().Equals("6") || stacks.Peek().Equals("SecretRoad1"))
                    {
                        foreach (CarAgent car in carList)
                        {
                            car.restartCarPos(-1f);

                        }
                        carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                        stacks.Clear();
                    }
                    else
                    {
                        stacks.Push("43");
                    }
                }
                else
                {
                    stacks.Push("43");
                }
            }
            catch (Exception)
            {
            }
        }
        if (other.gameObject.name.Equals("SecretRoad1"))
        {
            stacks.Push(other.gameObject.name);
            try
            {
                if (stacks.Pop().Equals("SecretRoad1"))
                {
                    if (stacks.Peek().Equals("43") || stacks.Peek().Equals("20") || stacks.Peek().Equals("SecretRoad2"))
                    {
                        foreach (CarAgent car in carList)
                        {
                            car.restartCarPos(-1f);

                        }
                        carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                        stacks.Clear();
                    }
                    else
                    {
                        stacks.Push("SecretRoad1");
                    }
                }
                else
                {
                    stacks.Push("SecretRoad1");
                }
            }
            catch (Exception)
            {
            }
        }
        if (other.gameObject.name.Equals("SecretRoad2"))
        {
            stacks.Push(other.gameObject.name);
            try
            {
                if (stacks.Pop().Equals("SecretRoad2"))
                {
                    if (stacks.Peek().Equals("6") || stacks.Peek().Equals("SecretRoad3") || stacks.Peek().Equals("34"))
                    {
                        foreach (CarAgent car in carList)
                        {
                            car.restartCarPos(-1f);

                        }
                        carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                        stacks.Clear();
                    }
                    else
                    {
                        stacks.Push("SecretRoad2");
                    }
                }
                else
                {
                    stacks.Push("SecretRoad2");
                }
            }
            catch (Exception)
            {
            }
        }
        if (other.gameObject.name.Equals("SecretRoad3"))
        {
            stacks.Push(other.gameObject.name);
            try
            {
                if (stacks.Pop().Equals("SecretRoad3"))
                {
                    if (stacks.Peek().Equals("20") || stacks.Peek().Equals("SecretRoad4") || stacks.Peek().Equals("43"))
                    {
                        foreach (CarAgent car in carList)
                        {
                            car.restartCarPos(-1f);

                        }
                        carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                        stacks.Clear();
                    }
                    else
                    {
                        stacks.Push("SecretRoad3");
                    }
                }
                else
                {
                    stacks.Push("SecretRoad3");
                }
            }
            catch (Exception)
            {
            }
        }
        if (other.gameObject.name.Equals("SecretRoad4"))
        {
            stacks.Push(other.gameObject.name);
            try
            {
                if (stacks.Pop().Equals("SecretRoad4"))
                {
                    if (stacks.Peek().Equals("34") || stacks.Peek().Equals("SecretRoad1") || stacks.Peek().Equals("6"))
                    {
                        foreach (CarAgent car in carList)
                        {
                            car.restartCarPos(-1f);

                        }
                        carRoad.GetComponent<TrackList>().roadList["43"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad4"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["34"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad3"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["20"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad2"].SetActive(false);
                        carRoad.GetComponent<TrackList>().roadList["6"].SetActive(true);
                        carRoad.GetComponent<TrackList>().roadList["SecretRoad1"].SetActive(false);
                        stacks.Clear();
                    }
                    else
                    {
                        stacks.Push("SecretRoad4");
                    }
                }
                else
                {
                    stacks.Push("SecretRoad4");
                }
            }
            catch (Exception)
            {
            }
        }

    }

    public void restartCarPos(float score)
    {
        SetReward(score);
        EndEpisode();
    }
}
public class returnvalues
{
    public bool car { get; set; }
    public bool wall { get; set; }
    public float hitdistance { get; set; }
    public float value { get; set; }

}