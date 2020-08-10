using Clanplanet.GUI;
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
    public partial class AboutPage : ContentPage
    {
        private HtmlWebViewSource webSource;
        private string donateText;

        public AboutPage()
        {
            InitializeComponent();
            //WebSource = new HtmlWebViewSource();
            //WebSource.Html = "<form action=\"https://www.paypal.com/cgi-bin/webscr\" method=\"post\" target=\"_top\">"+
            //                 "<input type=\"hidden\" name=\"cmd\" value=\"_s-xclick\">"+
            //                 "<input type=\"hidden\" name=\"hosted_button_id\" value=\"KDCEKSTN9TJY8\">"+
            //                 "<input type=\"image\" src=\"https://www.paypalobjects.com/de_DE/DE/i/btn/btn_donateCC_LG.gif\" border=\"0\" name=\"submit\" alt=\"Jetzt einfach, schnell und sicher online bezahlen – mit PayPal.\">"+
            //                 "<img alt=\" border=\"0\" src=\"https://www.paypalobjects.com/de_DE/i/scr/pixel.gif\" width=\"1\" height=\"1\">"+
            //                 "</form>";
            DonateText = "Ich habe mich entschlossen diese App kostenlos und werbefrei zur Verfügung zu stellen." + Environment.NewLine +
                "Über eine kleine Spende für meinen Aufwand würde ich mich sehr freuen. Die Spende ist aber kein Muss!" + Environment.NewLine +
                "Rückzahlungen sind nicht möglich.";
            BindingContext = this;
            DonatePic.SetAnimatedTapAction(() => {
                Device.OpenUri(new Uri("https://www.paypal.me/dfi"));
            });
        }

        public HtmlWebViewSource WebSource { get => webSource; set => webSource = value; }
        public string DonateText { get => donateText; set => donateText = value; }

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
}