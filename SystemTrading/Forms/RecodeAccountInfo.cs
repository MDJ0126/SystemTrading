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
    public partial class RecodeAccountInfo : SceneForm
    {
        public RecodeAccountInfo()
        {
            InitializeComponent();
            UpdateAccountIistUI();
            UpdateDataGridViewUI();
        }

        private void UpdateDataGridViewUI()
        {
            accountDataGridView.Rows.Clear();
            Dictionary<DateTime, AccountInfo> accountInfos;
            if (AccountInfoUtils.AccountInfos.TryGetValue(accountListComboBox.SelectedItem.ToString(), out accountInfos))
            {
                var enumerator = accountInfos.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var account = enumerator.Current.Value;
                    accountDataGridView.Rows.Add(enumerator.Current.Key.ToString("yyyy년 MM월 dd일"),
                                                 $"{account.Deposit.ToString("n0")}원",
                                                 $"{account.DepositAfter2Day.ToString("n0")}원",
                                                 $"{account.EstimatedAssets.ToString("n0")}원",
                                                 $"{account.TodayProfitRate.ToString("F2")}%",
                                                 $"{account.TodayProfitAmount.ToString("n0")}원");
                }
            }
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

        private void accountListComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGridViewUI();
        }
    }
}
