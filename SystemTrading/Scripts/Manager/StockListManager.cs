using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Office.Interop.Excel;

public class StockListManager : Singleton<StockListManager>
{
    public static readonly string DEFAULT_EXCEL_PATH = Path.Combine(Path.GetTempPath(), "상장법인목록.xls");
    private const int REGISTER_DYNAMIC_STOCKiNFO_MAX = 3500;

    private enum eStockRows
    {
        Name = 1,
        TradingSymbol,
        Sector,
        Item,
        ListingDate,
        ClosingMonth,
        Representative,
        Site,
        Region,
    }

    /// <summary>
    /// 종목 리스트 [종목코드, 정보]
    /// https://kind.krx.co.kr/corpgeneral/corpList.do?method=loadInitPage 에서 전체 선택된 상태에서 엑셀 파일 다운로드
    /// </summary>
    private Dictionary<string, StockInfo> _stockInfos = null;
    public Dictionary<string, StockInfo> StockInfos  => _stockInfos;
    public List<StockInfo> stockInfoList = new List<StockInfo>();

    /// <summary>
    /// 종목 리스트(_stockInfos) 중, 실시간 갱신 중인 종목 데이터
    /// </summary>
    private Queue<StockInfo> _dynimaicStockInfoQueue = new Queue<StockInfo>();

