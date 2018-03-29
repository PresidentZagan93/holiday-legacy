using UnityEngine;
using System.Collections;

public class CollisionChecker : MonoBehaviour {

    public GameObject collidedObject;

    void OnTriggerEnter(Collider col)
    {
        collidedObject = col.gameObject;
    }
    void OnTriggerExit(Collider col)
    {
        collidedObject = null;
    }
}
