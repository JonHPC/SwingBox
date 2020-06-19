using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RopeSystem : MonoBehaviour
{
    // 1
    //You’ll use these variables to keep track of the different components the RopeSystem script will interact with.
    public GameObject ropeHingeAnchor;
    public DistanceJoint2D ropeJoint;
    public Transform crosshair;
    public SpriteRenderer crosshairSprite;
    public PlayerMovement playerMovement;

    private bool ropeAttached;
    private Vector2 playerPosition;
    private Rigidbody2D ropeHingeAnchorRb;
    private SpriteRenderer ropeHingeAnchorSprite;

    public LineRenderer ropeRenderer;
    public LayerMask ropeLayerMask;
    private float ropeMaxCastDistance = 20f;
    private List<Vector2> ropePositions = new List<Vector2>();

    private bool distanceSet;
    private Dictionary<Vector2, int> wrapPointsLookup = new Dictionary<Vector2, int>();

    public float climbSpeed = 3f;
    private bool isColliding;



    void Awake()
    {
        // 2
        //The Awake method will run when the game starts and disables the ropeJoint (DistanceJoint2D component). It'll also set playerPosition to the current position of the Player.
        ropeJoint.enabled = false;
        playerPosition = transform.position;
        ropeHingeAnchorRb = ropeHingeAnchor.GetComponent<Rigidbody2D>();
        ropeHingeAnchorSprite = ropeHingeAnchor.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 3
        //This is the most important part of your main Update() loop. First, you capture the world position of the mouse cursor using the camera's ScreenToWorldPoint method.
        //You then calculate the facing direction by subtracting the player's position from the mouse position in the world. 
        //You then use this to create aimAngle, which is a representation of the aiming angle of the mouse cursor. The value is kept positive in the if-statement.
        var worldMousePosition =
            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
        var facingDirection = worldMousePosition - transform.position;
        var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
        if (aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        // 4
        //The aimDirection is a rotation for later use. You're only interested in the Z value, as you're using a 2D camera, and this is the only relevant axis. 
        //You pass in the aimAngle * Mathf.Rad2Deg which converts the radian angle to an angle in degrees.
        var aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;

        // 5
        //The player position is tracked using a convenient variable to save you from referring to transform.Position all the time.
        playerPosition = transform.position;

        // 6
        // Lastly, this is an if..else statement you'll soon use to determine if the rope is attached to an anchor point.
        if (!ropeAttached)
        {
            SetCrosshairPosition(aimAngle);
            playerMovement.isSwinging = false;

        }
        else
        {
            playerMovement.isSwinging = true;
            playerMovement.ropeHook = ropePositions.Last();
            crosshairSprite.enabled = false;
            // 1
            //If the ropePositions list has any positions stored, then...
            if (ropePositions.Count > 0)
            {
                // 2
                //Fire a raycast out from the player's position, in the direction of the player looking at the last rope position in the list 
                //— the pivot point where the grappling hook is hooked into the rock — with a raycast distance set to the distance between the player and rope pivot position.
                var lastRopePoint = ropePositions.Last();
                var playerToCurrentNextHit = Physics2D.Raycast(playerPosition, (lastRopePoint - playerPosition).normalized, Vector2.Distance(playerPosition, lastRopePoint) - 0.1f, ropeLayerMask);

                // 3
                //If the raycast hits something, then that hit object's collider is safe cast to a PolygonCollider2D. 
                //As long as it's a real PolygonCollider2D, then the closest vertex position on that collider is returned as a Vector2, using that handy-dandy method you wrote earlier.
                if (playerToCurrentNextHit)
                {
                    var colliderWithVertices = playerToCurrentNextHit.collider as PolygonCollider2D;
                    if (colliderWithVertices != null)
                    {
                        var closestPointToHit = GetClosestColliderPointFromRaycastHit(playerToCurrentNextHit, colliderWithVertices);

                        // 4
                        //The wrapPointsLookup is checked to make sure the same position is not being wrapped again. If it is, then it'll reset the rope and cut it, dropping the player.
                        if (wrapPointsLookup.ContainsKey(closestPointToHit))
                        {
                            ResetRope();
                            return;
                        }

                        // 5
                        //The ropePositions list is now updated, adding the position the rope should wrap around, and the wrapPointsLookup dictionary is also updated. 
                        //Lastly the distanceSet flag is disabled, so that UpdateRopePositions() method can re-configure the rope's distances to take into account the new rope length and segments.
                        ropePositions.Add(closestPointToHit);
                        wrapPointsLookup.Add(closestPointToHit, 0);
                        distanceSet = false;
                    }
                }
            }

        }

        HandleInput(aimDirection);
        UpdateRopePositions();
        HandleRopeLength();
        HandleRopeUnwrap();


    }

    private void SetCrosshairPosition(float aimAngle)
    {
        if (!crosshairSprite.enabled)
        {
            crosshairSprite.enabled = true;
        }

        var x = transform.position.x + 1f * Mathf.Cos(aimAngle);
        var y = transform.position.y + 1f * Mathf.Sin(aimAngle);

        var crossHairPosition = new Vector3(x, y, 0);
        crosshair.transform.position = crossHairPosition;
    }

    // 1
    //HandleInput is called from the Update() loop, and simply polls for input from the left and right mouse buttons.
    private void HandleInput(Vector2 aimDirection)
    {
        if (Input.GetMouseButton(0))
        {
            // 2
            //When a left mouse click is registered, the rope line renderer is enabled and a 2D raycast is fired out from the player position in the aiming direction.
            //A maximum distance is specified so that the grappling hook can't be fired in infinite distance, 
            //and a custom mask is applied so that you can specify which physics layers the raycast is able to hit.
            if (ropeAttached) return;
            ropeRenderer.enabled = true;

            var hit = Physics2D.Raycast(playerPosition, aimDirection, ropeMaxCastDistance, ropeLayerMask);

            // 3
            //If a valid raycast hit is found, ropeAttached is set to true, and a check is done on the list of rope vertex positions to make sure the point hit isn't in there already.
            if (hit.collider != null)
            {
                ropeAttached = true;
                if (!ropePositions.Contains(hit.point))
                {
                    // 4
                    // Jump slightly to distance the player a little from the ground after grappling to something.
                    //Provided the above check is true, then a small impulse force is added to the slug to hop him up off the ground, and the ropeJoint(DistanceJoint2D) is enabled, 
                    //and set with a distance equal to the distance between the slug and the raycast hitpoint. The anchor sprite is also enabled.
                    transform.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 2f), ForceMode2D.Impulse);
                    ropePositions.Add(hit.point);
                    ropeJoint.distance = Vector2.Distance(playerPosition, hit.point);
                    ropeJoint.enabled = true;
                    ropeHingeAnchorSprite.enabled = true;
                }
            }
            // 5
            //If the raycast doesn't hit anything, then the rope line renderer and rope joint are disabled, and the ropeAttached flag is set to false.
            else
            {
                ropeRenderer.enabled = false;
                ropeAttached = false;
                ropeJoint.enabled = false;
            }
        }

        if (Input.GetMouseButton(1))
        {
            ResetRope();
        }
    }

    // 6
    //If the right mouse button is clicked, the ResetRope() method is called, which will disable 
    //and reset all rope/grappling hook related parameters to what they should be when the grappling hook is not being used.
    private void ResetRope()
    {
        ropeJoint.enabled = false;
        ropeAttached = false;
        playerMovement.isSwinging = false;
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, transform.position);
        ropePositions.Clear();
        ropeHingeAnchorSprite.enabled = false;
        wrapPointsLookup.Clear();

    }

    private void UpdateRopePositions()
    {
        // 1
        //Return out of this method if the rope isn't actually attached.
        if (!ropeAttached)
        {
            return;
        }

        // 2
        //Set the rope's line renderer vertex count (positions) to whatever number of positions are stored in ropePositions, plus 1 more (for the player's position).
        ropeRenderer.positionCount = ropePositions.Count + 1;

        // 3
        //Loop backwards through the ropePositions list, and for every position (except the last position), 
        //set the line renderer vertex position to the Vector2 position stored at the current index being looped through in ropePositions.
        for (var i = ropeRenderer.positionCount - 1; i >= 0; i--)
        {
            if (i != ropeRenderer.positionCount - 1) // if not the Last point of line renderer
            {
                ropeRenderer.SetPosition(i, ropePositions[i]);

                // 4
                //Set the rope anchor to the second-to-last rope position where the current hinge/anchor should be, or if there is only one rope position, then set that one to be the anchor point. 
                //This configures the ropeJoint distance to the distance between the player and the current rope position being looped over.
                if (i == ropePositions.Count - 1 || ropePositions.Count == 1)
                {
                    var ropePosition = ropePositions[ropePositions.Count - 1];
                    if (ropePositions.Count == 1)
                    {
                        ropeHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                    else
                    {
                        ropeHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                }
                // 5
                //This if-statement handles the case where the rope position being looped over is the second-to-last one; 
                //that is, the point at which the rope connects to an object, a.k.a. the current hinge/anchor point.
                else if (i - 1 == ropePositions.IndexOf(ropePositions.Last()))
                {
                    var ropePosition = ropePositions.Last();
                    ropeHingeAnchorRb.transform.position = ropePosition;
                    if (!distanceSet)
                    {
                        ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                        distanceSet = true;
                    }
                }
            }
            else
            {
                // 6
                //This else block handles setting the rope's last vertex position to the player's current position.
                ropeRenderer.SetPosition(i, transform.position);
            }
        }
    }

    // 1
    //This method takes in two parameters, a RaycastHit2D object, and a PolygonCollider2D. 
    //All the rocks in the level have PolygonCollider2D colliders, so this will work well as long as you're always using PolygonCollider2D shapes.
    private Vector2 GetClosestColliderPointFromRaycastHit(RaycastHit2D hit, PolygonCollider2D polyCollider)
    {
        // 2
        /* Here be LINQ query magic! This converts the polygon collider's collection of points, into a dictionary of Vector2 positions (the value of each dictionary entry is the position itself),
          and the key of each entry, is set to the distance that this point is to the player's position (float value). 
          Something else happens here: the resulting position is transformed into world space (by default a collider's vertex positions are stored in local space 
          - i.e. local to the object the collider sits on, and we want the world space positions).
        */
        var distanceDictionary = polyCollider.points.ToDictionary<Vector2, float, Vector2>(
            position => Vector2.Distance(hit.point, polyCollider.transform.TransformPoint(position)),
            position => polyCollider.transform.TransformPoint(position));

        // 3
        //The dictionary is ordered by key. In other words, the distance closest to the player's current position,
        //and the closest one is returned, meaning that whichever point is returned from this method, 
        //is the point on the collider between the player and the current hinge point on the rope!
        var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);
        return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
    }


    private void HandleRopeLength()
    {
        // 1
        //if (Input.GetAxis("Vertical") >= 1f && ropeAttached && !isColliding)
        if (Input.GetAxis("Vertical") >= 1f && ropeAttached)
        {
            ropeJoint.distance -= Time.deltaTime * climbSpeed;
        }
        else if (Input.GetAxis("Vertical") < 0f && ropeAttached && ropeJoint.distance <= ropeMaxCastDistance)
        {
            ropeJoint.distance += Time.deltaTime * climbSpeed;
        }

        if(Input.GetKey(KeyCode.W) && ropeAttached && !isColliding)
        {
            //ropeJoint.distance -= Time.deltaTime * climbSpeed;
        }
    }

    void OnTriggerStay2D(Collider2D colliderStay)
    {
        isColliding = true;
    }

    private void OnTriggerExit2D(Collider2D colliderOnExit)
    {
        isColliding = false;
    }

    private void HandleRopeUnwrap()
    {
        if (ropePositions.Count <= 1)
        {
            return;
        }

        // Hinge = next point up from the player position
        // Anchor = next point up from the Hinge
        // Hinge Angle = Angle between anchor and hinge
        // Player Angle = Angle between anchor and player

        // 1
        //anchorIndex is the index in the ropePositions collection two positions from the end of the collection. 
        //You can look at this as two positions in the rope back from the slug's position. 
        //In the image below, this happens to be the grappling hook's first hook point into the terrain.
        //As the ropePositions collection fills with more wrap points, this point will always be the wrap point two positions away from the slug.
        var anchorIndex = ropePositions.Count - 2;

        // 2
        //hingeIndex is the index in the collection where the current hinge point is stored; 
        //in other words, the position where the rope is currently wrapping around a point closest to the 'slug' end of the rope.
        //It’s always one position away from the slug, which is why you use ropePositions.Count - 1.
        var hingeIndex = ropePositions.Count - 1;

        // 3
        //anchorPosition is calculated by referencing the anchorIndex location in the ropePositions collection, and is simply a Vector2 value of that position.
        var anchorPosition = ropePositions[anchorIndex];

        // 4
        //hingePosition is calculated by referencing the hingeIndex location in the ropePositions collection, and is simply a Vector2 value of that position.
        var hingePosition = ropePositions[hingeIndex];

        // 5
        //hingeDir a vector that points from the anchorPosition to the hingePosition. It is used in the next variable to work out an angle.
        var hingeDir = hingePosition - anchorPosition;

        // 6
        //hingeAngle is where the ever useful Vector2.Angle() helper function is used to calculate the angle between anchorPosition and the hinge point.
        var hingeAngle = Vector2.Angle(anchorPosition, hingeDir);

        // 7
        //playerDir is the vector that points from anchorPosition to the current position of the slug (playerPosition)
        var playerDir = playerPosition - anchorPosition;

        // 8
        //playerAngle is then calculated by getting the angle between the anchor point and the player (slug).
        var playerAngle = Vector2.Angle(anchorPosition, playerDir);

        if (!wrapPointsLookup.ContainsKey(hingePosition))
        {
            Debug.LogError("We were not tracking hingePosition (" + hingePosition + ") in the look up dictionary.");
            return;
        }


        if (playerAngle < hingeAngle)
        {
            // 1
            //If the current closest wrap point to the slug has a value of 1 at the point where playerAngle < hingeAngle then unwrap that point, and return so that the rest of the method is not handled.
            if (wrapPointsLookup[hingePosition] == 1)
            {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }

            // 2
            //Otherwise, if the wrap point was not last marked with a value of 1, but playerAngle is less than the hingeAngle, the value is set to -1 instead.
            wrapPointsLookup[hingePosition] = -1;
        }
        else
        {
            // 3
            //If the current closest wrap point to the slug has a value of -1 at the point where playerAngle > hingeAngle, unwrap the point and return.
            if (wrapPointsLookup[hingePosition] == -1)
            {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }

            // 4
            //Otherwise, set the wrap point dictionary entry value at the hinge position to 1.

            wrapPointsLookup[hingePosition] = 1;
        }

    }

    private void UnwrapRopePosition(int anchorIndex, int hingeIndex)
    {
        // 1
        //The current anchor index (the second rope position away from the slug) becomes the new hinge position 
        //and the old hinge position is removed (the one that was previously closest to the slug that we are now 'unwrapping').
        //The newAnchorPosition variable is set to the anchorIndex value in the rope positions list. 
        //This will be used to position the updated anchor position next.
        var newAnchorPosition = ropePositions[anchorIndex];
        wrapPointsLookup.Remove(ropePositions[hingeIndex]);
        ropePositions.RemoveAt(hingeIndex);

        // 2
        //The rope hinge RigidBody2D (which is what the rope's DistanceJoint2D is attached to) has its position changed here to the new anchor position.
        //This allows the seamless continued movement of the slug on his rope as he is connected to the DistanceJoint2D,
        //and this joint should allow him to continue swinging based off the new position he is anchored to — in other words, the next point down the rope from his position.
        ropeHingeAnchorRb.transform.position = newAnchorPosition;
        distanceSet = false;

        // Set new rope distance joint distance for anchor position if not yet set.
        //Next, the distance joint's distance value needs to be updated to account for the sudden change in distance of the slug to the new anchor point. 
        //A quick check against the distanceSet flag ensures that this is done, if not already done,
        //and the distance is set based on calculated the distance between the slug and the new anchor position.
        if (distanceSet)
        {
            return;
        }
        ropeJoint.distance = Vector2.Distance(transform.position, newAnchorPosition);
        distanceSet = true;

    }




}
