using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MD5Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region 类成员
        /// <summary>
        /// MD5结果
        /// </summary>
        string result = "";
        /// <summary>
        /// 帮助信息
        /// </summary>
        static readonly string helpinfo =
            "本程序用于计算MD5，计算结果是32位十六进制值。\r\n" +
            "==========================================\r\n" +
            "1. 计算字符串的MD5\r\n" +
            "\t请将需要计算的字符串输入或者粘贴到分割线上方的文本框中，\r\n" +
            "\t按下<回车>键，程序会将计算结果显示在分割线下方的文本框中。\r\n" +
            "\t注意：空格和空字符都可以计算MD5！\r\n" +
            "2. 计算文件的MD5\r\n" +
            "\t将文件拖入程序界面中，\r\n" +
            "\t或者在程序界面上双击 => 在弹出的窗口中选择文件\r\n" +
            "\t程序会将计算结果显示在分割线下方的文本框中。\r\n" +
            "++++++++++++++++++++++++++++++++++++++++++\r\n" +
            "快捷键说明：\r\n" +
            "\t<F1>\t\t显示本帮助信息。\r\n" +
            "\t<Esc>\t\t退出帮助页面，显示计算结果。\r\n" +
            "\t<Ctrl+Shift+C>\t清除计算结果。\r\n" +
            "\t<Ctrl+A>\t全选。\r\n" +
            "\t<Ctrl+C>\t复制。\r\n" +
            "\t<Ctrl+V>\t粘贴。\r\n";
        /// <summary>
        /// 当前显示的页面
        /// </summary>
        SHOW curshow = SHOW.HELPINFO;
        #endregion 类成员结束

        public MainWindow()
        {
            InitializeComponent();

            txtinfo.Text = helpinfo;

            #region 快捷键绑定
            CommandBindings.Add(new CommandBinding(
                CustomCommands.Clear,
                (sender, e) => { MenuItem_Click(sender, e); },
                (sender, e) => { e.CanExecute = result.Length > 0 && curshow.Equals(SHOW.RESULT); }));

            CommandBindings.Add(new CommandBinding(
                CustomCommands.Exit,
                (sender, e) => { txtinfo.Text = result; curshow = SHOW.RESULT; },
                (sender, e) => { e.CanExecute = curshow.Equals(SHOW.HELPINFO); }));

            CommandBindings.Add(new CommandBinding(
                ApplicationCommands.Help,
                (sender, e) => { txtinfo.Text = helpinfo; curshow = SHOW.HELPINFO; },
                (sender, e) => { e.CanExecute = curshow.Equals(SHOW.RESULT); }));
            #endregion 快捷键绑定结束
        }

        #region 事件处理
        /// <summary>
        /// 使TextBox可以拖拽
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 响应拖拽事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Drop(object sender, DragEventArgs e)
        {
            string filepath = "";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                filepath = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            }

            GetMD5FromFile(filepath);
        }

        /// <summary>
        /// 响应双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ofd.RestoreDirectory = true;
                ofd.ShowDialog();

                GetMD5FromFile(ofd.FileName);
            }
        }

        /// <summary>
        /// 响应回车事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtstr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                GetMD5FromString(txtstr.Text);
            }
        }

        /// <summary>
        /// 响应右键菜单=>清除结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            result = "";
            txtinfo.Text = result;
            curshow = SHOW.RESULT;
        }
        #endregion 事件处理结束

        #region 算法相关
        /// <summary>
        /// 将字节数组转换为十六进制字符串
        /// </summary>
        /// <param name="hashvalue"></param>
        /// <returns></returns>
        private string GetHexString(byte[] hashvalue)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashvalue)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 计算字符串MD5
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string GetMD5HashFromString(string s)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5hash = md5.ComputeHash(Encoding.UTF8.GetBytes(s));

            return GetHexString(md5hash);
        }

        /// <summary>
        /// 计算文件MD5
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetMD5HashFromFile(string filename)
        {
            FileStream fstream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5hash = md5.ComputeHash(fstream);
            fstream.Close();

            return GetHexString(md5hash);
        }
        #endregion 算法相关结束

        #region 计算与显示
        /// <summary>
        /// 计算并显示文件MD5
        /// </summary>
        /// <param name="filepath"></param>
        private void GetMD5FromFile(string filepath)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                if (File.Exists(filepath))
                {
                    result += filepath + "\t";
                    result += GetMD5HashFromFile(filepath) + "\r\n";
                    txtinfo.Text = result;
                    curshow = SHOW.RESULT;
                }
                else
                {
                    MessageBox.Show("找不到指定文件：\r\n" + filepath);
                }
            }
        }

        /// <summary>
        /// 计算并显示字符串MD5
        /// </summary>
        /// <param name="s"></param>
        private void GetMD5FromString(string s)
        {
            result += s + "\t";
            result += GetMD5HashFromString(s) + "\r\n";
            txtinfo.Text = result;
            curshow = SHOW.RESULT;
        }
        #endregion 计算与显示结束

    }

    #region 自定义命令类
    /// <summary>
    /// 自定义命令
    /// </summary>
    public static class CustomCommands
    {
        private static RoutedUICommand _clear;
        private static RoutedUICommand _exit;
        public static RoutedUICommand Clear
        {
            get
            {
                if (_clear == null)
                {
                    _clear = new RoutedUICommand(
                        "清除结果",
                        "clear",
                        typeof(MainWindow),
                        new InputGestureCollection { new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift) });
                }

                return _clear;
            }
        }

        public static RoutedUICommand Exit
        {
            get
            {
                if (_exit == null)
                {
                    _exit = new RoutedUICommand(
                        "退出帮助",
                        "exit",
                        typeof(MainWindow),
                        new InputGestureCollection { new KeyGesture(Key.Escape) });
                }

                return _exit;
            }
        }
    }
    #endregion 自定义命令类结束

    /// <summary>
    /// 显示界面枚举
    /// </summary>
    enum SHOW
    {
        RESULT = 0,
        HELPINFO = 1
    };

}
