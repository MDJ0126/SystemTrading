using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
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

            // 프로그램 재실행 타이머
            StartCoroutine("ProgramRestartTimer");

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

        private IEnumerator ProgramRestartTimer()
        {
            //[시스템 점검]
            //원할한 거래를 위한 시스템 점검 작업은 하루 한번 수행하며 이 시간에는 모든 접속이 일시적으로 차단되고
            //접속한 고객은 강제 로그오프됩니다.
            //점검시간은 월요일 ~토요일은 04시 55분 ~05시까지이며 일요일은 04시부터 04시 30분까지 접속단절과 접속이 제한됩니다.
            //그리고 오전 6시 50분 이전에 접속하시면 거래종목 정보, 전일 거래에 대한 결제분 등이 반영되지 않아 실제 잔고와 차이가 발생할 수 있습니다.

            // 시스템 점검 시간
            DateTime systemCheckStartTime = KiwoomManager.Instance.SystemCheckStartTime;
            DateTime systemCheckEndTime = KiwoomManager.Instance.SystemCheckEndTime;

            // 장 시작 하기 전에 한 번 재실행
            DateTime restartMorningTime = KiwoomManager.Instance.RestartMorningTime;

            while (true)
            {
                // 점검 때 재실행
                if (systemCheckStartTime <= ProgramConfig.NowTime && ProgramConfig.NowTime <= systemCheckEndTime)
                {
                    if (KiwoomManager.Instance.IsStaredAPI)
                        KiwoomManager.Instance.StopAPI();

                    double waitingTotalSeconds = new TimeSpan(systemCheckEndTime.Ticks - ProgramConfig.NowTime.Ticks).TotalSeconds;
                    yield return new WaitForSeconds((float)waitingTotalSeconds);
                    ProgramManager.Restart();
                }

                // 오전에 시작 전에 한 번 재실행
                if (restartMorningTime <= ProgramConfig.NowTime && ProgramConfig.NowTime <= restartMorningTime.AddMinutes(1f))
                {
                    if (KiwoomManager.Instance.IsStaredAPI)
                        KiwoomManager.Instance.StopAPI();

                    double waitingTotalSeconds = new TimeSpan(restartMorningTime.AddMinutes(1f).Ticks - ProgramConfig.NowTime.Ticks).TotalSeconds;
                    yield return new WaitForSeconds((float)waitingTotalSeconds);
                    ProgramManager.Restart();
                }

                yield return new WaitForSeconds(5f);
            }
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
