using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

public class ProgramOrderManager : Singleton<ProgramOrderManager>
{
    private DateTime _tradingStartTime, _tradingEndTime;

    private bool _isAutoProgramOrder = true;
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
    public float StartRate { get; set; } = 0f; //(%)

    /// <summary>
    /// 매수 한계선
    /// </summary>
    public float LimitRate { get; set; } = 20.0f; //(%)

    /// <summary>
    /// 매수 시도 기준 종목 개수
    /// </summary>
    public byte TryStockSellCount { get; set; } = 2; //(개)

    /// <summary>
    /// 손실 제한률
    /// </summary>
    public float StopLoss { get; set; } = 2.5f; //(%)

    /// <summary>
    /// 매도 최대 시점
    /// </summary>
    public float MaxPriceRate { get; set; } = 25f; //(%)

    /// <summary>
    /// 매도 시점
    /// </summary>
    public float SellProfit { get; set; } = 1.5f; //(%)

    /// <summary>
    /// 매입 제한 금액
    /// </summary>
    public long LimitPrice { get; set; } = 1000000; //(원)

    /// <summary>
    /// 매입 종목 제한 개수
    /// </summary>
    public long LimitCount { get; set; } = 20;  //(개)

    /// <summary>
    /// 잔고 최대 보유 종목 개수
    /// </summary>
    public sbyte MaxHaveStockCount { get; set; } = 10; //(개)

    /// <summary>
    /// 매수 기준 분당 성장률
    /// </summary>
    public float BaseGrowthRatePerMinute { get; set; } = 2.0f;

    /// <summary>
    /// 최대 매수 시도 개수
    /// </summary>
    public int MaxBuyCount { get; set; } = 300;

    /// <summary>
    /// 추천 매수 기준 순위
    /// </summary>
    public short BaseRank { get; set; } = 20;

    /// <summary>
    /// 거래가 가능한 시간인지
    /// </summary>
    public bool IsAvailableTradingTime => _tradingStartTime <= ProgramConfig.NowTime && _tradingEndTime >= ProgramConfig.NowTime;

    /// <summary>
    /// 금일 목표 수익률
    /// </summary>
    public float TodayTargetAccountProfitRate { get; set; } = 3.5f;

    /// <summary>
    /// 금일 목표 수익률 완료 여부
    /// </summary>
    public bool IsCompleteTodyTrading
    { 
        get
        {
            if (AccountInfo != null)
                return AccountInfo.TodayProfitRate >= TodayTargetAccountProfitRate;
            return false;
        }
    }

