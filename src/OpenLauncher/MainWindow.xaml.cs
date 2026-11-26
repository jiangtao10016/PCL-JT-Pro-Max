using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using OpenLauncher.Core;
using OpenLauncher.UI;
using OpenLauncher.UI.Pages;

namespace OpenLauncher
{
    public partial class MainWindow : Window
    {
        public static MainWindow Current { get; private set; }

        readonly Dictionary<RadioButton, Stack<FrameworkElement>> _stacks = new Dictionary<RadioButton, Stack<FrameworkElement>>();
        RadioButton _tab;
        DispatcherTimer _toastTimer;

        public LaunchPage LaunchPage { get; private set; }
        public DownloadPage DownloadPage { get; private set; }
        public SettingsPage SettingsPage { get; private set; }
        public MorePage MorePage { get; private set; }

        public MainWindow()
        {
            Current = this;
            InitializeComponent();
            VersionTag.Text = "v" + AppInfo.VersionString;
            TaskList.ItemsSource = TaskManager.Tasks;
            TaskManager.Tasks.CollectionChanged += (s, e) => UpdateTaskBadge();
            TaskManager.Added += t => { t.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(LauncherTask.State)) Dispatcher.BeginInvoke((Action)UpdateTaskBadge); }; UpdateTaskBadge(); };
            TaskManager.Finished += OnTaskFinished;

            LaunchPage = new LaunchPage();
            DownloadPage = new DownloadPage();
            SettingsPage = new SettingsPage();
            MorePage = new MorePage();
            _stacks[NavLaunch] = new Stack<FrameworkElement>(new FrameworkElement[] { LaunchPage });
            _stacks[NavDownload] = new Stack<FrameworkElement>(new FrameworkElement[] { DownloadPage });
            _stacks[NavSettings] = new Stack<FrameworkElement>(new FrameworkElement[] { SettingsPage });
            _stacks[NavMore] = new Stack<FrameworkElement>(new FrameworkElement[] { MorePage });

            ApplyBackground();
            NavLaunch.IsChecked = true;
            OpenStartupPage();

