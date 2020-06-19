using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    private static GameController _instance;

    public static GameController Instance { get { return _instance; }}

    public GameObject[] players;
    public Transform[] spawnPoints;
    public float RespawnTime = 5.0f;

    public GameObject spawnParticles;

    public int teamAScore;
    public int teamBScore;
    public TextMeshProUGUI teamAScoreText;
    public TextMeshProUGUI teamBScoreText;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        teamAScore = 0;
        teamBScore = 0;

        //spawn the players
        for (int i = 0; i < players.Length; i++)
        {
            GameObject go = Instantiate(players[i], spawnPoints[i].position, Quaternion.identity) as GameObject;
            GameObject particles = Instantiate(spawnParticles, spawnPoints[i].position, Quaternion.identity) as GameObject;
            StartCoroutine(DestroySpawnParticles(particles));
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateScore();
    }

    public void RespawnPlayer(int playerNumber)
    {
        StartCoroutine(RespawnDelay(playerNumber));
    }


     IEnumerator RespawnDelay(int playerNumber)
    {
        yield return new WaitForSeconds(RespawnTime);
        var go = Instantiate(players[playerNumber], spawnPoints[playerNumber].position, Quaternion.identity) as GameObject;

        var particles = Instantiate(spawnParticles, spawnPoints[playerNumber].position, Quaternion.identity) as GameObject;
        StartCoroutine(DestroySpawnParticles(particles));
    }

    IEnumerator DestroySpawnParticles(GameObject particles)
    {
        yield return new WaitForSeconds(1f);
        Destroy(particles);
    }

    public void UpdateScore()
    {
        teamAScoreText.text = teamAScore.ToString();
        teamBScoreText.text = teamBScore.ToString();

    }
}
