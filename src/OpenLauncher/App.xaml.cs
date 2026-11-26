using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using OpenLauncher.Accounts;
using OpenLauncher.Core;
using OpenLauncher.UI;

namespace OpenLauncher
{
    public partial class App : Application
    {
        static App()
        {
            // 与 App.config 相同的长路径开关；写在代码里，单独拷走 exe 也生效
            AppContext.SetSwitch("Switch.System.IO.UseLegacyPathHandling", false);
            AppContext.SetSwitch("Switch.System.IO.BlockLongPaths", false);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += OnUiException;
            AppDomain.CurrentDomain.UnhandledException += (s, ex) => Log.Error("未处理的异常", ex.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (s, ex) => { Log.Warn("未观察的任务异常：" + ex.Exception?.GetBaseException().Message); ex.SetObserved(); };

            TaskManager.Dispatcher = Dispatcher;
            Log.Info($"{Branding.AppName} {AppInfo.VersionString} 启动，系统 {Util.OsVersion} {Util.OsArch}，数据目录 {Paths.DataDir}");
            Settings.Load();
            try { AccountStore.Load(); } catch (Exception ex) { Log.Error("读取账号失败", ex); }
            Theme.Apply(Settings.ThemeColor);
#if DEBUG
            DemoCapture.Init(e.Args);
#endif

            // 用户协议：每次启动都强制弹出，读完 5 秒后才能「同意并继续」或「退出」
            // 关键：协议窗是启动时唯一的窗口，默认 OnLastWindowClose 会在它关闭时直接退出应用，
            // 必须临时切为 OnExplicitShutdown，同意后再恢复，否则主窗口永远起不来。
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var eula = new UserAgreementWindow();
            if (eula.ShowDialog() != true)
            {
                Shutdown();
                return;
            }
            ShutdownMode = ShutdownMode.OnLastWindowClose;

            var win = new MainWindow();
            MainWindow = win;
            win.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.SaveNow();
            base.OnExit(e);
        }

        void OnUiException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error("界面异常", e.Exception);
            e.Handled = true;
            try { Dialogs.Alert("出错了", e.Exception.GetBaseException().Message + "\n\n详细信息已写入启动器日志。"); }
            catch { MessageBox.Show(e.Exception.ToString(), Branding.AppName); }
        }
    }
}
