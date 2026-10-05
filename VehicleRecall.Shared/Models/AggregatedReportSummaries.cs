namespace VehicleRecall.Shared.Models;

public class VehicleRecallStatusSummary
{
    public string RecallStatus { get; set; } = string.Empty;
    public int TotalVehicles { get; set; }
}

public class CustomerCityStateSummary
{
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int TotalCustomers { get; set; }
}

public class UserRoleSummary
{
    public string Role { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
}

public class UserRegistrationSummary
{
    public string RegistrationStatus { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
}

public class AggregatedReportResponse
{
    public List<VehicleRecallStatusSummary> VehiclesByStatus { get; set; } = new();
    public List<CustomerCityStateSummary> CustomersByCityState { get; set; } = new();
    public List<UserRoleSummary> UsersByRole { get; set; } = new();
    public List<UserRegistrationSummary> UsersByRegistrationStatus { get; set; } = new();
}
