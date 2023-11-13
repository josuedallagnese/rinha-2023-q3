namespace Backend.Web.Domain
{
    public record Person(string NickName, string Name, DateOnly BirthDate, IEnumerable<string> Stack, string Stacks)
    {
        public const string BIRTH_FORMAT = "yyyy-MM-dd";
        public const string STACK_SEPARATOR = " ";

        public Guid Id { get; init; } = Guid.NewGuid();
        public string Stacks { get; set; } = Stacks;
        public string Search { get; set; }

        public Person(string NickName, string Name, DateOnly BirthDate, IEnumerable<string> stack)
            : this(NickName, Name, BirthDate, stack, Transform(stack))
        {
            Search = $"{NickName} {Name} {Stacks}";
        }

        public static string Transform(DateTime value) => value.ToString(BIRTH_FORMAT);
        public static string Transform(IEnumerable<string> values) => values == null ? string.Empty : string.Join(STACK_SEPARATOR, values.Select(s => s));
        public static IEnumerable<string> Transform(string values) => values.Split(STACK_SEPARATOR).ToArray();
    }
}
