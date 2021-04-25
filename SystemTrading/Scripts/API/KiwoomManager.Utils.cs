using System;
using System.Collections.Generic;

public partial class KiwoomManager
{
    /// <summary>
    /// 모든 주식 리스트 읽어서 가져오기
    /// </summary>
    public void GetAllStock()
    {

    }

    /// <summary>
    /// 주식 분석
    /// </summary>
    public void AnalysisAllStock()
    {
        // 1. 모든 종목을 요청해서 최근 30일간의 기록을 기준(보다 적을 수 있음)으로 성장 가능성 판단을 한다.
        // 2. 비슷한 종목끼리는 묶어서 같은 테마로 간주하고 성장 가능성을 판단한다.
        // 3. 성장 가능성을 기준으로 정렬하여 리스트를 보유하고 있는다. (파일 저장)
    }
}
