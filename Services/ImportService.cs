using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using dttbidsmxbb.Data;
using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;
using dttbidsmxbb.Models.Enum;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace dttbidsmxbb.Services
{
    public class ImportService(AppDbContext db) : IImportService
    {
        public async Task<ImportResult> ImportBackupAsync(Stream fileStream, bool cleanMode)
        {
            var result = new ImportResult();

            BackupExportDto? backup;
            try
            {
                await using var gz = new GZipStream(fileStream, CompressionMode.Decompress);
                backup = await JsonSerializer.DeserializeAsync<BackupExportDto>(gz, BackupJsonOpts);
            }
            catch
            {
                result.Errors.Add(new ImportError { Row = 0, Field = "Fayl", Message = "Fayl formatı yanlışdır." });
                return result;
            }

            if (backup?.Records == null || backup.Records.Count == 0)
            {
                result.Errors.Add(new ImportError { Row = 0, Field = "Fayl", Message = "Faylda heç bir məlumat tapılmadı." });
                return result;
            }

            result.TotalRows = backup.Records.Count;

            var baseLookup = await db.MilitaryBases.ToDictionaryAsync(x => x.Name.ToLower(), x => x.Id);
            var rankLookup = await db.MilitaryRanks.ToDictionaryAsync(x => x.Name.ToLower(), x => x.Id);
            var execLookup = await db.Executors.ToDictionaryAsync(x => x.FullInfo.ToLower(), x => x.Id);

            HashSet<string>? existingFingerprints = null;
            if (!cleanMode)
            {
                var existing = await db.Informations
                    .Where(x => !x.DeletedAt.HasValue)
                    .AsNoTracking()
                    .ToListAsync();
                existingFingerprints = new HashSet<string>(existing.Select(Fingerprint));
            }

            var toInsert = new List<Information>();

            for (var i = 0; i < backup.Records.Count; i++)
            {
                var rec = backup.Records[i];
                var rowNum = i + 1;

                var baseId = await ResolveLookup(rec.MilitaryBaseName, baseLookup, LookupKind.Base);
                var senderBaseId = await ResolveLookup(rec.SenderMilitaryBaseName, baseLookup, LookupKind.Base);
                var rankId = await ResolveLookup(rec.MilitaryRankName, rankLookup, LookupKind.Rank);
                var execId = await ResolveLookup(rec.ExecutorFullInfo, execLookup, LookupKind.Executor);

                if (baseId == 0 || senderBaseId == 0 || rankId == 0 || execId == 0)
                {
                    result.Errors.Add(new ImportError { Row = rowNum, Field = "Axtarış", Message = "Axtarış dəyərləri həll edilə bilmədi." });
                    result.SkippedRows++;
                    continue;
                }

                if (!Enum.IsDefined(typeof(PrivacyLevel), rec.PrivacyLevel))
                {
                    result.Errors.Add(new ImportError { Row = rowNum, Field = "Buraxılış forması", Message = "Yanlış dəyər." });
                    result.SkippedRows++;
                    continue;
                }

                var entity = new Information
                {
                    MilitaryBaseId = baseId,
                    SenderMilitaryBaseId = senderBaseId,
                    SentSerialNumber = rec.SentSerialNumber,
                    SentDate = rec.SentDate,
                    ReceivedSerialNumber = rec.ReceivedSerialNumber,
                    ReceivedDate = rec.ReceivedDate,
                    MilitaryRankId = rankId,
                    RegardingPosition = rec.RegardingPosition,
                    Position = rec.Position,
                    Lastname = rec.Lastname,
                    Firstname = rec.Firstname,
                    Fathername = rec.Fathername,
                    AssignmentDate = rec.AssignmentDate,
                    PrivacyLevel = (PrivacyLevel)rec.PrivacyLevel,
                    SendAwaySerialNumber = rec.SendAwaySerialNumber,
                    SendAwayDate = rec.SendAwayDate,
                    ExecutorId = execId,
                    FormalizationSerialNumber = rec.FormalizationSerialNumber,
                    FormalizationDate = rec.FormalizationDate,
                    RejectionInfo = rec.RejectionInfo,
                    SentBackInfo = rec.SentBackInfo,
                    Note = rec.Note,
                    CreatedAt = DateTime.UtcNow
                };

                if (!cleanMode && existingFingerprints != null)
                {
                    var fp = Fingerprint(entity);
                    if (existingFingerprints.Contains(fp))
                    {
                        result.SkippedRows++;
                        continue;
                    }
                    existingFingerprints.Add(fp);
                }

                toInsert.Add(entity);
            }

            if (cleanMode && toInsert.Count > 0)
                await db.Informations.Where(x => !x.DeletedAt.HasValue).ExecuteDeleteAsync();

            if (toInsert.Count > 0)
            {
                db.Informations.AddRange(toInsert);
                await db.SaveChangesAsync();
            }

            result.ImportedRows = toInsert.Count;
            result.Success = result.Errors.Count == 0;
            return result;
        }

        private enum LookupKind { Base, Rank, Executor }

        private async Task<int> ResolveLookup(string name, Dictionary<string, int> lookup, LookupKind kind)
        {
            if (string.IsNullOrWhiteSpace(name))
                return 0;

            var key = name.Trim().ToLower();
            if (lookup.TryGetValue(key, out var id))
                return id;

            var trimmed = name.Trim();
            int newId;

            switch (kind)
            {
                case LookupKind.Base:
                    var mb = new MilitaryBase { Name = trimmed };
                    db.MilitaryBases.Add(mb);
                    await db.SaveChangesAsync();
                    newId = mb.Id;
                    break;
                case LookupKind.Rank:
                    var mr = new MilitaryRank { Name = trimmed };
                    db.MilitaryRanks.Add(mr);
                    await db.SaveChangesAsync();
                    newId = mr.Id;
                    break;
                case LookupKind.Executor:
                    var ex = new Executor { FullInfo = trimmed };
                    db.Executors.Add(ex);
                    await db.SaveChangesAsync();
                    newId = ex.Id;
                    break;
                default:
                    return 0;
            }

            lookup[key] = newId;
            return newId;
        }

        private static string Fingerprint(Information i)
        {
            var sb = new StringBuilder(512);
            sb.Append(i.MilitaryBaseId).Append('|');
            sb.Append(i.SenderMilitaryBaseId).Append('|');
            sb.Append(i.SentSerialNumber).Append('|');
            sb.Append(i.SentDate).Append('|');
            sb.Append(i.ReceivedSerialNumber).Append('|');
            sb.Append(i.ReceivedDate).Append('|');
            sb.Append(i.MilitaryRankId).Append('|');
            sb.Append(i.RegardingPosition).Append('|');
            sb.Append(i.Position).Append('|');
            sb.Append(i.Lastname ?? "").Append('|');
            sb.Append(i.Firstname).Append('|');
            sb.Append(i.Fathername ?? "").Append('|');
            sb.Append(i.AssignmentDate).Append('|');
            sb.Append((int)i.PrivacyLevel).Append('|');
            sb.Append(i.SendAwaySerialNumber ?? "").Append('|');
            sb.Append(i.SendAwayDate?.ToString() ?? "").Append('|');
            sb.Append(i.ExecutorId).Append('|');
            sb.Append(i.FormalizationSerialNumber ?? "").Append('|');
            sb.Append(i.FormalizationDate?.ToString() ?? "").Append('|');
            sb.Append(i.RejectionInfo ?? "").Append('|');
            sb.Append(i.SentBackInfo ?? "").Append('|');
            sb.Append(i.Note ?? "");

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToHexString(bytes);
        }

        private static readonly JsonSerializerOptions BackupJsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

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
                if (int.TryParse(privacyStr, out var privacyInt) && Enum.IsDefined(typeof(PrivacyLevel), privacyInt))
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