using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;

namespace TeszterLibrary.Services
{
    /// <summary>
    /// A v4-es Fluent UI <c>IToastService</c> szinkron hívási formája (ShowError, ShowInfo, …).
    /// A Fluent UI v5-ben a toastokat az aszinkron <see cref="INotificationService"/> kezeli; ez az interfész
    /// a meglévő hívási helyeket változatlanul hagyja, a tesztek pedig továbbra is hamisíthatják.
    /// </summary>
    public interface IToastService
    {
        void ShowError(string message);
        void ShowWarning(string message);
        void ShowInfo(string message);
        void ShowSuccess(string message);
        /// <summary>v4-ben külön ikonú toast volt; v5-ben információs toastként jelenik meg.</summary>
        void ShowEvent(string message);
        /// <summary>v4-ben külön ikonú toast volt; v5-ben információs toastként jelenik meg.</summary>
        void ShowMention(string message);
        void ShowToast(ToastIntent intent, string message);
    }

    /// <summary>
    /// <see cref="IToastService"/> megvalósítás a v5-ös <see cref="INotificationService"/> fölött.
    /// A v5-ös Show*ToastAsync metódusok csak a toast bezárásakor térnek vissza, ezért a hívást
    /// nem várjuk meg ("tűzd ki és felejtsd el"), a hibákat naplózzuk.
    /// </summary>
    public class FluentToastService(INotificationService notifications, ILogger<FluentToastService> logger) : IToastService
    {
        public void ShowError(string message) => ShowToast(ToastIntent.Error, message);
        public void ShowWarning(string message) => ShowToast(ToastIntent.Warning, message);
        public void ShowInfo(string message) => ShowToast(ToastIntent.Info, message);
        public void ShowSuccess(string message) => ShowToast(ToastIntent.Success, message);
        public void ShowEvent(string message) => ShowToast(ToastIntent.Info, message);
        public void ShowMention(string message) => ShowToast(ToastIntent.Info, message);

        public void ShowToast(ToastIntent intent, string message)
        {
            Task task = intent switch
            {
                ToastIntent.Error => notifications.ShowErrorToastAsync(message),
                ToastIntent.Warning => notifications.ShowWarningToastAsync(message),
                ToastIntent.Success => notifications.ShowSuccessToastAsync(message),
                _ => notifications.ShowInfoToastAsync(message),
            };
            _ = task.ContinueWith(t => logger.LogWarning(t.Exception, "Nem sikerült a toast megjelenítése: {Message}", message),
                CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        }
    }
}
