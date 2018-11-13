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

using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Imaging;
using FileExplorer.Models;
using Windows.UI.Core.Preview;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FileExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Player : Page
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private MediaTimelineController mediaTimelineController = new MediaTimelineController();
        private TimeSpan duration;
        private DispatcherTimer timer;
        private SystemMediaTransportControls systemControls;

        public Player()
        {
            this.InitializeComponent();
            systemControls = mediaPlayer.SystemMediaTransportControls;
            mediaPlayer.CommandManager.IsEnabled = false;

            // Register to handle the following system transpot control buttons.
            systemControls.ButtonPressed += SystemControls_ButtonPressed;

            //  mediaPlayer.CurrentStateChanged += MediaElement_CurrentStateChanged;


            systemControls.IsPlayEnabled = true;
            systemControls.IsPauseEnabled = true;


            myMediaPlayer.SetMediaPlayer(mediaPlayer);
            var mediaSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/梨花雨.mp4"));
            mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
            myMediaPlayer.Source = mediaSource;
            mediaPlayer.CommandManager.IsEnabled = false;
            mediaPlayer.TimelineController = mediaTimelineController;
            //SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
            Window.Current.Closed += (ss, ee) =>
            {
                stopMediaPlayer();
            };
            Window.Current.Activated += (s, e) =>
            {
                startOrPause(null, null);
            };
            
        }
       

        protected override  void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameters = (FileItem)e.Parameter;
            var uri = new Uri(parameters.StorageItem.Path);
            var mediaSource = MediaSource.CreateFromUri(new Uri(parameters.StorageItem.Path));
            mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
            myMediaPlayer.Source = mediaSource;

            // Source = uri;
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
            //StorageFile f = await
            //   StorageFile.GetFileFromApplicationUriAsync(uri);

            // Debug.WriteLine(parameters);
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            Application.Current.Exit();
            base.OnNavigatedFrom(e);
            stopMediaPlayer();
        }

      
        async void SystemControls_ButtonPressed(SystemMediaTransportControls sender,
               SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        mediaTimelineController.Start();
                    });
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        mediaTimelineController.Pause();
                    });
                    break;
                default:
                    break;
            }
        }


        private async void MediaSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            duration = sender.Duration.GetValueOrDefault();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                timeLine.Minimum = 0;
                timeLine.Maximum = duration.TotalSeconds;
                timeLine.StepFrequency = 0.5;
            });
        }
        private void startOrPause(object sender, RoutedEventArgs e)
        {
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += timer_Tick;
                timer.Start();
                EllStoryboard.Begin();
                mediaTimelineController.Start();
                var pauseIcon = new SymbolIcon(Symbol.Pause);
                startAndPauseButton.Icon = pauseIcon;
                startAndPauseButton.Label = "Pause";

            }
            else
            {
                if (mediaTimelineController.State == MediaTimelineControllerState.Running)
                {

                    EllStoryboard.Pause();
                    mediaTimelineController.Pause();
                    var playIcon = new SymbolIcon(Symbol.Play);
                    startAndPauseButton.Icon = playIcon;
                    startAndPauseButton.Label = "Play";
                }
                else
                {
                    EllStoryboard.Begin();
                    mediaTimelineController.Resume();
                    var pauseIcon = new SymbolIcon(Symbol.Pause);
                    startAndPauseButton.Icon = pauseIcon;
                    startAndPauseButton.Label = "Pause";
                }
            }

        }

        private void timer_Tick(object sender, object e)
        {
            timeLine.Value = ((TimeSpan)mediaTimelineController.Position).TotalSeconds;
            if (timeLine.Value == timeLine.Maximum)
            {
                mediaTimelineController.Position = TimeSpan.FromSeconds(0);
                mediaTimelineController.Pause();
            }
        }
        private void stop_Click(object sender, RoutedEventArgs e)
        {
            stopMediaPlayer();
        }

        private void stopMediaPlayer()
        {

            EllStoryboard.Stop();
            mediaTimelineController.Position = TimeSpan.FromSeconds(0);
            mediaTimelineController.Pause();
            var playIcon = new SymbolIcon(Symbol.Play);
            startAndPauseButton.Icon = playIcon;
            startAndPauseButton.Label = "Play";
        }
        private async void add_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".mp3");
            fileOpenPicker.FileTypeFilter.Add(".mp4");
            fileOpenPicker.FileTypeFilter.Add(".wmv");
            fileOpenPicker.FileTypeFilter.Add(".wma");
            StorageFile file = await fileOpenPicker.PickSingleFileAsync();
            if (file != null)
            {
                myMediaPlayer.Source = MediaSource.CreateFromStorageFile(file);
                if (file.FileType == ".mp3" || file.FileType == ".wma")
                {
                    myMusicPlayer.Visibility = Visibility.Visible;
                }
                else
                {
                    myMusicPlayer.Visibility = Visibility.Collapsed;
                }
            }
            stopMediaPlayer();
        }

        private void ChangeMediaVolume(object sender, RangeBaseValueChangedEventArgs e)
        {
            mediaPlayer.Volume = (double)volumeSlider.Value;
        }

        private void display_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool isInFullScreenMode = view.IsFullScreenMode;
            if (isInFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }

        }
    }
}
