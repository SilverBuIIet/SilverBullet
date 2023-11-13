﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using CefSharp;
using Extreme.Net;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using MahApps.Metro.IconPacks;
using Microsoft.Scripting.Utils;
using OpenBullet.CefBrowser;
using OpenBullet.Editor.Search;
using OpenBullet.ViewModels;
using OpenBullet.Views.CustomMessageBox;
using OpenBullet.Views.Dialogs;
using RuriLib;
using RuriLib.LS;
using RuriLib.Models;
using RuriLib.Runner;
using SilverBullet.Tesseract;

namespace OpenBullet.Views.Main.Configs
{
    /// <summary>
    /// Logica di interazione per Stacker.xaml
    /// </summary>
    /// 
    public partial class Stacker : Page
    {
        private Stopwatch timer;
        private StackerViewModel vm = null;
        private AbortableBackgroundWorker debugger = new AbortableBackgroundWorker();
        XmlNodeList syntaxHelperItems, scriptCompletion;
        TextEditor toolTipEditor;
        CompletionWindow completionWindow;
        private ToolTip toolTip;
        private Task taskSwitchView = null, startCompileTask = null;
        private LoliScriptCompletionData.BlockParameters blockParameters;
        public delegate void SaveConfigEventHandler(object sender, EventArgs e);
        public event SaveConfigEventHandler SaveConfig;
        BrushConverter bc = new BrushConverter();
        SearchTextEditor searchTextEditor;
        OcrEngine _ocrEngine;

        protected virtual void OnSaveConfig()
        {
            SaveConfig?.Invoke(this, EventArgs.Empty);
            try
            {
                var log = SB.Logger.Entries.LastOrDefault();
                if (log == null || !log.LogString.StartsWith("Failed to save the config. Reason:") &&
                    log.LogLevel != LogLevel.Error)
                {
                    //green
                    iconSave.Foreground = bc.ConvertFrom("#FF5DF5A7") as Brush;
                    RestoreForegroundIconSave();
                }
                else
                {
                    //red
                    iconSave.Foreground = bc.ConvertFrom("#FFF5645D") as Brush;
                    RestoreForegroundIconSave();
                }
            }
            catch
            {
                try
                {
                    iconSave.Foreground = Brushes.White;
                }
                catch { }
            }
        }

        Task taskResto;
        private void RestoreForegroundIconSave()
        {
            try
            {
                try { taskResto.Dispose(); } catch { }
                taskResto = Task.Run(() =>
                 {
                     Task.Delay(1099).Wait();
                     Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                     {
                         iconSave.Foreground = Brushes.White;
                     });
                 });
            }
            catch { }
        }

