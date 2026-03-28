using System;
using System.Collections.Generic;

namespace BitzArt.UI.Tweaks.Services;

internal partial class GameStatusService
{
    private record DetailsSubscription(List<DetailRecord> Details, Action<object[]> Callback);
}
