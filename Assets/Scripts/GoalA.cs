using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalA : MonoBehaviour
{
    public float particleDuration = 3f;
    public GameObject winParticles;
    public int playerNumber;

    // Start is called before the first frame update
    void Start()
    {
        winParticles.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(PackageDelivery());
            playerNumber = other.gameObject.GetComponent<PlayerScript>().playerNumber;//gets the dying player's player number to feed into the respawn function
            GameController.Instance.RespawnPlayer(playerNumber);
            GameController.Instance.teamAScore++;
            Destroy(other.gameObject);

        }

    }

   

    IEnumerator PackageDelivery()
    {
        TurnOnWinParticles();
        yield return new WaitForSeconds(particleDuration);
        TurnOffWinParticles();

    }

    void TurnOnWinParticles()
    {
        winParticles.SetActive(true);
    }

    void TurnOffWinParticles()
    {
        winParticles.SetActive(false);
    }
    
}
