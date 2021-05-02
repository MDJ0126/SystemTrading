using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

public static class Utils
{
    private static string SOLUTION_NAME = Assembly.GetEntryAssembly().GetName().Name;
    private static string APPDATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"/{SOLUTION_NAME}";

    /// <summary>
    /// 파일 저장
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="instance"></param>
    public static void FileSave<T>(string name, T instance)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            try
            {
                // 부모 폴더가 없으면 생성
                DirectoryInfo di = new DirectoryInfo(APPDATA_PATH);
                if (!di.Exists) di.Create();

                // 바이너리 직렬화 후 파일로 저장
                var bf = new BinaryFormatter();
                bf.Serialize(ms, instance);
                using (StreamWriter writer = new StreamWriter(APPDATA_PATH + $"/{name}.dat"))
                {
                    writer.Write(Convert.ToBase64String(ms.ToArray()));
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                SystemTrading.Forms.ToastMessage.Show(e.Message);
            }
        }
    }

    /// <summary>
    /// 파일 불러오기
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T FileLoad<T>(string name)
    {
        try
        {
            // 파일 읽기
            byte[] bytes;
            using (StreamReader reader = new StreamReader(APPDATA_PATH + $"/{name}.dat"))
            {
                bytes = Convert.FromBase64String(reader.ReadToEnd());
                reader.Close();
            }

            // 바이너리 역직렬화 후 반환
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                object obj = new BinaryFormatter().Deserialize(ms);
                return (T)obj;
            }
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// 폼 캡쳐
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public static string FormCapture(Control control)
    {
        // 경로 참고: https://pcsak3.com/502
        string strFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        string strOutput = $"{strFolder}/SystemTrading.png";
        Bitmap bmp = new Bitmap(control.Size.Width, control.Size.Height);
        Graphics grp = Graphics.FromImage(bmp);
        grp.CopyFromScreen(new Point(control.Bounds.X, control.Bounds.Y), new Point(0, 0), control.Size);
        bmp.Save(strOutput, System.Drawing.Imaging.ImageFormat.Png);
        return strOutput;
    }

    /// <summary>
    /// Enum 확장메소드 Description 읽어오기
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string ToDescription(this Enum source)
    {
        FieldInfo fi = source.GetType().GetField(source.ToString());
        var att = (DescriptionAttribute)fi.GetCustomAttribute(typeof(DescriptionAttribute));
        if (att != null)
        {
            return att.Description;
        }
        else
        {
            return source.ToString();
        }
    }

    /// <summary>
    /// index로 Enum 값 찾기
    /// </summary>
    /// <typeparam name="T">Enum Type</typeparam>
    /// <param name="index">Enum item index</param>
    /// <returns></returns>

    public static T FindEnumValue<T>(int index) where T : Enum
    {
        return (T)Enum.ToObject(typeof(T), index);
    }

    /// <summary>
    /// string으로 Enum 값 찾기
    /// </summary>
    /// <typeparam name="T">Enum Type</typeparam>
    /// <param name="str">Enum item string</param>
    /// <returns></returns>
    public static T FindEnumValue<T>(string str) where T : Enum
    {
        string[] enums = Enum.GetNames(typeof(T));

        T result = (T)Enum.ToObject(typeof(T), 0);

        for (int i = 0; i < enums.Length; i++)
        {
            if (str == enums[i])
                result = (T)Enum.ToObject(typeof(T), i);
        }

        return result;
    }

    public static T GetLast<T>(this ICollection<T> collection)
    {
        if (collection.Count > 0)
        {
            T item = default(T);
            var enumerator = collection.GetEnumerator();
            while (true)
            {
                item = enumerator.Current;
                if (!enumerator.MoveNext())
                    return item;
            }
        }
        return default(T);
    }

    public static T GetFirst<T>(this ICollection<T> collection)
    {
        if (collection.Count > 0)
        {
            var enumerator = collection.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }
        return default(T);
    }

    /// <summary>
    /// 특정 행으로 스크롤 하기
    /// </summary>
    /// <param name="listView">ListView</param>
    /// <param name="rowIndex">행 인덱스</param>
    public static void Scroll(this ListView listView, int rowIndex)
    {
        if (listView.Items.Count == 0)
            return;

        rowIndex = Math.Min(Math.Max(0, rowIndex), listView.Items.Count - 1);
        listView.EnsureVisible(rowIndex);
    }


    /// <summary>
    /// 폼 활성화 여부
    /// </summary>
    /// <param name="form"></param>
    /// <param name="isActive"></param>
    public static void SetActive(this Form form, bool isActive)
    {
        if (isActive)
        {
            form.Opacity = 100f;
            form.ShowInTaskbar = true;
        }
        else
        {
            form.Opacity = 0f;
            form.ShowInTaskbar = false;
        }
    }

    /// <summary>
    /// 음수, 양수에 따른 텍스트 컬러 가져오기
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Color GetTextColor(float value)
    {
        if (value > 0f)
        {
            return Color.Red;
        }
        else if (value < 0f)
        {
            return Color.Blue;
        }
        return Color.Black;
    }

    /// <summary>
    /// 음수, 양수에 따른 텍스트 컬러 가져오기
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Color GetTextColor(string symbol)
    {
        if (symbol.Contains("▲") || symbol.Contains("+"))
        {
            return Color.Red;
        }
        else if (symbol.Contains("▼") || symbol.Contains("-"))
        {
            return Color.Blue;
        }
        return Color.Black;
    }

    /// <summary>
    /// 컨트롤 더블 버퍼링 설정
    /// </summary>
    /// <param name="contorl"></param>
    /// <param name="setting"></param>
    public static void SetDoubleBuffered(this Control contorl, bool setting) 
    {
        Type dgvType = contorl.GetType();
        PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
        pi.SetValue(contorl, setting, null); 
    }

    /// <summary>
    /// 한국 포맷 요일 가져오기
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToStringDayofWeekInKorea(this DateTime dateTime)
    {
        switch (dateTime.DayOfWeek)
        {
            case DayOfWeek.Sunday:
                return "일요일";
            case DayOfWeek.Monday:
                return "월요일";
            case DayOfWeek.Tuesday:
                return "화요일";
            case DayOfWeek.Wednesday:
                return "수요일";
            case DayOfWeek.Thursday:
                return "목요일";
            case DayOfWeek.Friday:
                return "금요일";
            case DayOfWeek.Saturday:
                return "토요일";
        }
        return string.Empty;
    }

    #region Serialize

    //오브젝트 시리얼라이즈 후 결과 값을 스트링으로 변환하여 반환
    public static string SerializeObjectToString(object obj)
    {
        using (var memoryStream = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(memoryStream, obj);
            memoryStream.Flush();
            memoryStream.Position = 0;

            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }

    //오브젝트를 시리얼라이즈 후 바이트 배열 형태로 반환
    public static byte[] SerializeObjectToByteArray(object obj)
    {
        using (var memoryStream = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(memoryStream, obj);
            memoryStream.Flush();
            memoryStream.Position = 0;

            return memoryStream.ToArray();
        }
    }

    #endregion

    #region Deserialize

    //스트링 타입의 시리얼라이즈된 데이타를 디시리얼라이즈 후 해당 타입으로 변환하여 반환
    public static T Deserialize<T>(string xmlText)
    {
        if (xmlText != null && xmlText != String.Empty)
        {
            byte[] b = Convert.FromBase64String(xmlText);
            using (var stream = new MemoryStream(b))
            {
                var formatter = new BinaryFormatter();
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
        else
        {
            return default(T);
        }
    }

    //바이트 배열 형태의 시리얼라이즈된 데이타를 디시리얼라이즈 후 해당 타입으로 변환하여 반환
    public static T Deserialize<T>(byte[] byteData)
    {
        using (var stream = new MemoryStream(byteData))
        {
            var formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(stream);
        }
    }

    #endregion
}