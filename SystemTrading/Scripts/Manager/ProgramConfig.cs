using System;
using System.Globalization;
using System.Net;
using System.Windows.Forms;

public static class ProgramConfig
{
    #region ## ProgramSettingValue ##

    public delegate void OnChangedUserSettingValue();
    private static event OnChangedUserSettingValue _onChangedUserSetting;
    public static event OnChangedUserSettingValue OnChangedUserSetting
    {
        add
        {
            _onChangedUserSetting -= value;
            _onChangedUserSetting += value;
        }
        remove
        {
            _onChangedUserSetting -= value;
        }
    }

    [Serializable]
    public class ProgramSettingValue
    {
        public string lineNotifyToken;
    }

    private static ProgramSettingValue _userSetting = null;
    public static ProgramSettingValue UserSetting
    {
        get
        {
            if (_userSetting == null)
                _userSetting = Utils.FileLoad<ProgramSettingValue>("ProgramSettingValue");
            return _userSetting;
        }
    }

    public static void SaveUserSetting()
    {
        Utils.FileSave("ProgramSettingValue", _userSetting);
        _onChangedUserSetting?.Invoke();
        Logger.Log("프로그램 설정 값이 수정되었습니다.");
    }

    #endregion

    /// <summary>
    /// 현재 장 운영 시간인지?
    /// </summary>
    public static eTradingTimeState CheckTradingState { get; private set; } = eTradingTimeState.장_종료;

    /// <summary>
    /// 현재 시간
    /// </summary>
    public static DateTime NowTime { get { return _serverTime.AddTicks((DateTime.Now - _refrashedTime).Ticks); } }
    private static DateTime _serverTime = DateTime.Now;
    private static DateTime _refrashedTime = DateTime.Now;


    private static DateTime _marketEndTime = DateTime.MinValue;
    /// <summary>
    /// 장 시작 또는 종료까지 남은 시간(초)
    /// </summary>
    public static int MarketRemainSecond { get { return (int)_marketEndTime.Subtract(NowTime).TotalSeconds; } }

    /// <summary>
    /// 서버 시간 세팅
    /// </summary>
    /// <param name="serverTime"></param>
    public static void SetServerTime(DateTime serverTime)
    {
        _serverTime = serverTime;
        _refrashedTime = DateTime.Now;
    }

    /// <summary>
    /// 거래 시간인지 체크
    /// </summary>
    public static void SetIsTradingTime(eTradingTimeState value)
    {
        CheckTradingState = value;

        if (value == eTradingTimeState.장_종료)
        {
            StockListManager.Instance.SaveStockList();
            LineNotify.SendMessage($"{NowTime.Year}년 {NowTime.Month}월 {NowTime.Day}일 ({NowTime.ToStringDayofWeekInKorea()}) 장종료되었습니다.", Utils.FormCapture(FormManager.MainForm));
        }
    }

    /// <summary>
    /// 장 시작 또는 종료까지 남은 시간 세팅
    /// </summary>
    /// <param name="second"></param>
    public static void SetMarketRemainSecond(int second)
    {
        _marketEndTime = NowTime.AddSeconds(second);
    }

    /// <summary>
    /// 종목 구매 구분 아이템 추가하기
    /// </summary>
    /// <param name="comboBox"></param>
    public static void SetStockOrderTypeComboBox(ComboBox comboBox)
    {
        if (comboBox != null)
        {
            comboBox.Items.Clear();
            var enumValues = Enum.GetValues(typeof(eSendType));
            for (int i = 0; i < enumValues.Length; i++)
            {
                var value = ((eSendType)enumValues.GetValue(i)).ToDescription();
                comboBox.Items.Add(value);
            }
            comboBox.SelectedIndex = 1;
        }
    }
}