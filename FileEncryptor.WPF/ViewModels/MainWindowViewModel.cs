﻿using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using FileEncryptor.WPF.Infrastructure.Commands;
using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.ViewModels.Base;

namespace FileEncryptor.WPF.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private const string __EncryptedFileSuffix = ".encrypted";

        private readonly IUserDialog _UserDialog;
        private readonly IEncryptor _Encryptor;

        #region Title : string - Заголовок окна

        /// <summary>Заголовок окна</summary>
        private string _Title = "Шифратор";

        /// <summary>Заголовок окна</summary>
        public string Title { get => _Title; set => Set(ref _Title, value); }

        #endregion

        #region Password : string - Пароль

        /// <summary>Пароль</summary>
        private string _Password = "123";

        /// <summary>Пароль</summary>
        public string Password { get => _Password; set => Set(ref _Password, value); }

        #endregion

        #region SelectedFile : FileInfo - Выбранный файл

        /// <summary>Выбранный файл</summary>
        private FileInfo _SelectedFile;

        /// <summary>Выбранный файл</summary>
        public FileInfo SelectedFile { get => _SelectedFile; set => Set(ref _SelectedFile, value); }

        #endregion

        #region Команды

        #region SelectFileCommand

        private ICommand _SelectFileCommand;

        public ICommand SelectFileCommand => _SelectFileCommand ??= new LambdaCommand(OnSelectFileCommandExecuted);

        private void OnSelectFileCommandExecuted()
        {
            if (!_UserDialog.OpenFile("Выбор файла для шифрования", out var file_path)) return;
            var selected_file = new FileInfo(file_path);
            SelectedFile = selected_file.Exists ? selected_file : null;
        }

        #endregion

        #region EncryptCommand

        private ICommand _EncryptCommand;

        public ICommand EncryptCommand => _EncryptCommand ??= new LambdaCommand(OnEncryptCommandExecuted, CanEncryptCommandExecute);

        private bool CanEncryptCommandExecute(object p) => (p is FileInfo file && file.Exists || SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

        private async void OnEncryptCommandExecuted(object p)
        {
            var file = p as FileInfo ?? SelectedFile;
            if (file is null) return;

            var default_file_name = file.FullName + __EncryptedFileSuffix;
            if (!_UserDialog.SaveFile("Выбор файл для сохранения", out var destination_path, default_file_name)) return;

            var timer = Stopwatch.StartNew();

            var encryption_task = _Encryptor.EncryptAsync(file.FullName, destination_path, Password);

            await encryption_task;

            timer.Stop();

            _UserDialog.Information("Шифрование", $"Шифрование файла успешно завершено за {timer.Elapsed.TotalSeconds:0.##} с");
        }

        #endregion

        #region DecryptCommand

        private ICommand _DecryptCommand;

        public ICommand DecryptCommand => _DecryptCommand ??= new LambdaCommand(OnDecryptCommandExecuted, CanDecryptCommandExecute);

        private bool CanDecryptCommandExecute(object p) => (p is FileInfo file && file.Exists || SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

        private void OnDecryptCommandExecuted(object p)
        {
            var file = p as FileInfo ?? SelectedFile;
            if (file is null) return;

            var default_file_name = file.FullName.EndsWith(__EncryptedFileSuffix)
                ? file.FullName.Substring(0, file.FullName.Length - __EncryptedFileSuffix.Length)
                : file.FullName;
            if (!_UserDialog.SaveFile("Выбор файл для сохранения", out var destination_path, default_file_name)) return;

            var timer = Stopwatch.StartNew();
            var success = _Encryptor.Decrypt(file.FullName, destination_path, Password);
            timer.Stop();

            if (success)
                _UserDialog.Information("Шифрование", $"Дешифровка файла выполнено успешно за {timer.Elapsed.TotalSeconds:0.##} с");
            else
                _UserDialog.Warning("Шифрование", "Ошибка при дешифровке файла: указан неверный пароль.");
        }

        #endregion

        #endregion

        public MainWindowViewModel(IUserDialog UserDialog, IEncryptor Encryptor)
        {
            _UserDialog = UserDialog;
            _Encryptor = Encryptor;
        }
    }
}