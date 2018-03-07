using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicObjectPool : MonoBehaviour
{

    public GameObject prefab;
    private Stack<GameObject> inActiveInstances = new Stack<GameObject>();

    public GameObject GetObject()
    {
        GameObject spawnedObject;

        if (inActiveInstances.Count > 0)
        {
            spawnedObject = inActiveInstances.Pop();
        }
        else
        {
            spawnedObject = Instantiate(prefab);

            PooledObject pooledObjet = spawnedObject.AddComponent<PooledObject>();

            pooledObjet.pool = this;
        }

        spawnedObject.SetActive(true);
        return spawnedObject;
    }

    public void ReturnObject(GameObject toReturn)
    {
        PooledObject pooledObject = toReturn.GetComponent<PooledObject>();

        if (pooledObject != null && pooledObject.pool == this)
        {
            toReturn.SetActive(false);
            inActiveInstances.Push(toReturn);
        }
        else
        {
            Destroy(toReturn);
        }
    }
}

public class PooledObject : MonoBehaviour
{
    public BasicObjectPool pool;

}


