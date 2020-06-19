using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{

    public float forceToAdd = 8;
    public int playerNumber;
    private Rigidbody2D rb;
    private ThrowHook th;


    // Start is called before the first frame update
    void Start()
    {
        forceToAdd = 8f;
        //gives it force
        rb = GetComponent<Rigidbody2D>();
        //rb.velocity = Vector2.up * 10;//gives the player a initial bump up to prevent falling immediately
        th = GetComponent<ThrowHook>();
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKey(KeyCode.A) && th.ropeActive)//for keyboard
        if(Input.GetAxis("LJoystickHorizontal") < -0.05f && th.ropeActive)
        {
            rb.AddForce(-Vector2.right * forceToAdd);
        }

        //if(Input.GetKey(KeyCode.D) && th.ropeActive)//for keyboard
        if(Input.GetAxis("LJoystickHorizontal") > 0.05f && th.ropeActive)
        {
            rb.AddForce(Vector2.right * forceToAdd);
        }

        //if(Input.GetKey(KeyCode.Space) && th.ropeActive)//for keyboard
        if(Input.GetButton("BButton") && th.ropeActive)
        {
            Debug.Log("retract hook");
        }
    }
}
