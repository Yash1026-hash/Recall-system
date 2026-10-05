namespace VehicleRecall.Shared.Models;

/// <summary>Payload for activating or deactivating a user account.</summary>
public sealed record UpdateUserStatusRequest(bool IsActive);
