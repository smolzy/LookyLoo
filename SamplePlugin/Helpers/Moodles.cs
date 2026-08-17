global using MoodlesStatusInfo = (
    int Version,
    System.Guid GUID,
    int IconID,
    string Title,
    string Description,
    string CustomVFXPath,
    long ExpireTicks,
    SamplePlugin.Helpers.StatusType Type,
    int Stacks,
    int StackSteps,
    uint Modifiers,
    System.Guid ChainedStatus,
    SamplePlugin.Helpers.ChainTrigger ChainTrigger,
    string Applier,
    string Dispeller,
    bool Permanent
);

using System;
using System.Text.RegularExpressions;
using System.Numerics;

namespace SamplePlugin.Helpers;

public enum StatusType
{
    Positive, Negative, Special
}

public enum ChainTrigger
{
    Dispel = 0,
    HitMaxStacks = 1,
    TimerExpired = 2,
}

public static class MoodlesHelper
{
    private static readonly Regex MoodlesTagRegex =
        new(@"\[/?(?:color|glow|i|b|u|s)(?:=[^\]]+)?\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string CleanMoodlesText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return MoodlesTagRegex
            .Replace(text, string.Empty)
            .Replace("&", string.Empty)
            .Trim();
    }

    public static Vector4 GetBuffTypeColor(StatusType type)
    {
        return type switch
        {
            StatusType.Positive => new Vector4(0.45f, 0.9f, 0.45f, 1f),
            StatusType.Negative => new Vector4(1f, 0.35f, 0.35f, 1f),
            StatusType.Special => new Vector4(0.55f, 0.75f, 1f, 1f),
            _ => new Vector4(1f, 1f, 1f, 1f)
        };
    }
}
