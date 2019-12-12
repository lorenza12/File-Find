using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace File_Find
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Searcher searcher = new Searcher();

        public MainWindow()
        {
            InitializeComponent();

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                //If the application is started with a directory argument, fill in the directory textbox
                directory_txtbx.Text = Environment.GetCommandLineArgs()[1].ToString();
            }

            //Set the default file-type as All Files
            fileType_cmbx.SelectedIndex = 4;
            EnforceStipulations();

        }

        private void browse_btn_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    directory_txtbx.Text = fbd.SelectedPath.ToString();

                    string[] files = Directory.GetFiles(fbd.SelectedPath);

                    searcher.SearchDirectory = fbd.SelectedPath.ToString();

                }

            }

            Enable_Search_Btn();
        }

        private void search_txtbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            Enable_Search_Btn();

        }

        private void fileType_cmbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            searcher.FileType = searcher.GetFileType((e.AddedItems[0] as ComboBoxItem).Content.ToString());

            EnforceStipulations();
            Enable_Search_Btn();

        }

        private void Enable_Search_Btn()
        {

            search_btn.IsEnabled = ((directory_txtbx.Text != null && directory_txtbx.Text.Length > 0) &&
                                    (search_txtbx.Text != null && search_txtbx.Text.Length > 0) &&
                                    (fileType_cmbx.SelectedItem != null && fileType_cmbx.SelectedItem.ToString().Length > 0));

        }

        private void directory_txtbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (directory_txtbx.Text.Length > 0)
            {
                searcher.SearchDirectory = directory_txtbx.Text.Trim();
            }

            Enable_Search_Btn();

        }

        private async void search_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EnforceStipulations();


                statusError_lbl.Content = "";
                loading_prgbar.IsIndeterminate = true;
                loading_prgbar.Visibility = Visibility.Visible;

                SetSearcherCheckboxes();

                searcher.SearchWord = search_txtbx.Text.Trim();

                await Task.Run(() => searcher.SearchForFiles());

                //After trying to search, set any error messages
                //SetErrorMessage(searcher.errorMessage);
                statusError_lbl.Content = searcher.errorMessage;

                foundFiles_lstbx.Items.Clear();

                if (searcher.FoundFiles != null && searcher.FoundFiles.Length > 0)
                {
                    foreach (string file in searcher.FoundFiles)
                    {
                        if (file != null)
                        {
                            foundFiles_lstbx.Items.Add(file.ToString());
                        }
                    }

                }

                fileFoundCount_lbl.Content = "Files Found: " + searcher.FoundFiles.Length.ToString();
                loading_prgbar.IsIndeterminate = false;
                loading_prgbar.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                loading_prgbar.IsIndeterminate = true;
                loading_prgbar.Visibility = Visibility.Visible;
                statusError_lbl.Content = "Error: " + ex.ToString();
            }
        }

        private void EnforceStipulations()
        {
            if (searcher.FileType == ".csv")
            {
                matchWholeWord_chbx.IsEnabled = false;
                matchWholeWord_chbx.IsChecked = false;
                wholeWord_lbl.IsEnabled = false;
                statusError_lbl.Content = "Note: Match whole word functionality doesn't properly work on csv formats";
            }

            else if (searcher.FileType == ".*" && (findInFiles_chbx.IsChecked ?? false))
            {

                statusError_lbl.Content = "Note: Find in files functionality doesn't work on all file types";

                //if we switched from csv to all files
                matchWholeWord_chbx.IsEnabled = true;
                wholeWord_lbl.IsEnabled = true;
            }
            else
            {

                matchWholeWord_chbx.IsEnabled = true;
                wholeWord_lbl.IsEnabled = true;
                statusError_lbl.Content = "";
            }
        }

        private void SetSearcherCheckboxes()
        {
            if (navSubDirs_chbx.IsChecked ?? false)
            {
                searcher.NavSubDirectories = true;
            }
            else
            {
                searcher.NavSubDirectories = false;
            }

            if (matchWholeWord_chbx.IsChecked ?? false)
            {
                searcher.MatchWholeWord = true;
            }
            else
            {
                searcher.MatchWholeWord = false;
            }

            if (findInFiles_chbx.IsChecked ?? false)
            {
                searcher.FindInFiles = true;
            }
            else
            {
                searcher.FindInFiles = false;
            }

            if (matchCase_chbx.IsChecked ?? false)
            {
                searcher.MatchCase = true;
            }
            else
            {
                searcher.MatchCase = false;
            }
            if (includeDir_chbx.IsChecked ?? false)
            {
                searcher.IncludeDirectories = true;
            }
            else
            {
                searcher.IncludeDirectories = false;
            }
        }

        private void CheckEnterHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key == Key.Return || e.Key == Key.Enter) && search_btn.IsEnabled)
            {
                search_btn_Click(sender, e);
            }

        }

        private void FoundFiles_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem lbi = (ListBoxItem)(foundFiles_lstbx.ItemContainerGenerator.ContainerFromIndex(foundFiles_lstbx.SelectedIndex));

            try
            {
                Process.Start((lbi.Content.ToString()));
                statusError_lbl.Content = "";
            }
            catch (Exception ex)
            {
                statusError_lbl.Content = "Error: Couldn't Open File";

            }
        }

        private void Chbx_Checked(object sender, RoutedEventArgs e)
        {
            EnforceStipulations();
        }

        private void OpenDirectoryContextMenu_OnClick(object sender, RoutedEventArgs e)
        {

            string errorMsg = "";

            foreach (var item in foundFiles_lstbx.SelectedItems)
            {
                try
                {
                    string dir = item.ToString().Substring(0, item.ToString().LastIndexOf('\\'));
                    Process.Start(dir);

                }
                catch (Exception ex)
                {
                    errorMsg += " " + item.ToString();

                }
            }
            if (errorMsg.Length > 0)
            {
                statusError_lbl.Content = $"Error: Couldn't Open Directory {errorMsg}";

            }
            else
            {
                statusError_lbl.Content = "";

            }

        }

        private void OpenFileContextMenu_OnClick(object sender, RoutedEventArgs e)
        {

            string errorMsg = "";

            foreach (var item in foundFiles_lstbx.SelectedItems)
            {
                try
                {
                    Process.Start((item.ToString()));

                }
                catch (Exception ex)
                {
                    errorMsg += " " + item.ToString();

                }
            }
            if (errorMsg.Length > 0)
            {
                statusError_lbl.Content = $"Error: Couldn't Open File(s) {errorMsg}";

            }
            else
            {
                statusError_lbl.Content = "";

            }

        }
        private void CopyDirectoryContextMenu_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItems = foundFiles_lstbx.SelectedItems;
            string clipboard = "";

            foreach (var item in selectedItems)
            {
                //Don't add newline character if its the last item
                if (selectedItems.IndexOf(item) == selectedItems.Count - 1)
                {
                    clipboard += item.ToString();
                }
                else
                {
                    clipboard += item.ToString() + System.Environment.NewLine;
                }

            }

            if (!string.IsNullOrEmpty(clipboard))
            {
                System.Windows.Clipboard.SetText(clipboard);
            }
        }

        private void FoundFilesCntxtmnu_open(object sender, ContextMenuEventArgs e)
        {
            bool itemSelected = foundFiles_lstbx.SelectedItems.Count > 0;

            openDirectory_contextMenu.IsEnabled = itemSelected;
            copyDirectory_contextMenu.IsEnabled = itemSelected;
            openFile_contextMenu.IsEnabled = itemSelected;

        }

        private void searchTxtbx_drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    string searchText = string.Empty;

                    foreach (var path in files)
                    {
                        if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                        {
                            string directory = Path.GetFileName(path);
                            searchText += $"{directory},";
                        }
                        else
                        {
                            string fileName = Path.GetFileNameWithoutExtension(path);
                            searchText += $"{fileName},";
                        }
                    }

                    ((System.Windows.Controls.TextBox)sender).Text = searchText.Trim(',');
                }
            }
        }

        private void directoryTxtbx_drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    ((System.Windows.Controls.TextBox)sender).Text = files[0];
                }
            }
        }

        private void directoryTxtbx_previwDrop(object sender, System.Windows.DragEventArgs e)
        {
            //Only allow one directory to be dropped into the directory box
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
                if (files != null && files.Length == 1)
                {
                    if (File.GetAttributes(files[0]).HasFlag(FileAttributes.Directory))
                    {
                        e.Handled = true;
                    }
                }
            }

        }

        private void searchTxtbx_previwDrop(object sender, System.Windows.DragEventArgs e)
        {
            //Any file or directory can be dropped into the search textbox
            e.Handled = true;
        }
    }
}
