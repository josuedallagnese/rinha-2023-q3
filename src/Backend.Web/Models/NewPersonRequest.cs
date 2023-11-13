using System.Text.Json.Serialization;
using Backend.Core.Validations;
using Backend.Web.Domain;

namespace Backend.Web.Models
{
    public class NewPersonRequest : RequestValidation
    {
        [JsonPropertyName("apelido")]
        public string Apelido { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("nascimento")]
        public string Nascimento { get; set; }

        [JsonPropertyName("stack")]
        public IEnumerable<string> Stack { get; set; }

        [JsonIgnore]
        public DateOnly DataNascimento { get; set; }

        public override bool Validate()
        {
            if (InvalidLength(Nome, 100))
                return false;

            if (InvalidLength(Apelido, 32))
                return false;

            var invalidDate = InvalidDate(Nascimento, Person.BIRTH_FORMAT, out var data);
            DataNascimento = data;

            if (invalidDate)
                return false;

            foreach (var item in Stack ?? Enumerable.Empty<string>())
                if (InvalidLength(item, 32))
                    return false;

            return true;
        }
    }
}
