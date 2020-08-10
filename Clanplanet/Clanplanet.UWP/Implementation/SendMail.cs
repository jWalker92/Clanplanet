using Clanplanet.Dependencies;
using System;
using System.Linq;
using Windows.ApplicationModel.Contacts;
using System.Threading.Tasks;
using Xamarin.Forms;
using Clanplanet.UWP.Implementation;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;

[assembly: Dependency(typeof(SendMail))]
namespace Clanplanet.UWP.Implementation
{
    class SendMail : ISendMail
    {
        public void Send(string errorMessage, string jsonData)
        {
            Contact dev = new Contact();
            dev.Emails.Add(new ContactEmail() { Address = "d.firus92@gmail.com" });
            ComposeEmail(dev, errorMessage, jsonData);
        }

        private async Task ComposeEmail(Contact recipient, string messageBody, string data)
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Body = messageBody;
            emailMessage.Subject = "Log-Daten für Clanplanet2.0";

            var email = recipient.Emails.FirstOrDefault();
            if (email != null)
            {
                var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient(email.Address);
                emailMessage.To.Add(emailRecipient);
            }
            StorageFolder MyFolder = ApplicationData.Current.LocalFolder;
            StorageFile attachmentFile = await MyFolder.CreateFileAsync("Logfile.txt", CreationCollisionOption.ReplaceExisting);
            if (attachmentFile != null)
            {
                await FileIO.WriteTextAsync(attachmentFile, data);
                var stream = RandomAccessStreamReference.CreateFromFile(attachmentFile);
                var attachment = new Windows.ApplicationModel.Email.EmailAttachment(
                         attachmentFile.Name,
                         stream);
                emailMessage.Attachments.Add(attachment);
            }

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        public async Task SaveTextAsync(string filename, string text)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await localFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, text);
        }
    }
}
