using ExcelToSQLConverter.Data;
using ExcelToSQLConverter.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IO;
using System.Text;

namespace ExcelToSQLConverter.Controllers
{
    public class ExcelController : Controller
    {
        private readonly YourDbContext _context;

        public ExcelController(YourDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a file.");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });

            var dataTable = result.Tables[0];
            string sqlStatements = GenerateSqlStatements(dataTable, Path.GetFileNameWithoutExtension(file.FileName));

            // Return the view with the SQL statements
            return View("SqlResult", new { SqlStatements = sqlStatements });
        }

        private string GenerateSqlStatements(DataTable dataTable, string tableName)
        {
            StringBuilder sqlBuilder = new StringBuilder();

            // CREATE TABLE statement
            sqlBuilder.AppendLine($"/* Showing results for {tableName} */");
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine($"/* CREATE TABLE */");
            sqlBuilder.AppendLine($"CREATE TABLE {tableName}(");
            foreach (DataColumn column in dataTable.Columns)
            {
                sqlBuilder.AppendLine($"[{column.ColumnName}] VARCHAR(MAX),"); // Adjust data type as needed
            }
            sqlBuilder.Length--; // Remove the last comma
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine(");");
            sqlBuilder.AppendLine();

            // INSERT statements
            int queryNumber = 1;
            foreach (DataRow row in dataTable.Rows)
            {
                string[] columns = dataTable.Columns.Cast<DataColumn>()
                    .Select(c => $"[{c.ColumnName}]")
                    .ToArray();

                string[] values = row.ItemArray
                    .Select(v => $"'{EscapeSqlValue(v.ToString())}'")
                    .ToArray();

                sqlBuilder.AppendLine($"/* INSERT QUERY NO: {queryNumber} */");
                sqlBuilder.AppendLine($"INSERT INTO {tableName}({string.Join(", ", columns)})");
                sqlBuilder.AppendLine($"VALUES ({string.Join(", ", values)});");
                sqlBuilder.AppendLine();

                queryNumber++;
            }

            return sqlBuilder.ToString();
        }

        private string EscapeSqlValue(string value)
        {
            return value.Replace("'", "''"); // Escape single quotes for SQL
        }
    }
}
