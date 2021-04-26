using System;
using System.Collections.Generic;

[Serializable]
public class AccountInfo
{
    public string AccountNumber { get; private set; }

    /// <summary>
    /// 금일 시작 D+2예수금
    /// </summary>
    public long TodayStartDepositAfter2Day = 0;

    /// <summary>
    /// 예수금
    /// </summary>
    public long Deposit { get; set; }

    /// <summary>
    /// D+2예수금
    /// </summary>
    public long DepositAfter2Day { get; set; }

    /// <summary>
    /// 사용 가능 금액 (주문 시 주문 가격도 차감됨)
    /// </summary>
    public long AvailableMoney { get; set; }

    /// <summary>
    /// 추정 자산
    /// </summary>
    public long EstimatedAssets { get; set; }

    /// <summary>
    /// 총 매입 금액
    /// </summary>
    public long TotalBuyAmount { get; set; }

    /// <summary>
    /// 총 평가 금액
    /// </summary>
    public long TotalEvaluationAmount { get; set; }

    /// <summary>
    /// 당일 손익 금액
    /// </summary>
    public long TodayProfitAmount { get; set; }

    /// <summary>
    /// 당일 손익률
    /// </summary>
    public float TodayProfitRate => TodayStartDepositAfter2Day != 0 ? (TodayProfitAmount - TodayStartDepositAfter2Day) / (float)TodayStartDepositAfter2Day * 100f : 0f;

    /// <summary>
    /// 보유 종목 리스트
    /// </summary>
    public List<BalanceStock> BalanceStocks { get; private set; } = new List<BalanceStock>();

    public AccountInfo(string accountNumber)
    {
        this.AccountNumber = accountNumber;
        AccountInfoUtils.ApplyLoadData(this);
    }

    /// <summary>
    /// 계좌 정보 요청 (TR 사용)
    /// </summary>
    /// <param name="passward"></param>
    public void RequestAccountInfo(string passward, Action<bool> onReceive = null)
    {
        KiwoomManager.Instance.SetTrValue("계좌번호", this.AccountNumber);
        KiwoomManager.Instance.SetTrValue("비밀번호", passward);
        KiwoomManager.Instance.SetTrValue("비밀번호입력매체구분", "00");
        KiwoomManager.Instance.SetTrValue("조회구분", "1");
        KiwoomManager.Instance.RequestTransaction(eOPTCode.계좌평가잔고내역요청);

        KiwoomManager.Instance.SetTrValue("계좌번호", this.AccountNumber);
        KiwoomManager.Instance.SetTrValue("비밀번호", passward);
        KiwoomManager.Instance.SetTrValue("상장폐지조회구분", "0");
        KiwoomManager.Instance.SetTrValue("비밀번호입력매체구분", "00");
        KiwoomManager.Instance.RequestTransaction(eOPTCode.계좌평가현황요청);

        //KiwoomManager.Instance.SetTrValue("계좌번호", this.AccountNumber);
        //KiwoomManager.Instance.RequestTransaction(eOPTCode.계좌수익률요청);

        KiwoomManager.Instance.SetTrValue("계좌번호", this.AccountNumber);
        KiwoomManager.Instance.SetTrValue("체결구분", "1");
        KiwoomManager.Instance.SetTrValue("매매구분", "2");
        KiwoomManager.Instance.RequestTransaction(eOPTCode.미체결요청, onReceive: onReceive);
    }

    /// <summary>
    /// 총 매입 금액 세팅
    /// </summary>
    /// <param name="totalBuyAmount"></param>
    public void SetTotalBuyAmount(long totalBuyAmount)
    {
        this.TotalBuyAmount = totalBuyAmount;
    }

    /// <summary>
    /// 총 평가 금액 세팅
    /// </summary>
    /// <param name="TotalEvaluationAmount"></param>
    public void SetTotalEvaluationAmount(long TotalEvaluationAmount)
    {
        this.TotalEvaluationAmount = TotalEvaluationAmount;
    }

    ///// <summary>
    ///// 당일 손익 금액 세팅
    ///// </summary>
    ///// <param name="todayProfitAmount"></param>
    //public void SetTodayProfitAmount(long todayProfitAmount)
    //{
    //    this.TodayProfitAmount = todayProfitAmount;
    //}

