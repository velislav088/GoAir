namespace GoAir.Services.Common
{
    public class ServiceResult
    {
        private readonly Dictionary<string, List<string>> errors = new(StringComparer.OrdinalIgnoreCase);

        public bool Succeeded => errors.Count == 0 && !NotFound;

        public bool NotFound { get; private set; }

        public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Errors =>
            errors.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyCollection<string>)kvp.Value.AsReadOnly(), StringComparer.OrdinalIgnoreCase);

        public static ServiceResult Success() => new();

        public static ServiceResult Missing() => new() { NotFound = true };

        public static ServiceResult Failure(string key, string message)
        {
            ServiceResult result = new();
            result.AddError(key, message);
            return result;
        }

        public void AddError(string key, string message)
        {
            string normalizedKey = string.IsNullOrWhiteSpace(key) ? string.Empty : key;

            if (!errors.TryGetValue(normalizedKey, out List<string>? fieldErrors))
            {
                fieldErrors = [];
                errors[normalizedKey] = fieldErrors;
            }

            fieldErrors.Add(message);
        }
    }
}