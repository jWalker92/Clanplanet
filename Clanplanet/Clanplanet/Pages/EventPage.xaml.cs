using Clanplanet.Dependencies;
using Clanplanet.GUI;
using Clanplanet.Models;
using Clanplanet.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Clanplanet.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EventPage : ContentPage
    {
        private EventPageViewModel ViewModel;

        public Label GegnerLinkLabel => GegnerLabel;

        public EventPage()
        {
            InitializeComponent();
            ViewModel = new EventPageViewModel();
            this.BindingContext = ViewModel;
        }

        public EventPage(ClanEvent cEvent)
        {
            ViewModel = new EventPageViewModel(cEvent, this);
            ViewModel.TitleName = cEvent.EventName;
            InitializeComponent();
            this.BindingContext = ViewModel;
            if (cEvent.ClanWar)
            {
                Icon = "degen_mini.png";
            }
            if (LoginService.CurrentLogin.CanEditEvents)
            {
                //if (!cEvent.ClanWar)
                //{
                    ToolbarItems.Add(new ToolbarItem("Bearbeiten", "edit.png", () =>
                    {
                        Device.BeginInvokeOnMainThread(() => App.CurrentApp.MasterDetail.NavPage.PushAsync(new EditEvent(cEvent)));
                    }));
                //}
                ToolbarItems.Add(new ToolbarItem("Löschen", "delete.png", async () => { var deleteConfirm = await DisplayAlert("", "Dieses Event löschen?", "Ja", "Abbrechen");
                    if (deleteConfirm)
                    {
                        LoginService.DeleteEvent(ViewModel.Event, (success) => 
                        {
                            if (success)
                            {
                                Device.BeginInvokeOnMainThread(() => App.CurrentApp.MasterDetail.NavPage.PopAsync());
                            }
                        } );
                    }
                } ));
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.RefreshEventDetails();
        }

        private void Accept_Clicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                InteractionStart();
                LoginService.SubscribeEvent(ViewModel.Event, false, ViewModel.Bemerkung, (success) => InteractionEnd());
                ViewModel.Event.Meldung = MeldeStatus.Anmeldung;
                ViewModel.PropChanged("Event");
                if (LoginService.CurrentLogin.AddReminderEnabled && Device.RuntimePlatform == Device.Android)
                {
                    if (LoginService.CurrentLogin.AutoTermin)
                    {
                        AddTermin();
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            var makeTermin = await DisplayAlert("Termin anlegen",
                                string.Format("Soll für das Event {0} am {1} um {2} ein Termin angelegt werden?",
                                            ViewModel.Event.EventName, ViewModel.Event.DateOnlyString, ViewModel.Event.TimeString),
                                "Ja", "Nein");
                            if (makeTermin)
                            {
                                AddTermin();
                            }
                        });
                    }
                }
            }
        }

        private void AddTermin()
        {
            var termin = DependencyService.Get<IExtraFunctions>().AddReminder(ViewModel.Event.EventName, ViewModel.Event.Day);
            if (termin != null && !termin.HasError)
            {
                App.CurrentApp.Notify("Clanplanet", ViewModel.Event.EventName + " wurde im Kalender eingetragen");
                App.Database.SaveItemAsync(new ClanEventAppointment() { EventID = ViewModel.Event.Id, Name = ViewModel.Event.EventName, TerminID = termin.AndroidEventUri, ReminderID = termin.AndroidReminderUri });
            }
            else
            {
                if (GlobalErrorValues.Current.IsCollectingReportData)
                {
                    if (termin != null)
                    {
                        SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = "Unbekannter Fehler", Origin = "AddTermin" });
                    }
                    else
                    {
                        SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = termin.Error.ToString(), Origin = "AddTermin" });
                    }
                }
            }
        }

        private void Decline_Clicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                InteractionStart();
                LoginService.SubscribeEvent(ViewModel.Event, true, ViewModel.Bemerkung, (success) => InteractionEnd() );
                ViewModel.Event.Meldung = MeldeStatus.Abwesend;
                ViewModel.PropChanged("Event");
            }
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                InteractionStart();
                ViewModel.Bemerkung = string.Empty;
                LoginService.UnSubscribeEvent(ViewModel.Event, (success) => InteractionEnd());
                ViewModel.Event.Meldung = MeldeStatus.Ungemeldet;
                ViewModel.PropChanged("Event");
                if (LoginService.CurrentLogin.AddReminderEnabled && Device.RuntimePlatform == Device.Android)
                {
                    Task.Run(async () =>
                    {
                        var clanDbEvents = await App.Database.GetByEventID(ViewModel.Event.Id);
                        if (clanDbEvents != null && clanDbEvents.Count > 0)
                        {
                            var clanDbEvent = clanDbEvents.First();
                            if (clanDbEvent.TerminID != null && clanDbEvent.ReminderID != null)
                            {
                                var delete = DependencyService.Get<IExtraFunctions>().DeleteReminder(clanDbEvent.TerminID, clanDbEvent.ReminderID);
                                if (delete != null && !delete.HasError)
                                {
                                    App.CurrentApp.Notify("Clanplanet", ViewModel.Event.EventName + " wurde aus dem Kalender entfernt");
                                    await App.Database.DeleteEventItemsAsync(clanDbEvent.EventID);
                                }
                                else
                                {
                                    if (GlobalErrorValues.Current.IsCollectingReportData)
                                    {
                                        if (delete != null)
                                        {
                                            SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = "Unbekannter Fehler", Origin = "DeleteTermin" });
                                        }
                                        else
                                        {
                                            SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = delete.Error.ToString(), Origin = "DeleteTermin" });
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }

        private void InteractionEnd()
        {
            ViewModel.RefreshEventDetails();
        }

        private void InteractionStart()
        {
            ViewModel.ZusageEnabled = false;
            ViewModel.AbsageEnabled = false;
            ViewModel.EnthaltungEnabled = false;
            ViewModel.ViewListEnabled = false;
            ViewModel.PropChanged("ZusageEnabled");
            ViewModel.PropChanged("AbsageEnabled");
            ViewModel.PropChanged("EnthaltungEnabled");
            ViewModel.PropChanged("ViewListEnabled");
        }

        private void ViewList_Clicked(object sender, EventArgs e)
        {
            App.CurrentApp.MasterDetail.NavPage.PushAsync(new ZusagenPage(ViewModel.Event));
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

    public class EventPageViewModel : INotifyPropertyChanged
    {
        private ClanEvent cEvent;
        private string meldeStatus;
        private string bemerkung;
        private ObservableCollection<Meldung> meldungen;
        private bool zusageEnabled;
        private bool absageEnabled;
        private bool enthaltungEnabled;
        private bool viewListEnabled;
        private string titleName;
        private EventPage page;

        public EventPageViewModel(ClanEvent cEvent, EventPage page)
        {
            this.cEvent = cEvent;
            this.page = page;
            Meldungen = new ObservableCollection<Meldung>();
        }

        public void RefreshEventDetails()
        {
            LoginService.GetEventDetails(cEvent, (cEvent) => HavingEvent(cEvent));
            if (cEvent.ClanWar)
            {
                LoginService.GetClanWarDetails(cEvent, (cEvent) => {
                    Event.CwDetails = cEvent.CwDetails;
                    if (cEvent.CwDetails != null)
                    {
                        if (!string.IsNullOrEmpty(cEvent.CwDetails.GegnerLink))
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                page.GegnerLinkLabel.TextColor = Color.Blue;
                                page.GegnerLinkLabel.SetAnimatedTapAction(() => Device.OpenUri(new Uri(cEvent.CwDetails.GegnerLink)));
                            });
                        }
                    }
                    OnPropertyChanged("Event");
                });
            }
        }

        private void HavingEvent(ClanEvent cEvent)
        {
            if (cEvent != null)
            {
                Event = cEvent;
                ViewListEnabled = true;
                var myMeldung = Event.Meldungen.FirstOrDefault(x => x.User.ToLower().Contains(LoginService.CurrentLogin.Username.ToLower()));
                if (myMeldung != null)
                {
                    MeldeStatus = myMeldung.MeldungDisplay;
                    Bemerkung = myMeldung.Bemerkung;
                    ZusageEnabled = false;
                    AbsageEnabled = false;
                    EnthaltungEnabled = true;
                }
                else
                {
                    if (Event.Meldung == Models.MeldeStatus.Ungemeldet)
                    {
                        MeldeStatus = "Nicht gemeldet";
                        ZusageEnabled = Event.MeldungPossible;
                        AbsageEnabled = true;
                        EnthaltungEnabled = false;
                    }
                    else
                    {
                        MeldeStatus = Meldung.GetMeldungAsString(Event.Meldung);
                        ZusageEnabled = false;
                        AbsageEnabled = false;
                        EnthaltungEnabled = true;
                    }
                }
                Meldungen.Clear();
                foreach (var meldung in Event.Meldungen)
                {
                    Meldungen.Add(meldung);
                }
                OnPropertyChanged("Event");
                OnPropertyChanged("Meldungen");
                OnPropertyChanged("MeldeStatus");
                OnPropertyChanged("Bemerkung");
                OnPropertyChanged("MeldungenCountDisplay");
                OnPropertyChanged("ZusageEnabled");
                OnPropertyChanged("AbsageEnabled");
                OnPropertyChanged("EnthaltungEnabled");
                OnPropertyChanged("ViewListEnabled");
            }
        }

        public EventPageViewModel()
        {
            this.cEvent = new ClanEvent();
        }

        public ClanEvent Event { get => cEvent; set => cEvent = value; }
        public string MeldeStatus { get => "Dein Status: " + meldeStatus; set => meldeStatus = value; }
        public string Bemerkung { get => bemerkung; set => bemerkung = value; }
        public ObservableCollection<Meldung> Meldungen { get => meldungen; set => meldungen = value; }
        public bool ZusageEnabled { get => zusageEnabled; set => zusageEnabled = value; }
        public bool AbsageEnabled { get => absageEnabled; set => absageEnabled = value; }
        public bool EnthaltungEnabled { get => enthaltungEnabled; set => enthaltungEnabled = value; }
        public bool ViewListEnabled { get => viewListEnabled; set => viewListEnabled = value; }
        public string TitleName { get => titleName; set => titleName = value; }

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
}