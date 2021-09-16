using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Bussiness.imgrespotroy
{
    public class ImageRepoFactory
    {
        private static List<IImageRepo> mImageRepoApiList = Assembly
                        .GetExecutingAssembly()
                        .GetTypes()
                        .Where(item => item.GetInterfaces().Contains(typeof(IImageRepo)))
                        .Select(type => (IImageRepo)Activator.CreateInstance(type))
                        .ToList();

        /// <summary>
        /// 获取一个随机的图床API
        /// </summary>
        /// <returns></returns>
        public static IImageRepo GetRandomApi()
        {
            var randomIndex = RandomUtil.GetRangeRandomNum(0, mImageRepoApiList.Count);
            return mImageRepoApiList[randomIndex];
        }

        /// <summary>
        /// 根据类型获取指定的图床API
        /// </summary>
        /// <param name="repoClassType"></param>
        /// <returns></returns>
        //public static IImageRepo GetApiByType(Type repoClassType)
        //{
        //    return mImageRepoApiList.Where(e => e.GetType().Equals(repoClassType)).FirstOrDefault();
        //}
    }
}
