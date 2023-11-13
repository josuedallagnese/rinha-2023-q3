using Backend.Core.Repositories;
using Backend.Web.Domain;
using Backend.Web.Models;
using Npgsql;
using System.Data;

namespace Backend.Web.Infra
{
    public class ReadRepository : RepositoryBase<NpgsqlConnection>
    {
        public ReadRepository(NpgsqlConnection connection)
            : base(connection)
        {
        }

        public async Task<PersonRequest> Get(Guid id)
        {
            await Connection.OpenAsync();

            using var command = new NpgsqlCommand(Queries.Get, Connection);

            var parameter = command.Parameters.AddWithValue(nameof(id), id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var pessoa = Read(id, reader);

                return pessoa;
            }

            return null;
        }

        public async Task<IEnumerable<PersonRequest>> GetAll(string t)
        {
            await Connection.OpenAsync();

            using var command = new NpgsqlCommand(Queries.GetAll, Connection);

            var parameter = command.Parameters.AddWithValue(nameof(t), $"%{t}%");

            using var reader = await command.ExecuteReaderAsync();

            var people = new List<PersonRequest>();

            while (await reader.ReadAsync())
            {
                var id = reader.GetGuid(nameof(Person.Id));

                var pessoa = Read(id, reader);

                people.Add(pessoa);
            }

            return people;
        }

        public async Task<long> Count()
        {
            await Connection.OpenAsync();

            using var command = new NpgsqlCommand(Queries.Count, Connection);

            var count = (long)await command.ExecuteScalarAsync();

            return count;
        }

        private static PersonRequest Read(Guid id, NpgsqlDataReader reader)
        {
            var pessoa = new PersonRequest()
            {
                Id = id,
                NickName = reader.GetString(nameof(Person.NickName)),
                BirthDate = Person.Transform(reader.GetDateTime(nameof(Person.BirthDate))),
                Name = reader.GetString(nameof(Person.Name)),
                Stack = Person.Transform(reader.GetString(nameof(Person.Stack))),
            };

            return pessoa;
        }
    }
}
