using Fielder.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fielder.Bussiness.Services
{
    public class FieldService
    {
        private readonly string _filePath;
        private readonly object _lock = new object(); // Для потокобезопасности операций записи
        private List<Field> _fields;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;
        public Task InitializingTask { get; }

        public FieldService(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _fields = new List<Field>();
            _isInitialized = false;

            Logger.Info($"Инициализация FieldService. Путь к файлу: {_filePath}");
            InitializingTask = InitializeFieldsAsync();
        }

        private async Task InitializeFieldsAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            Logger.Debug($"Начало инициализации полей из файла: {_filePath}");

            try
            {
                var directoryPath = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Logger.Info($"Создана директория для хранения данных: {directoryPath}");
                }

                if (!File.Exists(_filePath))
                {
                    File.Create(_filePath).Dispose();
                    Logger.Warning($"Файл данных не найден. Создан новый файл: {_filePath}");
                    _isInitialized = true;
                    Logger.Info($"Инициализация завершена (новый файл). Время: {stopwatch.ElapsedMilliseconds}мс. Полей: 0");
                    return;
                }

                var lines = await File.ReadAllLinesAsync(_filePath);
                Logger.Debug($"Загружено {lines.Length} строк из файла");

                int validCount = 0;
                int errorCount = 0;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var field = JsonConvert.DeserializeObject<Field>(line);
                        if (field != null)
                        {
                            _fields.Add(field);
                            validCount++;
                        }
                        else
                        {
                            Logger.Warning($"Пропущена пустая или некорректная запись: '{line}'");
                            errorCount++;
                        }
                    }
                    catch (JsonException jex)
                    {
                        Logger.Error($"Ошибка десериализации JSON: {jex.Message}", jex);
                        Logger.Debug($"Некорректная строка: '{line}'");
                        errorCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Неожиданная ошибка при обработке строки", ex);
                        errorCount++;
                    }
                }

                _isInitialized = true;
                stopwatch.Stop();

                Logger.Info($"Инициализация завершена успешно. Время: {stopwatch.ElapsedMilliseconds}мс. " +
                            $"Полей загружено: {validCount}, ошибок: {errorCount}, всего строк: {lines.Length}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.Critical($"Критическая ошибка инициализации FieldService", ex);
                throw; // Пробрасываем исключение, так как сервис неработоспособен
            }
        }

        public List<Field> GetAllFields()
        {
            if (!_isInitialized)
            {
                Logger.Warning("Попытка получить поля до завершения инициализации");
                throw new InvalidOperationException("FieldService не инициализирован. Дождитесь завершения InitializingTask.");
            }

            Logger.Debug($"Запрос всех полей. Количество: {_fields.Count}");
            return _fields.ToList(); // Возвращаем копию для безопасности
        }

        public async Task AddField(Field field)
        {
            if (!_isInitialized)
            {
                Logger.Warning("Попытка добавить поле до завершения инициализации");
                throw new InvalidOperationException("FieldService не инициализирован. Дождитесь завершения InitializingTask.");
            }

            if (field == null)
            {
                Logger.Error("Попытка добавить null поле");
                throw new ArgumentNullException(nameof(field));
            }

            if (string.IsNullOrWhiteSpace(field.Name))
            {
                Logger.Error("Попытка добавить поле с пустым именем");
                throw new ArgumentException("Имя поля не может быть пустым", nameof(field.Name));
            }

            // Проверка дубликата (регистронезависимо)
            var existing = _fields.FirstOrDefault(f =>
                f.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                Logger.Warning($"Попытка добавить дубликат поля '{field.Name}'.");
                throw new InvalidOperationException($"Поле с именем '{field.Name}'.");
            }

            var stopwatch = Stopwatch.StartNew();
            Logger.Info($"Добавление нового поля: {field.Name}");

            try
            {
                // Потокобезопасная запись в файл
                lock (_lock)
                {
                    using (var sw = new StreamWriter(_filePath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(JsonConvert.SerializeObject(field));
                    }
                }

                // Обновляем кэш в памяти
                _fields.Add(field);

                stopwatch.Stop();
                Logger.Info($"Поле успешно добавлено: {field.Name}. Время операции: {stopwatch.ElapsedMilliseconds}мс");
            }
            catch (IOException ioex)
            {
                Logger.Error($"Ошибка ввода-вывода при добавлении поля '{field.Name}'", ioex);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Неожиданная ошибка при добавлении поля '{field.Name}'", ex);
                throw;
            }
        }

        public async Task RemoveField(Field field)
        {
            if (!_isInitialized)
            {
                Logger.Warning("Попытка удалить поле до завершения инициализации");
                throw new InvalidOperationException("FieldService не инициализирован. Дождитесь завершения InitializingTask.");
            }

            if (field == null)
            {
                Logger.Error("Попытка удалить null поле");
                throw new ArgumentNullException(nameof(field));
            }

            var fieldToRemove = _fields.FirstOrDefault(f => f.Name == field.Name);
            if (fieldToRemove == null)
            {
                Logger.Warning($"Попытка удалить несуществующее поле {field.Name}");
                throw new InvalidOperationException($"Поле с Name '{field.Name}' не найдено в коллекции.");
            }

            var stopwatch = Stopwatch.StartNew();
            Logger.Info($"Удаление поля: {fieldToRemove.Name} (Name: {fieldToRemove.Name})");

            try
            {
                // Удаляем из памяти
                _fields.Remove(fieldToRemove);

                // Перезаписываем файл
                lock (_lock)
                {
                    using (var sw = new StreamWriter(_filePath, false, Encoding.UTF8))
                    {
                        foreach (var fl in _fields)
                        {
                            sw.WriteLine(JsonConvert.SerializeObject(fl));
                        }
                    }
                }

                stopwatch.Stop();
                Logger.Info($"Поле успешно удалено: {fieldToRemove.Name}. Осталось полей: {_fields.Count}. Время операции: {stopwatch.ElapsedMilliseconds}мс");
            }
            catch (IOException ioex)
            {
                // Восстанавливаем состояние в памяти при ошибке записи
                _fields.Add(fieldToRemove);
                Logger.Error($"Ошибка ввода-вывода при удалении поля '{fieldToRemove.Name}'. Состояние восстановлено в памяти.", ioex);
                throw;
            }
            catch (Exception ex)
            {
                // Восстанавливаем состояние в памяти при любой ошибке
                _fields.Add(fieldToRemove);
                Logger.Error($"Неожиданная ошибка при удалении поля '{fieldToRemove.Name}'. Состояние восстановлено в памяти.", ex);
                throw;
            }
        }

        /// <summary>
        /// Принудительная повторная инициализация сервиса (для сценариев горячей перезагрузки)
        /// </summary>
        public async Task ReloadAsync()
        {
            Logger.Info("Запуск принудительной перезагрузки FieldService");
            _isInitialized = false;
            _fields = new List<Field>();
            await InitializeFieldsAsync();
        }
    }
}