using AxKHOpenAPILib;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 실시간 관련 정리
/// </summary>
public partial class KiwoomManager
{
    /// <summary>
    /// 리얼 데이터 연결 리스트
    /// </summary>
    public List<ConnectingRealData> ConnectingRealDatas { get; private set; } = null;

    /// <summary>
    /// 실시간 데이터 등록
    /// </summary>
    public int RegisterRealData(string tradingSymbol, params eFID[] fids)
    {
        ConnectingRealData connectingRealData = new ConnectingRealData(GetScrNum(), tradingSymbol, fids);
        ConnectingRealDatas.Add(connectingRealData);
        return this.AxKHOpenAPI?.SetRealReg(connectingRealData.screenNumber, connectingRealData.tradingSymbol, connectingRealData.GetFidString(), "1") ?? -1;
    }

    /// <summary>
    /// 실시간 데이터 등록
    /// </summary>
    public int RegisterRealData(string screenNumber, string tradingSymbol, params eFID[] fids)
    {
        ConnectingRealData connectingRealData = new ConnectingRealData(screenNumber, tradingSymbol, fids);
        ConnectingRealDatas.Add(connectingRealData);
        return this.AxKHOpenAPI?.SetRealReg(connectingRealData.screenNumber, connectingRealData.tradingSymbol, connectingRealData.GetFidString(), "1") ?? -1;
    }

