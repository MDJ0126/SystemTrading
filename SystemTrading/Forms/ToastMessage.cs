using System;
using System.Collections;
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
    public partial class ToastMessage : SceneForm
    {
        private static ToastMessage _toastMessage;

        private static string _text = string.Empty;

        private double _opacityCach = 0f;

        public static void Show(string text)
        {
            if (_toastMessage == null)
                MultiThread.Start(() => Application.Run(new ToastMessage(text)));
            else
                _toastMessage.SetText(text);
        }

        public ToastMessage(string text)
        {
            InitializeComponent();
            ShowInTaskbar = false;
            _toastMessage = this;
            _opacityCach = Opacity;
            SetText(text);
        }

        public ToastMessage()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            _toastMessage = this;
        }

        /// <summary>
        /// 토스트 메세지 세팅
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            _text = text;
            PlayAnimaion();
        }

        private void PlayAnimaion()
        {
            StopCoroutine("DelayClose");
            StartCoroutine("DelayClose");
        }

        private IEnumerator DelayClose()
        {
            this.text.Text = _text;
            Opacity = _opacityCach;
            TopMost = true;
            yield return new WaitForSeconds(1f);
            Opacity = 0;
        }
    }
}
