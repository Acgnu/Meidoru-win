using AcgnuX.Source.Bussiness.Constants;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
        /// windows系统保留文件夹名
        /// </summary>
        private static readonly string[] RESERVED_FOLDER_NAMES = { "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1"};

        [DllImport("shell32.dll", ExactSpelling = true)]
        private static extern void ILFree(IntPtr pidlList);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll", ExactSpelling = true)]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlList, uint cild, IntPtr children, uint dwFlags);

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
        /// 增量将实体序列化为JSON存储到文件, 备份原文件
        /// </summary>
        /// <param name="data">目标数据</param>
        /// <param name="JsonFileFullPath">JSON完整保存目录</param>
        public static void IncrSaveJsonToFile(object data, string JsonFileFullPath)
        {
            if(File.Exists(JsonFileFullPath))
            {
                var originFileName = Path.GetFileNameWithoutExtension(JsonFileFullPath);
                var originFileExtension = Path.GetExtension(JsonFileFullPath);
                var orginFileDir = Path.GetDirectoryName(JsonFileFullPath);
                var bakFileName = originFileName + ".bak" + originFileExtension;
                var bakFileFullPath = Path.Combine(orginFileDir, bakFileName);
                File.Copy(JsonFileFullPath, bakFileFullPath, true);
            }
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
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            File.WriteAllText(Path.Combine(filePath, fileName), content, Encoding.UTF8);
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
        /// 删除文件夹
        /// </summary>
        /// <param name=""></param>
        private static void DeleteDir(string dir)
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
        /// 删除目标文件夹
        /// 例如删除 C:\AAA\BBB
        /// 参数1=C:\AAA, 参数2=BBB
        /// </summary>
        /// <param name="dirPrePath">目标文件夹上级路径</param>
        /// <param name="dirName">目标文件夹名称</param>
        public static void DeleteDirWithName(string dirPrePath, string dirName)
        {

            if (string.IsNullOrEmpty(dirName))
            {
                throw new Exception("文件夹名称不能为空");
            }
            DeleteDir(Path.Combine(dirPrePath, dirName));
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
        /// 替换文件名中的非法字符
        /// </summary>
        /// <param name="source">原始名称</param>
        /// <param name="conflict">冲突时名称</param>
        /// <returns></returns>
        public static string ReplaceInvalidChar(string source)
        {
            var invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                source = source.Replace(c.ToString(), "");
            }
            for (var i = 0; i < RESERVED_FOLDER_NAMES.Length; i++)
            {
                if (string.Compare(RESERVED_FOLDER_NAMES[i], source, true) == 0)
                {
                    return string.Empty;
                }
            }
            return source;
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

        /// <summary>
        /// 打开路径并定位文件...对于@"h:\Bleacher Report - Hardaway with the safe call ??.mp4"这样的，explorer.exe /select,d:xxx不认，用API整它
        /// </summary>
        /// <param name="filePath">文件绝对路径</param>
        public static void OpenAndChooseFile(string filePath)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath))
                return;

            if (Directory.Exists(filePath))
                Process.Start(@"explorer.exe", "/select,\"" + filePath + "\"");
            else
            {
                IntPtr pidlList = ILCreateFromPathW(filePath);
                if (pidlList != IntPtr.Zero)
                {
                    try
                    {
                        Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
                    }
                    finally
                    {
                        ILFree(pidlList);
                    }
                }
            }
        }

        /// <summary>
        /// 追加内容到文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        public static void AppendLine(string filePath, string content)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Append))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine(content);
            }
        }

        /// <summary>
        /// 返回指定源的文件名
        /// </summary>
        /// <param name="fullPaths"></param>
        /// <returns></returns>
        public static string[] GetFileNameFromFullPath(string[] fullPaths)
        {
            var rData = new string[fullPaths.Length];
            for (var i = 0; i < fullPaths.Length; i++)
            {
                rData[i] = Path.GetFileName(fullPaths[i]);
            }
            return rData;
        }

        /// <summary>
        /// Stream转Byte数组
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] Stream2Bytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始 
            //stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// 根据后缀名判断是否为支持的媒体文件类型, 用于读取MTP缩略图
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static SyncContentType GetMediaTypeByName(string fileName)
        {
            if (Regex.IsMatch(fileName, @"(.*?)\.(mp3|aac|flac|wma)$", RegexOptions.IgnoreCase))
            {
                return SyncContentType.AUDIO;
            }
            else if (Regex.IsMatch(fileName, @"(.*?)\.(mp4|3gp|wmv)$", RegexOptions.IgnoreCase))
            {
                return SyncContentType.VIDEO;
            }
            else if (Regex.IsMatch(fileName, @"(.*?)\.(jpg|jpeg|png|bmp|gif)$", RegexOptions.IgnoreCase))
            {
                return SyncContentType.IMAGE;
            }
            else
            {
                return SyncContentType.OTHER;
            }
        }

        /// <summary>
        /// 读取音频文件的专辑图片
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>byte数组</returns>
        public static byte[] GetAudioFileAlbum(string filePath)
        {
            try
            {
                TagLib.File file = TagLib.File.Create(filePath);
                var firstPicture = file.Tag.Pictures.FirstOrDefault();
                if (firstPicture != null)
                {
                    return firstPicture.Data.Data;
                }
            }
            catch (Exception)
            { 
            }
            return null;
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
