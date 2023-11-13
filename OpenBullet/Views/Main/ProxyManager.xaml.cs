﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Extreme.Net;
using Microsoft.Win32;
using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.Models;
using RuriLib.Runner;

namespace OpenBullet.Views.Main
{
    /// <summary>
    /// Logica di interazione per ProxyManager.xaml
    /// </summary>
    public partial class ProxyManager : Page
    {
        private ProxyManagerViewModel vm = null;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private WorkerStatus Status = WorkerStatus.Idle;
        private CancellationTokenSource cts = new CancellationTokenSource();

        private IEnumerable<CProxy> Selected => proxiesListView.SelectedItems.Cast<CProxy>();

        public ProxyManager()
        {
            vm = SB.ProxyManager;
            DataContext = vm;

            InitializeComponent();
            botsSlider.Maximum = ProxyManagerViewModel.maximumBots;
            vm.RefreshList();
            vm.UpdateProperties();
        }

        #region Start Button
        private async void checkButton_Click(object sender, RoutedEventArgs e)
        {
            switch (Status)
            {
                case WorkerStatus.Idle:
                    SB.Logger.LogInfo(Components.ProxyManager, "Disabling the UI and starting the checker");
                    checkButtonLabel.Text = "ABORT";
                    botsSlider.IsEnabled = false;
                    Status = WorkerStatus.Running;

                    if (SB.Settings.ProxyManagerSettings.ProxySiteUrls.Contains(vm.TestSite) == false)
                    {
                        SB.Settings.ProxyManagerSettings.ProxySiteUrls.Add(vm.TestSite);
                    }

                    if (SB.Settings.ProxyManagerSettings.ProxyKeys.Contains(vm.SuccessKey) == false)
                    {
                        SB.Settings.ProxyManagerSettings.ProxyKeys.Add(vm.SuccessKey);
                    }

                    IOManager.SaveSettings(SB.proxyManagerSettingsFile, SB.Settings.ProxyManagerSettings);

                    var items = vm.OnlyUntested ? vm.Proxies.Where(p => p.Working == ProxyWorking.UNTESTED) : vm.Proxies;

                    // Setup the progress bar
                    progressBar.Value = 0;

                    // Start checking
                    cts = new CancellationTokenSource();

                    try
                    {
                        await Task.Run(async () =>
                        {
                            await vm.CheckAllAsync(items, cts.Token,
                                new Action<ProxyCheckResult<ProxyResult>>(check =>
                                {
                                    if (cts.IsCancellationRequested) return;

                                    var result = check.result;
                                    var proxy = result.proxy;

                                    proxy.LastChecked = DateTime.Now;

                                    if (check.success)
                                    {
                                        // Set all the changed proxy fields
                                        proxy.Working = result.working ? ProxyWorking.YES : ProxyWorking.NO;
                                        proxy.Ping = result.ping;
                                        proxy.Country = result.country;

                                        var infoLog = $"[{DateTime.Now.ToLongTimeString()}] Check for proxy {proxy.Proxy} succeeded in {result.ping} milliseconds.";
                                        SB.Logger.LogInfo(Components.ProxyManager, infoLog);
                                    }
                                    else
                                    {
                                        proxy.Working = ProxyWorking.NO;
                                        proxy.Ping = 0;

                                        var errorLog = $"[{DateTime.Now.ToLongTimeString()}] Check for proxy {proxy.Proxy} failed with error: {check.error}";
                                        SB.Logger.LogError(Components.ProxyManager, errorLog);
                                    }

                                    if (proxy.Working == ProxyWorking.YES)
                                    {
                                        lock (proxy)
                                            vm.Working++;
                                    }
                                    else
                                    {
                                        lock (vm.notWorkingLock)
                                            vm.NotWorking++;
                                    }

                                    lock (vm.testedLock)
                                        vm.Tested++;

                                    // Update the proxy in the database
                                    vm.Update(proxy);
                                }),
                                new Progress<float>(progress =>
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        progressBar.Value = progress;
                                        vm.UpdateProperties();
                                    });
                                }));
                        });
                    }
                    catch
                    {
                        SB.Logger.LogWarning(Components.ProxyManager, "Abort signal received");
                    }
                    // Restore the GUI status
                    finally
                    {
                        checkButtonLabel.Text = "CHECK";
                        botsSlider.IsEnabled = true;
                        Status = WorkerStatus.Idle;
                    }
                    break;

