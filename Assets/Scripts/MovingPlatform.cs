using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform movingPlatform;
    public float moveSpeed;
    public Transform position1;
    public Transform position2;
    public Vector3 newPosition;
    public string currentState;
    public float resetTime;

    // Start is called before the first frame update
    void Start()
    {
        ChangeTarget();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        movingPlatform.position = Vector3.Lerp(movingPlatform.position, newPosition, moveSpeed * Time.deltaTime);
    }

    void ChangeTarget()
    {
        if(currentState == "Moving To Position 1")
        {
            currentState = "Moving To Position 2";
            newPosition = position2.position;
        }
        else if(currentState == "Moving To Position 2")
        {
            currentState = "Moving To Position 1";
            newPosition = position1.position;
        }
        else if(currentState == "")
        {
            currentState = "Moving To Position 2";
            newPosition = position2.position;
        }
        Invoke("ChangeTarget", resetTime);
    }
}
