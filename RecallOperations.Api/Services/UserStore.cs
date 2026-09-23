using System.Text.Json;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Services;

public class UserStore
{
    private readonly List<UserAccount> _users;
    private readonly string _filePath;

    public UserStore(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "users.json");

        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException("User data file not found.", _filePath);
        }

        var json = File.ReadAllText(_filePath);
        _users = JsonSerializer.Deserialize<List<UserAccount>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<UserAccount>();
    }

    public IReadOnlyList<UserAccount> GetAll() => _users;

   public (UserAccount? User, string? Error) RegisterCustomer(RegisterRequest request)
{
    var email = request.Email.Trim();

    var existingUser = _users.FirstOrDefault(user =>
        string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));

    if (existingUser is null)
    {
        return (null, "EmailNotFound");
    }

    if (!string.Equals(existingUser.Role, "Customer", StringComparison.OrdinalIgnoreCase))
    {
        return (null, "Only customer accounts can register.");
    }

    if (string.Equals(existingUser.RegistrationStatus, "Registered",
        StringComparison.OrdinalIgnoreCase))
    {
        return (null, "EmailAlreadyRegistered");
    }

    var usernameExists = _users.Any(user =>
        string.Equals(user.Username, request.Username.Trim(),
            StringComparison.OrdinalIgnoreCase));

    if (usernameExists)
    {
        return (null, "UsernameAlreadyUsed");
    }

    existingUser.Username = request.Username.Trim();
    existingUser.Password = request.Password;
    existingUser.FullName = request.FullName.Trim();
    existingUser.RegistrationStatus = "Registered";

    SaveUsers();

    return (existingUser, null);
}

    public UserAccount? ValidateCredentials(string username, string password)
    {
        return _users.FirstOrDefault(user =>
            string.Equals(user.Username, username.Trim(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(user.Password, password, StringComparison.Ordinal) &&
            string.Equals(user.RegistrationStatus, "Registered", StringComparison.OrdinalIgnoreCase));
    }

    private void SaveUsers()
{
    var json = JsonSerializer.Serialize(
        _users,
        new JsonSerializerOptions
        {
            WriteIndented = true
        });

    File.WriteAllText(_filePath, json);
}
}