            // 清爽实色背景（已移除系统亚克力玻璃效果）
            Background = (Brush)FindResource("Bg");
            Loaded += (s, e) =>
            {
                MoveNavPill(false);
                SizeChanged += (s2, e2) => MoveNavPill(false);
            };
#if DEBUG
            if (DemoCapture.Active)
            {
                ShowActivated = false;
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = -6000; Top = 100;
                Loaded += async (s, e) => await DemoCapture.RunAsync();
                return;
            }
#endif
            Loaded += async (s, e) => await StartupChecksAsync();
        }

        // ---------- 导航 ----------
        void Nav_Checked(object sender, RoutedEventArgs e)
        {
            _tab = (RadioButton)sender;
            MoveNavPill(true);
            Show(_stacks[_tab].Peek());
        }

        /// <summary>把滑动玻璃药丸移到当前导航项背后（CubicEase 缓动）</summary>
        void MoveNavPill(bool animate)
        {
            var rb = _tab;
            if (rb == null || NavBar == null || NavPill == null) return;
            try
            {
                var w = rb.ActualWidth;
                var h = rb.ActualHeight;
                if (w <= 0 || h <= 0) return;
                var pos = rb.TransformToAncestor(NavBar).Transform(new Point(0, 0));
                var margin = new Thickness(pos.X, 0, 0, 0);
                NavPill.Width = w;
                NavPill.Height = h;
                if (animate)
                {
                    var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
                    NavPill.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(140)));
                    NavPill.BeginAnimation(MarginProperty, new ThicknessAnimation(margin, TimeSpan.FromMilliseconds(240)) { EasingFunction = ease });
                }
                else
                {
                    NavPill.BeginAnimation(OpacityProperty, null);
                    NavPill.BeginAnimation(MarginProperty, null);
                    NavPill.Opacity = 1;
                    NavPill.Margin = margin;
                }
            }
            catch { }
        }

        void Show(FrameworkElement page)
        {
            PageHost.Content = page;
            page.Opacity = 0;
            var tt = new TranslateTransform(0, 10);
            page.RenderTransform = tt;
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            page.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease });
            tt.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(10, 0, TimeSpan.FromMilliseconds(220)) { EasingFunction = ease });
            (page as IPage)?.OnShown();
        }

        /// <summary>在当前标签页里打开子页面（有返回）</summary>
        public void Push(FrameworkElement page)
        {
            _stacks[_tab].Push(page);
            Show(page);
        }

        public void Back()
        {
            var st = _stacks[_tab];
            if (st.Count > 1) st.Pop();
            Show(st.Peek());
        }

        public void GoTab(string name, FrameworkElement sub = null)
        {
            var rb = name switch { "download" => NavDownload, "settings" => NavSettings, "more" => NavMore, _ => NavLaunch };
            var st = _stacks[rb];
            while (st.Count > 1) st.Pop();
            if (rb.IsChecked == true) Show(st.Peek()); else rb.IsChecked = true;
            if (sub != null) Push(sub);
        }

        /// <summary>命令行 --page download|settings|more|versions|mods|modsearch 直接打开某页（快捷方式 / 测试用）</summary>
        void OpenStartupPage()
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "--page");
            if (i < 0 || i + 1 >= args.Length) return;
            switch (args[i + 1])
            {
                case "download": GoTab("download"); break;
                case "settings": GoTab("settings"); break;
                case "more": GoTab("more"); break;
                case "versions": Push(new VersionsPage()); break;
                case "instance":
                    var inst = LaunchPage.SelectedInstance();
                    if (inst != null) Push(new InstancePage(inst, i + 2 < args.Length ? args[i + 2] : null));
                    break;
                case "modsearch":
                    GoTab("download");
                    DownloadPage.OpenContent(global::OpenLauncher.Content.ContentType.Mod, LaunchPage.SelectedInstance());
                    break;
            }
        }

        // ---------- 标题栏 ----------
        void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        void Max_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        void Close_Click(object sender, RoutedEventArgs e) => Close();

        void Window_StateChanged(object sender, EventArgs e)
        {
            // 无边框窗口最大化时会溢出屏幕边缘几个像素
            Root.Margin = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
            MaxButton.Content = WindowState == WindowState.Maximized ? "" : "";
        }

        // ---------- 任务 ----------
        void Tasks_Click(object sender, RoutedEventArgs e) => ToggleTasks(TaskLayer.Visibility != Visibility.Visible);
        void TaskLayer_MouseDown(object sender, MouseButtonEventArgs e) => ToggleTasks(false);
        void ClearTasks_Click(object sender, RoutedEventArgs e) => TaskManager.ClearFinished();
        void CancelTask_Click(object sender, RoutedEventArgs e) => ((sender as FrameworkElement)?.DataContext as LauncherTask)?.Cancel();

        public void ToggleTasks(bool show)
        {
            NoTasks.Visibility = TaskManager.Tasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (show)
            {
                TaskSlide.BeginAnimation(TranslateTransform.XProperty, null);
                TaskSlide.X = 360;
                TaskScrim.BeginAnimation(OpacityProperty, null);
                TaskScrim.Opacity = 0;
                TaskLayer.Visibility = Visibility.Visible;
                var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
                TaskSlide.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(360, 0, TimeSpan.FromMilliseconds(240)) { EasingFunction = ease });
                TaskScrim.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)));
            }
            else
            {
                var ease = new CubicEase { EasingMode = EasingMode.EaseIn };
                var anim = new DoubleAnimation(360, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease };
                anim.Completed += (s, e) => TaskLayer.Visibility = Visibility.Collapsed;
                TaskSlide.BeginAnimation(TranslateTransform.XProperty, anim);
                TaskScrim.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150)));
            }
        }

        void UpdateTaskBadge()
        {
            int running = TaskManager.Tasks.Count(t => t.IsActive);
            TaskCount.Text = running > 0 ? running.ToString() : "";
            NoTasks.Visibility = TaskManager.Tasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        void OnTaskFinished(LauncherTask t)
        {
            UpdateTaskBadge();
            if (t.Title.StartsWith("启动 ")) return; // 启动页自己会提示（窗口出现 / 失败对话框）
            if (t.State == TaskState.Done) Toast($"{t.Title}：完成", ToastKind.Ok);
            else if (t.State == TaskState.Failed) Toast($"{t.Title}：失败（点右上角任务查看原因）", ToastKind.Error);
        }

        // ---------- 提示 ----------
        public enum ToastKind { Info, Ok, Error }

        public void Toast(string text, ToastKind kind = ToastKind.Info)
        {
            ToastText.Text = text;
            ToastIcon.Text = kind == ToastKind.Ok ? "" : kind == ToastKind.Error ? "" : "";
            ToastBox.Visibility = Visibility.Visible;
            // 上滑 + 淡入
            ToastSlide.BeginAnimation(TranslateTransform.YProperty, null);
            ToastSlide.Y = 14;
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            ToastBox.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)));
            ToastSlide.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(14, 0, TimeSpan.FromMilliseconds(240)) { EasingFunction = ease });
            _toastTimer?.Stop();
            _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(kind == ToastKind.Error ? 5 : 3) };
            _toastTimer.Tick += (s, e) =>
            {
                _toastTimer.Stop();
                var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
                anim.Completed += (s2, e2) => ToastBox.Visibility = Visibility.Collapsed;
                ToastBox.BeginAnimation(OpacityProperty, anim);
                ToastSlide.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, 10, TimeSpan.FromMilliseconds(250)));
            };
            _toastTimer.Start();
        }

        // ---------- 对话框 ----------
        readonly Stack<object> _dialogStack = new Stack<object>();

        internal void ShowDialogContent(FrameworkElement content)
        {
            if (DialogHost.Content != null) _dialogStack.Push(DialogHost.Content);
            DialogHost.Content = content;
            DialogLayer.Visibility = Visibility.Visible;
            content.Opacity = 0;
            var st = new ScaleTransform(0.96, 0.96);
            content.RenderTransformOrigin = new Point(0.5, 0.5);
            content.RenderTransform = st;
            content.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(140)));
            st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.96, 1, TimeSpan.FromMilliseconds(160)));
            st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.96, 1, TimeSpan.FromMilliseconds(160)));
        }

        internal void CloseDialogContent(FrameworkElement content)
        {
            if (DialogHost.Content != content) return;
            if (_dialogStack.Count > 0) DialogHost.Content = _dialogStack.Pop();
            else { DialogHost.Content = null; DialogLayer.Visibility = Visibility.Collapsed; }
        }

        // ---------- 背景 ----------
        public void ApplyBackground()
        {
            var img = Theme.LoadImage(Settings.BackgroundImage, 1920);
            BgImage.Source = img;
            BgImage.Visibility = img == null ? Visibility.Collapsed : Visibility.Visible;
        }

        // ---------- 启动时 ----------
        async Task StartupChecksAsync()
        {
            _ = Task.Run(() => Mirrors.ProbeAsync());
            if (Environment.GetCommandLineArgs().Contains("--updated"))
                Toast($"已更新到 {AppInfo.VersionString}", ToastKind.Ok);

        }
    }

    /// <summary>页面显示时回调（刷新数据用）</summary>
    public interface IPage
    {
        void OnShown();
    }
}
