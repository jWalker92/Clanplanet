
using Clanplanet.Dependencies;
using Clanplanet.Models;
using System.Linq;
using Xamarin.Auth;
using Xamarin.Forms;
using System;
using Clanplanet.Droid.Implementation;
using System.Net;

[assembly: Dependency(typeof(SecureStore))]
namespace Clanplanet.Droid.Implementation
{
    class SecureStore : ISecureStore
    {
        public void SaveCredentials(Login login)
        {
            if (!string.IsNullOrWhiteSpace(login.Username) && !string.IsNullOrWhiteSpace(login.Password))
            {
                Account account = new Account
                {
                    Username = login.Username
                };
                account.Properties.Add("Password", login.Password);
                account.Properties.Add("ClanId", login.ClanID??"");
                account.Properties.Add("Reminder", login.AddReminderEnabled ? "true" : "false");
                account.Properties.Add("AutoRemind", login.AutoTermin ? "true" : "false");
                AccountStore.Create(Forms.Context).Save(account, App.AppName);
            }
        }
        public void DeleteCredentials()
        {
            var account = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).FirstOrDefault();
            if (account != null)
            {
                AccountStore.Create(Forms.Context).Delete(account, App.AppName);
            }
        }
        public string UserName
        {
            get
            {
                var account = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).FirstOrDefault();
                return (account != null) ? account.Username : null;
            }
        }

        public string Password
        {
            get
            {
                var account = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).FirstOrDefault();
                return (account != null) ? account.Properties["Password"] : null;
            }
        }

        public string ClanId
        {
            get
            {
                var account = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).FirstOrDefault();
                return (account != null) ? (account.Properties.ContainsKey("Reminder") ? (account.Properties["ClanId"] == "null" ? null : account.Properties["ClanId"]) : null) : null;
            }
        }

        public bool Reminder
        {
            get
            {
                var account = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).FirstOrDefault();
                return (account != null && account.Properties.ContainsKey("Reminder")) ? account.Properties["Reminder"].Equals("true") : false;
            }
        }

        public bool AutoRemind
        {
            get
            {
                var account = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).FirstOrDefault();
                return (account != null && account.Properties.ContainsKey("AutoRemind")) ? account.Properties["AutoRemind"].Equals("true") : false;
            }
        }

        public Cookie SessionCookie
        {
            get
            {
                var account = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).FirstOrDefault();
                return (account != null && account.Properties.ContainsKey("Cookie")) ? new Cookie("ClanSession", account.Properties["Cookie"]) : null;
            }
        }

    public Login GetLogin()
        {
            return new Login()
            {
                Username = this.UserName,
                Password = this.Password,
                ClanID = this.ClanId,
                AutoTermin = this.AutoRemind,
                AddReminderEnabled = this.Reminder
            };
        }

        public bool DoCredentialsExist()
        {
            return AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).Any() ? true : false;
        }

        public void StoreCookie(Cookie cookie)
        {
            var account = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).FirstOrDefault();
            if (account != null)
            {
                account.Properties.Add("Cookie", cookie.Value);
                DeleteCredentials();
                AccountStore.Create(Forms.Context).Save(account, App.AppName);
            }
        }
    }
}