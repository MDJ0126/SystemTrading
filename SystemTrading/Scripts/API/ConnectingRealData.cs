using System.Collections.Generic;
using System.Text;

/// <summary>
/// 실시간 데이터 요청 데이터 정보
/// </summary>
public class ConnectingRealData
{
    public StockInfo stockInfo;     // null일 수 있음. (꼭 종목데이터가 아닌 경우가 있음. ex => 장시작시간)
    public string screenNumber;     // 통신간 고유 번호
    public string tradingSymbol;    // 거래 종목 코드 (거래용이 아니면 공란)
    public List<eFID> fids = new List<eFID>();

    public ConnectingRealData(string screenNumber, string tradingSymbol, params eFID[] fids)
    {
        this.screenNumber = screenNumber;
        this.tradingSymbol = tradingSymbol;
        this.stockInfo = StockListManager.Instance.GetStockInfo(tradingSymbol);
        this.fids.Clear();
        for (int i = 0; i < fids.Length; i++)
        {
            this.fids.Add(fids[i]);
        }
    }

    private StringBuilder _sb = new StringBuilder();
    public string GetFidString()
    {
        _sb.Length = 0;
        for (int i = 0; i < fids.Count; i++)
        {
            _sb.Append(((int)fids[i]).ToString()).Append(';');
        }
        return _sb.ToString();
    }
}