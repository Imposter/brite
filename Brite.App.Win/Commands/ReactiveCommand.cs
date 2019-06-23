using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace Brite.App.Win.Commands
{
    public sealed class ReactiveCommand : ReactiveCommand<object>
    {
        private ReactiveCommand(IObservable<bool> canExecute)
            : base(canExecute.StartWith(false))
        {
        }

        public new static ReactiveCommand Create()
        {
            return new ReactiveCommand(Observable.Return(true).StartWith(true));
        }

        public new static ReactiveCommand Create(IObservable<bool> canExecute)
        {
            return new ReactiveCommand(canExecute);
        }
    }

    public class ReactiveCommand<T> : IObservable<T>, ICommand, IDisposable
    {
        private readonly Subject<T> _execute;
        private readonly IDisposable _canDisposable;
        private bool _currentCanExecute;

        protected ReactiveCommand(IObservable<bool> canExecute)
        {
            _canDisposable = canExecute.Subscribe(x =>
            {
                _currentCanExecute = x;
                CommandManager.InvalidateRequerySuggested();
            });

            _execute = new Subject<T>();
        }

        public static ReactiveCommand<T> Create()
        {
            return new ReactiveCommand<T>(Observable.Return(true));
        }

        public static ReactiveCommand<T> Create(IObservable<bool> canExecute)
        {
            return new ReactiveCommand<T>(canExecute);
        }

        public void Dispose()
        {
            _canDisposable.Dispose();

            _execute.OnCompleted();
            _execute.Dispose();
        }

        public virtual void Execute(object parameter)
        {
            var typedParameter = parameter is T variable ? variable : default(T);

            if (CanExecute(typedParameter))
            {
                _execute.OnNext(typedParameter);
            }
        }

        public virtual bool CanExecute(object parameter)
        {
            return _currentCanExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _execute.Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
        }
    }
}
