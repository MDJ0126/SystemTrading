using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemTrading.Scripts.Manager
{
    public partial class MarketIndex : IModel
    {
        public event Action onChangeData;

        public class Index
        {
            public eMarketIndexType marketIndex;
            public string total;
            public string todayUpDown;
            public string todayUpDownRate;

            public Index(eMarketIndexType marketIndex)
            {
                this.marketIndex = marketIndex;
            }

            public void Set(string total, string todayUpDown, string todayUpDownRate)
            {
                this.total = total;
                this.todayUpDown = todayUpDown;
                this.todayUpDownRate = todayUpDownRate;
            }

            /// <summary>
            /// 총 지수 텍스트 가져오기
            /// </summary>
            /// <returns></returns>
            public string ToStringGetTotal()
            {
                return string.IsNullOrEmpty(total) ? "0" : total.Replace("+", "▲").Replace("-", "▼");
            }

            /// <summary>
            /// 금일 등락 텍스트 가져오기
            /// </summary>
            /// <returns></returns>
            public string ToStringGetToday()
            {
                return string.IsNullOrEmpty(todayUpDown) ? "0" : $"{todayUpDown.Replace("+", "▲").Replace("-", "▼")}({todayUpDownRate}%)";
            }
        }

        public Index KOSPI { get; private set; } = new Index(eMarketIndexType.KOSPI);
        public Index KOSPI200 { get; private set; } = new Index(eMarketIndexType.KOSPI200);
        public Index KOSDAQ { get; private set; } = new Index(eMarketIndexType.KOSDAQ);

        /// <summary>
        /// 지수 세팅
        /// </summary>
        /// <param name="marketIndex">마켓 구분</param>
        /// <param name="total">총 지수</param>
        /// <param name="todayUpDown">금일 등락지수</param>
        /// <param name="todayUpDownRate">금일 등락률</param>
        public void SetMarketIndex(eMarketIndexType marketIndex, string total, string todayUpDown, string todayUpDownRate)
        {
            switch (marketIndex)
            {
                case eMarketIndexType.KOSPI:
                    KOSPI.Set(total, todayUpDown, todayUpDownRate);
                    break;
                case eMarketIndexType.KOSPI200:
                    KOSPI200.Set(total, todayUpDown, todayUpDownRate);
                    break;
                case eMarketIndexType.KOSDAQ:
                    KOSDAQ.Set(total, todayUpDown, todayUpDownRate);
                    break;
            }
            onChangeData?.Invoke();
        }

        public void Initialize()
        {
            KiwoomManager.Instance.SetTrValue("시장구분", "0");
            KiwoomManager.Instance.SetTrValue("업종코드", "001");
            KiwoomManager.Instance.RequestTransaction(eOPTCode.업종현재가요청, KiwoomManager.SCREEN_NUMBER_KOSPI_INDEX);
            KiwoomManager.Instance.RegisterRealData(KiwoomManager.SCREEN_NUMBER_KOSPI_INDEX, "001", eFID.현재가, eFID.전일대비, eFID.등락율);

            KiwoomManager.Instance.SetTrValue("시장구분", "1");
            KiwoomManager.Instance.SetTrValue("업종코드", "101");
            KiwoomManager.Instance.RequestTransaction(eOPTCode.업종현재가요청, KiwoomManager.SCREEN_NUMBER_KOSDAQ_INDEX);
            KiwoomManager.Instance.RegisterRealData(KiwoomManager.SCREEN_NUMBER_KOSDAQ_INDEX, "101", eFID.현재가, eFID.전일대비, eFID.등락율);

            KiwoomManager.Instance.SetTrValue("시장구분", "2");
            KiwoomManager.Instance.SetTrValue("업종코드", "201");
            KiwoomManager.Instance.RequestTransaction(eOPTCode.업종현재가요청, KiwoomManager.SCREEN_NUMBER_KOSPI200_INDEX);
            KiwoomManager.Instance.RegisterRealData(KiwoomManager.SCREEN_NUMBER_KOSPI200_INDEX, "201", eFID.현재가, eFID.전일대비, eFID.등락율);
        }

        public void Release()
        {

        }
    }
}
