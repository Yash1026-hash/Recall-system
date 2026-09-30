namespace VehicleRecallApp.Models;

public class RecallCampaign
{
    public string NhtsaId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RemedyInstructions { get; set; } = string.Empty;
    public string AffectedComponent { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int AffectedVins { get; set; }
    public List<CampaignUserStatus> Users { get; set; } = new();
    public int ImportedRecords { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public List<CampaignImportError> ImportErrors { get; set; } = new();

    public int BookedAppointments => Users.Count(user => user.AppointmentBooked);
    public int RepairedVehicles => Users.Count(user => user.RepairCompleted);
}

public class CampaignUserStatus
{
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public bool AppointmentBooked { get; set; }
    public bool RepairCompleted { get; set; }

    public string AppointmentStatus => AppointmentBooked ? "Booked" : "Not booked";
    public string RepairStatus => RepairCompleted ? "Repaired" : "Not repaired";
}

public class CampaignImportError
{
    public int RowNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}