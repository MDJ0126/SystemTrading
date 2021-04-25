using AxKHOpenAPILib;
using System;
using System.Collections.Generic;
using System.Text;

public partial class KiwoomManager : Singleton<KiwoomManager>
{
    /// <summary>
    /// OpenAPI 컨트롤러
    /// (참고 : https://download.kiwoom.com/web/openapi/kiwoom_openapi_plus_devguide_ver_1.1.pdf)
    /// </summary>
    public AxKHOpenAPI AxKHOpenAPI { get; private set; } = null;

    /// <summary>
    /// 로그인 정보
    /// </summary>
    public LoginInfo LoginInfo { get; private set; } = new LoginInfo();

    /// <summary>
    /// 로그인 상태
    /// </summary>
    /// <returns></returns>
    public bool IsLogin => LoginInfo.IsAllowLoginInfo;

    /// <summary>
    /// 연결되었는지
    /// </summary>
    public bool IsConnect { get { return LoginInfo.IsAllowLoginInfo && this.AxKHOpenAPI != null; } }

    private Action<bool> _onFinishedLogin;

    /// <summary>
    /// API 세팅
    /// </summary>
    /// <param name="axKHOpenAPI"></param>
    public void SetAPI(AxKHOpenAPI axKHOpenAPI)
    {
        // API를 View단에서 제공해서 따로 가져옴 (winform을 사용하면서 어쩔 수가 없는 부분이 있는 것 같음)
        this.AxKHOpenAPI = axKHOpenAPI;
    }

    /// <summary>
    /// 로그인창 띄우기
    /// </summary>
    public void OpenLoginForm(Action<bool> onFinshed)
    {
        if (AxKHOpenAPI != null)
        {
            int result = this.AxKHOpenAPI.CommConnect();
            if (result == 0)
            {
                Logger.Log("Open Login Form");
                this.AxKHOpenAPI.OnReceiveTrData        += OnReceiveTransactionData;
                this.AxKHOpenAPI.OnReceiveRealData      += OnReceiveRealData;
                this.AxKHOpenAPI.OnReceiveChejanData    += OnReceiveChejanData;
                this.AxKHOpenAPI.OnEventConnect         += OnEventConnect;
                this.AxKHOpenAPI.OnReceiveRealCondition += OnReceiveRealCondition;
                this.AxKHOpenAPI.OnReceiveTrCondition   += OnReceiveTrCondition;
                this.AxKHOpenAPI.OnReceiveMsg           += OnReceiveMsg;
                this.AxKHOpenAPI.OnReceiveConditionVer  += OnReceiveConditionVer;
                _onFinishedLogin = onFinshed;
            }
            else
            {
                Logger.LogError("Failed Open Login Form");
                onFinshed.Invoke(false);
            }
        }
    }

    /// <summary>
    /// 로그아웃
    /// </summary>
    public void LogOut()
    {
        if (this.IsLogin)
        {
            LineNotify.SendMessage($"'{this.LoginInfo.UserName}'님의 계정이 시스템 트레이딩에서 로그아웃 처리되었습니다.");
            this.LoginInfo.Clear();
            DisconnectAllRealData();
            Logger.Log("[Logout]");
        }
    }
    // 0부터 시작하면 안됨
    // 사용되는 스크린 번호가 200개가 넘어가면 예기치 못한 오류가 생길 수 있다고 한다.
    private const int SCREEN_AUTO_NUMBER_MIN = 1;
    private const int SCREEN_AUTO_NUMBER_MAX = 100;

    // 주식 종목 실시간 전용 화면 번호
    private static int _stockScreenNumber = 150;
    private static int _registerStockScreenNumerCount = 0;
    private const int STOCK_SCREEN_NUMBER_COUNT = 100;
    
