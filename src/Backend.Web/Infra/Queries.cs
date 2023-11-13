namespace Backend.Web.Infra
{
    public static class Queries
    {
        public const string GetAll = "SELECT * FROM people WHERE search ILIKE @t ORDER BY nickname LIMIT 50";
        public const string Get = "SELECT nickname, name, birthdate, stack FROM people WHERE id = @id";
        public const string Count = "SELECT count(1) FROM people";
        public const string Insert = "INSERT INTO people (id, nickname, name, birthdate, stack, search) VALUES ($1, $2, $3, $4, $5, $6) ON CONFLICT DO NOTHING;";
    }
}
