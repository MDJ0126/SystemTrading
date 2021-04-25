using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class Logger : Singleton<Logger>
{
    private static int LOG_COUNT_MAX = 50;

    private enum LogType
    {
        Defalut,
        Error,
        Warning,
    }

    private class Message
    {
        public DateTime dateTime;
        public LogType type;
        public string log;

        public Message(LogType type, object log)
        {
            this.type = type;
            this.log = log.ToString();
            dateTime = ProgramConfig.NowTime;
        }
    }

    public delegate void OnAdd(string log);
    private event OnAdd _onAddLog;
    public event OnAdd OnAddLog
    {
        add
        {
            _onAddLog -= value;
            _onAddLog += value;
        }
        remove
        {
            _onAddLog -= value;
        }
    }

    private List<Message> _logs;

    protected override void Install()
    {
        _logs = new List<Message>();
    }

    protected override void Release()
    {
        if (_logs != null)
        {
            _logs.Clear();
            _logs = null;
        }
        _onAddLog = null;
    }

    private void WriteLog(Message message)
    {
        if (IsOpenedConsole)
        {
            switch (message.type)
            {
                case LogType.Defalut:
                    Console.WriteLine($"[{message.dateTime}] ==> {message.log}");
                    break;
                case LogType.Error:
                case LogType.Warning:
                    Console.WriteLine($"[{message.dateTime}] ==> [{message.type}] {message.log}");
                    break;
            }
        }
        _onAddLog?.Invoke(message.log);
    }

    public static void Log(object log)
    {
        AddLog(new Message(LogType.Defalut, log));
    }

    public static void LogError(object log)
    {
        AddLog(new Message(LogType.Error, log));
    }

    public static void LogWarning(object log)
    {
        AddLog(new Message(LogType.Warning, log));
    }

    private static object lockObject = new object();
    private static void AddLog(Message message)
    {
        lock (lockObject)
        {
            var logs = Instance._logs;
            if (logs.Count > LOG_COUNT_MAX)
                logs.RemoveAt(0);
            logs.Add(message);
            Instance.WriteLog(message);
        }
    }

    #region ## Console Function ##

    private const int STANDARD_OUTPUT_HANDLE = -11;
    /// <summary>
    /// 표준 핸들 구하기
    /// </summary>
    /// <param name="standardHandle">표준 핸들</param>
    /// <returns>표준 핸들</returns>
    [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr GetStdHandle(int standardHandle);

    /// <summary>
    /// 콘솔 할당하기
    /// </summary>
    /// <returns>처리 결과</returns>
    [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllocConsole();

    /// <summary>
    /// 콘솔 창 숨기기
    /// </summary>
    /// <returns>처리 결과</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeConsole();


    public static bool IsOpenedConsole { get; private set; } = false;
    public static bool OpenConsole()
    {
        bool isResult = false;
        if (!Debugger.IsAttached)
        {
            if (!IsOpenedConsole)
            {
                IsOpenedConsole = true;
                isResult = AllocConsole();
                IntPtr standardHandle = GetStdHandle(STANDARD_OUTPUT_HANDLE);
                SafeFileHandle standardSafeFileHandle = new SafeFileHandle(standardHandle, true);
                FileStream fileStream = new FileStream(standardSafeFileHandle, FileAccess.Write);
                Encoding encoding = Encoding.Default;
                StreamWriter streamWriter = new StreamWriter(fileStream, encoding);
                streamWriter.AutoFlush = true;
                Console.SetOut(streamWriter);

                for (int i = 0; i < Instance._logs.Count; i++)
                {
                    Instance.WriteLog(Instance._logs[i]);
                }
            }
        }
        else
            Log("디버그 모드에서는 실행되지 않습니다. 응용프로그램 모드(Ctrl + F5)에서 사용바랍니다.");
        return isResult;
    }

    public static bool CloseConsole()
    {
        if (IsOpenedConsole)
        {
            IsOpenedConsole = false;
            return FreeConsole();
        }
        else
            return false;
    }

    #endregion
}