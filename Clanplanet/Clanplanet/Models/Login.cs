using Clanplanet.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Clanplanet.Models
{
    public class Login
    {
        private string username;
        private string password;
        private string clanID;
        private bool addReminderEnabled;
        private bool autoTermin;

        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string ClanID { get => clanID; set => clanID = value; }
        public bool CanEditEvents { get; internal set; }
        public bool AddReminderEnabled { get => addReminderEnabled; set => addReminderEnabled = value; }
        public bool AutoTermin { get => autoTermin; set => autoTermin = value; }

        public void SaveCredentials()
        {
            var secuStore = DependencyService.Get<ISecureStore>();
            if (secuStore.DoCredentialsExist())
            {
                secuStore.DeleteCredentials();
            }
            secuStore.SaveCredentials(this);
        }
    }
}
