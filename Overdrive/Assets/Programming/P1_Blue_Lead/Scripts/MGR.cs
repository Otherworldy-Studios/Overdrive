using UnityEngine;


public class MGR<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    [SerializeField] private bool shouldPersist;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name + "_MGR";
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (shouldPersist)
            {
                DontDestroyOnLoad(gameObject); 
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}