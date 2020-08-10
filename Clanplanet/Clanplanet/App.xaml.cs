using Clanplanet.Dependencies;
using Clanplanet.Models;
using Clanplanet.Pages;
using Clanplanet.Service;
using Plugin.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Xamarin.Forms;

namespace Clanplanet
{
    public partial class App : Application
    {
        public ClanMainPage MasterDetail;
        public static App CurrentApp { get { return (App)(Current); } }

        static ClanplanetDatabase database;

        internal static ClanplanetDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = new ClanplanetDatabase(DependencyService.Get<IExtraFunctions>().GetDatabasePath("clanplanet.db3"));
                }
                return database;
            }
        }

        public static string AppName = "Clanplanet Eventplaner";

        public App()
        {
            InitializeComponent();
            try
            {
                MasterDetail = new ClanMainPage();
                MainPage = MasterDetail;
            }
            catch (Exception exc)
            {

                throw;
            }
        }

        public void Notify(string title, string text)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                Device.BeginInvokeOnMainThread(() => DependencyService.Get<IExtraFunctions>().ShortAlert(text));
            }
            else
            {
                var notificator = DependencyService.Get<IToastNotificator>();

                var options = new NotificationOptions()
                {
                    Title = title,
                    Description = text,
                    ClearFromHistory = true,
                };

                notificator.Notify(options);
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
