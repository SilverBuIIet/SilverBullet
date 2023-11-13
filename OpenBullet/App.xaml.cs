﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using OpenBullet.CefBrowser;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Define how to handle unhandled exception
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                OnUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            Dispatcher.UnhandledException += (s, e) =>
                OnUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            Application.Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            //AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            //  OnUnhandledException(e.Exception, "AppDomain.CurrentDomain.FirstChanceException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                OnUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            OnUnhandledException(e.Exception, "Dispatcher_UnhandledException");
        }

        public void OnUnhandledException(Exception ex, string @event)
        {
            File.AppendAllText(SB.logFile, $"[FATAL][{@event}] UHANDLED EXCEPTION{Environment.NewLine}{ex}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool InitializeCefSharp(ChromiumWebBrowser browser)
        {
            try
            {
                var settings = new CefSettings()
                {
                    IgnoreCertificateErrors = SB.Settings.RLSettings.CefSharp.IgnoreCertificateErrors,
                    MultiThreadedMessageLoop = true,
                    UserAgent = SB.Settings.RLSettings.CefSharp.UserAgent,
                    PackLoadingDisabled = SB.Settings.RLSettings.CefSharp.PackLoadingDisabled,
                    BackgroundColor = Utils.GetBrush("BackgroundMain").Color.ColorToUInt()
                };

                // Set BrowserSubProcessPath based on app bitness at runtime
                settings.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                       Environment.Is64BitProcess ? "x64" : "x86",
                                                       "CefSharp.BrowserSubprocess.exe");

                // Make sure you set performDependencyCheck false
                var initialized = Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);

                if (initialized && browser != null)
                {
                    //Set up the new handler instance
                    browser.BrowserSettings.Javascript = CefState.Enabled;
                    browser.BrowserSettings.WebSecurity = CefState.Disabled;
                    browser.MenuHandler = new CustomMenuHandler();
                }
                return initialized;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Will attempt to load missing assembly from either x86 or x64 subdir
        public Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                       Environment.Is64BitProcess ? "x64" : "x86",
                                                       assemblyName);

                return File.Exists(archSpecificPath)
                           ? Assembly.LoadFile(archSpecificPath)
                           : null;
            }
            return null;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try { Cef.Shutdown(); } catch { }
            base.OnExit(e);
        }
    }
}
