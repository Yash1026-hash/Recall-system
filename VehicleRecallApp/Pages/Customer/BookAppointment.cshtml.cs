using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRecallApp.Pages.Customer;

public class BookAppointmentModel : PageModel
{
    public List<ServiceCenter> ServiceCenters { get; private set; } = new();

    [BindProperty]
    public BookingInput Input { get; set; } = new();

    public bool BookingConfirmed { get; private set; }
    public string BookingReference { get; private set; } = string.Empty;

    public void OnGet()
    {
        LoadServiceCenters();
        Input.AppointmentDate = DateTime.Today.AddDays(1);
    }

    public IActionResult OnPost()
    {
        LoadServiceCenters();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Temporary mock confirmation.
        // Later, validate bay capacity and save an Appointment in the database.
        BookingReference = $"RC-{Random.Shared.Next(100000, 999999)}";
        BookingConfirmed = true;

        return Page();
    }

    private void LoadServiceCenters()
    {
        ServiceCenters = new List<ServiceCenter>
        {
            new ServiceCenter
            {
                Id = "SC-001",
                Name = "Metro Auto Service Center",
                City = "Bengaluru",
                Distance = "2.4 km away"
            },
            new ServiceCenter
            {
                Id = "SC-002",
                Name = "SafeDrive Motors",
                City = "Bengaluru",
                Distance = "5.8 km away"
            },
            new ServiceCenter
            {
                Id = "SC-003",
                Name = "Prime Vehicle Care",
                City = "Bengaluru",
                Distance = "8.1 km away"
            }
        };
    }

    public class BookingInput
    {
        [Required(ErrorMessage = "Select a service center.")]
        [Display(Name = "Service Center")]
        public string ServiceCenterId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select an appointment date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Preferred Date")]
        public DateTime? AppointmentDate { get; set; }

        [Required(ErrorMessage = "Select an available time slot.")]
        [Display(Name = "Time Slot")]
        public string TimeSlot { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your name.")]
        [Display(Name = "Owner Name")]
        public string OwnerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter an email address.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a phone number.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string Phone { get; set; } = string.Empty;
    }

    public class ServiceCenter
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
    }
}