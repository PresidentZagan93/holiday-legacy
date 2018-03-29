using UnityEngine;
using System.Collections;

public class PausableObject : MonoBehaviour, IRefreshable {
    
    new Rigidbody2D rigidbody;
    bool lastPaused;
    GunShooter gunShooter;

    bool wasKinematic;
    bool originalKinematic;
    Vector2 lastVelocity;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        if(rigidbody)
        {
            originalKinematic = rigidbody.isKinematic;
        }
        gunShooter = GetComponent<GunShooter>();

        ObjectManager.Add(this);
    }

    public int GetValue()
    {
        return 0;
    }

    void OnDestroy()
    {
        ObjectManager.Remove(this);
    }

    void OnEnable()
    {
        lastPaused = GameManager.Paused;

        if(rigidbody)
        {
            rigidbody.isKinematic = originalKinematic;
        }
    }

    public void Refresh()
    {
        lastPaused = !GameManager.Paused;
    }

    void Update()
    {
        if (lastPaused != GameManager.Paused)
        {
            lastPaused = GameManager.Paused;

            if (lastPaused)
            {
                if (rigidbody)
                {
                    wasKinematic = rigidbody.isKinematic;
                    lastVelocity = rigidbody.velocity;
                    rigidbody.isKinematic = true;
                    rigidbody.velocity = Vector2.zero;
                }
                if(gunShooter)
                {
                    gunShooter.enabled = false;
                }
            }
            else
            {
                if (rigidbody)
                {
                    rigidbody.isKinematic = wasKinematic;

                    if(rigidbody.bodyType != RigidbodyType2D.Static)
                    {
                        rigidbody.velocity = lastVelocity;
                    }
                }
                if (gunShooter)
                {
                    gunShooter.enabled = true;
                }
            }
        }
    }
}