    ///// <summary>
    ///// 당일 손익률 세팅
    ///// </summary>
    ///// <param name="todayProfitRate"></param>
    //public void SetTodayProfitRate(float todayProfitRate)
    //{
    //    this.TodayProfitRate = todayProfitRate;
    //}

    /// <summary>
    /// 보유 종목 추가
    /// </summary>
    /// <param name="traingSymbol"></param>
    /// <param name="haveCnt"></param>
    /// <param name="buyingMoney"></param>
    public void BalanceStock(string traingSymbol, int haveCnt, long buyingMoney, int stockPrice)
    {
        BalanceStock balanceStock = GetMyBalanceStock(traingSymbol);
        if (balanceStock != null)
        {
            // 수정
            balanceStock.SetData(traingSymbol, haveCnt, buyingMoney, stockPrice);
        }
        else
        {
            // 추가
            balanceStock = new BalanceStock(traingSymbol, haveCnt, buyingMoney, stockPrice);
            this.BalanceStocks.Add(balanceStock);
        }
    }

    /// <summary>
    /// 종목 주문 접수
    /// </summary>
    /// <param name="orderNumber">주문 번호</param>
    /// <param name="traingSymbol">종목 번호</param>
    /// <param name="orderCount">주문 수량</param>
    /// <param name="orderPrice">주문 가격</param>
    /// <param name="waitOrderCnt">미체결 수량</param>
    /// <param name="orderType">주문 타입</param>
    /// <param name="orderTime">주문 시간</param>
    /// <param name="isSendOrderSuccess">주문 접수 완료 여부</param>
    public void OrderStock(string screenNumber, string orderNumber, string traingSymbol, int orderCount, long orderPrice, int waitOrderCnt, string orderType, string orderTime, bool isSendOrderSuccess)
    {
        // orderTime 확인 필요
        BalanceStock balanceStock = GetMyBalanceStock(traingSymbol);
        if (orderType.Contains("매수"))
        {
            if (balanceStock == null)
            {
                balanceStock = new BalanceStock(traingSymbol, 0, 0);
                BalanceStocks.Add(balanceStock);
            }
            balanceStock.SetOrder(screenNumber, isSendOrderSuccess, orderNumber, orderCount, orderPrice, waitOrderCnt, DateTime.MinValue);
            balanceStock.SetOrderState(eBalanceStockState.Buying);
        }
        else if (orderType.Contains("매도"))
        {
            if (balanceStock != null)
            {
                if (orderType.Contains("취소"))
                {
                    balanceStock.InitializeOrderState();
                }
                else
                {
                    balanceStock.SetOrder(screenNumber, isSendOrderSuccess, orderNumber, orderCount, orderPrice, waitOrderCnt, DateTime.MinValue);
                    balanceStock.SetOrderState(eBalanceStockState.Selling);
                }
            }
        }
    }

    /// <summary>
    /// 종목 거래 체결
    /// </summary>
    public void TradingStock(string traingSymbol, int resultCount, long resultPrice, int waitOrderCnt, int orderCount, string orderType, long fees, long tax)
    {
        bool isEndTrading = waitOrderCnt == 0;  // 거래가 완전히 끝났는지?
        BalanceStock balanceStock = GetMyBalanceStock(traingSymbol);
        if (balanceStock != null)
        {
            if (orderType.Contains("매수"))
            {
                if (isEndTrading)
                {
                    //this.Deposit -= resultPrice - fees - tax;
                    this.DepositAfter2Day -= resultPrice - fees - tax;
                }
                balanceStock.SetBuyCount(waitOrderCnt, resultCount, resultPrice);
            }
            else if (orderType.Contains("매도"))
            {
                if (isEndTrading)
                {
                    //this.Deposit += resultPrice - fees - tax;
                    this.DepositAfter2Day += resultPrice - fees - tax;
                    this.AvailableMoney += resultPrice - fees - tax;

                    if (balanceStock.BuyingMoney != 0)
                    {
                        TodayProfitAmount += resultPrice - fees - tax - balanceStock.BuyingMoney;
                        var profit = resultPrice - fees - tax - balanceStock.BuyingMoney;
                        var profitRate = profit / balanceStock.BuyingMoney * 100f;
                        LineNotify.SendMessage($"{balanceStock.StockName}를 매도 체결되었습니다. 차익 : {profit:N0}원({profitRate:F2}%)");
                        LineNotify.SendMessage($"balanceStock.BuyingMoney: {balanceStock.BuyingMoney}, resultPrice: {resultPrice}, fees: {fees}, tax: {tax}");
                    }
                    else
                        LineNotify.SendMessage($"{balanceStock.StockName}를 매도 체결되었습니다. 차익 계산에 실패했습니다. 이익률에 반영하지 않습니다.");

                    BalanceStocks.Remove(balanceStock);
                }
                balanceStock.SetSellCount(waitOrderCnt, resultCount, orderCount);
            }
        }
        SaveAccountInfo();
    }

