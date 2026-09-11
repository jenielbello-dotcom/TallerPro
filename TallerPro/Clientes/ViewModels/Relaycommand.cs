using System;
using System.Windows.Input;

namespace TallerPro.Clientes.ViewModels
{
    /// <summary>
    /// Implementación genérica de ICommand para enlazar métodos del
    /// ViewModel directamente a botones (Guardar, Limpiar, Eliminar, Buscar).
    /// Si tu proyecto ya tiene una clase RelayCommand en otro lugar,
    /// elimina este archivo para evitar un nombre duplicado.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool>? canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => execute();
    }
}