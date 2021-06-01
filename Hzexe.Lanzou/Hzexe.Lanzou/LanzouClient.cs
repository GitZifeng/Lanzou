using Hzexe.Lanzou.Model.Lanzou;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hzexe.Lanzou
{
    public class LanzouClient
    {
        readonly CookieContainer cookieContainer;
        const string refer = "https://pc.woozooo.com";
        const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Safari/537.36 Edg/91.0.864.37";
        readonly HttpMessageHandler handler;

        public LanzouClient(string cookieStr)
        {
            cookieContainer = new CookieContainer();
            string[] cookstr = cookieStr.Split(';');
            foreach (string str in cookstr)
            {
                string[] cookieNameValue = str.Split('=');
                Cookie ck = new Cookie(cookieNameValue[0].Trim().ToString(), cookieNameValue[1].Trim().ToString());
                ck.Domain = ".woozooo.com";
                cookieContainer.Add(ck);
            }
#if NETCOREAPP3_0
            handler = new SocketsHttpHandler()
            {
                AllowAutoRedirect = false,
                CookieContainer = cookieContainer,
                PooledConnectionLifetime = TimeSpan.FromHours(1),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(20),
                MaxConnectionsPerServer = 6,
            };
#else
            handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                CookieContainer = cookieContainer,
                MaxConnectionsPerServer = 6,
            };
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder_id">-1=传输到根目录</param>
        /// <param name="file">文件流</param>
        /// <param name="filename">文件名</param>
        /// <param name="filesize">文件大小</param>
        /// <returns></returns>
        public async Task<LanZouFileResult> FileUploadAsync(string folder_id, Stream file, string filename, int filesize)
        {
            HttpClient client = new HttpClient(handler, false);
            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StringContent("1"), "task");
            content.Add(new StringContent("2"), "ve");
            content.Add(new StringContent("WU_FILE_0"), "id");//WU_FILE_0=根目录 WU_FILE_1=第一层目录
            content.Add(new StringContent(filename, Encoding.UTF8), "name");
            content.Add(new StringContent("text/plain"), "type");
            content.Add(new StringContent(ToGMTFormat(DateTime.Now.AddDays(-50))), "lastModifiedDate");
            content.Add(new StringContent(filesize.ToString()), "size");
            content.Add(new StringContent(folder_id.ToString()), "folder_id_bb_n");
            content.Add(new StreamContent(file), "upload_file", filename);
            client.DefaultRequestHeaders.Add("user-agent", userAgent);
            client.DefaultRequestHeaders.Add("referer", refer);
            Debug.WriteLine(DateTime.Now);
            Debug.WriteLine(content);
            string json = null;
            try
            {//https://pc.woozooo.com/mydisk.php
                //https://pc.woozooo.com/fileup.php
                var rm = await client.PostAsync("https://pc.woozooo.com/fileup.php", content);
                if (rm.IsSuccessStatusCode)
                {
                    json = await rm.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("上传错误", ex);
            }
            finally
            {
                client.Dispose();
            }
            Debug.WriteLine(json);
            var lanZouFileResult = System.Text.Json.JsonSerializer.Deserialize<LanZouFileResult>(json);
            var file_id = lanZouFileResult.text[0].id;
            return lanZouFileResult;
        }

        public async Task<GetDirResponse> LsDirAsync(string folder_id)
        {
            HttpClient client = new HttpClient(handler, false);
            var dic = new Dictionary<string, string>(3);
            dic.Add("folder_id", folder_id);
            dic.Add("task", "47");
            var encodedContent = new FormUrlEncodedContent(dic);
            client.DefaultRequestHeaders.Add("user-agent", userAgent);
            client.DefaultRequestHeaders.Add("referer", refer);
            string json = null;
            try
            {
                var rm = await client.PostAsync("https://pc.woozooo.com/doupload.php", encodedContent);
                if (rm.IsSuccessStatusCode)
                {
                    json = await rm.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("上传错误", ex);
            }
            finally
            {
                client.Dispose();
            }

            var lanZouFileResult = System.Text.Json.JsonSerializer.Deserialize<GetDirResponse>(json);
            //var file_id = lanZouFileResult.text[0].id;
            return lanZouFileResult;
        }

        public async Task<GetFilesResponse> LsFilesAsync(string folder_id, int page = 1)
        {
            HttpClient client = new HttpClient(handler, false);
            var dic = new Dictionary<string, string>(3);
            dic.Add("folder_id", folder_id);
            dic.Add("task", "5");
            dic.Add("pg", "" + page);
            var encodedContent = new FormUrlEncodedContent(dic);
            client.DefaultRequestHeaders.Add("user-agent", userAgent);
            client.DefaultRequestHeaders.Add("referer", refer);
            string json = null;
            try
            {
                var rm = await client.PostAsync("https://pc.woozooo.com/doupload.php", encodedContent);
                if (rm.IsSuccessStatusCode)
                {
                    json = await rm.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("上传错误", ex);
            }
            finally
            {
                client.Dispose();
            }

            var lanZouFileResult = System.Text.Json.JsonSerializer.Deserialize<GetFilesResponse>(json);
            //var file_id = lanZouFileResult.text[0].id;
            return lanZouFileResult;
        }

        public async Task<MkdirResponse> MkdirAsync(string folder_id, string folder_name, string folder_description)
        {
            HttpClient client = new HttpClient(handler, false);
            var dic = new Dictionary<string, string>(3);
            dic.Add("parent_id", folder_id);
            dic.Add("task", "2");
            dic.Add("folder_name", folder_name);
            dic.Add("folder_description", folder_description);
            var encodedContent = new FormUrlEncodedContent(dic);
            client.DefaultRequestHeaders.Add("user-agent", userAgent);
            client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("referer", refer);
            string json = null;
            try
            {
                var rm = await client.PostAsync("https://pc.woozooo.com/doupload.php", encodedContent);
                rm.EnsureSuccessStatusCode();
                json = await rm.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("目录创建错误", ex);
            }
            finally
            {
                client.Dispose();
            }

            var obj = System.Text.Json.JsonSerializer.Deserialize<MkdirResponse>(json);
            return obj;
        }


        public async Task<string> FileDownloadAsync(string url)
        {
        LinkInfoException:
            HttpClient client = new HttpClient(handler, false);
            client.DefaultRequestHeaders.Add("user-agent", userAgent);
            client.DefaultRequestHeaders.Add("referer", url);

            //第一次请求，获取iframe的地址
            var res = await client.GetAsync(url);
            res.EnsureSuccessStatusCode();
            var html = await res.Content.ReadAsStringAsync();
            string p = @"<iframe.*?name=""\d{5,}"".*?src=""(.*?)""";
            var src = Regex.Match(html, p).Groups[1].Value;
            Uri u = new Uri(url);
            var hostbase = u.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            var frame = hostbase + src;

            //第二次请求得到js里的json数据里的sign
            res = await client.GetAsync(frame);
            res.EnsureSuccessStatusCode();
            html = await res.Content.ReadAsStringAsync();
            var mc = Regex.Match(html, @"var pdownload = '(.+?)'");

            //第三次请求 通过参数发起post请求,返回json数据
            Dictionary<string, string> ps = new Dictionary<string, string>(5)
            {
            { "action","downprocess"},
            { "sign",mc.Groups[1].Value},
            { "ves","1"},
            };
            FormUrlEncodedContent encodedContent = new FormUrlEncodedContent(ps);
            var linkUrl = hostbase + "/ajaxm.php";
            res = await client.PostAsync(linkUrl, encodedContent);
            res.EnsureSuccessStatusCode();
            html = await res.Content.ReadAsStringAsync();
            GetLinkResponse linkinfo = null;
            try
            {
                linkinfo = System.Text.Json.JsonSerializer.Deserialize<GetLinkResponse>(html);
            }
            catch 
            {
                goto LinkInfoException;
            }

            if (!linkinfo.zt.HasValue || linkinfo.zt.Value != 1)
                throw new Exception("获取直链失败，状态码：" + html);
            res = await client.GetAsync(linkinfo.FullUrl);
            res.EnsureSuccessStatusCode();
            html = await res.Content.ReadAsStringAsync();

            //通过json的数据拼接出最终的URL发起第最终请求,并得到响应信息头
            if (html.Contains("网络异常"))
            {
                Thread.Sleep(2000);
                client.DefaultRequestHeaders.Remove("referer");
                client.DefaultRequestHeaders.Add("referer", linkinfo.FullUrl);
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                var file_token = Regex.Match(html, @"'file':'(.+?)'").Groups[1].Value;
                var file_sign = Regex.Match(html, @"'sign':'(.+?)'").Groups[1].Value;
                var check_api = linkinfo.dom + "/file/ajax.php";
                var dic = new Dictionary<string, string>(3);
                dic.Add("file", file_token);
                dic.Add("sign", file_sign);
                dic.Add("el", "2");
                encodedContent = new FormUrlEncodedContent(dic);
                var c = new StringContent($"file_token={file_token}&file_sign={file_sign}&el=2", Encoding.UTF8, "application/x-www-form-urlencoded");
                res = await client.PostAsync(check_api, encodedContent);
                res.EnsureSuccessStatusCode();
                var resJson = await res.Content.ReadAsStringAsync();
                var jd = System.Text.Json.JsonDocument.Parse(resJson);
                var aurl = jd.RootElement.GetProperty("url").GetString();
                client.Dispose();
                return aurl;
            }
            else
            {
                client.Dispose();
                //重定向后的真直链
                return res.Content.Headers.ContentLocation.ToString();
            }
        }


        /// <summary>  
        /// 本地时间转成GMT格式的时间  
        /// </summary>  
        private static string ToGMTFormat(DateTime dt)
        {
            return dt.ToString("r") + dt.ToString("zzz").Replace(":", "");
        }

    }
}
