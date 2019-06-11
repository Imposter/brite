using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Brite.Win.App.Daemon
{
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