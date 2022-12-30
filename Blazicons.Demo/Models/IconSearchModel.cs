using System.ComponentModel;
using System.Reactive.Linq;
using BindingBits;

namespace Blazicons.Demo.Models;

public class IconSearchModel : ObservableObject
{
    public string? Query { get => Get<string?>(); set => Set(value); }

    public IObservable<string?> WhenPropertyChanged
    {
        get
        {
            return Observable
                .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    x => this.PropertyChanged += x,
                    x => this.PropertyChanged -= x)
                .Select(x => x.EventArgs.PropertyName);
        }
    }
}