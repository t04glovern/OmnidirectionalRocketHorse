using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalker : MonoBehaviour {

    Transform target;
    public float walkingSpeed = 2f;

	void Update() 
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        transform.LookAt(target);
        transform.Translate(Vector3.forward * walkingSpeed * Time.deltaTime);
	}

    void OnTriggerEnter(Collider target)
    {
        if(target.gameObject.tag == "Player")
        {
            Destroy(target.gameObject);
            Application.LoadLevel("Menu");
        }
    }
}
