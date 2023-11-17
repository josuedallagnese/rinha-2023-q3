using System.Data;
using Npgsql;

namespace Backend.Core.Infra
{
    public class RepositoryBase
    {
        private bool _disposedValue;
        private readonly NpgsqlConnection _connection;

        public RepositoryBase(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        ~RepositoryBase() => Dispose(false);

        protected async Task OpenConnection()
        {
            if (_connection.State == ConnectionState.Open)
                return;

            await _connection.OpenAsync();
        }

        protected async Task<NpgsqlCommand> CreateCommand()
        {
            await OpenConnection();

            return _connection.CreateCommand();
        }

        protected async Task<NpgsqlBatch> CreateBatch()
        {
            await OpenConnection();

            return _connection.CreateBatch();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _connection.Dispose();
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
