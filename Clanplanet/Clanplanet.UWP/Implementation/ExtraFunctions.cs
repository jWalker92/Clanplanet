using Clanplanet.Dependencies;
using Clanplanet.UWP.Implementation;
using System;
using System.IO;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(ExtraFunctions))]
namespace Clanplanet.UWP.Implementation
{
    public class ExtraFunctions : IExtraFunctions
    {
        public ReminderReturn AddReminder(string title, DateTime time)
        {
            return null;
        }

        public ReminderReturn DeleteReminder(string eventuri, string reminderuri)
        {
            return null;
        }

        public string GetDatabasePath(string fileName)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, fileName);
        }

        public void LongAlert(string message)
        {

        }

        public void ShortAlert(string message)
        {

        }
    }
}
