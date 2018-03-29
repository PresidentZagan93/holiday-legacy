using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repel : MonoBehaviour {

    List<Collider2D> collisions = new List<Collider2D>();
    public float repelPower = 1f;

    public bool ignoreProjectiles = true;

    private void Awake()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if(!collider)
        {
            Debug.LogWarning("(Repel.cs) The object " + name + " doesnt have a Collider2D component (not harmful).");
            enabled = false;
            return;
        }

        collider.isTrigger = true;

        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        if(!rigidbody)
        {
            rigidbody = gameObject.AddComponent<Rigidbody2D>();
        }

        rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rigidbody.isKinematic = true;
        
        if(ignoreProjectiles)
        {
            gameObject.SetLayer("PropGhost");
        }
        else
        {
            gameObject.SetLayer("Prop");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collisions.Contains(collision))
        {
            collisions.Add(collision);
        }

        if(collisions.Count > 0)
        {
            enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collisions.Contains(collision))
        {
            collisions.Remove(collision);
        }

        if (collisions.Count == 0)
        {
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (collisions.Count == 0) enabled = false;

        for(int i = 0; i < collisions.Count;i++)
        {
            if(collisions[i])
            {
                Vector2 dir = collisions[i].transform.position - transform.position;
                dir.Normalize();

                collisions[i].transform.position += (Vector3)dir * repelPower * 0.3f;
            }
        }
    }
}
