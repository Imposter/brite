using Brite.App.Win.Commands;
using System;
using System.Reactive;

namespace Brite.App.Win.ViewModels
{
    public interface ICloseableViewModel : IViewModel
    {
        IObservable<Unit> Confirmed { get; }
        IObservable<Unit> Denied { get; }
        IObservable<Unit> Closed { get; } // On keyboard escape or after it is confirmed/denied

        ReactiveCommand ConfirmCommand { get; }
        ReactiveCommand DenyCommand { get; }
        ReactiveCommand CancelCommand { get; }
    }
}
