using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[action]")]
    public class GetTableController : ControllerBase
    {
        private readonly ConfigurationSettings _configurationSettings;


        public GetTableController(ConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
        }

        // Endpoint for getting employees/Çalýþanlarý getirmek için bir endpoint

        [HttpGet(Name = "getTable")]
        public ActionResult<IEnumerable<Employee>> GetTable()
        {
            var employees = new List<Employee>();

            // Veritabaný baðlantý dizesi
            string connectionString = _configurationSettings.ConnectionString;

            // Veritabaný baðlantýsý ve sorgu
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("GetEmployeeDetails", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var employee = new Employee
                            {
                                EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                BirthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate")),
                                JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
                                CreatedByUserId = reader.IsDBNull(reader.GetOrdinal("CreatedByUserId"))
                                                  ? (int?)null
                                                  : reader.GetInt32(reader.GetOrdinal("CreatedByUserId")),
                                CreatedByUsername = reader.IsDBNull(reader.GetOrdinal("CreatedByUsername"))
                                                    ? string.Empty
                                                    : reader.GetString(reader.GetOrdinal("CreatedByUsername"))
                            };
                            employees.Add(employee);
                        }
                    }
                }
            }

            return Ok(employees);
        }


    }
}
