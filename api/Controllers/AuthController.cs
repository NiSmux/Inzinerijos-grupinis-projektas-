using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MyBackend.Models; 
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations; 
using Microsoft.AspNetCore.Authorization; 

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // api/Auth
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        // Register a new user
        [HttpPost("register")] // api/Auth/register
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with email: {Email}", model.Email);

                // Jūsų įvesta slaptažodžio patikra
                if (model.Password.Length < 8 || !model.Password.Any(char.IsDigit) || !model.Password.Any(char.IsUpper) || !model.Password.Any(char.IsLower))
                {
                    return BadRequest(new { Message = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, and one number." });
                }

                var user = new User
                {
                    UserName = model.Email, // Ar UserName turi būti tas pats kaip Email? Patikrinkite savo Identity konfigūraciją
                    Email = model.Email,
                    Name = model.Name
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User registered successfully: {Email}", model.Email);
                    return Ok(new { Message = "User registered successfully" });
                }

                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("Registration failed for email {Email} with errors: {Errors}", model.Email, string.Join(", ", errors));
                return BadRequest(new { Message = "Registration failed", Errors = errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email {Email}", model.Email);
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        // Login and generate JWT token
        [HttpPost("login")] // api/Auth/login
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            _logger.LogInformation("Attempting login for user: {Email}", model.Email);

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                _logger.LogWarning("Login attempt failed: Email or password missing");
                return BadRequest(new { Message = "Email and password are required" });
            }

            // Bandyti prisijungti per Email. Jei UserName yra Email, tai veiks.
            // Jei UserName ir Email skiriasi, gali reiketi FindByEmailAsync + CheckPasswordAsync
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                 _logger.LogWarning("Login attempt failed for {Email}: User not found", model.Email);
                 return Unauthorized(new { Message = "Invalid credentials" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);


            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);
                _logger.LogInformation("User logged in successfully: {Email}", model.Email);
                return Ok(new { Token = token });
            }

            _logger.LogWarning("Login attempt failed for {Email}: Invalid password", model.Email);
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        [Authorize] // Šis endpoint'as pasiekiamas tik prisijungusiems vartotojams
        [HttpPost("change-email")] // api/Auth/change-email
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Geriau gauti ID tiesiai is claim'u
            _logger.LogInformation("Attempting to change email for user ID: {UserId} to {NewEmail}", userId, model.NewEmail);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Change email request model validation failed for user ID: {UserId}", userId);
                return BadRequest(ModelState);
            }

            // Rasti prisijungusį vartotoją
            var user = await _userManager.FindByIdAsync(userId); // Rasti user pagal ID is claim'u

            if (user == null)
            {
                 _logger.LogError("Authorized user not found in UserManager during email change attempt for ID: {UserId}", userId);
                return Unauthorized(new { message = "User not found" });
            }

             // Atliekame slaptažodžio patikrinimą PRIEŠ el. pašto keitimą
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordCheck)
            {
                 _logger.LogWarning("Invalid current password provided for email change for user ID: {UserId}", userId);
                return BadRequest(new { message = "Invalid current password" });
            }

            // Generuojame tokeną ir keičiame el. paštą
            var setEmailToken = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
            var setEmailResult = await _userManager.ChangeEmailAsync(user, model.NewEmail, setEmailToken);


            if (!setEmailResult.Succeeded)
            {
                var errors = setEmailResult.Errors.Select(e => e.Description);
                _logger.LogError("SetEmailAsync failed for user ID: {UserId} with errors: {Errors}", userId, string.Join(", ", errors));
                return BadRequest(setEmailResult.Errors);
            }

            // Jei vartotojo vardas (UserName) yra tas pačias kaip el. paštas, atnaujinti ir jį
            // Tai priklauso nuo jūsų Identity konfigūracijos ir loginimo būdo
            // Dažnai Identity naudoja UserName kaip unikalų identifikatorių loginimui, o Email gali būti kitas.
            // Jei jūsų registracijos logika UserName nustatė kaip Email, galite norėti atnaujinti UserName
            // Svarbu: Jei loginatės per UserName, o ne Email, ši logika gali būti būtina arba nereikalinga.
            // Patikrinkite savo Identity konfigūraciją Startup.cs ar Program.cs
             if (user.UserName == user.Email) // Pavyzdys loginimui per Email
             {
                 var setUserNameResult = await _userManager.SetUserNameAsync(user, model.NewEmail);
                 if (!setUserNameResult.Succeeded)
                 {
                     // Tvarkyti klaidas, jei nepavyko atnaujinti vartotojo vardo
                     var errors = setUserNameResult.Errors.Select(e => e.Description);
                     _logger.LogError("SetUserNameAsync failed after email change for user ID: {UserId} with errors: {Errors}", userId, string.Join(", ", errors));
                     // Galite grąžinti klaida, arba tik log'inti ir leisti el. pasto keitimui pavykti, bet userName neatnaujinti
                     // return BadRequest(setUserNameResult.Errors); // Galite grąžinti ir šią klaidą
                 }
             }

             // Jei pasikeitė Username (nes buvo tas pats kaip Email) arba Email, atnaujinti vartotojo Claims ir SecurityStamp
             await _userManager.UpdateSecurityStampAsync(user);


            _logger.LogInformation("Email changed successfully for user ID: {UserId}", userId);
            return Ok(new { message = "Email changed successfully" });
        }

        [Authorize] // Šis endpoint'as pasiekiamas tik prisijungusiems vartotojams
        [HttpPost("change-password")] // api/Auth/change-password
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Geriau gauti ID tiesiai is claim'u
            _logger.LogInformation("Attempting to change password for user ID: {UserId}", userId);

            if (!ModelState.IsValid)
            {
                 _logger.LogWarning("Change password request model validation failed for user ID: {UserId}", userId);
                return BadRequest(ModelState);
            }

            // Patikrinti, ar naujas slaptažodis ir patvirtinimas sutampa
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                 _logger.LogWarning("New password and confirmation do not match for user ID: {UserId}", userId);
                return BadRequest(new { message = "New password and confirmation do not match" });
            }

            // Rasti prisijungusį vartotoją
            var user = await _userManager.FindByIdAsync(userId); // Rasti user pagal ID is claim'u


            if (user == null)
            {
                 _logger.LogError("Authorized user not found in UserManager during password change attempt for ID: {UserId}", userId);
                return Unauthorized(new { message = "User not found" });
            }

            // Pakeisti slaptažodį
            // ChangePasswordAsync reikalauja DABARTINIO slaptažodžio
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                 var errors = changePasswordResult.Errors.Select(e => e.Description);
                 _logger.LogError("ChangePasswordAsync failed for user ID: {UserId} with errors: {Errors}", userId, string.Join(", ", errors));
                // Grąžinti klaidas, jei ChangePasswordAsync nepavyko (pvz., neteisingas dabartinis slaptažodis, naujas slaptažodis neatitinka politikos)
                return BadRequest(changePasswordResult.Errors);
            }

             // Atnaujinti SecurityStamp po slaptažodžio pakeitimo, kad anuliuoti senus tokenus, jei reikia
             await _userManager.UpdateSecurityStampAsync(user);


            _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);
            return Ok(new { message = "Password changed successfully" });
        }


        // Generate JWT token
        private string GenerateJwtToken(User user)
        {
            // Patikrinti, ar JWT Key yra nustatytas konfiguracijoje, jei ne, naudoti numatyta
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogWarning("JWT:Key is not configured. Using default key. ENSURE THIS IS SET IN PRODUCTION!");
                 jwtKey = "SuperSecretKeySuperSecretKeySuperSecretKeySuperSecretKey"; // Naudokite ilgą, atsitiktinį numatytąjį raktą
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), // Unikalus vartotojo ID (string)
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName), // Subjektas (dažnai Username)
                new Claim(JwtRegisteredClaimNames.Email, user.Email), // El. paštas
                // Pridedame Name claim'ą tik jei jis yra
                // user.Name prop greičiausiai neateina su default IdentityUser,
                // bet jei jis pridėtas jūsų User modelyje, kaip matosi
                // is Register metodo, galite ji prideti
                 new Claim("name", user.Name ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unikalus tokeno ID
            };

            // Pridėti rolės claim'ą, jei vartotojas yra administratorius
            if (user.IsAdmin) // Patikrinkite, ar User modelis turi IsAdmin savybę
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
             // Jei naudojate ASP.NET Core Identity Roles, geriau gauti roles is userManager
             // var roles = await _userManager.GetRolesAsync(user);
             // foreach(var role in roles)
             // {
             //     claims.Add(new Claim(ClaimTypes.Role, role));
             // }


            // Nustatykite Issuer ir Audience is konfigūracijos
             var issuer = _configuration["Jwt:Issuer"] ?? "your_issuer"; // Numatytasis, jei nėra konfigūracijoje
             var audience = _configuration["Jwt:Audience"] ?? "your_audience"; // Numatytasis, jei nėra konfigūracijoje

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Tokeno galiojimo laikas (1 valanda)
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // DTO - Data Transfer Objects
    // Modeliai, naudojami duomenims priimti is frontend

    // Register model (naudojamas registracijai)
    public class RegisterModel
    {
        [Required] // Reikalingas, kad patikrinti, jog laukas nera tuscias
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress] // Patikrina el. pašto formato validumą
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)] // Nurodo, kad tai slaptažodis (skirta UI, nevaliduoja stiprumo)
        public string Password { get; set; }
    }

    // Login model (naudojamas prisijungimui)
    public class LoginModel
    {
        [Required]
        [EmailAddress] // Ar loginatės per Email, ar per UserName? Login metodas naudoja Email.
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    // Modelis keisti el. paštą
    public class ChangeEmailRequest
    {
        [Required(ErrorMessage = "Naujas el. paštas yra privalomas")] // Galite pridėti lietuviškus pranešimus
        [EmailAddress(ErrorMessage = "Neteisingas el. pašto formatas")]
        public string NewEmail { get; set; }

        [Required(ErrorMessage = "Dabartinis slaptažodis yra privalomas")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
    }

    // Modelis keisti slaptažodį
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Dabartinis slaptažodis yra privalomas")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Naujas slaptažodis yra privalomas")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Naujas slaptažodis turi būti bent 8 simbolių ilgio")] // Galite pridėti papildomas validacijas
        // Identity slaptažodžio politika konfigūruojama atskirai Identity serveces setup metu
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Pakartokite naują slaptažodį")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Naujas slaptažodis ir jo patvirtinimas nesutampa")]
        public string ConfirmNewPassword { get; set; }
    }
}
