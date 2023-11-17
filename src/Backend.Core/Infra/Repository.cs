using System.Data;
using Backend.Core.Domain;
using Backend.Core.Models;
using Npgsql;
using NpgsqlTypes;

namespace Backend.Core.Infra
{
    public class Repository(NpgsqlConnection connection) : RepositoryBase(connection)
    {
        public async Task Insert(IEnumerable<Person> people)
        {
            using var batch = await CreateBatch();

            foreach (var person in people)
            {
                var command = batch.CreateBatchCommand();

                command.CommandText = Queries.Insert;
                command.Parameters.AddWithValue(person.Id);
                command.Parameters.AddWithValue(person.NickName);
                command.Parameters.AddWithValue(person.Name);
                command.Parameters.AddWithValue(person.BirthDate);
                command.Parameters.AddWithValue(person.Stacks);
                command.Parameters.AddWithValue(person.Search);

                batch.BatchCommands.Add(command);
            }

            await batch.ExecuteNonQueryAsync();
        }

        public async Task<PersonRequest> Get(Guid id)
        {
            using var command = await CreateCommand();

            command.CommandText = Queries.Get;

            var parameter = command.Parameters.AddWithValue(nameof(id), NpgsqlDbType.Uuid, id);

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
            using var command = await CreateCommand();

            command.CommandText = Queries.GetAll;

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
            using var command = await CreateCommand();

            command.CommandText = Queries.Count;

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
