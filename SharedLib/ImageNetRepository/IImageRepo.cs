using SharedLib.Model;

namespace SharedLib.ImageNetRepository
{
    public interface IImageRepo
    {
        InvokeResult<ImageRepoUploadResult> Upload(ImageRepoUploadArg arg);

        string GetApiCode();
    }
}
