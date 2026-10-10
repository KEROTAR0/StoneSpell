using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAfterImagePool : MonoBehaviour
{
    [SerializeField]
    private GameObject afterImagePrefab;

    private Queue<GameObject> availableObj = new Queue<GameObject>();

    public static PlayerAfterImagePool Instance {get; private set;}
    private void Awake()
    {
        Instance = this;
        GrowPool();
    }
    private void GrowPool()
    {
        for(int i = 1; i < 10; i++)
        {
            var instanceToAdd = Instantiate(afterImagePrefab);
            instanceToAdd.transform.SetParent(transform);
            AddToPool(instanceToAdd);
        }
    }
    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        availableObj.Enqueue(instance);

    }

    public GameObject GetFromPool()
    {
        if(availableObj.Count == 0)
        {
            GrowPool();
        }

        var instance = availableObj.Dequeue();
        instance.SetActive(true);
        return instance;
    }
}
