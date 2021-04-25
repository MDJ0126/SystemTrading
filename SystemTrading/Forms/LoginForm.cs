﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemTrading.Forms
{
    public partial class LoginForm : SceneForm
    {
        public LoginForm()
        {
            InitializeComponent();
            this.SetActive(false);
            StartProcess();
        }

        /// <summary>
        /// 메인 프로세스
        /// </summary>
        private void StartProcess()
        {
            // BI
            ToastMessage.Show("반갑습니다.");

            // 1. API폼 열기
            Thread thread = new Thread(OpenAPI);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            // 2. 메인폼 열기
            MultiThread.Start(
                () =>
                {
                    var mainForm = new MainForm();
                    FormManager.MainForm = mainForm;
                    Application.Run(mainForm);
                    FormManager.MainForm = null;
                    ProgramManager.Realese();
                    Application.Exit();
                });
        }

        /// <summary>
        /// API폼 열기
        /// </summary>
        [STAThread]
        private void OpenAPI()
        {
            Application.Run(new APIController());
            HandlerKiwoomAPI.Instance.NotifyOnDisconnect();
        }
    }
}
