using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Windows.Controls;
using Brite.App.Win.Commands;
using Brite.App.Win.Services;
using Brite.Utility.Hardware.Serial;
using MahApps.Metro.IconPacks;
using Brite.App.Win.Extensions;
using System;
using DynamicData;

namespace Brite.App.Win.ViewModels.Pages
{
    public sealed class DevicesPageViewModel : BaseViewModel, IPageViewModel
    {
        public int Order => 1;
        public string Title => "Devices";
        public Control Icon => new PackIconModern { Kind = PackIconModernKind.HardwareCpu };

        private readonly IBriteService _briteService;
        
        public IList<SerialDeviceInfo> HardwareDevices { get; private set; }
        public ReactiveCommand RefreshHardwareDevicesCommand { get; }

        public DevicesPageViewModel(IBriteService briteService)
        {
            _briteService = briteService;

            // Create commands
            RefreshHardwareDevicesCommand = ReactiveCommand.Create()
                .DisposeWith(this);
            RefreshHardwareDevicesCommand.Subscribe(o => RefreshHardwareDevices())
                .DisposeWith(this);
        }

        private async void RefreshHardwareDevices()
        {
            HardwareDevices = await _briteService.GetHardwareDevicesAsync();
            this.RaisePropertyChanged(() => HardwareDevices);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
