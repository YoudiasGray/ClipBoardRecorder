using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSharp_ClipBoardRecorder
{


    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //参考：
        //https://blog.csdn.net/shalyf/article/details/8854591  消息处理
        string FileName = "";  //log文件名
        FileStream FileReader = null; //为了防止出现意外，在设置选择文件的时候就打开占用log文件        
        string PreContent = "";  //用作优化，如果和上一次一样，则不记录        
        bool StartRecordFlag = false;
        string NextLine = "\n";
        //初始加载窗体，这个时候只有Set/Exit 按钮可用
        public MainWindow()
        {
            InitializeComponent();            
            this.Start.IsEnabled = false;
            this.Stop.IsEnabled = false;
            //string abc = "测试中文字符";
            //this.Content.Content = abc;
        }

        //设置log文件，如果没有选择合适的文件询问是否重新选择或者退出
        private void Set_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "请选择文件：";
            dialog.Filter = "文本文件(*.log;*.txt)| *.log;*.txt"; //log文件后缀可选 log/txt
            try
            {
                if (dialog.ShowDialog() == true)
                {
                    if (this.FileReader != null)
                    {
                        this.FileReader.Close(); //如果打开过文件，解除对上次选择文件的占用，用于重新设置log文件
                    }
                    this.FileName = dialog.FileName;
                    this.FileReader = new FileStream(this.FileName, FileMode.Append, FileAccess.Write);
                    //如果设定了log文件，start可选
                    this.Start.IsEnabled = true;
                    this.FileLabel.Content = this.FileName;
                }
                else
                {
                    MessageBoxResult mr = System.Windows.MessageBox.Show("未选择合适文件，请重新选择或退出！\nYes 重新选择，No 退出", "Warring", MessageBoxButton.YesNo);
                    if (mr == MessageBoxResult.No)
                    {
                        Environment.Exit(0);
                    }
                    this.Set_Click(sender, e);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }


        //开始按钮
        //按下开始按钮后，就只有stop按钮可用
        //开始监听clipboard，并将数据格式化后存储到log文件中
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            this.Start.IsEnabled = false;
            this.Exit.IsEnabled = false;
            this.Stop.IsEnabled = true;
            this.Set.IsEnabled = false;
            this.StartRecordFlag = true;
        }
   
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            //点击stop后停止记录，此时只有start exit set可选
            this.Stop.IsEnabled = false;
            this.Start.IsEnabled = true;
            this.Exit.IsEnabled = true;
            this.Set.IsEnabled = true;
            this.StartRecordFlag = false;

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (this.FileReader != null)
            {
                this.FileReader.Close();
            }
            //RemoveClipboardFormatListener(this.CurrentWindowHander);
            Environment.Exit(0);
        }

 
        private string GetStringData()
        {
            string ret = "";
            System.Windows.IDataObject iData = System.Windows.Clipboard.GetDataObject();
            if (iData.GetDataPresent(System.Windows.DataFormats.Text))
            {
                ret = (string)iData.GetData(System.Windows.DataFormats.Text);
            }
            return ret;
        }
        //复制内容格式化，格式为
        /*
        【时间】        
        XXXX
         */

        private string FormatString(string info)
        {
            string ret = "";
            DateTime current = System.DateTime.Now;
            ret ="【"+ current.Year.ToString("d4") + "-" + current.Month.ToString("d2") + "-" + current.Day.ToString("d2") + " " + current.Hour.ToString("d2") + ":" + current.Minute.ToString("d2") + ":" + current.Second.ToString("d2")+"】"+this.NextLine;
            ret += info+this.NextLine + this.NextLine;
            return ret;
        }
        string GetTimeString()
        {
            string time="";

            return time;
        }
        private void ShowInText(string content)
        {
            this.Content.Content = content;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {

            base.OnSourceInitialized(e);
            HwndSource hs = PresentationSource.FromVisual(this) as HwndSource;            
            hs.AddHook(WndProc);            
        }
        //https://blog.csdn.net/xlm289348/article/details/8050957
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                string tmp = GetStringData();
                if (tmp != this.PreContent && this.StartRecordFlag == true)
                //if (tmp != this.PreContent)
                {
                    this.PreContent = tmp;
                    tmp = FormatString(tmp);
                    this.ShowInText(tmp);
                    //https://blog.csdn.net/zhuyu19911016520/article/details/46502857
                    StreamWriter tmpSW = new StreamWriter(this.FileReader,Encoding.UTF8);                    
                    tmpSW.Write(tmp);
                    tmpSW.Flush();  //测试过直接点X关闭，数据也能写入文件                    
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                
            }
            return IntPtr.Zero;
        }
    }
}
