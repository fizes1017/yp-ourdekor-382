using MetalStructuresAPI.Data;
using MetalStructuresAPI.DTOs;
using MetalStructuresAPI.Models;
using MetalStructuresAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MetalStructuresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;

    public AuthController(ApplicationDbContext context, PasswordService passwordService, JwtService jwtService)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(registerDto.Email) ||
            string.IsNullOrWhiteSpace(registerDto.Phone) ||
            string.IsNullOrWhiteSpace(registerDto.Password) ||
            string.IsNullOrWhiteSpace(registerDto.FullName))
        {
            return BadRequest(new { message = "Все поля обязательны для заполнения" });
        }

        // Validate password confirmation
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return BadRequest(new { message = "Пароли не совпадают" });
        }

        // Validate password length
        if (registerDto.Password.Length < 6)
        {
            return BadRequest(new { message = "Пароль должен содержать минимум 6 символов" });
        }

        // Validate email format (basic)
        if (!registerDto.Email.Contains('@') || !registerDto.Email.Contains('.'))
        {
            return BadRequest(new { message = "Некорректный формат email" });
        }

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            return BadRequest(new { message = "Пользователь с таким email уже существует" });
        }

        // Check if phone already exists
        if (await _context.Users.AnyAsync(u => u.Phone == registerDto.Phone))
        {
            return BadRequest(new { message = "Пользователь с таким телефоном уже существует" });
        }

        try
        {
            // Create new user
            var user = new User
            {
                Email = registerDto.Email,
                Phone = registerDto.Phone,
                PasswordHash = _passwordService.HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                Role = "Manager",
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);

            var response = new AuthResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.Phone,
                    FullName = user.FullName,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(response);
        }
        catch (DbUpdateException ex)
        {
            // Handle database constraint violations
            Console.WriteLine($"Database error during registration: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            
            // Check if it's a unique constraint violation
            if (ex.InnerException?.Message?.Contains("duplicate key") == true || 
                ex.InnerException?.Message?.Contains("UNIQUE constraint") == true)
            {
                return BadRequest(new { message = "Пользователь с таким email или телефоном уже существует" });
            }
            
            return StatusCode(500, new { message = "Ошибка при регистрации: " + ex.Message });
        }
        /*catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error during registration: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { message = "Ошибка при регистрации: " + ex.Message });
        }*/
        catch (Exception ex)
        {
            // Это выведет полную ошибку прямо в браузер
            var fullError = ex.Message;
            if (ex.InnerException != null)
            {
                fullError += " | INNER: " + ex.InnerException.Message;
            }
            return StatusCode(500, new { message = fullError, stack = ex.StackTrace });
        }
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return BadRequest(new { message = "Email и пароль обязательны" });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Неверный email или пароль" });
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(user.Id, user.Email);

        var response = new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Phone = user.Phone,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };

        return Ok(response);
    }


}

