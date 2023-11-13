﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using OpenBullet.Plugins;
using OpenBullet.ViewModels;
using OpenBullet.Views.Main;
using OpenBullet.Views.Main.Runner;
using OpenBullet.Views.Main.Settings;
using OpenBullet.Views.StackerBlocks;
using OpenBullet.Views.UserControls;
using PluginFramework;
using RuriLib;
using RuriLib.LS;
using RuriLib.ViewModels;
using Color = System.Windows.Media.Color;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random rand = new Random();
        private int snowBuffer = 0;

        public RunnerManager RunnerManagerPage { get; set; }
        // TODO: Do not create a different View for each RunnerInstance, but instead just replace the vm!
        public Runner CurrentRunnerPage { get; set; }
        public ProxyManager ProxyManagerPage { get; set; }
        public WordlistManager WordlistManagerPage { get; set; }
        public ConfigsSection ConfigsPage { get; set; }
        public HitsDB HitsDBPage { get; set; }
        public Settings OBSettingsPage { get; set; }
        public ToolsSection ToolsPage { get; set; }
        public PluginsSection PluginsPage { get; set; }
        public Help AboutPage { get; set; }
        public Rectangle Bounds { get; private set; }

        public SilverZone SilverZonePage { get; private set; }

        System.Windows.Point _startPosition;
        bool _isResizing = false,
            _canQuit;

        Timer silverZoneTimer;

        public MainWindow()
        {
            SB.MainWindow = this;

            // Clean or create log file
            File.WriteAllText(SB.logFile, "");

            InitializeComponent();

            var title = $"SilverBullet {SB.Version}";
            Title = title;
            titleLabel.Content = title;
            try
            {
                Task.Run(() =>
            {
                var update = CheckUpdate.Run<LatestRelease>("https://api.github.com/repos/mohamm4dx/SilverBullet/releases/latest");
                Dispatcher.Invoke(() => updateButton.Visibility = update.Available ? Visibility.Visible : Visibility.Collapsed);
            });
            }
            catch { }
            // Make sure all folders are there or recreate them
            var folders = new string[] { "Captchas", "ChromeExtensions", "Configs", "DB", "Hits", "Plugins", "Screenshots", "Settings", "Sounds", "Wordlists", "Js", "Compiled" };
            foreach (var folder in folders.Select(f => Path.Combine(Directory.GetCurrentDirectory(), f)))
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }

            // Initialize Environment Settings
            try
            {
                SB.Settings.Environment = IOManager.ParseEnvironmentSettings(SB.envFile);
            }
            catch
            {
                SB.Logger.LogError(Components.Main,
                    "Could not find / parse the Environment Settings file. Please fix the issue and try again.", true);
                Environment.Exit(0);
            }

            if (SB.Settings.Environment.WordlistTypes.Count == 0 || SB.Settings.Environment.CustomKeychains.Count == 0)
            {
                SB.Logger.LogError(Components.Main,
                    "At least one WordlistType and one CustomKeychain must be defined in the Environment Settings file.", true);
                Environment.Exit(0);
            }

            try
            {
                var vcInstalledVersion = SilverBullet.RedistributableChecker.RedistributablePackage.GetInstalledVersion();

                if (vcInstalledVersion.Length == 0)
                {
                    SB.Logger.LogError(Components.Main, "Visual C++ Redistributable 2015+ Not Installed\nInstall vc_redist.x64 AND vc_redist.x86\nDl Link: microsoft.com/en-us/download/details.aspx?id=48145\nGo to Help tab and then to Requirements tab", true);
                }

                else if (!vcInstalledVersion.Any(v => v.Contains("x86")))
                {
                    SB.Logger.LogError(Components.Main, "Visual C++ Redistributable 2015+ (x86) Not Installed\nDl Link: microsoft.com/en-us/download/details.aspx?id=48145\nGo to Help tab and then to Requirements tab", true);
                }
                else if (!vcInstalledVersion.Any(v => v.Contains("x64")))
                {
                    SB.Logger.LogError(Components.Main, "Visual C++ Redistributable 2015+ (x64) Not Installed\nDl Link: microsoft.com/en-us/download/details.aspx?id=48145\nGo to Help tab and then to Requirements tab", true);
                }
            }
            catch
            {
                SB.Logger.LogError(Components.Main, "Error on check Visual C++ is installed");
            }

            // Initialize Settings
            SB.Settings.RLSettings = new RLSettingsViewModel();
            SB.Settings.ProxyManagerSettings = new ProxyManagerSettings();
            SB.SBSettings = new SBSettingsViewModel();

            // Create / Load Settings
            if (!File.Exists(SB.rlSettingsFile))
            {
                MessageBox.Show("RuriLib Settings file not found, generating a default one");
                SB.Logger.LogWarning(Components.Main, "RuriLib Settings file not found, generating a default one");
                IOManager.SaveSettings(SB.rlSettingsFile, SB.Settings.RLSettings);
                SB.Logger.LogInfo(Components.Main, $"Created the default RuriLib Settings file {SB.rlSettingsFile}");
            }
            else
            {
                SB.Settings.RLSettings = IOManager.LoadSettings<RLSettingsViewModel>(SB.rlSettingsFile);
                SB.Logger.LogInfo(Components.Main, "Loaded the existing RuriLib Settings file");
            }

            if (!File.Exists(SB.proxyManagerSettingsFile))
            {
                SB.Logger.LogWarning(Components.Main, "Proxy manager Settings file not found, generating a default one");
                SB.Settings.ProxyManagerSettings.ProxySiteUrls.Add(SB.defaultProxySiteUrl);
                SB.Settings.ProxyManagerSettings.ActiveProxySiteUrl = SB.defaultProxySiteUrl;
                SB.Settings.ProxyManagerSettings.ProxyKeys.Add(SB.defaultProxyKey);
                SB.Settings.ProxyManagerSettings.ActiveProxyKey = SB.defaultProxyKey;
                IOManager.SaveSettings(SB.proxyManagerSettingsFile, SB.Settings.ProxyManagerSettings);
                SB.Logger.LogInfo(Components.Main, $"Created the default proxy manager Settings file {SB.proxyManagerSettingsFile}");
            }
            else
            {
                SB.Settings.ProxyManagerSettings = IOManager.LoadSettings<ProxyManagerSettings>(SB.proxyManagerSettingsFile);
                SB.Logger.LogInfo(Components.Main, "Loaded the existing proxy manager Settings file");
            }

            if (!File.Exists(SB.obSettingsFile))
            {
                MessageBox.Show("SilverBullet Settings file not found, generating a default one");
                SB.Logger.LogWarning(Components.Main, "SilverBullet Settings file not found, generating a default one");
                SBIOManager.SaveSettings(SB.obSettingsFile, SB.SBSettings);
                SB.Logger.LogInfo(Components.Main, $"Created the default SilverBullet Settings file {SB.obSettingsFile}");
            }
            else
            {
                SB.SBSettings = SBIOManager.LoadSettings(SB.obSettingsFile);
                SB.Logger.LogInfo(Components.Main, "Loaded the existing SilverBullet Settings file");
            }

            // If there is no DB backup or if it's more than 1 day old, back up the DB
            try
            {
                if (SB.SBSettings.General.BackupDB &&
                    (!File.Exists(SB.dataBaseBackupFile) ||
                    (File.Exists(SB.dataBaseBackupFile) && ((DateTime.Now - File.GetCreationTime(SB.dataBaseBackupFile)).TotalDays > 1))))
                {
                    // Check that the DB is not corrupted by accessing a random collection. If this fails, an exception will be thrown.
                    using (var db = new LiteDB.LiteDatabase(SB.dataBaseFile))
                    {
                        var coll = db.GetCollection<RuriLib.Models.CProxy>("proxies");
                    }

                    // Delete the old file and copy over the new one
                    File.Delete(SB.dataBaseBackupFile);
                    File.Copy(SB.dataBaseFile, SB.dataBaseBackupFile);
                    SB.Logger.LogInfo(Components.Main, "Backed up the DB");
                }
            }
            catch (Exception ex)
            {
                SB.Logger.LogError(Components.Main, $"Could not backup the DB: {ex.Message}");
            }

            Topmost = SB.SBSettings.General.AlwaysOnTop;

            // Load Plugins
            (IEnumerable<PluginControl>, IEnumerable<IBlockPlugin>, IEnumerable<string>) plugins;
            //(plugins, blockPlugins)
            try
            {
                plugins = Loader.LoadPlugins(SB.pluginsFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in load plugins\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                throw;
            }
            SB.BlockPlugins = plugins.Item2.ToList();
            SB.PluginNames = plugins.Item3;

            // Set mappings
            SB.BlockMappings = new List<(Type, Type, LinearGradientBrush)>()
            {
                ( typeof(BlockBypassCF),        typeof(PageBlockBypassCF),          Colors.DarkSalmon.GetLinearGradientBrush() ),
                ( typeof(BlockImageCaptcha),    typeof(PageBlockCaptcha),           Colors.DarkOrange.GetLinearGradientBrush()),
                ( typeof(BlockReportCaptcha),   typeof(PageBlockReportCaptcha),     Colors.DarkOrange.GetLinearGradientBrush()),
                ( typeof(BlockFunction),        typeof(PageBlockFunction),          Colors.YellowGreen.GetLinearGradientBrush()),
                ( typeof(BlockKeycheck),        typeof(PageBlockKeycheck),          Colors.DodgerBlue.GetLinearGradientBrush()),
                ( typeof(BlockLSCode),          typeof(PageBlockLSCode),            Colors.White.GetLinearGradientBrush()),
                ( typeof(BlockParse),           typeof(PageBlockParse),             Colors.Gold.GetLinearGradientBrush()),
                ( typeof(BlockRecaptcha),       typeof(PageBlockRecaptcha),         Colors.Turquoise.GetLinearGradientBrush()),
                ( typeof(BlockSolveCaptcha),    typeof(PageBlockSolveCaptcha),      Colors.Turquoise.GetLinearGradientBrush()),
                ( typeof(BlockRequest),         typeof(PageBlockRequest),           Colors.LimeGreen.GetLinearGradientBrush()),
                ( typeof(BlockTCP),             typeof(PageBlockTCP),               Colors.MediumPurple.GetLinearGradientBrush()),
                ( typeof(BlockOcr),             typeof(PageBlockOcr),               Color.FromRgb(230, 230, 230).GetLinearGradientBrush()),
                ( typeof(BlockWebSocket),       typeof(PageBlockWebSocket),         Color.FromRgb(245, 180, 0).GetLinearGradientBrush()),
                ( typeof(BlockSpeechToText),    typeof(PageBlockSpeechToText),      Color.FromRgb(164, 198, 197).GetLinearGradientBrush()),
                ( typeof(BlockUtility),         typeof(PageBlockUtility),           Colors.Wheat.GetLinearGradientBrush()),
                ( typeof(SBlockBrowserAction),  typeof(PageSBlockBrowserAction),    Colors.Green.GetLinearGradientBrush()),
                ( typeof(SBlockElementAction),  typeof(PageSBlockElementAction),    Colors.Firebrick.GetLinearGradientBrush()),
                ( typeof(SBlockExecuteJS),      typeof(PageSBlockExecuteJS),        Color.FromRgb(60, 193, 226).GetLinearGradientBrush()),
                ( typeof(SBlockNavigate),       typeof(PageSBlockNavigate),         Colors.RoyalBlue.GetLinearGradientBrush())
            };

            // Add block plugins to mappings
            foreach (var plugin in plugins.Item2)
            {
                try
                {
                    SB.BlockMappings.Add((plugin.GetType(), typeof(BlockPluginPage), plugin.LinearGradientBrush));
                    BlockParser.BlockMappings.Add(plugin.Name, plugin.GetType());
                    SB.Logger.LogInfo(Components.Main, $"Initialized {plugin.Name} block plugin");
                }
                catch (Exception ex)
                {
                    SB.Logger.LogError(Components.Main, $"{ex.Message}\n{plugin.Name}.dll", true);
                    Environment.Exit(0);
                }
            }


            // ViewModels
            SB.RunnerManager = new RunnerManagerViewModel();
            SB.ProxyManager = new ProxyManagerViewModel();
            SB.WordlistManager = new WordlistManagerViewModel();
            SB.ConfigManager = new ConfigManagerViewModel();
            SB.HitsDB = new HitsDBViewModel();

            // Views
            RunnerManagerPage = new RunnerManager();

            // If we create first runner and there was no session to restore
            if (SB.SBSettings.General.AutoCreateRunner & !SB.RunnerManager.RestoreSession())
            {
                var firstRunner = SB.RunnerManager.Create();
                CurrentRunnerPage = SB.RunnerManager.RunnersCollection.FirstOrDefault().View;
            }

            SB.Logger.LogInfo(Components.Main, "Initialized RunnerManager");
            ProxyManagerPage = new ProxyManager();
            SB.Logger.LogInfo(Components.Main, "Initialized ProxyManager");
            WordlistManagerPage = new WordlistManager();
            SB.Logger.LogInfo(Components.Main, "Initialized WordlistManager");
            ConfigsPage = new ConfigsSection();
            SB.Logger.LogInfo(Components.Main, "Initialized ConfigManager");
            HitsDBPage = new HitsDB();
            SB.Logger.LogInfo(Components.Main, "Initialized HitsDB");
            OBSettingsPage = new Settings();
            SB.Logger.LogInfo(Components.Main, "Initialized Settings");
            ToolsPage = new ToolsSection();
            SB.Logger.LogInfo(Components.Main, "Initialized Tools");
            PluginsPage = new PluginsSection(plugins.Item1);
            SB.Logger.LogInfo(Components.Main, "Initialized Plugins");
            AboutPage = new Help();
            SilverZonePage = new SilverZone();

            menuOptionRunner_Click(this, null);

            var width = SB.SBSettings.General.StartingWidth;
            var height = SB.SBSettings.General.StartingHeight;
            if (width > 800) Width = width;
            if (height > 600) Height = height;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (SB.SBSettings.Themes.EnableSnow)
                Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10000 / SB.SBSettings.Themes.SnowAmount) };
            t.Tick += (s, ea) => Snow();
            t.Start();
        }

        private void Snow()
        {
            if (snowBuffer >= 100)
            {
                int i = 0;
                while (i < Root.Children.Count)
                {
                    // Remove first snowflake you find (oldest) before putting another one so there are max 100 snowflakes on screen
                    if (Root.Children[i].GetType() == typeof(Snowflake)) { Root.Children.RemoveAt(i); break; }
                    i++;
                }
            }

            var x = rand.Next(-500, (int)Root.ActualWidth - 100);
            var y = -100;
            var s = rand.Next(5, 15);

            var flake = new Snowflake
            {
                Width = s,
                Height = s,
                RenderTransform = new TranslateTransform
                {
                    X = x,
                    Y = y
                },
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false
            };

            Grid.SetColumn(flake, 1);
            Grid.SetRow(flake, 2);
            Root.Children.Add(flake);

            var d = TimeSpan.FromSeconds(rand.Next(1, 4));

            x += rand.Next(100, 500);
            var ax = new DoubleAnimation { To = x, Duration = d };
            flake.RenderTransform.BeginAnimation(TranslateTransform.XProperty, ax);

            y = (int)Root.ActualHeight + 200;
            var ay = new DoubleAnimation { To = y, Duration = d };
            flake.RenderTransform.BeginAnimation(TranslateTransform.YProperty, ay);

            snowBuffer++;
        }

        public void ShowRunnerManager()
        {
            CurrentRunnerPage = null;
            Main.Content = RunnerManagerPage;
        }

        public void ShowRunner(Runner page)
        {
            CurrentRunnerPage = page;
            Main.Content = page;
        }

        #region Menu Options Click Events
        public void menuOptionRunner_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentRunnerPage == null) Main.Content = RunnerManagerPage;
            else Main.Content = CurrentRunnerPage;
            menuOptionSelected(menuOptionRunner);
        }

        private void menuOptionProxyManager_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = ProxyManagerPage;
            menuOptionSelected(menuOptionProxyManager);
        }

        private void menuOptionWordlistManager_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = WordlistManagerPage;
            menuOptionSelected(menuOptionWordlistManager);
        }

        private void menuOptionConfigCreator_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = ConfigsPage;
            menuOptionSelected(menuOptionConfigCreator);
        }

        private void menuOptionHitsDatabase_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = HitsDBPage;
            menuOptionSelected(menuOptionHitsDatabase);
        }

        private void menuOptionTools_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = ToolsPage;
            menuOptionSelected(menuOptionTools);
        }

        private void menuOptionPlugins_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = PluginsPage;
            menuOptionSelected(menuOptionPlugins);
        }

        private void menuOptionSettings_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = OBSettingsPage;
            menuOptionSelected(menuOptionSettings);
        }

        private void menuOptionAbout_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = AboutPage;
            menuOptionSelected(menuOptionAbout);
        }

        private void menuOptioSilverZone_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = SilverZonePage;
            menuOptionSelected(menuOptionSilverZone);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() => menuOptionAbout_Click(sender, e), DispatcherPriority.Background);
                Dispatcher.Invoke(() => AboutPage.checkForUpdateLabel_MouseLeftButtonUp(sender, null), DispatcherPriority.Background);
                Dispatcher.Invoke(() => AboutPage.CheckUpdatePage.CheckForUpdate(), DispatcherPriority.Background);
            });
        }

        private void menuOptionSelected(object sender)
        {
            foreach (var child in topMenu.Children)
            {
                try
                {
                    Button option;
                    if (child is Badged badged)
                    {
                        option = badged.Content as Button;
                    }
                    else
                    {
                        option = (Button)child;
                    }
                    option.Foreground = Utils.GetBrush("ForegroundMain");
                }
                catch { }
            }
            ((Button)sender).Foreground = Utils.GetBrush("ForegroundMenuSelected");
        }

        private void IconDiscord_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var url = string.Empty;
                using (Task.Run(() =>
                {
                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0");
                        url = wc.DownloadString("https://raw.githubusercontent.com/mohamm4dx/SilverBullet/master/OpenBullet/Discoard.txt");
                    }
                }).ContinueWith(_ =>
                {
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        MessageBox.Show("not found!", "ERROR");
                        return;
                    }
                    using (Process.Start(url)) ;
                })) ;
            }
            catch (InvalidOperationException) { }
            catch (NullReferenceException) { }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred", "ERROR");
            }
        }

        private void IconTelegram_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start("tg://resolve?domain=SilverBulletSoft");
            }
            catch (Exception ex) { }
        }

        #endregion

        private void quitButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckOnQuit())
            {
                try { NotepadPlusExtensions.Close(); } catch { }
                Environment.Exit(0);
            }
        }

        private void quitButton_MouseEnter(object sender, MouseEventArgs e)
        {
            quitButton.Background = new SolidColorBrush(Colors.DarkRed);
        }

        private void quitButton_MouseLeave(object sender, MouseEventArgs e)
        {
            quitButton.Background = new SolidColorBrush(Colors.Transparent);
        }

        private bool CheckOnQuit()
        {
            var active = SB.RunnerManager.RunnersCollection.Count(r => r.ViewModel.Busy);
            if (!SB.SBSettings.General.DisableQuitWarning || active > 0)
            {
                SB.Logger.LogWarning(Components.Main, "Prompting quit confirmation");

                if (active == 0)
                {
                    if (MessageBox.Show($"Are you sure you want to quit?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        return false;
                }
                else
                {
                    if (MessageBox.Show($"There are {active} active runners. Are you sure you want to quit?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        return false;
                }
            }

            if (!SB.SBSettings.General.DisableNotSavedWarning && !SB.MainWindow.ConfigsPage.ConfigManagerPage.CheckSaved())
            {
                SB.Logger.LogWarning(Components.Main, "Config not saved, prompting quit confirmation");
                if (MessageBox.Show("The Config in Stacker wasn't saved.\nAre you sure you want to quit?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return false;
            }

            SB.Logger.LogInfo(Components.Main, "Saving RunnerManager session to the database");
            SB.RunnerManager.SaveSession();

            SB.Logger.LogInfo(Components.Main, "Quit sequence initiated");
            return true;
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WindowState == WindowState.Normal)
                {
                    var screen = WpfScreen.GetScreenFrom(this);
                    MaxHeight = screen.WorkingArea.Height;
                    MaxWidth = screen.WorkingArea.Width;
                    WindowState = WindowState.Maximized;
                    return;
                }
                WindowState = WindowState.Normal;
            }
            catch { }
        }

        private void maximizeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            maximizeButton.Background = new SolidColorBrush(Colors.DimGray);
        }

        private void maximizeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            maximizeButton.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void minimizeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            minimizeButton.Background = new SolidColorBrush(Colors.DimGray);
        }

        private void minimizeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            minimizeButton.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void titleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                maximizeButton_Click(sender, e);
            }
        }

        private void dragPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                WindowDrag(sender, e);
            }
        }

        private void gripImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Capture(gripImage))
            {
                _isResizing = true;
                _startPosition = Mouse.GetPosition(this);
            }
        }

        private void gripImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                System.Windows.Point currentPosition = Mouse.GetPosition(this);
                double diffX = currentPosition.X - _startPosition.X;
                double diffY = currentPosition.Y - _startPosition.Y;
                Width = Width + diffX;
                Height = Height + diffY;
                _startPosition = currentPosition;
            }
        }

        private void gripImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing == true)
            {
                _isResizing = false;
                Mouse.Capture(null);
            }
        }

        private void screenshotImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var bitmap = CopyScreen((int)Width, (int)Height, (int)Top, (int)Left);
            Clipboard.SetImage(bitmap);
            GetBitmap(bitmap).Save("screenshot.jpg", ImageFormat.Jpeg);
            SB.Logger.LogInfo(Components.Main, "Acquired screenshot");
        }

        private static BitmapSource CopyScreen(int width, int height, int top, int left)
        {
            using (var screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(left, top, 0, 0, screenBmp.Size);
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        private static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        private void logImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SB.LogWindow == null)
            {
                SB.LogWindow = new LogWindow();
                SB.LogWindow.Show();
            }
            else
            {
                SB.LogWindow.Show();
            }
        }

        public void SetStyle()
        {
            try
            {
                var brush = Utils.GetBrush("BackgroundMain");

                if (!SB.SBSettings.Themes.UseImage)
                {
                    Background = brush;
                    Main.Background = brush;
                }
                else
                {
                    // BACKGROUND
                    if (File.Exists(SB.SBSettings.Themes.BackgroundImage))
                    {
                        var bbrush = new ImageBrush(new BitmapImage(new Uri(SB.SBSettings.Themes.BackgroundImage)));
                        bbrush.Opacity = (double)((double)SB.SBSettings.Themes.BackgroundImageOpacity / (double)100);
                        Background = bbrush;
                    }
                    else
                    {
                        Background = brush;
                    }

                    // LOGO
                    if (File.Exists(SB.SBSettings.Themes.BackgroundLogo))
                    {
                        var lbrush = new ImageBrush(new BitmapImage(new Uri(SB.SBSettings.Themes.BackgroundLogo)));
                        lbrush.AlignmentX = AlignmentX.Center;
                        lbrush.AlignmentY = AlignmentY.Center;
                        lbrush.Stretch = Stretch.None;
                        lbrush.Opacity = (double)((double)SB.SBSettings.Themes.BackgroundImageOpacity / (double)100);
                        Main.Background = lbrush;
                    }
                    else
                    {
                        Main.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/"
                            + Assembly.GetExecutingAssembly().GetName().Name
                            + ";component/"
                            + "Images/Themes/empty.png", UriKind.Absolute)));
                    }
                }
            }
            catch { }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                silverZoneTimer = new Timer(_ =>
                {
                    try
                    {
                        var badge = SilverZonePage.GetBadge();
                        Dispatcher.Invoke(() => silverZoneBadged.Badge = badge > 99 ? "99+" : badge.ToString());
                    }
                    catch { }
                }, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            }
            catch { }
        }

        private HwndSource _hwndSource;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        //Attach this to the MouseDown event of your drag control to move the window in place of the title bar
        private void WindowDrag(object sender, MouseButtonEventArgs e) // MouseDown
        {
            ReleaseCapture();
            SendMessage(new WindowInteropHelper(this).Handle,
                0xA1, (IntPtr)0x2, (IntPtr)0);
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

    }
}
