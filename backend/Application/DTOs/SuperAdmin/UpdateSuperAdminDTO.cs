namespace Application.DTOs.SuperAdmin
{
    public class UpdateSuperAdminDTO
    {
        public string Name { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }

        public string Address { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Phone { get; set; } = null!;
    }
}
