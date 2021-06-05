using System;
using System.Collections.Generic;

/// <summary>
/// 종목 정보 클래스
/// </summary>
[Serializable]
public class StockInfo
{
    private class UpDownRateRecord
    {
        private DateTime _todayFirstAddTime = DateTime.MinValue;

        private float _oneMinuteAverage = 0f;
        /// <summary>
        /// 최근 1분 평균
        /// </summary>
        public float? OneMinuteAverage
        { 
            get
            {
                //TrimRecord();
                // 프로그램이 시작된지 1분이상이 되었을 때부터 측정
                if (ProgramConfig.StartUpTimeSeconds > 60f && _todayFirstAddTime.AddMinutes(1) < ProgramConfig.NowTime)
                    return _oneMinuteAverage;
                return null;
            }
        }

        private struct RateRecord
        {
            public DateTime inputTime;
            public float rate;

            public RateRecord(DateTime inputTime, float rate)
            {
                this.inputTime = inputTime;
                this.rate = rate;
            }
        }
        
        // 기록 저장큐
        private Queue<RateRecord> _rateRecordQueue = new Queue<RateRecord>();

        /// <summary>
        /// 기록 추가
        /// </summary>
        /// <param name="currentRate"></param>
        public void AddRecord(float currentRate)
        {
            if (_todayFirstAddTime.Date != ProgramConfig.NowTime.Date)
                _todayFirstAddTime = ProgramConfig.NowTime;
            _rateRecordQueue.Enqueue(new RateRecord(ProgramConfig.NowTime, currentRate));
            TrimRecord();
            AverageRate();
        }

        /// <summary>
        /// 평균
        /// </summary>
        /// <returns></returns>
        private void AverageRate()
        {
            if (_rateRecordQueue.Count > 0)
            {
                float totalRate2MinuteAgo = 0f, totalRate1MinuteAgo = 0;
                int count2MinuteAgo = 0, count1MinuteAgo = 0;

                var enumerator = _rateRecordQueue.GetEnumerator();
                var nowTime = ProgramConfig.NowTime;
                while (enumerator.MoveNext())
                {
                    RateRecord rateRecord = enumerator.Current;
                    if (rateRecord.inputTime < nowTime.AddMinutes(-1f))
                    {
                        // 2분 전 기록들
                        totalRate2MinuteAgo += rateRecord.rate;
                        ++count2MinuteAgo;
                    }
                    else
                    {
                        // 1분 전 기록들
                        totalRate1MinuteAgo += rateRecord.rate;
                        ++count1MinuteAgo;
                    }
                }
                if (count1MinuteAgo > 0 && count2MinuteAgo > 0)
                {
                    _oneMinuteAverage = (totalRate1MinuteAgo / count1MinuteAgo) - (totalRate2MinuteAgo / count2MinuteAgo);
                }
            }
        }

        /// <summary>
        /// 2분 이상 지난 기록은 제거
        /// </summary>
        private void TrimRecord()
        {
            var minuteAgo = ProgramConfig.NowTime.AddMinutes(-2f);    // 2분 전 변수 캐싱
            while (_rateRecordQueue.Count > 0)
            {
                RateRecord peek = _rateRecordQueue.Peek();
                if (minuteAgo > peek.inputTime)
                    _rateRecordQueue.Dequeue();
                else
                    break;
            }
        }
    }

    /// <summary>
    /// 호가 정보 클래스
    /// </summary>
    [Serializable]
    public class OrderInfo
    {
        public eOrderType callType; // Unique Key
        public long price;
        public long count;

        public OrderInfo(eOrderType callType, long price, long count)
        {
            this.callType = callType;
            Set(price, count);
        }

        public void Set(long price, long count)
        {
            this.price = price;
            this.count = count;
        }
    }

    /// <summary>
    /// 차트 정보
    /// </summary>
    [Serializable]
    public class ChartInfo
    {
        public string dateTimeStr; // Unique Key
        public long startPrice;
        public long maxPrice;
        public long minPrice;
        public long endPrice;

        public ChartInfo(string dateTimeStr, long startPrice, long maxPrice, long minPrice, long endPrice)
        {
            this.dateTimeStr = dateTimeStr;
            Set(startPrice, maxPrice, minPrice, endPrice);
        }

