using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Brite.UWP.App.Core
{
    // TODO: Don't do everything in the windows AppServices thing, use Debug builds for release? -- figure out a way to keep our debug privileges and have optimized code
    // TODO: Only use the Daemon as a background service for notifications?? or any tasks the frontend wants to run in the background?
    class DynamicViewModel : INotifyPropertyChanged
    {
        private readonly ExpandoObject _obj;

        public object this[string key]
        {
            get => GetProperty<object>(key);
            set => SetProperty(value, key);
        }

        public DynamicViewModel()
        {
            _obj = new ExpandoObject();
        }

        private T GetProperty<T>([CallerMemberName] string propertyName = "")
        {
            var objDict = (IDictionary<string, object>)_obj;
            return (T)objDict[propertyName];
        }

        private void SetProperty<T>(T newValue, [CallerMemberName] string propertyName = "")
        {
            var objDict = (IDictionary<string, object>)_obj;
            if (objDict.ContainsKey(propertyName) && Equals(objDict[propertyName], newValue))
                return;

            objDict[propertyName] = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
