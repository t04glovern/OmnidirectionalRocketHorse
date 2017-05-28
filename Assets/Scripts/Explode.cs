using UnityEngine;
using System.Collections;

public class Explode : MonoBehaviour {

    public Debris debris;
    public int totalDebris = 10;

    public void OnExplode()
    {
        if(debris != null)
        {
            var t = transform;

            for(int i = 0; i < totalDebris; i++)
            {
                t.TransformPoint(0, -100, 0);
                var clone = Instantiate(debris, t.position, Quaternion.identity) as Debris;
                var body = clone.GetComponent<Rigidbody>();
                body.AddForce(Vector3.right * Random.Range(-1000, 1000));
                body.AddForce(Vector3.up * Random.Range(500, 2000));
            }
        }

        Destroy(gameObject);
    }
}
