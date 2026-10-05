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
        int pageSize = 10,
        int? departmentId = null,
        bool? isActive = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _context.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .Include(user => user.Department)
            .Include(user => user.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleFilter = role.Trim();
            query = query.Where(user => user.UserRoles.Any(userRole => userRole.Role.Name == roleFilter));
        }

        if (!string.IsNullOrWhiteSpace(registrationStatus))
        {
            var statusFilter = registrationStatus.Trim();
            query = query.Where(user => user.RegistrationStatus == statusFilter);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(user => user.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(user => user.IsActive == isActive.Value);
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
            .Include(user => user.Department)
            .Include(user => user.Customer)
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

        var departmentName = !string.IsNullOrWhiteSpace(request.Department)
            ? request.Department.Trim()
            : request.Role == "Manager" ? "Operations Management" : "Field Engineering & Service";
        var department = request.DepartmentId > 0
            ? _context.Departments.SingleOrDefault(existingDepartment => existingDepartment.DepartmentId == request.DepartmentId)
            : _context.Departments.SingleOrDefault(existingDepartment => existingDepartment.Name == departmentName);
        if (department is null)
        {
            throw new InvalidOperationException("DepartmentNotFound");
        }

        var user = new UserAccount
        {
            Username = username,
            FullName = request.FullName.Trim(),
            Email = email,
            Role = request.Role,
            DepartmentId = department.DepartmentId,
            Department = department,
            RegistrationStatus = "Registered",
            CreatedAt = DateTime.UtcNow,
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
            .Include(existingUser => existingUser.Department)
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

        user.UserRoles.Clear();
        user.UserRoles.Add(new UserRole { UserId = userId, Role = relatedRole });
        user.Role = relatedRole.Name;
        _context.SaveChanges();
        return user;
    }

    public UserAccount? UpdateUser(int userId, RegisterRequest request)
    {
        var user = _context.Users
            .Include(existingUser => existingUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .Include(existingUser => existingUser.Department)
            .FirstOrDefault(existingUser => existingUser.UserId == userId);

        if (user is null)
        {
            return null;
        }

        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

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

    public UserAccount? UpdateUserProfile(int userId, UpdateUserRequest request)
    {
        var user = _context.Users
            .Include(existingUser => existingUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .Include(existingUser => existingUser.Department)
            .FirstOrDefault(existingUser => existingUser.UserId == userId);

        if (user is null)
        {
            return null;
        }

        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

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
        var department = _context.Departments.SingleOrDefault(
            existingDepartment => existingDepartment.DepartmentId == request.DepartmentId);
        if (department is null)
        {
            throw new InvalidOperationException("DepartmentNotFound");
        }

        user.DepartmentId = department.DepartmentId;
        user.Department = department;

        _context.SaveChanges();
        return user;
    }

    public IReadOnlyList<Role> GetRoles() =>
        _context.Roles.AsNoTracking().OrderBy(role => role.Name).ToList();

    public UserAccount? UpdateUserRoles(int userId, IReadOnlyList<string> roleNames)
    {
        if (roleNames is null || roleNames.Count == 0 || roleNames.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("AtLeastOneRoleRequired");
        }

        var distinctNames = roleNames
            .Select(roleName => roleName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var availableRoles = _context.Roles
            .Where(role => distinctNames.Contains(role.Name))
            .ToList();
        if (availableRoles.Count != distinctNames.Count)
        {
            throw new InvalidOperationException("RoleNotFound");
        }
        var roles = distinctNames
            .Select(name => availableRoles.Single(role =>
                string.Equals(role.Name, name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var user = _context.Users
            .Include(existingUser => existingUser.UserRoles)
            .Include(existingUser => existingUser.Department)
            .FirstOrDefault(existingUser => existingUser.UserId == userId);
        if (user is null)
        {
            return null;
        }

        user.UserRoles.Clear();
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { UserId = userId, Role = role });
        }

        user.Role = roles[0].Name;
        _context.SaveChanges();
        return user;
    }

    public UserAccount? UpdateUserStatus(int userId, bool isActive)
    {
        var user = GetById(userId);
        if (user is null)
        {
            return null;
        }

        user.IsActive = isActive;
        _context.SaveChanges();
        return user;
    }

    public IReadOnlyList<DepartmentResponse> GetDepartmentSummaries() =>
        _context.Departments
            .AsNoTracking()
            .OrderBy(department => department.Name)
            .Select(department => new DepartmentResponse(
                department.DepartmentId,
                department.Name,
                department.Description,
                department.Users.Count))
            .ToList();

    public Department? GetDepartmentById(int departmentId) =>
        _context.Departments
            .Include(department => department.Users)
            .FirstOrDefault(department => department.DepartmentId == departmentId);

    public Department CreateDepartment(CreateDepartmentRequest request)
    {
        var name = request.Name.Trim();
        if (_context.Departments.Any(department => department.Name == name))
        {
            throw new InvalidOperationException("DepartmentAlreadyExists");
        }

        var department = new Department
        {
            Name = name,
            Description = (request.Description ?? string.Empty).Trim()
        };
        _context.Departments.Add(department);
        _context.SaveChanges();
        return department;
    }

    public Department? UpdateDepartment(int departmentId, UpdateDepartmentRequest request)
    {
        var department = _context.Departments.Find(departmentId);
        if (department is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        if (_context.Departments.Any(existingDepartment =>
                existingDepartment.DepartmentId != departmentId &&
                existingDepartment.Name == name))
        {
            throw new InvalidOperationException("DepartmentAlreadyExists");
        }

        department.Name = name;
        department.Description = (request.Description ?? string.Empty).Trim();
        _context.SaveChanges();
        return department;
    }

    public bool DeleteDepartment(int departmentId)
    {
        var department = _context.Departments.Find(departmentId);
        if (department is null)
        {
            return false;
        }

        if (_context.Users.Any(user => user.DepartmentId == departmentId))
        {
            throw new InvalidOperationException("DepartmentInUse");
        }

        _context.Departments.Remove(department);
        _context.SaveChanges();
        return true;
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

        var department = _context.Departments.SingleOrDefault(
            existingDepartment => existingDepartment.Name == "Customer Support & Warranty");
        if (department is null)
        {
            throw new InvalidOperationException("DepartmentNotFound");
        }

        var userAccount = new UserAccount
        {
            Username = username,
            Password = string.Empty,
            Role = "Customer",
            FullName = customer.FullName,
            Email = customer.Email,
            DepartmentId = department.DepartmentId,
            Department = department,
            RegistrationStatus = "Registered",
            CreatedAt = DateTime.UtcNow,
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
            .Include(account => account.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .Include(account => account.Department)
            .FirstOrDefault(account => account.Username.ToUpper() == normalizedUsername);

        if (user is null ||
            !user.IsActive ||
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