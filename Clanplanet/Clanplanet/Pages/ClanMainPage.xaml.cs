using Clanplanet.Dependencies;
using Clanplanet.GUI;
using Clanplanet.Pages.Popup;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Clanplanet.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClanMainPage : MasterDetailPage
    {
        private NavigationPage navPage;
        public ClanMainPage()
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.UWP)
            {
                this.MasterBehavior = MasterBehavior.Popover;
            }
            MasterPage.ListView.ItemSelected += ListView_ItemSelected;

            if (!DependencyService.Get<ISecureStore>().DoCredentialsExist())
            {
                Device.BeginInvokeOnMainThread(() => NavPage.PushAsync(new LoginView()));
            }
        }

        public NavigationPage NavPage
        {
            get
            {
                if (navPage == null)
                {
                    navPage = (NavigationPage)Detail;
                }
                return navPage;
            }
            set => navPage = value;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as ClanMainPageMenuItem;
            if (item == null)
                return;

            var page = (Page)Activator.CreateInstance(item.TargetType);
            page.Title = item.Title;
            if (!item.TargetType.Equals(typeof(MainPage)))
            {
                if (!(item.TargetType.Equals(NavPage.CurrentPage.GetType())))
                {
                    if (item.TargetType.Equals(typeof(ClanSelectPopup)))
                    {
                        if (!(NavPage.CurrentPage is MainPage))
                        {
                            NavPage.PopToRootAsync(true);
                        }
                        ((MainPage)NavPage.CurrentPage).GetClanSelection(true);
                    }
                    else
                    {
                        NavPage.PushAsync(page);
                    }
                }
            }
            else
            {
                NavPage = new NavigationPage(page);
                Detail = NavPage;
            }
            IsPresented = false;

            MasterPage.ListView.SelectedItem = null;
        }
    }
}