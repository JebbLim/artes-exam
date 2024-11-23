using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : Singleton<ObjectPooling>
{
    [System.Serializable]
    public struct PoolData
    {
        public int Amount;
        public GameObject Prefab;
    }

    [SerializeField] private List<PoolData> poolDatas;
    private Dictionary<GameObject, List<Poolable>> pools = new();

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        foreach (PoolData poolData in poolDatas)
        {
            RegisterPrefab(poolData.Prefab, poolData.Amount);
        }
    }

    public void RegisterPrefab(GameObject prefab, int amount = 0)
    {
        if (pools.ContainsKey(prefab))
        {
            Debug.LogError($"Pooler already contains: {prefab.name}");
            return;
        }

        pools.Add(prefab, new());

        for (int i = 0; i < amount; i++)
        {
            Poolable newObject = CreateInstance(prefab, this.transform);
            Pool(newObject);
        }

        Debug.LogWarning($"Registered Prefab: {prefab.GetInstanceID()}");
    }

    public void Pool(Poolable poolable)
    {
        if (!pools.ContainsKey(poolable.Key))
        {
            Debug.LogError($"Pooler does not contain: {poolable.Key.name}; InstanceID: {poolable.Key.GetInstanceID()}");
            return;
        }

        pools[poolable.Key].Add(poolable);
        poolable.transform.SetParent(this.transform);
        poolable.gameObject.SetActive(false);
        poolable.IsPooled = true;
    }

    public GameObject GetPooledObject(GameObject key, Transform parent = null)
    {
        if (!pools.ContainsKey(key))
        {
            Debug.LogError($"Pooler does not contain: {key.name}");
            return null;
        }

        if (pools[key].Count > 0)
        {
            for (int i = 0; i < pools[key].Count; i++)
            {
                Poolable poolable = pools[key][i];

                if (poolable.IsPooled)
                {
                    pools[key].Remove(poolable);
                    poolable.IsPooled = false;
                    return poolable.gameObject;
                }
            }
        }

        Poolable newObject = CreateInstance(key, parent);
        newObject.IsPooled = false;
        return newObject.gameObject;
    }

    public GameObject GetPooledObject(MonoBehaviour keyBehavior, Transform parent = null)
    {
        return GetPooledObject(keyBehavior.gameObject, parent);
    }

    public T GetPooledObject<T>(GameObject key, Transform parent = null) where T : MonoBehaviour
    {
        return GetPooledObject(key, parent).GetComponent<T>();
    }

    public T GetPooledObject<T>(MonoBehaviour keyBehavior, Transform parent = null) where T : MonoBehaviour
    {
        return GetPooledObject(keyBehavior.gameObject, parent).GetComponent<T>();
    }

    private Poolable CreateInstance(GameObject prefab, Transform parent)
    {
        Poolable newObject = Instantiate(prefab, parent).AddComponent<Poolable>();
        newObject.Setup(this, prefab);
        return newObject;
    }
}
