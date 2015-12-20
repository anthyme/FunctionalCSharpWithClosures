using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;

namespace Closure
{
    public class AsyncCommand : RelayCommand
    {
        public AsyncCommand(Func<Task> execute) : base(ToSync(execute)) { }
        public AsyncCommand(Func<Task> execute, Func<bool> canExecute) : base(ToSync(execute), canExecute) { }

        private static Action ToSync(Func<Task> execute)
        {
            return async () =>
            {
                try { await execute(); }
                catch (Exception) { /* todo handle erros*/  }
            };
        }
    }

    public class AsyncCommand<T> : RelayCommand<T>
    {
        public AsyncCommand(Func<T, Task> execute) : base(ToSync(execute)) { }
        public AsyncCommand(Func<T, Task> execute, Func<T, bool> canExecute) : base(ToSync(execute), canExecute) { }

        private static Action<T> ToSync(Func<T, Task> execute)
        {
            return async x =>
            {
                try { await execute(x); }
                catch (Exception) { /* todo handle erros */  }
            };
        }
    }
}