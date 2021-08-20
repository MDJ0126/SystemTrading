using AxKHOpenAPILib;
using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Transaction 직접 요청 정리
/// </summary>
public partial class KiwoomManager
{
    /// <summary>
    /// Tran 카운트 관리
    /// </summary>
    public TRCount TRCount { get; set; } = new TRCount();

    /// <summary>
    /// Tran 요청 리스트
    /// </summary>
    public List<TransactionData> RequestTransactionDatas { get; private set; } = null;

    public List<TransactionData> ResponseTransactionDatas { get; private set; } = null;

    Dictionary<string, string> inputValues = new Dictionary<string, string>();
    /// <summary>
    /// Transaction 데이터 세팅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    public void SetTrValue(string id, string value)
    {
        inputValues.Add(id, value);
    }

    /// <summary>
    /// SetTrValue()으로 세팅된 Transaction 요청
    /// </summary>
    public void RequestTransaction(eOPTCode optCode, string screenNumber = null, Action<bool> onReceive = null)
    {
        string tradingSymbol;
        inputValues.TryGetValue("종목코드", out tradingSymbol);
        TransactionData requestTransactionData = new TransactionData(screenNumber ?? GetScrNum(), tradingSymbol, optCode, inputValues, onReceive);
        inputValues.Clear();
        RequestTransactionDatas.Add(requestTransactionData);
        _waitQueue.Enqueue(requestTransactionData);
    }

