using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poolable : MonoBehaviour
{
    public GameObject Key { get; private set; }
    public bool IsPooled { get; set; } = false;

    private ObjectPooling pooler;

    public void Setup(ObjectPooling pooler, GameObject prefab)
    {
        Key = prefab;
        this.pooler = pooler;

        gameObject.SetActive(false);
    }

    public void Pool()
    {
        pooler.Pool(this);
    }
}