    /// <summary>
    /// 보유 주식 가져오기 (null일 수 있음)
    /// </summary>
    /// <param name="tradingSymbol"></param>
    /// <returns></returns>
    public BalanceStock GetMyBalanceStock(string tradingSymbol)
    {
        return this.BalanceStocks.Find(stock => stock.TraingSymbol.Contains(tradingSymbol) || tradingSymbol.Contains(stock.TraingSymbol));
    }

    /// <summary>
    /// 파일 저장
    /// </summary>
    public void SaveAccountInfo()
    {
        AccountInfoUtils.SaveAccountInfos(this);
    }

    public override string ToString()
    {
        return this.AccountNumber;
    }
}

public static class AccountInfoUtils
{
    /// <summary>
    /// Key : 계좌번호
    /// Value : 날짜별 해당 계좌 정보
    /// </summary>
    private static Dictionary<string, Dictionary<DateTime, AccountInfo>> _accountInfos = null;

    public static Dictionary<string, Dictionary<DateTime, AccountInfo>> AccountInfos
    { 
        get
        {
            if (_accountInfos == null)
                LoadAccountInfos();
            return _accountInfos;
        }
    }

    public static void LoadAccountInfos()
    {
        _accountInfos = Utils.FileLoad<Dictionary<string, Dictionary<DateTime, AccountInfo>>>("AccountInfos");
        if (_accountInfos == null)
            _accountInfos = new Dictionary<string, Dictionary<DateTime, AccountInfo>>();
    }

    public static void SaveAccountInfos(AccountInfo accountInfo)
    {
        if (_accountInfos == null)
            LoadAccountInfos();

        Dictionary<DateTime, AccountInfo> accountInfos = null;
        if (_accountInfos.TryGetValue(accountInfo.AccountNumber, out accountInfos))
        {
            AccountInfo todayAccountInfo = null;
            if (accountInfos.TryGetValue(ProgramConfig.NowTime.Date, out todayAccountInfo))
            {
                todayAccountInfo = accountInfo;
            }
            else
            {
                accountInfos.Add(ProgramConfig.NowTime.Date, accountInfo);
            }
        }
        else
        {
            accountInfos = new Dictionary<DateTime, AccountInfo>();
            accountInfos.Add(ProgramConfig.NowTime.Date, accountInfo);
            _accountInfos.Add(accountInfo.AccountNumber, accountInfos);
        }

        Utils.FileSave("AccountInfos", _accountInfos);
    }

    /// <summary>
    /// 금일 저장된 파일 내용 불러오기
    /// </summary>
    /// <returns></returns>
    public static void ApplyLoadData(AccountInfo accountInfo)
    {
        if (_accountInfos == null)
            LoadAccountInfos();

        Dictionary<DateTime, AccountInfo> accountInfos = null;
        if (_accountInfos.TryGetValue(accountInfo.AccountNumber, out accountInfos))
        {
            AccountInfo todayAccountInfo = null;
            if (accountInfos.TryGetValue(ProgramConfig.NowTime.Date, out todayAccountInfo))
            {
                // 금일 시작 금액
                accountInfo.TodayStartDepositAfter2Day = todayAccountInfo.TodayStartDepositAfter2Day;

                // 금일 손익금
                accountInfo.TodayProfitAmount = todayAccountInfo.TodayProfitAmount;
            }
        }
    }
}