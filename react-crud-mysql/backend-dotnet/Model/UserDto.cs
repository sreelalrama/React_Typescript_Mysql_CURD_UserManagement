using System.ComponentModel.DataAnnotations;

namespace BackendDotnet.Models
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required")]
        [Range(1, 150, ErrorMessage = "Age must be between 1 and 150")]
        public int Age { get; set; }
    }

    public class UpdateUserDto
    {
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
        public string? Email { get; set; }

        [Range(1, 150, ErrorMessage = "Age must be between 1 and 150")]
        public int? Age { get; set; }
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}