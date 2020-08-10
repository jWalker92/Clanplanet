using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clanplanet.Dependencies
{
    public interface IExtraFunctions
    {
        ReminderReturn AddReminder(string title, DateTime time);

        ReminderReturn DeleteReminder(string eventuri, string reminderuri);

        string GetDatabasePath(string fileName);

            void LongAlert(string message);
            void ShortAlert(string message);
    }

    public class ReminderReturn
    {
        public string AndroidEventUri { get; set; }
        public string AndroidReminderUri { get; set; }
        public bool HasError { get; set; }
        public Exception Error { get; set; }
    }
}