        public Stacker()
        {
            vm = SB.Stacker;
            DataContext = vm;

            try
            {
                if (!Cef.IsInitialized)
                    App.InitializeCefSharp(null);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Could not load file or assembly 'Cef") || ex.Message.Contains("Could not load file or assembly"))
                {
                    SB.Logger.LogError(Components.Stacker, "Visual C++ Redistributable 2015+ (x86 or x64) Not Installed\nDl Link: microsoft.com/en-us/download/details.aspx?id=48145", true);
                }
            }

            InitializeComponent();

            htmlViewBrowser.MenuHandler = new CustomMenuHandler();
            htmlViewBrowser.FrameLoadEnd += htmlViewBrowser_FrameLoadEnd;
            htmlViewBrowser.LoadingStateChanged += htmlViewBrowser_LoadingStateChanged;
            htmlViewBrowser.StatusMessage += htmlViewBrowser_StatusMessage;

            // Style the LoliScript editor
            loliScriptEditor.ShowLineNumbers = true;
            loliScriptEditor.TextArea.Foreground = new SolidColorBrush(Colors.Gainsboro);
            loliScriptEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            using (XmlReader reader = XmlReader.Create("LSHighlighting.xshd"))
            {
                loliScriptEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            // Load the Syntax Helper XML
            XmlDocument doc = new XmlDocument();
            try
            {

                doc.Load("SyntaxHelper.xml");
                var main = doc.DocumentElement.SelectSingleNode("/doc");
                syntaxHelperItems = main.ChildNodes;

                // Only bind the keydown event if the XML was successfully loaded
                loliScriptEditor.KeyDown += loliScriptEditor_KeyDown;
                loliScriptEditor.KeyUp += LoliScriptEditor_KeyUp;
            }
            catch { }

            try
            {
                doc.Load("ScriptCompletion.xml");
                scriptCompletion = doc.DocumentElement
                    .SelectSingleNode("/Keywords")
                    .ChildNodes;
            }
            catch { }

            // Make the Avalon Editor for Syntax Helper and style it
            toolTipEditor = new TextEditor();
            toolTipEditor.TextArea.Foreground = Utils.GetBrush("ForegroundMain");
            toolTipEditor.Background = new SolidColorBrush(Color.FromArgb(22, 22, 22, 50));
            toolTipEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            toolTipEditor.FontSize = 11;
            toolTipEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            toolTipEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            using (XmlReader reader = XmlReader.Create("LSHighlighting.xshd"))
            {
                toolTipEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            toolTip = new ToolTip { Placement = PlacementMode.Relative, PlacementTarget = loliScriptEditor };
            toolTip.Content = toolTipEditor;
            loliScriptEditor.ToolTip = toolTip;

            // Load the script
            vm.LS = new LoliScript(SB.ConfigManager.CurrentConfig.Config.Script);
            loliScriptEditor.Text = vm.LS.Script;

            // If the user prefers Stack view, switch to it
            if (!SB.SBSettings.General.DisplayLoliScriptOnLoad)
            {
                stackButton_Click(this, null);
            }

            logRTB.TextArea.TextView.LinkTextUnderline = false;
            logRTB.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            logRTB.Options.EnableEmailHyperlinks = false;
            logRTB.Options.EnableImeSupport = false;
            logRTB.Options.CutCopyWholeLine = false;
            logRTB.Options.EnableTextDragDrop = false;
            logRTB.Options.EnableVirtualSpace = false;
            logRTB.Options.ShowTabs = false;
            logRTB.TextArea.TextView.Triggers.Clear();
            logRTB.Options.EnableRectangularSelection = false;
            searchTextEditor = SearchTextEditor.Install(logRTB);


            foreach (string i in Enum.GetNames(typeof(ProxyType)))
                if (i != "Chain") proxyTypeCombobox.Items.Add(i);

            proxyTypeCombobox.SelectedIndex = 0;

            foreach (var t in SB.Settings.Environment.GetWordlistTypeNames())
                testDataTypeCombobox.Items.Add(t);

            testDataTypeCombobox.SelectedIndex = 0;

            // Initialize debugger
            debugger.WorkerSupportsCancellation = true;
            debugger.Status = WorkerStatus.Idle;
            debugger.DoWork += new DoWorkEventHandler(DebuggerCheck);
            debugger.RunWorkerCompleted += new RunWorkerCompletedEventHandler(debuggerCompleted);

            SaveConfig += SB.MainWindow.ConfigsPage.ConfigManagerPage.OnSaveConfig;

            //Any CefSharp references have to be in another method with NonInlining
            // attribute so the assembly rolver has time to do it's thing.
            vm.Stack.CollectionChanged += Stack_CollectionChanged;
        }

        private void Stack_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StackerBlockViewModel newBlock in e.NewItems)
                {
                    newBlock.Page.LostFocus += delegate { AutoSaveConfig(); };
                }
            }
            AutoSaveConfig();
        }

        private void TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Space &&
                    e.KeyboardDevice.Modifiers == ModifierKeys.Control && vm.ScriptCompletion)
                {
                    InvokeCompletionWindow(GetDataList(string.Empty), Brushes.White);
                    e.Handled = true;
                }
            }
            catch { }
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text == "\n" || !vm.ScriptCompletion)
                {
                    return;
                }
                if (completionWindow == null && e.Text == " ")
                {
                    InvokeCompletionWindow(GetDataList(e.Text),
                        Brushes.White);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private IEnumerable<Tuple<string, string>> GetDataList(string text)
        {
            return null;
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (!vm.ScriptCompletion) return;
            if (e.Text.Length > 0 && completionWindow != null && char.IsLetter(e.Text[0]))
            {
                // Whenever a non-letter is typed while the completion window is open,
                // insert the currently selected element.
                completionWindow.CompletionList.RequestInsertion(e);
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void InvokeCompletionWindow(IEnumerable<Tuple<string, string>> scriptAutoCompleteList,
            Brush foreground)
        {
            var completionWindow = new CompletionWindow(loliScriptEditor.TextArea);

            #region Set brush
            completionWindow.Background =
               completionWindow.CompletionList.ListBox.Background =
                completionWindow.CompletionList.Background = Brushes.Black;
            completionWindow.CompletionList.Foreground =
                completionWindow.CompletionList.ListBox.Foreground = foreground;
            #endregion Set brush

            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            if (scriptAutoCompleteList.Any())
            {
                foreach (var autoCompleteScript in scriptAutoCompleteList)
                {
                    data.Add(new LoliScriptCompletionData(autoCompleteScript.Item1, autoCompleteScript.Item2));
                }
                completionWindow.Show();
                completionWindow.Closed += delegate { completionWindow = null; };
            }
        }

        private void ClearDebuggerLog(object sender, EventArgs e)
        {
            if (SB.SBSettings.General.SendDebuggerLogToNotepadPlus)
            {
                try { NotepadPlusExtensions.Clear(); } catch { }
            }
            logRTB.Clear();
        }

        #region Buttons
        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                var icon = VisualTreeHelper.GetChild((Grid)e.OriginalSource, 0)
                    as PackIconBase;

                icon.Width = 27.5;
                icon.Height = 27.5;
            }
            catch (InvalidCastException)
            {
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                var icon = VisualTreeHelper.GetChild((Grid)e.OriginalSource, 0)
                   as PackIconBase;

                icon.Width = 24;
                icon.Height = 24;
            }
            catch (InvalidCastException)
            {
            }
        }

        private void AddBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (new MainDialog(new DialogAddBlock(this), "Add Block")).ShowDialog();
        }

        public void AddBlock(BlockBase block)
        {
            int position;
            if (vm.CurrentBlockIndex == -1) position = vm.Stack.Count;
            else position = vm.Stack.Count > 0 ? vm.CurrentBlockIndex + 1 : 0;
            SB.Logger.LogInfo(Components.Stacker, $"Added a block of type {block.GetType()} in position {position}");
            vm.AddBlock(block, position);
        }

        private void RemoveBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: Bring back Ctrl+Z stuff
            //vm.LastDeletedBlock = IOManager.DuplicateBlock(vm.CurrentBlock.Block);
            //vm.LastDeletedIndex = vm.Stack.IndexOf(vm.CurrentBlock);

            foreach (var block in vm.SelectedBlocks) vm.Stack.Remove(block);
            vm.CurrentBlock = null;
            BlockInfo.Content = null;
            vm.UpdateHeights();
        }

        private void DisableBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var b in vm.SelectedBlocks) b.Disable();
        }

        private void CloneBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            vm.ConvertKeychains();
            foreach (var block in vm.SelectedBlocks)
                vm.AddBlock(IOManager.CloneBlock(block.Block), vm.Stack.IndexOf(block) + 1);
        }

        private void MoveUpBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var block in vm.SelectedBlocks) vm.MoveBlockUp(block);
        }

        private void MoveDownBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var block in vm.SelectedBlocks.AsEnumerable().Reverse()) vm.MoveBlockDown(block);
        }

        private void SaveConfig_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSaveConfig();
        }
        #endregion

        #region Keyboard Events
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.Z:
                        if (vm.LastDeletedBlock != null)
                        {
                            vm.AddBlock(vm.LastDeletedBlock, vm.LastDeletedIndex);
                            SB.Logger.LogInfo(Components.Stacker, $"Readded block of type {vm.LastDeletedBlock.GetType()} in position {vm.LastDeletedIndex}");
                            vm.LastDeletedBlock = null;
                        }
                        else SB.Logger.LogError(Components.Stacker, "Nothing to undo");
                        break;

                    case System.Windows.Input.Key.C:
                        if (SB.SBSettings.General.DisableCopyPasteBlocks) return;
                        try { Clipboard.SetText(IOManager.SerializeBlocks(vm.SelectedBlocks.Select(b => b.Block).ToList())); }
                        catch { SB.Logger.LogError(Components.Stacker, "Exception while copying blocks"); }
                        break;

                    case System.Windows.Input.Key.V:
                        if (SB.SBSettings.General.DisableCopyPasteBlocks) return;
                        try
                        {
                            foreach (var block in IOManager.DeserializeBlocks(Clipboard.GetText()))
                                vm.AddBlock(block);
                        }
                        catch { SB.Logger.LogError(Components.Stacker, "Exception while pasting blocks"); }
                        break;

                    case System.Windows.Input.Key.S:
                        vm.LS.Script = loliScriptEditor.Text;
                        OnSaveConfig();
                        break;

                    default:
                        break;
                }

            }
        }
        #endregion

        #region Debugger
        private void startDebuggerButton_Click(object sender, RoutedEventArgs e)
        {
            AutoSaveConfig();
            switch (debugger.Status)
            {
                case WorkerStatus.Idle:
                    if (vm.View == StackerView.Blocks)
                        vm.LS.FromBlocks(vm.GetList());
                    else
                        vm.LS.Script = loliScriptEditor.Text;

                    if (debuggerTabControl.SelectedIndex == 1)
                        logRTB.Focus();
                    vm.ControlsEnabled = false;
                    if (!SB.SBSettings.General.PersistDebuggerLog)
                        ClearDebuggerLog(null, null);
                    dataRTB.Document.Blocks.Clear();

                    if (!debugger.IsBusy)
                    {
                        debugger.RunWorkerAsync();
                        SB.Logger.LogInfo(Components.Stacker, "Started the debugger");
                    }
                    else { SB.Logger.LogError(Components.Stacker, "Cannot start the debugger (busy)"); }

                    startDebuggerButtonLabel.Text = "Abort";
                    startDebuggerButtonLabel.Margin = new Thickness(2, 0, 0, 0);
                    startDebuggerButtonIcon.Kind = PackIconMaterialKind.Stop;
                    startDebuggerButtonIcon.Height = 10;
                    debugger.Status = WorkerStatus.Running;
                    break;

                case WorkerStatus.Running:
                    if (debugger.IsBusy)
                    {
                        debugger.CancelAsync();
                        SB.Logger.LogInfo(Components.Stacker, "Sent Cancellation Request to the debugger");
                    }

                    startDebuggerButtonLabel.Text = "Force";
                    startDebuggerButtonLabel.Margin = new Thickness(2, 0, 0, 0);
                    startDebuggerButtonIcon.Kind = PackIconMaterialKind.Stop;
                    startDebuggerButtonIcon.Height = 10;
                    debugger.Status = WorkerStatus.Stopping;
                    break;

                case WorkerStatus.Stopping:
                    debugger.Abort();
                    SB.Logger.LogInfo(Components.Stacker, "Hard aborted the debugger");
                    startDebuggerButtonLabel.Text = "Start";
                    startDebuggerButtonLabel.Margin = new Thickness(0, 0, 0, 0);
                    startDebuggerButtonIcon.Kind = PackIconMaterialKind.Play;
                    startDebuggerButtonIcon.Height = 13;
                    debugger.Status = WorkerStatus.Idle;
                    vm.ControlsEnabled = true;
                    break;
            }
        }

        private void DebuggerCheck(object sender, DoWorkEventArgs e)
        {
            // Dispose of previous browser (if any)
            if (vm.BotData != null)
            {
                if (vm.BotData.BrowserOpen)
                {
                    SB.Logger.LogInfo(Components.Stacker, "Quitting the previously opened browser");
                    vm.BotData.Driver.Quit();
                    SB.Logger.LogInfo(Components.Stacker, "Quitted correctly");
                }
            }

            // Convert Observables
            SB.Logger.LogInfo(Components.Stacker, "Converting Observables");
            vm.ConvertKeychains();

            // Initialize Request Data
            SB.Logger.LogInfo(Components.Stacker, "Initializing the request data");
            CProxy proxy = null;
            if (vm.TestProxy.StartsWith("(")) // Parse in advanced mode
            {
                try { proxy = new CProxy().Parse(vm.TestProxy); }
                catch { SB.Logger.LogError(Components.Stacker, "Invalid Proxy Syntax", true); }
            }
            else // Parse in standard mode
            {
                var proxyAddress = string.Empty;
                var user = string.Empty;
                var pass = string.Empty;
                if (vm.TestProxy.Contains(":"))
                {
                    var prox = vm.TestProxy.Split(':');
                    proxyAddress = prox[0] + ":" + prox[1];
                    try { user = prox[2]; } catch { }
                    try { pass = prox[3]; } catch { }
                }
                proxy = new CProxy(proxyAddress, vm.ProxyType);
                proxy.Username = user;
                proxy.Password = pass;
            }

            // Initialize BotData and Reset LS
            var cData = new CData(vm.TestData, SB.Settings.Environment.GetWordlistType(vm.TestDataType));
            try { _ocrEngine?.DisposeEngines(); } catch { }
            vm.BotData = new BotData(SB.Settings.RLSettings, vm.Config.Config.Settings, cData, proxy, vm.UseProxy, new Random(), 1) { BotsAmount = 1, OcrEngine = _ocrEngine ?? (_ocrEngine = new OcrEngine()), Worker = debugger };
            vm.LS.Reset();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
            {
                browserStatus.Text = "Idle";
            });

            // Ask for user input
            foreach (var input in vm.BotData.ConfigSettings.CustomInputs)
            {
                SB.Logger.LogInfo(Components.Stacker, $"Asking for user input: {input.Description}");
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    (new MainDialog(new DialogCustomInput(vm, input.VariableName, input.Description), "Custom Input")).ShowDialog();
                }));
            }

            // Set start block
            SB.Logger.LogInfo(Components.Stacker, "Setting the first block as the current block");

            // Print start line
            var proxyEnabledText = vm.UseProxy ? "ENABLED" : "DISABLED";
            vm.BotData.LogBuffer.Add(new LogEntry($"===== DEBUGGER STARTED FOR CONFIG {vm.Config.Name} WITH DATA {vm.TestData} AND PROXY {vm.TestProxy} ({vm.ProxyType}) {proxyEnabledText} ====={Environment.NewLine}", Colors.White));

            vm.LS.SelectLine += LS_SelectLine;
            timer = new Stopwatch();
            timer.Start();

            // Open browser if Always Open
            if (vm.Config.Config.Settings.AlwaysOpen)
            {
                SB.Logger.LogInfo(Components.Stacker, "Opening the Browser");
                SBlockBrowserAction.OpenBrowser(vm.BotData);
            }

            // Step-by-step
            if (vm.SBS)
            {
                vm.SBSClear = true; // Good to go for the first round
                do
                {
                    Thread.Sleep(100);

                    if (debugger.CancellationPending)
                    {
                        SB.Logger.LogInfo(Components.Stacker, "Found cancellation pending, aborting debugger");
                        return;
                    }

                    if (vm.SBSClear)
                    {
                        vm.SBSEnabled = false;
                        Process();
                        SB.Logger.LogInfo(Components.Stacker, $"Block processed in SBS mode, can proceed: {vm.LS.CanProceed}");
                        vm.SBSEnabled = true;
                        vm.SBSClear = false;
                    }
                }
                while (vm.LS.CanProceed);
            }

            // Normal
            else
            {
                do
                {
                    if (debugger.CancellationPending)
                    {
                        SB.Logger.LogInfo(Components.Stacker, "Found cancellation pending, aborting debugger");
                        return;
                    }

                    Process();
                }
                while (vm.LS.CanProceed);
            }

            // Quit Browser if Always Quit
            if (vm.Config.Config.Settings.AlwaysQuit || (vm.Config.Config.Settings.QuitOnBanRetry && (vm.BotData.Status == BotStatus.BAN || vm.BotData.Status == BotStatus.RETRY)))
            {
                try
                {
                    vm.BotData.Driver.Quit();
                    vm.BotData.BrowserOpen = false;
                    SB.Logger.LogInfo(Components.Stacker, "Successfully quit the browser");
                }
                catch (Exception ex) { SB.Logger.LogError(Components.Stacker, $"Cannot quit the browser - {ex.Message}"); }
            }
        }

        private void LS_SelectLine(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var line = (int)sender;
            //    Dispatcher.Invoke(() =>
            //    {
            //        loliScriptEditor.ScrollToLine(line);
            //        var docLine = loliScriptEditor.Document.GetLineByNumber(line);
            //        loliScriptEditor.TextArea.Selection = Selection.Create(loliScriptEditor.TextArea, docLine.Offset, docLine.EndOffset);
            //    });
            //}
            //catch { }
        }

        private void Process()
        {
            try
            {
                vm.LS.TakeStep(vm.BotData);
                SB.Logger.LogInfo(Components.Stacker, $"Processed {BlockBase.TruncatePretty(vm.LS.CurrentLine, 20)}");
            }
            catch (Exception ex)
            {
                SB.Logger.LogError(Components.Stacker, $"Processing of line {BlockBase.TruncatePretty(vm.LS.CurrentLine, 20)} failed, exception: {ex.Message}");
            }

            PrintBotData();
            var task = Task.Run(() => PrintLogBuffer());
            task.Wait(); task.Dispose();
            DisplayHTML();
        }

        private void PrintLogBuffer()
        {
            if (vm.BotData.LogBuffer.Count == 0) return;

            for (var i = 0; i < vm.BotData.LogBuffer.Count; i++)
            {
                var entry = vm.BotData.LogBuffer[i];
                if (!SB.SBSettings.General.DisableDebuggerLog)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input,
                        (ThreadStart)delegate
                    {
                        logRTB.AppendTextToEditor(entry.LogString, entry.LogColor);
                        logRTB.TextArea.Caret.BringCaretToView();
                        logRTB.ScrollToLine(logRTB.LineCount);
                    });
                }
                if (SB.SBSettings.General.SendDebuggerLogToNotepadPlus)
                {
                    NotepadPlusExtensions.ShowText(entry.LogString + Environment.NewLine);
                }
            }
            vm.BotData.LogBuffer.Add(new LogEntry(Environment.NewLine, Colors.White));
        }

        private void PrintBotData()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                dataRTB.Document.Blocks.Clear();
                dataRTB.AppendText(Environment.NewLine);
                dataRTB.AppendText($"BOT STATUS: {vm.BotData.StatusString}" + Environment.NewLine, Colors.White);
                dataRTB.AppendText("VARIABLES:" + Environment.NewLine, Colors.Yellow);
                if (SB.SBSettings.General.DisplayCapturesLast)
                {
                    foreach (var variable in vm.BotData.Variables.All.Where(v => !v.Hidden && !v.IsCapture))
                        dataRTB.AppendText(variable.Name + $" ({variable.Type}) = " + variable.ToString() + Environment.NewLine, Colors.Yellow);

                    foreach (var variable in vm.BotData.Variables.All.Where(v => !v.Hidden && v.IsCapture))
                        dataRTB.AppendText(variable.Name + $" ({variable.Type}) = " + variable.ToString() + Environment.NewLine, Colors.Tomato);
                }
                else
                {
                    foreach (var variable in vm.BotData.Variables.All.Where(v => !v.Hidden))
                        dataRTB.AppendText(variable.Name + $" ({variable.Type}) = " + variable.ToString() + Environment.NewLine, variable.IsCapture ? Colors.Tomato : Colors.Yellow);
                }
            }));
        }

        private void DisplayHTML()
        {
            if (SB.SBSettings.General.DisableHTMLView) return;
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    if (vm.BotData.IsImage)
                    {
                        //htmlViewBrowser.GetCookieManager().DeleteCookies();
                        //foreach (var cookie in vm.BotData.Cookies)
                        //    htmlViewBrowser.GetCookieManager().SetCookieAsync(vm.BotData.Address,
                        //        new CefSharp.Cookie()
                        //        {
                        //            Name = cookie.Key,
                        //            Value = cookie.Value,
                        //            Path = "/"
                        //        });
                        if (htmlViewBrowser.IsBrowserInitialized)
                        {
                            htmlViewBrowser.Load(vm.BotData.Address);
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                            {
                                browserStatus.Text = "Browser Not Initialized!";
                            });
                        }
                        vm.BotData.IsImage = false;
                    }

                    else if (!string.IsNullOrEmpty(vm.BotData.ResponseSource))
                    {
                        //vm.BotData.ResponseSource.Replace("alert(", "(")
                        if (htmlViewBrowser.IsBrowserInitialized)
                        {
                            htmlViewBrowser.LoadHtml(vm.BotData.ResponseSource, vm.BotData.Address, Encoding.UTF8);
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                            {
                                browserStatus.Text = "Browser Not Initialized!";
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The browser has not been initialized"))
                    {
                        App.InitializeCefSharp(htmlViewBrowser);
                    }
                }
            }));
        }

        public void HideScriptErrors(WebBrowser wb, bool hide)
        {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            var objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null)
            {
                wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
                return;
            }
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }

        private void debuggerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer.Stop();
            debugger.Status = WorkerStatus.Idle;
            startDebuggerButtonLabel.Text = "Start";
            startDebuggerButtonLabel.Margin = new Thickness(0, 0, 0, 0);
            startDebuggerButtonIcon.Kind = PackIconMaterialKind.Play;
            startDebuggerButtonIcon.Height = 13;
            vm.SBSEnabled = false;
            vm.ControlsEnabled = true;

            // Print final line
            vm.BotData.LogBuffer.Clear();

            // Check if the input data was valid
            if (!vm.BotData.Data.IsValid)
                vm.BotData.LogBuffer.Add(new LogEntry($"WARNING: The test input data did not respect the validity regex for the selected wordlist type!", Colors.Tomato));

            if (!vm.BotData.Data.RespectsRules(vm.Config.Config.Settings.DataRules.ToList()))
                vm.BotData.LogBuffer.Add(new LogEntry($"WARNING: The test input data did not respect the data rules of this config!", Colors.Tomato));

            vm.BotData.LogBuffer.Add(new LogEntry($"===== DEBUGGER ENDED AFTER {timer.ElapsedMilliseconds / 1000.0} SECOND(S) WITH STATUS: {vm.BotData.StatusString} =====", Colors.White));
            PrintLogBuffer();
            SB.Logger.LogInfo(Components.Stacker, "Debugger completed");
        }

        private void nextStepButton_Click(object sender, RoutedEventArgs e)
        {
            vm.SBSClear = true;
        }

        private void proxyTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.ProxyType = (ProxyType)proxyTypeCombobox.SelectedIndex;
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Logger.LogInfo(Components.Stacker, $"Seaching for {vm.SearchString}");

            // Reset all highlights
            //logRTB.SelectAll();
            logRTB.TextArea.ClearSelection();

            // Check for empty search
            if (vm.SearchString == string.Empty)
            {
                vm.TotalSearchMatches = 0;
                vm.CurrentSearchMatch = 0;
                //vm.UpdateTotalSearchMatches();
                return;
            }

            logRTB.SelectionStart = 0;

            searchTextEditor.SearchPattern = vm.SearchString;
            searchTextEditor.UpdateSearch();
            searchTextEditor.DoSearch(true);

            vm.TotalSearchMatches = searchTextEditor.Count;
            //vm.UpdateTotalSearchMatches();

            SB.Logger.LogInfo(Components.Stacker, $"Found {vm.TotalSearchMatches} matches", false);

            if (vm.TotalSearchMatches > 0)
                vm.CurrentSearchMatch = 1;
        }

        public static List<int> AllIndexesOf(string str, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index, StringComparison.InvariantCultureIgnoreCase);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }

        private void previousMatchButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.TotalSearchMatches == 0) // If no matches, do nothing
            {
                return;
            }
            if (vm.CurrentSearchMatch == 1)
            {
                vm.CurrentSearchMatch = vm.TotalSearchMatches;
            }
            else vm.CurrentSearchMatch--;
            searchTextEditor.FindPrevious();
        }

        private void nextMatchButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.TotalSearchMatches == 0) // If no matches, do nothing
            {
                return;
            }
            if (vm.CurrentSearchMatch == vm.TotalSearchMatches)
            {
                vm.CurrentSearchMatch = 1;
            }
            else vm.CurrentSearchMatch++;
            searchTextEditor.FindNext();
        }

        private void labelTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vm.CurrentBlock != null)
                vm.CurrentBlock.Block.Label = labelTextbox.Text;
        }

        public static TextPointer GetTextPointAt(TextPointer from, int pos)
        {
            TextPointer ret = from;
            int i = 0;

            while ((i < pos) && (ret != null))
            {
                if ((ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text) || (ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.None))
                    i++;

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;

                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            return ret;
        }

        private void testDataTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (testDataTypeCombobox.SelectedItem == null)
            {
                testDataTypeCombobox.SelectedIndex = testDataTypeCombobox.Items.IndexOf(vm.TestDataType);
                return;
            }
            vm.TestDataType = (string)testDataTypeCombobox.SelectedItem;
        }
        #endregion

        #region Block Clicked
        private void blockClicked(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            var block = vm.GetBlockById((int)toggle.Tag);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                block.Selected = !block.Selected;
            }
            else
            {
                vm.DeselectAll();
                block.Selected = true;
            }

            try { blockInfoScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto; } catch { }

            if ((vm.CurrentBlock = vm.SelectedBlocks.LastOrDefault()) == null)
                return;

            if (vm.CurrentBlock.Page != null)
                BlockInfo.Content = vm.CurrentBlock.Page; // Display the Block Info page

            Keyboard.ClearFocus();

            if (vm.CurrentBlock.Page.Title == "PageBlockKeycheck")
            {
                blockInfoScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }

            labelTextbox.Text = vm.CurrentBlock.Block.Label;
        }
        #endregion

        public void SetScript()
        {
            if (vm.View == StackerView.Blocks)
                vm.LS.FromBlocks(vm.GetList());
            else
                vm.LS.Script = loliScriptEditor.Text;

            vm.Config.Config.Script = vm.LS.Script;
        }

        private void loliScriptButton_Click(object sender, RoutedEventArgs e)
        {
            // Convert the Blocks to Script
            vm.LS.FromBlocks(vm.GetList());

            // Display the converted Script into the avalon editor
            loliScriptEditor.Text = vm.LS.Script;

            // Switch tab
            vm.View = StackerView.LoliScript;
            stackerTabControl.SelectedIndex = 0;
        }

        private void stackButton_Click(object sender, RoutedEventArgs e)
        {
            // Try to convert to blocks
            List<BlockBase> blocks = null;

            var action = new Action(async () =>
            {
                try
                {
                    blocks = vm.LS.ToBlocks();
                }
                catch (Exception ex)
                {
                    await Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        MessageBox.Show($"Error while converting to blocks, please check the syntax!\n{ex.Message}");
                    });
                    vm.View = StackerView.LoliScript; // Make sure the view is back to LoliScript
                    return;
                }

                // Add the block viewmodels to the stack
                try
                {
                    await Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        vm.ClearBlocks();
                    });
                }
                catch (Exception ex) { }
                blocks.ForEach(async block =>
                {
                    await Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        vm.AddBlock(block);
                    });
                });

                // Clear the last Block Info page
                vm.CurrentBlock = null;
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    BlockInfo.Content = null;
                });
                // Switch tab
                vm.View = StackerView.Blocks;
                await Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    stackerTabControl.SelectedIndex = 1;
                });
            });

            try
            {
                if (vm.View != StackerView.LoliScript)
                {
                    AutoSaveConfig();
                }
                taskSwitchView?.Dispose();
                if (sender is Button)
                {
                    stackerTabControl.BlurApply(0, 5, TimeSpan.FromSeconds(0.1));
                    stackerTabControl.IsEnabled = false;
                    taskSwitchView = Task.Run(action)
                    .ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            stackerTabControl.BlurDisable(TimeSpan.FromSeconds(0.1));
                            stackerTabControl.IsEnabled = true;
                        });
                    });
                }
                else
                {
                    taskSwitchView = Task.Run(action);
                    taskSwitchView.Wait();
                    taskSwitchView.Dispose();
                }
            }
            catch
            {
                stackerTabControl.IsEnabled = true;
                stackerTabControl.BlurDisable(TimeSpan.FromSeconds(0.1));
            }
        }

        private void loliScriptEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            vm.LS.Script = loliScriptEditor.Text;
            toolTip.IsOpen = false;
            AutoSaveConfig();
        }

        private void loliScriptEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == System.Windows.Input.Key.S)
                {
                    vm.LS.Script = loliScriptEditor.Text;
                    OnSaveConfig();
                }
                else if (e.Key == System.Windows.Input.Key.F)
                {
                    Button_Click(null, null);
                }
            }

            if (SB.SBSettings.General.AutoSaveConfigOnStacker &&
                vm.LS.Script != loliScriptEditor.Text)
            {
                vm.LS.Script = loliScriptEditor.Text;
                OnSaveConfig();
            }

            if (SB.SBSettings.General.DisableSyntaxHelper) return;

            DocumentLine line = loliScriptEditor.Document.GetLineByOffset(loliScriptEditor.CaretOffset);
            var blockLine = loliScriptEditor.Document.GetText(line.Offset, line.Length);

            // Scan for the first non-indented line
            while (blockLine.StartsWith(" ") || blockLine.StartsWith("\t"))
            {
                try
                {
                    line = line.PreviousLine;
                    blockLine = loliScriptEditor.Document.GetText(line.Offset, line.Length);
                }
                catch { break; }
            }

            if (BlockParser.IsBlock(blockLine))
            {
                var blockName = BlockParser.GetBlockType(blockLine);

                var caret = loliScriptEditor.TextArea.Caret.CalculateCaretRectangle();
                toolTip.HorizontalOffset = caret.Right;
                toolTip.VerticalOffset = caret.Bottom;

                XmlNode node = null;
                for (int i = 0; i < syntaxHelperItems.Count; i++)
                {
                    if (syntaxHelperItems[i].Attributes["name"].Value.ToUpper() == blockName.ToUpper())
                    {
                        node = syntaxHelperItems[i];
                        break;
                    }
                }
                if (node == null) return;

                toolTipEditor.Text = node.InnerText;
            }
            else
            {
                toolTip.IsOpen = false;
            }
        }

        private void LoliScriptEditor_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (SB.SBSettings.General.AutoSaveConfigOnStacker &&
                    vm.LS.Script != loliScriptEditor.Text)
                {
                    vm.LS.Script = loliScriptEditor.Text;
                    OnSaveConfig();
                }
            }
            catch { }
        }

        private void openDocButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogLSDoc(), "LoliScript Documentation")).Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FindReplaceDialog.ShowForFind(loliScriptEditor);
            }
            catch { }
        }

        private void loliScriptEditor_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void debuggerTabControl_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.SystemKey == System.Windows.Input.Key.F10)
                {
                    nextStepButton_Click(null, e);
                }
            }
            catch { }
        }

        //search textbox
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((e.OriginalSource as TextBox).Text == string.Empty)
                {
                    searchButton_Click(sender, e);
                }
            }
            catch { }
        }
        //search textbox
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    searchButton_Click(sender, e);
                }
                else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    if (e.Key == System.Windows.Input.Key.Next)
                    {
                        nextMatchButton_MouseDown(sender, null);
                    }
                    else if (e.Key == System.Windows.Input.Key.PageUp)
                    {
                        previousMatchButton_MouseDown(sender, null);
                    }
                }
            }
            catch { }
        }

        private void htmlViewBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            //Wait for the MainFrame to finish loading
            try
            {
                if (e.Frame.IsMain)
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        browserStatus.Text = "MainFrame finished loading";
                    });
                }
            }
            catch { }
        }

        private void htmlViewBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            try
            {
                //Wait for the Page to finish loading
                if (e.IsLoading == false)
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        browserStatus.Text = "All Resources Have Loaded";
                    });
                }
                else
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        browserStatus.Text = "Loading...";
                    });
                }
            }
            catch { }
        }

        private void htmlViewBrowser_StatusMessage(object sender, StatusMessageEventArgs e)
        {
            try
            {
                if (e.Value == "") return;
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    browserStatus.Text = e.Value;
                });
            }
            catch { }
        }

        private void browserStatus_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount != 2) return;
                if (labelInitBrowser.Visibility != Visibility.Visible) return;
                if (!htmlViewBrowser.IsBrowserInitialized)
                {
                    var resultInit = App.InitializeCefSharp(htmlViewBrowser);
                    if (resultInit)
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                        {
                            browserStatus.Text = "Browser Initialized!";
                        });
                        if (htmlViewBrowser.IsBrowserInitialized)
                        {
                            DisplayHTML();
                        }
                    }
                }
                else
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        browserStatus.Text = "Browser Initialized!";
                    });
                    if (htmlViewBrowser.IsBrowserInitialized)
                    {
                        DisplayHTML();
                    }
                }
            }
            catch { }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClearDebuggerLog(sender, e);
        }

        //grid browser
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            browserStatus_MouseDown(sender, e);
        }

        private void browserStatus_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((e.OriginalSource as TextBox).Text == "Browser Not Initialized!")
                    labelInitBrowser.Visibility = Visibility.Visible;
                else labelInitBrowser.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch { }
        }

        //open log in notepad
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                NotepadExtensions.ShowText(logRTB.Text);
            }
            catch (Exception ex)
            {
                SB.Logger.LogError(Components.Stacker, ex.Message, true);
            }
        }

        private void loliScriptEditor_TextChanged(object sender, EventArgs e)
        {
            AutoSaveConfig();
        }

        private void Compile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                stackerTabControl.BlurApply(0, 5, TimeSpan.FromSeconds(0.1));
                stackerTabControl.IsEnabled = false;
                try { startCompileTask?.Dispose(); } catch { }
                startCompileTask = Task.Run(StartCompile)
                .ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        stackerTabControl.BlurDisable(TimeSpan.FromSeconds(0.1));
                        stackerTabControl.IsEnabled = true;
                    });
                });
            }
            catch (Exception ex)
            {
                CustomMsgBox.ShowError(ex.Message);
            }
        }

        private void StartCompile()
        {
            try
            {
                var currentConfig = SB.MainWindow.ConfigsPage.CurrentConfig;
                if (string.IsNullOrWhiteSpace(currentConfig.Config.Script))
                {
                    Dispatcher.Invoke(() => CustomMsgBox.ShowError("Script is empty!!"));
                    return;
                }
                if (currentConfig.Config.BlocksAmount == 0)
                {
                    Dispatcher.Invoke(() => CustomMsgBox.ShowError("Blocks amount is zero!!"));
                    return;
                }
                var settings = currentConfig.Config.Settings;
                if (settings.HitInfoFormat.Split('{').Where(i => i.Contains(".")).Any(i => !i.StartsWith("hit.")))
                {
                    Dispatcher.Invoke(() => CustomMsgBox.ShowError("Hit information format is invalid"));
                    return;
                }
                var configName = Path.GetFileNameWithoutExtension(currentConfig.FileName);
                var dirCompile = "Compiled";
                if (!Directory.Exists(dirCompile)) Directory.CreateDirectory(dirCompile);
                if (!Directory.Exists(dirCompile + "\\bin")) Directory.CreateDirectory(dirCompile + "\\bin");
                using (var compiler = new ScriptCompiler()
                {
                    Output = $"{dirCompile}\\bin\\{configName}.exe",
                    Title = settings.Title,
                    IconPath = settings.IconPath,
                    Message = settings.Message,
                    MessageColor = settings.MessageColor.ConvertToString(),
                    AuthorColor = settings.AuthorColor.ConvertToString(),
                    BotsColor = settings.BotsColor.ConvertToString(),
                    CPMColor = settings.CPMColor.ConvertToString(),
                    CustomColor = settings.CustomColor.ConvertToString(),
                    CustomInputColor = settings.CustomInputColor.ConvertToString(),
                    FailsColor = settings.FailsColor.ConvertToString(),
                    HitsColor = settings.HitsColor.ConvertToString(),
                    OcrRateColor = settings.OcrRateColor.ConvertToString(),
                    ProgressColor = settings.ProgressColor.ConvertToString(),
                    ProxiesColor = settings.ProxiesColor.ConvertToString(),
                    RetriesColor = settings.RetriesColor.ConvertToString(),
                    ToCheckColor = settings.ToCheckColor.ConvertToString(),
                    WordlistColor = settings.WordlistColor.ConvertToString(),
                    SvbConfig = IOManager.SerializeConfig(currentConfig.Config),
                    Config = currentConfig.Config,
                    HitInformationFormat = settings.HitInfoFormat,
                    LicenseSource = string.IsNullOrWhiteSpace(settings.LicenseSource) ? string.Empty : File.ReadAllText(settings.LicenseSource)
                })
                {
                    compiler.AddOption("/optimize");

                    compiler.AddReferences(new[]
                    {
                        "System.dll",
                        "System.Drawing.dll",
                        "System.Core.dll",
                        "mscorlib.dll",
                        "System.Linq.dll",
                        "System.Collections.dll",
                 });

                    compiler.AddReferences(Directory.GetFiles(@"bin", "*.dll")
                          .Where(d => !d.Contains("MahApps.Metro") &&
                          !d.Contains("Xceed.") &&
                          !d.Contains("MaterialDesign") &&
                          !d.Contains("WPFToolkit") &&
                          !d.Contains("ControlzEx.dll") &&
                          !d.Contains("ICSharpCode.AvalonEdit") &&
                          !d.Contains("CefSharp.Wpf"))
                          .ToArray());

                    if (settings.RequiredPlugins?.Length > 0 && compiler.Supports(GeneratorSupport.Resources))
                    {
                        compiler.InjectPluginLoader();
                        var plugins = Directory.GetFiles(SB.pluginsFolder, "*.dll");
                        if (plugins.Length > 0)
                        {
                            List<string> reqPlugins = new List<string>();
                            settings.RequiredPlugins.Distinct().ToList().ForEach(requiredPlugins =>
                            {
                                var pluginName = SB.PluginNames.ToList()[SB.BlockPlugins.IndexOf(SB.BlockPlugins.First(p => p.Name == requiredPlugins))];
                                reqPlugins.Add(pluginName);
                                foreach (var plugin in plugins)
                                {
                                    if (Path.GetFileNameWithoutExtension(plugin) == pluginName)
                                    {
                                        reqPlugins.Remove(pluginName);
                                        compiler.AddEmbeddedResource(plugin);
                                    }
                                }
                            });
                            if (reqPlugins.Count > 0)
                            {
                                Dispatcher.Invoke(() => CustomMsgBox.ShowError($"\"{string.Join(", ", reqPlugins)}\" Plugin(s) not found!"));
                                return;
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(() => CustomMsgBox.ShowError("Required plugins not found!"));
                            return;
                        }
                    }

                    var result = compiler.GetResult(compiler.Execute());
                    if (result.Item2)
                    {
                        Dispatcher.Invoke(() => CustomMsgBox.ShowError(result.Item1));
                        return;
                    }
                    compiler.CopyReferencesAndDependencies();
                    compiler.CopySettings();
                    compiler.CreateRunner(configName, currentConfig.Config);
                    Dispatcher.Invoke(() => CustomMsgBox.Show(result.Item1));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => CustomMsgBox.ShowError(ex.Message));
            }
        }

        public void AutoSaveConfig()
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (SB.MainWindow.ConfigsPage.ConfigManagerPage.CheckSaved()) return;
                    if (SB.SBSettings.General.AutoSaveConfigOnStacker)
                    {
                        OnSaveConfig();
                    }
                });
            });
        }
    }
}

