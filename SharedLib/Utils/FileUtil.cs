using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SharedLib.Utils
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
        /// 读取JSON内容到 JsonDocument
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        /// <returns>需要使用using</returns>
        public static async Task<JsonDocument> ParseJsonDocumentAsync(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return await JsonDocument.ParseAsync(fileStream);
            }
        }

        /// <summary>
        /// 读取JSON内容到 JsonNode
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        /// <returns></returns>
        public static async Task<JsonNode> ParseJsonNodeAsync(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return await JsonNode.ParseAsync(fileStream);
            }
        }

        /// <summary>
        /// 将实体序列化为JSON存储到文件
        /// </summary>
        /// <param name="data">目标数据</param>
        /// <param name="JsonFileFullPath">JSON完整保存目录</param>
        public static void SaveJsonToFile(object data, string JsonFileFullPath)
        {
            var s = JsonSerializer.Serialize(data);
            File.WriteAllText(JsonFileFullPath, s);
        }

        /// <summary>
        /// 创建 文件名.bak.后缀 备份
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>备份的文件名</returns>
        public static string Backup(string filePath)
        {
            var backupPath = GenerateBackupFilePath(filePath);
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
            File.Move(filePath, backupPath);
            return backupPath;
        }

        public static string GenerateBackupFilePath(string filePath)
        {
            var originFileName = Path.GetFileNameWithoutExtension(filePath);
            var originFileExtension = Path.GetExtension(filePath);
            var orginFileDir = Path.GetDirectoryName(filePath);
            var bakFileName = originFileName + ".bak" + originFileExtension;
            var bakFileFullPath = Path.Combine(orginFileDir, bakFileName);
            return bakFileFullPath;
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
        /// 获取文件MD5
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns></returns>
        public static string GetMD5(string fileFullPath)
        {
            using (HashAlgorithm hash = HashAlgorithm.Create())
            {
                using (FileStream file1 = new FileStream(fileFullPath, FileMode.Open))
                {
                    byte[] buffer = hash.ComputeHash(file1);
                    hash.Clear();
                    //将字节数组转换成十六进制的字符串形式
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        stringBuilder.Append(buffer[i].ToString("x2"));
                    }
                    return stringBuilder.ToString();
                }
            }
        }

        /// <summary>
        /// 从指定目录中获取图像文件, 优先从缓存中获取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="folder"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static List<string> GetImageFilesWithMemoryCache(string key, string folder, bool force)
        {
            if (!Directory.Exists(folder)) return new List<string>();

            var mc = MemoryCache.Default;
            var files = mc[key] as List<string>;
            if (null == files || force)
            {
                files = Directory.GetFiles(folder)
                        .Where(e => Path.GetExtension(e).Equals(".jpg") || Path.GetExtension(e).Equals(".png"))
                        .ToList();
                if (files.Count == 0) return new List<string>();
                mc[key] = files;
            }
            return files;
        }

        /// <summary>
        /// 获取皮肤目录中随机的一个图片文件
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static string GetRandomSkinFile(string folder)
        {
            var files = GetImageFilesWithMemoryCache("skinFolder", folder, false);
            if (files.Count == 0) return null;
            string resultFile;
            System.Drawing.Image drawingImg;
            do
            {
                resultFile = RandomUtil.GetRandomItem(files.ToArray());
                drawingImg = System.Drawing.Image.FromFile(resultFile);
                if (drawingImg.Height > drawingImg.Width)
                {
                    //只保留横屏图
                    files.Remove(resultFile);
                    continue;
                }
                break;
            } while (files.Count > 0);
            return resultFile;
        }
    }
}
