using Fielder.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fielder.Bussiness.Services
{
    public class FieldService
    {
        private readonly string _filePath;
        private List<Field> _fields;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;
        public Task InitializingTask;

        public FieldService(string filePath)
        {
            _isInitialized = false;
            _filePath = filePath;
            _fields = new List<Field>();

            InitializingTask = InitializeFields();
        }

        private async Task InitializeFields()
        {
            var directoryPath = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose();
                return;
            }

            var lines = await File.ReadAllLinesAsync(_filePath);

            foreach (var line in lines)
            {
                try
                {
                    var field = JsonConvert.DeserializeObject<Field>(line);
                    if (field != null)
                    {
                        _fields.Add(field);
                    }
                }
                catch (JsonException)
                {
                    // Log or handle the error as needed
                }
            }

            _isInitialized = true;
        }

        public List<Field> GetAllFields()
        {
            return _fields;
        }

        public async Task AddField(Field field)
        {
            if (_fields.Any(f => f.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A field with the name '{field.Name}' already exists.");
            }

            using (StreamWriter sw = new StreamWriter(_filePath, true))
            {
                await sw.WriteLineAsync(JsonConvert.SerializeObject(field));
            }
        }

        public async Task RemoveField(Field field)
        {
            _fields.Remove(field);

            using (StreamWriter sw = new StreamWriter(_filePath, false))
            {
                foreach (var fl in _fields)
                {
                    await sw.WriteLineAsync(JsonConvert.SerializeObject(fl));
                }
            }
        }
    }
}
