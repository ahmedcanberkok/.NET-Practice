
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly ConfigurationSettings _configurationSettings;
        private readonly JwtSettings _jwtSettings;

        public AuthController(ConfigurationSettings configurationSettings, IOptions<JwtSettings> jwtSettings)
        {
            _configurationSettings = configurationSettings;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost]
        public IActionResult Register([FromBody] User user)
        {
            string connectionString = _configurationSettings.ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("RegisterUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                        return Ok("User registered successfully.");
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error: " + ex.Message);
                    }
                }
            }
        }
        [HttpPost]
        public IActionResult Login([FromBody] User user)
        {
            string connectionString = _configurationSettings.ConnectionString;
            bool isAuthenticated = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("AuthenticateUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user.UserID = Convert.ToInt64(reader["UserID"].ToString());
                                user.Role = Convert.ToInt32(reader["Role"].ToString());
                                isAuthenticated = true;
                            }
                        }
                        connection.Close();

                        if (isAuthenticated)
                        {
                            var token = GenerateJwtToken(user.Username, user.UserID, user.Role);
                            return Ok(new { Token = token });
                        }
                        else
                        {
                            return Unauthorized("Invalid username or password");
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error: " + ex.Message);
                    }
                }
            }
        }

        private string GenerateJwtToken(string username, long userId, int role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("username", username),
                    new Claim("userId", userId.ToString()),
                    new Claim("role", role.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private int GetUserIdFromDatabase(string username)
        {
            string connectionString = _configurationSettings.ConnectionString;
            int userId = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("GetUserId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", username);

                    try
                    {
                        connection.Open();
                        userId = (int)command.ExecuteScalar();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error fetching userId: " + ex.Message);
                    }
                }
            }

            return userId;
        }
    }
}

