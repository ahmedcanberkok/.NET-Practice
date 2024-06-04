
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using WebApplication1.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class EmployeeCrudController : ControllerBase
    {
        private readonly ConfigurationSettings _configurationSettings;

        public EmployeeCrudController(ConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
        }

        private int GetUserIdFromToken()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private string GetUsernameById(int id)
        {
            string _connectionString = _configurationSettings.ConnectionString;
            string username = string.Empty;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT Username FROM Users WHERE UserID = @UserID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", id);
                    connection.Open();
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        username = result.ToString();
                    }
                }
            }

            return username;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            string _connectionString = _configurationSettings.ConnectionString;
            string query = "GetEmployee";
            List<Employee> employees = new();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@EmployeeID", id);
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var createdByUserId = reader.IsDBNull(reader.GetOrdinal("CreatedByUserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CreatedByUserId"));
                            var createdByUsername = GetUsernameById(createdByUserId);

                            var employee = new Employee
                            {
                                EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                BirthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate")),
                                JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
                                CreatedByUsername = createdByUsername
                            };
                            employees.Add(employee);
                        }
                    }
                }
            }

            if (employees.Count == 0)
            {
                return NotFound();
            }

            return Ok(employees);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] Employee employee)
        {
            string _connectionString = _configurationSettings.ConnectionString;
            string query = "AddEmployee";
            int createdByUserId = GetUserIdFromToken(); // Token'dan kullanıcı ID'sini alıyoruz

            if (employee.BirthDate < new DateTime(1753, 1, 1) || employee.BirthDate > new DateTime(9999, 12, 31))
            {
                return BadRequest("Error: BirthDate must be between 1/1/1753 and 12/31/9999.");
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@BirthDate", employee.BirthDate);
                    command.Parameters.AddWithValue("@JobTitle", employee.JobTitle);
                    command.Parameters.AddWithValue("@CreatedByUserId", createdByUserId); // Token'dan alınan kullanıcı ID'sini ekliyoruz
                    try
                    {
                        connection.Open();
                        await command.ExecuteNonQueryAsync();
                        connection.Close();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error: " + ex.Message);
                    }
                }
            }
        }





        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee([FromBody] Employee employee)
        {
            string _connectionString = _configurationSettings.ConnectionString;
            string query = "UpdateEmployee";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@BirthDate", employee.BirthDate);
                    command.Parameters.AddWithValue("@JobTitle", employee.JobTitle);
                    try
                    {
                        connection.Open();
                        await command.ExecuteNonQueryAsync();
                        connection.Close();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error: " + ex.Message);
                    }
                }
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            string _connectionString = _configurationSettings.ConnectionString;
            string query = "DeleteEmployee";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@EmployeeID", id);
                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    return Ok();
                }
            }
        }
    }
}
