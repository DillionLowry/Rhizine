using CommunityToolkit.WinUI.Notifications;
using Rhizine.WPF.Services.Interfaces;
using Windows.UI.Notifications;

namespace Rhizine.WPF.Services;

public partial class ToastNotificationsService : IToastNotificationsService
{
    public ToastNotificationsService()
    {
    }

    public void ShowToastNotification(ToastNotification toastNotification)
    {
        //ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
    }

    /*
    public void ShowToastNotificationSample()
    {
        throw new NotImplementedException();
    }
    */
}