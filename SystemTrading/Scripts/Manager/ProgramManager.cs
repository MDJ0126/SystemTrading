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
        KiwoomManager.Instance.Clear();

        // ModelCenter 정리
        ModelCenter.Release();

        // MultiThread 해제
        MultiThread.Release();

        // 마지막으로 GC 콜렉트
        GC.Collect();
    }

    /// <summary>
    /// 프로그램 재실행
    /// </summary>
    public static void Restart()
    {
        LineNotify.SendMessage("프로그램을 재실행합니다.");
        Application.Restart();
    }

    /// <summary>
    /// 비밀번호 임시 저장
    /// </summary>
    public static void SaveTempPassWard(string passward)
    {
        Utils.FileSave("TempSavePassWard", passward);
    }

    /// <summary>
    /// 임시 저장한 비밀번호 제거
    /// </summary>
    public static void ClearTempPassWard()
    {
        Utils.FileSave("TempSavePassWard", "");
    }

    /// <summary>
    /// 임시 저장 비밀번호 읽어오기
    /// </summary>
    /// <returns></returns>
    public static string GetTempPassward()
    {
        return Utils.FileLoad<string>("TempSavePassWard") ?? string.Empty;
    }
}