    private Queue<TransactionData> _waitQueue = new Queue<TransactionData>();
    /// <summary>
    /// (쓰레드함수) 대기 큐에 걸어서 트렌젝션을 순차적으로 요청한다. (너무 많은 요청 시, 과부하 방지를 위해 딜레이 존재)
    /// </summary>
    private void RequestTransactionProcess()
    {
        while (true)
        {
            if (_waitQueue.Count > 0)
            {
                TRCount.WaitUse();
                var requestTransactionData = _waitQueue.Dequeue();

                // Set Value
                var enumerator = requestTransactionData.values.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    this.AxKHOpenAPI.SetInputValue(enumerator.Current.Key, enumerator.Current.Value);
                }

                // Excute Request
                var result = this.AxKHOpenAPI.CommRqData(requestTransactionData.optCode.ToString(), requestTransactionData.optCode.ToDescription(), 0, requestTransactionData.screenNumber);
                if (result != 0)
                    Logger.Log(result);
            }
            Thread.Sleep(100);
        }
    }
    private int tempcount = 0;
    private string _sTrCode, _sRQName;
    private int _currenRepeatCnt;
    /// <summary>
    /// Transaction 수신시 이벤트
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnReceiveTransactionData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
    {
        ++tempcount;

        // Caching
        _sTrCode = e.sTrCode;
        _sRQName = e.sRQName;

        // Update Data
        eOPTCode optCode = Utils.FindEnumValue<eOPTCode>(e.sRQName);
        int repeatCnt = this.AxKHOpenAPI.GetRepeatCnt(_sTrCode, _sRQName);
        TransactionData requestTransactionData = RequestTransactionDatas.Find(data => data.optCode.Equals(optCode) && data.screenNumber.Equals(e.sScrNo));
        if (requestTransactionData != null)
        {
            requestTransactionData.responseTime = ProgramConfig.NowTime;
            int errorCode = 0;
            int.TryParse(e.sErrorCode, out errorCode);
            if (Error.IsError(errorCode))
            {
                requestTransactionData.state = TransactionData.eState.Error;
                requestTransactionData.isReceiveResult = false;
            }
            else
            {

                for (_currenRepeatCnt = 0; _currenRepeatCnt == 0 || _currenRepeatCnt < repeatCnt; _currenRepeatCnt++)
                {
                    // 처음 받는 경우 (반복해서 받을 필요가 없는 정보에 사용할 때 이용)
                    bool isFirstResponse = _currenRepeatCnt == 0;

                    switch (optCode)
                    {
                        case eOPTCode.신규매수:
                            {

                            }
                            break;
                        case eOPTCode.신규매도:
                            {

                            }
                            break;
                        case eOPTCode.매수취소:
                            break;
                        case eOPTCode.매도취소:
                            break;
                        case eOPTCode.매수정정:
                            break;
                        case eOPTCode.매도정정:
                            break;
                        case eOPTCode.주식기본정보요청:
                            {
                                requestTransactionData.stockInfo.SetPriceData(
                                    GetTransactionData("현재가"),
                                    GetTransactionData("전일대비"),
                                    GetTransactionData("등락율"),
                                    GetTransactionData("거래량"),
                                    GetTransactionData("기준가"));
                                //Logger.Log($"{requestTransactionData.stockInfo.name}, 현재가 : {requestTransactionData.stockInfo.StockPrice}, 전일대비 : {requestTransactionData.stockInfo.UpDownPrice}, 등락율 : {requestTransactionData.stockInfo.UpDownRate}, 거래량 : {requestTransactionData.stockInfo.TradingVolume}");
                            }
                            break;
                        case eOPTCode.주식거래원요청:
                            break;
                        case eOPTCode.체결정보요청:
                            break;
                        case eOPTCode.주식호가요청:
                            {
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매도10호가, GetTransactionData("매도10차선호가"), GetTransactionData("매도10차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매도9호가, GetTransactionData("매도9차선호가"), GetTransactionData("매도9차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매도8호가, GetTransactionData("매도8차선호가"), GetTransactionData("매도8차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매도7호가, GetTransactionData("매도7차선호가"), GetTransactionData("매도7차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매도6호가, GetTransactionData("매도6차선호가"), GetTransactionData("매도6차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매도5호가, GetTransactionData("매도5차선호가"), GetTransactionData("매도5차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매도4호가, GetTransactionData("매도4차선호가"), GetTransactionData("매도4차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매도3호가, GetTransactionData("매도3차선호가"), GetTransactionData("매도3차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매도2호가, GetTransactionData("매도2차선호가"), GetTransactionData("매도2차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매도최우선호가, GetTransactionData("매도최우선호가"), GetTransactionData("매도최우선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매수최우선호가, GetTransactionData("매수최우선호가"), GetTransactionData("매수최우선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매수2호가, GetTransactionData("매수2차선호가"), GetTransactionData("매수2차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매수3호가, GetTransactionData("매수3차선호가"), GetTransactionData("매수3차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매수4호가, GetTransactionData("매수4차선호가"), GetTransactionData("매수4차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매수5호가, GetTransactionData("매수5차선호가"), GetTransactionData("매수5차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매수6호가, GetTransactionData("매수6우선호가"), GetTransactionData("매수6우선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매수7호가, GetTransactionData("매수7차선호가"), GetTransactionData("매수7차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매수8호가, GetTransactionData("매수8차선호가"), GetTransactionData("매수8차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매수9호가, GetTransactionData("매수9차선호가"), GetTransactionData("매수9차선잔량"));
                                requestTransactionData.stockInfo.SetOrderInfos(eOrderType.매수10호가, GetTransactionData("매수10차선호가"), GetTransactionData("매수10차선잔량"));
                            }
                            break;
                        case eOPTCode.주식일주월시분요청:
                            break;
                        case eOPTCode.주식시분요청:
                            break;
                        case eOPTCode.시세표성정보요청:
                            break;
                        case eOPTCode.주식외국인요청:
                            break;
                        case eOPTCode.주식기관요청:
                            break;
                        case eOPTCode.업종프로그램요청:
                            break;
                        case eOPTCode.신주인수권전체시세요청:
                            break;
                        case eOPTCode.주문체결요청:
                            break;
                        case eOPTCode.신용매매동향요청:
                            break;
                        case eOPTCode.공매도추이요청:
                            break;
                        case eOPTCode.일별거래상세요청:
                            break;
                        case eOPTCode.신고저가요청:
                            break;
                        case eOPTCode.상하한가요청:
                            break;
                        case eOPTCode.고가가근접요청:
                            break;
                        case eOPTCode.가격급락요청:
                            break;
                        case eOPTCode.호가잔량상위요청:
                            break;
                        case eOPTCode.호가잔량급증요청:
                            break;
                        case eOPTCode.잔량율급증요청:
                            break;
                        case eOPTCode.거래량급증요청:
                            break;
                        case eOPTCode.거래량갱신요청:
                            break;
                        case eOPTCode.매물대집중요청:
                            break;
                        case eOPTCode.고저PER요청:
                            break;
                        case eOPTCode.전일대비등락률상위요청:
                            break;
                        case eOPTCode.시가대비등락률상위요청:
                            break;
                        case eOPTCode.예상체결등락률상위요청:
                            break;
                        case eOPTCode.당일거래량상위요청:
                            break;
                        case eOPTCode.전일거래량상위요청:
                            break;
                        case eOPTCode.거래대금상위요청:
                            break;
                        case eOPTCode.신용비율상위요청:
                            break;
                        case eOPTCode.외인기간별매매상위요청:
                            break;
                        case eOPTCode.외인연속순매매상위요청:
                            break;
                        case eOPTCode.외인한동소진율증가상위:
                            break;
                        case eOPTCode.외국계창구매매상위요청:
                            break;
                        case eOPTCode.종목별증권사순위요청:
                            break;
                        case eOPTCode.증권사별매매상위요청:
                            break;
                        case eOPTCode.당일주요거래원요청:
                            break;
                        case eOPTCode.조기종료통화단위요청:
                            break;
                        case eOPTCode.순매수거래원순위요청:
                            break;
                        case eOPTCode.거래원매물대분석요청:
                            break;
                        case eOPTCode.일별기관매매종목요청:
                            break;
                        case eOPTCode.종목별기관매매추이요청:
                            break;
                        case eOPTCode.체결강도추이시간별요청:
                            break;
                        case eOPTCode.체결강도추이일별요청:
                            break;
                        case eOPTCode.ELW일별민감도지표요청:
                            break;
                        case eOPTCode.ELW투자지표요청:
                            break;
                        case eOPTCode.ELW민감도지표요청:
                            break;
                        case eOPTCode.업종별투자자순매수요청:
                            break;
                        case eOPTCode.거래원순간거래량요청:
                            break;
                        case eOPTCode.당일상위이탈원요청:
                            break;
                        case eOPTCode.변동성완화장치발동종목요청:
                            break;
                        case eOPTCode.당일전일체결대량요청:
                            break;
                        case eOPTCode.투자자별일별매매종목요청:
                            break;
                        case eOPTCode.종목별투자자기관별요청:
                            break;
                        case eOPTCode.종목별투자자기관별차트요청:
                            break;
                        case eOPTCode.종목별투자자기관별합계요청:
                            break;
                        case eOPTCode.동일순매매순위요청:
                            break;
                        case eOPTCode.장중투자자별매매요청:
                            break;
                        case eOPTCode.장중투자자별매매차트요청:
                            break;
                        case eOPTCode.장중투자자별매매상위요청:
                            break;
                        case eOPTCode.대차거래내역요청:
                            break;
                        case eOPTCode.대차거래추이요청:
                            break;
                        case eOPTCode.대차거래상위10종목요청:
                            break;
                        case eOPTCode.시간대별전일비거래비중요청:
                            break;
                        case eOPTCode.일자별종목별실현손익요청:
                            break;
                        case eOPTCode.일자별실현손익요청:
                            break;
                        case eOPTCode.미체결요청:
                            {
                                if (IsLogin)
                                {
                                    string requestAccountNumber;
                                    if (requestTransactionData.values.TryGetValue("계좌번호", out requestAccountNumber))
                                    {
                                        var accountInfo = LoginInfo.Accounts.Find(info => info.AccountNumber.Equals(requestAccountNumber));
                                        if (accountInfo != null)
                                        {
                                            if (!string.IsNullOrEmpty(GetTransactionData("주문번호")))
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

                                                orderNumber = GetTransactionData("주문번호");
                                                traingSymbol = GetTransactionData("종목코드");
                                                stockName = GetTransactionData("종목명");
                                                int.TryParse(GetTransactionData("주문수량"), out orderCount);
                                                long.TryParse(GetTransactionData("주문가격"), out orderPrice);
                                                int.TryParse(GetTransactionData("미체결수량").Replace("-", ""), out waitOrderCount);
                                                long.TryParse(GetTransactionData("현재가"), out currentPrice);
                                                orderType = GetTransactionData("주문구분");
                                                orderTime = GetTransactionData("시간");
                                                accountInfo.OrderStock(requestTransactionData.screenNumber, orderNumber, traingSymbol, orderCount, orderPrice, waitOrderCount, orderType, orderTime, false);
                                                BalanceStock balanceStock = accountInfo.GetMyBalanceStock(traingSymbol);
                                                if (balanceStock != null)
                                                    _requestBalanceStocks.Add(balanceStock);

                                                if (isFirstResponse)
                                                    accountInfo.AvailableMoney = accountInfo.DepositAfter2Day;

                                                if (orderType.Contains("매수"))
                                                    accountInfo.AvailableMoney -= orderPrice * orderCount;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case eOPTCode.체결요청:
                            break;
                        case eOPTCode.당일실현손익상세요청:
                            break;
                        case eOPTCode.증권사별종목매매동향요청:
                            break;
                        case eOPTCode.주식틱차트조회요청:
                            break;
                        case eOPTCode.주식분봉차트조회요청:
                            {
                                string day = GetTransactionData("체결시간").Substring(0, 8);
                                if (day.Equals(ProgramConfig.NowTime.ToString("yyyyMMdd"))
                                    || day.Equals(ProgramConfig.NowTime.AddDays(-1).ToString("yyyyMMdd")))
                                    requestTransactionData.stockInfo.SetChartData(eChartType.분봉, GetTransactionData("체결시간"), GetTransactionData("시가"), GetTransactionData("고가"), GetTransactionData("저가"), GetTransactionData("현재가"));
                            }
                            break;
                        case eOPTCode.주식일봉차트조회요청:
                            {
                                requestTransactionData.stockInfo.SetChartData(eChartType.일봉, GetTransactionData("일자"), GetTransactionData("시가"), GetTransactionData("고가"), GetTransactionData("저가"), GetTransactionData("현재가"));
                            }
                            break;
                        case eOPTCode.주식주봉차트조회요청:
                            break;
                        case eOPTCode.주식월봉차트조회요청:
                            break;
                        case eOPTCode.당일전일체결요청:
                            break;
                        case eOPTCode.계좌수익률요청:
                            {

                            }
                            break;
                        case eOPTCode.일별주가요청:
                            break;
                        case eOPTCode.시간외단일가요청:
                            break;
                        case eOPTCode.주식년봉차트조회요청:
                            break;
                        case eOPTCode.업종현재가요청:
                            {
                                if (isFirstResponse)
                                {
                                    bool isMarketIndex = false;
                                    eMarketIndexType marketIndexType = eMarketIndexType.none;

                                    if (requestTransactionData.screenNumber.Equals(SCREEN_NUMBER_KOSPI_INDEX))
                                    {
                                        isMarketIndex = true;
                                        marketIndexType = eMarketIndexType.KOSPI;
                                    }

                                    if (requestTransactionData.screenNumber.Equals(SCREEN_NUMBER_KOSDAQ_INDEX))
                                    {
                                        isMarketIndex = true;
                                        marketIndexType = eMarketIndexType.KOSDAQ;
                                    }

                                    if (requestTransactionData.screenNumber.Equals(SCREEN_NUMBER_KOSPI200_INDEX))
                                    {
                                        isMarketIndex = true;
                                        marketIndexType = eMarketIndexType.KOSPI200;
                                    }

                                    if (isMarketIndex)
                                        ModelCenter.MarketIndex.SetMarketIndex(marketIndexType, GetTransactionData("현재가"), GetTransactionData("전일대비"), GetTransactionData("등락률"));
                                }
                            }
                            break;
                        case eOPTCode.업종별주가요청:
                            break;
                        case eOPTCode.전업종지수요청:
                            break;
                        case eOPTCode.업종틱차트조회요청:
                            break;
                        case eOPTCode.업종분봉조회요청:
                            break;
                        case eOPTCode.업종일봉조회요청:
                            break;
                        case eOPTCode.업종주봉조회요청:
                            break;
                        case eOPTCode.업종월봉조회요청:
                            break;
                        case eOPTCode.업종현재가일별요청:
                            break;
                        case eOPTCode.업종년봉조회요청:
                            break;
                        case eOPTCode.대차거래추이요청_종목별:
                            break;
                        case eOPTCode.ELW가격급등락요청:
                            break;
                        case eOPTCode.거래원별ELW순매매상위요청:
                            break;
                        case eOPTCode.ELWLP보유일별추이요청:
                            break;
                        case eOPTCode.ELW괴리율요청:
                            break;
                        case eOPTCode.ELW조건검색요청:
                            break;
                        case eOPTCode.ELW등락율순위요청:
                            break;
                        case eOPTCode.ELW잔량순위요청:
                            break;
                        case eOPTCode.ELW근접율요청:
                            break;
                        case eOPTCode.ELW종목상세정보요청:
                            break;
                        case eOPTCode.ETF수익율요청:
                            break;
                        case eOPTCode.ETF종목정보요청:
                            break;
                        case eOPTCode.ETF일별추이요청:
                            break;
                        case eOPTCode.ETF전체시세요청:
                            break;
                        case eOPTCode.ETF시간대별추이요청:
                            break;
                        case eOPTCode.ETF시간대별체결요청:
                            break;
                        case eOPTCode.ETF일자별체결요청:
                            break;
                        case eOPTCode.선옵현재가정보요청:
                            break;
                        case eOPTCode.선옵일자별체결요청:
                            break;
                        case eOPTCode.선옵시고저가요청:
                            break;
                        case eOPTCode.콜옵션행사가요청:
                            break;
                        case eOPTCode.선옵시간별거래량요청:
                            break;
                        case eOPTCode.선옵체결추이요청:
                            break;
                        case eOPTCode.선물시세추이요청:
                            break;
                        case eOPTCode.프로그램매매추이차트요청:
                            break;
                        case eOPTCode.선옵시간별잔량요청:
                            break;
                        case eOPTCode.선옵호가잔량추이요청:
                            break;
                        case eOPTCode.선옵타임스프레드차트요청:
                            break;
                        case eOPTCode.선물가격대별비중차트요청:
                            break;
                        case eOPTCode.선물미결제약정일차트요청:
                            break;
                        case eOPTCode.베이시스추이차트요청:
                            break;
                        case eOPTCode.풋콜옵션비율차트요청:
                            break;
                        case eOPTCode.선물옵션현재가정보요청:
                            break;
                        case eOPTCode.복수종목결제원별시세요청:
                            break;
                        case eOPTCode.콜종목결제월별시세요청:
                            break;
                        case eOPTCode.풋종목결제월별시세요청:
                            break;
                        case eOPTCode.민감도지표추이요청:
                            break;
                        case eOPTCode.일별변동성분석그래프요청:
                            break;
                        case eOPTCode.시간별변동성분석그래프요청:
                            break;
                        case eOPTCode.선옵주문체결요청:
                            break;
                        case eOPTCode.선옵잔고요청:
                            break;
                        case eOPTCode.선물틱차트요청:
                            break;
                        case eOPTCode.선물옵션분차트요청:
                            break;
                        case eOPTCode.선물옵션일차트요청:
                            break;
                        case eOPTCode.선옵잔고손익요청:
                            break;
                        case eOPTCode.선옵당일실현손익요청:
                            break;
                        case eOPTCode.선옵잔존일조회요청:
                            break;
                        case eOPTCode.선옵전일가격요청:
                            break;
                        case eOPTCode.지수변동성차트요청:
                            break;
                        case eOPTCode.주요지수변동성차트요청:
                            break;
                        case eOPTCode.코스피200지수요청:
                            break;
                        case eOPTCode.투자자별만기손익차트요청:
                            break;
                        case eOPTCode.투자자별포지션종합요청:
                            break;
                        case eOPTCode.주식선물거래량상위종목요청:
                            break;
                        case eOPTCode.주식선물시세표요청:
                            break;
                        case eOPTCode.선물미결제약정분차트요청:
                            break;
                        case eOPTCode.옵션미결제약정일차트요청:
                            break;
                        case eOPTCode.옵션미결제약정분차트요청:
                            break;
                        case eOPTCode.풋옵션행사가요청:
                            break;
                        case eOPTCode.옵션틱차트요청:
                            break;
                        case eOPTCode.옵션분차트요청:
                            break;
                        case eOPTCode.옵션일차트요청:
                            break;
                        case eOPTCode.선물주차트요청:
                            break;
                        case eOPTCode.선물월차트요청:
                            break;
                        case eOPTCode.선물년차트요청:
                            break;
                        case eOPTCode.테마그룹별요청:
                            break;
                        case eOPTCode.테마구성종목요청:
                            break;
                        case eOPTCode.프로그램순매수상위50요청:
                            break;
                        case eOPTCode.종목별프로그램매매현황요청:
                            break;
                        case eOPTCode.프로그램매매추이요청:
                            break;
                        case eOPTCode.프로그램매매차익잔고추이요청:
                            break;
                        case eOPTCode.프로그램매매누적추이요청:
                            break;
                        case eOPTCode.종목시간별프로그램매매추이요청:
                            break;
                        case eOPTCode.외국인기관매매상위요청:
                            break;
                        case eOPTCode.차익잔고현황요청:
                            break;
                        case eOPTCode.종목일별프로그램매매추이요청:
                            break;
                        case eOPTCode.선물전체시세요청:
                            break;
                        case eOPTCode.관심종목정보요청:
                            break;
                        case eOPTCode.관심종목투자자정보요청:
                            break;
                        case eOPTCode.관심종목프로그램정보요청:
                            break;
                        case eOPTCode.예수금상세현황요청:
                            break;
                        case eOPTCode.일별추정예탁자산현황요청:
                            break;
                        case eOPTCode.추정자산조회요청:
                            break;
                        case eOPTCode.계좌평가현황요청:
                            {
                                if (IsLogin)
                                {
                                    string requestAccountNumber;
                                    if (requestTransactionData.values.TryGetValue("계좌번호", out requestAccountNumber))
                                    {
                                        var accountInfo = LoginInfo.Accounts.Find(info => info.AccountNumber.Equals(requestAccountNumber));
                                        if (accountInfo != null)
                                        {
                                            if (isFirstResponse)
                                            {
                                                accountInfo.Deposit = long.Parse(GetTransactionData("예수금"));
                                                accountInfo.DepositAfter2Day = long.Parse(GetTransactionData("D+2추정예수금"));
                                                accountInfo.AvailableMoney = accountInfo.DepositAfter2Day;
                                                accountInfo.EstimatedAssets = long.Parse(GetTransactionData("추정예탁자산"));

                                                // API로는 제공해주지 않는 데이터들 (따로 계산을 해야할 것 같다)
                                                //accountInfo.TodayProfitAmount = long.Parse(GetTransactionData("당일투자손익"));
                                                //accountInfo.TodayProfitRate = float.Parse(GetTransactionData("당일손익율"));
                                            }

                                            if (!string.IsNullOrEmpty(GetTransactionData("종목코드")))
                                            {
                                                string traingSymbol;
                                                int haveCnt;
                                                long buyingMoney;
                                                //int stockPrice;
                                                //long estimatedProfit;
                                                //float estimatedProfitRate;

                                                traingSymbol = GetTransactionData("종목코드");
                                                int.TryParse(GetTransactionData("보유수량"), out haveCnt);
                                                long.TryParse(GetTransactionData("매입금액"), out buyingMoney);
                                                //int.TryParse(GetTransactionData("현재가"), out stockPrice);
                                                //long.TryParse(GetTransactionData("손익금액"), out estimatedProfit);
                                                //float.TryParse(GetTransactionData("손익율"), out estimatedProfitRate);
                                                accountInfo.CreateBalanceStock(traingSymbol, haveCnt, buyingMoney);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case eOPTCode.체결잔고요청:
                            break;
                        case eOPTCode.관리자별주문체결내역요청:
                            break;
                        case eOPTCode.계좌별주문체결내역상세요청:
                            break;
                        case eOPTCode.계좌별익일결제예정내역요청:
                            break;
                        case eOPTCode.계좌별주문체결현황요청:
                            break;
                        case eOPTCode.주문인출가능금액요청:
                            break;
                        case eOPTCode.증거금율별주문가능수량조회요청:
                            break;
                        case eOPTCode.신용보증금율별주문가능수량조회:
                            break;
                        case eOPTCode.증거금세부내역조회요청:
                            break;
                        case eOPTCode.비밀번호일치여부요청:
                            break;
                        case eOPTCode.위탁종합거래내역요청:
                            break;
                        case eOPTCode.일별계좌수익률상세현황요청:
                            break;
                        case eOPTCode.계좌별당일현황요청:
                            break;
                        case eOPTCode.계좌평가잔고내역요청:
                            {
                                if (IsLogin)
                                {
                                    string requestAccountNumber;
                                    if (requestTransactionData.values.TryGetValue("계좌번호", out requestAccountNumber))
                                    {
                                        var accountInfo = LoginInfo.Accounts.Find(info => info.AccountNumber.Equals(requestAccountNumber));
                                        if (accountInfo != null)
                                        {
                                            long totalBuyAmount = 0;
                                            long.TryParse(GetTransactionData("총매입금액"), out totalBuyAmount);
                                            accountInfo.SetTotalBuyAmount(totalBuyAmount);

                                            long totalEvaluationAmount = 0;
                                            long.TryParse(GetTransactionData("총평가금액"), out totalEvaluationAmount);
                                            accountInfo.SetTotalEvaluationAmount(totalEvaluationAmount);
                                        }
                                    }
                                }
                            }
                            break;
                        case eOPTCode.선물옵션청산주문위탁증거금가계산요청:
                            break;
                        case eOPTCode.선옵당일매매변동현황요청:
                            break;
                        case eOPTCode.선옵기간손익조회요청:
                            break;
                        case eOPTCode.선옵주문체결내역상세요청:
                            break;
                        case eOPTCode.선옵주문체결내역상세평균가요청:
                            break;
                        case eOPTCode.선옵잔고상세현황요청:
                            break;
                        case eOPTCode.선옵잔고현황정산가기준요청:
                            break;
                        case eOPTCode.계좌별결제예상내역조회요청:
                            break;
                        case eOPTCode.선옵계좌별주문가능수량요청:
                            break;
                        case eOPTCode.선옵예탁금및증거금조회요청:
                            break;
                        case eOPTCode.선옵계좌예비증거금상세요청:
                            break;
                        case eOPTCode.선옵증거금상세내역요청:
                            break;
                        case eOPTCode.계좌미결제청산가능수량조회요청:
                            break;
                        case eOPTCode.선옵실시간증거금산출요청:
                            break;
                        case eOPTCode.옵션매도주문증거금현황요청:
                            break;
                        case eOPTCode.신용융자_가능종목요청:
                            break;
                        case eOPTCode.신용융자_가능문의:
                            break;
                        default:
                            break;
                    }
                }
                requestTransactionData.state = TransactionData.eState.Response;
                requestTransactionData.isReceiveResult = true;
            }
            this.RequestTransactionDatas.Remove(requestTransactionData);
            this.ResponseTransactionDatas.Add(requestTransactionData);
            ClaerOldResponseTransactionData();
        }
    }

    /// <summary>
    /// 요청 받은지 오래된 데이터 삭제
    /// </summary>
    private void ClaerOldResponseTransactionData()
    {
        // 5분 지난 데이터는 삭제한다.
        DateTime checkTime = ProgramConfig.NowTime.AddMinutes(5);
        for (int i = ResponseTransactionDatas.Count - 1; i >= 0; i--)
        {
            var responseData = ResponseTransactionDatas[i];
            if (responseData.responseTime > checkTime)
                ResponseTransactionDatas.Remove(responseData);
        }
    }

    /// <summary>
    /// OnReceiveTransactionData 데이터 가져오기
    /// </summary>
    /// <param name="currenRepeatCnt"></param>
    /// <param name="strItemName"></param>
    /// <returns></returns>
    private string GetTransactionData(string strItemName)
    {
        return this.AxKHOpenAPI.GetCommData(_sTrCode, _sRQName, _currenRepeatCnt, strItemName).Trim();
    }
}
