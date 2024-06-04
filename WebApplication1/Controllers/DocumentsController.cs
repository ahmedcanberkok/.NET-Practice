    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using WebApplication1.Models;

    namespace WebApplication1.Controllers
    {
        [ApiController]
        [Route("[controller]")]
        [Authorize]
    public class DocumentsController : ControllerBase
        {
            private readonly ConfigurationSettings _configurationSettings;

            public DocumentsController(ConfigurationSettings configurationSettings)
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


        [HttpPost]
        public async Task<IActionResult> AddDocument([FromForm] Document model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int userId = GetUserIdFromToken(); // Token'dan kullanıcı ID'sini alıyoruz

            if (userId == 0)
            {
                return BadRequest("User ID could not be retrieved from the token.");
            }

            model.UploadedBy = userId;
            model.CreatedAt = DateTime.Now;

            foreach (var file in model.Files)
            {
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();
                    model.DocumentBase64 = Convert.ToBase64String(fileBytes); // Dosyanın Base64 formatında saklanması
                    model.FileName = file.FileName; // Dosya ismi ve uzantısının saklanması
                }
            }

            using (var connection = new SqlConnection(_configurationSettings.ConnectionString))
            {
                using (var command = new SqlCommand("AddDocument", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DocumentName", model.DocumentName); // Kullanıcıdan alınan belge adı
                    command.Parameters.AddWithValue("@FileName", model.FileName); // Dosya ismi ve uzantısı
                    command.Parameters.AddWithValue("@DocumentBase64", model.DocumentBase64); // Dosyanın Base64 formatındaki hali
                    command.Parameters.AddWithValue("@UploadedBy", model.UploadedBy); // Giriş yapmış kullanıcının ID'si
                    command.Parameters.AddWithValue("@CreatedAt", model.CreatedAt); // Dosyanın yüklenme zamanı

                    try
                    {
                        connection.Open();
                        var result = await command.ExecuteScalarAsync();
                        model.DocumentId = Convert.ToInt32(result); // Veritabanında otomatik oluşan DocumentId
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error: " + ex.Message);
                    }
                }
            }

            return Ok(model.DocumentId); // DocumentId'yi döner
        }


        [HttpGet]
        public async Task<IActionResult> GetDocuments()
        {
            var documents = new List<Document>();

            using (var connection = new SqlConnection(_configurationSettings.ConnectionString))
            {
                using (var command = new SqlCommand("GetDocuments", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            documents.Add(new Document
                            {
                                DocumentId = reader.GetInt32(0),
                                DocumentName = reader.GetString(1),
                                FileName = reader.GetString(2),
                                UploadedByUsername = reader.GetString(3), // Correct index for username
                                CreatedAt = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }

            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(int id)
        {
            Document document = null;

            using (var connection = new SqlConnection(_configurationSettings.ConnectionString))
            {
                using (var command = new SqlCommand("GetDocumentDetailsById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DocumentId", id);

                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            document = new Document
                            {
                                DocumentId = reader.GetInt32(0),
                                DocumentName = reader.GetString(1),
                                FileName = reader.GetString(2),
                                DocumentBase64 = reader.GetString(3),
                                UploadedByUsername = reader.GetString(4),
                                CreatedAt = reader.GetDateTime(5)
                            };
                        }
                    }
                }
            }

            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }



        [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteDocument(int id)
            {
                using (var connection = new SqlConnection(_configurationSettings.ConnectionString))
                {
                    using (var command = new SqlCommand("DeleteDocument", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@DocumentId", id);

                        connection.Open();
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok();
            }
        }
    }