    /// <summary>
    /// 감시 시간 리셋
    /// </summary>
    private void ResetTimer()
    {
        // 프로그램 자동 거래 시간 금일 오전 9시 10분 ~ 오후 2시 30분까지
        _tradingStartTime   = ProgramConfig.NowTime.Date.AddHours(9).AddMinutes(00);
        _tradingEndTime     = ProgramConfig.NowTime.Date.AddHours(14).AddMinutes(30);
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

    // 금일 매도 리스트 (내역)
    private List<StockInfo> _sellStockInfos = new List<StockInfo>();

    private void Update()
    {
        // 매수 시도할 리스트 임시 저장
        List<StockInfo> tempBuyList = new List<StockInfo>();

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

        bool isTradingStart = true;
        bool isTradingEnd = false;

        // 계좌 잔고 리스트
        var balanceStocks = AccountInfo.BalanceStocks;
        while (true)
        {
            recommendeds.Clear();
            if (_tradingStartTime.Date != ProgramConfig.NowTime.Date)
                ResetTimer();

            if (IsAutoProgramOrder)
            {
                // 설정된 거래 시간 범위 기준으로 거래 진행 + 목표 수익률 실현하면 거래 안함
                if (_tradingStartTime <= ProgramConfig.NowTime && _tradingEndTime >= ProgramConfig.NowTime)
                {
                    // 0. 최초 시작
                    if (isTradingStart)
                    {
                        isTradingStart = false;
                        isTradingEnd = false;
                        _sellStockInfos.Clear();

                        // 모두 주문 취소하기
                        for (int i = 0; i < balanceStocks.Count; i++)
                        {
                            var balanceStock = balanceStocks[i];
                            if (balanceStock.BalanceStockState != eBalanceStockState.Have)
                                OrderCancel(balanceStock.stockInfo);
                        }
                    }

                    // 1. 매수 예정 리스트 체크
                    for (int i = 0; i < orderStockInfos.Count; i++)
                    {
                        var stockInfo = orderStockInfos[i];
                        if (stockInfo != null)
                        {
                            bool isBuy = false;

                            // 조건: 당일 갱신 기준
                            if (stockInfo.RefreshTime.Date == ProgramConfig.NowTime.Date)
                            {
                                // 조건: 매수시 등락율 범위
                                if (stockInfo.UpDownRate >= StartRate && stockInfo.UpDownRate <= LimitRate)
                                {
                                    // 조건: 분당 성장률 2%이상일 때
                                    if (stockInfo.GrowthRatePerMinute >= BaseGrowthRatePerMinute)
                                    {
                                        isBuy = true;
                                    }
                                }
                            }

                            if (isBuy)
                            {
                                if (!recommendeds.Exists(stock => stockInfo.Equals(stock)) &&
                                    !_sellStockInfos.Exists(stock => stockInfo.Equals(stock)))
                                {
                                    // 동전주는 위험하니 받지 않는다.
                                    //if (stockInfo.StockPrice > 1000)
                                        recommendeds.Add(stockInfo);
                                }
                            }
                        }
                    }

                    // 1-1. 매수하기
                    // 주문 진행 중인 경우에는 불가하도록 (완전히 계산되지 않을 때는 엉뚱하게 계속 주문하게됨. 이를 방지)
                    // + 목표 수익률 달성하면 매수 안 함.
                    bool isBuyAvailableState = !AccountInfo.BalanceStocks.Exists(balanceStock => balanceStock.BalanceStockState != eBalanceStockState.Have);
                    if (isBuyAvailableState && !IsCompleteTodyTrading)
                    {
                        if (AccountInfo != null)
                        {
                            long useAvailableMoney = AccountInfo.AvailableMoney;
                            // 사용 가능 금액이 총 평가 금액보다 50% 이상 많을 경우에 매수 시도
                            // 계속 반복하다보면 1개씩 매입하는 비효율 현상이 생겨서 분기태움.(안전 장치)
                            if (useAvailableMoney >= AccountInfo.EstimatedAssets_Calc * 0.5f)
                            {
                                if (TryStockSellCount <= recommendeds.Count)
                                {
                                    // 잔고 최대 보유 가능 종목 개수 넘지 않도록 처리
                                    tempBuyList.Clear();
                                    for (int i = 0; i < recommendeds.Count; i++)
                                    {
                                        if (AccountInfo.BalanceStocks.Count + i < MaxHaveStockCount)
                                            tempBuyList.Add(recommendeds[i]);
                                    }

                                    float sumGrowthRatePerMinute = 0f;
                                    for (int i = 0; i < tempBuyList.Count; i++)
                                    {
                                        sumGrowthRatePerMinute += tempBuyList[i].GrowthRatePerMinute;
                                    }

                                    for (int i = 0; i < tempBuyList.Count; i++)
                                    {
                                        var stockInfo = tempBuyList[i];
                                        // 매수 시도할 분배된 금액
                                        int buyMoney = (int)(useAvailableMoney * (stockInfo.GrowthRatePerMinute / sumGrowthRatePerMinute));
                                        // 매수 개수
                                        int buyCount = Math.Min(buyMoney / stockInfo.StockPrice, MaxBuyCount);
                                        // 매수
                                        if (buyCount > 0)
                                        {
                                            OrderBuy(stockInfo, buyCount);
                                            _sellStockInfos.Add(stockInfo);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // 2. 매도 체크
                    for (int i = 0; i < balanceStocks.Count; i++)
                    {
                        if (balanceStocks[i].BalanceStockState == eBalanceStockState.Have)
                        {
                            bool isSell = false;
                            // 조건1: 최대 손익율에서 스탑 로스 책정
                            if (balanceStocks[i].MaxProfitRate - balanceStocks[i].EstimatedProfitRate >= StopLoss)
                            {
                                isSell = true;
                            }

                            // 조건2: 최대 이익 시점 도달하는 경우
                            //if (balanceInfos[i].EstimatedProfitRate >= MaxPriceRate)
                            //{
                            //    isSell = true;
                            //}

                            // 조건3: 특정 손익율 도달하는 경우
                            //if (balanceStocks[i].EstimatedProfitRate >= SellProfit)
                            //{
                            //    isSell = true;
                            //}

                            // 조건4: 보유 시간이 20분 이상 지나면 목표치를 절반으로 줄임
                            //if (balanceStocks[i].EstimatedProfitRate >= SellProfit / 2f)
                            //{
                            //    isSell = true;
                            //}

                            // 조건5: 목표 등락율 달성
                            if (balanceStocks[i].stockInfo.UpDownRate >= balanceStocks[i].targetUpDownRate)
                            {
                                isSell = true;
                            }

                            // 조건6: 최대 등락율(25%) 달성, 조건5 방어 코드
                            if (balanceStocks[i].stockInfo.UpDownRate >= MaxPriceRate)
                            {
                                isSell = true;
                            }

                            if (isSell)
                            {
                                if (!sellStocks.Exists(item => item.Equals(balanceStocks[i])))
                                    sellStocks.Add(balanceStocks[i]);
                            }
                        }
                    }

                    // 2-1. 매도하기
                    for (int i = 0; i < sellStocks.Count; i++)
                    {
                        var balanceStock = sellStocks[i];
                        if (balanceStock.BalanceStockState == eBalanceStockState.Have)
                            OrderSell(balanceStock.stockInfo, balanceStock.HaveCnt);
                    }
                    sellStocks.Clear();

                    // 3. 너무 오래동안 매수 주문 걸려있으면 취소 처리
                    for (int i = 0; i < balanceStocks.Count; i++)
                    {
                        var balanceStock = balanceStocks[i];
                        if (balanceStock.BalanceStockState == eBalanceStockState.RequestBuy)
                        {
                            // 10분 이상인 경우 취소 처리
                            if (balanceStock.OrderTime != null)
                            {
                                if (balanceStock.OrderTime.Value.AddMinutes(10) <= ProgramConfig.NowTime)
                                    OrderCancel(balanceStock.stockInfo);
                            }
                        }
                    }
                }
                else
                {
                    isTradingStart = true;
                    if (!isTradingEnd)
                    {
                        if (IsCompleteTodyTrading)
                        {
                            LineNotify.SendMessage($"{AccountInfo.TodayProfitRate:F2}%의 수익으로 금일 거래에 안정적인 거래로 완료되었습니다.😆" +
                                                    $"\n(설정된 목표 수익률 : {TodayTargetAccountProfitRate:F2}%)");
                        }
                        else
                        {
                            LineNotify.SendMessage($"{AccountInfo.TodayProfitRate:F2}%의 수익으로 금일 거래가 아쉽게 마무리되었습니다.😭" +
                                                    $"\n(설정된 목표 수익률 : {TodayTargetAccountProfitRate:F2}%)");
                        }

                        isTradingEnd = true;

                        // 모두 주문 취소하기
                        for (int i = 0; i < balanceStocks.Count; i++)
                        {
                            var balanceStock = balanceStocks[i];
                            if (balanceStock.BalanceStockState == eBalanceStockState.RequestBuy)
                            {
                                OrderCancel(balanceStock.stockInfo);
                            }
                        }

                        // 손해 보더라도 더 이상의 거래는 무의미하므로 접는다.
                        // 모두 매도하기
                        for (int i = 0; i < balanceStocks.Count; i++)
                        {
                            var balanceStock = balanceStocks[i];
                            if (balanceStock.BalanceStockState == eBalanceStockState.Have)
                                OrderSell(balanceStock.stockInfo, balanceStock.HaveCnt);
                        }
                    }
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