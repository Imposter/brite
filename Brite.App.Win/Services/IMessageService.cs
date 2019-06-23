using Brite.App.Win.Models;
using Brite.App.Win.ViewModels;
using System;

namespace Brite.App.Win.Services
{
    public interface IMessageService : IService
    {
        IObservable<Message> Show { get; }

        void Post(string title, ICloseableViewModel viewModel);
    }
}
