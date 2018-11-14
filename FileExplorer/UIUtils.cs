using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
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
using Windows.Storage;
using Windows.Services.Store;
using Newtonsoft.Json.Linq;

namespace FileExplorer
{
   public sealed  class UIUtils
    {
        public static async Task<string> InputTextDialogAsync(string title) {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "OK";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                return inputTextBox.Text;
            }
            else return "";

        }
        public static async void ComposeEmail(string recipient, string contain, Stream attactment)
        {
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.To.Add(new EmailRecipient(recipient));
            emailMessage.Subject = "Backup Password";
            emailMessage.Body = contain;
            //if (attactment != null)
            //{
            //    IRandomAccessStream randomAccessStream = attactment.AsRandomAccessStream();
            //    var streamReference = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromStream(randomAccessStream);
            //    var Attachment = new Windows.ApplicationModel.Email.EmailAttachment(
            //        "sharebyemail.html", streamReference);
            //    emailMessage.Attachments.Add(Attachment);
            //}
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }
        // https://www.pmichaels.net/2015/12/18/deleting-files-in-a-storage-folder-using-uwp/
        public static async void deleteFile(StorageFolder folder) {
            var files = await folder.GetFilesAsync();
            foreach (var file in files) {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }
        public static async Task<bool> ShowRatingReviewDialog() {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Services.Store.StoreRequestHelper"))
            {
                StoreSendRequestResult result = await StoreRequestHelper.SendRequestAsync(StoreContext.GetDefault(), 16, String.Empty);
                if (result.ExtendedError == null)
                {
                    JObject jsonObject = JObject.Parse(result.Response);
                    if (jsonObject.SelectToken("status").ToString() == "success")
                    {
                        // The customer rated or reviewed the app.
                        return true;
                    }
                }
            }

           
            return false;
        }

    }
}
