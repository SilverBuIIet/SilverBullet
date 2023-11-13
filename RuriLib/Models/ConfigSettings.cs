﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Newtonsoft.Json;
using RuriLib.Models;
using RuriLib.ViewModels;

namespace RuriLib
{
    /// <summary>
    /// Contains all the settings of a Config.
    /// </summary>
    public class ConfigSettings : ViewModelBase
    {
        #region General
        private string name = "";
        /// <summary>The name of the Config.</summary>
        public string Name { get { return name; } set { name = value; OnPropertyChanged(); } }

        private int suggestedBots = 1;
        /// <summary>The suggested amount of Bots that should be used with the config.</summary>
        public int SuggestedBots { get { return suggestedBots; } set { suggestedBots = value; OnPropertyChanged(); } }

        private int maxCPM = 0;
        /// <summary>The maximum CPM around which the Master Worker will stop starting bots and wait for the CPM to decrease below the threshold.</summary>
        public int MaxCPM { get { return maxCPM; } set { maxCPM = value; OnPropertyChanged(); } }

        private DateTime lastModified = DateTime.Now;
        /// <summary>When the Config was last modified.</summary>
        public DateTime LastModified { get { return lastModified; } set { lastModified = value; OnPropertyChanged(); } }

        private string additionalInfo = "";
        /// <summary>Additional information on the Config usage.</summary>
        public string AdditionalInfo { get { return additionalInfo; } set { additionalInfo = value; OnPropertyChanged(); } }

        private string[] requiredPlugins = new string[] { };
        /// <summary>The plugins that are necessary in order for this config to run.</summary>
        public string[] RequiredPlugins { get { return requiredPlugins; } set { requiredPlugins = value; OnPropertyChanged(); OnPropertyChanged(nameof(RequiredPluginsString)); } }

        /// <summary>The required plugins list as a comma separated string.</summary>
        [JsonIgnore]
        public string RequiredPluginsString => string.Join(", ", RequiredPlugins);

        private string author = "";
        /// <summary>The name of the Author of the Config.</summary>
        public string Author { get { return author; } set { author = value; OnPropertyChanged(); } }

        private string version = "1.2.2";
        /// <summary>The version of RuriLib the Config was made with.</summary>
        public string Version { get { return version; } set { version = value; OnPropertyChanged(); } }

        private bool saveEmptyCaptures = false;
        /// <summary>Whether to remove the empty captures before saving the hits to the database.</summary>
        public bool SaveEmptyCaptures { get { return saveEmptyCaptures; } set { saveEmptyCaptures = value; OnPropertyChanged(); } }

        private bool continueOnCustom = false;
        /// <summary>Whether to continue execution after a Custom status has been reached.</summary>
        public bool ContinueOnCustom { get { return continueOnCustom; } set { continueOnCustom = value; OnPropertyChanged(); } }

        // HACK: This setting should not be here, it should be in the individual runner's scope!!!
        // I just put it here as a quick patch as this is going to be moved in OB2 and I don't want to touch the runner interface.
        private bool saveHitsToTextFile = false;
        /// <summary>Whether to save hits to a text file instead of the database.</summary>
        public bool SaveHitsToTextFile { get { return saveHitsToTextFile; } set { saveHitsToTextFile = value; OnPropertyChanged(); } }
        #endregion

        #region Requests
        private bool ignoreResponseErrors = false;
        /// <summary>Whether to proceed if an HTTP request fails instead of giving the ERROR status.</summary>
        public bool IgnoreResponseErrors { get { return ignoreResponseErrors; } set { ignoreResponseErrors = value; OnPropertyChanged(); } }

        private int _maxRedirects = 8;
        /// <summary>The maximum amount of times we can be redirected to different URLs for a single request.</summary>
        public int MaxRedirects { get { return _maxRedirects; } set { _maxRedirects = value; OnPropertyChanged(); } }
        #endregion

        #region Proxy
        private bool needsProxies = false;
        /// <summary>Whether the Config needs proxies to work (the Runner can override this).</summary>
        public bool NeedsProxies { get { return needsProxies; } set { needsProxies = value; OnPropertyChanged(); } }

