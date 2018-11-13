using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FileExplorer.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SimplePdfViewer;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FileExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PdfView : Page, INotifyPropertyChanged
    {
        public PdfView()
        {
             this.Background = new SolidColorBrush(Colors.DarkGray);
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

            private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
           // OpenRemote();
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameters = (FileItem)e.Parameter;
            var uri = new Uri(parameters.StorageItem.Path);
            // Source = uri;
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
            //StorageFile f = await
            //   StorageFile.GetFileFromApplicationUriAsync(uri);
            StorageFile f =  await StorageFile.GetFileFromPathAsync(parameters.StorageItem.Path);
            PdfDocument doc = await PdfDocument.LoadFromFileAsync(f);

            Load(doc);
            // Debug.WriteLine(parameters);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Uri), typeof(PdfView),
                new PropertyMetadata(null, OnSourceChanged));

        public bool IsZoomEnabled
        {
            get { return (bool)GetValue(IsZoomEnabledProperty); }
            set { SetValue(IsZoomEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsZoomEnabledProperty =
            DependencyProperty.Register("IsZoomEnabled", typeof(bool), typeof(PdfView),
                new PropertyMetadata(true, OnIsZoomEnabledChanged));

        internal ZoomMode ZoomMode
        {
            get { return IsZoomEnabled ? ZoomMode.Enabled : ZoomMode.Disabled; }
        }

        public bool AutoLoad { get; set; }

        internal ObservableCollection<BitmapImage> PdfPages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

       

        private static void OnIsZoomEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PdfView)d).OnIsZoomEnabledChanged();
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PdfView)d).OnSourceChanged();
        }

        private void OnIsZoomEnabledChanged()
        {
            OnPropertyChanged(nameof(ZoomMode));
        }

        private void OnSourceChanged()
        {
            if (AutoLoad)
            {
                LoadAsync();
            }
        }

        public async Task LoadAsync()
        {
            if (Source == null)
            {
                PdfPages.Clear();
            }
            else
            {
                if (Source.IsFile || !Source.IsWebUri())
                {
                    await LoadFromLocalAsync();
                }
                else if (Source.IsWebUri())
                {
                    await LoadFromRemoteAsync();
                }
                else
                {
                    throw new ArgumentException($"Source '{Source.ToString()}' could not be recognized!");
                }
            }
        }

        private async Task LoadFromRemoteAsync()
        {
            HttpClient client = new HttpClient();
            var stream = await
                client.GetStreamAsync(Source);
            var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            memStream.Position = 0;
            PdfDocument doc = await PdfDocument.LoadFromStreamAsync(memStream.AsRandomAccessStream());

            Load(doc);
        }

        private async Task LoadFromLocalAsync()
        {
            StorageFile f = await
                StorageFile.GetFileFromApplicationUriAsync(Source);
            PdfDocument doc = await PdfDocument.LoadFromFileAsync(f);

            Load(doc);
        }

        private async void Load(PdfDocument pdfDoc)
        {
            PdfPages.Clear();

            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                BitmapImage image = new BitmapImage();

                var page = pdfDoc.GetPage(i);

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await page.RenderToStreamAsync(stream);
                    await image.SetSourceAsync(stream);
                }

                PdfPages.Add(image);
            }
        }

        public void OnPropertyChanged([CallerMemberName]string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