        public void Set(long startPrice, long maxPrice, long minPrice, long endPrice)
        {
            this.startPrice = startPrice;
            this.maxPrice = maxPrice;
            this.minPrice = minPrice;
            this.endPrice = endPrice;
        }
    }

    // 종목 코드
    public string tradingSymbol = string.Empty;
    // 종목 이름
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 기준가
    /// </summary>
    public int pivotPrice = 0;

    /// <summary>
    /// 현재가
    /// </summary>
    public int StockPrice { get; set; } = 0;

    /// <summary>
    /// 전일대비
    /// </summary>
    public long UpDownPrice { get; private set; } = 0;

    // 분당 평균 등락률
    [NonSerialized] private UpDownRateRecord _rateRecord = new UpDownRateRecord();

    // 현재 등락률
    private float _currRate = 0f;

    /// <summary>
    /// 등락률
    /// </summary>
    public float UpDownRate
    {
        get { return _currRate; }
        set
        {
            _currRate = value;
            _rateRecord?.AddRecord(_currRate);
        }
    }

    /// <summary>
    /// 거래량
    /// </summary>
    public long tradingVolume = 0;

    /// <summary>
    /// 금일 거래 회전율
    /// </summary>
    public float TodayTradingRate { get; private set; } = 0f;

    /// <summary>
    /// 종목에 대한 평가 점수
    /// </summary>
    public double Score
    {
        get
        {
            // 현재 등락률 * 금일 거래 회전율
            return UpDownRate * TodayTradingRate;
        }
    }

    /// <summary>
    /// 분당 성장률 평균
    /// </summary>
    public float GrowthRatePerMinute
    {
        get
        {
            var oneMinuteAverage = _rateRecord?.OneMinuteAverage;
            if (oneMinuteAverage != null)
                return oneMinuteAverage.Value;
            return 0f;
        }
    }

    public Dictionary<eOrderType, OrderInfo> OrderInfos { get; private set; } = new Dictionary<eOrderType, OrderInfo>();

