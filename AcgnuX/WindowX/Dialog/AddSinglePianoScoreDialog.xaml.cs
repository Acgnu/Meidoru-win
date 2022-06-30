using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Model;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 添加弹琴吧曲谱窗口
    /// </summary>
    public partial class AddSinglePianoScoreDialog : BaseDialog
    {
        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        public event EditConfirmHandler<PianoScore> editConfirmHnadler;

        public AddSinglePianoScoreDialog(PianoScore pianoScore)
        {
            InitializeComponent();
            if (null != pianoScore)
            {
                TextBoxMsuicId.Text = Convert.ToString(pianoScore.id);
                //ID不能修改, 禁用输入框
                TextBoxMsuicId.IsEnabled = false;
                //禁用复选框
                AutoDownloadCheckBox.IsEnabled = false;
                TextBoxName.Text = pianoScore.Name;
            }
            DataContext = this;
        }

        /// <summary>
        /// 点击确定事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            //先将按钮禁用, 避免重复点击
            button.IsEnabled = false;

            var pianoScore = GetEditPianoScore();
            //执行任务
            var result = editConfirmHnadler?.Invoke(pianoScore);
            //更新按钮状态
            button.IsEnabled = true;
            if (result.success)
            {
                DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// 获取编辑中的琴谱对象
        /// </summary>
        /// <returns>琴谱对象</returns>
        private PianoScore GetEditPianoScore()
        {
            //如果必要数据不存在, 返回null
            var ypid = TextBoxMsuicId.Text;
            if (string.IsNullOrEmpty(ypid)) return null;
            //返回乐谱信息
            return new PianoScore()
            {
                id = Convert.ToInt32(ypid),
                Name = TextBoxName.Text,
                AutoDownload = AutoDownloadCheckBox.IsChecked.GetValueOrDefault(),
                UseProxy = UseProxyCheckBox.IsChecked.GetValueOrDefault()
            };
        }

        /// <summary>
        /// 复选框选中状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCheckboxChange(object sender, RoutedEventArgs e)
        {
            var ckBox = sender as CheckBox;
            //自动下载切换
            if(ckBox.Name == AutoDownloadCheckBox.Name)
            {
                //更改名称输入框禁用状态
                TextBoxName.IsEnabled = !ckBox.IsChecked.GetValueOrDefault();
            }
            //启用代理切换
            if(ckBox.Name == UseProxyCheckBox.Name)
            {
                //Do Nothing
            }
        }
    }
}