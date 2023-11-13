﻿namespace RuriLib.ViewModels
{
    /// <summary>
    /// The settings used across RuriLib classes. Contains the smaller settings categories.
    /// </summary>
    public class RLSettingsViewModel
    {
        /// <summary>The General Settings of RuriLib.</summary>
        public SettingsGeneral General { get; set; } = new SettingsGeneral();
        /// <summary>The Proxy Settings of RuriLib.</summary>
        public SettingsProxies Proxies { get; set; } = new SettingsProxies();
        /// <summary>The Captcha Settings of RuriLib.</summary>
        public SettingsCaptchas Captchas { get; set; } = new SettingsCaptchas();
        /// <summary>The Selenium Settings of RuriLib.</summary>
        public SettingsSelenium Selenium { get; set; } = new SettingsSelenium();
        /// <summary>The OCR Settings of RuriLib.</summary>
        public SettingsOcr Ocr { get; set; } = new SettingsOcr();
        /// <summary>The CefSharp Settings of RuriLib.</summary>
        public SettingsCefSharp CefSharp { get; set; } = new SettingsCefSharp();

        /// <summary>
        /// Resets the properties to their default value.
        /// </summary>
        public void Reset()
        {
            General.Reset();
            Proxies.Reset();
            Captchas.Reset();
            Selenium.Reset();
            Ocr.Reset();
            
        }
    }
}
