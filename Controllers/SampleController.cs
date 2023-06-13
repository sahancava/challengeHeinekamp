using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using challengeHeinekamp.Models;
using challengeHeinekamp.Helpers;

namespace challengeHeinekamp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {
        private readonly string _connectionString;
        private bool _isDatabaseInitialized;
        private readonly IConfiguration _configuration;

        public SampleController(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionStringBuilder = new SqliteConnectionStringBuilder{
                DataSource = "./mydb.db"
            };
            _connectionString = connectionStringBuilder.ConnectionString;
            _isDatabaseInitialized = _configuration.GetValue<bool>("isDatabaseInitialized");
        }

        [HttpGet]
        public IEnumerable<InputModel> Get()
        {
            List<InputModel> list = new List<InputModel>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                InitializeDatabase(connection);
                list = SeedData(connection);
                connection.Close();
            }
            return list.ToArray();
        }

        [HttpPost]
        [Route("UploadFile")]
        public JsonResult UploadFile([FromForm] IFormFile file, [FromForm] string name, [FromForm] string uploadedBy, [FromForm] bool shareableFile, [FromForm] int numberOfHoursToExpire)
        {
            if (file == null || file.Length == 0)
            {
                return new JsonResult(BadRequest(new ErrorResponse { Error = "No file uploaded." }));
            }

            if (string.IsNullOrEmpty(name))
            {
                return new JsonResult(new ErrorResponse { Error = "No file name provided." }) { StatusCode = 400 };
            }

            if (string.IsNullOrEmpty(uploadedBy))
            {
                return new JsonResult(new ErrorResponse { Error = "No user name provided." }) { StatusCode = 400 };
            }

            if (shareableFile) {
                if (numberOfHoursToExpire == 0) {
                    return new JsonResult(new ErrorResponse { Error = "No number of hours to expire provided." }) { StatusCode = 400 };
                }
            }

            var allowedExtensions = new[] { ".pdf", ".xls", ".xlsx", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return new JsonResult(new ErrorResponse { Error = "Invalid file type. Only PDF, Excel, Word, text, and image files are allowed." }) { StatusCode = 400 };
            }

            var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine("ClientApp/public/files", fileName);
            var sharingGUID = Guid.NewGuid();

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    var insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = "INSERT INTO files (name, originalFileName, uploadDateTime, downloadCount, uploadedBy, extension, GUID, latestDownloadDateTime) VALUES (@name, @originalFileName, @uploadDateTime, @downloadCount, @uploadedBy, @extension, @GUID, @latestDownloadDateTime)";
                    insertCmd.Parameters.AddWithValue("@name", fileName);
                    insertCmd.Parameters.AddWithValue("@originalFileName", name);
                    insertCmd.Parameters.AddWithValue("@uploadDateTime", DateTime.Now);
                    insertCmd.Parameters.AddWithValue("@downloadCount", 0);
                    insertCmd.Parameters.AddWithValue("@uploadedBy", uploadedBy);
                    insertCmd.Parameters.AddWithValue("@extension", fileExtension);
                    insertCmd.Parameters.AddWithValue("@GUID", shareableFile ? sharingGUID : Guid.Empty);
                    insertCmd.Parameters.AddWithValue("@latestDownloadDateTime", shareableFile ? DateTime.Now.AddHours(numberOfHoursToExpire) : DateTime.MinValue);
                    insertCmd.ExecuteNonQuery();
                }

                return new JsonResult(new SuccessResponse { Message = sharingGUID.ToString() }) { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new ErrorResponse { Error = "An error occurred while uploading the file.", Details = ex.Message }) { StatusCode = 500 };
            }
        }

        [HttpGet]
        [Route("DownloadFile")]
        public async Task<IActionResult> DownloadFile(int? id, string guid)
        {
            if (id == null || id == 0)
            {
                return NotFound(new ErrorResponse { Error = "No file id provided." });
            }

            if (string.IsNullOrEmpty(guid))
            {
                return BadRequest(new ErrorResponse { Error = "No GUID provided." });
            }

            bool checkIfFileExists = false;
            string nameOfTheFile = string.Empty;
            var sharingGUID = Guid.Empty;
            DateTime latestDownloadDateTime = DateTime.MinValue;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                InitializeDatabase(connection);
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = "select id, name, GUID, latestDownloadDateTime from files where id = @id";
                insertCmd.Parameters.AddWithValue("@id", id);
                using (var reader = insertCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nameOfTheFile = reader.GetString(1);
                        sharingGUID = reader.GetGuid(2);
                        latestDownloadDateTime = reader.GetDateTime(3);
                        checkIfFileExists = true;
                    }
                }
                connection.Close();
                if (checkIfFileExists)
                {
                    if (sharingGUID != Guid.Empty && sharingGUID.ToString() == guid && DateTime.Now < latestDownloadDateTime) {
                        try
                        {
                            connection.Open();
                            var updateCMD = connection.CreateCommand();
                            var selectCMD = connection.CreateCommand();
                            updateCMD.CommandText = "UPDATE files SET downloadCount = downloadCount + 1 WHERE id = @id";
                            selectCMD.CommandText = "SELECT name, extension FROM files WHERE id = @id";
                            selectCMD.Parameters.AddWithValue("@id", id);
                            updateCMD.Parameters.AddWithValue("@id", id);

                            updateCMD.ExecuteNonQuery();
                            using (var reader = selectCMD.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var nameOfTheSelectedFile = reader.GetString(0);
                                    var extensionOfTheFile = GetMimeType(reader.GetString(1));
                                    
                                    string filePath = Path.Combine("ClientApp/public/files", nameOfTheSelectedFile);
                                    var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                                    return File(bytes, extensionOfTheFile, Path.GetFileName(filePath));
                                }
                            }
                            connection.Close();
                        }
                        catch (System.Exception)
                        {
                            return NotFound(new ErrorResponse { Error = "Error with downloading the file." });
                        }
                    } else {
                        return BadRequest(new ErrorResponse { Error = "File is not shareable." });
                    }
                }
                else
                {
                    return NotFound(new ErrorResponse { Error = "File not found." });
                }
            }

            return Ok(new SuccessResponse { Message = nameOfTheFile });
        }
        private void InitializeDatabase(SqliteConnection connection)
        {
            if (!_isDatabaseInitialized)
            {
                DropAndCreateTable(connection);
                _isDatabaseInitialized = true;
                _configuration["isDatabaseInitialized"] = "true";
            }
        }
        public void DropAndCreateTable(SqliteConnection connection)
        {
            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS files (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name VARCHAR(30),
                    originalFileName VARCHAR(255),
                    uploadDateTime DATETIME,
                    downloadCount INTEGER,
                    uploadedBy VARCHAR(50),
                    extension VARCHAR(10),
                    GUID VARCHAR(36),
                    latestDownloadDateTime DATETIME
                )";
                createTableCmd.ExecuteNonQuery();
            }
            catch (System.Exception)
            {
                throw new System.Exception("Error creating table");
            }
        }
        public List<InputModel> SeedData(SqliteConnection connection)
        {
            List<InputModel> list = new List<InputModel>();
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT id, name, originalFileName, uploadDateTime, downloadCount, uploadedBy, extension, GUID, latestDownloadDateTime FROM files ORDER BY id DESC";

            try
            {
                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var name = reader.GetString(1);
                        var originalFileName = reader.GetString(2);
                        var uploadDateTime = reader.GetDateTime(3);
                        var downloadCount = reader.GetInt32(4);
                        var uploadedBy = reader.GetString(5);
                        var iconType = GetIconType(originalFileName);
                        var extension = reader.GetString(6).Split('.')[1];
                        var guid = reader.GetGuid(7);
                        var _latestDownloadDateTime = reader.GetDateTime(8);
                        list.Add(new InputModel { Id = id, Name = name, OriginalFileName = originalFileName, UploadDateTime = uploadDateTime, DownloadCount = downloadCount, UploadedBy = uploadedBy, Icon = iconType, Extension = extension, Guid = guid, latestDownloadDateTime = _latestDownloadDateTime });
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new System.Exception("Error reading data");
            }

            return list;
        }
        private string GetIconType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (extension)
            {
                case ".pdf":
                    return "faFilePdf";
                case ".xls":
                case ".xlsx":
                    return "faFileExcel";
                case ".doc":
                case ".docx":
                    return "faFileWord";
                case ".txt":
                    return "faFileText";
                case ".jpg":
                case ".jpeg":
                case ".png":
                    return "faImage";
                default:
                    return "faFileImage";
            }
        }
        private string GetMimeType(string extension) {
            string _extension = extension.ToLowerInvariant();
            switch (_extension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".xls":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".txt":
                    return "text/plain";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                default:
                    return "NONE";
            }
        }
    }
}
