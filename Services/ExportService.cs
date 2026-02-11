using ClosedXML.Excel;
using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;
using dttbidsmxbb.Models.Enum;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using QuestDocument = QuestPDF.Fluent.Document;
using WordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using WordPageSize = DocumentFormat.OpenXml.Wordprocessing.PageSize;

namespace dttbidsmxbb.Services
{
    public class ExportService : IExportService
    {
        private static readonly (string Header, Func<Information, string> ValueFn)[] AllColumns =
        [
            ("Göndərən h/h", i => i.SenderMilitaryBase?.Name ?? ""),
            ("Hərbi hissə", i => i.MilitaryBase?.Name ?? ""),
            ("Göndərilmə №", i => i.SentSerialNumber),
            ("Göndərilmə tarixi", i => i.SentDate.ToString("dd.MM.yyyy")),
            ("Daxil olma №", i => i.ReceivedSerialNumber),
            ("Daxil olma tarixi", i => i.ReceivedDate.ToString("dd.MM.yyyy")),
            ("Rütbə", i => i.MilitaryRank?.Name ?? ""),
            ("Rəsmiləşdirildiyi vəzifə", i => i.RegardingPosition),
            ("Vəzifə", i => i.Position),
            ("Soyad", i => i.Lastname ?? ""),
            ("Ad", i => i.Firstname),
            ("Ata adı", i => i.Fathername ?? ""),
            ("Təyin olunma tarixi", i => i.AssignmentDate.ToString("dd.MM.yyyy")),
            ("Buraxılış forması", i => i.PrivacyLevel == PrivacyLevel.TopSecret ? "Tam məxfi" : "Məxfi"),
            ("DTX-a göndərilmə №", i => i.SendAwaySerialNumber ?? ""),
            ("DTX-a göndərilmə tarixi", i => i.SendAwayDate?.ToString("dd.MM.yyyy") ?? ""),
            ("İcraçı", i => i.Executor?.FullInfo ?? ""),
            ("Vərəqə №", i => i.FormalizationSerialNumber ?? ""),
            ("Vərəqə tarixi", i => i.FormalizationDate?.ToString("dd.MM.yyyy") ?? ""),
            ("İmtina", i => i.RejectionInfo ?? ""),
            ("Geri qaytarılma", i => i.SentBackInfo ?? ""),
            ("Qeyd", i => i.Note ?? "")
        ];

        private static (string[] Headers, Func<Information, string>[] ValueFns) ResolveColumns(int[]? visibleColumns)
        {
            var indices = visibleColumns ?? Enumerable.Range(0, AllColumns.Length).ToArray();
            var valid = indices.Where(i => i >= 0 && i < AllColumns.Length).ToArray();
            if (valid.Length == 0)
                valid = Enumerable.Range(0, AllColumns.Length).ToArray();

            return (
                valid.Select(i => AllColumns[i].Header).ToArray(),
                valid.Select(i => AllColumns[i].ValueFn).ToArray()
            );
        }

        private static string[] GetRowValues(Information item, int rowNum, Func<Information, string>[] fns)
        {
            var vals = new string[fns.Length + 1];
            vals[0] = rowNum.ToString();
            for (var i = 0; i < fns.Length; i++)
                vals[i + 1] = fns[i](item);
            return vals;
        }

