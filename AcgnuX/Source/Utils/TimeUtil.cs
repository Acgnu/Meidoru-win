using System;

namespace AcgnuX.Utils
{
    class TimeUtil
    {
        public static long CurrentMillis()
        {
            DateTime timeStampStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((DateTime.Now.ToUniversalTime() - timeStampStartTime).TotalMilliseconds);

            //TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            //return Convert.ToInt64(ts.TotalMilliseconds);
            //return 1570586499L * 1000;
        }

        public static DateTime LongTimeStampToDateTime(long longTimeStamp)
        {
            DateTime timeStampStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return timeStampStartTime.AddMilliseconds(longTimeStamp).ToLocalTime();
        }

        /// <summary>
        /// 返回d1 - d2 经过的毫秒数
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        public static long PassedTimeMillis(DateTime d1, DateTime d2)
        {
            return Convert.ToInt64((d1.ToUniversalTime() - d2).TotalMilliseconds);
        }
    }
}