    // 고유 화면 번호 리스트
    public const string SCREEN_NUMBER_KOSPI_INDEX       = "0111";   // 코스피 지수
    public const string SCREEN_NUMBER_KOSDAQ_INDEX      = "0112";   // 코스닥 지수
    public const string SCREEN_NUMBER_KOSPI200_INDEX    = "0113";   // 코스피200 지수
    private static int _scrNum = SCREEN_AUTO_NUMBER_MIN;

    /// <summary>
    /// 주식 종목 전용 화면 번호
    /// </summary>
    /// <returns></returns>
    public static string GetStockScrNum()
    {
        // 실시간 등록은 화면 번호당 최대 100종목까지 지원함
        if (_registerStockScreenNumerCount + 1 > STOCK_SCREEN_NUMBER_COUNT)
        {
            ++_stockScreenNumber;
            _registerStockScreenNumerCount = 0;
        }
        ++_registerStockScreenNumerCount;
        return _stockScreenNumber.ToString("D4");
    }

    /// <summary>
    /// 화면번호 생산
    /// </summary>
    /// <returns></returns>
    private string GetScrNum()
    {
        if (_scrNum < SCREEN_AUTO_NUMBER_MAX)
            _scrNum++;
        else
            _scrNum = SCREEN_AUTO_NUMBER_MIN;

        return _scrNum.ToString("D4");
    }

    /// <summary>
    /// 실시간 연결 종료
    /// </summary>
    private void DisconnectAllRealData()
    {
        for (int i = _scrNum; i > 5000; i--)
        {
            this.AxKHOpenAPI.DisconnectRealData(i.ToString());
        }
        _scrNum = 5000;
    }

    /// <summary>
    /// 통신 연결 상태 변경시 이벤트
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
    {
        if (!Error.IsError(e.nErrCode))
        {
            // 주식 리스트 정보 받아오기
            List<StockInfo> stockList = new List<StockInfo>();
            string kospiStr = this.AxKHOpenAPI.GetCodeListByMarket("0");
            string kosdaqStr = this.AxKHOpenAPI.GetCodeListByMarket("10");
            string stockStr = kospiStr + kosdaqStr;
            string[] stockCodeArray = stockStr.Split(';');
            for (int i = 0; i < stockCodeArray.Length; i++)
            {
                string tradingSymbol = stockCodeArray[i];
                if (!string.IsNullOrEmpty(tradingSymbol))
                    stockList.Add(new StockInfo(tradingSymbol, this.AxKHOpenAPI.GetMasterCodeName(tradingSymbol)));
            }
            StockListManager.Instance.ApplyStockInfos(stockList);

            // 로그인 정보 세팅 (로그인 완료 콜백)
            this.LoginInfo.SetLoginInfo(this.AxKHOpenAPI, _onFinishedLogin);
            LineNotify.SendMessage($"'{this.LoginInfo.UserName}'님의 계정으로 시스템 트레이딩에 연결되었습니다.");
            Logger.Log("[Loggin Success] " + Error.GetErrorMessage());
            _onLogin?.Invoke();

            // 장시작시간 통신 등록
            RegisterRealData("", eFID.장운영구분, eFID.체결시간, eFID.장시작예상잔여시간);
            HandlerKiwoomAPI.Instance.NotifyOnConnect();
        }
        else
        {
            _onFinishedLogin?.Invoke(false);
            Logger.Log("[Loggin Failed] " + Error.GetErrorMessage());
        }
    }

    /// <summary>
    /// 주문 중인 종목(잔고)
    /// </summary>
    private List<BalanceStock> _requestBalanceStocks = new List<BalanceStock>();

