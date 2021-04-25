
using System;
using System.Collections.Generic;
/// <summary>
/// Transaction 요청 데이터 정보
/// </summary>
public class TransactionData
{
    public enum eState
    {
        /// <summary>
        /// 요청함
        /// </summary>
        Requested,
        /// <summary>
        /// 수신 받음
        /// </summary>
        Response,
        /// <summary>
        /// 수신 애러
        /// </summary>
        Error,
    }

    public StockInfo stockInfo;     // null일 수 있음
    public string screenNumber;     // 통신간 고유 번호
    public string tradingSymbol;    // 거래 종목 코드 (거래용이 아니면 공란)
    public eOPTCode optCode;
    public Dictionary<string, string> values = new Dictionary<string, string>();
    public eState state = eState.Requested;
    public DateTime? requestTime;
    public DateTime? responseTime;
    public Action<bool> OnReceive { get; private set; } = null;
    public bool? isReceiveResult = null;

    public TransactionData(string screenNumber, string tradingSymbol, eOPTCode optCode, Dictionary<string, string> values, Action<bool> onReceive)
    {
        this.screenNumber = screenNumber;
        this.tradingSymbol = tradingSymbol;
        this.stockInfo = StockListManager.Instance.GetStockInfo(tradingSymbol);
        this.optCode = optCode;
        this.state = eState.Requested;
        this.requestTime = ProgramConfig.NowTime;
        this.OnReceive = onReceive;

        var enumerator = values.GetEnumerator();
        while (enumerator.MoveNext())
        {
            this.values.Add(enumerator.Current.Key, enumerator.Current.Value);
        }
    }
}