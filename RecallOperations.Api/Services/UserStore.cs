using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecallOperations.Api.Data;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Services;

public class UserStore
{
    private const string PasswordHashPrefix = "hash$v1$";
    private readonly UserDbContext _context;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public UserStore(UserDbContext context, IPasswordHasher<UserAccount> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public IReadOnlyList<UserAccount> SearchUsers(
        string? role,
        string? registrationStatus,
        string? search = null,
        string? vin = null,
        string? vehicleStatus = null,
        string? campaign = null,
        string? sort = null,
        int page = 1,
        int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _context.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleFilter = role.Trim();
            query = query.Where(user => user.Role == roleFilter);
        }

        if (!string.IsNullOrWhiteSpace(registrationStatus))
        {
            var statusFilter = registrationStatus.Trim();
            query = query.Where(user => user.RegistrationStatus == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(user =>
                user.Username.Contains(searchTerm) ||
                user.FullName.Contains(searchTerm) ||
                user.Email.Contains(searchTerm) ||
                user.Role.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(vin))
        {
            var vinFilter = vin.Trim();

            var matchingCustomerIds = _context.CustomerVehicles
                .AsNoTracking()
                .Where(link => link.Vin.Contains(vinFilter))
                .Select(link => link.CustomerId)
                .Distinct();

            query = query.Where(user =>
                user.CustomerId.HasValue &&
                matchingCustomerIds.Contains(user.CustomerId.Value));
        }

        if (!string.IsNullOrWhiteSpace(vehicleStatus))
        {
            var statusFilter = vehicleStatus.Trim();

            var matchingCustomerIds = _context.CustomerVehicles
                .AsNoTracking()
                .Where(link => link.Vehicle != null && link.Vehicle.RecallStatus == statusFilter)
                .Select(link => link.CustomerId)
                .Distinct();

            query = query.Where(user =>
                user.CustomerId.HasValue &&
                matchingCustomerIds.Contains(user.CustomerId.Value));
        }

        if (!string.IsNullOrWhiteSpace(campaign))
        {
            var campaignFilter = campaign.Trim();

            query = query.Where(user =>
                user.Username.Contains(campaignFilter) ||
                user.FullName.Contains(campaignFilter) ||
                user.Email.Contains(campaignFilter));
        }

        query = (sort ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "vin" => query
                .Select(user => new
                {
                    User = user,
                    Vin = _context.CustomerVehicles
                        .Where(link => link.CustomerId == user.CustomerId)
                        .Select(link => link.Vin)
                        .FirstOrDefault() ?? string.Empty
                })
                .OrderBy(x => x.Vin)
                .Select(x => x.User),

            "vehiclestatus" or "status" => query
                .Select(user => new
                {
                    User = user,
                    VehicleStatus = _context.CustomerVehicles
                        .Where(link => link.CustomerId == user.CustomerId)
                        .Select(link => link.Vehicle != null ? link.Vehicle.RecallStatus : string.Empty)
                        .FirstOrDefault() ?? string.Empty
                })
                .OrderBy(x => x.VehicleStatus)
                .Select(x => x.User),

            "registrationstatus" => query.OrderBy(user => user.RegistrationStatus),

            "username" => query.OrderBy(user => user.Username),

            _ => query.OrderBy(user => user.UserId)
        };

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public UserAccount? GetById(int userId) =>
        _context.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .FirstOrDefault(user => user.UserId == userId);

    public UserAccount CreateStaffUser(StaffUserRequest request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (_context.Users.Any(user => user.Username == username))
        {
            throw new InvalidOperationException("UsernameAlreadyUsed");
        }

        if (_context.Users.Any(user => user.Email == email))
        {
            throw new InvalidOperationException("EmailAlreadyRegistered");
        }

        var role = _context.Roles.SingleOrDefault(existingRole => existingRole.Name == request.Role)
            ?? throw new InvalidOperationException("RoleNotFound");

        var user = new UserAccount
        {
            Username = username,
            FullName = request.FullName.Trim(),
            Email = email,
            Role = request.Role,
            RegistrationStatus = "Registered",
            CustomerId = null,
            UserRoles = new List<UserRole> { new() { Role = role } }
        };
        user.Password = HashPassword(user, request.Password);

        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    public UserAccount? UpdateUserRole(int userId, string role)
    {
        var user = _context.Users
            .Include(existingUser => existingUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .FirstOrDefault(existingUser => existingUser.UserId == userId);
        if (user is null)
        {
            return null;
        }

        var relatedRole = _context.Roles.SingleOrDefault(existingRole => existingRole.Name == role);
        if (relatedRole is null)
        {
            throw new InvalidOperationException("RoleNotFound");
        }

        user.Role = role;
        foreach (var existingRole in user.UserRoles.Where(userRole => userRole.RoleId != relatedRole.RoleId).ToList())
        {
            user.UserRoles.Remove(existingRole);
        }

        if (user.UserRoles.All(userRole => userRole.RoleId != relatedRole.RoleId))
        {
            user.UserRoles.Add(new UserRole { UserId = userId, Role = relatedRole });
        }

        _context.SaveChanges();
        return user;
    }

    public UserAccount? UpdateUser(int userId, RegisterRequest request)
    {
        var user = _context.Users
            .Include(existingUser => existingUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .FirstOrDefault(existingUser => existingUser.UserId == userId);

        if (user is null)
        {
            return null;
        }

        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim();

        if (_context.Users.Any(existingUser =>
                existingUser.UserId != userId &&
                existingUser.Username == normalizedUsername))
        {
            throw new InvalidOperationException("UsernameAlreadyUsed");
        }

        if (_context.Users.Any(existingUser =>
                existingUser.UserId != userId &&
                existingUser.Email == normalizedEmail))
        {
            throw new InvalidOperationException("EmailAlreadyRegistered");
        }

        user.Username = normalizedUsername;
        user.Email = normalizedEmail;
        user.FullName = request.FullName.Trim();

        _context.SaveChanges();
        return user;
    }

    public (int Added, int Updated) ImportCustomerRoster(
        IReadOnlyCollection<CustomerRosterEntryRequest> entries)
    {
        var normalizedEntries = entries
            .Select(entry => new CustomerRosterEntryRequest
            {
                FullName = entry.FullName.Trim(),
                Email = entry.Email.Trim().ToLowerInvariant(),
                Address = NormalizeOptional(entry.Address),
                City = NormalizeOptional(entry.City),
                State = NormalizeOptional(entry.State),
                PostalCode = NormalizeOptional(entry.PostalCode)
            })
            .GroupBy(entry => entry.Email, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .ToList();

        var emails = normalizedEntries.Select(entry => entry.Email).ToList();
        var existingCustomers = _context.Customers
            .Where(customer => emails.Contains(customer.Email))
            .ToList();
        var existingByEmail = existingCustomers.ToDictionary(
            customer => customer.Email,
            StringComparer.OrdinalIgnoreCase);

        var added = 0;
        var updated = 0;
        foreach (var entry in normalizedEntries)
        {
            if (existingByEmail.TryGetValue(entry.Email, out var customer))
            {
                customer.FullName = entry.FullName;
                customer.Address = entry.Address ?? customer.Address;
                customer.City = entry.City ?? customer.City;
                customer.State = entry.State ?? customer.State;
                customer.PostalCode = entry.PostalCode ?? customer.PostalCode;
                updated++;
                continue;
            }

            customer = new RecallCustomer
            {
                FullName = entry.FullName,
                Email = entry.Email,
                Address = entry.Address,
                City = entry.City,
                State = entry.State,
                PostalCode = entry.PostalCode
            };
            _context.Customers.Add(customer);
            existingByEmail.Add(entry.Email, customer);
            added++;
        }

        _context.SaveChanges();
        return (added, updated);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public bool DeleteUser(int userId)
    {
        var user = _context.Users.FirstOrDefault(existingUser => existingUser.UserId == userId);

        if (user is null)
        {
            return false;
        }

        _context.Users.Remove(user);
        _context.SaveChanges();
        return true;
    }

    public (UserAccount? User, string? Error) RegisterCustomer(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var username = request.Username.Trim();
        var normalizedUsername = username.ToUpperInvariant();

        var customer = _context.Customers.SingleOrDefault(record => record.Email == email);

        if (customer is null)
        {
            return (null, "EmailNotFound");
        }

        if (_context.Users.Any(user => user.CustomerId == customer.CustomerId))
        {
            return (null, "EmailAlreadyRegistered");
        }

        if (_context.Users.Any(user => user.Username.ToUpper() == normalizedUsername))
        {
            return (null, "UsernameAlreadyUsed");
        }

        var customerRole = _context.Roles.SingleOrDefault(role => role.Name == "Customer");
        if (customerRole is null)
        {
            throw new InvalidOperationException("RoleNotFound");
        }

        var userAccount = new UserAccount
        {
            Username = username,
            Password = string.Empty,
            Role = "Customer",
            FullName = customer.FullName,
            Email = customer.Email,
            RegistrationStatus = "Registered",
            CustomerId = customer.CustomerId,
            UserRoles = new List<UserRole> { new() { Role = customerRole } }
        };

        userAccount.Password = HashPassword(userAccount, request.Password);
        _context.Users.Add(userAccount);
        _context.SaveChanges();

        return (userAccount, null);
    }

    public UserAccount? ValidateCredentials(string username, string password)
    {
        var normalizedUsername = username.Trim().ToUpperInvariant();
        var user = _context.Users
            .FirstOrDefault(account => account.Username.ToUpper() == normalizedUsername);

        if (user is null ||
            !string.Equals(user.RegistrationStatus, "Registered", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (user.Password.StartsWith(PasswordHashPrefix, StringComparison.Ordinal))
        {
            var verification = _passwordHasher.VerifyHashedPassword(
                user,
                user.Password[PasswordHashPrefix.Length..],
                password);

            if (verification == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Password = HashPassword(user, password);
                _context.SaveChanges();
            }

            return user;
        }

        if (!string.Equals(user.Password, password, StringComparison.Ordinal))
        {
            return null;
        }

        user.Password = HashPassword(user, password);
        _context.SaveChanges();
        return user;
    }

    private string HashPassword(UserAccount user, string password) =>
        PasswordHashPrefix + _passwordHasher.HashPassword(user, password);
}