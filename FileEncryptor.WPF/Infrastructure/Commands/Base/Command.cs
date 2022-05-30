﻿using System;
using System.Windows.Input;

namespace FileEncryptor.WPF.Infrastructure.Commands.Base
{
    internal abstract class Command : ICommand
    {
        bool ICommand.CanExecute(object? parameter) => _Executable && CanExecute(parameter);

        void ICommand.Execute(object? parameter)
        {
            if(CanExecute(parameter))
                Execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private bool _Executable = true;

        public bool Executable
        {
            get => _Executable;
            set
            {
                if (_Executable == value) return;
                _Executable = value;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        protected virtual bool CanExecute(object parameter) => true;

        protected abstract void Execute(object parameter);
    }
}