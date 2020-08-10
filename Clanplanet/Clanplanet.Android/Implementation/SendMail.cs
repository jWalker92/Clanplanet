using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Xamarin.Forms;
using Clanplanet.Droid.Implementation;
using Clanplanet.Dependencies;
using System.IO;
using Android;
using Android.Support.V4.App;
using System.Threading.Tasks;

[assembly: Dependency(typeof(SendMail))]
namespace Clanplanet.Droid.Implementation
{
    class SendMail : ISendMail
    {
        public void Send(string errorMessage, string jsonData)
        {
            SendMailAsync(errorMessage, jsonData);
        }

        private async Task SendMailAsync(string message, string data)
        {
            var intent = new Intent(Intent.ActionSend);
            intent.PutExtra(Intent.ExtraEmail, new string[] { "d.firus92@gmail.com" });
            intent.PutExtra(Intent.ExtraSubject, "Log-Daten für Clanplanet2.0");
            intent.PutExtra(Intent.ExtraText, message);
            intent.SetType("message/rfc822");

            var downloadsFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
            var filePath = Path.Combine(downloadsFolder.Path, "Logfile.txt");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var createdFile = File.CreateText(filePath);
            await createdFile.WriteAsync(data);
            await createdFile.FlushAsync();

            var file = new Java.IO.File(filePath);
            file.SetReadable(true, false);
            var uri = Android.Net.Uri.FromFile(file);
            intent.PutExtra(Intent.ExtraStream, uri);

            Forms.Context.StartActivity(Intent.CreateChooser(intent, "Email senden..."));
        }
    }
}