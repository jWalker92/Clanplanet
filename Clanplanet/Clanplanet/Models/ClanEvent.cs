using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Clanplanet.Models
{
    public class ClanEvent
    {
        private DateTime day;
        private string dateString;
        private string eventName;
        private string color;
        private string signUp;
        private string id;
        private bool clanWar;
        private string clanId;
        private MeldeStatus meldung;
        private List<Meldung> meldungen;
        private ClanWarDetails cwDetails;
        private bool meldungAllowed;
        private string fullName;
        private int meldungenCount;

        public string EventName { get => eventName; set => eventName = value; }
        public string DateString { get => dateString; set => dateString = value; }
        public bool ClanWar { get => clanWar; set => clanWar = value; }
        public string Id { get => id; set => id = value; }
        public string SignUp { get => signUp; set => signUp = value; }
        public string Color { get => color; set => color = value; }
        public DateTime Day { get => day; set => day = value; }
        public string ClanId { get => clanId; set => clanId = value; }
        public MeldeStatus Meldung { get => meldung; set => meldung = value; }
        public List<Meldung> Meldungen { get => meldungen; set => meldungen = value; }
        public ClanWarDetails CwDetails { get => cwDetails; set => cwDetails = value; }
        public string DateOnlyString { get => Day.ToString("dd.MM"); }
        public string TimeString { get => Day.ToString("HH:mm"); }
        public bool MeldungAllowed { get => meldungAllowed; set => meldungAllowed = value; }
        public string FullName { get => fullName; set => fullName = value; }
        public int MeldungenCount { get => meldungenCount; set => meldungenCount = value; }
        public bool MeldungPossible { get {
#if DEBUG
                return true;
#endif
                return DateTime.Now < Day; } }
        public FileImageSource StatusImage
        {
            get
            {
                switch (Meldung)
                {
                    case MeldeStatus.Anmeldung:
                        //if (Device.RuntimePlatform == Device.Windows)
                        //{
                        //    return "Assets/anwesend.png";
                        //}
                        return "anwesend.png";
                    case MeldeStatus.Abwesend:
                        return "abwesend.png";
                    case MeldeStatus.Ungemeldet:
                        return "ungemeldet.png";
                    default:
                        return null;
                }
            }
        }

        public ClanEvent()
        {
            Day = DateTime.MinValue;
            DateString = "01.01. 00:00 ";
            EventName = "";
            Color = "000000";
            SignUp = "";
            ClanWar = false;
            Meldungen = new List<Meldung>();
        }
    }

    public enum MeldeStatus
    {
        Anmeldung,
        Abwesend,
        Ungemeldet
    }

    public class Meldung
    {
        private string user;
        private MeldeStatus status;
        private DateTime meldeTag;
        private string bemerkung;

        public string User { get => user; set => user = value; }
        public MeldeStatus Status { get => status; set => status = value; }
        public string MeldungDisplay
        {
            get
            {
                return GetMeldungAsString(status);
            }
        }

        public static string GetMeldungAsString(MeldeStatus status)
        {
            switch (status)
            {
                case MeldeStatus.Anmeldung:
                    return "Anmeldung";
                case MeldeStatus.Abwesend:
                    return "Abwesend";
                case MeldeStatus.Ungemeldet:
                    return "Nicht gemeldet";
                default:
                    return null;
            }
        }

        public DateTime MeldeTag { get => meldeTag; set => meldeTag = value; }
        public string MeldeTagDisplay { get => MeldeTag.ToString("dd.MM."); }
        public string Bemerkung { get => bemerkung; set => bemerkung = value; }
    }

    public class ClanWarDetails
    {
        private string kontakt;
        private string clanID;
        private string spiel;
        private string squad;
        private string ort;
        private string details;
        private string sichtbarkeit;
        private string zuschauerOrt;
        private string teilnahme;
        private string ergebnis;
        private string gegnerName;
        private string gegnerLink;
        private string kürzel;
        private string spielId;

        public string Gegner { get { return Kürzel + " - " + GegnerName; } }
        public string Spiel { get => spiel; set => spiel = value; }
        public string Squad { get => squad; set => squad = value; }
        public string Ort { get => ort; set => ort = value; }
        public string Details { get => details; set => details = value; }
        public string Sichtbarkeit { get => sichtbarkeit; set => sichtbarkeit = value; }
        public string ZuschauerOrt { get => zuschauerOrt; set => zuschauerOrt = value; }
        public string Teilnahme { get => teilnahme; set => teilnahme = value; }
        public string Ergebnis { get => ergebnis; set => ergebnis = value; }
        public string GegnerName { get => gegnerName; set => gegnerName = value; }
        public string GegnerLink { get => gegnerLink; set => gegnerLink = value; }
        public string ClanID { get => clanID; set => clanID = value; }
        public string Kürzel { get => kürzel; set => kürzel = value; }
        public string Kontakt { get => kontakt; set => kontakt = value; }
        public string SpielId { get => spielId; set => spielId = value; }

        public ClanWarDetails()
        {
            SpielId = "0";
            Squad = "0";
            ClanID = "0";
        }

        public string GetParamsString()
        {
            if (ClanID == "0")
            {
                string paramsString = string.Format("&clanwar=on&cw_clanid={0}&cw_clanname={1}&cw_clankuerzel={2}&cw_weburl={3}&cw_kontaktperson={4}" +
                                                       "&cw_kurzinfo={5}&cw_location={6}&cw_observer={7}&cw_spielid={8}&cw_squadid={9}", ClanID, GegnerName, Kürzel, GegnerLink, Kontakt,
                                                       Details, Ort, ZuschauerOrt, SpielId, Squad);
                return paramsString;
            }
            else
            {
                string paramsString = string.Format("&clanwar=on&cw_clanid={0}&cw_kontaktperson={1}" +
                                                       "&cw_kurzinfo={2}&cw_location={3}&cw_observer={4}&cw_spielid={5}&cw_squadid={6}", ClanID, Kontakt,
                                                       Details, Ort, ZuschauerOrt, SpielId, Squad);
                return paramsString;
            }   
        }
    }

    public class CreateInfos
    {
        private List<Gruppe> gruppen;
        private bool intern;
        private int selectedType;
        private List<ClanPlanetEnemy> enemies;
        private List<Game> games;
        private List<Squad> squads;

        public List<Gruppe> Gruppen { get => gruppen; set => gruppen = value; }
        public bool Intern { get => intern; set => intern = value; }
        public int SelectedType { get => selectedType; set => selectedType = value; }
        public List<ClanPlanetEnemy> Enemies { get => enemies; set => enemies = value; }
        public List<Game> Games { get => games; set => games = value; }
        public List<Squad> Squads { get => squads; set => squads = value; }

        public CreateInfos()
        {
            Gruppen = new List<Gruppe>();
            Enemies = new List<ClanPlanetEnemy>();
            Games = new List<Game>();
            Squads = new List<Squad>();
        }


    }

    public class ClanPlanetEnemy
    {
        private string name;
        private string clanId;

        public string Name { get => name; set => name = value; }
        public string ClanId { get => clanId; set => clanId = value; }
        public bool Selected { get => selected; set => selected = value; }

        private bool selected;
    }

    public class Squad
    {
        private string id;
        private string name;

        public string Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public bool Selected { get => selected; set => selected = value; }

        private bool selected;
    }

    public class Game
    {
        private string id;
        private string name;

        public string Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public bool Selected { get => selected; set => selected = value; }

        private bool selected;
    }

    public class Gruppe
    {
        private string id;
        private string name;

        public string Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public bool Selected { get => selected; set => selected = value; }

        private bool selected;
    }

    public class Clan
    {
        private string name;
        private string id;
        private string rang;
        private DateTime memberSince;
        public bool ShowClanSub { get { return !string.IsNullOrEmpty(Id); } }

        public string Name { get => name; set => name = value; }
        public string Id { get => id; set => id = value; }
        public string Rang { get => rang; set => rang = value; }
        public DateTime MemberSinceDT { get => memberSince; set => memberSince = value; }
        public string MemberSince { get => memberSince.ToString("dd.MM.yyyy"); }
    }
}
