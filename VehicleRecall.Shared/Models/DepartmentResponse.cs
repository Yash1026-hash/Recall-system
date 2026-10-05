namespace VehicleRecall.Shared.Models;

/// <summary>Department returned by the management API.</summary>
public sealed record DepartmentResponse(int DepartmentId, string Name, string Description, int UserCount);
