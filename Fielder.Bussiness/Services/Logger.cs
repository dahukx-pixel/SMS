using Fielder.Bussiness.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fielder.Bussiness.Services
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static readonly string _logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        private static readonly string _logFileNamePrefix = "test-sms-wpf-app-";
        private static readonly string _logFileExtension = ".log";
        private static readonly LogLevel _minLogLevel = LogLevel.Debug; // Минимальный уровень для записи

        // Кэшируем имя файла текущего дня для избежания частых вычислений
        private static string _currentLogFileName = string.Empty;
        private static DateTime _lastDateChecked = DateTime.MinValue;

        /// <summary>
        /// Записывает сообщение уровня DEBUG
        /// </summary>
        public static void Debug(string message, Exception? exception = null) =>
            Log(LogLevel.Debug, message, exception);

        /// <summary>
        /// Записывает сообщение уровня INFO
        /// </summary>
        public static void Info(string message, Exception? exception = null) =>
            Log(LogLevel.Info, message, exception);

        /// <summary>
        /// Записывает сообщение уровня WARNING
        /// </summary>
        public static void Warning(string message, Exception? exception = null) =>
            Log(LogLevel.Warning, message, exception);

        /// <summary>
        /// Записывает сообщение уровня ERROR
        /// </summary>
        public static void Error(string message, Exception? exception = null) =>
            Log(LogLevel.Error, message, exception);

        /// <summary>
        /// Записывает сообщение уровня CRITICAL
        /// </summary>
        public static void Critical(string message, Exception? exception = null) =>
            Log(LogLevel.Critical, message, exception);

        /// <summary>
        /// Основной метод записи лога
        /// </summary>
        public static void Log(LogLevel level, string message, Exception? exception = null)
        {
            // Фильтрация по минимальному уровню логирования
            if (level < _minLogLevel)
                return;

            // Формируем строку лога
            var logEntry = FormatLogEntry(level, message, exception);

            // Потокобезопасная запись
            lock (_lock)
            {
                try
                {
                    // Убеждаемся, что директория существует
                    Directory.CreateDirectory(_logDirectory);

                    // Получаем путь к файлу лога за текущую дату
                    var logFilePath = GetLogFilePathForToday();

                    // Записываем в файл
                    File.AppendAllText(logFilePath, logEntry, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    // Критически важно: логгер НЕ должен падать и крашить приложение
                    // В крайнем случае выводим ошибку в stderr
                    Console.Error.WriteLine($"[LOGGER FAILURE] {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Форматирует строку лога с временной меткой
        /// Формат: [2026-02-02 15:30:45.123] [INFO] Message
        /// </summary>
        private static string FormatLogEntry(LogLevel level, string message, Exception? exception)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var levelStr = level.ToString().ToUpperInvariant();
            var entry = $"[{timestamp}] [{levelStr}] {message}{Environment.NewLine}";

            if (exception != null)
            {
                entry += $"[EXCEPTION] {exception.GetType().Name}: {exception.Message}{Environment.NewLine}";
                entry += $"[STACKTRACE] {exception.StackTrace}{Environment.NewLine}";
            }

            return entry;
        }

        /// <summary>
        /// Возвращает путь к файлу лога за текущую дату с ротацией по дате
        /// Формат: logs/test-sms-wpf-app-20260202.log
        /// </summary>
        private static string GetLogFilePathForToday()
        {
            var today = DateTime.Today;

            // Проверяем, нужно ли обновить кэшированное имя файла (раз в день)
            if (_lastDateChecked.Date != today)
            {
                var dateSuffix = today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                _currentLogFileName = $"{_logFileNamePrefix}{dateSuffix}{_logFileExtension}";
                _lastDateChecked = today;
            }

            return Path.Combine(_logDirectory, _currentLogFileName);
        }

        /// <summary>
        /// Опциональный вывод в консоль с цветовой дифференциацией уровней
        /// </summary>
        private static void WriteToConsole(LogLevel level, string logEntry)
        {
            // Сохраняем исходный цвет консоли
            var originalColor = Console.ForegroundColor;

            try
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Critical:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                }

                // Выводим только первые 200 символов для консоли (чтобы не засорять)
                var consoleMessage = logEntry.Length > 200
                    ? logEntry.Substring(0, 200) + "..."
                    : logEntry;

                Console.Write(consoleMessage);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        /// <summary>
        /// Принудительная очистка кэша имени файла (для тестирования)
        /// </summary>
        internal static void ResetCache()
        {
            lock (_lock)
            {
                _currentLogFileName = string.Empty;
                _lastDateChecked = DateTime.MinValue;
            }
        }
    }
}
