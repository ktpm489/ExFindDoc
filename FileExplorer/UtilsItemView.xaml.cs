using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FileExplorer.Models;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FileExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UtilsItemView : Page
    {
        public UtilsItemView()
        {
            this.InitializeComponent();
            Loaded += UtilsItemView_Loaded;
        }

        public async void UtilsItemView_Loaded(object sender, RoutedEventArgs e)
        {
             ComposeEmail("ktpm489@gmail.com", "Hello", null);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            var parameters = (FileItem)e.Parameter;
            Debug.WriteLine(parameters);
        }


        public async void ComposeEmail(string recipient, string contain, Stream attactment)
        {
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.To.Add(new EmailRecipient(recipient));
            emailMessage.Subject = "Backup Password";
            emailMessage.Body = contain;
            if (attactment != null)
            {
                IRandomAccessStream randomAccessStream = attactment.AsRandomAccessStream();
                var streamReference = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromStream(randomAccessStream);
                var Attachment = new Windows.ApplicationModel.Email.EmailAttachment(
                    "sharebyemail.html", streamReference);
                emailMessage.Attachments.Add(Attachment);
            }
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }
    }
}
