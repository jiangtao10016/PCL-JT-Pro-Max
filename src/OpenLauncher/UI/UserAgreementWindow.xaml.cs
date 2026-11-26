using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace OpenLauncher.UI
{
    /// <summary>
    /// 用户协议弹窗：5 秒内不可关闭（按钮禁用、点 X 无效），倒计时结束后才能「同意并继续」或「退出」。
    /// 协议正文见 EulaText.Full，其中 **…** 会被渲染为加粗（重点条款）。
    /// </summary>
    public partial class UserAgreementWindow : Window
    {
        const int ReadSeconds = 5;
        readonly DispatcherTimer _timer;
        int _remain;

        public UserAgreementWindow()
        {
            InitializeComponent();
            Doc.Document = BuildDocument(EulaText.Full);

            // 清爽实色背景（已移除系统亚克力玻璃效果）
            Background = (Brush)FindResource("Bg");

            _remain = ReadSeconds;
            Hint.Text = $"请阅读协议（剩余 {_remain}s）";
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTick;
            _timer.Start();
        }

        void OnTick(object sender, EventArgs e)
        {
            _remain--;
            if (_remain <= 0)
            {
                _timer.Stop();
                Hint.Text = "感谢阅读，您可点击「同意并继续」或「退出」。";
                AgreeBtn.IsEnabled = true;
                ExitBtn.IsEnabled = true;
            }
            else
            {
                Hint.Text = $"请阅读协议（剩余 {_remain}s）";
            }
        }

        void AgreeBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // 5 秒内禁止通过右上角 X 关闭，强制阅读
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_timer != null && _timer.IsEnabled)
            {
                e.Cancel = true;
                return;
            }
            base.OnClosing(e);
        }

        static FlowDocument BuildDocument(string text)
        {
            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Microsoft YaHei UI, Microsoft YaHei, SimSun"),
                FontSize = 13,
                PagePadding = new Thickness(2),
            };
            var re = new Regex(@"\*\*(.+?)\*\*");
            foreach (var raw in text.Replace("\r\n", "\n").Split('\n'))
            {
                var line = raw;
                var p = new Paragraph { Margin = new Thickness(0, 0, 0, 6) };
                if (string.IsNullOrWhiteSpace(line))
                {
                    p.Margin = new Thickness(0);
                    doc.Blocks.Add(p);
                    continue;
                }
                int last = 0;
                foreach (Match m in re.Matches(line))
                {
                    if (m.Index > last)
                        p.Inlines.Add(new Run(line.Substring(last, m.Index - last)));
                    p.Inlines.Add(new Bold(new Run(m.Groups[1].Value)));
                    last = m.Index + m.Length;
                }
                if (last < line.Length)
                    p.Inlines.Add(new Run(line.Substring(last)));
                doc.Blocks.Add(p);
            }
            return doc;
        }
    }
}
