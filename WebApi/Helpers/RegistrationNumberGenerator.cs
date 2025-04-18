namespace WebApi.Helpers;

public static class RegistrationNumberGenerator
{
    private static readonly Dictionary<string, int> Counters = new();

    public static string Generate(string letter)
    {
        Counters.TryAdd(letter, 0);

        var number = Counters[letter];
        Counters[letter] = (Counters[letter] + 1) % 100000;

        return $"{letter}{number:D5}";
    }
}