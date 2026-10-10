using System.Collections.Generic;
using UnityEngine;

public class ManaPool : MonoBehaviour
{
    [SerializeField] private GameObject manaPrefab;
    private Queue<GameObject> availableObj = new Queue<GameObject>();

    public static ManaPool Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        GrowPool(20);
    }

    private void GrowPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var instanceToAdd = Instantiate(manaPrefab, transform);
            instanceToAdd.SetActive(false);
            availableObj.Enqueue(instanceToAdd);
        }
    }

    public GameObject GetFromPool(Vector3 position)
    {
        if (availableObj.Count == 0) GrowPool(10);

        var instance = availableObj.Dequeue();
        instance.transform.position = position;
        instance.SetActive(true);
        return instance;
    }

    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        availableObj.Enqueue(instance);
    }
}