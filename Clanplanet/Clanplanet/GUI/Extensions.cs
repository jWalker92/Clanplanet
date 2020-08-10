using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Clanplanet.GUI
{
    public static class Extensions
    {

        /// <summary>
        /// Diese Funktion entfernt alle GestureRecognizers und Fügt einen neuen, animierten hinzu.
        /// </summary>
        /// <param name="element"></param>
        public static void SetAnimatedTapAction(this View element, Action onTap, View animatedElement = null, double zoomFactor = 0.95)
        {
            if (animatedElement == null)
            {
                animatedElement = element;
            }
            element.GestureRecognizers.Clear();
            element.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            await animatedElement.ScaleTo(zoomFactor, 70, Easing.CubicOut);
                            await animatedElement.ScaleTo(1, 70, Easing.CubicIn);
                        }
                        catch (Exception e)
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine(e);
                            System.Diagnostics.Debug.WriteLine("AnimatedTapAction Exception occured");
#endif
                        }
                    });
                    onTap.Invoke();
                })
            });
        }
    }
}