    /// <summary>
    /// 종목 가져오기
    /// </summary>
    /// <param name="tradingSymbol">종목 코드</param>
    /// <returns></returns>
    public StockInfo GetStockInfo(string tradingSymbol)
    {
        StockInfo stock = null;
        if (!string.IsNullOrEmpty(tradingSymbol))
        {
            if (_stockInfos.TryGetValue(tradingSymbol, out stock))
            {
                // 추가 처리 하는 부분..
            }
            else
            {
                // 비슷한 종목 코드인지 찾아보기 (A001122 식으로 오기도 함)
                var enumerator = _stockInfos.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (tradingSymbol.Contains(enumerator.Current.Value.tradingSymbol))
                    {
                        stock = enumerator.Current.Value;
                        break;
                    }
                }
            }
        }
        return stock;
    }

    /// <summary>
    /// 종목 가져오기(이름)
    /// </summary>
    /// <param name="name">종목 이름</param>
    /// <returns></returns>
    public StockInfo GetStockInfoByName(string name)
    {
        StockInfo stock = null;
        var enumerator = _stockInfos.GetEnumerator();
        while (enumerator.MoveNext())
        {
            StockInfo currentInfo = enumerator.Current.Value;
            if (name.Equals(currentInfo.Name))
            {
                stock = currentInfo;
                break;
            }
        }
        return stock;
    }

    private List<StockInfo> _results = new List<StockInfo>();
    /// <summary>
    /// 종목 검색 리스트 반환
    /// </summary>
    /// <param name="searchText">검색 내용</param>
    /// <returns>검색 결과 리스트</returns>
    public List<StockInfo> SearchStockInfos(string searchText)
    {
        var enumerator = _stockInfos.GetEnumerator();
        _results.Clear();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Value.Name.Contains(searchText))
                _results.Add(enumerator.Current.Value);
        }
        return _results;
    }

    /// <summary>
    /// Transaction을 통한 정보 가져오기 + 실시간 데이터 구독
    /// </summary>
    /// <param name="stockInfo"></param>
    public void RequestStockInfo(StockInfo stockInfo, System.Action<bool> onReceive = null)
    {
        if (stockInfo != null)
        {
            // 주식 기본 정보 요청
            KiwoomManager.Instance.SetTrValue("종목코드", stockInfo.tradingSymbol);
            KiwoomManager.Instance.RequestTransaction(eOPTCode.주식기본정보요청, onReceive: onReceive);

            // 주식 호가 요청
            //KiwoomManager.Instance.SetTrValue("종목코드", stockInfo.tradingSymbol);
            //KiwoomManager.Instance.RequestTransaction(eOPTCode.주식호가요청, onReceive: onReceive);

            // 주식 일봉 차트 조회 요청
            //KiwoomManager.Instance.SetTrValue("종목코드", stockInfo.tradingSymbol);
            //KiwoomManager.Instance.SetTrValue("틱범위", "1");  // 1분봉
            //KiwoomManager.Instance.SetTrValue("수정주가구분", "1");
            //KiwoomManager.Instance.RequestTransaction(eOPTCode.주식분봉차트조회요청);

            // + 실시간 데이터 구독 (최대 구독 가능 카운트: REGISTER_DYNAMIC_STOCKiNFO_MAX)
            //ConnectionDynamicData(stockInfo);
        }
    }

    /// <summary>
    /// 모든 종목 실시간 등록
    /// </summary>
    public void ConnectionAllStocks(System.Action onFinished = null)
    {
        MultiThread.Start(() =>
        {
            if (_stockInfos.Count > 0)
            {
                var enumerator = _stockInfos.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (KiwoomManager.Instance.IsConnect)
                    {
                        ConnectionDynamicData(enumerator.Current.Value);
                        Logger.Log(enumerator.Current.Value.Name + "등록");
                    }
                    else
                        break;
                }
            }
        }, onFinished);
    }

    /// <summary>
    /// 실시간 갱신 데이터 등록
    /// </summary>
    /// <param name="stockInfo"></param>
    public void ConnectionDynamicData(StockInfo stockInfo)
    {
        KiwoomManager.Instance.RegisterRealData(KiwoomManager.GetStockScrNum(), stockInfo.tradingSymbol, eFID.현재가, eFID.등락율, eFID.전일대비, eFID.거래량, eFID.거래회전율, eFID.최우선_매도호가, eFID.최우선_매수호가);
        _dynimaicStockInfoQueue.Enqueue(stockInfo);
        stockInfo.ConnectDynamicData();
        if (_dynimaicStockInfoQueue.Count > REGISTER_DYNAMIC_STOCKiNFO_MAX)
        {
            var dequeue = _dynimaicStockInfoQueue.Dequeue();
            dequeue.DisconnedDynamicData();
        }
    }

    /// <summary>
    /// API로 부터 얻은 종목 리스트 적용
    /// </summary>
    /// <param name="stockInfoList"></param>
    public void ApplyStockInfos(List<StockInfo> stockInfoList)
    {
        _stockInfos.Clear();
        for (int i = 0; i < stockInfoList.Count; i++)
        {
            if (!_stockInfos.ContainsKey(stockInfoList[i].tradingSymbol))
            {
                _stockInfos.Add(stockInfoList[i].tradingSymbol, stockInfoList[i]);
                this.stockInfoList.Add(stockInfoList[i]);
            }
        }
        if (!ProgramOrderManager.Instance.IsAvailableTradingTime)
            LoadStockList();
    }

    private bool _isFileUse = false;
    /// <summary>
    /// 주식 종목 리스트 저장하기
    /// </summary>
    public void SaveStockList()
    {
        if (!_isFileUse)
        {
            _isFileUse = true;
            MultiThread.Start(() =>
            {
                Utils.FileSave("StockInfos", _stockInfos);
                _isFileUse = false;
            });
        }
    }

    /// <summary>
    /// 주식 종목 리스트 기록 불러오기
    /// </summary>
    public void LoadStockList()
    {
        if (!_isFileUse)
        {
            _isFileUse = true;
            MultiThread.Start(() =>
            {
                var loadData = Utils.FileLoad<Dictionary<string, StockInfo>>("StockInfos");
                if (loadData != null)
                {
                    var enumerator = loadData.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var loadStock = enumerator.Current.Value;
                        var stock = GetStockInfo(enumerator.Current.Key);
                        if (stock != null)
                            stock.LoadDataApply(loadStock);
                    }
                }

                // 디버깅 체크 구간
                int maxRateQueue = 0;
                StockInfo stockInfo = null;
                for (int i = 0; i < stockInfoList.Count; i++)
                {
                    if (maxRateQueue < stockInfoList[i].RateQueueCount)
                    {
                        maxRateQueue = stockInfoList[i].RateQueueCount;
                        stockInfo = stockInfoList[i];
                    }
                }
                _isFileUse = false;
            });
        }
    }

    protected override void Install()
    {
        _stockInfos = new Dictionary<string, StockInfo>();
    }

    protected override void Release()
    {
        if (_stockInfos != null)
        {
            _stockInfos.Clear();
            _stockInfos = null;
        }
    }
}
