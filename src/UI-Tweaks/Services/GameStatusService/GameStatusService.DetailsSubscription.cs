using System;
using System.Collections.Generic;

namespace BitzArt.UI.Tweaks.Services;

public partial class GameStatusService
{
    private record DetailsSubscription(List<DetailRecord> Details, Action<object[]> Callback);
}
