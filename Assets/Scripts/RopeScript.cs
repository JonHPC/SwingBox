using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeScript : MonoBehaviour
{

    public Vector2 destiny;
    public float speed = 1;
    public float distance = 2;
    public GameObject nodePrefab;
    public GameObject player;
    public GameObject lastNode;
    bool done = false;
    public List<GameObject> Nodes = new List<GameObject>();
    int vertexCount = 2;//initial points between hook and player
    public LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        lastNode = transform.gameObject;
        Nodes.Add(transform.gameObject);//adds the hook as the first object of the Nodes list
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //shoots the hook to the destination point
        transform.position = Vector2.MoveTowards(transform.position, destiny, speed);

        if((Vector2)transform.position != destiny)//if the hook hasnt reached the destination yet...
        {
            if(Vector2.Distance(player.transform.position, lastNode.transform.position) > distance)//check the distance between the current hook position compared to the specified distance
            {
                //Instantiate node prefabs until the hook reaches the destination
                CreateNode();
            }
        }

        else if(done == false)
        {
            done = true; //makes sure player only connects to rope once

            while(Vector2.Distance(player.transform.position, lastNode.transform.position) > distance)
            {
                CreateNode();//create nodes just in case the hook gets to the destination too fast
            }
            lastNode.GetComponent<HingeJoint2D>().connectedBody = player.GetComponent<Rigidbody2D>();
        }

        RenderLine();
    }

    void CreateNode()
    {
        Vector2 pos2Create = player.transform.position - lastNode.transform.position;
        pos2Create.Normalize();
        pos2Create *= distance;
        pos2Create += (Vector2)lastNode.transform.position;

        GameObject go = Instantiate(nodePrefab, pos2Create, Quaternion.identity) as GameObject;

        go.transform.SetParent(transform);//connects the node to the hook

        lastNode.GetComponent<HingeJoint2D>().connectedBody = go.GetComponent<Rigidbody2D>();

        lastNode = go;

        Nodes.Add(lastNode); //adds the created node to the list

        vertexCount++;//adds one to the vertex count
    }

    void RenderLine()
    {
        //iterate the whole node list and render 
        lr.positionCount = vertexCount;//sets the line renderer positioncount to the vertext count

        int i;
        for (i = 0; i < Nodes.Count; i++)
        {
            lr.SetPosition(i, Nodes[i].transform.position);

        }

        lr.SetPosition(i, player.transform.position);
    }

    void DestroyNode()
    {

    }


}
