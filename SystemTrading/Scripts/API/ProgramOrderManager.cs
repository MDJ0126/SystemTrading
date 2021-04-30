using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

public class ProgramOrderManager : Singleton<ProgramOrderManager>
{
    private DateTime _tradingStartTime, _tradingEndTime;

    private bool _isAutoProgramOrder = false;
    /// <summary>
    /// 자동 매매 시작 여부
    /// </summary>
    public bool IsAutoProgramOrder
    {
        get
        {
            return _isAutoProgramOrder;
        }
        set
        {
            _isAutoProgramOrder = value;
            if (value)
                LineNotify.SendMessage("자동 매매가 시작되었습니다.");
            else
                LineNotify.SendMessage("자동 매매가 중지되었습니다.");
        }
    }

    /// <summary>
    /// 매매에 사용되는 계좌
    /// </summary>
    public AccountInfo AccountInfo { get; private set; } = null;

    /// <summary>
    /// 매매에 사용되는 계좌 비밀번호
    /// </summary>
    public string AccountPassward { get; private set; } = "1234";

    /// <summary>
    /// 매수 시도 최소 등락율
    /// </summary>
    public float StartRate { get; set; } = 3.5f; //(%)

    /// <summary>
    /// 매수 한계선
    /// </summary>
    public float LimitRate { get; set; } = 22.0f; //(%)

    /// <summary>
    /// 매수 시도 기준 종목 개수
    /// </summary>
    public byte TryStockSellCount { get; set; } = 4; //(개)

    /// <summary>
    /// 손실 제한률
    /// </summary>
    public float StopLoss { get; set; } = 2.5f; //(%)

    /// <summary>
    /// 매도 최대 시점
    /// </summary>
    public float MaxPriceRate { get; set; } = 27.5f; //(%)

    /// <summary>
    /// 매도 시점
    /// </summary>
    public float SellProfit { get; set; } = 2.5f; //(%)

    /// <summary>
    /// 매입 제한 금액
    /// </summary>
    public long LimitPrice { get; set; } = 1000000; //(원)

    /// <summary>
    /// 매입 종목 제한 개수
    /// </summary>
    public long LimitCount { get; set; } = 20;  //(개)

    /// <summary>
    /// 최대 보유 주식 개수
    /// </summary>
    public sbyte MaxHaveStockCount { get; set; } = 10; //(개)

    /// <summary>
    /// 매수 기준 분당 성장률
    /// </summary>
    public float BaseGrowthRatePerMinute { get; set; } = 1.0f;

    /// <summary>
    /// 추천 매수 기준 순위
    /// </summary>
    public short BaseRank { get; set; } = 20;

    /// <summary>
    /// 거래가 가능한 시간인지
    /// </summary>
    public bool IsAvailableTradingTime => _tradingStartTime <= ProgramConfig.NowTime && _tradingEndTime >= ProgramConfig.NowTime;

    /// <summary>
    /// 감시 시간 리셋
    /// </summary>
    private void ResetTimer()
    {
        // 프로그램 자동 거래 시간 금일 오전 9시 10분 ~ 오후 2시 45분까지
        _tradingStartTime = ProgramConfig.NowTime.Date.AddHours(9).AddMinutes(00);
        _tradingEndTime = ProgramConfig.NowTime.Date.AddHours(14).AddMinutes(45);
        _sellStockInfos.Clear();
    }

    protected override void Install()
    {
        ResetTimer();
    }

    protected override void Release()
    {

    }

    // 추천 매수 리스트
    public List<StockInfo> recommendeds = new List<StockInfo>();

    // 금일 매수 리스트
    private List<StockInfo> _sellStockInfos = new List<StockInfo>();

    private void Update()
    {
        // 매도 리스트
        List<BalanceStock> sellStocks = new List<BalanceStock>();

        // 전체 종목 리스트
        List<StockInfo> orderStockInfos = new List<StockInfo>();
        var enumerator = StockListManager.Instance.StockInfos.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var stockInfo = enumerator.Current.Value;
            orderStockInfos.Add(stockInfo);
        }