        private bool onlySocks = false;
        /// <summary>Whether only SOCKS proxies should be used.</summary>
        public bool OnlySocks { get { return onlySocks; } set { onlySocks = value; OnPropertyChanged(); } }

        private bool onlySsl = false;
        /// <summary>Whether only SSL proxies should be used.</summary>
        public bool OnlySsl { get { return onlySsl; } set { onlySsl = value; OnPropertyChanged(); } }

        private int maxProxyUses = 0;
        /// <summary>The maximum amount of times a proxy can be used before being banned by the Runner (0 for infinite).</summary>
        public int MaxProxyUses { get { return maxProxyUses; } set { maxProxyUses = value; OnPropertyChanged(); } }

        private bool banProxyAfterGoodStatus = false;
        /// <summary>Whether to ban the proxy after a SUCCESS, CUSTOM or NONE status (not FAIL or RETRY).</summary>
        public bool BanProxyAfterGoodStatus { get { return banProxyAfterGoodStatus; } set { banProxyAfterGoodStatus = value; OnPropertyChanged(); } }

        private int banLoopEvasionOverride = -1;
        /// <summary>
        /// The maximum amount of times a data line ends up with a BAN status before it's marked as ToCheck.
        /// Overrides the global setting unless set to -1.
        /// </summary>
        public int BanLoopEvasionOverride { get { return banLoopEvasionOverride; } set { banLoopEvasionOverride = value; OnPropertyChanged(); } }
        #endregion

        #region Data
        private bool encodeData = false;
        /// <summary>Whether the data should be URLencoded after being sliced.</summary>
        public bool EncodeData { get { return encodeData; } set { encodeData = value; OnPropertyChanged(); } }

        private string allowedWordlist1 = "";
        /// <summary>The name of the first allowed WordlistType.</summary>
        public string AllowedWordlist1 { get { return allowedWordlist1; } set { allowedWordlist1 = value; OnPropertyChanged(); } }

        private string allowedWordlist2 = "";
        /// <summary>The name of the second allowed WordlistType.</summary>
        public string AllowedWordlist2 { get { return allowedWordlist2; } set { allowedWordlist2 = value; OnPropertyChanged(); } }

        /// <summary>The collection of data rules that check if a data line is valid.</summary>
        public ObservableCollection<DataRule> DataRules { get; set; } = new ObservableCollection<DataRule>();
        #endregion

        #region Inputs
        /// <summary>The collection of custom inputs that the Runner asks to the user upon start.</summary>
        public ObservableCollection<CustomInput> CustomInputs { get; set; } = new ObservableCollection<CustomInput>();
        #endregion

        #region OCR

        private string captchaUrl = "";

        /// <summary>The URL used for OCR image identification.</summary>
        public string CaptchaUrl { get { return captchaUrl; } set { captchaUrl = value; OnPropertyChanged(); } }

        private bool isBase64;
        /// <summary></summary>
        public bool IsBase64 { get { return isBase64; } set { isBase64 = value; OnPropertyChanged(); } }

        private ObservableCollection<string> filterList =
            new ObservableCollection<string>();

        /// <summary>
        /// filter list
        /// </summary>
        public ObservableCollection<string> FilterList
        {
            get { return filterList; }
            set { filterList = value; OnPropertyChanged(); }
        }

        private bool evaluateMathOCR;
        public bool EvaluateMathOCR
        {
            get { return evaluateMathOCR; }
            set { evaluateMathOCR = value; OnPropertyChanged(); }
        }

        private Functions.Requests.SecurityProtocol securityProtocol = Functions.Requests.SecurityProtocol.SystemDefault;
        public Functions.Requests.SecurityProtocol SecurityProtocol
        {
            get => securityProtocol;
            set
            {
                securityProtocol = value; OnPropertyChanged();
            }
        }

        #endregion OCR

        #region Selenium
        private bool forceHeadless = false;
        /// <summary>Whether to override the default choice of the user and force headless mode.</summary>
        public bool ForceHeadless { get { return forceHeadless; } set { forceHeadless = value; OnPropertyChanged(); } }

        private bool alwaysOpen = false;
        /// <summary>Whether to always open a browser at the start of the checking process (if not already open).</summary>
        public bool AlwaysOpen { get { return alwaysOpen; } set { alwaysOpen = value; OnPropertyChanged(); } }

