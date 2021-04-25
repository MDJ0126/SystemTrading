using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class LineNotify
{
    private static readonly string TARGET_ADDRESS = "https://notify-api.line.me/api/notify";
    public static string MyToken { get; set; } = string.Empty;
    //MyTokey = "73p5tNH3KVvnpo15bg7kCM4UVTfXHnlSNlTCzEAdo8b";

    static LineNotify()
    {
        SetHeaders();
        ProgramConfig.OnChangedUserSetting += OnChangedUserSetting;
    }

    private static void SetHeaders()
    {
        MyToken = ProgramConfig.UserSetting.lineNotifyToken;
    }

    private static void OnChangedUserSetting()
    {
        SetHeaders();
    }

    /// <summary>
    /// LINE 메세지 보내기
    /// </summary>
    /// <param name="msg">메세지 내용</param>
    public static async void SendMessage(string msg, string imgPage = null)
    {
        var request = new Dictionary<string, object>();
        request.Add("message", msg);
        if (!string.IsNullOrEmpty(imgPage))
            request.Add("imageFile", new FormFile() { Name = "image.jpg", ContentType = "image/jpeg", FilePath = imgPage });
        var task = Task.Run(() => PostMultipart(MyToken, TARGET_ADDRESS, request));
        string webResult = await task;
        Logger.Log(webResult);
        #region Old Script
        //if (!string.IsNullOrEmpty(MyToken))
        //{
        //    try
        //    {
        //        WebClient webClient = new WebClient();
        //        webClient.Headers["Authorization"] = $"Bearer {MyToken}";

        //        NameValueCollection nameValue = new NameValueCollection();
        //        nameValue["message"] = msg as string;

        //        var task = Task.Run(() => webClient.UploadValuesTaskAsync(new Uri(TARGET_ADDRESS), nameValue));
        //        byte[] webResult = await task;
        //        string resultStr = Encoding.UTF8.GetString(webResult);
        //        //Logger.Log(resultStr);
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Log(e.Message);
        //    }
        //}
        #endregion
        /// 나중에 스티커 보내기할꺼면 이곳  참조 https://developers.line.biz/en/docs/messaging-api/sticker-list/
    }

    private class FormFile
    {
        public string Name { get; set; }

        public string ContentType { get; set; }

        public string FilePath { get; set; }

        public Stream Stream { get; set; }
    }

    private static string PostMultipart(string token, string url, Dictionary<string, object> parameters)
    {
        try
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;
            request.Headers.Add("Authorization", "Bearer " + token);

            if (parameters != null && parameters.Count > 0)
            {

                using (Stream requestStream = request.GetRequestStream())
                {

                    foreach (KeyValuePair<string, object> pair in parameters)
                    {

                        requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                        if (pair.Value is FormFile)
                        {
                            FormFile file = pair.Value as FormFile;
                            string header = "Content-Disposition: form-data; name=\"" + pair.Key + "\"; filename=\"" + file.Name + "\"\r\nContent-Type: " + file.ContentType + "\r\n\r\n";
                            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(header);
                            requestStream.Write(bytes, 0, bytes.Length);
                            byte[] buffer = new byte[32768];
                            int bytesRead;
                            if (file.Stream == null)
                            {
                                // upload from file
                                using (FileStream fileStream = File.OpenRead(file.FilePath))
                                {
                                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                        requestStream.Write(buffer, 0, bytesRead);
                                    fileStream.Close();
                                }
                            }
                            else
                            {
                                // upload from given stream
                                while ((bytesRead = file.Stream.Read(buffer, 0, buffer.Length)) != 0)
                                    requestStream.Write(buffer, 0, bytesRead);
                            }
                        }
                        else
                        {
                            string data = "Content-Disposition: form-data; name=\"" + pair.Key + "\"\r\n\r\n" + pair.Value;
                            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                            requestStream.Write(bytes, 0, bytes.Length);
                        }
                    }

                    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    requestStream.Write(trailer, 0, trailer.Length);
                    requestStream.Close();
                }
            }

            using (WebResponse response = request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                    return reader.ReadToEnd();
            }
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}