        public Task<byte[]> ExportToPdfAsync(List<Information> data, int[]? visibleColumns = null)
        {
            var (headers, fns) = ResolveColumns(visibleColumns);
            var allHeaders = new[] { "№" }.Concat(headers).ToArray();

            QuestPDF.Settings.License = LicenseType.Community;

            var document = QuestDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A3.Landscape());
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontSize(7));
                    page.Header().Text("Məlumatlar").FontSize(14).Bold().AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.5f);
                            for (var i = 1; i < allHeaders.Length; i++)
                                columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            foreach (var h in allHeaders)
                                header.Cell().Background(Colors.Grey.Lighten3)
                                    .Border(0.5f).BorderColor(Colors.Grey.Medium)
                                    .Padding(2).Text(h).Bold().FontSize(6);
                        });

                        var rowNum = 1;
                        foreach (var item in data)
                        {
                            foreach (var val in GetRowValues(item, rowNum++, fns))
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(2).Text(val).FontSize(6);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Səhifə ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }

        public async Task<byte[]> ExportToExcelAsync(List<Information> data, int[]? visibleColumns = null)
        {
            var (headers, fns) = ResolveColumns(visibleColumns);
            var allHeaders = new[] { "№" }.Concat(headers).ToArray();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Məlumatlar");

            for (var i = 0; i < allHeaders.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = allHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            var rowNum = 1;
            for (var r = 0; r < data.Count; r++)
            {
                var values = GetRowValues(data[r], rowNum++, fns);
                for (var c = 0; c < values.Length; c++)
                {
                    var cell = ws.Cell(r + 2, c + 1);
                    cell.Value = values[c];
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
            }

            ws.Columns().AdjustToContents();

            await using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportToWordAsync(List<Information> data, int[]? visibleColumns = null)
        {
            var (headers, fns) = ResolveColumns(visibleColumns);
            var allHeaders = new[] { "№" }.Concat(headers).ToArray();

            await using var stream = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new WordDocument();
                var body = mainPart.Document.AppendChild(new Body());

                var sectionProps = new SectionProperties(
                    new WordPageSize { Width = 16838, Height = 11906, Orient = PageOrientationValues.Landscape },
                    new PageMargin { Top = 500, Right = 500, Bottom = 500, Left = 500 });

                body.AppendChild(new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                    new Run(
                        new RunProperties(new Bold(), new FontSize { Val = "28" }),
                        new Text("Məlumatlar"))));

                var table = new Table();
                table.AppendChild(new TableProperties(
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 4 },
                        new BottomBorder { Val = BorderValues.Single, Size = 4 },
                        new LeftBorder { Val = BorderValues.Single, Size = 4 },
                        new RightBorder { Val = BorderValues.Single, Size = 4 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }),
                    new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" }));

                var headerRow = new TableRow();
                foreach (var h in allHeaders)
                    headerRow.AppendChild(new TableCell(
                        new TableCellProperties(new Shading { Fill = "D9D9D9", Val = ShadingPatternValues.Clear }),
                        new Paragraph(new Run(
                            new RunProperties(new Bold(), new FontSize { Val = "14" }),
                            new Text(h)))));
                table.AppendChild(headerRow);

                var rowNum = 1;
                foreach (var item in data)
                {
                    var tr = new TableRow();
                    foreach (var val in GetRowValues(item, rowNum++, fns))
                        tr.AppendChild(new TableCell(
                            new Paragraph(new Run(
                                new RunProperties(new FontSize { Val = "14" }),
                                new Text(val)))));
                    table.AppendChild(tr);
                }

                body.AppendChild(table);
                body.AppendChild(sectionProps);
            }

            return stream.ToArray();
        }

        public async Task<byte[]> GenerateImportTemplateAsync()
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Şablon");

            string[] templateHeaders =
            [
                "Göndərən h/h", "Hərbi hissə", "Göndərilmə №", "Göndərilmə tarixi",
                "Daxil olma №", "Daxil olma tarixi", "Rütbə", "Rəsmiləşdirildiyi vəzifə",
                "Vəzifə", "Soyad", "Ad", "Ata adı", "Təyin olunma tarixi",
                "Buraxılış forması (1=Tam məxfi, 2=Məxfi)", "DTX-a göndərilmə №", "DTX-a göndərilmə tarixi",
                "İcraçı", "Vərəqə №", "Vərəqə tarixi", "İmtina", "Geri qaytarılma", "Qeyd"
            ];

            for (var i = 0; i < templateHeaders.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = templateHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            ws.Columns().AdjustToContents();

            await using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportBackupAsync(List<Information> data)
        {
            var dto = new BackupExportDto
            {
                ExportedAt = DateTime.UtcNow,
                RecordCount = data.Count,
                Records = data.Select(i => new InformationBackupDto
                {
                    MilitaryBaseName = i.MilitaryBase?.Name ?? "",
                    SenderMilitaryBaseName = i.SenderMilitaryBase?.Name ?? "",
                    SentSerialNumber = i.SentSerialNumber,
                    SentDate = i.SentDate,
                    ReceivedSerialNumber = i.ReceivedSerialNumber,
                    ReceivedDate = i.ReceivedDate,
                    MilitaryRankName = i.MilitaryRank?.Name ?? "",
                    RegardingPosition = i.RegardingPosition,
                    Position = i.Position,
                    Lastname = i.Lastname,
                    Firstname = i.Firstname,
                    Fathername = i.Fathername,
                    AssignmentDate = i.AssignmentDate,
                    PrivacyLevel = (int)i.PrivacyLevel,
                    SendAwaySerialNumber = i.SendAwaySerialNumber,
                    SendAwayDate = i.SendAwayDate,
                    ExecutorFullInfo = i.Executor?.FullInfo ?? "",
                    FormalizationSerialNumber = i.FormalizationSerialNumber,
                    FormalizationDate = i.FormalizationDate,
                    RejectionInfo = i.RejectionInfo,
                    SentBackInfo = i.SentBackInfo,
                    Note = i.Note
                }).ToList()
            };

            var json = JsonSerializer.SerializeToUtf8Bytes(dto, BackupJsonOpts);

            await using var ms = new MemoryStream();
            await using (var gz = new GZipStream(ms, CompressionLevel.Fastest, leaveOpen: true))
                await gz.WriteAsync(json);

            return ms.ToArray();
        }

        public string GeneratePrintHtml(List<Information> data, int[]? visibleColumns = null)
        {
            var (headers, fns) = ResolveColumns(visibleColumns);
            var sb = new StringBuilder();

            sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
            sb.Append("<title>Məlumatlar</title>");
            sb.Append("<style>");
            sb.Append("body{font-family:Arial,sans-serif;font-size:9px;margin:10px;}");
            sb.Append("h2{text-align:center;font-size:14px;margin-bottom:8px;}");
            sb.Append("table{width:100%;border-collapse:collapse;}");
            sb.Append("th,td{border:1px solid #999;padding:3px 4px;text-align:left;}");
            sb.Append("th{background:#e0e0e0;font-weight:bold;}");
            sb.Append("tr:nth-child(even){background:#f5f5f5;}");
            sb.Append("@media print{@page{size:landscape;margin:8mm;}}");
            sb.Append("</style></head><body>");
            sb.Append("<h2>Məlumatlar</h2>");
            sb.Append("<table><thead><tr><th>№</th>");

            foreach (var h in headers)
                sb.Append("<th>").Append(Enc(h)).Append("</th>");

            sb.Append("</tr></thead><tbody>");

            var rowNum = 1;
            foreach (var item in data)
            {
                sb.Append("<tr><td>").Append(rowNum++).Append("</td>");
                foreach (var fn in fns)
                    sb.Append("<td>").Append(Enc(fn(item))).Append("</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            sb.Append("<script>window.onload=function(){window.print();}</script>");
            sb.Append("</body></html>");

            return sb.ToString();
        }

        private static string Enc(string s) => HtmlEncoder.Default.Encode(s);

        private static readonly JsonSerializerOptions BackupJsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}