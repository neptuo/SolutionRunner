﻿using Neptuo;
using Neptuo.Productivity.SolutionRunner.Services;
using Neptuo.Productivity.SolutionRunner.Services.Configuration;
using Neptuo.Productivity.SolutionRunner.Services.Logging;
using Neptuo.Productivity.SolutionRunner.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EventManager = Neptuo.Productivity.SolutionRunner.ViewModels.EventManager;

namespace Neptuo.Productivity.SolutionRunner.Views
{
    public partial class ConfigurationWindow : Window
    {
        private readonly INavigator navigator;
        private readonly ILogProvider logProvider;
        private readonly ISettingsFactory settingsFactory;
        private readonly ISettingsMapper settingsMapper;

        public bool IsSaveRequired { get; private set; }

        public ConfigurationViewModel ViewModel
        {
            get { return (ConfigurationViewModel)DataContext; }
            set { DataContext = value; }
        }

        public ConfigurationWindow(ConfigurationViewModel viewModel, INavigator navigator, ILogProvider logProvider, ISettingsFactory settingsFactory, ISettingsMapper settingsMapper, bool isSaveRequired)
        {
            Ensure.NotNull(viewModel, "viewModel");
            Ensure.NotNull(navigator, "navigator");
            Ensure.NotNull(logProvider, "logProvider");
            Ensure.NotNull(settingsFactory, "settingsFactory");
            Ensure.NotNull(settingsMapper, "settingsMapper");
            ViewModel = viewModel;
            IsSaveRequired = isSaveRequired;
            this.navigator = navigator;
            this.logProvider = logProvider;
            this.settingsFactory = settingsFactory;
            this.settingsMapper = settingsMapper;

            InitializeComponent();
            EventManager.ConfigurationSaved += OnConfigurationSaved;

            if (isSaveRequired)
                btnCancel.Content = "Exit";
        }

        private void OnConfigurationSaved(ConfigurationViewModel viewModel)
        {
            EventManager.ConfigurationSaved -= OnConfigurationSaved;
            IsSaveRequired = false;
            Close();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            btnErrorLog.IsEnabled = logProvider.GetFileNames().Any();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            //tbxSourceDirectory.Focus();
            dbrSourceDirectory.Focus();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (IsSaveRequired)
            {
                string message = "Source root directory is not set. Terminate the application?";
                string caption = "Configuration";
                e.Cancel = MessageBox.Show(this, message, caption, MessageBoxButton.YesNo) == MessageBoxResult.No;
                return;
            }

            navigator.OpenMain();
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.neptuo.com/project/desktop/solutionrunner");
        }

        private void btnViewStatistics_Click(object sender, RoutedEventArgs e)
        {
            navigator.OpenStatistics();
        }
        
        private void btnErrorLog_Click(object sender, RoutedEventArgs e)
        {
            string fileName = logProvider.GetFileNames()
                .OrderBy(f => f)
                .LastOrDefault();

            if (fileName == null)
            {
                MessageBox.Show("No errors");
                return;
            }

            string log = logProvider.FindFileContent(fileName);
            if (log == null)
            {
                MessageBox.Show("No errors");
                return;
            }

            if (log.Length > 800)
                log = log.Substring(0, 800);

            MessageBox.Show(log);
        }

        private async void btnImportConfiguration_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true
            };

            string path = ViewModel.ConfigurationPath;
            if (String.IsNullOrEmpty(path))
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            }
            else
            {
                dialog.FileName = System.IO.Path.GetFileNameWithoutExtension(path);
                dialog.DefaultExt = System.IO.Path.GetExtension(path);
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(path);
                dialog.Filter = "Configuration File (*.json)|*.json";
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = dialog.FileName;

                ISettingsService target = settingsFactory.CreateForFile(path);
                await target.LoadAsync();

                settingsMapper.Map()
            }
        }

        private void btnTodayBackup_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
