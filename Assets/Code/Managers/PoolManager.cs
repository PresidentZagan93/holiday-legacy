using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour {

    public int defaultMax = 50;
    [System.Serializable]
    public struct PoolItem
    {
        public GameObject gameObject;
        public int max;

        public PoolItem(GameObject gameObject, int max)
        {
            this.gameObject = gameObject;
            this.max = max;
        }
    }
    [System.Serializable]
    public struct QueuedPool
    {
        public GameObject gameObject;
        public float destroyAt;

        public QueuedPool(GameObject obj)
        {
            gameObject = obj;
            destroyAt = 0f;
        }
        public QueuedPool(GameObject obj, float destroyDelay)
        {
            gameObject = obj;
            destroyAt = Time.time + destroyDelay;
        }
    }

    public List<PoolItem> poolItems = new List<PoolItem>();
    public List<GameObject> pooledObjects = new List<GameObject>();
    public List<QueuedPool> queue = new List<QueuedPool>();

    void Awake()
    {
        singleton = this;
    }
    
    float nextCreatePool;

    IEnumerator CreatePool(PoolItem item)
    {
        for (int i = 0; i < item.max;i++)
        {
            yield return null;
            GameObject newObj = Instantiate(item.gameObject) as GameObject;
            newObj.name = item.gameObject.name;
            newObj.transform.SetParent(transform);
            pooledObjects.Add(newObj);

            SetObjectState(newObj, false);
        }
    }

    GameObject ObjectFromPool(string objectName)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if(pooledObjects[i])
            {
                if (pooledObjects[i].name == objectName)
                {
                    if (!pooledObjects[i].activeInHierarchy)
                    {
                        return pooledObjects[i];
                    }
                }
            }
            else
            {
                pooledObjects.RemoveAt(i);
            }
        }
        return null;
    }

    void SetObjectState(GameObject obj, bool state)
    {
        if(!obj)
        {
            return;
        }

        obj.SetActive(state);
        MonoBehaviour[] comps = obj.GetComponentsInChildren<MonoBehaviour>();
        for (int i = 0; i < comps.Length;i++)
        {
            comps[i].enabled = state;
        }
    }

    GameObject Inst(GameObject objectToSpawn, Vector3 position, Quaternion rotation)
    {
        if(!objectToSpawn)
        {
            return null;
        }
        for (int i = 0; i < poolItems.Count; i++)
        {
            if (poolItems[i].gameObject.name == objectToSpawn.name)
            {
                GameObject objSpawned = ObjectFromPool(objectToSpawn.name);
                if (objSpawned != null)
                {
                    objSpawned.transform.position = position;
                    objSpawned.transform.rotation = rotation;
                    SetObjectState(objSpawned, true);
                }
                else
                {
                    GameObject newObj = Instantiate(objectToSpawn, position, rotation) as GameObject;
                    newObj.name = objectToSpawn.name;
                    newObj.transform.SetParent(transform);
                    pooledObjects.Add(newObj);

                    objSpawned = newObj;
                }
                return objSpawned;
            }
        }
        PoolItem newPoolItem = new PoolItem(objectToSpawn, defaultMax);
        poolItems.Add(newPoolItem);
        CreatePool(newPoolItem);
        return null;
    }

    public static GameObject PoolInstantiate(string objectToSpawn, Vector3 position, Quaternion rotation)
    {
        if (!singleton) singleton = FindObjectOfType<PoolManager>();
        GameObject objToSpawn = null;
        for (int i = 0; i < singleton.poolItems.Count; i++)
        {
            if (singleton.poolItems[i].gameObject.name == objectToSpawn)
            {
                objToSpawn = singleton.poolItems[i].gameObject;
            }
        }
        return singleton.Inst(objToSpawn, position, rotation);
    }

    public static GameObject PoolInstantiate(GameObject objectToSpawn, Vector3 position, Quaternion rotation)
    {
        if (!singleton) singleton = FindObjectOfType<PoolManager>();
        return singleton.Inst(objectToSpawn, position, rotation);
    }

    float nextCheck;
    public static PoolManager singleton;
    int itemsToPool;
    int itemsToPoolAmount;

    void Update()
    {
        if(!singleton)
        {
            singleton = this;
        }
        
        if(itemsToPool < poolItems.Count)
        {
            if (itemsToPoolAmount < poolItems[itemsToPool].max && poolItems[itemsToPool].gameObject)
            {
                GameObject newObj = Instantiate(poolItems[itemsToPool].gameObject) as GameObject;
                newObj.name = poolItems[itemsToPool].gameObject.name;
                newObj.transform.SetParent(transform);
                pooledObjects.Add(newObj);

                SetObjectState(newObj, false);

                itemsToPoolAmount++;
            }
            itemsToPool++;
        }

        if (Time.time > nextCheck)
        {
            nextCheck = Time.time + 0.1f;
            for (int i = 0; i < queue.Count; i++)
            {
                if(Time.time > queue[i].destroyAt)
                {
                    SetObjectState(queue[i].gameObject, false);
                    queue.RemoveAt(i);
                }
            }
        }
    }

    public static void PoolDestroy(GameObject objectToDestroy, float timer = 0f)
    {
        if (!singleton) singleton = FindObjectOfType<PoolManager>();
        if(!objectToDestroy)
        {
            return;
        }
        for (int i = 0; i < singleton.poolItems.Count; i++)
        {
            if (singleton.poolItems[i].gameObject.name == objectToDestroy.name)
            {
                if(timer > 0f)
                {
                    if(singleton.queue.Count == 0)
                    {
                        singleton.queue.Add(new QueuedPool(objectToDestroy, timer));
                        return;
                    }
                    for (int t = 0; t < singleton.queue.Count; t++)
                    {
                        if (singleton.queue[t].gameObject != objectToDestroy)
                        {
                            singleton.queue.Add(new QueuedPool(objectToDestroy, timer));
                            return;
                        }
                    }
                }
                else
                {
                    objectToDestroy.transform.SetParent(singleton.transform);
                    singleton.SetObjectState(objectToDestroy, false);
                }
                return;
            }
        }
        Destroy(objectToDestroy, timer);
    }
}
