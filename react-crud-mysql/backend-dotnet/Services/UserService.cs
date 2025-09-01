using Microsoft.EntityFrameworkCore;
using BackendDotnet.Data;
using BackendDotnet.Models;

namespace BackendDotnet.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users
                    .OrderByDescending(u => u.Id)
                    .ToListAsync();

                return users.Select(MapToResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users");
                throw;
            }
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                return user != null ? MapToResponseDto(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
                throw;
            }
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Check if email already exists
                if (await EmailExistsAsync(createUserDto.Email))
                {
                    throw new InvalidOperationException("Email already exists");
                }

                var user = new User
                {
                    Name = createUserDto.Name.Trim(),
                    Email = createUserDto.Email.Trim().ToLower(),
                    Age = createUserDto.Age
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new user with ID: {UserId}", user.Id);
                return MapToResponseDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return null;
                }

                // Check if any field is provided for update
                if (string.IsNullOrEmpty(updateUserDto.Name) && 
                    string.IsNullOrEmpty(updateUserDto.Email) && 
                    !updateUserDto.Age.HasValue)
                {
                    throw new InvalidOperationException("No fields to update");
                }

                // Check email uniqueness if email is being updated
                if (!string.IsNullOrEmpty(updateUserDto.Email) && 
                    await EmailExistsAsync(updateUserDto.Email, id))
                {
                    throw new InvalidOperationException("Email already exists");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateUserDto.Name))
                {
                    user.Name = updateUserDto.Name.Trim();
                }

                if (!string.IsNullOrEmpty(updateUserDto.Email))
                {
                    user.Email = updateUserDto.Email.Trim().ToLower();
                }

                if (updateUserDto.Age.HasValue)
                {
                    user.Age = updateUserDto.Age.Value;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated user with ID: {UserId}", user.Id);
                return MapToResponseDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return false;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted user with ID: {UserId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                throw;
            }
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            var query = _context.Users.Where(u => u.Email.ToLower() == email.ToLower());
            
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }

        private static UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Age = user.Age,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}