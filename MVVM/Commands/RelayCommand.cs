using System;
using System.Windows.Input;

namespace ReolMarked.MVVM.Commands
{
    /// <summary>
    /// RelayCommand klasse til at håndtere knap klik og andre UI kommandoer
    /// Bruges til at koble knapper i View til metoder i ViewModel
    /// </summary>
    public class RelayCommand : ICommand
    {
        // Private felter til at gemme de metoder der skal køres
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // Konstruktør uden CanExecute - kommandoen kan altid køres
        /// <summary>
        /// Opretter en ny RelayCommand der altid kan køres
        /// </summary>
        /// <param name="execute">Metoden der skal køres når kommandoen kaldes</param>
        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        // Konstruktør med CanExecute - kan kontrollere om kommandoen må køres
        /// <summary>
        /// Opretter en ny RelayCommand med kontrol over om den kan køres
        /// </summary>
        /// <param name="execute">Metoden der skal køres når kommandoen kaldes</param>
        /// <param name="canExecute">Metoden der tjekker om kommandoen må køres</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            // Tjek at execute metoden ikke er null
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        // ICommand interface implementation

        /// <summary>
        /// Event der bliver kaldt når CanExecute status ændres
        /// WPF bruger dette til at opdatere knapper (enable/disable)
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Tjekker om kommandoen kan køres lige nu
        /// </summary>
        /// <param name="parameter">Parameter fra UI (kan være null)</param>
        /// <returns>True hvis kommandoen kan køres, ellers false</returns>
        public bool CanExecute(object parameter)
        {
            // Hvis der ikke er nogen CanExecute metode, kan kommandoen altid køres
            if (_canExecute == null)
                return true;

            // Kør CanExecute metoden og returner resultatet
            return _canExecute(parameter);
        }

        /// <summary>
        /// Kører kommandoen
        /// </summary>
        /// <param name="parameter">Parameter fra UI (kan være null)</param>
        public void Execute(object parameter)
        {
            // Kør execute metoden
            _execute(parameter);
        }

        /// <summary>
        /// Tvinger WPF til at tjekke CanExecute igen
        /// Bruges når noget ændres der påvirker om kommandoen kan køres
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}