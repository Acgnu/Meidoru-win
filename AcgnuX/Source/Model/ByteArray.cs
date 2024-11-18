namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 字节数组包装
    /// </summary>
    public class ByteArray
    {
        /// <summary>
        /// 字节数组数据
        /// </summary>
        public byte[] Data { get; set; }


        public ByteArray(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// true 空的
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return null == Data || Data.Count() == 0;
        }
    }
}