        private bool alwaysQuit = false;
        /// <summary>Whether to always quit the browser and dispose of the WebDriver at the end of the checking process (if any browser is open).</summary>
        public bool AlwaysQuit { get { return alwaysQuit; } set { alwaysQuit = value; OnPropertyChanged(); } }

        private bool quitOnBanRetry = false;
        /// <summary>Whether to quit the browser and dispose of the WebDriver at the end of the checking process (if any browser is open) on a BAN or RETRY status.</summary>
        public bool QuitOnBanRetry { get { return quitOnBanRetry; } set { quitOnBanRetry = value; OnPropertyChanged(); } }

        private bool acceptInsecureCertificates = true;
        /// <summary>Gets or sets a value indicating whether the browser should accept self-signed
        /// SSL certificates.</summary>
        public bool AcceptInsecureCertificates { get { return acceptInsecureCertificates; } set { acceptInsecureCertificates = value; OnPropertyChanged(); } }

        private bool disableNotifications = false;
        /// <summary>Whether to disable notifications the lock the page and make it impossible to proceed.</summary>
        public bool DisableNotifications { get { return disableNotifications; } set { disableNotifications = value; OnPropertyChanged(); } }

        private bool disableImageLoading;
        public bool DisableImageLoading
        {
            get { return disableImageLoading; }
            set { disableImageLoading = value; OnPropertyChanged(); }
        }

        private bool defaultProfileDirectory;
        /// <summary>
        /// Whether to default profile directory
        /// </summary>
        public bool DefaultProfileDirectory { get { return defaultProfileDirectory; } set { defaultProfileDirectory = value; OnPropertyChanged(); } }

        private string customUserAgent = "";
        /// <summary>The custom User-Agent header that should be used for every request of the browser.</summary>
        public string CustomUserAgent { get { return customUserAgent; } set { customUserAgent = value; OnPropertyChanged(); } }

        private bool randomUA = false;
        /// <summary>Whether to choose a random User-Agent header when opening a browser instance.</summary>
        public bool RandomUA { get { return randomUA; } set { randomUA = value; OnPropertyChanged(); } }

        private string customCMDArgs = "";
        /// <summary>The custom command line arguments that are sent when running the executable file.</summary>
        public string CustomCMDArgs { get { return customCMDArgs; } set { customCMDArgs = value; OnPropertyChanged(); } }
        #endregion

        #region Compile
        private string title;
        /// <summary>
        /// Compiler title
        /// </summary>
        public string Title
        {
            get => title;
            set { title = value; OnPropertyChanged(); }
        }

        private string iconPath;
        /// <summary>
        /// Compiler icon path
        /// </summary>
        public string IconPath
        {
            get => iconPath;
            set { iconPath = value; OnPropertyChanged(); }
        }

        private string licenseSource;
        public string LicenseSource
        {
            get => licenseSource;
            set { licenseSource = value; OnPropertyChanged(); }
        }

        private string message;
        /// <summary>
        /// Compiler message
        /// </summary>
        public string Message
        {
            get => message;
            set { message = value; OnPropertyChanged(); }
        }

        private Color messageColor = Color.FromRgb(255, 255, 255);
        /// <summary>
        /// Compiler message color
        /// </summary>
        public Color MessageColor
        {
            get => messageColor;
            set { messageColor = value; OnPropertyChanged(); }
        }

        private string hitInfoFormat = "[{hit.Type}][{hit.Proxy}] {hit.Data} - [{hit.CapturedString}]";
        /// <summary>
        /// hit information output format
        /// </summary>
        public string HitInfoFormat
        {
            get => hitInfoFormat;
            set { hitInfoFormat = value; OnPropertyChanged(); }
        }

        private Color authorColor = Color.FromRgb(255, 178, 102);
        /// <summary>
        /// Compiler author color
        /// </summary>
        public Color AuthorColor
        {
            get => authorColor;
            set { authorColor = value; OnPropertyChanged(); }
        }

        private Color wordlistColor = Color.FromRgb(181, 194, 225);
        /// <summary>
        /// Compiler wordlist color
        /// </summary>
        public Color WordlistColor
        {
            get => wordlistColor;
            set { wordlistColor = value; OnPropertyChanged(); }
        }

