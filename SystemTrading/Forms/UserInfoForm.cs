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
    public partial class UserInfoForm : Form
    {
        public UserInfoForm()
        {
            InitializeComponent();
            InitUI();
            UpdateUI();

            KiwoomManager.Instance.OnLogin += OnLogin;
        }

        private void OnLogin()
        {
            UpdateUI();
        }

        private void InitUI()
        {
            this.idLabel.Text = "(접속 아이디)";
            this.nameLabel.Text = "(이름)";
            this.keyboardLabel.Text = "(키보드 보안 여부)";
            this.firewallLabel.Text = "(방화벽 사용 여부)";
            this.accountComboBox.Items.Clear();
        }

        private void UpdateUI()
        {
            if (KiwoomManager.Instance.IsLogin)
            {
                LoginInfo loginInfo = KiwoomManager.Instance.LoginInfo;
                if (loginInfo.IsAllowLoginInfo)
                {
                    this.idLabel.Text = loginInfo.UserId;
                    this.nameLabel.Text = loginInfo.UserName;
                    this.keyboardLabel.Text = loginInfo.KeyBsecgb.ToString();
                    this.firewallLabel.Text = loginInfo.FirewSecgb.ToString();
                    this.accountComboBox.Items.AddRange(loginInfo.Accounts.ToArray());
                    this.accountComboBox.SelectedIndex = 0;
                }
            }
        }
    }
}
