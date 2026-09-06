using System;

namespace Reminder.Models
{
    /// <summary>
    /// Класс, где собрано обращение ко времени в одном месте.
    /// Полезен для диагностических целей и тестирования конкретного
    /// времени без перевода часов в операционной системе.
    /// </summary>
    internal static class TimeProvider
    {
        private static ITimeProviderScope _scope = BuildInScope.Instance;

        public static DateTime Now { get { return _scope.Now; } }
        public static DateTime Today { get { return _scope.Today; } }

        public static IDisposable CreateScope()
        {
            if (_scope != BuildInScope.Instance) throw new InvalidOperationException();
            _scope = new DiagnosticScope();
            return _scope;
        }

        private interface ITimeProviderScope : IDisposable
        {
            DateTime Now { get; }
            DateTime Today { get; }
        }

        class BuildInScope : ITimeProviderScope
        {
            public static BuildInScope Instance { get; } = new BuildInScope();
            private BuildInScope() { }

            DateTime ITimeProviderScope.Now => DateTime.Now;

            DateTime ITimeProviderScope.Today => DateTime.Today;

            void IDisposable.Dispose()
            {
                throw new NotSupportedException();
            }
        }

        class DiagnosticScope : DisposableObject, ITimeProviderScope
        {
            private int _nowCounter;
            private int _todayCounter;

            public DateTime Now
            {
                get 
                {
                    ++_nowCounter;
                    return DateTime.Now;
                }
            }

            public DateTime Today
            {
                get
                {
                    ++_todayCounter;
                    return DateTime.Today;
                }
            }

            protected override void DisposeManaged()
            {
                Console.WriteLine($"Обращений к Now {_nowCounter}, обращений Today {_todayCounter}.");
                _scope = BuildInScope.Instance;
            }
        }

        class DateBasedScope : DisposableObject, ITimeProviderScope
        {
            readonly DateTime _date;

            public DateTime Now => _date + DateTime.Now.TimeOfDay;

            public DateTime Today => _date;

            public DateBasedScope(DateTime date)
            { 
                _date = date.Date;
            }

            protected override void DisposeManaged()
            {
                _scope = BuildInScope.Instance;
            }
        }
    }
}
