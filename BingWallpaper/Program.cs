using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Bing
{
    class Program
    {
        private static WebClient client = new WebClient();

        private static void Main(string[] args)
        {
            // 获取壁纸接口数据
            byte[] bytes = Program.client.DownloadData("https://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-cn");
            string json = Encoding.UTF8.GetString(bytes);
            Console.WriteLine(json);
            // 解析
            HPImageArchive hpimageArchive = (HPImageArchive)new DataContractJsonSerializer(typeof(HPImageArchive))
                .ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            // 壁纸Url
            string url = "http://cn.bing.com" + hpimageArchive.images[0].url;
            // 壁纸保存文件名
            string fileName = DateTime.Today.ToLongDateString() + " " + hpimageArchive.images[0].copyright + " " + Path.GetFileName(url);
            // 将其中特殊字符去除
            foreach (char oldChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(oldChar, '_');
            }
            fileName = fileName.Replace('©', 'C');
            // 保存文件夹
            string dir = DateTime.Today.Year + "\\" + DateTime.Today.Month;
            Directory.CreateDirectory(dir);
            fileName = dir + "\\" + fileName;
            Console.WriteLine(fileName);
            // 下载文件
            Program.client.DownloadFile(url, fileName);
            // 复制出一份，可以作为浏览器新标签页壁纸用
            File.Copy(fileName, "wallpaper.jpg", true);
            // 设为Windows壁纸
            Program.SetWallpaper(Directory.GetCurrentDirectory() + "\\" + fileName);
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static void SetWallpaper(string path)
        {
            Program.SystemParametersInfo(20, 0, path, 3);
        }
    }

    public class HPImageArchive
    {
        public HPImageArchive.Image[] images;

        public class Image
        {
            public string url;
            public string copyright;
        }
    }
}
