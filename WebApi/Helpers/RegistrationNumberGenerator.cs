using WebApi.Data;

namespace WebApi.Helpers;

public static class RegistrationNumberGenerator
{
    private static readonly Dictionary<string, int> Counters = new();
    
    public static void Initialize(ApplicationDbContext context)
    {
        var categories = context.Queue
            .Where(q => !string.IsNullOrEmpty(q.QueueCode) && q.QueueCode.Length >= 1)
            .Select(q => q.QueueCode.Substring(0, 1))
            .Distinct()
            .ToList();

        foreach (var letter in categories)
        {
            var lastQueueCode = context.Queue
                .Where(q => !string.IsNullOrEmpty(q.QueueCode) && q.QueueCode.StartsWith(letter))
                .OrderByDescending(q => q.ID)
                .Select(q => q.QueueCode)
                .FirstOrDefault();

            var lastNumber = 0;

            if (!string.IsNullOrEmpty(lastQueueCode) && lastQueueCode.Length > 1)
            {
                var digits = lastQueueCode[1..];
                if (int.TryParse(digits, out var parsed))
                    lastNumber = (parsed + 1) % 100000;
            }

            Counters[letter] = lastNumber;
        }
    }
    
    public static string Generate(string letter)
    {
        Counters.TryAdd(letter, 0);

        var number = Counters[letter];
        Counters[letter] = (Counters[letter] + 1) % 100000;

        return $"{letter}{number:D5}";
    }
}