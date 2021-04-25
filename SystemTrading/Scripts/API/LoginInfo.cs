using AxKHOpenAPILib;
using System;
using System.Collections.Generic;

/// <summary>
/// 로그인 정보 클레스
/// </summary>
public class LoginInfo
{
    /// <summary>
    /// 이상 없이 로그인 정보가 세팅되었는지 여부
    /// </summary>
    public bool IsAllowLoginInfo { get; private set; }

    /// <summary>
    /// 사용자 ID
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// 사용자 이름
    /// </summary>
    public string UserName { get; private set; }

    /// <summary>
    /// 계좌 리스트
    /// </summary>
    public List<AccountInfo> Accounts { get; private set; }

    /// <summary>
    /// 현재 사용중인 계좌
    /// </summary>
    public AccountInfo SelectAccount { get; private set; }

    public enum eKEY_BSECGB
    {
        Active = 0, // 설정
        Cancel = 1, // 해지
    }

    /// <summary>
    /// 키보드 보안 사용 여부
    /// </summary>
    public eKEY_BSECGB KeyBsecgb { get; private set; }

    public enum eFIREW_SECGB
    {
        Unused = 0, // 미설정
        Active = 1, // 설정
        Cancel = 2, // 해지
    }

    /// <summary>
    /// 방화벽 설정 여부
    /// </summary>
    public eFIREW_SECGB FirewSecgb { get; private set; }

    public void SetLoginInfo(AxKHOpenAPI axKHOpenAPI, Action<bool> onFinished)
    {
        Clear();
        if (axKHOpenAPI != null)
        {
            this.IsAllowLoginInfo = true;

            // 사용자 아이디 가져오기
            this.UserId = axKHOpenAPI.GetLoginInfo("USER_ID");
            if (string.IsNullOrEmpty(this.UserId))
                this.IsAllowLoginInfo = false;

            // 사용자 이름 가져오기
            this.UserName = axKHOpenAPI.GetLoginInfo("USER_NAME");
            if (string.IsNullOrEmpty(this.UserName))
                this.IsAllowLoginInfo = false;

            // 계좌 리스트 가져오기
            string responseAccountString = axKHOpenAPI.GetLoginInfo("ACCNO");
            string[] accountArray = responseAccountString.Split(';');
            this.Accounts = new List<AccountInfo>();
            for (int i = 0; i < accountArray.Length; i++)
            {
                if (!string.IsNullOrEmpty(accountArray[i]))
                    this.Accounts.Add(new AccountInfo(accountArray[i]));
            }

            if (this.Accounts.Count == 0)
                this.IsAllowLoginInfo = false;
            //else
            //{
            //    SelectAccount = this.Accounts[0];
            //}

            // 키보드 보안 사용 여부 가져오기
            int keyBsecgbResult;
            if (Int32.TryParse(axKHOpenAPI.GetLoginInfo("KEY_BSECGB"), out keyBsecgbResult))
            {
                this.KeyBsecgb = (eKEY_BSECGB)keyBsecgbResult;
            }
            else
                this.IsAllowLoginInfo = false;

            // 방화벽 설정 여부 가져오기
            int firewSecgbResult;
            if (Int32.TryParse(axKHOpenAPI.GetLoginInfo("FIREW_SECGB"), out firewSecgbResult))
            {
                this.FirewSecgb = (eFIREW_SECGB)firewSecgbResult;
            }
            else
                this.IsAllowLoginInfo = false;
            onFinished?.Invoke(this.IsAllowLoginInfo);
        }
        else
            onFinished?.Invoke(false);
    }

    public void SetSelectAccount(AccountInfo accountInfo)
    {
        this.SelectAccount = accountInfo;
    }

    public AccountInfo GetAccountInfo(string accountNumber)
    {
        return this.Accounts.Find(info => info.AccountNumber.Equals(accountNumber));
    }

    public void Clear()
    {
        this.IsAllowLoginInfo = false;
        this.UserId = string.Empty;
        this.UserName = string.Empty;
        this.Accounts = null;
        this.KeyBsecgb = eKEY_BSECGB.Cancel;
        this.FirewSecgb = eFIREW_SECGB.Cancel;
    }
}