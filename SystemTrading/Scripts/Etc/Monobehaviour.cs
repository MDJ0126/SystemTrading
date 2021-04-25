using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;

/// <summary>
/// 유니티 Monobehaviour 클래스 모방, 유니티 Mono Lift Cycle 참고하면 됩니다.
/// 2021.02.07 제작
/// </summary>
public abstract class Monobehaviour : IDisposable
{
    #region Static Members

    public static uint TargetFrame = 10;

    public static List<Monobehaviour> monobehaviours = new List<Monobehaviour>();
    public static int CurrentProcessingCount { get { return monobehaviours.Count; } }

    #endregion

    private Dictionary<string, MethodInfo> _instanceMethodInfos = new Dictionary<string, MethodInfo>();
    private List<string> _unInstanceMethodInfos = new List<string>();
    
    private bool _isAwake = false;
    private bool _isStart = false;
    private bool _isProcessing = false;

    private MethodInfo _awake = null;
    private MethodInfo _start = null;
    private MethodInfo _onEnable = null;
    private MethodInfo _update = null;
    private MethodInfo _onDisable = null;
    private MethodInfo _onDestroy = null;

    /// <summary>
    /// 코루틴
    /// </summary>
    private List<Coroutine> _coroutines = new List<Coroutine>();

    /// <summary>
    /// Monobehaviour 활성화 여부
    /// </summary>
    public bool enable
    {
        get
        {
            return _isProcessing;
        }
        set
        {
            if (value)
                Resume();
            else
                Pause();
        }
    }

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="isProcessing"></param>
    public Monobehaviour()
    {
        monobehaviours.Add(this);
        RegisterDefualtMethod();
        Resume();
    }

    /// <summary>
    /// 소멸자
    /// </summary>
    ~Monobehaviour()
    {
        Dispose();
    }

    public void UpdateProcess()
    {
        _update?.Invoke(this, null);
        UpdateCoroutines();
    }

    private Queue<string> _removeCoroutineQueue = new Queue<string>();
    /// <summary>
    /// 코루틴 실행
    /// </summary>
    private void UpdateCoroutines()
    {
        while (_removeCoroutineQueue.Count > 0)
        {
            string methodName = _removeCoroutineQueue.Dequeue();
            Coroutine coroutine = _coroutines.Find(co => co.name.Equals(methodName));
            if (coroutine != null)
            {
                _coroutines.Remove(coroutine);
            }
        }

        for (int i = 0; i < _coroutines.Count; i++)
        {
            IEnumerator enumerator = _coroutines[i].enumerator;
            if (enumerator != null)
            {
                if (enumerator.Current == null)
                    enumerator.MoveNext();

                if (enumerator != null)
                {
                    iKeepWait keepWait = enumerator.Current as iKeepWait;
                    if (keepWait != null && keepWait.IsMoveNext())
                    {
                        var isEnd = !enumerator.MoveNext();
                        if (isEnd)
                            _removeCoroutineQueue.Enqueue(_coroutines[i].name);
                    }
                }
            }
        }
    }

    private void RegisterDefualtMethod()
    {
        Type t     = this.GetType();
        _awake     = t.GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _start     = t.GetMethod("Start", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _onEnable  = t.GetMethod("OnEnable", BindingFlags.Public | BindingFlags.NonPublic |  BindingFlags.Instance);
        _update    = t.GetMethod("Update", BindingFlags.Public | BindingFlags.NonPublic |  BindingFlags.Instance);
        _onDisable = t.GetMethod("OnDisable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _onDestroy = t.GetMethod("OnDestroy", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public void Dispose()
    {
        Pause();
        _onDestroy?.Invoke(this, null);
    }

    private void Pause()
    {
        if (_isProcessing)
        {
            _isProcessing = false;
            _onDisable?.Invoke(this, null);
        }
    }

    private void Resume()
    {
        if (!_isProcessing)
        {
            _isProcessing = true;

            if (!_isAwake)
            {
                _isAwake = true;
                _awake?.Invoke(this, null);
            }

            if (!_isStart)
            {
                _isStart = true;
                _start?.Invoke(this, null);
            }

            _onEnable?.Invoke(this, null);
        }
    }

    /// <summary>
    /// String으로 함수를 호출합니다.
    /// </summary>
    /// <param name="methodName">함수 이름</param>
    /// <param name="args">사용되는 매개변수</param>
    public void SendMessage(string methodName, params object[] args)
    {
        if (!_unInstanceMethodInfos.Exists(_methodName => _methodName.Equals(methodName)))
        {
            bool isRegister = false;
            if (!_instanceMethodInfos.ContainsKey(methodName))
            {
                // Caching Method
                Type t = this.GetType();
                MethodInfo method = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    _instanceMethodInfos.Add(methodName, method);
                    isRegister = true;
                }
                else
                {
                    _unInstanceMethodInfos.Add(methodName);
                    isRegister = false;
                }
            }
            else
                isRegister = true;

            if (isRegister)
                ProcessMethod(methodName, args);
        }
    }

    private void ProcessMethod(string methodName, params object[] args)
    {
        MethodInfo method = null;
        if (_instanceMethodInfos.TryGetValue(methodName, out method))
        {
            try
            {
                method.Invoke(this, args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    public void StartCoroutine(string methodName)
    {
        Type t = this.GetType();
        var method = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null)
        {
            var iEnumerator = (IEnumerator)method.Invoke(this, null);
            _coroutines.Add(new Coroutine(methodName, iEnumerator));
        }
    }

    public void StopCoroutine(string methodName)
    {
        if (_coroutines.Exists(co => co.name.Equals(methodName)))
            _removeCoroutineQueue.Enqueue(methodName);
    }
}