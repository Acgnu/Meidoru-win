using AcgnuX.Properties;
using AcgnuX.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedLib.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AcgnuX.ViewModel
{
    /// <summary>
    /// 密码库账号模型
    /// </summary>
    public class AccountViewModel : ObservableObject
    {
        public long? Id { get; set; }

        private string site;
        public string Site { get => site; set => SetProperty(ref site, value); }

        private string describe;
        public string Describe { get => describe; set => SetProperty(ref describe, value); }

        private string uname;
        public string Uname { get => uname; set => SetProperty(ref uname, value); }

        private string upass;
        public string Upass { get => upass; set => SetProperty(ref upass, value); }

        private string remark;
        public string Remark { get => remark; set => SetProperty(ref remark, value); }

        /// <summary>
        /// 保存项目
        /// </summary>
        /// <param name="account"></param>
        internal async Task SaveAsync()
        {
            var node = await FileUtil.ParseJsonNodeAsync(Settings.Default.AccountFilePath);
            var editNode = new JsonObject
            {
                ["Site"] = Site,
                ["Describe"] = Describe,
                ["Uname"] = Uname,
                ["Upass"] = Upass,
                ["Remark"] = Remark
            };
            if (null == Id) 
            {
                //新增
                Id = TimeUtil.CurrentMillis();
                editNode["Id"] = Id.GetValueOrDefault();
                var array = node.Root.AsArray();
                array.Insert(0, editNode);
            }
            else
            {
                //修改
                editNode["Id"] = Id.GetValueOrDefault();
                var nodeArray = node.Root.AsArray();
                for (var i = 0; i < nodeArray.Count; i++)
                {
                    if(nodeArray[i]["Id"].GetValue<long>().Equals(Id.GetValueOrDefault()))
                    {
                        nodeArray.RemoveAt(i);
                        nodeArray.Insert(i, editNode);
                        break;
                    }
                }
            }
            FileUtil.Backup(Settings.Default.AccountFilePath);
            using (var fileStream = File.OpenWrite(Settings.Default.AccountFilePath))
            {
                var writer = new Utf8JsonWriter(fileStream, new JsonWriterOptions()
                {
                    Indented = true
                });
                node.WriteTo(writer);
                await writer.FlushAsync();
            }
        }
    }
}
