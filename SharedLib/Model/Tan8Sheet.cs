﻿namespace SharedLib.Model
{
    /// <summary>
    /// 乐谱数据库对象
    /// </summary>
    public class Tan8Sheet
    {
        // 对应弹琴吧的乐谱id
        public int Ypid { get; set; }
        // 乐谱名称
        public string Name { get; set; }
        // 喜好评级 1 - 5
        public byte Star { get; set; }
        public byte YpCount { get; set; }
        //版本 1, 2, 3
        public byte Ver { get; set; }
        //是否热门
        public byte Hot { get ; set ; }
    }
}