#region VirtualKey
public enum VKeys : int
{
    VK_LBUTTON = 0x01,  //Left mouse button
    VK_RBUTTON = 0x02,  //Right mouse button
    VK_CANCEL = 0x03,  //Control-break processing
    VK_MBUTTON = 0x04,  //Middle mouse button (three-button mouse)
    VK_BACK = 0x08,  //BACKSPACE key
    VK_TAB = 0x09,  //TAB key
    VK_CLEAR = 0x0C,  //CLEAR key
    VK_RETURN = 0x0D,  //ENTER key
    VK_SHIFT = 0x10,  //SHIFT key
    VK_CONTROL = 0x11,  //CTRL key
    VK_MENU = 0x12,  //ALT key
    VK_PAUSE = 0x13,  //PAUSE key
    VK_CAPITAL = 0x14,  //CAPS LOCK key
    VK_HANGUL = 0x15,
    VK_ESCAPE = 0x1B,  //ESC key
    VK_SPACE = 0x20,  //SPACEBAR
    VK_PRIOR = 0x21,  //PAGE UP key
    VK_NEXT = 0x22,  //PAGE DOWN key
    VK_END = 0x23,  //END key
    VK_HOME = 0x24,  //HOME key
    VK_LEFT = 0x25,  //LEFT ARROW key
    VK_UP = 0x26,  //UP ARROW key
    VK_RIGHT = 0x27,  //RIGHT ARROW key
    VK_DOWN = 0x28,  //DOWN ARROW key
    VK_SELECT = 0x29,  //SELECT key
    VK_PRINT = 0x2A,  //PRINT key
    VK_EXECUTE = 0x2B,  //EXECUTE key
    VK_SNAPSHOT = 0x2C,  //PRINT SCREEN key
    VK_INSERT = 0x2D,  //INS key
    VK_DELETE = 0x2E,  //DEL key
    VK_HELP = 0x2F,  //HELP key
    VK_0 = 0x30,  //0 key
    VK_1 = 0x31,  //1 key
    VK_2 = 0x32,  //2 key
    VK_3 = 0x33,  //3 key
    VK_4 = 0x34,  //4 key
    VK_5 = 0x35,  //5 key
    VK_6 = 0x36,  //6 key
    VK_7 = 0x37,  //7 key
    VK_8 = 0x38,  //8 key
    VK_9 = 0x39,  //9 key
    VK_A = 0x41,  //A key
    VK_B = 0x42,  //B key
    VK_C = 0x43,  //C key
    VK_D = 0x44,  //D key
    VK_E = 0x45,  //E key
    VK_F = 0x46,  //F key
    VK_G = 0x47,  //G key
    VK_H = 0x48,  //H key
    VK_I = 0x49,  //I key
    VK_J = 0x4A,  //J key
    VK_K = 0x4B,  //K key
    VK_L = 0x4C,  //L key
    VK_M = 0x4D,  //M key
    VK_N = 0x4E,  //N key
    VK_O = 0x4F,  //O key
    VK_P = 0x50,  //P key
    VK_Q = 0x51,  //Q key
    VK_R = 0x52,  //R key
    VK_S = 0x53,  //S key
    VK_T = 0x54,  //T key
    VK_U = 0x55,  //U key
    VK_V = 0x56,  //V key
    VK_W = 0x57,  //W key
    VK_X = 0x58,  //X key
    VK_Y = 0x59,  //Y key
    VK_Z = 0x5A,  //Z key
    VK_NUMPAD0 = 0x60,  //Numeric keypad 0 key
    VK_NUMPAD1 = 0x61,  //Numeric keypad 1 key
    VK_NUMPAD2 = 0x62,  //Numeric keypad 2 key
    VK_NUMPAD3 = 0x63,  //Numeric keypad 3 key
    VK_NUMPAD4 = 0x64,  //Numeric keypad 4 key
    VK_NUMPAD5 = 0x65,  //Numeric keypad 5 key
    VK_NUMPAD6 = 0x66,  //Numeric keypad 6 key
    VK_NUMPAD7 = 0x67,  //Numeric keypad 7 key
    VK_NUMPAD8 = 0x68,  //Numeric keypad 8 key
    VK_NUMPAD9 = 0x69,  //Numeric keypad 9 key
    VK_SEPARATOR = 0x6C,  //Separator key
    VK_SUBTRACT = 0x6D,  //Subtract key
    VK_DECIMAL = 0x6E,  //Decimal key
    VK_DIVIDE = 0x6F,  //Divide key
    VK_F1 = 0x70,  //F1 key
    VK_F2 = 0x71,  //F2 key
    VK_F3 = 0x72,  //F3 key
    VK_F4 = 0x73,  //F4 key
    VK_F5 = 0x74,  //F5 key
    VK_F6 = 0x75,  //F6 key
    VK_F7 = 0x76,  //F7 key
    VK_F8 = 0x77,  //F8 key
    VK_F9 = 0x78,  //F9 key
    VK_F10 = 0x79,  //F10 key
    VK_F11 = 0x7A,  //F11 key
    VK_F12 = 0x7B,  //F12 key
    VK_SCROLL = 0x91,  //SCROLL LOCK key
    VK_LSHIFT = 0xA0,  //Left SHIFT key
    VK_RSHIFT = 0xA1,  //Right SHIFT key
    VK_LCONTROL = 0xA2,  //Left CONTROL key
    VK_RCONTROL = 0xA3,  //Right CONTROL key
    VK_LMENU = 0xA4,   //Left MENU key
    VK_RMENU = 0xA5,  //Right MENU key
    VK_PLAY = 0xFA,  //Play key
    VK_ZOOM = 0xFB, //Zoom key
}
#endregion
public static class WPFRichTextBoxExtensions
{
    public static void AppendText(this System.Windows.Forms.RichTextBox box, string text, Color color)
    {
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;

        box.SelectionColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        box.AppendText(text);
        box.SelectionColor = box.ForeColor;
        box.AppendText(Environment.NewLine);
    }
    public static void AppendTextToEditor(this TextEditor editor, string text, Color color)
    {
        editor.TextArea.ClearSelection();
        editor.TextArea.TextView.LineTransformers.Add(new LineColorizer(editor.LineCount, new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B))));
        editor.AppendText(text);
        editor.AppendText(Environment.NewLine);
    }
}
class LineColorizer : DocumentColorizingTransformer
{
    public int LineNumber { get; set; }
    public Brush Brush { get; set; }

    public LineColorizer(int lineNumber, Brush brush)
    {
        LineNumber = lineNumber;
        Brush = brush;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (!line.IsDeleted && line.LineNumber == LineNumber)
        {
            ChangeLinePart(line.Offset, line.EndOffset, ApplyChanges);
        }
    }

    void ApplyChanges(VisualLineElement element)
    {
        // This is where you do anything with the line
        element.TextRunProperties.SetForegroundBrush(Brush);
    }
}
