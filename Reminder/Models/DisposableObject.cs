using System;

namespace Reminder.Models
{
    public class DisposableObject
    {
        private bool _disposed;

        protected DisposableObject()
        {
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeManaged();
                }
                _disposed = true;
            }
        }

        protected virtual void DisposeManaged()
        { 
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}