using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class AccountInfo
{
    public string AccountNumber { get; private set; }

    private DateTime _todayStartDepositAfter2DayRefreshTime = DateTime.MinValue;
    private long _todayStartDepositAfter2Day = 0;
    /// <summary>
    /// 금일 시작 D+2예수금
    /// </summary>
    public long TodayStartDepositAfter2Day
    { 
        get
        {
            if (_todayStartDepositAfter2DayRefreshTime.Date != ProgramConfig.NowTime.Date)
            {
                _todayStartDepositAfter2DayRefreshTime = ProgramConfig.NowTime;
                _todayStartDepositAfter2Day = DepositAfter2Day;
            }
            return _todayStartDepositAfter2Day;
        }
        set
        {
            _todayStartDepositAfter2DayRefreshTime = ProgramConfig.NowTime;
            _todayStartDepositAfter2Day = value;
        }
    }

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
    /// 추정 자산 (직접 계산한 예상 자산)
    /// </summary>
    public long EstimatedAssets_Calc 
    { 
        get
        {
            long calcEst = DepositAfter2Day;
            for (int i = 0; i < BalanceStocks.Count; i++)
            {
                var current = BalanceStocks[i];
                calcEst += (long)Math.Truncate(current.CurrentTotalPrice * 0.99385f);
            }
            return calcEst;
        } 
    }

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
    public float TodayProfitRate => TodayStartDepositAfter2Day != 0 ? (TodayProfitAmount / (float)TodayStartDepositAfter2Day) * 100f : -99f;

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

            if (orderType.Contains("취소"))
            {
                balanceStock.InitializeOrderState();
                BalanceStocks.Remove(balanceStock);
            }
            else
            {
                balanceStock.SetOrder(screenNumber, isSendOrderSuccess, orderNumber, orderCount, orderPrice, waitOrderCnt, DateTime.MinValue);
                balanceStock.SetOrderState(eBalanceStockState.Buying);
            }
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

    private StringBuilder _sb = new StringBuilder();
    /// <summary>
    /// 종목 거래 체결
    /// </summary>
    public void TradingStock(string traingSymbol, int unitPrice, int resultCount, long resultPrice, int waitOrderCnt, int orderCount, string orderType, long fees, long tax)
    {
        bool isEndTrading = waitOrderCnt == 0;  // 거래가 완전히 끝났는지?
        if (isEndTrading)
        {
            // 거래 수수료는 모두 합쳐서 발생(체결될 때마다 수수료가 누적되기에 마지막에 거래가 끝난 기준으로 한번에 정산)
            this.DepositAfter2Day -= fees;
            this.AvailableMoney -= fees;
            this.TodayProfitAmount -= fees;
        }
        // 세금은 각 단위체결당 발생
        this.DepositAfter2Day -= tax;
        this.AvailableMoney -= tax;
        this.TodayProfitAmount -= tax;

        BalanceStock balanceStock = GetMyBalanceStock(traingSymbol);
        if (balanceStock != null)
        {
            if (orderType.Contains("매수"))
            {
                balanceStock.SetBuyCount(waitOrderCnt, resultCount, resultPrice);
                balanceStock.buyFees = fees;
                balanceStock.buyTax += tax; // 매수 시, 세금 0원
                if (isEndTrading)
                {
                    this.DepositAfter2Day -= resultPrice;
                    this.AvailableMoney -= resultPrice;

                    _sb.Length = 0;
                    _sb.Append($"\n[{balanceStock.StockName}] 매수 체결되었습니다.");
                    _sb.Append($"\n=======상세명세서=======");
                    _sb.Append($"\n주문 수량: {orderCount:N0}개");
                    _sb.Append($"\n총 매입가: {resultPrice:N0}원");
                    _sb.Append($"\n거래수수료: {balanceStock.buyFees:N0}원");
                    _sb.Append($"\n세금: {balanceStock.buyTax:N0}원");
                    _sb.Append($"\n======================");
                    LineNotify.SendMessage(_sb.ToString());
                }
            }
            else if (orderType.Contains("매도"))
            {
                balanceStock.sellFees = fees;
                balanceStock.sellTax += tax;
                if (isEndTrading)
                {
                    this.DepositAfter2Day += resultPrice;
                    this.AvailableMoney += resultPrice;
                    this.TodayProfitAmount += resultPrice - balanceStock.BuyingMoney;       // 금일 손익금에 반영
                    BalanceStocks.Remove(balanceStock);

                    // 로그
                    long oneStockBuyMoney = balanceStock.BuyingMoney / orderCount;              // 스톡 하나당 매입가
                    long profit = resultPrice - fees - tax - balanceStock.BuyingMoney;          // 종목 손익금
                    float profitRate = profit / (float)balanceStock.BuyingMoney * 100f;         // 종목 손익률

                    _sb.Length = 0;
                    _sb.Append($"\n[{balanceStock.StockName}] 매도 체결되었습니다.");
                    _sb.Append($"\n=======상세명세서=======");
                    _sb.Append($"\n종목 개당 매입가: {oneStockBuyMoney:N0}원");
                    _sb.Append($"\n주문 수량: {orderCount:N0}개");
                    _sb.Append($"\n총 매입가: {balanceStock.BuyingMoney:N0}원");
                    _sb.Append($"\n총 매도가: {resultPrice:N0}원");
                    _sb.Append($"\n거래수수료: {balanceStock.sellFees:N0}원");
                    _sb.Append($"\n세금: {balanceStock.sellTax:N0}원");
                    _sb.Append($"\n======================");
                    _sb.Append($"\n차익=> {profit:N0}원({profitRate:F2}%)");
                    LineNotify.SendMessage(_sb.ToString());
                }
                balanceStock.SetSellCount(waitOrderCnt);
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
    /// 주문 취소 처리
    /// </summary>
    /// <param name="tradingSymbol"></param>
    public void OrderCancelBalanceStock(string tradingSymbol)
    {
        var balanceStock = this.BalanceStocks.Find(stock => stock.TraingSymbol.Contains(tradingSymbol) || tradingSymbol.Contains(stock.TraingSymbol));
        if (balanceStock != null)
        {
            balanceStock.InitializeOrderState();
            if (balanceStock.HaveCnt == 0)
                this.BalanceStocks.Remove(balanceStock);
        }
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