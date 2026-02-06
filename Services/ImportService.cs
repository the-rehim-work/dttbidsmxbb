using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using dttbidsmxbb.Data;
using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;
using dttbidsmxbb.Models.Enum;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace dttbidsmxbb.Services
{
    public class ImportService(AppDbContext db) : IImportService
    {
        public async Task<ImportResult> ImportAsync(Stream fileStream, string fileName, bool useAsDb)
        {
            var result = new ImportResult();
            var rows = fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? ParseCsv(fileStream)
                : ParseExcel(fileStream);

            result.TotalRows = rows.Count;

            var militaryBases = await db.MilitaryBases.ToDictionaryAsync(x => x.Name.ToLower(), x => x.Id);
            var militaryRanks = await db.MilitaryRanks.ToDictionaryAsync(x => x.Name.ToLower(), x => x.Id);
            var executors = await db.Executors.ToDictionaryAsync(x => x.FullInfo.ToLower(), x => x.Id);

            var validEntities = new List<Information>();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var rowNum = i + 2;
                var entity = new Information();
                var rowValid = true;

                if (!TryResolveFk(row, 0, "Göndərən h/h", militaryBases, rowNum, result, out var senderBaseId))
                    rowValid = false;
                else
                    entity.SenderMilitaryBaseId = senderBaseId;

                if (!TryResolveFk(row, 1, "Hərbi hissə", militaryBases, rowNum, result, out var baseId))
                    rowValid = false;
                else
                    entity.MilitaryBaseId = baseId;

                if (!TryResolveRequired(row, 2, "Göndərilmə №", rowNum, result, out var sentSerial))
                    rowValid = false;
                else
                    entity.SentSerialNumber = sentSerial;

                if (!TryResolveDate(row, 3, "Göndərilmə tarixi", rowNum, result, out var sentDate))
                    rowValid = false;
                else
                    entity.SentDate = sentDate;

                if (!TryResolveRequired(row, 4, "Daxil olma №", rowNum, result, out var recSerial))
                    rowValid = false;
                else
                    entity.ReceivedSerialNumber = recSerial;

                if (!TryResolveDate(row, 5, "Daxil olma tarixi", rowNum, result, out var recDate))
                    rowValid = false;
                else
                    entity.ReceivedDate = recDate;

                if (!TryResolveFk(row, 6, "Rütbə", militaryRanks, rowNum, result, out var rankId))
                    rowValid = false;
                else
                    entity.MilitaryRankId = rankId;

                if (!TryResolveRequired(row, 7, "Rəsmiləşdirildiyi vəzifə", rowNum, result, out var regPos))
                    rowValid = false;
                else
                    entity.RegardingPosition = regPos;

                if (!TryResolveRequired(row, 8, "Vəzifə", rowNum, result, out var pos))
                    rowValid = false;
                else
                    entity.Position = pos;

                entity.Lastname = GetCell(row, 9);
                entity.Firstname = GetCell(row, 10) ?? "";
                entity.Fathername = GetCell(row, 11);

                if (string.IsNullOrWhiteSpace(entity.Firstname))
                {
                    result.Errors.Add(new ImportError { Row = rowNum, Field = "Ad", Message = "Ad sahəsi mütləqdir." });
                    rowValid = false;
                }

                if (!TryResolveDate(row, 12, "Təyin olunma tarixi", rowNum, result, out var assignDate))
                    rowValid = false;
                else
                    entity.AssignmentDate = assignDate;

                var privacyStr = GetCell(row, 13);
                if (int.TryParse(privacyStr, out var privacyInt) && System.Enum.IsDefined(typeof(PrivacyLevel), privacyInt))
                    entity.PrivacyLevel = (PrivacyLevel)privacyInt;
                else
                {
                    result.Errors.Add(new ImportError { Row = rowNum, Field = "Buraxılış forması", Message = "1 (Tam məxfi) və ya 2 (Məxfi) olmalıdır." });
                    rowValid = false;
                }

                if (!TryResolveRequired(row, 14, "DTX-a göndərilmə №", rowNum, result, out var sendAwaySerial))
                    rowValid = false;
                else
                    entity.SendAwaySerialNumber = sendAwaySerial;

                if (!TryResolveDate(row, 15, "DTX-a göndərilmə tarixi", rowNum, result, out var sendAwayDate))
                    rowValid = false;
                else
                    entity.SendAwayDate = sendAwayDate;

                if (!TryResolveFk(row, 16, "İcraçı", executors, rowNum, result, out var execId))
                    rowValid = false;
                else
                    entity.ExecutorId = execId;

                if (!TryResolveRequired(row, 17, "Vərəqə №", rowNum, result, out var formSerial))
                    rowValid = false;
                else
                    entity.FormalizationSerialNumber = formSerial;

                if (!TryResolveDate(row, 18, "Vərəqə tarixi", rowNum, result, out var formDate))
                    rowValid = false;
                else
                    entity.FormalizationDate = formDate;

                entity.RejectionInfo = GetCell(row, 19);
                entity.SentBackInfo = GetCell(row, 20);
                entity.Note = GetCell(row, 21);

                if (rowValid)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                    validEntities.Add(entity);
                }
                else
                {
                    result.SkippedRows++;
                }
            }

            if (validEntities.Count > 0)
            {
                if (useAsDb)
                    await db.Informations.ExecuteDeleteAsync();

                db.Informations.AddRange(validEntities);
                await db.SaveChangesAsync();
            }

            result.ImportedRows = validEntities.Count;
            result.Success = result.Errors.Count == 0;

            return result;
        }

        private static List<string?[]> ParseExcel(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var rows = new List<string?[]>();
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 1;

            for (int r = 2; r <= lastRow; r++)
            {
                var row = new string?[lastCol];
                for (int c = 1; c <= lastCol; c++)
                    row[c - 1] = worksheet.Cell(r, c).GetString()?.Trim();

                if (row.All(string.IsNullOrWhiteSpace))
                    continue;

                rows.Add(row);
            }

            return rows;
        }

        private static List<string?[]> ParseCsv(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            });

            csv.Read();
            csv.ReadHeader();
            var rows = new List<string?[]>();

            while (csv.Read())
            {
                var fieldCount = csv.Parser.Count;
                var row = new string?[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                    row[i] = csv.GetField(i)?.Trim();

                if (row.All(string.IsNullOrWhiteSpace))
                    continue;

                rows.Add(row);
            }

            return rows;
        }

        private static string? GetCell(string?[] row, int index) =>
            index < row.Length ? row[index] : null;

        private static bool TryResolveRequired(string?[] row, int index, string fieldName, int rowNum, ImportResult result, out string value)
        {
            value = GetCell(row, index) ?? "";
            if (string.IsNullOrWhiteSpace(value))
            {
                result.Errors.Add(new ImportError { Row = rowNum, Field = fieldName, Message = $"{fieldName} sahəsi mütləqdir." });
                return false;
            }
            return true;
        }

        private static bool TryResolveDate(string?[] row, int index, string fieldName, int rowNum, ImportResult result, out DateOnly value)
        {
            value = default;
            var raw = GetCell(row, index);
            if (string.IsNullOrWhiteSpace(raw))
            {
                result.Errors.Add(new ImportError { Row = rowNum, Field = fieldName, Message = $"{fieldName} sahəsi mütləqdir." });
                return false;
            }

            string[] formats = ["dd.MM.yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "d.M.yyyy", "d/M/yyyy"];
            if (DateOnly.TryParseExact(raw, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
                return true;

            result.Errors.Add(new ImportError { Row = rowNum, Field = fieldName, Message = $"Tarix formatı yanlışdır: {raw}" });
            return false;
        }

        private static bool TryResolveFk(string?[] row, int index, string fieldName, Dictionary<string, int> lookup, int rowNum, ImportResult result, out int id)
        {
            id = 0;
            var raw = GetCell(row, index);
            if (string.IsNullOrWhiteSpace(raw))
            {
                result.Errors.Add(new ImportError { Row = rowNum, Field = fieldName, Message = $"{fieldName} sahəsi mütləqdir." });
                return false;
            }

            if (lookup.TryGetValue(raw.ToLower(), out id))
                return true;

            result.Errors.Add(new ImportError { Row = rowNum, Field = fieldName, Message = $"\"{raw}\" bazada tapılmadı." });
            return false;
        }
    }
}