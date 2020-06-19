using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyStick : MonoBehaviour
{
    public bool isButton;
    public bool leftJoystick;
    public string buttonName;
    private Vector3 startPos;
    private Transform thisTransform;
    private SpriteRenderer sr;
    public Vector3 inputDirection;

    public float h;
    public float v;
    // Start is called before the first frame update
    void Start()
    {
        thisTransform = transform;
        startPos = thisTransform.position;
        sr = thisTransform.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isButton)
        {
            sr.enabled = Input.GetButton(buttonName);
        }
        else
        {

            h = Input.GetAxis("LJoystickHorizontal");
            v = Input.GetAxis("LJoystickVertical");
            inputDirection = new Vector3(h, v, 0);
            thisTransform.position = startPos + inputDirection;
        }
    }
}
