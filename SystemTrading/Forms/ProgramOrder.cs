using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace SystemTrading.Forms
{
    public partial class ProgramOrder : SceneForm
    {
        private bool _isMouseDown = false;

        public ProgramOrder()
        {
            InitializeComponent();
            UpdateUI();
            UpdateOrderUI();
            HandlerKiwoomAPI.Instance.onConnect += OnConnect;
            HandlerKiwoomAPI.Instance.onDisconnect += OnDisconnect;
            this.Enabled = false;

            // 잔고 데이터그리드뷰 교체
            var column = balanceDataGridView.Columns["상태"];
            var progressColumn = new DataGridViewProgressColumn();
            progressColumn.HeaderText = "상태";
            balanceDataGridView.Columns.Insert(column.Index, progressColumn);
            balanceDataGridView.Columns.Remove(column);
            balanceDataGridView.SetDoubleBuffered(true);
            allStockdataGridView.SetDoubleBuffered(true);
            recommendDataGridView.SetDoubleBuffered(true);
        }

        public void OnConnect()
        {
            this.Enabled = true;
            UpdateUI();
            string tempPassward = ProgramManager.GetTempPassward();
            if (!string.IsNullOrEmpty(tempPassward))
            {
                passwardTextBox.Text = tempPassward;
                StartCoroutine("LinkAccount");
            }
        }

        public void OnDisconnect()
        {
            this.Enabled = false;
        }

        private void Start()
        {
            HandlerKiwoomAPI.Instance.onRecieveTransactionData += OnRecieveTransactionData;
            HandlerKiwoomAPI.Instance.onRecieveRealData += OnRecieveRealData;
            HandlerKiwoomAPI.Instance.onReceiveChejanData += OnReceiveChejanData;
            //StartCoroutine("RequestAllStockInfo");
            //StartCoroutine("Test");
        }

        //private IEnumerator Test()
        //{
        //    yield return new WaitForSeconds(10f);
        //    var stock = StockListManager.Instance.GetStockInfoByName("동화약품");
        //    int count = 0;
        //    float rate = 1;
        //    while (true)
        //    {
        //        if (stock != null)
        //        {
        //            stock.SetPriceData("10000", "10000", rate.ToString(), "1");
        //            count++;
        //            yield return new WaitForSeconds(1f);
        //            if (count >= 60)
        //            {
        //                count = 0;
        //                rate++;
        //            }    
        //        }
        //    }
        //}

        private void OnReceiveChejanData()
        {
            // 체결 로그 업데이트
            //UpdateSendeOrderUI();
        }

        private void OnRecieveRealData()
        {
            // 시장 지수 UI 업데이트
            UpdateMarketIndexUI();

            // 모든 주식 종목
            UpdateAllStockDataGridViewUI();

            // 잔고 업데이트
            UpdateLinkAccountUI();
        }

        private void SceneUpdate()
        {
            // 추천 주식 종목
            UpdateRecommendStockDataGridViewUI();
        }

        private void OnRecieveTransactionData()
        {
            // 계좌 리스트 업데이트
            UpdateAccountIistUI();

            // 시장 지수 UI 업데이트
            UpdateMarketIndexUI();

            // 모든 주식 종목
            UpdateAllStockDataGridViewUI();

            // 잔고 업데이트
            UpdateLinkAccountUI();
        }

        private void UpdateUI()
        {
            // 계좌 리스트 업데이트
            UpdateAccountIistUI();

            // 시장 지수 UI 업데이트
            UpdateMarketIndexUI();

            // 모든 주식 종목
            UpdateAllStockDataGridViewUI();

            // 잔고 업데이트
            UpdateLinkAccountUI();

            // 프로그램 오더 영역
            UpdateProgramOrderExecuteUI();
        }

        /// <summary>
        /// 업종 지수 UI 업데이트
        /// </summary>
        private void UpdateMarketIndexUI()
        {
            if (marketIndexUICheckBox.Checked)
            {
                kospi.Text = ModelCenter.MarketIndex.KOSPI.ToStringGetToday();
                kosdaq.Text = ModelCenter.MarketIndex.KOSDAQ.ToStringGetToday();
                kospi200.Text = ModelCenter.MarketIndex.KOSPI200.ToStringGetToday();
            }
            else
            {
                kospi.Text = ModelCenter.MarketIndex.KOSPI.ToStringGetTotal();
                kosdaq.Text = ModelCenter.MarketIndex.KOSDAQ.ToStringGetTotal();
                kospi200.Text = ModelCenter.MarketIndex.KOSPI200.ToStringGetTotal();
            }
            kospi.ForeColor = Utils.GetTextColor(kospi.Text);
            kosdaq.ForeColor = Utils.GetTextColor(kosdaq.Text);
            kospi200.ForeColor = Utils.GetTextColor(kospi200.Text);
        }

        /// <summary>
        /// 계좌 리스트 UI 업데이트
        /// </summary>
        private void UpdateAccountIistUI()
        {
            if (KiwoomManager.Instance.IsLogin)
            {
                LoginInfo loginInfo = KiwoomManager.Instance.LoginInfo;
                if (loginInfo != null)
                {
                    if (loginInfo.IsAllowLoginInfo)
                    {
                        accountListComboBox.Items.Clear();
                        accountListComboBox.Items.AddRange(loginInfo.Accounts.ToArray());
                        accountListComboBox.SelectedIndex = 0;
                    }
                }
            }
        }

        private class StockDataByGrid
        {
            public StockInfo stockInfo;
            public string Name => stockInfo?.Name ?? "";
            public string StockPrice => $"{stockInfo?.StockPrice.ToString("n0")}원" ?? "";
            public string UpDownRate => $"{stockInfo?.UpDownRate.ToString("F2")}%" ?? "";
            public string TodayTradingRate => $"{stockInfo?.TodayTradingRate.ToString("F2")}%" ?? "";
            public double Score => stockInfo?.Score ?? 0;
            public string GrowthRatePerMinute => $"{stockInfo?.GrowthRatePerMinute.ToString("F2")}%" ?? "";
            public string SellName => "매수";

            public StockDataByGrid(StockInfo stockInfo)
            {
                this.stockInfo = stockInfo;
            }
        }

        private List<StockDataByGrid> _stockDataByGrids = null;

        /// <summary>
        /// 전체 종목 리스트 UI 업데이트
        /// </summary>
        private void UpdateAllStockDataGridViewUI()
        {
            if (_isMouseDown)
                return;

            if (_stockDataByGrids == null || _stockDataByGrids.Count == 0)
            {
                _stockDataByGrids = new List<StockDataByGrid>();
                var enumerator = StockListManager.Instance.StockInfos.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    _stockDataByGrids.Add(new StockDataByGrid(enumerator.Current.Value));
                }
            }

            int horizontalScrollingOffset = allStockdataGridView.HorizontalScrollingOffset;
            int firstDisplayedScrollingRowIndex = allStockdataGridView.FirstDisplayedScrollingRowIndex;
            allStockdataGridView.DataSource = _stockDataByGrids.OrderByDescending(a => a.Score).ThenByDescending(a => a.stockInfo.UpDownRate).ThenBy(a => a.stockInfo.tradingSymbol).ToList();
            allStockdataGridView.HorizontalScrollingOffset = horizontalScrollingOffset;
            if (firstDisplayedScrollingRowIndex >= 0)
                allStockdataGridView.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
            allStockdataGridView.ClearSelection();
        }

        private List<StockDataByGrid> _recommendStockDataByGrids = new List<StockDataByGrid>();

        /// <summary>
        /// 추천 종목 리스트 UI 업데이트
        /// </summary>
        private void UpdateRecommendStockDataGridViewUI()
        {
            if (_isMouseDown)
                return;

            _recommendStockDataByGrids.Clear();
            var recommendeds = ProgramOrderManager.Instance.recommendeds;
            for (int i = 0; i < recommendeds.Count; i++)
            {
                _recommendStockDataByGrids.Add(new StockDataByGrid(recommendeds[i]));
            }

            int horizontalScrollingOffset = recommendDataGridView.HorizontalScrollingOffset;
            int firstDisplayedScrollingRowIndex = recommendDataGridView.FirstDisplayedScrollingRowIndex;
            recommendDataGridView.DataSource = _recommendStockDataByGrids;
            recommendDataGridView.HorizontalScrollingOffset = horizontalScrollingOffset;
            if (firstDisplayedScrollingRowIndex >= 0)
                recommendDataGridView.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
            recommendDataGridView.ClearSelection();
        }

        /// <summary>
        /// 잔고 업데이트
        /// </summary>
        private void UpdateLinkAccountUI()
        {
            if (_isMouseDown)
                return;

            //balanceDataGridView.Refresh();
            todayProfitAmount.Text = "-";
            todayProfitRate.Text = "-";
            myStockCount.Text = "보유 종목 : 0개";
            summaryAmount.Text = string.Empty;

            var accountInfo = ProgramOrderManager.Instance.AccountInfo;
            if (accountInfo != null)
            {
                int horizontalScrollingOffset = balanceDataGridView.HorizontalScrollingOffset;
                int firstDisplayedScrollingRowIndex = balanceDataGridView.FirstDisplayedScrollingRowIndex;
                balanceDataGridView.Rows.Clear();
                for (int i = 0; i < accountInfo.BalanceStocks.Count; i++)
                {
                    var myStock = accountInfo.BalanceStocks[i];
                    DataGridViewProgressValue progressValue = new DataGridViewProgressValue();
                    switch (myStock.BalanceStockState)
                    {
                        case eBalanceStockState.RequestBuy:
                            progressValue.Value = 0f;
                            progressValue.Text = "매수 요청 중...";
                            break;
                        case eBalanceStockState.Buying:
                            progressValue.Value = (float)((myStock.OrderCount - myStock.WaitOrderCount) / myStock.OrderCount) * 100f;
                            progressValue.Text = $"매수 진행 중 ({myStock.OrderCount - myStock.WaitOrderCount}/{myStock.OrderCount})";
                            break;
                        case eBalanceStockState.RequestSell:
                            progressValue.Value = 0f;
                            progressValue.Text = "매도 요청 중...";
                            break;
                        case eBalanceStockState.Selling:
                            progressValue.Value = (float)((myStock.OrderCount - myStock.WaitOrderCount) / myStock.OrderCount) * 100f;
                            progressValue.Text = $"매도 진행 중 ({myStock.OrderCount - myStock.WaitOrderCount}/{myStock.OrderCount})";
                            break;
                        case eBalanceStockState.Have:
                            progressValue.Value = 100f;
                            progressValue.Text = "보유중";
                            break;
                        default:
                            break;
                    }

                    string buttonName = string.Empty;
                    switch (myStock.BalanceStockState)
                    {
                        case eBalanceStockState.RequestBuy:
                            buttonName = "매수주문취소";
                            break;
                        case eBalanceStockState.Buying:
                            buttonName = "매수취소";
                            break;
                        case eBalanceStockState.RequestSell:
                            buttonName = "매도주문취소";
                            break;
                        case eBalanceStockState.Selling:
                            buttonName = "매도취소";
                            break;
                        case eBalanceStockState.Have:
                            buttonName = "즉시청산";
                            break;
                        default:
                            break;
                    }
                    balanceDataGridView.Rows.Add(myStock.TraingSymbol
                                                , myStock.StockName
                                                , $"{myStock.BuyingMoney.ToString("n0")}원"
                                                , $"{myStock.HaveCnt}개"
                                                , $"{myStock.stockInfo.UpDownRate}%"
                                                , $"{myStock.stockInfo.GrowthRatePerMinute}%"
                                                , $"{myStock.CurrentTotalPrice.ToString("n0")}원"
                                                , $"{myStock.EstimatedProfitRate.ToString("F2")}%"
                                                , $"{myStock.EstimatedProfit.ToString("n0")}원"
                                                , progressValue
                                                , buttonName);
                    balanceDataGridView.Rows[i].DefaultCellStyle.ForeColor = Utils.GetTextColor(myStock.EstimatedProfitRate);
                }

                todayProfitAmount.Text = $"{accountInfo.TodayProfitAmount}원";
                todayProfitAmount.ForeColor = Utils.GetTextColor(accountInfo.TodayProfitAmount);
                todayProfitRate.Text = $"{accountInfo.TodayProfitRate:F2}%";
                todayProfitRate.ForeColor = Utils.GetTextColor(accountInfo.TodayProfitRate);
                myStockCount.Text = $"보유 종목 : {accountInfo.BalanceStocks.Count}개";
                summaryAmount.Text = $"예수금: {accountInfo.Deposit.ToString("n0")}원 / " +
                                     $"D+2예수금: {accountInfo.DepositAfter2Day.ToString("n0")}원 / " +
                                     $"추정자산: {accountInfo.EstimatedAssets.ToString("n0")}원 ({accountInfo.EstimatedAssets_Calc.ToString("n0")}원) /" +
                                     $"사용가능액 : {accountInfo.AvailableMoney.ToString("n0")}원";

                balanceDataGridView.HorizontalScrollingOffset = horizontalScrollingOffset;
                if (firstDisplayedScrollingRowIndex >= 0)
                    balanceDataGridView.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
                balanceDataGridView.ClearSelection();
            }
        }

        /// <summary>
        /// 주문 영역 UI 업데이트
        /// </summary>
        private void UpdateOrderUI()
        {
            startRateUpDown.Value = (decimal)ProgramOrderManager.Instance.StartRate;
            stopLossUpDown.Value = (decimal)ProgramOrderManager.Instance.StopLoss;
            limitPriceUpDown.Value = (decimal)ProgramOrderManager.Instance.LimitPrice;
            limitCountUpDown.Value = (decimal)ProgramOrderManager.Instance.LimitCount;
            limitRateUpDown.Value = (decimal)ProgramOrderManager.Instance.LimitRate;
            maxPriceRateUpDown.Value = (decimal)ProgramOrderManager.Instance.MaxPriceRate;
            baseRankUpDown.Value = (decimal)ProgramOrderManager.Instance.BaseRank;
            baseGrowthRatePerMinuteUpDown.Value = (decimal)ProgramOrderManager.Instance.BaseGrowthRatePerMinute;
        }

        /// <summary>
        /// 주문 기록 영역 UI 업데이트
        /// </summary>
        private void UpdateSendeOrderUI()
        {
            string orderNumber = KiwoomManager.Instance.GetSendOrderResultData(eFID.주문번호);
            string orderStatus = KiwoomManager.Instance.GetSendOrderResultData(eFID.주문상태);
            string orderStockName = KiwoomManager.Instance.GetSendOrderResultData(eFID.종목명);
            string orderStockCount = KiwoomManager.Instance.GetSendOrderResultData(eFID.주문수량);
            long orderPrice = long.Parse(KiwoomManager.Instance.GetSendOrderResultData(eFID.주문가격));
            string orderType = KiwoomManager.Instance.GetSendOrderResultData(eFID.주문구분);

            //recordListBox.Items.Add("======================================================");
            //recordListBox.Items.Add($"주문번호: {orderNumber}, 주문상태: {orderStatus}");
            //recordListBox.Items.Add($"종목명: {orderStockName}, 주문수량: {orderStockCount}");
            //recordListBox.Items.Add($"주문가격: {string.Format("{0:#,###}", orderPrice)}");
            //recordListBox.Items.Add($"주문구분: {orderType}");
            //recordListBox.Items.Add("======================================================");
        }
        
        /// <summary>
        /// 모든 종목 요청 및 분석 시작
        /// </summary>
        /// <returns></returns>
        private IEnumerator RequestAllStockInfo()
        {
            yield return new WaitUntil(() => KiwoomManager.Instance.IsLogin);

            // 오후 4시부터 23시 50분까지 돌아감
            yield return new WaitUntil(() => ProgramConfig.NowTime >= ProgramConfig.NowTime.Date.AddHours(16) 
                                             && ProgramConfig.NowTime <= ProgramConfig.NowTime.Date.AddHours(23).AddMinutes(50)
                                             || ProgramConfig.NowTime.DayOfWeek == DayOfWeek.Sunday 
                                             || ProgramConfig.NowTime.DayOfWeek == DayOfWeek.Saturday);

            int current = 0;
            int total = StockListManager.Instance.StockInfos.Count;
            var enumerator = StockListManager.Instance.StockInfos.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return new WaitUntil(() => KiwoomManager.Instance.IsLogin);
                StockInfo stockInfo = enumerator.Current.Value;
                ++current;
                DateTime startTime = ProgramConfig.NowTime;

                if (stockInfo.tradingVolume == 0)
                {
                    bool isReceive = false;
                    StockListManager.Instance.RequestStockInfo(stockInfo, (result) =>
                    {
                        isReceive = true;
                    });
                    yield return new WaitUntil(() => isReceive);
                    yield return new WaitForSeconds(3.5f);
                }
            }
        }

        private void marketIndexUICheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMarketIndexUI();
        }

        private void linkAccountButton_Click(object sender, EventArgs e)
        {
            StartCoroutine("LinkAccount");
        }

        private void passwardTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                StartCoroutine("LinkAccount");
        }

        /// <summary>
        /// 계좌 연결
        /// </summary>
        private IEnumerator LinkAccount()
        {
            if (accountListComboBox.Enabled && passwardTextBox.TextLength == 4)
            {
                if (accountListComboBox.SelectedItem != null)
                {
                    string targetNumber = accountListComboBox.SelectedItem.ToString();
                    var accountInfo = KiwoomManager.Instance.LoginInfo.Accounts.Find(account => account.AccountNumber.Equals(targetNumber));
                    if (accountInfo != null)
                    {
                        accountListComboBox.Enabled = false;
                        passwardTextBox.Enabled = false;
                        linkAccountButton.Text = "해제";

                        bool responseResult = false;
                        accountInfo.RequestAccountInfo(passwardTextBox.Text, (result) =>
                        {
                            if (result)
                            {
                                ToastMessage.Show("계좌 연결이 완료됐습니다.");
                                KiwoomManager.Instance.LoginInfo.SetSelectAccount(accountInfo);
                                ProgramOrderManager.Instance.LinkAccount(accountInfo, passwardTextBox.Text);
                                ProgramManager.SaveTempPassWard(passwardTextBox.Text);
                            }
                            responseResult = true;
                        });
                        yield return new WaitUntil(() => responseResult);
                        UpdateLinkAccountUI();
                    }
                }
            }
            else
            {
                KiwoomManager.Instance.LoginInfo.SetSelectAccount(null);
                ProgramOrderManager.Instance.UnlinkAccount();
                ProgramManager.ClearTempPassWard();
                accountListComboBox.Enabled = true;
                passwardTextBox.Enabled = true;
                passwardTextBox.Text = string.Empty;
                linkAccountButton.Text = "연결";
                balanceDataGridView.Rows.Clear();
            }
        }

        private void balanceDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                string traingSymbol = senderGrid.Rows[e.RowIndex].Cells["종목코드"].Value as string;
                string buttonName = senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
                var balanceStock = ProgramOrderManager.Instance.AccountInfo.BalanceStocks.Find(stock => stock.TraingSymbol.Equals(traingSymbol));
                if (balanceStock != null)
                {
                    if (buttonName.Equals("즉시청산"))
                    {
                        ProgramOrderManager.Instance.OrderSell(balanceStock.stockInfo, balanceStock.HaveCnt);
                    }
                    else if (buttonName.Equals("매수취소"))
                    {
                        ProgramOrderManager.Instance.OrderCancel(balanceStock.stockInfo);
                    }
                    else if (buttonName.Equals("매도취소"))
                    {
                        ProgramOrderManager.Instance.OrderCancel(balanceStock.stockInfo);
                    }
                    else if (buttonName.Equals("매수주문취소"))
                    {
                        ProgramOrderManager.Instance.OrderCancel(balanceStock.stockInfo);
                    }
                    else if (buttonName.Equals("매도주문취소"))
                    {
                        ProgramOrderManager.Instance.OrderCancel(balanceStock.stockInfo);
                    }
                }
            }
        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            senderGrid.ClearSelection();
        }

        private void allStockdataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                if (senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.Equals("매수"))
                {
                    string stockName = senderGrid.Rows[e.RowIndex].Cells["이름"].Value as string;
                    var stockInfo = StockListManager.Instance.GetStockInfoByName(stockName);
                    if (stockInfo != null)
                        ProgramOrderManager.Instance.OrderBuy(stockInfo, 1);
                }
            }
        }

        private void recommendDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                if (senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.Equals("매수"))
                {
                    string stockName = senderGrid.Rows[e.RowIndex].Cells["이름_추천"].Value as string;
                    var stockInfo = StockListManager.Instance.GetStockInfoByName(stockName);
                    if (stockInfo != null)
                        ProgramOrderManager.Instance.OrderBuy(stockInfo, 1);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LineNotify.SendMessage("화면 캡쳐", Utils.FormCapture(FormManager.MainForm));
            ToastMessage.Show("화면 캡쳐했습니다.");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FormManager.OpenForm(eForm.ProgramSetting);
        }

        private void kiwoomCSButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://bbn.kiwoom.com/bbn.openAPIQnaBbsList.do");
        }

        private void KGzeroCSButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.kiwoom.co.kr/nkw.templateFrameSet.do?m=m1101100000");
        }

        private void DataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            this._isMouseDown = true;
            Logger.Log("DOWN");
        }

        private void DataGridView_MouseUp(object sender, MouseEventArgs e)
        {
            this._isMouseDown = false;
            Logger.Log("UP");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (KiwoomManager.Instance.LoginInfo.SelectAccount != null)
                KiwoomManager.Instance.LoginInfo.SelectAccount.SaveAccountInfo();
            FormManager.OpenForm(eForm.RecodeAccountInfo);
        }

        private void programOrderExecuteButton_Click(object sender, EventArgs e)
        {
            ProgramOrderManager.Instance.IsAutoProgramOrder = !ProgramOrderManager.Instance.IsAutoProgramOrder;
            UpdateProgramOrderExecuteUI();
        }

        private void UpdateProgramOrderExecuteUI()
        {
            bool isAutoProgramOrder = ProgramOrderManager.Instance.IsAutoProgramOrder;
            if (isAutoProgramOrder)
            {
                programOrderExecuteButton.Text = "자동 매매 중지 (실행중..)";
            }
            else
            {
                programOrderExecuteButton.Text = "자동 매매 시작";
            }

            startRateUpDown.Enabled = !isAutoProgramOrder;
            stopLossUpDown.Enabled = !isAutoProgramOrder;
            limitPriceUpDown.Enabled = !isAutoProgramOrder;
            limitCountUpDown.Enabled = !isAutoProgramOrder;
            limitRateUpDown.Enabled = !isAutoProgramOrder;
            maxPriceRateUpDown.Enabled = !isAutoProgramOrder;
            baseRankUpDown.Enabled = !isAutoProgramOrder;
            baseGrowthRatePerMinuteUpDown.Enabled = !isAutoProgramOrder;
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            var senderUpDown = (NumericUpDown)sender;
            if (senderUpDown.Equals(startRateUpDown))
                ProgramOrderManager.Instance.StartRate = (float)startRateUpDown.Value;
            if (senderUpDown.Equals(stopLossUpDown))
                ProgramOrderManager.Instance.StopLoss = (float)stopLossUpDown.Value;
            if (senderUpDown.Equals(limitPriceUpDown))
                ProgramOrderManager.Instance.LimitPrice = (long)limitPriceUpDown.Value;
            if (senderUpDown.Equals(limitCountUpDown))
                ProgramOrderManager.Instance.LimitCount = (long)limitCountUpDown.Value;
            if (senderUpDown.Equals(limitRateUpDown))
                ProgramOrderManager.Instance.LimitRate = (float)limitRateUpDown.Value;
            if (senderUpDown.Equals(maxPriceRateUpDown))
                ProgramOrderManager.Instance.MaxPriceRate = (float)maxPriceRateUpDown.Value;
            if (senderUpDown.Equals(baseRankUpDown))
                ProgramOrderManager.Instance.BaseRank = (short)baseRankUpDown.Value;
            if (senderUpDown.Equals(baseGrowthRatePerMinuteUpDown))
                ProgramOrderManager.Instance.BaseGrowthRatePerMinute = (float)baseGrowthRatePerMinuteUpDown.Value;
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            // 계좌 리스트 업데이트
            UpdateAccountIistUI();

            // 시장 지수 UI 업데이트
            UpdateMarketIndexUI();

            // 모든 주식 종목
            UpdateAllStockDataGridViewUI();

            // 잔고 업데이트
            UpdateLinkAccountUI();

            // 추천 주식 종목
            UpdateRecommendStockDataGridViewUI();

            StartCoroutine("DelayRefreshButton");
        }

        private IEnumerator DelayRefreshButton()
        {
            string originalButtonText = refreshButton.Text;
            refreshButton.Enabled = false;
            refreshButton.Text = "대기";
            yield return new WaitForSeconds(1f);
            refreshButton.Enabled = true;
            refreshButton.Text = originalButtonText;
        }
        
        private void restartButton_Click(object sender, EventArgs e)
        {
            ProgramManager.Restart();
        }
    }
}
