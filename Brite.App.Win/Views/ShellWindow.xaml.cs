using Brite.App.Win.Services;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using SourceChord.FluentWPF;

namespace Brite.App.Win.Views
{
    /// <summary>
    /// Interaction logic for ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow : AcrylicWindow//MetroWindow
    {
        private readonly IDisposable _disposable;

        public ShellWindow(ISchedulerService schedulerService, IMessageService messageService)
        {
            InitializeComponent();

            //_disposable = messageService.Show
            //    // Delay to make sure there is time for the animations
            //    .Delay(TimeSpan.FromMilliseconds(250), schedulerService.TaskPool)
            //    .ObserveOn(schedulerService.Dispatcher)
            //    .Select(x => new MessageDialog(x))
            //    .SelectMany(ShowDialogAsync, (x, y) => x)
            //    .Subscribe();

            Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e)
        {
          //  _disposable.Dispose();
        }

        //private IObservable<Unit> ShowDialogAsync(MessageDialog dialog)
        //{
        //    var settings = new MetroDialogSettings
        //    {
        //        ColorScheme = MetroDialogColorScheme.Accented
        //    };

        //    return this.ShowMetroDialogAsync(dialog, settings)
        //        .ToObservable()
        //        .SelectMany(x => dialog.CloseableContent.Closed, (x, y) => x)
        //        .SelectMany(x => this.HideMetroDialogAsync(dialog).ToObservable(), (x, y) => x);
        //}
    }
}