    /// <summary>
    /// 종목 주문
    /// </summary>
    /// <param name="sendOrderType">주문 타입</param>
    /// <param name="stockInfo">종목 정보</param>
    /// <param name="count">수량</param>
    /// <param name="price">가격</param>
    /// <param name="sendType">거래 구분</param>
    /// <param name="orderNumber">원주문번호, 정정/취소에서만 입력</param>
    public void SendOrder(string accountNumber, eSendOrderType sendOrderType, StockInfo stockInfo, int count, int price, eSendType sendType, string orderNumber = "")
    {
        // 중복 주문 못 하도록 처리
        // 중복 주문을 했을 경우, 기존 주문을 정정한다.
        AccountInfo accountInfo = LoginInfo.GetAccountInfo(accountNumber);
        if (accountInfo != null)
        {
            var balanceStock = accountInfo.GetMyBalanceStock(stockInfo.tradingSymbol);
            if (balanceStock != null)
            {
                if (!string.IsNullOrEmpty(balanceStock.OrderNumber))
                {
                    // 주문이 있으면 정정
                    switch (sendOrderType)
                    {
                        case eSendOrderType.신규매수:
                            {
                                if (balanceStock.BalanceStockState == eBalanceStockState.Buying)
                                    sendOrderType = eSendOrderType.매수정정;
                                else
                                {
                                    return;
                                }
                            }
                            break;
                        case eSendOrderType.신규매도:
                            {
                                if (balanceStock.BalanceStockState == eBalanceStockState.Selling)
                                    sendOrderType = eSendOrderType.매도정정;
                                else
                                {
                                    return;
                                }
                            }
                            break;
                    }
                    orderNumber = balanceStock.OrderNumber;
                    count = balanceStock.OrderCount;
                }
            }

            if (sendType == eSendType.시장가)
                price = 0;
            eOPTCode optCode = Utils.FindEnumValue<eOPTCode>(sendOrderType.ToString());

            if (this.IsLogin)
            {
                string screenNumber = GetScrNum();

                TransactionData requestTransactionData = new TransactionData(screenNumber, stockInfo.tradingSymbol, optCode, inputValues, null);
                RequestTransactionDatas.Add(requestTransactionData);

                if (balanceStock == null)
                {
                    balanceStock = new BalanceStock(stockInfo.tradingSymbol, 0, 0, stockInfo.StockPrice);
                    if (!accountInfo.BalanceStocks.Exists(stock => stock.Equals(balanceStock.stockInfo)))
                        accountInfo.BalanceStocks.Add(balanceStock);
                }

                if (!_requestBalanceStocks.Exists(data => data.Equals(balanceStock)))
                    _requestBalanceStocks.Add(balanceStock);

                switch (sendOrderType)
                {
                    case eSendOrderType.신규매수:
                    case eSendOrderType.신규매도:
                        {
                            if (sendOrderType == eSendOrderType.신규매수)
                                accountInfo.AvailableMoney -= price * count;

                            if (balanceStock != null) balanceStock.SetOrderState(eBalanceStockState.RequestBuy);
                            this.AxKHOpenAPI.SendOrder(sendOrderType.ToString(), screenNumber, accountNumber, (int)sendOrderType, stockInfo.tradingSymbol, count, price, sendType.ToDescription(), "");
                        }
                        break;
                    case eSendOrderType.매수정정:
                    case eSendOrderType.매도정정:
                        {
                            if (sendOrderType == eSendOrderType.매수정정)
                            {
                                accountInfo.AvailableMoney += balanceStock.OrderPrice;
                                accountInfo.AvailableMoney -= price * count;
                            }

                            if (balanceStock != null) balanceStock.SetOrderState(eBalanceStockState.RequestSell);
                            this.AxKHOpenAPI.SendOrder(sendOrderType.ToString(), screenNumber, this.LoginInfo.SelectAccount.AccountNumber, (int)sendOrderType, stockInfo.tradingSymbol, count, 0, sendType.ToDescription(), orderNumber);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 주문 취소 요청
    /// </summary>
    /// <param name="accountNumber"></param>
    /// <param name="stockInfo"></param>
    public void SendCancelOrder(string accountNumber, StockInfo stockInfo)
    {
        AccountInfo accountInfo = LoginInfo.GetAccountInfo(accountNumber);
        if (accountInfo != null)
        {
            var balanceStock = accountInfo.GetMyBalanceStock(stockInfo.tradingSymbol);
            if (balanceStock != null && !string.IsNullOrEmpty(balanceStock.OrderNumber))
            {
                eSendOrderType sendOrderType = eSendOrderType.None;
                if (balanceStock.BalanceStockState == eBalanceStockState.Buying) sendOrderType = eSendOrderType.매수취소;
                if (balanceStock.BalanceStockState == eBalanceStockState.Selling) sendOrderType = eSendOrderType.매도취소;
                if (sendOrderType != eSendOrderType.None)
                {
                    eOPTCode optCode = Utils.FindEnumValue<eOPTCode>(sendOrderType.ToString());
                    TransactionData requestTransactionData = new TransactionData(balanceStock.OrderScreenNumber, stockInfo.tradingSymbol, optCode, inputValues, null);
                    RequestTransactionDatas.Add(requestTransactionData);

                    var result = this.AxKHOpenAPI.SendOrder(sendOrderType.ToString(), balanceStock.OrderScreenNumber, this.LoginInfo.SelectAccount.AccountNumber, (int)sendOrderType, stockInfo.tradingSymbol, balanceStock.OrderCount, 0, eSendType.시장가.ToDescription(), balanceStock.OrderNumber);
                    if (result == 0)
                    {
                        if (_requestBalanceStocks.Exists(data => data.Equals(balanceStock)))
                            _requestBalanceStocks.Remove(balanceStock);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 로컬에 사용자조건식 저장 성공여부 응답 이벤트
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnReceiveConditionVer(object sender, _DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
    {
        Logger.Log("로컬에 사용자조건식 저장 성공여부 응답 이벤트");
    }

    /// <summary>
    /// 조건검색 조회응답 이벤트
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnReceiveTrCondition(object sender, _DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
    {
        Logger.Log("조건검색 조회응답 이벤트");
    }

    /// <summary>
    /// 조건검색 실시간 편입,이탈종목 이벤트
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnReceiveRealCondition(object sender, _DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
    {
        Logger.Log("조건검색 실시간 편입,이탈종목 이벤트");
    }

    /// <summary>
    /// 주문 접수/확인 수신시 이벤트
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnReceiveChejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
    {
        Logger.Log("주문 접수/확인 수신시 이벤트");
        switch (e.sGubun)
        {
            case "0":
                // 주문과 체결
                AccountInfo accountInfo = LoginInfo.GetAccountInfo(GetSendOrderResultData(eFID.계좌번호));
                if (accountInfo != null)
                {
                    string orderNumber;
                    string traingSymbol;
                    string stockName;
                    int orderCount;
                    long orderPrice;
                    int waitOrderCount;
                    long currentPrice;
                    string orderType;
                    string orderTime;
                    string screenNumber;
                    int resultCount;
                    long resultPrice;
                    long tradePrice;
                    long fees;
                    long tax;

                    orderNumber = GetSendOrderResultData(eFID.주문번호);
                    traingSymbol = GetSendOrderResultData(eFID.종목코드_업종코드);
                    stockName = GetSendOrderResultData(eFID.종목명);
                    int.TryParse(GetSendOrderResultData(eFID.주문수량), out orderCount);    // 최초 주문했던 수량이 있다.
                    long.TryParse(GetSendOrderResultData(eFID.주문가격), out orderPrice);
                    int.TryParse(GetSendOrderResultData(eFID.미체결수량).Replace("-", ""), out waitOrderCount);  // 체결되지 못한 총량
                    long.TryParse(GetSendOrderResultData(eFID.현재가), out currentPrice);
                    orderType = GetSendOrderResultData(eFID.주문구분);
                    orderTime = GetSendOrderResultData(eFID.주문체결시간);
                    screenNumber = GetSendOrderResultData(eFID.화면번호);
                    long.TryParse(GetSendOrderResultData(eFID.체결누계금액), out resultPrice);
                    int.TryParse(GetSendOrderResultData(eFID.체결량_누적), out resultCount); // 체결될 때마다 수신하여 누적한 값을 준다. (체결된 총량)
                    long.TryParse(GetSendOrderResultData(eFID.체결가), out tradePrice);
                    long.TryParse(GetSendOrderResultData(eFID.당일매매수수료), out fees);
                    long.TryParse(GetSendOrderResultData(eFID.당일매매세금), out tax);

                    string state = GetSendOrderResultData(eFID.주문상태);
                    if (state.Contains("체결"))
                    {
                        // 체결
                        accountInfo.TradingStock(traingSymbol, resultCount, resultPrice, waitOrderCount, orderCount, orderType, fees, tax);

                        BalanceStock balanceStock = accountInfo.GetMyBalanceStock(traingSymbol);
                        if (balanceStock != null && balanceStock.BalanceStockState == eBalanceStockState.Have)
                        {
                            if (_requestBalanceStocks.Exists(data => data.Equals(balanceStock)))
                                _requestBalanceStocks.Remove(balanceStock);
                        }
                    }
                    else
                    {
                        // 주문 접수
                        accountInfo.OrderStock(screenNumber, orderNumber, traingSymbol, orderCount, orderPrice, waitOrderCount, orderType, orderTime, true);
                    }
                }
                break;
            case "1":
                // 국내주식 잔고 변경 통보
                // OPT10085 참고로 수익률 반영이 가능하다고 하는데.. 흐음
                break;
            case "4":
                // 파생잔고 변경 통보

                break;
            default:
                break;
        }
        
        HandlerKiwoomAPI.Instance.NotifyOnReceiveChejanData();
    }

    /// <summary>
    /// OnReceiveChejanData에서 FID 값 가져오기
    /// </summary>
    /// <param name="tradingSymbol"></param>
    /// <param name="fid"></param>
    public string GetSendOrderResultData(eFID fid)
    {
        return this.AxKHOpenAPI.GetChejanData((int)fid);
    }

    /// <summary>
    /// 수신 메시지 이벤트
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnReceiveMsg(object sender, _DKHOpenAPIEvents_OnReceiveMsgEvent e)
    {
        Logger.Log(e.sMsg);
        if (e.sMsg.Contains("[00Z218]"))
        {
            // "[00Z218] 모의투자 장종료 상태입니다"
            var requestTransactionData = RequestTransactionDatas.Find(data => data.screenNumber.Equals(e.sScrNo));
            if (requestTransactionData != null)
            {
                RequestTransactionDatas.Remove(requestTransactionData);
                string tradingSymbol = requestTransactionData.stockInfo.tradingSymbol;
                var requestBalanceStock = _requestBalanceStocks.Find(stock => stock.TraingSymbol.Contains(tradingSymbol) || tradingSymbol.Contains(stock.TraingSymbol));
                if (requestBalanceStock != null)
                {
                    requestBalanceStock.RestoreOrderState();
                    _requestBalanceStocks.Remove(requestBalanceStock);
                }
            }
        }
        SystemTrading.Forms.ToastMessage.Show(e.sMsg);
    }

    protected override void Install()
    {
        ConnectingRealDatas = new List<ConnectingRealData>();
        RequestTransactionDatas = new List<TransactionData>();
        ResponseTransactionDatas = new List<TransactionData>();
        MultiThread.Start(RequestTransactionProcess);
    }

    protected override void Release()
    {
        if (ConnectingRealDatas != null)
        {
            ConnectingRealDatas.Clear();
            ConnectingRealDatas = null;
        }
        if (RequestTransactionDatas != null)
        {
            RequestTransactionDatas.Clear();
            RequestTransactionDatas = null;
        }
    }

    public delegate void OnAPIEvent();
    private event OnAPIEvent _onLogin;
    public event OnAPIEvent OnLogin
    {
        add
        {
            _onLogin -= value;
            _onLogin += value;
        }
        remove
        {
            _onLogin -= value;
        }
    }
}
