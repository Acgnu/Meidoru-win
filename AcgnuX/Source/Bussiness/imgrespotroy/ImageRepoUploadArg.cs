using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Bussiness.imgrespotroy
{
    public class ImageRepoUploadArg
    {
        public string FullFilePath { get; set; }
        public JObject ExtraArgs { get; set; }
    }
}
