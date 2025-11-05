namespace Application.DTOs.SuperAdmin
{
    public class UpdateSuperAdminDTO
    {
        public string Name { get; set; }

        public DateOnly DateOfBirth { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}
