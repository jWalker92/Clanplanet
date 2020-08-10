using Clanplanet.GUI;
using Clanplanet.Pages.Popup;
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
    public partial class ClanMainPageMaster : ContentPage
    {
        public ListView ListView => ListViewMenuItems;

        public ClanMainPageMaster()
        {
            InitializeComponent();
            BindingContext = new ClanMainPageMasterViewModel();
        }

        class ClanMainPageMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<ClanMainPageMenuItem> MenuItems { get; }
            public ClanMainPageMasterViewModel()
            {
                MenuItems = new ObservableCollection<ClanMainPageMenuItem>(new[]
                {
                    new ClanMainPageMenuItem { Id = 0, Title = "Events", TargetType = typeof(MainPage) },
                    new ClanMainPageMenuItem { Id = 1, Title = "Login", TargetType = typeof(LoginView) },
                    new ClanMainPageMenuItem { Id = 1, Title = "Meine Clans", TargetType = typeof(ClanSelectPopup) },
                    new ClanMainPageMenuItem { Id = 2, Title = "Info", TargetType = typeof(AboutPage) },
                    new ClanMainPageMenuItem { Id = 3, Title = "Fehlermeldung", TargetType = typeof(SendErrorPage) },
                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            #endregion
        }
    }
}