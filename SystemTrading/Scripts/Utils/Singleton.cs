using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

public abstract class Singleton<T> : IDisposable where T : Singleton<T>, new()
{
    private static T _instance = null;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
                _instance.Install();
            }
            return _instance;
        }
    }

    ~Singleton()
    {
        Dispose();
    }


    public void Dispose()
    {
        ReleaseInstance();
    }

    static public bool IsInstance
    {
        get
        {
            if (_instance != null)
                return true;
            else
                return false;
        }
    }

    static public void ReleaseInstance()
    {
        if (_instance != null)
            _instance.Release();
        _instance = null;
    }

    protected abstract void Install();
    protected abstract void Release();
}
