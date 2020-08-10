using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clanplanet.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Clanplanet.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ZusagenPage : ContentPage, INotifyPropertyChanged
    {
        private ObservableCollection<Meldung> meldungen;

        public ZusagenPage()
        {
            InitializeComponent();
            this.BindingContext = this;
        }

        public ZusagenPage(ClanEvent cEvent)
        {
            this.Meldungen = new ObservableCollection<Meldung>(cEvent.Meldungen);
            InitializeComponent();
            this.Title = cEvent.EventName;
            this.BindingContext = this;
            ListViewClanEvents.ItemSelected += ListViewClanEvents_ItemSelected;
        }

        private void ListViewClanEvents_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ListViewClanEvents.SelectedItem = null;
        }

        public ObservableCollection<Meldung> Meldungen { get => meldungen; set => meldungen = value; }

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
    public class MeldestatusColorConverter : IValueConverter
    {

        #region IValueConverter implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MeldeStatus)
            {
                switch ((MeldeStatus)value)
                {
                    case MeldeStatus.Anmeldung:
                        return ClanColors.Anmeldung;
                    case MeldeStatus.Abwesend:
                        return ClanColors.Absage;
                    case MeldeStatus.Ungemeldet:
                    default:
                        return Color.Gray;
                }
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