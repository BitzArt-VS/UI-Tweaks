using Newtonsoft.Json;
using System.ComponentModel;

namespace BitzArt.UI.Tweaks.Config;

public record GameTweaksConfig : INotifyPropertyChanged
{
    [JsonProperty("correctCalendarYear", Order = 1)]
    public bool CorrectCalendarYear { get; set; } = true;

    [JsonProperty("showMealRemainingIndicator", Order = 2)]
    public bool ShowMealRemainingIndicator { get; set; } = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
