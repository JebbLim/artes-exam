using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectPooling : MonoBehaviour
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

    public void RegisterPrefab(GameObject prefab, int amount)
    {
        if (pools.ContainsKey(prefab))
        {
            Debug.LogError($"Pooler already contains: {prefab.name}");
            return;
        }

        pools.Add(prefab, new());

        for (int j = 0; j < amount; j++)
        {
            Poolable newObject = CreateInstance(prefab);
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

    public GameObject GetPooledObject(GameObject key)
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

            //foreach (Poolable poolable in pools[key])
            //{
            //    if (poolable.IsPooled)
            //    {
            //        pools[key].Remove(poolable);
            //        poolable.IsPooled = false;
            //        return poolable.gameObject;
            //    }
            //}

            //Poolable poolable = pools[key].FirstOrDefault(o => o.IsPooled);
            //if (poolable)
            //{
            //    pools[key].Remove(poolable);
            //    poolable.IsPooled = false;
            //    return poolable.gameObject;
            //}
        }

        Poolable newObject = CreateInstance(key);
        newObject.IsPooled = false;
        return newObject.gameObject;
    }

    private Poolable CreateInstance(GameObject prefab)
    {
        Poolable newObject = Instantiate(prefab).AddComponent<Poolable>();
        newObject.Setup(this, prefab);
        return newObject;
    }
}