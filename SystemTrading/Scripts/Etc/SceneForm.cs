using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// Monobehaviour처럼 작동하는 Form
/// </summary>
public class SceneForm : Form
{
    public static uint TargetFrame = 10;
    private readonly static int nextUpdateTime = (int)(1f / TargetFrame * 1000f);

    private Timer timer = new Timer();

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
    /// 클래스에 선언된 Monobehaviour
    /// </summary>
    private List<Monobehaviour> _monobehaviours = new List<Monobehaviour>();

    /// <summary>
    /// 코루틴
    /// </summary>
    private List<Coroutine> _coroutines = new List<Coroutine>();

    public SceneForm()
    {
        // 클래스에 선언된 Monobehaviour 등록
        AddMonobehaviours();

        // 클래스에 선언된 예약 함수 등록
        RegisterDefualtMethod();

        // Update 등록
        timer.Tick += UpdateProcess;
        timer.Interval = nextUpdateTime;

        // OnDestroy 등록
        this.Disposed += Dispose;

        // 프로그램 시작
        Resume();
    }

    /// <summary>
    /// 클래스에서 선언된 Monobehaviour를 찾아서 등록
    /// </summary>
    private void AddMonobehaviours()
    {
        Type type = this.GetType();
        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo fieldInfo in fieldInfos)
        {
            if (fieldInfo.FieldType.BaseType != null &&
                fieldInfo.FieldType.BaseType.FullName.Equals("Monobehaviour"))
            {
                _monobehaviours.Add((Monobehaviour)fieldInfo.GetValue(this));
            }
        }
    }

    /// <summary>
    /// 예약 함수를 찾아서 등록한다.
    /// </summary>
    private void RegisterDefualtMethod()
    {
        Type t     = this.GetType();
        _awake     = t.GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _start     = t.GetMethod("Start", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _onEnable  = t.GetMethod("OnEnable", BindingFlags.Public | BindingFlags.NonPublic |  BindingFlags.Instance);
        _update    = t.GetMethod("SceneUpdate", BindingFlags.Public | BindingFlags.NonPublic |  BindingFlags.Instance);
        _onDisable = t.GetMethod("OnDisable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _onDestroy = t.GetMethod("OnDestroy", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    /// <summary>
    /// 업데이트 프로세스
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UpdateProcess(object sender, EventArgs e)
    {
        _update?.Invoke(this, null);
        UpdateCoroutines();

        for (int i = 0; i < _monobehaviours.Count; i++)
        {
            _monobehaviours[i].UpdateProcess();
        }
    }

    private Queue<string> _removeCoroutineQueue = new Queue<string>();
    /// <summary>
    /// 코루틴 실행
    /// </summary>
    private void UpdateCoroutines()
    {
        for (int i = 0; i < _coroutines.Count; i++)
        {
            if (_coroutines[i] != null)
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
                            {
                                _removeCoroutineQueue.Enqueue(_coroutines[i].name);
                            }
                        }
                    }
                }
            }
        }

        while (_removeCoroutineQueue.Count > 0)
        {
            string methodName = _removeCoroutineQueue.Dequeue();
            int removeIndex = _coroutines.FindIndex(co => co != null && co.name.Equals(methodName));
            if (removeIndex != -1)
                _coroutines.RemoveAt(removeIndex);
        }

        for (int i = 0; i < _coroutines.Count; i++)
        {
            int nullIndex = _coroutines.FindIndex(co => co == null);
            if (nullIndex != -1)
                _coroutines.RemoveAt(nullIndex);
        }
    }

    public void Dispose(object sender, EventArgs e)
    {
        Pause();
        _onDestroy?.Invoke(this, null);
    }

    private void Pause()
    {
        if (_isProcessing)
        {
            timer.Enabled = false;
            _isProcessing = false;
            _onDisable?.Invoke(this, null);
        }
    }

    private void Resume()
    {
        if (!_isProcessing)
        {
            timer.Enabled = true;
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
        for (int i = 0; i < _coroutines.Count; i++)
        {
            if (_coroutines[i] != null)
            {
                if (_coroutines[i].name.Equals(methodName))
                {
                    _removeCoroutineQueue.Enqueue(methodName);
                    break;
                }
            }
        }
    }
}
