using AcgnuX.Source.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Bussiness.imgrespotroy
{
    public interface IImageRepo
    {
        string GetApiCode();

        InvokeResult<ImageRepoUploadResult> Upload(ImageRepoUploadArg arg);
    }
}
