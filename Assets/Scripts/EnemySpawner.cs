using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

    public GameObject enemyPrefab;
    public float spawnTime = 3f;

	void Start() 
    {
        InvokeRepeating ("SpawnEnemy", spawnTime, spawnTime);
	}

    void SpawnEnemy()
    {
        GameObject.Instantiate(enemyPrefab, transform.position, transform.rotation);
    }
}
