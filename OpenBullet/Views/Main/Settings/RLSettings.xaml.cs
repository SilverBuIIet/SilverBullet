﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OpenBullet.Views.Main.Settings.RL;
using RuriLib;

namespace OpenBullet.Views.Main.Settings
{
    /// <summary>
    /// Logica di interazione per RLSettingsPage.xaml
    /// </summary>
    public partial class RLSettings : Page
    {
        General GeneralPage = new General();
        Proxies ProxiesPage = new Proxies();
        Captchas CaptchasPage = new Captchas();
        Selenium SeleniumPage = new Selenium();
        Ocr OcrPage = new Ocr();
        RL.CefSharp cefSharpPage = new RL.CefSharp();

        public RLSettings()
        {
            InitializeComponent();
            menuOptionGeneral_MouseDown(this, null);
        }

        #region Menu Options
        private void menuOptionGeneral_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = GeneralPage;
            menuOptionSelected(menuOptionGeneral);
        }

        private void menuOptionProxies_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = ProxiesPage;
            menuOptionSelected(menuOptionProxies);
        }

        private void menuOptionCaptchas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = CaptchasPage;
            menuOptionSelected(menuOptionCaptchas);
        }

        private void menuOptionSelenium_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = SeleniumPage;
            menuOptionSelected(menuOptionSelenium);
        }

        private void menuOptionOcr_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = OcrPage;
            menuOptionSelected(menuOptionOcr);
        }

        private void menuOptionCefSharpBrw_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Main.Content = cefSharpPage;
            menuOptionSelected(sender);
        }

        private void menuOptionSelected(object sender)
        {
            foreach (var child in topMenu.Children)
            {
                try
                {
                    var c = (Label)child;
                    c.Foreground = Utils.GetBrush("ForegroundMain");
                }
                catch { }
            }
            ((Label)sender).Foreground = Utils.GetBrush("ForegroundGood");
        }

        #endregion

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            IOManager.SaveSettings(SB.rlSettingsFile, SB.Settings.RLSettings);
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all your RuriLib settings?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                SB.Settings.RLSettings.Reset();
        }
    }
}
