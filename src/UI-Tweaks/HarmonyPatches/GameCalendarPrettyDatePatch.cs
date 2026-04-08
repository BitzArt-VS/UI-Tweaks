using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Vintagestory.Common;

namespace BitzArt.UI.Tweaks;

/// <summary>
/// A patch to adjust the year displayed by GameCalendar.PrettyDate by adding 1 to it.
/// </summary>
[HarmonyPatch(typeof(GameCalendar), nameof(GameCalendar.PrettyDate))]
internal static class GameCalendarPrettyDatePatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var getYear = AccessTools.PropertyGetter(typeof(GameCalendar), nameof(GameCalendar.Year));

        foreach (var instruction in instructions)
        {
            yield return instruction;

            if (instruction.Calls(getYear))
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                yield return new CodeInstruction(OpCodes.Add);
            }
        }
    }
}
