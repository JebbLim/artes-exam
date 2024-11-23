using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class ConfigRepository : Singleton<ConfigRepository>
{
    [SerializeField] private Object[] _configs;

    public static T GetConfig<T>()
        where T : Object
    {
        T settings = Instance._configs.Where(s => s is T).FirstOrDefault() as T;
        Assert.IsNotNull(settings, typeof(T).ToString() + " does not exist");

        return settings;
    }
}