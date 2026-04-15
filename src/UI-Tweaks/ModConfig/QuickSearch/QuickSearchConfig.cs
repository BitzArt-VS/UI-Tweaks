using Newtonsoft.Json;
using System.ComponentModel;

namespace BitzArt.UI.Tweaks.Config;

public record QuickSearchConfig : INotifyPropertyChanged
{
    [JsonProperty("enable", Order = 1)]
    public bool Enable { get; set; } = true;

    [JsonProperty("resultListHeight", Order = 2)]
    public int ResultListHeight { get; set; } = 200;

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

