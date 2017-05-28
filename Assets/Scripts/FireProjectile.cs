using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProjectile : MonoBehaviour {

    public float shootDelay = .5f;
    public GameObject projectilePrefab;
    public Vector2 firePosition;

    private float timeElapsed = 0f;

    public void Fire(bool facingRight, Vector3 position) 
    {
        firePosition = new Vector2(position.x, position.y);

        if(projectilePrefab != null)
        {
            if(timeElapsed > shootDelay)
            {
                CreateProjectile(firePosition, facingRight);
                timeElapsed = 0;
            }

            timeElapsed += Time.deltaTime;
        }
    }

    public void CreateProjectile(Vector2 pos, bool facingRight)
    {
        var clone = Instantiate(projectilePrefab, pos, Quaternion.identity) as GameObject;
        clone.transform.localScale = transform.localScale;

        if(facingRight)
            clone.GetComponent<Rigidbody>().AddForce(new Vector3(1000, -600, 0));
        else
            clone.GetComponent<Rigidbody>().AddForce(new Vector3(-1000, -600, 0));
    }
}
