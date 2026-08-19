namespace SinaMN75U.Utils;

using System;
using System.IO;
using System.Collections.Concurrent;

public enum ULogLevel {
	Info = 0,
	Success = 1,
	Warning = 2,
	Error = 3,
	Debug = 4
}

public sealed class ULogEntry {
	public DateTime Time { get; init; }
	public string Level { get; init; } = "";
	public string Message { get; init; } = "";
}

public static class ULog {
	public static bool EnableDebug { get; set; } = false;

	private static string? _logFilePath;

	private const int MaxBufferedLogs = 2000;
	private static readonly ConcurrentQueue<ULogEntry> Buffer = new();

	static ULog() => Console.OutputEncoding = Encoding.UTF8;

	// Newest first, so a "take N" on the API returns the most recent entries.
	public static IReadOnlyList<ULogEntry> GetLogs() => Buffer.Reverse().ToList();

	public static void ClearLogs() => Buffer.Clear();

	private static void Capture(string level, string message) {
		Buffer.Enqueue(new ULogEntry { Time = DateTime.Now, Level = level, Message = message });
		while (Buffer.Count > MaxBufferedLogs && Buffer.TryDequeue(out _)) { }
	}

	public static void EnableFileLogging(string filePath) {
		_logFilePath = filePath;
		string? directory = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
	}

	public static void Info(string message) {
		WriteWithColor($"[INFO] {message}", ConsoleColor.DarkBlue);
		LogToFile("INFO", message);
	}

	public static void Success(string message) {
		WriteWithColor($"[SUCCESS] {message}", ConsoleColor.Green);
		LogToFile("SUCCESS", message);
	}

	public static void Warning(string message) {
		WriteWithColor($"[WARNING] {message}", ConsoleColor.Yellow);
		LogToFile("WARNING", message);
	}

	public static void Error(string message) {
		WriteWithColor($"[ERROR] {message}", ConsoleColor.Red);
		LogToFile("ERROR", message);
	}

	public static void Error(Exception ex, string? context = null) {
		string message = context == null
			? ex.Message
			: $"{context}: {ex.Message}";

		WriteWithColor($"[ERROR] {message}", ConsoleColor.Red);

		if (ex.StackTrace != null) {
			WriteWithColor($"  StackTrace: {ex.StackTrace}", ConsoleColor.DarkRed);
		}

		LogToFile("ERROR", $"{message}\nStackTrace: {ex.StackTrace}");
	}

	public static void Debug(string message) {
		if (!EnableDebug) return;

		WriteWithColor($"[DEBUG] {message}", ConsoleColor.Cyan);
		LogToFile("DEBUG", message);
	}

	public static void Log(ULogLevel level, string message, ConsoleColor? customColor = null) {
		ConsoleColor color = customColor ?? GetDefaultColorForLevel(level);
		string levelName = level.ToString().ToUpper();

		WriteWithColor($"[{levelName}] {message}", color);
		LogToFile(levelName, message);
	}

	public static void WriteColored(string message, ConsoleColor color) {
		ConsoleColor originalColor = Console.ForegroundColor;
		Console.ForegroundColor = color;
		Console.Write(message);
		Console.ForegroundColor = originalColor;
	}

	public static void WriteLineColored(string message, ConsoleColor color) {
		ConsoleColor originalColor = Console.ForegroundColor;
		Console.ForegroundColor = color;
		Console.WriteLine(message);
		Console.ForegroundColor = originalColor;
	}

	public static void Separator(char character = '=', ConsoleColor color = ConsoleColor.DarkGray) {
		int width = Console.WindowWidth - 1;
		if (width <= 0) width = 80;

		string line = new string(character, Math.Min(width, 120));
		WriteLineColored(line, color);
	}

	public static void ShowProgress(string message, Action action) {
		char[] spinChars = ['|', '/', '-', '\\'];
		int spinIndex = 0;
		ConsoleColor originalColor = Console.ForegroundColor;

		Console.Write($"{message} ");


		Task task = Task.Run(action);

		while (!task.IsCompleted) {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(spinChars[spinIndex % spinChars.Length]);
			Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
			spinIndex++;
			Thread.Sleep(100);
		}


		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("✓");
		Console.ForegroundColor = originalColor;


		task.Wait();
	}

	private static void WriteWithColor(string message, ConsoleColor color) {
		ConsoleColor originalColor = Console.ForegroundColor;
		Console.ForegroundColor = color;


		string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";

		Console.WriteLine(timestampedMessage);
		Console.ForegroundColor = originalColor;
	}

	private static ConsoleColor GetDefaultColorForLevel(ULogLevel level) {
		return level switch {
			ULogLevel.Info => ConsoleColor.DarkBlue,
			ULogLevel.Success => ConsoleColor.Green,
			ULogLevel.Warning => ConsoleColor.Yellow,
			ULogLevel.Error => ConsoleColor.Red,
			ULogLevel.Debug => ConsoleColor.Cyan,
			_ => ConsoleColor.DarkBlue
		};
	}

	private static void LogToFile(string level, string message) {
		Capture(level, message);

		if (string.IsNullOrEmpty(_logFilePath)) return;

		try {
			string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
			File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
		}
		catch {
			// ignored
		}
	}
}