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
    public partial class ProgramSetting : Form
    {
        public ProgramSetting()
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
            this.lineTokeyTextBox.Text = string.Empty;
            this.ReferenceStockListText.Text = string.Empty;
        }

        private void UpdateUI()
        {
            this.lineTokeyTextBox.Text = ProgramConfig.UserSetting.lineNotifyToken;
            this.ReferenceStockListText.Text = StockListManager.DEFAULT_EXCEL_PATH;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            ProgramConfig.UserSetting.lineNotifyToken = this.lineTokeyTextBox.Text;
            ProgramConfig.SaveUserSetting();
        }

        /// <summary>
        /// 예제 (추후에 쓸 것 같아서)
        /// </summary>
        private void 파일열기()
        {
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                string path = OFD.FileName;
            }
        }
    }
}
