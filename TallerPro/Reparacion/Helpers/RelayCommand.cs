using System;
using System.Windows.Input;

namespace TallerPro.Reparacion.Helpers
{
    // Versión no genérica (para comandos sin parámetros como Guardar o Limpiar)
    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool>? canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute();
        }

        public void Execute(object? parameter)
        {
            execute();
        }

        public event EventHandler? CanExecuteChanged;

        public void ActualizarEstado()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // Versión genérica (para comandos con parámetros como SeleccionarEquipo)
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> execute;
        private readonly Func<T?, bool>? canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
            {
                return canExecute == null || canExecute(default);
            }

            return canExecute == null || canExecute((T?)parameter);
        }

        public void Execute(object? parameter)
        {
            execute((T?)parameter);
        }

        public event EventHandler? CanExecuteChanged;

        public void ActualizarEstado()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}