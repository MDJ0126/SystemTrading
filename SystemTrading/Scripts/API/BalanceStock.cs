using System;
using System.Collections.Generic;

/// <summary>
/// 보유 종목 클래스
/// </summary>
[Serializable]
public class BalanceStock
{
    /// <summary>
    /// 주문 접수 상태
    /// </summary>
    public bool? IsSendOrderSuccess { get; set; } = null;

    /// <summary>
    /// 주문 스크린 번호
    /// </summary>
    public string OrderScreenNumber { get; set; } = string.Empty;

    /// <summary>
    /// 보유 주식 상태
    /// </summary>
    public eBalanceStockState BalanceStockState { get; set; } = eBalanceStockState.Have;

    /// <summary>
    /// 주문 번호
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// 주문 수량
    /// </summary>
    public int OrderCount { get; set; } = 0;

    /// <summary>
    /// 대기 중인 남은 주문 잔량
    /// </summary>
    public int WaitOrderCount { get; set; } = 0;

    /// <summary>
    /// 주문 금액
    /// </summary>
    public long OrderPrice { get; set; } = 0;

    /// <summary>
    /// 주문 시간
    /// </summary>
    public DateTime? OrderTime { get; set; } = null;

    /// <summary>
    /// 체결 이후 구매해서 가지고 있기 시작한 시간
    /// </summary>
    public DateTime BuyTime { get; set; } = DateTime.MinValue;

    /// <summary>
    /// 종목 정보
    /// </summary>
    public StockInfo stockInfo = null;

    /// <summary>
    /// 종목 코드
    /// </summary>
    public string TraingSymbol { get; private set; } = string.Empty;

    /// <summary>
    /// 종목 이름
    /// </summary>
    public string StockName { get; private set; } = string.Empty;

    /// <summary>
    /// 보유 수량
    /// </summary>
    public int HaveCnt { get; private set; } = 0;

    /// <summary>
    /// 매입 금액
    /// </summary>
    public long BuyingMoney { get; private set; } = 0;

    /// <summary>
    /// 평가 금액
    /// </summary>
    public long CurrentTotalPrice { get; private set; } = 0;

    /// <summary>
    /// 손익 금액
    /// </summary>
    public long EstimatedProfit { get; private set; } = 0;

    /// <summary>
    /// 손익율
    /// </summary>
    public float EstimatedProfitRate { get; private set; } = 0f;

    /// <summary>
    /// 매도 매수 거래 수수료, 세금 캐싱
    /// </summary>
    public long buyFees, buyTax, sellFees, sellTax;

    /// <summary>
    /// 프로그램 시작 이후 포착된 최대 손익율 (스탑 로스에 사용됨)
    /// </summary>
    public float? MaxProfitRate { get; private set; } = null;

    /// <summary>
    /// 목표 등락율 캐싱
    /// </summary>
    public float targetUpDownRate = 0f;

    public BalanceStock(string traingSymbol, int haveCnt, long buyingMoney, int stockPrice = 0)
    {
        SetData(traingSymbol, haveCnt, buyingMoney, stockPrice);
    }

    /// <summary>
    /// 세팅
    /// </summary>
    public void SetData(string traingSymbol, int haveCnt, long buyingMoney, int stockPrice = 0)
    {
        stockInfo = StockListManager.Instance.GetStockInfo(traingSymbol);
        if (stockInfo != null)
        {
            stockInfo.OnChangedData += OnChangedStockInfo;
            if (stockPrice != 0)
                stockInfo.StockPrice = stockPrice;
            this.TraingSymbol = traingSymbol;
            this.StockName = stockInfo.Name;
            this.HaveCnt = haveCnt;
            this.OrderCount = haveCnt;
            this.BuyingMoney = buyingMoney;
            InitializeOrderState();
            OnChangedStockInfo();
        }
    }

    /// <summary>
    /// 매수 체결량 수정
    /// </summary>
    public void SetBuyCount(int waitOrderCount, int resultCount, long resultPrice)
    {
        this.BuyingMoney = resultPrice;
        this.HaveCnt = resultCount;
        this.WaitOrderCount = waitOrderCount;
        if (waitOrderCount == 0)
        {
            // 주문 잔량이 0개면 주문 정보 삭제
            InitializeOrderState();
            this.BuyTime = ProgramConfig.NowTime;
        }
    }

    /// <summary>
    /// 매도 체결량 수정
    /// </summary>
    public void SetSellCount(int waitOrderCount)
    {
        this.HaveCnt = waitOrderCount;
        this.WaitOrderCount = waitOrderCount;
        if (waitOrderCount == 0)
        {
            // 주문 잔량이 0개면 주문 정보 삭제
            InitializeOrderState();
        }
    }

    /// <summary>
    /// 주문 정보 초기화
    /// </summary>
    public void InitializeOrderState()
    {
        this.OrderNumber = string.Empty;
        this.IsSendOrderSuccess = null;
        this.BalanceStockState = eBalanceStockState.Have;
        this.OrderCount = 0;
        this.OrderPrice = 0;
        this.WaitOrderCount = 0;
        this.OrderTime = null;
    }

    /// <summary>
    /// 주문 접수
    /// </summary>
    /// <param name="state">상태</param>
    /// <param name="orderNumber">주문 번호</param>
    /// <param name="orderCount">주문 수량</param>
    /// <param name="waitOrderCnt">미체결 수량</param>
    /// <param name="orderTime">주문 시간</param>
    public void SetOrder(string screenNumber, bool isSendOrderSuccess, string orderNumber, int orderCount, long orderPrice, int waitOrderCnt, DateTime orderTime)
    {
        this.OrderScreenNumber = screenNumber;
        this.IsSendOrderSuccess = isSendOrderSuccess;
        this.OrderNumber = orderNumber;
        this.OrderCount = orderCount;
        this.OrderPrice = orderPrice;
        this.WaitOrderCount = waitOrderCnt;
        this.OrderTime = orderTime;
    }

    private eBalanceStockState _prevBalanceStockState = eBalanceStockState.None;
    /// <summary>
    /// 주문 상태 설정
    /// </summary>
    /// <param name="state"></param>
    public void SetOrderState(eBalanceStockState state)
    {
        _prevBalanceStockState = this.BalanceStockState;
        this.BalanceStockState = state;
    }

    /// <summary>
    /// 목표 등락률 설정
    /// </summary>
    /// <param name="rate"></param>
    public void ResetTargetRate()
    {
        this.targetUpDownRate = this.stockInfo.UpDownRate + (this.stockInfo.GrowthRatePerMinute * 2f);
    }

    public void RestoreOrderState()
    {
        this.BalanceStockState = _prevBalanceStockState;
    }

    private void OnChangedStockInfo()
    {
        this.CurrentTotalPrice = Math.Abs(stockInfo.StockPrice) * HaveCnt;
        this.EstimatedProfit = CurrentTotalPrice - BuyingMoney;
        this.EstimatedProfitRate = BuyingMoney > 0 ? (float)EstimatedProfit / (float)BuyingMoney * 100f : 0;
        RefreshMaxProfitRate();
    }

    /// <summary>
    /// 최대 손익율 갱신
    /// </summary>
    private void RefreshMaxProfitRate()
    {
        if (MaxProfitRate == null || MaxProfitRate < EstimatedProfitRate)
            this.MaxProfitRate = EstimatedProfitRate;
    }
}
