using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThrowHook : MonoBehaviour
{

    public GameObject hook;
    public bool ropeActive;
    GameObject curHook;

    public SpriteRenderer crosshairSprite;
    public Transform crosshair;
    private Vector2 playerPosition;
    public bool ropeAttached;

    public LayerMask ropeLayerMask;
    public float ropeMaxCastDistance = 5f;

    public Vector3 joystickDir;
    public float h;
    public float v;


    void Awake()
    {
        playerPosition = transform.position;

    }

   

    // Update is called once per frame
    void Update()
    {
        h = Input.GetAxis("LJoystickHorizontal");
        v = Input.GetAxis("LJoystickVertical");

        joystickDir = new Vector3(h, v, 0f);


        //var worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)); //for mouse
        //var facingDirection = worldMousePosition - transform.position; //for mouse
        //var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);//for mouse

        var aimAngle = Mathf.Atan2(-joystickDir.y, joystickDir.x);
        if(aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        var aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;

        playerPosition = transform.position;

        if(!ropeAttached)
        {
            SetCrosshairPosition(aimAngle);
        }
        else
        {
            crosshairSprite.enabled = false;
        }




        /*if(Input.GetMouseButtonDown(0))
        {
            if(ropeActive == false)
            {
                Vector2 destiny = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                curHook = Instantiate(hook, transform.position, Quaternion.identity) as GameObject;

                curHook.GetComponent<RopeScript>().destiny = destiny;

                ropeActive = true;
            }
            else
            {
                //delete the rope
               
                Destroy(curHook);
                ropeActive = false;
            }

        }*/


        HandleInput(aimDirection);
    }

    private void SetCrosshairPosition(float aimAngle)
    {
        if(!crosshairSprite.enabled)
        {
            crosshairSprite.enabled = true;
        }

        var x = transform.position.x + 1f * Mathf.Cos(aimAngle);
        var y = transform.position.y + 1f * Mathf.Sin(aimAngle);

        var crossHairPosition = new Vector3(x, y, 0);
        crosshair.transform.position = crossHairPosition;
    }

    private void HandleInput(Vector2 aimDirection)
    {
        //if(Input.GetMouseButtonDown(0) && ropeActive == false)//for mouse
        if(Input.GetButtonDown("AButton") && ropeActive == false)
        {
            if (ropeAttached) return;
           
            var hit = Physics2D.Raycast(playerPosition, aimDirection, ropeMaxCastDistance, ropeLayerMask);
            Debug.DrawRay(playerPosition, aimDirection, Color.green);

            if (hit.collider != null)
            {
                ropeAttached = true;
                //attach a rope now to where the raycast collided
                //Debug.Log("rope hit and attached");
                if (ropeActive == false)
                {
                    //Vector2 destiny = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 destiny = new Vector2(hit.point.x, hit.point.y);

                    curHook = Instantiate(hook, transform.position, Quaternion.identity) as GameObject;

                    curHook.GetComponent<RopeScript>().destiny = destiny;

                    ropeActive = true;
                    
                }
            }
            else
            {
                ropeAttached = false;
                //rope is not attached to anything
                //Debug.Log("Rope missed");

                //I stil want to shoot the rope at the aim location and get destroyed after reaching full length
                Ray r = new Ray(playerPosition, new Vector3(aimDirection.x, aimDirection.y, 0));
                //print(r.GetPoint(ropeMaxCastDistance));

                Vector2 destiny = new Vector2(r.GetPoint(ropeMaxCastDistance).x, r.GetPoint(ropeMaxCastDistance).y);
                curHook = Instantiate(hook, transform.position, Quaternion.identity) as GameObject;

                curHook.GetComponent<RopeScript>().destiny = destiny;

                ropeActive = true;



                StartCoroutine(MissedHook(curHook));
            }

           


        }
        //else if(Input.GetMouseButtonDown(0) && ropeActive == true)//for mouse
        else if (Input.GetButtonDown("AButton") && ropeActive == true)
        {
            //delete the rope and turn the cursor back on
            Destroy(curHook);
            ropeActive = false;
            ropeAttached = false;

        }
        

    }

    IEnumerator MissedHook(GameObject missed)
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(missed);
        ropeActive = false;
    }


}
