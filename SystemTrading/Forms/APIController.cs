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
    public partial class APIController : SceneForm
    {
        public APIController()
        {
            InitializeComponent();
            this.SetActive(false);
            KiwoomManager.Instance.SetAPI(this.axKHOpenAPI);
            StartCoroutine("StartProcess");
        }

        public void CloseForm()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(CloseForm));
            }
            else
            {
                KiwoomManager.Instance.Clear();
                Close();
            }
        }

        /// <summary>
        /// 메인 프로세스
        /// </summary>
        private IEnumerator StartProcess()
        {
            // 1. 자동 로그인
            bool isLogin = false;
            KiwoomManager.Instance.OpenLoginForm((result) =>
            {
                if (result)
                {
                    isLogin = true;
                }
            });
            yield return new WaitUntil(() => isLogin);
            ToastMessage.Show("키움 API 연결됐습니다.");

            // 2. 모델센터 세팅
            ModelCenter.Initialize();

            // 3. 리얼 데이터 모두 등록
            StockListManager.Instance.ConnectionAllStocks(() =>
            {
                Logger.Log("모든 종목 실시간 갱신 데이터 연결 완료");
            });
        }
    }
}
