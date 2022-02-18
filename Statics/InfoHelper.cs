using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Text;
using MP3DL.Media;

namespace Media_Downloader_App.Statics
{
    public class InfoHelper
    {
        public static void ShowNotification(string Text, string Title)
        {
            new ToastContentBuilder()
                .AddText(Title)
                .AddText(Text)
                .Show();
        }
        public static void ShowNotification(string Text, string Title, Uri ImageUri)
        {
            new ToastContentBuilder()
                .AddText(Title)
                .AddText(Text)
                .AddAppLogoOverride(ImageUri, ToastGenericAppLogoCrop.Default)
                .Show();
        }
        public static void ShowInAppNotification(string Text)
        {
            InAppNotification notif = MainPage.Current.InAppNotif;
            notif.Content = new TextBlock()
            {
                FontSize = 16,
                Text = Text,
            };
            notif.Show(3000);
        }
        public static void ShowInAppNotification(string Text, InAppNotification notif)
        {
            notif.Content = new TextBlock()
            {
                FontSize = 16,
                Text = Text,
            };
            notif.Show(3000);
        }
    }
}
