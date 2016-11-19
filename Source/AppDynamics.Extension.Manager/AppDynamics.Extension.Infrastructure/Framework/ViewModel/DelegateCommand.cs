using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace AppDynamics.Infrastructure.Framework.ViewModel
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Predicate<object> _canExecute;        

        /// <summary>
        /// Creates new command that can always execute
        /// </summary>        
        /// <param name="action">The execution logic.</param>
        public DelegateCommand(Action<object> action)
            :this(action,null)
        {
            
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="action">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public DelegateCommand(Action<object> action, Predicate<object> canExecute)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            this._action = action;
            this._canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);     
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
