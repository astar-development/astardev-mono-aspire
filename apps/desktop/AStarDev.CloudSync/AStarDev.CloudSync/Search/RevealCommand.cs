namespace AStarDev.CloudSync.Search;

/// <summary>A program and its arguments that show a file in the operating system's file manager.</summary>
/// <param name="Program">The program to run.</param>
/// <param name="Arguments">Its arguments, one per item.</param>
public sealed record RevealCommand(string Program, IReadOnlyList<string> Arguments);
