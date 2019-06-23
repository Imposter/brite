using Brite.App.Win.Commands;
using Brite.App.Win.Services;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Brite.App.Win.ViewModels
{
    public class ExceptionViewModel : CloseableViewModel, IExceptionViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly Exception _exception;

        public string Message => _exception.ToString();

        public ReactiveCommand CopyDetailsCommand { get; }
        public ReactiveCommand ExitCommand { get; }
        public ReactiveCommand RestartCommand { get; }

        public ExceptionViewModel(IApplicationService applicationService, Exception exception)
        {
            _applicationService = applicationService;
            _exception = exception;

            CopyDetailsCommand = ReactiveCommand.Create(Observable.Return(_exception != null))
                .DisposeWith(this);

            CopyDetailsCommand.Subscribe(o => CopyDetails())
                .DisposeWith(this);

            ExitCommand = ReactiveCommand.Create()
                .DisposeWith(this);

            ExitCommand.Subscribe(o => Exit())
                .DisposeWith(this);

            RestartCommand = ReactiveCommand.Create()
                .DisposeWith(this);

            RestartCommand.Subscribe(o => Restart())
                .DisposeWith(this);
        }

        private void CopyDetails()
        {
            _applicationService.CopyToClipboard(_exception.ToString());
        }

        private void Exit()
        {
            _applicationService.Exit();
        }

        private void Restart()
        {
            _applicationService.Restart();
        }
    }
}
