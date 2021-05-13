using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace AcgnuX.Source.Utils
{
    /// <summary>
    /// 文件工具类
    /// </summary>
    public class FileUtil
    {
        /// <summary>
        /// 从文件反序列化JSON到目标实体
        /// </summary>
        /// <typeparam name="T">目标实体</typeparam>
        /// <param name="JsonFileFullPath">json文件的完整路径</param>
        /// <returns>目标实体</returns>
        public static T DeserializeJsonFromFile<T>(string JsonFileFullPath) where T : new()
        {
            //文件不存在则直接返回
            if (!File.Exists(JsonFileFullPath))
            {
                return new T();
            }
            var jsonData = File.ReadAllText(JsonFileFullPath);
            var obj = JsonConvert.DeserializeObject<T>(jsonData);
            return obj;
        }

        /// <summary>
        /// 将实体序列化为JSON存储到文件
        /// </summary>
        /// <param name="data">目标数据</param>
        /// <param name="JsonFileFullPath">JSON完整保存目录</param>
        public static void SaveJsonToFile(object data, string JsonFileFullPath)
        {
            // Update json data string
            var jsonData = JsonConvert.SerializeObject(data);
            File.WriteAllText(JsonFileFullPath, jsonData);
        }

        /// <summary>
        /// 将文本保存至文件
        /// 文件存在则覆盖
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="fullPath">全路径</param>
        /// <param name="fileName">文件名称</param>
        public static void SaveStringToFile(string content, string filePath, string fileName)
        {
            if (!File.Exists(filePath + fileName))
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                File.WriteAllText(filePath + fileName, content, Encoding.UTF8);
            }
        }

        /// <summary>
        /// 创建目标文件夹(存在则跳过)
        /// </summary>
        /// <param name="folderPath">目标目录</param>
        /// <returns>true 成功</returns>
        public static bool CreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return true;
        }

        /// <summary>
        /// 删除目标文件夹
        /// </summary>
        /// <param name="dir">目标文件夹</param>
        public static void DeleteDir(string dir)
        {
            if (Directory.Exists(dir)) //如果存在这个文件夹删除之 
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                        File.Delete(d); //直接删除其中的文件                        
                    else
                        DeleteDir(d); //递归删除子文件夹 
                }
                Directory.Delete(dir, true); //删除已空文件夹                 
            }
        }

       /// <summary>
       /// 删除目标文件
       /// </summary>
       /// <param name="filePath"></param>
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        /// <summary>
        /// 返回不占用文件的 BitmapImage
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage GetBitmapImage(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = new MemoryStream(File.ReadAllBytes(path));
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="source">源完整路径</param>
        /// <param name="dest">目标完整路径</param>
        public static void CopyFile(string source, string dest)
        {
            //三个参数分别是源文件路径，存储路径，若存储路径有相同文件是否替换
            File.Copy(source, dest, true);
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="source">源完整路径</param>
        /// <param name="destPath">目标目录</param>
        /// <param name="destName">目标文件名</param>
        public static void CopyFile(string source, string destPath, string destName)
        {
            //if(destPath.LastIndexOf())
            CopyFile(source, destPath + destName);
        }

        /// <summary>
        /// 打开文件对话框, 返回所选文件完整路径
        /// </summary>
        /// <param name="initialPath">初始化路径</param>
        /// <param name="filter">文件过滤</param>
        /// <returns></returns>
        public static string OpenFileDialogForPath(string initialPath, string filter)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = initialPath,
                Filter = filter,
                RestoreDirectory = true,
                FilterIndex = 1
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return "";
        }

        /// <summary>
        /// 打开文件对话框, 返回所选文件夹完整路径
        /// </summary>
        /// <param name="initialPath">初始化路径</param>
        /// <param name="filter">文件过滤</param>
        /// <returns></returns>
        public static string OpenFolderDialogForPath(string initialPath)
        {
            var dialog = new FolderBrowserDialog()
            {
                Description = "请选择文件路径"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return "";
        }

        /// <summary>
        /// 重命名文件夹
        /// </summary>
        /// <param name="originPath">原始文件夹完整路径</param>
        /// <param name="newFolderName">新的名称</param>
        public static void RenameFolder(string originPath, string newFolderName)
        {
            Computer MyComputer = new Computer();
            MyComputer.FileSystem.RenameDirectory(originPath, newFolderName);
        }

        /// <summary>
        /// 校验图片是否损坏
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true 文件有效</returns>
        public static bool CheckImgIsValid(string path)
        {
            try
            {
                var bitmap = GetBitmapImage(path);
                if(null == bitmap)
                {
                    GC.Collect();
                    return false;
                }
                bitmap = null;
                GC.Collect();
                return true;
            }
            catch (Exception)
            {
                GC.Collect();
            }
            return false;
        }

        /// <summary>
        /// 替换文件名中的非法字符
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ReplaceInvalidChar(string source)
        {
            return source.Replace("\\", "")
                .Replace("/", "")
                .Replace("|", "")
                .Replace("*", "")
                .Replace(":", "")
                .Replace("?", "")
                .Replace("<", "")
                .Replace(">", "");
        }

        /// <summary>
        /// 以文本方式读取资源文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetApplicationResourceAsString(string path)
        {
            var uri = new Uri(path, UriKind.Relative);
            var info = System.Windows.Application.GetResourceStream(uri);
            StreamReader reader = new StreamReader(info.Stream, Encoding.UTF8);
            string text = reader.ReadToEnd();
            info.Stream.Close();
            return text;
        }

        /// <summary>
        /// 以流的方式读取资源文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static StreamResourceInfo GetApplicationResourceAsStream(string path)
        {
            var uri = new Uri(path, UriKind.Relative);
            var info = System.Windows.Application.GetResourceStream(uri);
            return info;
        }

        //public static JArray LoadJsonFile(string JsonFileFullPath)
        //{
        //    using (System.IO.StreamReader file = System.IO.File.OpenText(JsonFileFullPath))
        //    {
        //        using (JsonTextReader reader = new JsonTextReader(file))
        //        {
        //            return (JArray)JToken.ReadFrom(reader);
        //        }
        //    }

        //FileStream fs = new FileStream(JsonFileFullPath, FileMode.Open);
        //StreamReader fileStream = new StreamReader(fs);
        //string str = "";
        //string line;
        //while ((line = fileStream.ReadLine()) != null)
        //{
        //    str += line;
        //}
        ////上面的代码没有意义，只是将Json文件的内容加载到字符串中

        //JObject jObject = new JObject();        //新建 操作对象
        //AccessTokenModel a = JsonConvert.DeserializeObject<AccessTokenModel>(str);

        //Console.WriteLine(a.access_token);    //随意输出一个属性
        //Console.ReadKey();

        //}
    }
}
