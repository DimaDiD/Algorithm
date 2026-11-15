﻿using System.Reflection;



namespace MMSA.Health;



public static class AppVersion
{

    public static string Value { get; } =

        typeof(AppVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion

        ?? "unknown";



    public static string Commit { get; } =

        Value.Contains('+') ? Value[(Value.IndexOf('+') + 1)..] : "local";

}