    /// <summary>
    /// 실시간 시세 이벤트
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnReceiveRealData(object sender, _DKHOpenAPIEvents_OnReceiveRealDataEvent e)
    {
        ConnectingRealData connectingRealData = KiwoomManager.Instance.ConnectingRealDatas.Find(data => data.tradingSymbol.Equals(e.sRealKey));
        switch (e.sRealType)
        {
            case "주식시세":
                break;
            case "주식체결":
                {
                    if (connectingRealData.stockInfo != null)
                    {
                        connectingRealData.stockInfo.SetPriceData(
                            GetRealData(eFID.현재가),
                            GetRealData(eFID.전일대비),
                            GetRealData(eFID.등락율),
                            GetRealData(eFID.누적거래량));
                        connectingRealData.stockInfo.SetTodayTradingRate(GetRealData(eFID.거래회전율));
                        connectingRealData.stockInfo.SetOrderInfos(eOrderType.매도최우선호가, GetRealData(eFID.최우선_매도호가));
                        connectingRealData.stockInfo.SetOrderInfos(eOrderType.매수최우선호가, GetRealData(eFID.최우선_매수호가));
                        Logger.Log($"{connectingRealData.stockInfo.Name}, 현재가 : {connectingRealData.stockInfo.StockPrice}, 전일대비 : {connectingRealData.stockInfo.UpDownPrice}, 등락율 : {connectingRealData.stockInfo.UpDownRate}, 거래량 : {connectingRealData.stockInfo.tradingVolume}");
                    }
                }
                break;
            case "주식우선호가":
                break;
            case "주식호가잔량":
                break;
            case "주식시간외호가":
                break;
            case "주식당일거래원":
                break;
            case "ETF NAV":
                break;
            case "ELW 지표":
                break;
            case "ELW 이론가":
                break;
            case "주식예상체결":
                break;
            case "주식종목정보":
                break;
            case "선물옵션우선호가":
                break;
            case "선물시세":
                break;
            case "선물호가잔량":
                break;
            case "선물이론가":
                break;
            case "옵션시세":
                break;
            case "옵션호가잔량":
                break;
            case "옵션이론가":
                break;
            case "업종지수":
                {
                    if (connectingRealData != null)
                    {
                        bool isMarketIndex = false;
                        eMarketIndexType marketIndexType = eMarketIndexType.none;

                        if (connectingRealData.screenNumber.Equals(SCREEN_NUMBER_KOSPI_INDEX))
                        {
                            isMarketIndex = true;
                            marketIndexType = eMarketIndexType.KOSPI;
                        }

                        if (connectingRealData.screenNumber.Equals(SCREEN_NUMBER_KOSDAQ_INDEX))
                        {
                            isMarketIndex = true;
                            marketIndexType = eMarketIndexType.KOSDAQ;
                        }

                        if (connectingRealData.screenNumber.Equals(SCREEN_NUMBER_KOSPI200_INDEX))
                        {
                            isMarketIndex = true;
                            marketIndexType = eMarketIndexType.KOSPI200;
                        }

                        if (isMarketIndex)
                            ModelCenter.MarketIndex.SetMarketIndex(marketIndexType, GetRealData(eFID.현재가), GetRealData(eFID.전일대비), GetRealData(eFID.등락율));
                    }
                }
                break;
            case "업종등락":
                break;
            case "장시작시간":
                {
                    // 장시작시간은 '개장 시작 20분 전부터 ~ 개장 시작 시간', '장 종료 10분 전부터 ~ 종료'에 발생한다.
                    // 장 종료 1분 전에는 10초마다 이벤트가 발생된다.
                    string nowTimeStr = GetRealData(eFID.체결시간);
                    int hour = 0;
                    int.TryParse(nowTimeStr.Substring(0, 2), out hour);
                    int minute = 0;
                    int.TryParse(nowTimeStr.Substring(2, 2), out minute);
                    int second = 0;
                    int.TryParse(nowTimeStr.Substring(4, 2), out second);
                    if (hour >= 0 && hour <= 24
                        && minute >= 0 && minute <= 60
                        && second >= 0 && second <= 60)
                    {
                        DateTime nowTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, hour, minute, second);
                        ProgramConfig.SetServerTime(nowTime);
                    }
                    else
                        LineNotify.SendMessage($"알 수 없는 장시작시간을 받았습니다. => eFID.체결시간: '{nowTimeStr}'");

                    // 현재 장이 운영 중인지 판단 세팅 (평소와 다른날도 있으므로 중요)
                    string marketTypeStr = GetRealData(eFID.장운영구분);
                    int tradingTimeStateInt = -1;
                    int.TryParse(marketTypeStr, out tradingTimeStateInt);
                    eTradingTimeState tradingTimeState = (eTradingTimeState)(tradingTimeStateInt);
                    ProgramConfig.SetIsTradingTime(tradingTimeState);

                    // 장 시작 또는 종료까지 남은 시간 세팅
                    string marketRemainTimeStr = GetRealData(eFID.장시작예상잔여시간);
                    if (hour >= 0 && hour <= 24
                        && minute >= 0 && minute <= 60
                        && second >= 0 && second <= 60)
                    {
                        int remainHour = 0;
                        int.TryParse(marketRemainTimeStr.Substring(0, 2), out remainHour);
                        remainHour *= 360;
                        int remainMinute = 0;
                        int.TryParse(marketRemainTimeStr.Substring(2, 2), out remainMinute);
                        remainMinute *= 60;
                        int remainSecond = 0;
                        int.TryParse(marketRemainTimeStr.Substring(4, 2), out remainSecond);
                        ProgramConfig.SetMarketRemainSecond(remainHour + remainMinute + remainSecond);
                    }
                    else
                        LineNotify.SendMessage($"알 수 없는 장시작시간을 받았습니다. => eFID.장시작예상잔여시간: '{marketRemainTimeStr}'");

                    //Logger.Log($"{eFID.장운영구분}: {tradingTimeState}, {eFID.체결시간}: {nowTimeStr}, {eFID.장시작예상잔여시간}: {marketRemainTimeStr}");
                }
                break;
            case "VI발동/해제":
                break;
            case "주문체결":
            case "파생잔고":
            case "잔고":
                {
                    // OnReceiveChejanData()에서만 처리된다고 함
                }
                break;
            case "종목프로그램매매":
                break;
            case "예상업종지수":
                {
                    //if (connectingRealData != null)
                    //{
                    //    bool isMarketIndex = false;
                    //    eMarketIndexType marketIndexType = eMarketIndexType.none;

                    //    if (connectingRealData.screenNumber.Equals(SCREEN_NUMBER_KOSPI_INDEX))
                    //    {
                    //        isMarketIndex = true;
                    //        marketIndexType = eMarketIndexType.KOSPI;
                    //    }

                    //    if (connectingRealData.screenNumber.Equals(SCREEN_NUMBER_KOSDAQ_INDEX))
                    //    {
                    //        isMarketIndex = true;
                    //        marketIndexType = eMarketIndexType.KOSDAQ;
                    //    }

                    //    if (connectingRealData.screenNumber.Equals(SCREEN_NUMBER_KOSPI200_INDEX))
                    //    {
                    //        isMarketIndex = true;
                    //        marketIndexType = eMarketIndexType.KOSPI200;
                    //    }

                    //    if (isMarketIndex)
                    //        ModelCenter.MarketIndex.SetMarketIndex(marketIndexType, GetRealData(eFID.현재가), GetRealData(eFID.전일대비), GetRealData(eFID.등락율));
                    //}
                }
                break;
            default:
                Logger.Log("실시간 데이터 이벤트[" + e.sRealType + "]!! 없는 케이스입니다. 확인 바랍니다.");
                break;
        }
        HandlerKiwoomAPI.Instance.NotifyReceiveRealData();
    }

    /// <summary>
    /// OnReceiveRealData에서 FID 값 가져오기
    /// </summary>
    /// <param name="tradingSymbol"></param>
    /// <param name="fid"></param>
    private string GetRealData(eFID fid, string tradingSymbol = "")
    {
        return KiwoomManager.Instance.AxKHOpenAPI.GetCommRealData(tradingSymbol, (int)fid);
    }
}
