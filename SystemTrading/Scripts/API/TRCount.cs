using System;
using System.Threading;

/// <summary>
/// Tran 카운트 관리 클레스
/// 키움 OpenAPI에서는 TR 요청에 카운트 제한이 있다. (초당 5회)
/// 제한을 넘을 경우 일시 정지가 되므로 관리가 필요하다.
/// </summary>
public class TRCount
{
    public event Action onChangedData;

    // http://www.softore.co.kr/index.php?mid=StockNotice&document_srl=7871
    // TR Count Monitoring => 1sec: 3 / 60sec: 77 / 600sec: 619 / 1800sec: 1562 / 3600sec: 2197
    private const float RECHARGE_INTERVAL_SECONDS = 60;         // 충전 주기 (초)
    private const int RECHARGE_TRCOUNT = 30;                    // 충전량
    private const int WEIGHT_MAX_STACK = 200;                   // 최대 가중치

    private DateTime _refreshedTime = DateTime.MinValue;        // 최근 갱신 시간
    private int _currentRemainCount = 0;                        // 현재 요청할 수 있는 카운트
    private int _weightStackCount = 0;                          // 연속으로 TR을 사용하는 경우 가중치가 쌓이는 변수

    /// <summary>
    /// Transaction 카운트 사용 (사용 요청 대기가 걸릴 수 있음)
    /// </summary>
    /// <returns></returns>
    public void WaitUse()
    {
        // 재충전
        if (_refreshedTime.AddSeconds(RECHARGE_INTERVAL_SECONDS) <= ProgramConfig.NowTime)
        {
            _refreshedTime = ProgramConfig.NowTime;
            _currentRemainCount  = RECHARGE_TRCOUNT;
        }

        // 카운트 사용 처리
        if (_currentRemainCount > 0)
        {
            // 카운트 소진 이상무
            --_currentRemainCount;
            onChangedData?.Invoke();
        }
        else
        {
            // 현재 남은 카운트가 없어서 대기 후 재귀
            Thread.Sleep(100);
            WaitUse();
            return;
        }

        // 연속으로 사용할 경우 가중치 증가
        if (_refreshedTime.AddSeconds(RECHARGE_INTERVAL_SECONDS) >= ProgramConfig.NowTime)
        {
            ++_weightStackCount;
            if (_weightStackCount > WEIGHT_MAX_STACK)
            {
                _weightStackCount = 0;

                const int WAIT_MINUTE = 5;
                Logger.Log($"연속적인 TR 요청으로 사용을 일시 중단합니다. (일시정지 시간 : {WAIT_MINUTE}분)");
                Thread.Sleep(1000 * 60 * WAIT_MINUTE);   // 5분 휴식
            }
        }
        else
        {
            --_weightStackCount;
        }
    }

    public string ToStringCurrentTRCount()
    {
        return $"{_currentRemainCount} / {RECHARGE_TRCOUNT}";
    }
}