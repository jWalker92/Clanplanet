using Clanplanet.Dependencies;
using Clanplanet.Models;
using Clanplanet.Pages.Popup;
using Clanplanet.Service;
using Plugin.Toasts;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Clanplanet.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        MainPageViewModel ViewModel;
        public ListView ListView => ListViewClanEvents;
        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainPageViewModel(this);
            this.BindingContext = ViewModel;
            ListView.ItemSelected += ListView_ItemSelected;
            ViewModel.PropChanged("ShowEvents");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.RefreshEvents(ListViewClanEvents);
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                if (!ListViewClanEvents.IsRefreshing)
                {
                    var cEvent = (ClanEvent)e.SelectedItem;
                    if (cEvent.MeldungAllowed || cEvent.ClanWar || LoginService.CurrentLogin.CanEditEvents)
                    {
                        App.CurrentApp.MasterDetail.NavPage.PushAsync(new EventPage(cEvent));
                    }
                    ListView.SelectedItem = null;
                }
                ListView.SelectedItem = null;
            }
        }

        public void OnZusage(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            if (mi != null)
            {
                var cEvent = ((ClanEvent)mi.CommandParameter);
                if (cEvent.MeldungAllowed && cEvent.MeldungPossible)
                {
                    ListViewClanEvents.IsRefreshing = true;
                    if (cEvent.Meldung != MeldeStatus.Ungemeldet)
                    {
                        LoginService.UnSubscribeEvent(cEvent,
                            (success) =>
                            {
                                if (success)
                                {
                                    LoginService.SubscribeEvent(cEvent, false, "", (success2) => ViewModel.RefreshEvents(ListViewClanEvents));
                                }
                            });
                    }
                    else
                    {
                        LoginService.SubscribeEvent(cEvent, false, "", (success2) => ViewModel.RefreshEvents(ListViewClanEvents));
                    }
                }
            }
        }

        internal void GetClanSelection(bool v)
        {
            ViewModel.GetClanSelection(v);
        }

        public void OnAbsage(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            if (mi != null)
            {
                var cEvent = ((ClanEvent)mi.CommandParameter);
                if (cEvent.MeldungAllowed && cEvent.MeldungPossible)
                {
                    ListViewClanEvents.IsRefreshing = true;
                    if (cEvent.Meldung != MeldeStatus.Ungemeldet)
                    {
                        LoginService.UnSubscribeEvent(cEvent,
                            (success) =>
                            {
                                if (success)
                                {
                                    LoginService.SubscribeEvent(cEvent, true, "", (success2) => ViewModel.RefreshEvents(ListViewClanEvents));
                                }
                            });
                    }
                    else
                    {
                        LoginService.SubscribeEvent(cEvent, true, "", (success2) => ViewModel.RefreshEvents(ListViewClanEvents));
                    }
                }
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!(App.CurrentApp.MasterDetail.NavPage.CurrentPage is SendErrorPage))
                {
                    App.CurrentApp.MasterDetail.NavPage.PushAsync(new SendErrorPage());
                }
            });
        }
    }

    public class EventList : List<ClanEvent>
    {
        public string Heading { get; set; }
        public Color HeaderColor { get; set; }
        public FontAttributes CurrentDayAttribute { get; set; }
        public List<ClanEvent> ClanEvents => this;
    }

    public class MainPageViewModel : INotifyPropertyChanged
    {
        private ListView currentListView;
        private MainPage viewPage;
        private string headerTitle;


        private ObservableCollection<EventList> clanEvents;
        private int showEvents = 1;
        public string HeaderTitle
        {
            get => headerTitle; set
            {
                headerTitle = value;
                OnPropertyChanged("HeaderTitle");
            }
        }
        public ObservableCollection<EventList> ClanEvents { get => clanEvents; set { clanEvents = value; OnPropertyChanged("ClanEvents"); } }
        List<string> pickerItems = new List<string>
        {
            "letztem Monat",
            "diesem Monat",
            "nächsten Monat",
        };
        public ICommand PullRefreshCommand { get; set; }
        public List<string> PickerItems => pickerItems;
        public int ShowEvents
        {
            get => showEvents; set
            {
                showEvents = value;
                switch (showEvents)
                {
                    case 0:
                    case 2:
                        HeaderTitle = "Events vom";
                        break;
                    case 1:
                        HeaderTitle = "Events von";
                        break;
                    default:
                        break;
                }
                if (currentListView != null)
                {
                    RefreshEvents(currentListView);
                }
            }
        }

        public MainPageViewModel(MainPage view)
        {
            viewPage = view;
            PullRefreshCommand = new Command(() => RefreshEvents(currentListView));
            HeaderTitle = "Events von";
            ClanEvents = new ObservableCollection<EventList>();
            ShowEvents = 1;
        }

        public void RefreshEvents(ListView listViewClanEvents)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                currentListView = listViewClanEvents;
                currentListView.IsRefreshing = true;
                if (LoginService.CurrentCookie != null)
                {
                    LoginService.GetEvents(ShowEvents, onComplete: (success, evList) => HavingEvents(evList));
                    GetClanSelection(false);
                }
                else
                {
                    if (DependencyService.Get<ISecureStore>().DoCredentialsExist())
                    {
                        LoginService.PerformLogin(DependencyService.Get<ISecureStore>().GetLogin(), (success, cookie) =>
                        {
                            if (success)
                            {
                                GetClanSelection(false);
                                LoginService.GetEvents(ShowEvents, onComplete: (subSuccess, evList) => HavingEvents(evList));
                            }
                            else
                            {
                                App.CurrentApp.Notify("Login", "Fehlgeschlagen");
                                HavingEvents(null);
                            }
                        });
                    }
                }
            });
        }

        public void GetClanSelection(bool forceShow)
        {
            LoginService.GetClans((clans) => ShowClanSelectionPage(clans, forceShow));
        }

        private void ShowClanSelectionPage(List<Clan> clans, bool forceShow)
        {
            if (forceShow || string.IsNullOrEmpty(LoginService.CurrentLogin.ClanID) || !clans.Any(x => x.Id.Equals(LoginService.CurrentLogin.ClanID)))
            {
                MessagingCenter.Subscribe<ClanSelectPopup, Clan>(this, "ClanSelected",
                    (sender, selectedClan) => {
                        MessagingCenter.Unsubscribe<ClanSelectPopup, Clan>(this, "ClanSelected");
                        ClanSelected(selectedClan);
                    });
                Device.BeginInvokeOnMainThread(() => PopupNavigation.PushAsync(new ClanSelectPopup(clans)));
            }
        }

        private void ClanSelected(Clan selectedClan)
        {
            if (selectedClan != null)
            {
                LoginService.CurrentLogin.ClanID = selectedClan.Id;
                DependencyService.Get<ISecureStore>().SaveCredentials(LoginService.CurrentLogin);
                RefreshEvents(currentListView);
            }
        }

        private void HavingEvents(List<ClanEvent> events)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ClanEvents.Clear();
                if (events != null)
                {
                    var allevents = new ObservableCollection<EventList>();
                    var subEvents = new EventList();
                    DateTime lastDT = DateTime.MinValue;
                    object scrollTo = null;
                    foreach (ClanEvent clanEvent in events.OrderBy(x => x.Day)) //Order by just to be sure
                    {
                        if (lastDT.Date != clanEvent.Day.Date)
                        {
                            if (!lastDT.Equals(DateTime.MinValue) && subEvents.Count > 0)
                            {
                                allevents.Add(subEvents);
                            }
                            bool isToday = clanEvent.Day.Date.Equals(DateTime.Today);
                            if (isToday && scrollTo == null)
                            {
                                scrollTo = clanEvent;
                            }
                            subEvents = new EventList() { Heading = clanEvent.Day.ToString("ddd - dd.MM"),
                                HeaderColor = Color.FromHex(isToday ? "7092be" : "#bfbfbf"),
                                CurrentDayAttribute = isToday ? FontAttributes.Bold : FontAttributes.None };
                            lastDT = clanEvent.Day.Date;
                        }
                        subEvents.Add(clanEvent);
                    }
                    if (subEvents.Count > 0)
                    {
                        allevents.Add(subEvents);
                    }
                    ClanEvents = allevents;
                    if (currentListView != null && scrollTo != null)
                    {
                        Device.BeginInvokeOnMainThread(() => currentListView.ScrollTo(scrollTo, ScrollToPosition.End, true));
                    }
                    viewPage.ToolbarItems.Clear();
                    if (LoginService.CurrentLogin.CanEditEvents)
                    {
                        viewPage.ToolbarItems.Add(new ToolbarItem("Neu", "neu.png", () =>
                                App.CurrentApp.MasterDetail.NavPage.PushAsync(new EditEvent())));
                    }
                }
                if (currentListView != null)
                {
                    currentListView.IsRefreshing = false;
                }
            });
        }

        public void PropChanged(string memberName)
        {
            OnPropertyChanged(memberName);
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
    public class CurrentDayColorConverter : IValueConverter
    {

        #region IValueConverter implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is DateTime)
            {
                if (((DateTime)value).Date.Equals(DateTime.Today.Date))
                {
                    return Color.Black;
                }
                return Color.Gray;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}
