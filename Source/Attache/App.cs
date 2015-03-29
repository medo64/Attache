using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace Attache {
    internal static class App {

        private static readonly bool IsRunningOnMono = (Type.GetType("Mono.Runtime") != null);

        [STAThread]
        static void Main() {
            bool createdNew;
            var mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));
            using (var setupMutex = new Mutex(false, @"Global\JosipMedved_Attache", out createdNew, mutexSecurity)) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Medo.Application.UnhandledCatch.Attach();
                Medo.Application.UnhandledCatch.ThreadException += new EventHandler<ThreadExceptionEventArgs>(UnhandledCatch_ThreadException);

                bool runInteractive = (Medo.Application.Args.Current.ContainsKey("Interactive")) || IsRunningOnMono;

                if (runInteractive) { //start service threads

                    if (!IsRunningOnMono) { Tray.Show(); }
                    AppServiceThread.Start();
                    Application.Run();
                    AppServiceThread.Stop();
                    if (!IsRunningOnMono) { Tray.Hide(); }

                } else if (Medo.Application.Args.Current.ContainsKey("Install")) {

                    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                    Environment.Exit(0);

                } else if (Medo.Application.Args.Current.ContainsKey("Uninstall")) {

                    try {
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                        Environment.Exit(0);
                    } catch (InstallException) { //no service with that name
                        Environment.Exit(-1);
                    }

                } else {

                    if (Environment.UserInteractive) { //show tray icon
                        Tray.Show();
                        Application.Run();
                        Tray.Hide();
                    } else {
                        ServiceBase.Run(new ServiceBase[] { AppService.Instance });
                    }

                }
            }
        }

        public static void Terminate(string format, params object[] args) {
            var text = string.Format(CultureInfo.InvariantCulture, format, args);
            Trace.WriteLine(text);

            App.Exit(1066, text); //ERROR_SERVICE_SPECIFIC_ERROR
        }


        private static void UnhandledCatch_ThreadException(object sender, ThreadExceptionEventArgs e) {
            var ex = e.Exception as Exception;
            if (ex != null) {
                Trace.WriteLine("E: " + ex.GetType().Name + ": " + ex.Message);
                Trace.WriteLine("E: " + ex.StackTrace);
            }

#if !DEBUG
            Medo.Diagnostics.ErrorReport.SaveToTemp(ex);

            App.Exit(1064, ex.Message); //ERROR_EXCEPTION_IN_SERVICE
#else
            throw e.Exception;
#endif
        }

        public static void Log(EventLogEntryType type, string format, params object[] args) {
            var text = string.Format(CultureInfo.InvariantCulture, format, args);
            Trace.WriteLine(type.ToString().Substring(0, 1) + ": " + text);

            var source = AppService.Instance.ServiceName;
            if (!EventLog.SourceExists(source)) { EventLog.CreateEventSource(source, "Application"); }
            EventLog.WriteEntry(source, text, type);
        }

        private static void Exit(int exitCode, string message) {
            if (Tray.IsVisible) { Tray.Hide(); }
            if (message == null) { message = "Exit due to an error."; }

            App.Log(EventLogEntryType.Error, message);

            AppService.Instance.ExitCode = exitCode;
            AppService.Instance.AutoLog = false;

            Environment.Exit(exitCode);
        }

    }
}
