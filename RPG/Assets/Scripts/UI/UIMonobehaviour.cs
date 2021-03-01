using UnityEngine;
using UnityEngine.UI;

public class UIMonoBehaviour : MonoBehaviour
{
    protected bool CheckUIConfigAndLogError(object o, string name)
    {
        if (o == null)
        {
            LogManager.LogError($"{name} passed null config.");
            return true;
        }
        return false;
    }

    protected bool ConvertConfig<T>(object o, out T t) 
    {
        t = (T)o;
        if (t == null)
        {
            LogManager.LogError($"{o} cannot be converted to IStateParam.");
            return false;
        }
        return true;
    }
}

public static class GameObjectExtension
{
    public static GameObject SafeSetActive(this GameObject o, bool active)
    {
        if (o == null)
        {
            LogManager.LogError("Tried setting null object active");
            return o;
        }
        o.SetActive(active);
        return o;
    }
}