namespace SinaMN75U.Utils;

using System;
using System.Collections.Concurrent;

public static class ULog {
	private static readonly ConcurrentQueue<string> Buffer = new();

	static ULog() => Console.OutputEncoding = Encoding.UTF8;

	public static IReadOnlyList<string> GetLogs() => Buffer.Reverse().ToList();

	public static void ClearLogs() => Buffer.Clear();
	
	public static void Info(string message) => WriteWithColor($"[INFO] {message}", ConsoleColor.Blue);

	public static void Success(string message) => WriteWithColor($"[SUCCESS] {message}", ConsoleColor.Green);

	public static void Warning(string message) => WriteWithColor($"[WARNING] {message}", ConsoleColor.Yellow);

	public static void Error(string message) => WriteWithColor($"[ERROR] {message}", ConsoleColor.Red);

	public static void Error(Exception ex, string? context = null) {
		string message = context == null ? ex.Message : $"{context}: {ex.Message}";
		WriteWithColor($"[ERROR] {message}", ConsoleColor.Red);
		if (ex.StackTrace != null) WriteWithColor($"  StackTrace: {ex.StackTrace}", ConsoleColor.DarkRed);
	}

	public static void Debug(string message) => WriteWithColor($"[DEBUG] {message}", ConsoleColor.Cyan);
	
	private static void WriteWithColor(string message, ConsoleColor color) {
		string line = $"[{DateTime.Now:HH:mm:ss}] {message}";

		Buffer.Enqueue(line);
		while (Buffer.Count > 5000 && Buffer.TryDequeue(out _)) { }

		ConsoleColor original = Console.ForegroundColor;
		Console.ForegroundColor = color;
		Console.WriteLine(line);
		Console.ForegroundColor = original;
	}
}