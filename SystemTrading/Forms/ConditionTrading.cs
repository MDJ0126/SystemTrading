using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemTrading.Forms
{
    public partial class ConditionTrading : Form
    {
        private StockInfo _searchStockInfo = null;

        public ConditionTrading()
        {
            InitializeComponent();
            SetMyAccountInfos();
            UpdateAccountUI();
        }

        private void OnReceiveSendOrderResult(eSendOrderResultType sendOrderResultType)
        {
            switch (sendOrderResultType)
            {
                case eSendOrderResultType.체결:
                    UpdateSendeOrderUI();
                    break;
                case eSendOrderResultType.국내주식잔고변경:
                    UpdateTodayProfitUI();
                    break;
                case eSendOrderResultType.파생잔고변경:
                    break;
                default:
                    break;
            }
        }

        private void SetMyAccountInfos()
        {
            passwardTextBox.Text = string.Empty;
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

        /// <summary>
        /// 주문 영역 UI 업데이트
        /// </summary>
        private void UpdateOrderUI()
        {
            // Init UI
            stockNameLabel.Text = "-";
            stockOrderTypeComboBox.Items.Clear();
            orderPriceUpDown.Value = 0;
            orderCountUpDown.Value = 0;

            // Update UI
            if (_searchStockInfo != null)
            {
                stockNameLabel.Text = $"{_searchStockInfo.Name}\n({_searchStockInfo.tradingSymbol})";
                ProgramConfig.SetStockOrderTypeComboBox(stockOrderTypeComboBox);
                orderPriceUpDown.Value = _searchStockInfo.GetOrderPrice(eOrderType.매수최우선호가);
                orderCountUpDown.Value = 1;
            }
        }

        /// <summary>
        /// 계좌정보 영역 UI 업데이트
        /// </summary>
        private void UpdateAccountUI()
        {
            // Init UI
            //accountListComboBox.Items.Clear();
            //passwardTextBox.Text = string.Empty;
            accountDepositLabel.Text = "0원";
            totalBuyAmount.Text = "0원";
            totalEvaluationAmount.Text = "0원";
            todayProfitAmount.Text = "0원";
            todayProfitRate.Text = "0.0%";

            // Update UI
            if (KiwoomManager.Instance.IsLogin)
            {
                var accountInfo = KiwoomManager.Instance.LoginInfo.Accounts.Find(info => info.AccountNumber.Equals(accountListComboBox.Text));
                if (accountInfo != null)
                {
                    accountDepositLabel.Text = accountInfo.Deposit.ToString("n0") + "원";
                    totalBuyAmount.Text = accountInfo.TotalBuyAmount.ToString("n0") + "원";
                    totalEvaluationAmount.Text = accountInfo.TotalEvaluationAmount.ToString("n0") + "원";
                    todayProfitAmount.Text = accountInfo.TodayProfitAmount.ToString("n0") + "원";
                    todayProfitRate.Text = accountInfo.TodayProfitRate.ToString("0.0") + "%";
                }
            }
        }
        
        /// <summary>
        /// 잔고 영역 UI 업데이트
        /// </summary>
        private void UpdateBalanceUI()
        {
            if (KiwoomManager.Instance.IsLogin)
            {
                var accountInfo = KiwoomManager.Instance.LoginInfo.Accounts.Find(info => info.AccountNumber.Equals(accountListComboBox.Text));
                if (accountInfo != null)
                {
                    balanceDataGridView.DataSource = accountInfo.BalanceStocks;
                }
            }
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

            recordListBox.Items.Add("======================================================");
            recordListBox.Items.Add($"주문번호: {orderNumber}, 주문상태: {orderStatus}");
            recordListBox.Items.Add($"종목명: {orderStockName}, 주문수량: {orderStockCount}");
            recordListBox.Items.Add($"주문가격: {string.Format("{0:#,###}", orderPrice)}");
            recordListBox.Items.Add($"주문구분: {orderType}");
            recordListBox.Items.Add("======================================================");
        }

        /// <summary>
        /// 당일 손익 영역 UI 업데이트
        /// </summary>
        private void UpdateTodayProfitUI()
        {
            string stockName = KiwoomManager.Instance.GetSendOrderResultData(eFID.종목명);
            long currentPrice = long.Parse(KiwoomManager.Instance.GetSendOrderResultData(eFID.현재가).Replace("-", ""));
            string profitRate = KiwoomManager.Instance.GetSendOrderResultData(eFID.손익율);
            long totalBuyingPrice = long.Parse(KiwoomManager.Instance.GetSendOrderResultData(eFID.총매입가));
            long profitMoney = long.Parse(KiwoomManager.Instance.GetSendOrderResultData(eFID.당일총매도손일));

            todayProfitAmount.Text = totalBuyingPrice.ToString("n0");
            todayProfitRate.Text = profitRate;
        }

        private void stockSearchButton_Click(object sender, EventArgs e)
        {
            var searchStock = FormManager.OpenForm<SearchStock>(eForm.SearchStock);
            searchStock.onSelectStock = delegate (StockInfo stockInfo)
            {
                _searchStockInfo = stockInfo;
                UpdateOrderUI();
            };
        }

        private void passwardTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                var accountInfo = KiwoomManager.Instance.LoginInfo.Accounts.Find(info => info.AccountNumber.Equals(accountListComboBox.Text));
                if (accountInfo != null)
                {
                    //accountInfo.RequestAccountInfo(passwardTextBox.Text, (result) =>
                    //{
                    //    if (result)
                    //    {
                    //        UpdateAccountUI();
                    //        UpdateBalanceUI();
                    //        UpdateOrderStockUI();
                    //    }
                    //});
                }
            }
        }

        private void longButton_Click(object sender, EventArgs e)
        {
            //if (_searchStockInfo != null)
            //{
            //    KiwoomManager.Instance.SendOrder(eSendOrderType.신규매수, _searchStockInfo, (int)orderCountUpDown.Value, (int)orderPriceUpDown.Value, (eSendType)stockOrderTypeComboBox.SelectedIndex);
            //}
            //else
            //    ToastMessage.Show("종목이 선택되지 않았습니다.");
        }

        private void shortButton_Click(object sender, EventArgs e)
        {
            //if (_searchStockInfo != null)
            //{
            //    KiwoomManager.Instance.SendOrder(eSendOrderType.신규매도, _searchStockInfo, (int)orderCountUpDown.Value, (int)orderPriceUpDown.Value, (eSendType)stockOrderTypeComboBox.SelectedIndex);
            //}
            //else
            //    ToastMessage.Show("종목이 선택되지 않았습니다.");
        }
    }
}
