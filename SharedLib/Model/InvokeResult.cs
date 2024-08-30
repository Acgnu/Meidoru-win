namespace SharedLib.Model
{
    /// <summary>
    /// 函数处理结果对象
    /// </summary>
    /// <typeparam name="T">返回的数据</typeparam>
    public class InvokeResult<T>
    {
        /// <summary>
        /// 是否以预期的结果关闭
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// 结果码
        /// </summary>
        public byte code { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 返回的数据
        /// </summary>
        public T data { get; set; }
    }
}
