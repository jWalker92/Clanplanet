using Clanplanet.Models;
using Clanplanet.Service;
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
    public partial class EditEvent : ContentPage
    {
        private ClanEvent clanEvent;

        private List<AnmeldungenType> types;
        public string VisibilityDescription { get => visibility; set => visibility = value; }
        public AnmeldungenType Anmeldung { get => anmeldung; set => anmeldung = value; }
        public Gruppe SelectedGroup { get => selectedGroup; set => selectedGroup = value; }
        public CreateInfos Infos { get => infos; set => infos = value; }
        public bool FinishedLoading { get => finishedLoading; set => finishedLoading = value; }
        public string EventName { get => eventName; set => eventName = value; }
        public DateTime SelectedDate { get => selectedDate; set => selectedDate = value; }
        public TimeSpan SelectedTime { get => selectedTime; set => selectedTime = value; }
        private bool clanWar;
        public bool Intern { get => intern; set { intern = value;
                SetVisibilityDesc();
            } }
        private DateTime minDate;

        private void SetVisibilityDesc()
        {
            VisibilityDescription = intern ? "Intern (Nur für Clan sichtbar)" : "Nicht Intern (Für Jeden sichtbar)";
            OnPropertyChanged("VisibilityDescription");
        }

        public List<AnmeldungenType> AnmeldungTypen { get => types; set => types = value; }
        public DateTime MinDate { get => minDate; set => minDate = value; }
        public bool ClanWar { get => clanWar; set { clanWar = value; OnPropertyChanged("ClanWar"); } }

        public ClanPlanetEnemy SelectedCpEnemy
        {
            get => selectedCpEnemy;
            set
            {
                selectedCpEnemy = value;
                if (selectedCpEnemy != null && ClanWarDetails != null)
                {
                    if (selectedCpEnemy.ClanId.Equals("0"))
                    {
                        if (clanEvent == null || (clanEvent != null && !Manual))
                        {
                            ClanWarDetails.GegnerName = string.Empty;
                            ClanWarDetails.Kürzel = string.Empty;
                            ClanWarDetails.GegnerLink = string.Empty;
                        }
                        Manual = true;
                    }
                    else
                    {
                        ClanWarDetails.GegnerName = "-automatisch-";
                        ClanWarDetails.Kürzel = "-automatisch-";
                        ClanWarDetails.GegnerLink = "-automatisch-";
                        Manual = false;
                    }
                } 
                OnPropertyChanged("Manual");
                OnPropertyChanged("ClanWarDetails");
            }
        }
        public bool Manual { get => manual; set => manual = value; }
        public ClanWarDetails ClanWarDetails { get => clanWarDetails; set => clanWarDetails = value; }
        public Game SelectedCpGame { get => selectedCpGame; set => selectedCpGame = value; }
        public Squad SelectedCpSquad { get => selectedCpSquad; set => selectedCpSquad = value; }

        private ClanWarDetails clanWarDetails;
        private bool manual = true;
        private ClanPlanetEnemy selectedCpEnemy;
        private bool intern;
        private DateTime selectedDate;
        private TimeSpan selectedTime;
        private string eventName;
        private CreateInfos infos;
        private string visibility;
        private AnmeldungenType anmeldung;
        private Gruppe selectedGroup;
        private Game selectedCpGame;
        private Squad selectedCpSquad;
        private bool finishedLoading = false;

        public EditEvent()
        {
            SetGeneralConsts();
            SetDefaultValues();
            InitializeComponent();
            Title = "Event eintragen";
            LoginService.GetCreateInfos(onComplete: (success, newInfos) => {
                if (success)
                {
                    this.Infos = newInfos;
                    if (Infos.Gruppen.Count > 0)
                    {
                        SelectedGroup = Infos.Gruppen[0];
                    }
                    else
                    {
                        Infos.Gruppen.Add(new Gruppe() { Name = "-Alle Mitglieder-", Id = "0", Selected = true });
                        SelectedGroup = Infos.Gruppen[0];
                    }
                    if (Infos.Enemies.Count > 0)
                    {
                        SelectedCpEnemy = Infos.Enemies[0];
                    }
                    if (Infos.Games.Count > 0)
                    {
                        SelectedCpGame = Infos.Games[0];
                    }
                    if (Infos.Squads.Count > 0)
                    {
                        SelectedCpSquad = Infos.Squads[0];
                    }
                    else
                    {
                        Infos.Squads.Add(new Squad() { Name = "-Alle Mitglieder-", Id = "0", Selected = true });
                        SelectedCpSquad = Infos.Squads[0];
                    }
                    FinishedLoading = true;
                    OnPropertyChanged("Infos");
                    OnPropertyChanged("FinishedLoading");
                    OnPropertyChanged("SelectedCpEnemy");
                    OnPropertyChanged("SelectedCpGame");
                    OnPropertyChanged("SelectedCpSquad");
                    OnPropertyChanged("SelectedGroup");
                    Device.BeginInvokeOnMainThread(() => EntryEvent.Focus());
                }
            });
        }

        private void SetGeneralConsts()
        {
            MinDate = DateTime.Today;
            ClanWarDetails = new ClanWarDetails();
            types = new List<AnmeldungenType>();
            types.Add(new AnmeldungenType() { Id = "0", Description = "Nicht möglich" });
            types.Add(new AnmeldungenType() { Id = "1", Description = "Nur Clanmitglieder" });
            types.Add(new AnmeldungenType() { Id = "2", Description = "Alle Clanplanet Benutzer" });
            SetVisibilityDesc();
            BindingContext = this;
            Anmeldung = types[1];
            ToolbarItems.Add(new ToolbarItem("Speichern", "okay.png", () => Create_Clicked(null, null) ));

        }

        private void SetDefaultValues()
        {
            selectedDate = DateTime.Today;
            selectedTime = new TimeSpan(20, 0, 0);
        }

        public EditEvent(ClanEvent cEvent)
        {
            SetGeneralConsts();
            clanEvent = cEvent;
            clanWarDetails = cEvent.CwDetails;
            ClanWar = cEvent.ClanWar;
            EventName = clanEvent.EventName;
            SelectedDate = clanEvent.Day.Date;
            SelectedTime = clanEvent.Day.TimeOfDay;
            MinDate = SelectedDate;
            InitializeComponent();
            Title = "Event bearbeiten";
            LoginService.GetCreateInfos(cEvent, onComplete: (success, newInfos) => {
                if (success)
                {
                    this.Infos = newInfos;
                    if (Infos.Gruppen.Count > 0)
                    {
                        SelectedGroup = Infos.Gruppen.FirstOrDefault(x => x.Selected);
                        if (SelectedGroup == null)
                        {
                            SelectedGroup = Infos.Gruppen[0];
                        }
                    }
                    else
                    {
                        Infos.Gruppen.Add(new Gruppe() { Name = "-Alle Mitglieder-", Id = "0", Selected = true });
                        SelectedGroup = Infos.Gruppen[0];
                    }
                    if (Infos.Enemies.Count > 0)
                    {
                        var selEnemy = Infos.Enemies.FirstOrDefault(x => x.Selected);
                        SelectedCpEnemy = selEnemy;
                        if (SelectedCpEnemy == null)
                        {
                            SelectedCpEnemy = Infos.Enemies[0];
                        }
                    }
                    if (Infos.Games.Count > 0)
                    {
                        SelectedCpGame = Infos.Games.FirstOrDefault(x => x.Selected);
                        if (SelectedCpGame == null)
                        {
                            SelectedCpGame = Infos.Games[0];
                        }
                    }
                    if (Infos.Squads.Count > 0)
                    {
                        SelectedCpSquad = Infos.Squads.FirstOrDefault(x => x.Selected);
                        if (SelectedCpSquad == null)
                        {
                            SelectedCpSquad = Infos.Squads[0];
                        }
                    }
                    else
                    {
                        Infos.Squads.Add(new Squad() { Name = "-Alle Mitglieder-", Id = "0", Selected = true });
                        SelectedCpSquad = Infos.Squads[0];
                    }
                    Intern = Infos.Intern;
                    Anmeldung = types[Infos.SelectedType];
                    FinishedLoading = true;
                    OnPropertyChanged("FinishedLoading");
                    OnPropertyChanged("Infos");
                    OnPropertyChanged("Intern");
                    OnPropertyChanged("Anmeldung");
                    OnPropertyChanged("SelectedGroup");
                    OnPropertyChanged("SelectedCpEnemy");
                    OnPropertyChanged("SelectedCpGame");
                    OnPropertyChanged("SelectedCpSquad");
                }
            });
        }

        private void Create_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.EventName))
            {
                App.CurrentApp.Notify("Fehler", "Bitte einen EventNamen eingeben");
            }
            else if (ClanWar && (string.IsNullOrWhiteSpace(ClanWarDetails.GegnerName) || 
                string.IsNullOrWhiteSpace(ClanWarDetails.Kürzel) || string.IsNullOrWhiteSpace(ClanWarDetails.GegnerLink)))
            {
                App.CurrentApp.Notify("Fehler", "Bitte Gegnernamen, Kürzel und Website eingeben");
            }
            else if (ClanWar && string.IsNullOrWhiteSpace(ClanWarDetails.Details))
            {
                App.CurrentApp.Notify("Fehler", "Bitte ein paar kurze Infos eingeben");
            }
            else if (ClanWar && string.IsNullOrWhiteSpace(ClanWarDetails.Ort))
            {
                App.CurrentApp.Notify("Fehler", "Bitte einen Eintragungsort eingeben");
            }
            else
            {
                createButton.IsEnabled = false;
                var cEvent = new ClanEvent() { Day = SelectedDate.Add(SelectedTime), EventName = this.EventName, ClanId = LoginService.CurrentLogin.ClanID };
                if (clanEvent != null)
                {
                    cEvent.Id = clanEvent.Id;
                }
                if (ClanWar)
                {
                    ClanWarDetails.ClanID = SelectedCpEnemy.ClanId;
                    ClanWarDetails.SpielId = SelectedCpGame.Id;
                    ClanWarDetails.Squad = SelectedCpSquad.Id;
                }
                LoginService.CreateEvent(cEvent, ClanWar ? ClanWarDetails : null, SelectedGroup.Id, Intern ? "0" : "10", Anmeldung.Id, (success) =>
                {
                    if (success)
                    {
                        Device.BeginInvokeOnMainThread(() => App.CurrentApp.MasterDetail.NavPage.PopToRootAsync());
                    }
                });
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

    public class AnmeldungenType
    {
        private string id;
        private string description;

        public string Id { get => id; set => id = value; }
        public string Description { get => description; set => description = value; }
    }
}