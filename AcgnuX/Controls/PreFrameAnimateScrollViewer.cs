using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace AcgnuX.Controls
{
    internal class PreFrameAnimateScrollViewer : ScrollViewer
    {
        //需要滚动的量
        private double mTargetOffset = 0;
        //滚动速度增量
        private double mScrollVelocity;
        //标记最后一次设置滚动的量, 因为小数问题可能除不尽, 会一直触发, 使用此标记当数量不变的时候就不触发了, 节约性能
        private double mLastScrollValue;
        //标记当前滚动事件是否由本Smoother触发, 如果不是, 则不引发FrameUpdate
        private bool mIsScrollByThis = false;
        //自定义的滚轮滚动量
        private readonly double mMouseWheelDelta = 60;
        //滚动力
        private readonly double mScrollForce = 0.015;
        private readonly double mFact = 0.6;


        public PreFrameAnimateScrollViewer()
        {
            this.PreviewMouseWheel += OnPreviewMouseWheel;
            this.ScrollChanged += OnScrollChanged;
            CompositionTarget.Rendering += FrameUpdate;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!mIsScrollByThis)
            {
                mTargetOffset = this.ContentVerticalOffset;
            }
            mIsScrollByThis = false;
        }

        /// <summary>
        /// 每帧触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrameUpdate(object sender, EventArgs e)
        {
            var currentOffset = this.ContentVerticalOffset;

            var distanceOffset = mTargetOffset - currentOffset;

            if (distanceOffset == 0) return;

            mScrollVelocity += distanceOffset * mScrollForce; //滚动速度=力 * 距离

            mScrollVelocity *= mFact;    //降低速度增加稳定性

            //mScrollVelocity = Math.Ceiling(mScrollVelocity * 1000) / 1000;        //保留4小数精度

            currentOffset += mScrollVelocity; //速度 + 当前偏移量

            currentOffset = Math.Ceiling(currentOffset * 10000) / 10000;        //保留5小数精度

            if (mLastScrollValue == currentOffset) return;

            mIsScrollByThis = true;
            this.ScrollToVerticalOffset(currentOffset);  //滚动到指定位置
            mLastScrollValue = currentOffset;
        }

        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var lDelta = e.Delta > 0 ? mMouseWheelDelta : -mMouseWheelDelta;
            var temp = mTargetOffset - lDelta;
            if (temp > this.ScrollableHeight)
            {
                mTargetOffset = this.ScrollableHeight;
            }
            else
            {
                mTargetOffset = temp < 0 ? 0 : temp;
            }
            e.Handled = true;
        }
    }
}
