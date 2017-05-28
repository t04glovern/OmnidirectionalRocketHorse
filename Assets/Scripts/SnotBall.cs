using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnotBall : MonoBehaviour {

    public int bounces = 2;

    private Rigidbody body;
    private ScoreController scoreController;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        scoreController = GameObject.Find("ScoreController").GetComponent<ScoreController>();
    }

    void OnCollisionEnter(Collision target)
    {
        if(target.gameObject.transform.position.y < transform.position.y)
        {
            bounces--;
        }

        if(bounces <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider target)
    {
        if(target.gameObject.tag == "Enemy")
        {
            scoreController.AddPoint();
            target.GetComponent<Explode>().OnExplode();
            Destroy(gameObject);
        }
    }
}
