using System.Text.Json.Serialization;

namespace Backend.Web.Models
{
    public class PersonRequest
    {
        public Guid Id { get; set; }

        [JsonPropertyName("apelido")]
        public string NickName { get; set; }

        [JsonPropertyName("nome")]
        public string Name { get; set; }

        [JsonPropertyName("nascimento")]
        public string BirthDate { get; set; }

        [JsonPropertyName("stack")]
        public IEnumerable<string> Stack { get; set; }
    }
}
