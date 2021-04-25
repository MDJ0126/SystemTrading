using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SystemTrading.Forms
{
    public partial class MainForm : SceneForm
    {
        public MainForm()
        {
            InitializeComponent();
            
            // 초기화
            Initialize();

            // 타이틀 시간 업데이터
            StartCoroutine("UpdateTimer");

            // 주식 갱신 Notify 업데이터
            StartCoroutine("SaveStockList");

            // 프로그램 오더 폼 열기
            FormManager.OpenForm(eForm.ProgramOrder, panel);
        }

        private void Initialize()
        {
            logText.Text = string.Empty;
            Logger.Instance.OnAddLog += OnAddLog;
        }

        private string _log = string.Empty;
        private void OnAddLog(string log)
        {
            _log = log;
        }

        private void SceneUpdate()
        {
            logText.Text = _log;
            trCount.Text = $"남은 요청 횟수 : {KiwoomManager.Instance.TRCount.ToStringCurrentTRCount()}";
        }

        private IEnumerator SaveStockList()
        {
            while (true)
            {
                // 5분마다 모든 종목 저장
                yield return new WaitForSeconds(300f);
                if (KiwoomManager.Instance.IsLogin)
                {
                    //if (ProgramConfig.CheckTradingState == eTradingTimeState.장_중)
                    StockListManager.Instance.SaveStockList();
                }
            }
        }

        private IEnumerator UpdateTimer()
        {
            string title = this.Text;
            while (true)
            {
                string currentState = string.Empty;
                switch (ProgramConfig.CheckTradingState)
                {
                    case eTradingTimeState.장_미운영시간:
                        {
                            currentState = "장 미운영시간";
                        }
                        break;
                    case eTradingTimeState.장시작전:
                        {
                            currentState = "장 시작 전";
                        }
                        break;
                    case eTradingTimeState.장_종료_10분전_동시호가:
                        {
                            var remainMinute = ProgramConfig.MarketRemainSecond / 60;
                            var remainSecond = ProgramConfig.MarketRemainSecond % 60;
                            currentState = $"장 종료까지 {remainMinute}분 {remainSecond}초 남았습니다.";
                        }
                        break;
                    case eTradingTimeState.장_중:
                        {
                            currentState = "장 운영 중";
                        }
                        break;
                    case eTradingTimeState.장_종료:
                        {
                            currentState = "장 종료";
                        }
                        break;
                    default:
                        break;
                }
                this.Text = $"{title} [{ProgramConfig.NowTime} ({currentState})]";
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