    /// <summary>
    /// 정보 갱신 시간
    /// </summary>
    public DateTime RefreshTime { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// 통신되어 갱신 중인 데이터인지
    /// </summary>
    public bool IsConnectingData { get; private set; } = false;

    /// <summary>
    /// 차트 정보
    /// </summary>
    public Dictionary<eChartType, List<ChartInfo>> ChartInfos { get; private set; } = new Dictionary<eChartType, List<ChartInfo>>();

    public StockInfo()
    {
        this.RefreshTime = ProgramConfig.NowTime;
    }

    public StockInfo(string tradingSymbol, string name)
    {
        this.RefreshTime = ProgramConfig.NowTime;
        this.tradingSymbol = tradingSymbol;
        this.Name = name;
    }

    /// <summary>
    /// 기본 정보 갱신
    /// </summary>
    /// <param name="stockPriceStr">현재가</param>
    /// <param name="upDownPriceStr">전일대비</param>
    /// <param name="upDownPriceRateStr">등락률</param>
    /// <param name="tradingVolumeStr">거래량</param>
    public void SetPriceData(string stockPriceStr, string upDownPriceStr, string upDownPriceRateStr, string tradingVolumeStr, string pivotPrice = "")
    {
        this.RefreshTime = ProgramConfig.NowTime;
        this.StockPrice = Math.Abs(int.Parse(stockPriceStr));
        this.UpDownPrice = long.Parse(upDownPriceStr);
        this.UpDownRate = float.Parse(upDownPriceRateStr);
        this.tradingVolume = long.Parse(tradingVolumeStr);
        if (!string.IsNullOrEmpty(pivotPrice))
            this.pivotPrice = int.Parse(pivotPrice);
        _onChangedData?.Invoke();
    }

    /// <summary>
    /// 거래 회전율 갱신
    /// </summary>
    public void SetTodayTradingRate(string todayTradingRate)
    {
        this.TodayTradingRate = float.Parse(todayTradingRate);
    }

    /// <summary>
    /// 호가 세팅
    /// </summary>
    /// <param name="type">호가위치</param>
    /// <param name="price">가격</param>
    /// <param name="count">잔량</param>
    public void SetOrderInfos(eOrderType type, string priceStr, string countStr = "")
    {
        long price = 0, count = 0;
        if (!string.IsNullOrEmpty(priceStr))
            price = Int64.Parse(priceStr);
        if (!string.IsNullOrEmpty(countStr))
            count = Int64.Parse(countStr);

        OrderInfo orderInfo;
        if (OrderInfos.TryGetValue(type, out orderInfo))
        {
            orderInfo.Set(price, count);
        }
        else
        {
            OrderInfos.Add(type, new OrderInfo(type, price, count));
        }
    }

    /// <summary>
    /// 호가 가격 가져오기
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public long GetOrderPrice(eOrderType type)
    {
        long price = 0;
        OrderInfo orderInfo;
        if (OrderInfos.TryGetValue(type, out orderInfo))
        {
            price = Math.Abs(orderInfo.price);
        }
        return price;
    }

    /// <summary>
    /// 차트 데이터 입력
    /// </summary>
    /// <param name="type"></param>
    /// <param name="date"></param>
    /// <param name="price"></param>
    public void SetChartData(eChartType type, string dateTimeStr, string startPriceStr, string maxPriceStr, string minPriceStr, string endPriceStr)
    {
        long startPrice = 0;
        long maxPrice = 0;
        long minPrice = 0;
        long endPrice = 0;

        if (!string.IsNullOrEmpty(startPriceStr))
            startPrice = Int64.Parse(startPriceStr);

        if (!string.IsNullOrEmpty(maxPriceStr))
            maxPrice = Int64.Parse(maxPriceStr);

        if (!string.IsNullOrEmpty(minPriceStr))
            minPrice = Int64.Parse(minPriceStr);

        if (!string.IsNullOrEmpty(endPriceStr))
            endPrice = Int64.Parse(endPriceStr);

        //if (ProgramConfig.CheckTradingState == eTradingTimeState.장_종료)
        //{
        //    if (type == eChartType.분봉)
        //    {
        //        float rate = (float)(endPrice - this.PivotPrice) / this.PivotPrice;
        //        if (_averageRate != 0f)
        //        {
        //            _averageRate += rate;
        //            _averageRate *= 0.5f;
        //        }
        //        else
        //            _averageRate += rate;
        //    }
        //}

        List<ChartInfo> chartInfoList = null;
        if (!ChartInfos.TryGetValue(type, out chartInfoList))
        {
            chartInfoList = new List<ChartInfo>();
            ChartInfos.Add(type, chartInfoList);
        }

        if (chartInfoList != null)
        {
            var info = chartInfoList.Find(item => item.dateTimeStr.Equals(dateTimeStr));
            if (info != null)
            {
                info.Set(startPrice, maxPrice, minPrice, endPrice);
            }
            else
            {
                chartInfoList.Insert(0, new ChartInfo(dateTimeStr, startPrice, maxPrice, minPrice, endPrice));
            }
        }
    }

    public void ConnectDynamicData()
    {
        this.IsConnectingData = true;
    }

    public void LoadDataApply(StockInfo stockInfo)
    {
        if (stockInfo != null)
        {
            if (stockInfo.tradingSymbol.Equals(tradingSymbol))
            {
                this.StockPrice = stockInfo.StockPrice;
                this.UpDownPrice = stockInfo.UpDownPrice;
                this.UpDownRate = stockInfo.UpDownRate;
                this.tradingVolume = stockInfo.tradingVolume;
                this.TodayTradingRate = stockInfo.TodayTradingRate;
                this._currRate = stockInfo._currRate;
                this.RefreshTime = stockInfo.RefreshTime;
            }
        }
    }

    /// <summary>
    /// 실시간 정보 갱신 종료
    /// </summary>
    public void DisconnedDynamicData()
    {
        this.IsConnectingData = false;
    }

    #region ## Event ##
    public delegate void OnChangeData();
    private event OnChangeData _onChangedData;
    public event OnChangeData OnChangedData
    {
        add
        {
            _onChangedData -= value;
            _onChangedData += value;
        }
        remove
        {
            _onChangedData -= value;
        }
    }
    #endregion
}
