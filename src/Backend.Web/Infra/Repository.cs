using Backend.Core.Repositories;
using Backend.Web.Domain;
using Npgsql;

namespace Backend.Web.Infra
{
    public class Repository : RepositoryBase<NpgsqlConnection>
    {
        public Repository(NpgsqlConnection connection) : base(connection)
        {
        }

        public async Task Insert(IEnumerable<Person> people)
        {
            await Connection.OpenAsync();

            using var batch = Connection.CreateBatch();

            foreach (var pessoa in people)
            {
                var command = batch.CreateBatchCommand();

                command.CommandText = Queries.Insert;
                command.Parameters.AddWithValue(pessoa.Id);
                command.Parameters.AddWithValue(pessoa.NickName);
                command.Parameters.AddWithValue(pessoa.Name);
                command.Parameters.AddWithValue(pessoa.BirthDate);
                command.Parameters.AddWithValue(pessoa.Stacks);
                command.Parameters.AddWithValue(pessoa.Search);

                batch.BatchCommands.Add(command);
            }

            await batch.ExecuteNonQueryAsync();
        }
    }
}
