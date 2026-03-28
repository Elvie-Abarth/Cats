using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace RESTcats.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        // Any user with a valid token can access this (both "Admin" and "User")
        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            return Ok("Here is the public data for logged-in users.");
        }

        // Only users with the "Admin" role in their token can access this
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            return Ok($"Item {id} was successfully deleted by an Admin.");
        }

        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        //[HttpPost("login")]
        //public IActionResult Login([FromBody] LoginRequest login)
        //{
        //    // 1. Validate the user (In a real scenario, check your database here)
        //    // Here we use a simple hardcoded check:
        //    if (login.Username == "admin" && login.Password == "1234")
        //    {
        //        var token = GenerateJwtToken(login.Username);
        //        return Ok(new { token });
        //    }

        //    return Unauthorized("Invalid username or password.");
        //}

        // REST Exercise 6: part:1 - Add role-based authentication
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            // 1. Validate the user and assign a role
            string role = "";

            if (login.Username == "admin" && login.Password == "1234")
            {
                role = "Admin";
            }
            else if (login.Username == "user" && login.Password == "1234")
            {
                role = "User";
            }
            else
            {
                return Unauthorized("Invalid username or password.");
            }

            // Pass the role to the generator
            var token = GenerateJwtToken(login.Username, role);
            return Ok(new { token });
        }


        //    private string GenerateJwtToken(string username)
        //    {
        //        var jwtSettings = _config.GetSection("Jwt");
        //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //        // Claims are the pieces of information "baked" into the token
        //        var claims = new[]
        //        {
        //            new Claim(JwtRegisteredClaimNames.Sub, username),
        //            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //            new Claim(ClaimTypes.Name, username),
        //            new Claim(ClaimTypes.Role, "Admin") // You can add roles here
        //        };

        //        var token = new JwtSecurityToken(
        //            issuer: jwtSettings["Issuer"],
        //            audience: jwtSettings["Audience"],
        //            claims: claims,
        //            expires: DateTime.Now.AddHours(2), // Token is valid for 2 hours
        //            signingCredentials: creds
        //        );

        //        return new JwtSecurityTokenHandler().WriteToken(token);
        //    }
        //}

        //REST Exercise 6: part: 1 - Modify the token generator to accept a role parameter and include it in the claims. This way, the token will carry information about the user's role, which can be used for authorization in protected endpoints.
        private string GenerateJwtToken(string username, string role) // Added role parameter
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims are the pieces of information "baked" into the token
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role) // The token now bakes the dynamic role into the passport!
    };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // Helper class to receive JSON data
        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
