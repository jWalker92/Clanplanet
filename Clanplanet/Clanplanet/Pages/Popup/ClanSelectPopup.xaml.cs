using Clanplanet.Models;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Clanplanet.Pages.Popup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClanSelectPopup : PopupPage
    {
        private List<Clan> clans;

        public ClanSelectPopup(List<Clan> inClans)
        {
            this.Clans = inClans;
            if (this.Clans == null || this.Clans.Count == 0)
            {
                this.Clans.Add(new Clan() { Name = "Anscheinend hast du momentan keine Clanmitgliedschaften auf Clanplanet!\r\n" +
                    "Komm wieder wenn dich ein Clan aufgenommen, oder du einen erstellt hast!" });
            }
            this.BindingContext = this;
            InitializeComponent();
            ClansListe.ItemSelected += ClansListe_ItemSelected;
        }

        public ClanSelectPopup()
        {
            this.Clans = new List<Clan>();
            if (this.Clans == null || this.Clans.Count == 0)
            {
                this.Clans.Add(new Clan()
                {
                    Name = "Anscheinend hast du momentan keine Clanmitgliedschaften auf Clanplanet!\r\n" +
                    "Komm wieder wenn dich ein Clan aufgenommen, oder du einen erstellt hast!"
                });
            }
            this.BindingContext = this;
            InitializeComponent();
            ClansListe.ItemSelected += ClansListe_ItemSelected;
        }

        private void ClansListe_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ClansListe.SelectedItem = null;
            MessagingCenter.Send(this, "ClanSelected", (Clan)e.SelectedItem);
            PopupNavigation.PopAsync();
        }

        protected override void OnDisappearing()
        {
            MessagingCenter.Send<ClanSelectPopup, Clan>(this, "ClanSelected", null);
        }

        public List<Clan> Clans { get => clans; set => clans = value; }
    }
}