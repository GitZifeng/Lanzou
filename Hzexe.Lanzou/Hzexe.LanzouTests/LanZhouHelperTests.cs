using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hzexe.Lanzou;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Hzexe.Lanzou.Tests
{
    [TestClass()]
    public class LanZhouHelperTests
    {
        const string COOKIE = "这里是cookie";//测试时cookie必须填
         [TestMethod()]
        public void FileUploadAndDownAsyncTest()
        {
            LanzouClient client = new LanzouClient(COOKIE);
            var f = File.OpenRead(@"c:\somefile.txt");//测试时请在C盘下创建somefile.txt文件
            var tt = client.FileUploadAsync("-1", f, DateTime.Now.ToFileTime() + ".zip", (int)f.Length).Result;
            f.Close();
            Assert.AreEqual(tt.zt, 1);
            var url = client.FileDownloadAsync(tt.text[0].is_newd + "/" + tt.text[0].f_id).Result;
            Assert.IsNotNull(url);
            WriteLog("c:\\url.txt", url);
        }


        [TestMethod()]
        public void FileDownAsyncTest()
        {
            LanzouClient client = new LanzouClient(COOKIE);
            var url = client.FileDownloadAsync("https://wwa.lanzoui.com/ijZl7pol87a").Result;
            Assert.IsNotNull(url);
        }

        void WriteLog(string path,string text)
        {
            FileStream fs = null;
            if (!File.Exists(path))
            {
                fs= new FileStream(path, FileMode.Create);
            }
            else
            {
                fs=new FileStream(path, FileMode.Append);
            }        
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(text);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
    }
}