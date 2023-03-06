using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.UI.Xaml.Controls;

namespace Melody.Statics
{
    public static class InfoHelper
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
