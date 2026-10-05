namespace VehicleRecall.Shared.Models;

public class Department
{
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<UserAccount> Users { get; set; } = new List<UserAccount>();
}