        // 계좌 잔고 리스트
        var balanceInfos = AccountInfo.BalanceStocks;
        while (true)
        {
            recommendeds.Clear();
            if (_tradingStartTime.Date != ProgramConfig.NowTime.Date)
                ResetTimer();

            // 설정된 거래 시간 범위 기준으로 거래 진행
            if (_tradingStartTime <= ProgramConfig.NowTime && _tradingEndTime >= ProgramConfig.NowTime)
            {
                // 매수 예정 리스트 체크
                for (int i = 0; i < orderStockInfos.Count; i++)
                {
                    var stockInfo = orderStockInfos[i];
                    if (stockInfo != null)
                    {
                        // 조건: 매수 한계선
                        if (stockInfo.UpDownRate <= LimitRate)
                        {
                            // 조건: 최소 매수 진입 등락률
                            if (stockInfo.UpDownRate >= StartRate)
                            {
                                // 조건: 분당 성장률
                                if (stockInfo.GrowthRatePerMinute >= BaseGrowthRatePerMinute)
                                {
                                    if (!recommendeds.Exists(stock => stockInfo.Equals(stock)) &&
                                        !_sellStockInfos.Exists(stock => stockInfo.Equals(stock)))
                                    {
                                        recommendeds.Add(stockInfo);
                                    }
                                }
                            }

                            // 조건: 급등주
                            if (stockInfo.GrowthRatePerMinute >= 5f)
                            {
                                if (!recommendeds.Exists(stock => stockInfo.Equals(stock)) &&
                                    !_sellStockInfos.Exists(stock => stockInfo.Equals(stock)))
                                {
                                    recommendeds.Add(stockInfo);
                                }
                            }
                        }
                    }
                }

                // 매수하기
                if (IsAutoProgramOrder)
                {
                    if (AccountInfo != null)
                    {
                        long useAvailableMoney = Math.Min(AccountInfo.AvailableMoney, (long)(AccountInfo.EstimatedAssets_Calc * 0.9f));
                        // 사용 가능 금액이 총 평가 금액보다 50% 이상 많을 경우에 매수 시도
                        // 계속 반복하다보면 1개씩 매입하는 비효율 현상이 생겨서 분기태움.(안전 장치)
                        if (useAvailableMoney >= AccountInfo.EstimatedAssets_Calc * 0.5f)
                        {
                            if (TryStockSellCount <= recommendeds.Count)
                            {
                                float sumGrowthRatePerMinute = 0f;
                                for (int i = 0; i < recommendeds.Count; i++)
                                {
                                    sumGrowthRatePerMinute += recommendeds[i].GrowthRatePerMinute;
                                }

                                for (int i = 0; i < recommendeds.Count; i++)
                                {
                                    var stockInfo = recommendeds[i];
                                    // 매수 시도할 분배된 금액
                                    int buyMoney = (int)(useAvailableMoney * (stockInfo.GrowthRatePerMinute / sumGrowthRatePerMinute));
                                    // 매수 개수
                                    int buyCount = buyMoney / stockInfo.StockPrice;
                                    // 매수
                                    if (buyCount > 0 && AccountInfo.BalanceStocks.Count < MaxHaveStockCount)
                                    {
                                        OrderBuy(stockInfo, buyCount);
                                        _sellStockInfos.Add(stockInfo);
                                    }
                                }
                            }
                        }
                    }
                }

                // 매도 체크
                for (int i = 0; i < balanceInfos.Count; i++)
                {
                    if (balanceInfos[i].BalanceStockState == eBalanceStockState.Have)
                    {
                        bool isSell = false;
                        // 조건1: 최대 손익율에서 스탑 로스 책정
                        if (balanceInfos[i].MaxProfitRate - balanceInfos[i].EstimatedProfitRate >= StopLoss)
                        {
                            isSell = true;
                        }

                        // 조건2: 최대 이익 시점 도달하는 경우
                        //if (balanceInfos[i].EstimatedProfitRate >= MaxPriceRate)
                        //{
                        //    isSell = true;
                        //}

                        // 조건3: 특정 손익율 도달하는 경우
                        if (balanceInfos[i].EstimatedProfitRate >= SellProfit)
                        {
                            isSell = true;
                        }    

                        if (isSell)
                        {
                            if (!sellStocks.Exists(item => item.Equals(balanceInfos[i])))
                                sellStocks.Add(balanceInfos[i]);
                        }
                    }
                }

                // 매도하기
                if (IsAutoProgramOrder)
                {
                    for (int i = 0; i < sellStocks.Count; i++)
                    {
                        var balanceStock = sellStocks[i];
                        if (balanceStock.BalanceStockState == eBalanceStockState.Have)
                            OrderSell(balanceStock.stockInfo, balanceStock.HaveCnt);
                    }
                    sellStocks.Clear();
                }
            }
            Thread.Sleep(500);
        }
    }

    private MultiThread.ThreadInfo _threadInfo = null;
    /// <summary>
    /// 계좌 연결
    /// </summary>
    /// <param name="accountInfo"></param>
    /// <param name="passward"></param>
    public void LinkAccount(AccountInfo accountInfo, string passward)
    {
        this.AccountInfo = accountInfo;
        this.AccountPassward = passward;
        _threadInfo?.Stop();
        _threadInfo = MultiThread.Start(Update);
    }

    /// <summary>
    /// 계좌 연결 해제
    /// </summary>
    public void UnlinkAccount()
    {
        this.AccountInfo = null;
        this.AccountPassward = string.Empty;
        _threadInfo?.Stop();
        _threadInfo = null;
    }

    /// <summary>
    /// 자동 매매 시작
    /// </summary>
    public void Start()
    {

    }

    /// <summary>
    /// 자동 매매 중지
    /// </summary>
    public void Stop()
    {

    }

    /// <summary>
    /// 매수 주문
    /// </summary>
    public void OrderBuy(StockInfo stockInfo, int count)
    {
        if (AccountInfo != null)
        {
            if (AccountInfo.AvailableMoney >= count * stockInfo.StockPrice)
            {
                KiwoomManager.Instance.SendOrder(AccountInfo.AccountNumber, eSendOrderType.신규매수, stockInfo, count, 0, eSendType.시장가);
                LineNotify.SendMessage($"[{stockInfo.Name}] 매수 신청을 했습니다.");
            }
            else
                LineNotify.SendMessage($"예수금 부족으로 [{stockInfo.Name}] 매수 신청 실패했습니다.");
        }
    }

    /// <summary>
    /// 매도 주문
    /// </summary>
    public void OrderSell(StockInfo stockInfo, int count)
    {
        if (AccountInfo != null)
        {
            KiwoomManager.Instance.SendOrder(AccountInfo.AccountNumber, eSendOrderType.신규매도, stockInfo, count, 0, eSendType.시장가);
            LineNotify.SendMessage($"[{stockInfo.Name}] 매도 신청을 했습니다.");
        }
    }

    /// <summary>
    /// 취소 주문
    /// </summary>
    /// <param name="stockInfo"></param>
    public void OrderCancel(StockInfo stockInfo)
    {
        if (AccountInfo != null)
        {
            KiwoomManager.Instance.SendCancelOrder(AccountInfo.AccountNumber, stockInfo);
            LineNotify.SendMessage($"[{stockInfo.Name}] 취소 신청을 했습니다.");
        }
    }
}