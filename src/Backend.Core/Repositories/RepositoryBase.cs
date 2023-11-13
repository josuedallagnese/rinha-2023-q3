namespace Backend.Core.Repositories
{
    public class RepositoryBase<DbConnection> : IDisposable
        where DbConnection : IDisposable
    {
        private bool _disposedValue;
        protected readonly DbConnection Connection;

        public RepositoryBase(DbConnection connection)
        {
            Connection = connection;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Connection.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
