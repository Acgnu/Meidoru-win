namespace SharedLib.ImageNetRepository
{
    public class ImageRepoUploadArg
    {
        public string FullFilePath { get; set; }
        public Dictionary<string, string> ExtraArgs { get; set; }
    }
}
