using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathWall : MonoBehaviour
{
    public GameObject deathParticles;
    public float angle;
    private int playerNumber;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        playerNumber = other.gameObject.GetComponent<PlayerScript>().playerNumber;//gets the dying player's player number to feed into the respawn function
        GameController.Instance.RespawnPlayer(playerNumber);
        StartCoroutine(SpawnDeathParticles(other.gameObject.transform));
        Destroy(other.gameObject);
    }

    IEnumerator SpawnDeathParticles(Transform deathTransform)
    {
        var particles = Instantiate(deathParticles, deathTransform.position, Quaternion.Euler(0f, 0f, angle)) as GameObject;
        yield return new WaitForSeconds(2.0f);
        Destroy(particles);

    }
}
