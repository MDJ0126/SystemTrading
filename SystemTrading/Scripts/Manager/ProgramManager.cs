using System;
using System.Collections.Generic;
using System.Windows.Forms;

public class ProgramManager
{
    /// <summary>
    /// 프로그램 초기화
    /// </summary>
    public static void Initialize()
    {

    }

    /// <summary>
    /// 프로그램 소멸자
    /// </summary>
    public static void Realese()
    {
        // 키움 API 종료
        KiwoomManager.Instance.LogOut();

        // ModelCenter 정리
        ModelCenter.Release();

        // MultiThread 해제
        MultiThread.Release();

        // 마지막으로 GC 콜렉트
        GC.Collect();
    }
}