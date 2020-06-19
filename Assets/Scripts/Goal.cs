using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public float timeToDeliver = 3f;
    public GameObject winParticles;
    public float currentTime;
    public float completionTime = 4f;
    public float completionPercentage = 0f;
    public bool onGoal;

    // Start is called before the first frame update
    void Start()
    {
        winParticles.SetActive(false);
        onGoal = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(onGoal == true)
        {
            currentTime += Time.deltaTime;
        }
        else
        {
            currentTime = 0f;
        }

        completionPercentage = currentTime / completionTime * 100f;

        if(completionPercentage >= 100f)
        {
            TurnOnWinParticles();
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            //StartCoroutine(PackageDelivery());
            onGoal = true;
        }

    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            //StopCoroutine(PackageDelivery());
            onGoal = false;
           // Debug.Log("Package not delivered yet!");
        }

    }

    IEnumerator PackageDelivery()
    {
        Debug.Log("Delivering...");
        yield return new WaitForSeconds(timeToDeliver);
        Debug.Log("Package Delivered!");
        TurnOnWinParticles();
        
    }

    void TurnOnWinParticles()
    {
        winParticles.SetActive(true);
    }
}