        private Color botsColor = Color.FromRgb(168, 255, 255);
        /// <summary>
        /// Compiler bots color
        /// </summary>
        public Color BotsColor
        {
            get => botsColor;
            set { botsColor = value; OnPropertyChanged(); }
        }

        private Color customInputColor = Color.FromRgb(214, 199, 199);
        /// <summary>
        /// Compiler custom input color
        /// </summary>
        public Color CustomInputColor
        {
            get => customInputColor;
            set { customInputColor = value; OnPropertyChanged(); }
        }

        private Color cpmColor = Color.FromRgb(255, 255, 255);
        /// <summary>
        /// Compiler cpm color
        /// </summary>
        public Color CPMColor
        {
            get => cpmColor;
            set { cpmColor = value; OnPropertyChanged(); }
        }

        private Color progressColor = Color.FromRgb(173, 147, 227);
        /// <summary>
        /// Compiler progress color
        /// </summary>
        public Color ProgressColor
        {
            get => progressColor;
            set { progressColor = value; OnPropertyChanged(); }
        }

        private Color hitsColor = Color.FromRgb(102, 255, 102);
        /// <summary>
        /// Compiler hits color
        /// </summary>
        public Color HitsColor
        {
            get => hitsColor;
            set { hitsColor = value; OnPropertyChanged(); }
        }

        private Color customColor = Color.FromRgb(255, 178, 102);
        /// <summary>
        /// Compiler custom color
        /// </summary>
        public Color CustomColor
        {
            get => customColor;
            set { customColor = value; OnPropertyChanged(); }
        }

        private Color toCheckColor = Color.FromRgb(127, 255, 212);
        /// <summary>
        /// Compiler to check color
        /// </summary>
        public Color ToCheckColor
        {
            get => toCheckColor;
            set { toCheckColor = value; OnPropertyChanged(); }
        }

        private Color failsColor = Color.FromRgb(255, 51, 51);
        /// <summary>
        /// Compiler fails color
        /// </summary>
        public Color FailsColor
        {
            get => failsColor;
            set { failsColor = value; OnPropertyChanged(); }
        }

        private Color retriesColor = Color.FromRgb(255, 255, 153);
        /// <summary>
        /// Compiler retries color
        /// </summary>
        public Color RetriesColor
        {
            get => retriesColor;
            set { retriesColor = value; OnPropertyChanged(); }
        }

        private Color ocrRateColor = Color.FromRgb(70, 152, 253);
        /// <summary>
        /// Compiler ocrRate color
        /// </summary>
        public Color OcrRateColor
        {
            get => ocrRateColor;
            set { ocrRateColor = value; OnPropertyChanged(); }
        }

        private Color proxiesColor = Color.FromRgb(255, 255, 255);
        /// <summary>
        /// Compiler proxies color
        /// </summary>
        public Color ProxiesColor
        {
            get => proxiesColor;
            set { proxiesColor = value; OnPropertyChanged(); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Removes a DataRule given its id.
        /// </summary>
        /// <param name="id">The unique id of the DataRule</param>
        public void RemoveDataRuleById(int id)
        {
            DataRules.Remove(GetDataRuleById(id));
        }

        /// <summary>
        /// Gets a DataRule given its id.
        /// </summary>
        /// <param name="id">The unique id of the DataRule</param>
        /// <returns>The DataRule</returns>
        public DataRule GetDataRuleById(int id)
        {
            return DataRules.FirstOrDefault(r => r.Id == id);
        }

        /// <summary>
        /// Removes a CustomInput given its id.
        /// </summary>
        /// <param name="id">The unique id of the CustomInput</param>
        public void RemoveCustomInputById(int id)
        {
            CustomInputs.Remove(GetCustomInputById(id));
        }

        /// <summary>
        /// Gets a CustomInput given its id.
        /// </summary>
        /// <param name="id">The unique id of the CustomInput</param>
        /// <returns>The CustomInput</returns>
        public CustomInput GetCustomInputById(int id)
        {
            return CustomInputs.FirstOrDefault(c => c.Id == id);
        }
        #endregion
    }
}
