using Clanplanet.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Clanplanet.Models;
using Plugin.Toasts;
using Clanplanet.Dependencies;

namespace Clanplanet.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginView : ContentPage
    {
        private string username = "";
        private string password = "";
        private bool addReminderEnabled;
        private bool userSwitch = false;

        private Login login;
        private ISecureStore secuStore;

        public LoginView()
        {
            secuStore = DependencyService.Get<ISecureStore>();
            if (secuStore.DoCredentialsExist())
            {
                Login = secuStore.GetLogin();
            }
            else
            {
                Login = new Login() { Username = username, Password = password };
            }
            InitializeComponent();
            BindingContext = this;
            userSwitch = true;
        }

        private void LoginFinished(bool loggedIn, Cookie cookie)
        {
            Device.BeginInvokeOnMainThread(() => {
                App.CurrentApp.Notify("Login", loggedIn ? "Erfolgreich angemeldet" : "Login fehlgeschlagen");

                if (loggedIn)
                {
                    App.CurrentApp.MasterDetail.NavPage.PopAsync();
                }
            });
        }

        public Login Login { get => login; set => login = value; }
        public bool AddReminderEnabled { get => addReminderEnabled; set { addReminderEnabled = value;} }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Login.Username) && !string.IsNullOrEmpty(Login.Password))
            {
                if (secuStore.DoCredentialsExist())
                {
                    secuStore.DeleteCredentials();
                }
                secuStore.SaveCredentials(Login);
                LoginService.PerformLogin(Login, (loggedin, cookie) => LoginFinished(loggedin, cookie));
            }
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!(App.CurrentApp.MasterDetail.NavPage.CurrentPage is SendErrorPage))
                {
                    App.CurrentApp.MasterDetail.NavPage.PushAsync(new SendErrorPage());
                }
            });
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (e.Value && userSwitch)
            {
                Device.BeginInvokeOnMainThread(async() => {
                    var answer = await DisplayAlert("Termin-Erinnerungen", "Termine automatisch anlegen, oder jedes mal nachfragen?", "Automatisch", "Nachfragen");
                    Login.AutoTermin = answer;
                    if (!string.IsNullOrEmpty(Login.Username) && !string.IsNullOrEmpty(Login.Password))
                    {
                        if (secuStore.DoCredentialsExist())
                        {
                            secuStore.DeleteCredentials();
                        }
                        secuStore.SaveCredentials(Login);
                    }
                });
            }
        }
    }
}