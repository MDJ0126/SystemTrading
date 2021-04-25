using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public class HandlerKiwoomAPI : Singleton<HandlerKiwoomAPI>
{
    private Timer _timer = null;

    /// <summary>
    /// KiwoomAPI 연결
    /// </summary>
    public event Action onConnect;
    /// <summary>
    /// KiwoomAPI 연결 해제
    /// </summary>
    public event Action onDisconnect;
    /// <summary>
    /// TR 데이터 수신
    /// </summary>
    public event Action onRecieveTransactionData;
    /// <summary>
    /// 실시간 데이터 수신
    /// </summary>
    public event Action onRecieveRealData;
    /// <summary>
    /// 주문 결과 및 거래 체결 수신
    /// </summary>
    public event Action onReceiveChejanData;

    private bool _isOnConnect = false;
    private bool _isOnDisconnect = false;
    private bool _isReceiveRealData = false;
    private bool _isReceiveChejanData = false;
    private List<TransactionData> _responseTransactionDatas = new List<TransactionData>();

    protected override void Install()
    {
        _timer = new Timer();
        _timer.Tick += OnUpdate;
        _timer.Interval = 500;
        _timer.Enabled = true;
    }

    protected override void Release()
    {

    }

    private void OnUpdate(object sender, EventArgs e)
    {
        if (_isOnConnect)
        {
            _isOnConnect = false;
            onConnect?.Invoke();
        }

        if (_isOnDisconnect)
        {
            _isOnDisconnect = false;
            onDisconnect?.Invoke();
        }

        if (KiwoomManager.Instance.ResponseTransactionDatas.Count > 0)
        {
            _responseTransactionDatas.Clear();
            _responseTransactionDatas.AddRange(KiwoomManager.Instance.ResponseTransactionDatas);
            for (int i = 0; i < _responseTransactionDatas.Count; i++)
            {
                KiwoomManager.Instance.ResponseTransactionDatas.Remove(_responseTransactionDatas[i]);
                bool isResult = _responseTransactionDatas[i].isReceiveResult.Value;
                _responseTransactionDatas[i].OnReceive?.Invoke(isResult);
            }
            onRecieveTransactionData?.Invoke();
        }

        if (_isReceiveRealData)
        {
            _isReceiveRealData = false;
            onRecieveRealData?.Invoke();
        }

        if (_isReceiveChejanData)
        {
            _isReceiveChejanData = true;
            onReceiveChejanData?.Invoke();
        }
    }

    public void NotifyOnConnect()
    {
        _isOnConnect = true;
    }

    public void NotifyOnDisconnect()
    {
        _isOnDisconnect = true;
    }

    public void NotifyReceiveRealData()
    {
        _isReceiveRealData = true;
    }

    public void NotifyOnReceiveChejanData()
    {
        _isReceiveChejanData = true;
    }
}
