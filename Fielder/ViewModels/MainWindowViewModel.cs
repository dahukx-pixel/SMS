using Fielder.Bussiness.Services;
using Fielder.Domain.Models;
using Fielder.Properties;
using Fielder.Validators;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fielder.ViewModels
{
    public class MainWindowViewModel : MvxViewModel
    {
        private readonly FieldService _fieldService;

        private ObservableCollection<Field> _fields;

        private string _newFieldName = string.Empty;
        private string _newFieldValue = string.Empty;
        private string _newFieldComment = string.Empty;

        public string NewFieldName
        {
            get => _newFieldName;
            set => SetProperty(ref _newFieldName, value);
        }

        public string NewFieldValue
        {
            get => _newFieldValue;
            set => SetProperty(ref _newFieldValue, value);
        }

        public string NewFieldComment
        {
            get => _newFieldComment;
            set => SetProperty(ref _newFieldComment, value);
        }

        public ObservableCollection<Field> Fields
        {
            get => _fields;
            set => SetProperty(ref _fields, value);
        }

        public IMvxCommand RemoveFieldCommand => new MvxCommand<Field>(RemoveField);
        public IMvxCommand AddFieldCommand => new MvxCommand(AddField);

        public MainWindowViewModel()
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Resources.FieldsPath);

            _fieldService = new FieldService(filePath);

            _ = InitializeFields();
        }

        private async void AddField()
        {
            var field = new Field
            {
                Name = NewFieldName,
                Value = NewFieldValue,
                Comment = NewFieldComment
            };

            FieldValidator validator = new FieldValidator();
            var results = validator.Validate(field);

            if (!results.IsValid)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var failure in results.Errors)
                {
                    sb.AppendLine(failure.ErrorMessage);
                }
                MessageBox.Show(sb.ToString(), "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _fields.Add(field);
            try
            {
                await _fieldService.AddField(field);
            }
            catch
            {
                //log
            }

            NewFieldName = string.Empty;
            NewFieldValue = string.Empty;
            NewFieldComment = string.Empty;
        }

        private async void RemoveField(Field field)
        {
            _fields.Remove(field);
            await _fieldService.RemoveField(field);
        }

        private async Task InitializeFields()
        {
            if (!_fieldService.IsInitialized)
            {
                await _fieldService.InitializingTask;
            }

            Fields = new ObservableCollection<Field>(_fieldService.GetAllFields());
        }
    }
}