                case WorkerStatus.Running:
                    try { cts.Cancel(); } catch { }
                    try { cts.Dispose(); } catch { }
                    break;
            }
        }
        #endregion

        // TODO: Refactor this function, it shouldn't belong in a view!
        public void AddProxies(IEnumerable<string> raw, ProxyType defaultType = ProxyType.Http, string defaultUsername = "", string defaultPassword = "")
        {
            SB.Logger.LogInfo(Components.ProxyManager, $"Adding {raw.Count()} {defaultType} proxies to the database");

            // Check if they're valid
            var proxies = new List<CProxy>();

            foreach (var p in raw.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList())
            {
                try
                {
                    CProxy proxy = new CProxy().Parse(p, defaultType, defaultUsername, defaultPassword);
                    if (!proxy.IsNumeric || proxy.IsValidNumeric)
                    {
                        proxies.Add(proxy);
                    }
                }
                catch { }
            }

            vm.AddRange(proxies);

            // Refresh
            vm.UpdateProperties();
        }

        private void botsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vm.BotsAmount = (int)e.NewValue;
        }

        #region Buttons
        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File |*.txt";
            sfd.Title = "Export proxies";
            sfd.ShowDialog();

            if (sfd.FileName != string.Empty)
            {
                if (Selected.Count() > 0)
                {
                    SB.Logger.LogInfo(Components.ProxyManager, $"Exporting {proxiesListView.Items.Count} proxies");
                    Selected.SaveToFile(sfd.FileName, p => p.Proxy);
                }
                else
                {
                    MessageBox.Show("No proxies selected!");
                    SB.Logger.LogWarning(Components.ProxyManager, "No proxies selected");
                }
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Logger.LogInfo(Components.ProxyManager, $"Deleting {proxiesListView.SelectedItems.Count} proxies");
            vm.Remove(Selected);
            vm.UpdateProperties();
            SB.Logger.LogInfo(Components.ProxyManager, "Proxies deleted successfully");
        }

        private void deleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Logger.LogWarning(Components.ProxyManager, "Purging all proxies");
            vm.RemoveAll();
            vm.UpdateProperties();
        }

        private void deleteNotWorkingButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Logger.LogInfo(Components.ProxyManager, "Deleting all non working proxies");

            vm.RemoveNotWorking();
            vm.UpdateProperties();
        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogAddProxies(this), "Import Proxies")).ShowDialog();
        }

        private void deleteDuplicatesButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Logger.LogInfo(Components.ProxyManager, "Deleting duplicate proxies");

            vm.RemoveDuplicates();
            vm.UpdateProperties();
        }

        private void DeleteUntestedButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Logger.LogInfo(Components.ProxyManager, "Deleting all untested proxies");

            vm.RemoveUntested();
            vm.UpdateProperties();
        }
        #endregion

        #region ListView
        private void ProxyListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files.Where(x => x.EndsWith(".txt")).ToArray())
                {
                    try
                    {
                        var lines = File.ReadAllLines(file);

                        if ((file.ToLower()).Contains("http"))
                        {
                            AddProxies(lines, ProxyType.Http);
                        }
                        else if ((file.ToLower()).Contains("socks4a"))
                        {
                            AddProxies(lines, ProxyType.Socks4a);
                        }
                        else if ((file.ToLower()).Contains("socks4"))
                        {
                            AddProxies(lines, ProxyType.Socks4);
                        }
                        else if ((file.ToLower()).Contains("socks5"))
                        {
                            AddProxies(lines, ProxyType.Socks5);
                        }
                        else
                        {
                            SB.Logger.LogError(Components.ProxyManager, "Failed to parse proxies type from file name, defaulting to HTTP");
                            AddProxies(lines);
                        }
                    }
                    catch (Exception ex)
                    {
                        SB.Logger.LogError(Components.ProxyManager, $"Failed to open file {file} - {ex.Message}");
                    }
                }
            }
        }

        private void copySelectedProxies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Selected.CopyToClipboard(p => $"{p.Host}:{p.Port}");
                SB.Logger.LogInfo(Components.ProxyManager, $"Copied {Selected.Count()} proxies");
            }
            catch (Exception ex)
            {
                SB.Logger.LogError(Components.ProxyManager, $"Failed to copy proxies - {ex.Message}");
            }
        }

        private void copySelectedProxiesFull_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Selected.CopyToClipboard(p => $"({p.Type}){p.Host}:{p.Port}" + (string.IsNullOrEmpty(p.Username) ? "" : $":{p.Username}:{p.Password}"));
                SB.Logger.LogInfo(Components.ProxyManager, $"Copied {Selected.Count()} proxies");
            }
            catch (Exception ex)
            {
                SB.Logger.LogError(Components.ProxyManager, $"Failed to copy proxies - {ex.Message}");
            }
        }

        private void listViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                proxiesListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            proxiesListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void ListViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void AddProxySiteUrl_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SB.Settings.ProxyManagerSettings.ProxySiteUrls.Contains(vm.TestSite) == false)
            {
                SB.Settings.ProxyManagerSettings.ProxySiteUrls.Add(vm.TestSite);
                IOManager.SaveSettings(SB.proxyManagerSettingsFile, SB.Settings.ProxyManagerSettings);
            }
        }

        private void RemoveProxySiteUrl_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image image && 
                image.Parent is Grid grid && 
                grid.Children.Count == 2 && 
                grid.Children[0] is TextBlock textBlock)
            {
                SB.Settings.ProxyManagerSettings.ProxySiteUrls.Remove(textBlock.Text);
                IOManager.SaveSettings(SB.proxyManagerSettingsFile, SB.Settings.ProxyManagerSettings);
            }
        }

        private void AddProxyKey_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SB.Settings.ProxyManagerSettings.ProxyKeys.Contains(vm.SuccessKey) == false)
            {
                SB.Settings.ProxyManagerSettings.ProxyKeys.Add(vm.SuccessKey);
                IOManager.SaveSettings(SB.proxyManagerSettingsFile, SB.Settings.ProxyManagerSettings);
            }
        }

        private void RemoveProxyKey_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image image &&
                image.Parent is Grid grid &&
                grid.Children.Count == 2 &&
                grid.Children[0] is TextBlock textBlock)
            {
                SB.Settings.ProxyManagerSettings.ProxyKeys.Remove(textBlock.Text);
                IOManager.SaveSettings(SB.proxyManagerSettingsFile, SB.Settings.ProxyManagerSettings);
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Image image)
            {
                image.Width = 25;
                image.Height = 25;
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Image image)
            {
                image.Width = 20;
                image.Height = 20;
            }
        }
        #endregion
    }
}
