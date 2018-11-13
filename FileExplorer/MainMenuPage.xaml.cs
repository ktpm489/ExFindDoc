using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FileExplorer.Models;
using FileExplorer.SortItem;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FileExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenuPage : Page
    {
        private StorageFolder currentFolder = null;
        private ObservableCollection<MenuFolderItem> MenuFolderItems = new ObservableCollection<MenuFolderItem>();
        private ObservableCollection<FileItem> FileItems = new ObservableCollection<FileItem>();
        private FileOpenPicker filePicker = new FileOpenPicker();
        public MainMenuPage()
        {
            this.InitializeComponent();
            //  StorageApplicationPermissions.FutureAccessList.Entries.ToList().ForEach(e => Debug.WriteLine("Metadata: " + e.Metadata + " Token: " + e.Token));
            //  StorageApplicationPermissions.FutureAccessList.Clear();
            Loaded += MainPage_Loaded;
            SetWindowSize();
            InitFilePicker();
        }
        private void SetWindowSize()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(800, 500));
            ApplicationView.PreferredLaunchViewSize = new Size(800, 500);
            ApplicationView.PreferredLaunchWindowingMode =
            ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void InitFilePicker()
        {
            filePicker.ViewMode = PickerViewMode.List;
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            //filePicker.FileTypeFilter.Add(".jpg");
            //filePicker.FileTypeFilter.Add(".jpeg");
            //filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add("*");
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //await ApplicationData.Current.LocalFolder.CreateFolderAsync("Data");
                StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
                await rootFolder.CreateFolderAsync("Data", CreationCollisionOption.FailIfExists);
                await rootFolder.CreateFolderAsync("RecycleBin", CreationCollisionOption.FailIfExists);
                //  StorageApplicationPermissions.FutureAccessList.Add(projectFolder, projectFolder.Path);
                // Debug.WriteLine("Location " +projectFolder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Location Exception" + ex);
            }


        }

        private void MenuButtonMainLeft_Click(object sender, RoutedEventArgs e)
        {
            MenuSplitViewMainLeft.IsPaneOpen = !MenuSplitViewMainLeft.IsPaneOpen;
        }

        private async void FileListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            MenuButtonMainAddFolder.SelectedIndex = -1;
            var fi = e.ClickedItem as FileItem;
            //var storageItem = fi.StorageItem;
            ////TODO change code read here
            //Debug.WriteLine("[FileListView_ItemClick] Clicked on: " + fi.Name);
            //Debug.WriteLine(fi.ToolTipText);
            //if (fi.IsFolder)
            //{
            //    await NavigateToFolder(storageItem as StorageFolder);
            //}
            if (fi.StorageItem.Name.IndexOf(".pdf") > 0)
            {
                Debug.Write("OK it is pdf");
                CoreApplicationView newView = CoreApplication.CreateNewView();
                int newViewId = 0;
                await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    Frame frame = new Frame();


                    frame.Navigate(typeof(PdfView), fi);
                   // frame.Navigate(typeof(UtilsItemView), fi);
                    Window.Current.Content = frame;
                    // You have to activate the window in order to show it later.
                    Window.Current.Activate();

                    newViewId = ApplicationView.GetForCurrentView().Id;
                });
                bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
               
            }
            else if (fi.StorageItem.Name.IndexOf(".mp4") > 0 || fi.StorageItem.Name.IndexOf(".mp3") > 0)
            {
                Debug.Write("OK it is video");
                CoreApplicationView newView = CoreApplication.CreateNewView();
                int newViewId = 0;
                await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    Frame frame = new Frame();


                    frame.Navigate(typeof(Player), fi);
                    // frame.Navigate(typeof(UtilsItemView), fi);
                    Window.Current.Content = frame;
                    // You have to activate the window in order to show it later.
                    Window.Current.Activate();

                    newViewId = ApplicationView.GetForCurrentView().Id;
                });
                bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);

            }
            else
            {
                //  frame.Navigate(typeof(UtilsItemView), fi);
                var storageItem = fi.StorageItem;
                if (storageItem.IsOfType(StorageItemTypes.File))
                {
                    var storageFile = storageItem as StorageFile;
                    bool success = await Launcher.LaunchFileAsync(storageFile);
                    Debug.WriteLine("file", storageFile);
                    if (!success)
                    {
                        success = await Launcher.LaunchFileAsync(storageFile, OpenWithLaucherOptions);
                    }
                    Debug.WriteLineIf(!success, "Launching the file failed");
                }
            }



          
        }

        private async void MenuButtonMainAddFolder_ItemClick(object sender, ItemClickEventArgs e)
        {
            MenuButtonMainAddFolder.SelectedIndex = -1;
            MenuListViewFolders.SelectedIndex = -1;
            IReadOnlyList<StorageFile> fileList = await filePicker.PickMultipleFilesAsync();

            if (fileList != null && fileList.Count > 0)
            {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Data");

                foreach (var item in fileList)
                {

                    StorageFile newFile = await item.CopyAsync(folder, item.Name, NameCollisionOption.GenerateUniqueName);
                }
                StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
                await NavigateToFolder(await rootFolder.GetFolderAsync("Data"));
                await new MessageDialog("Done.. !!! ^^").ShowAsync();
            }


            //var folderPicker = new FolderPicker();
            //folderPicker.FileTypeFilter.Add("*");
            //folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            //folderPicker.ViewMode = PickerViewMode.List;
            //var folder = await folderPicker.PickSingleFolderAsync();

            //var filderPicker = new FileOpenPicker();

            //if (folder != null)
            //{
            //    StorageApplicationPermissions.FutureAccessList.Add(folder, folder.Path);
            //    Debug.WriteLine("Opened the folder: " + folder.DisplayName);
            //    MenuFolderItems.Add(new MenuFolderItem(folder));
            //}
            //MenuButtonMainAddFolder.SelectedIndex = -1;
        }

        private void MenuListViewItemRemove_Click(object sender, RoutedEventArgs e)
        {
            var source = e.OriginalSource as MenuFlyoutItem;
            Debug.WriteLine("source.Tag: " + source.Tag);
            var storageFolder = source.Tag as StorageFolder;
            var f = MenuFolderItems.ToList().Find(item => item.Folder == storageFolder);
            //var entry = StorageApplicationPermissions.FutureAccessList.Entries.ToList().Find(item => item.Metadata == f.Folder.Path);
            //StorageApplicationPermissions.FutureAccessList.Remove(entry.Token);
            MenuFolderItems.Remove(f);
            //  Debug.WriteLine("Removed FolderItem with Name: " + entry.Metadata + " Token: " + entry.Token);
        }

        private async void MenuListViewFolders_Loaded(object sender, RoutedEventArgs e)
        {
            /* foreach (var entry in StorageApplicationPermissions.FutureAccessList.Entries.OrderBy(item => item.Metadata))
             {
                 var folder = await StorageFolder.GetFolderFromPathAsync(entry.Metadata);
                 StorageApplicationPermissions.FutureAccessList.Add(folder, folder.Path);
                 Debug.WriteLine("Opened the folder: " + folder.DisplayName);
                 MenuFolderItems.Add(new MenuFolderItem(folder));
             } */

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFolder> allFolders = await localFolder.GetFoldersAsync();
            foreach (var folder in allFolders)
            {
                MenuFolderItems.Add(new MenuFolderItem(folder));
            }
            //  StorageFolder folder = await localFolder.GetFolderAsync("Data");
            //StorageApplicationPermissions.FutureAccessList.Add(folder, folder.Path);
            // Debug.WriteLine("Opened the folder: " + folder.DisplayName);
            //MenuFolderItems.Add(new MenuFolderItem(folder));

        }

        private async void MenuListViewFolders_ItemClick(object sender, ItemClickEventArgs e)
        {
            MenuButtonMainAddFolder.SelectedIndex = -1;
            var f = e.ClickedItem as MenuFolderItem;
            FileItems.Clear();
            Debug.WriteLine("[MenuListViewFolders_ItemClick] Clicked on: " + f.DisplayName);
            await NavigateToFolder(f.Folder);
        }

        private async void FolderUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentFolder != null)
            {
                var parentFolder = await this.currentFolder.GetParentAsync();
                await NavigateToFolder(parentFolder);
            }
            else
            {
                Debug.WriteLine("There was no currentFolder");
            }
        }

        private async void UpdateCurrentFolderPathPanel()
        {
            if (this.currentFolder != null)
            {
                CurrentFolderPathPanel.Children.Clear();

                var folder = this.currentFolder;
                var parts = new List<StorageFolder>();
                parts.Add(folder);

                try
                {
                    while ((folder = await folder.GetParentAsync()) != null)
                    {
                        parts.Add(folder);
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("You don't have access permissions to this parent folder!");
                }

                parts.Reverse();
                CurrentFolderPathPanel.Children.Add(BuildCurrentFolderPathButton(parts.ElementAt(0)));
                for (int i = 1; i < parts.Count; i++)
                {
                    CurrentFolderPathPanel.Children.Add(BuildCurrentFolderPathSeperator());
                    CurrentFolderPathPanel.Children.Add(BuildCurrentFolderPathButton(parts.ElementAt(i)));
                }
            }
        }

        private Button BuildCurrentFolderPathButton(StorageFolder folder)
        {
            var btn = new Button();
            btn.Content = folder.Name.TrimEnd('\\');
            btn.Tag = folder;
            btn.FontSize = 20;
            btn.FontWeight = FontWeights.SemiBold;
            btn.Background = new SolidColorBrush(Colors.Transparent);
            btn.VerticalAlignment = VerticalAlignment.Stretch;
            btn.BorderThickness = new Thickness();
            btn.Click += NavigateTo_Click;
            return btn;
        }

        private async void NavigateTo_Click(object sender, RoutedEventArgs e)
        {
            await NavigateToFolder((e.OriginalSource as Button).Tag as StorageFolder);
        }

        private TextBlock BuildCurrentFolderPathSeperator()
        {
            var tb = new TextBlock();
            tb.FontFamily = new FontFamily("Segoe MDL2 Assets");
            tb.Text = "\xE937";
            tb.FontSize = 12;
            tb.Padding = new Thickness(4, 4, 0, 0);
            tb.VerticalAlignment = VerticalAlignment.Center;
            return tb;
        }

        private void CommandBar_Opening(object sender, object e)
        {
            FolderUpButton.LabelPosition = CommandBarLabelPosition.Default;
        }

        private void CommandBar_Closing(object sender, object e)
        {
            FolderUpButton.LabelPosition = CommandBarLabelPosition.Collapsed;
        }

        private void MenuSplitViewMainLeft_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 640)
            {
                MenuSplitViewMainLeft.DisplayMode = SplitViewDisplayMode.CompactOverlay;
            }
            else
            {
                MenuSplitViewMainLeft.DisplayMode = SplitViewDisplayMode.CompactInline;
            }
        }

        private async void RefreshFolderButton_Click(object sender, RoutedEventArgs e)
        {
          //  search.Focus(FocusState.Pointer);
            await NavigateToFolder(this.currentFolder);
        }

        private async Task NavigateToFolder(StorageFolder folder, Boolean isLoading = true)
        {
            MenuButtonMainAddFolder.SelectedIndex = -1;
            FileItems.Clear();
            // FileItemOpen.text = "Restore";
            if (folder != null)
            {
                var folderItems = await folder.GetItemsAsync();
                foreach (var folderItem in folderItems)
                {
                    var fileItem = new FileItem(folderItem);
                    await fileItem.FetchProperties();
                    FileItems.Add(fileItem);
                }


            }
            else
            {
                Debug.WriteLine("folder was null");
            }
            if (isLoading) this.currentFolder = folder;
            UpdateCurrentFolderPathPanel();
        }

        private void ToggleViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileItemsListView.Visibility == Visibility.Visible)
            {
                ToggleViewButton.Icon = new SymbolIcon(Symbol.List);
                FileItemsListView.Visibility = Visibility.Collapsed;
                FileItemsGridView.Visibility = Visibility.Visible;
            }
            else
            {
                ToggleViewButton.Icon = new SymbolIcon(Symbol.ViewAll);
                FileItemsListView.Visibility = Visibility.Visible;
                FileItemsGridView.Visibility = Visibility.Collapsed;
            }
        }

        private static readonly LauncherOptions OpenWithLaucherOptions = new LauncherOptions() { DisplayApplicationPicker = true };
        private async void FileItemOpen_Click(object sender, RoutedEventArgs e)
        {
            var source = e.OriginalSource as MenuFlyoutItem;
            var storageItem = source.Tag as IStorageItem;
            Debug.WriteLine("storageItem = " + storageItem);
            if (storageItem.IsOfType(StorageItemTypes.File))
            {
                var storageFile = storageItem as StorageFile;
                bool success = await Launcher.LaunchFileAsync(storageFile);
                Debug.WriteLine("file", storageFile);
                if (!success)
                {
                    success = await Launcher.LaunchFileAsync(storageFile, OpenWithLaucherOptions);
                }
                Debug.WriteLineIf(!success, "Launching the file failed");
            }
            else
            {
                await NavigateToFolder(storageItem as StorageFolder);
            }
        }

        private async void FileItemMoveRecycle_Click(object sender, RoutedEventArgs e)
        {
            var source = e.OriginalSource as MenuFlyoutItem;
            var storageItem = source.Tag as IStorageItem;
            Debug.WriteLine("storageItem = " + storageItem);
            if (storageItem.IsOfType(StorageItemTypes.File))
            {

                var storageFile = storageItem as StorageFile;
                //  https://www.c-sharpcorner.com/UploadFile/2b876a/how-to-use-folders-and-files-in-windows-phone-8/
                if (this.currentFolder.DisplayName == "Data")
                {
                    var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("RecycleBin");
                    await storageFile.MoveAsync(folder);
                    await NavigateToFolder(this.currentFolder);
                }
                else
                {
                    await storageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    await NavigateToFolder(this.currentFolder);
                }
                // var file = await ApplicationData.Current.LocalFolder.GetFileAsync("FileName.txt");




                //bool success = await Launcher.LaunchFileAsync(storageFile);
                //Debug.WriteLine("file", storageFile);
                //if (!success)
                //{
                //    success = await Launcher.LaunchFileAsync(storageFile, OpenWithLaucherOptions);
                //}
                //Debug.WriteLineIf(!success, "Launching the file failed");
            }
        }


        private async void FileItemExport_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.ViewMode = PickerViewMode.List;
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                var source = e.OriginalSource as MenuFlyoutItem;
                var storageItem = source.Tag as IStorageItem;
                Debug.WriteLine("storageItem = " + storageItem);
                if (storageItem.IsOfType(StorageItemTypes.File))
                {

                    try
                    {
                        var storageFile = storageItem as StorageFile;
                        await storageFile.CopyAsync(folder);
                        await new MessageDialog("Done.. !!! ^^").ShowAsync();
                    }
                    catch (Exception)
                    {
                        await new MessageDialog("Please try again ^_^").ShowAsync();
                    }

                }

            }



        }




        private async void FileItemMove_Click(object sender, RoutedEventArgs e)
        {
            var source = e.OriginalSource as MenuFlyoutItem;
            var storageItem = source.Tag as IStorageItem;
            Debug.WriteLine("storageItem = " + storageItem);
            if (storageItem.IsOfType(StorageItemTypes.File))
            {

                var storageFile = storageItem as StorageFile;
                //  https://www.c-sharpcorner.com/UploadFile/2b876a/how-to-use-folders-and-files-in-windows-phone-8/
                if (this.currentFolder.DisplayName == "RecycleBin")
                {
                    var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Data");
                    await storageFile.MoveAsync(folder);
                    await NavigateToFolder(this.currentFolder);
                }
            }
        }


        private async Task SortFolder(StorageFolder folder, int type = 1, string sortName = "")
        {
            MenuButtonMainAddFolder.SelectedIndex = -1;
            FileItems.Clear();
            // search.QueryText = "";
            // FileItemOpen.text = "Restore";
            if (folder != null)
            {
                var folderItems = await folder.GetItemsAsync();
                // type = 1 : sort by data created
                // type = 2 : sort by name
                // type = 3: contains data
                if (type == 1)
                {
                    foreach (var folderItem in folderItems.OrderBy(a => a.DateCreated))
                    {
                        var fileItem = new FileItem(folderItem);
                        await fileItem.FetchProperties();
                        FileItems.Add(fileItem);
                    }
                }
                else if (type == 3 && sortName.Length > 0)
                {
                    foreach (var folderItem in folderItems)
                    {
                        if (folderItem.Name.ToLower().Contains(sortName.ToLower()))
                        {
                            var fileItem = new FileItem(folderItem);
                            await fileItem.FetchProperties();
                            FileItems.Add(fileItem);
                        }
                    }
                }
                else
                {
                    foreach (var folderItem in folderItems.OrderBy(a => a.Name))
                    {
                        var fileItem = new FileItem(folderItem);
                        await fileItem.FetchProperties();
                        FileItems.Add(fileItem);
                    }
                }



            }
            else
            {
                Debug.WriteLine("folder was null");
            }
            this.currentFolder = folder;
            UpdateCurrentFolderPathPanel();
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxItem = e.AddedItems[0] as ComboBoxItem;
            if (comboBoxItem == null) return;
            var content = comboBoxItem.Content as string;
            Debug.WriteLine("Change content", content);
            if (content != null && this.currentFolder != null)
            {
                if (content.Equals("Name"))
                {
                    await this.SortFolder(this.currentFolder);
                }
                else if (content.Equals("Date Created"))
                {
                    await this.SortFolder(this.currentFolder, 2);
                }
                //do what ever you want
            }
        }

        private async Task<string> InputTextDialogAsync(string title = "Change file name ", string btn1 = "OK", string btn2 = "Cancel")
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = btn1;
            dialog.SecondaryButtonText = btn2;
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                return inputTextBox.Text;
            }
            return "";
        }


        private async void FileItemRenameFile(object sender, RoutedEventArgs e)
        {
            var source = e.OriginalSource as MenuFlyoutItem;
            var storageItem = source.Tag as IStorageItem;
            Debug.WriteLine("storageItem = " + storageItem);
            if (storageItem.IsOfType(StorageItemTypes.File))
            {

                var storageFile = storageItem as StorageFile;
                string text = await InputTextDialogAsync();
                if (text.Length > 0)
                {
                    try
                    {
                        await storageFile.RenameAsync(text + storageFile.FileType);
                        await NavigateToFolder(this.currentFolder);
                        // await new MessageDialog("Change name successfully!").ShowAsync();
                    }
                    catch (Exception)
                    {
                        await new MessageDialog("Please try again!").ShowAsync();
                    }


                }

            }
        }

        private void search_QueryChanged(SearchBox sender, SearchBoxQueryChangedEventArgs args)
        {
            //   string textQuery = search.QueryText.ToUpper();
            //      Debug.WriteLine("string query", textQuery);

        }
        private async void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            string textQuery = search.QueryText.ToUpper();

            if (textQuery.Length > 0)
            {
                await SortFolder(this.currentFolder, 3, textQuery);
                //  await Task.Delay(600);
                // search.QueryText = null;
                // search.Focus(FocusState.Pointer);

                //   search.Focus(FocusState.Unfocused);

            }
            else
            {
                await NavigateToFolder(this.currentFolder);
            }
          //  search.Focus(FocusState.Programmatic);
            search.QueryText = string.Empty;
            search.SearchHistoryEnabled = false;
            search.SearchHistoryContext = string.Empty;

            //  Debug.WriteLine("SearchBox_QuerySubmitted", textQuery);
        }

    }
}

