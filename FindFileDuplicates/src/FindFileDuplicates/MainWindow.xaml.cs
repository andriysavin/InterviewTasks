/*
 * Copyright (c) 2026 Andriy Savin
 *
 * This code is licensed under the MIT License.
 * See the LICENSE file in the repository root for full license text.
 * 
 * Attribution is appreciated when reusing this code.
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace FindFileDuplicates
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FindDuplicatesApplication application;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            pathTextBox.Text = dialog.SelectedPath;

            StartSearchingDuplicates(dialog.SelectedPath);
        }

        private async void StartSearchingDuplicates(string pathToSearch)
        {
            SetBuildIndexStartedState();

            var progress = new Progress<int>(OnProgressReported);

            var comparisonBytesCount = GetComparisonBytesCount();

            application = new FindDuplicatesApplication(0, comparisonBytesCount);

            await application.BuildFilesIndexAsync(pathToSearch, progress);

            DisplayHashesStatistics();

            SetLookingForDuplicatesState();

            DisplayDuplicates(application.EnumerateDuplicates());

            SetFinishedState();
        }

        private long GetComparisonBytesCount()
        {
            long comparisonBytesCount;
            
            if (!long.TryParse(comparisonBytesCountTextBox.Text, out comparisonBytesCount))
            {
                // Reset to default if cant parse
                comparisonBytesCountTextBox.Text = comparisonBytesCount.ToString();
            }

            return comparisonBytesCount;
        }

        private void OnProgressReported(int filesIndexed)
        {
            filesIndexedTextBlock.Text = filesIndexed.ToString();
            DisplayHashesStatistics();
        }

        private void DisplayHashesStatistics()
        {
            hashesComputedTextBlock.Text =
                application.GetHashesComputedCount().ToString();
        }

        private void DisplayDuplicates(IEnumerable<FileInfo[]> duplicates)
        {
            duplicatesList.ItemsSource = duplicates;
        }

        private void SetFinishedState()
        {
            progressBar.Visibility = Visibility.Collapsed;
            chooseFolderButton.IsEnabled = true;
            statusTextBlock.Text = "Finished!";
        }

        private void SetLookingForDuplicatesState()
        {
            statusTextBlock.Text = "Looking for duplicates...";
        }

        private void SetBuildIndexStartedState()
        {
            duplicatesList.ItemsSource = null;
            progressBar.Visibility = Visibility.Visible;
            chooseFolderButton.IsEnabled = false;
            statusTextBlock.Text = "Building files index...";
        }
    }
}
