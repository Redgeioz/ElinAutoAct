﻿using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace AutoActMod;

[BepInPlugin("redgeioz.plugin.AutoAct", "AutoAct", "1.0.0")]
public class AutoActMod : BaseUnityPlugin
{
    void Awake()
    {
        Instance = this;
        Settings.startFromCenter = Config.Bind("Settings", "StartFromCenter", true);
        Settings.detDistSq = Config.Bind("Settings", "DetectionRangeSquared", 100, "Sqaure of detection range.");
        Settings.buildRangeW = Config.Bind("Settings", "BuildingRangeW", 5);
        Settings.buildRangeH = Config.Bind("Settings", "BuildingRangeH", 5);
        Settings.sowRangeExists = Config.Bind("Settings", "SowingRangeExists", false);
        Settings.pourDepth = Config.Bind("Settings", "PouringDepth", 1, "The depth of water pouring");
        Settings.seedReapingCount = Config.Bind("Settings", "SeedReapingCount", 25);
        Settings.staminaCheck = Config.Bind("Settings", "StaminaCheck", true);
        Settings.ignoreEnemySpotted = Config.Bind("Settings", "IgnoreEnemySpotted", true);
        Settings.simpleIdentify = Config.Bind("Settings", "SimpleIdentify", false);
        Settings.sameFarmfieldOnly = Config.Bind("Settings", "SameFarmfieldOnly", true, "Only auto harvest the plants on the same farmfield.");
        Settings.keyMode = Config.Bind("Settings", "KeyMode", false, "false = Press, true = Toggle");
        Settings.keyCode = Config.Bind("Settings", "KeyCode", KeyCode.LeftShift);
        new Harmony("AutoActMod").PatchAll();
    }

    void Update()
    {
        if (!Settings.KeyMode)
        {
            return;
        }

        if (Input.GetKeyDown(Settings.KeyCode))
        {
            switchOn = !switchOn;
            Say(AALang.GetText(switchOn ? "on" : "off"));
        }
    }

    public static void Say(string text)
    {
        Msg.SetColor(Msg.colors.TalkGod);
        Msg.Say(text);
    }

    internal static void Log(object payload)
    {
#if DEBUG
        Instance.Logger.LogMessage(payload);
#else
        Instance.Logger.LogInfo(payload);
#endif
    }

    internal static void LogWarning(object payload)
    {
        Instance.Logger.LogWarning(payload);
    }

    public static bool active = false;
    public static bool switchOn = false;
    public static bool IsSwitchOn => Settings.KeyMode ? switchOn : Input.GetKey(Settings.KeyCode);
    public static Point lastHitPoint = Point.Zero;
    public static AutoActMod Instance { get; private set; }
}

public static class Utils
{
    public static void Trace()
    {
        var stackTrace = new System.Diagnostics.StackTrace(true);
        AutoActMod.Log($"StackTrace:");
        foreach (var frame in stackTrace.GetFrames())
        {
            var method = frame.GetMethod();
            if (method.HasValue())
            {
                AutoActMod.Log($"\t{method.DeclaringType?.Name}.{method}");
            }
        }
    }

    public static int Dist2(Point p1, Point p2)
    {
        var dx = p1.x - p2.x;
        var dz = p1.z - p2.z;
        return dx * dx + dz * dz;
    }

    public static int MaxDelta(Point p1, Point p2)
    {
        var dx = Math.Abs(p1.x - p2.x);
        var dz = Math.Abs(p1.z - p2.z);
        return Math.Max(dx, dz);
    }

    public static int GetBit(this int n, int digit) => (n >> digit) & 1;

    public static bool HasValue(this object obj) => obj != null;
}