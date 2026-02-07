using ClosedXML.Excel;
using dttbidsmxbb.Models;
using dttbidsmxbb.Models.Enum;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using QuestDocument = QuestPDF.Fluent.Document;
using WordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using WordPageSize = DocumentFormat.OpenXml.Wordprocessing.PageSize;

namespace dttbidsmxbb.Services
{
    public class ExportService : IExportService
    {
        private static readonly string[] ColumnHeaders =
        [
            "№", "Göndərən h/h", "Hərbi hissə", "Göndərilmə №", "Göndərilmə tarixi",
            "Daxil olma №", "Daxil olma tarixi", "Rütbə", "Rəsmiləşdirildiyi vəzifə",
            "Vəzifə", "Soyad", "Ad", "Ata adı", "Təyin olunma tarixi",
            "Buraxılış forması", "DTX-a göndərilmə №", "DTX-a göndərilmə tarixi",
            "İcraçı", "Vərəqə №", "Vərəqə tarixi", "İmtina", "Geri qaytarılma", "Qeyd"
        ];

        public Task<byte[]> ExportToPdfAsync(List<Information> data)
        {
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
                            for (int i = 1; i < ColumnHeaders.Length; i++)
                                columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            foreach (var h in ColumnHeaders)
                            {
                                header.Cell().Background(Colors.Grey.Lighten3)
                                    .Border(0.5f).BorderColor(Colors.Grey.Medium)
                                    .Padding(2).Text(h).Bold().FontSize(6);
                            }
                        });

                        var rowNum = 1;
                        foreach (var item in data)
                        {
                            var values = GetRowValues(item, rowNum++);
                            foreach (var val in values)
                            {
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(2).Text(val).FontSize(6);
                            }
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

            var bytes = document.GeneratePdf();
            return Task.FromResult(bytes);
        }

        public Task<byte[]> ExportToExcelAsync(List<Information> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Məlumatlar");

            for (int i = 0; i < ColumnHeaders.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = ColumnHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            var rowNum = 1;
            for (int r = 0; r < data.Count; r++)
            {
                var values = GetRowValues(data[r], rowNum++);
                for (int c = 0; c < values.Length; c++)
                {
                    var cell = worksheet.Cell(r + 2, c + 1);
                    cell.Value = values[c];
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }

        public Task<byte[]> ExportToWordAsync(List<Information> data)
        {
            using var stream = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new WordDocument();
                var body = mainPart.Document.AppendChild(new Body());

                var sectionProps = new SectionProperties(
                    new WordPageSize { Width = 16838, Height = 11906, Orient = PageOrientationValues.Landscape },
                    new PageMargin { Top = 500, Right = 500, Bottom = 500, Left = 500 });

                var title = new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                    new Run(
                        new RunProperties(new Bold(), new FontSize { Val = "28" }),
                        new Text("Məlumatlar")));
                body.AppendChild(title);

                var table = new Table();
                var tblProps = new TableProperties(
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 4 },
                        new BottomBorder { Val = BorderValues.Single, Size = 4 },
                        new LeftBorder { Val = BorderValues.Single, Size = 4 },
                        new RightBorder { Val = BorderValues.Single, Size = 4 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }),
                    new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" });
                table.AppendChild(tblProps);

                var headerRow = new TableRow();
                foreach (var h in ColumnHeaders)
                {
                    var tc = new TableCell(
                        new TableCellProperties(new Shading { Fill = "D9D9D9", Val = ShadingPatternValues.Clear }),
                        new Paragraph(new Run(
                            new RunProperties(new Bold(), new FontSize { Val = "14" }),
                            new Text(h))));
                    headerRow.AppendChild(tc);
                }
                table.AppendChild(headerRow);

                var rowNum = 1;
                foreach (var item in data)
                {
                    var values = GetRowValues(item, rowNum++);
                    var tr = new TableRow();
                    foreach (var val in values)
                    {
                        var tc = new TableCell(
                            new Paragraph(new Run(
                                new RunProperties(new FontSize { Val = "14" }),
                                new Text(val))));
                        tr.AppendChild(tc);
                    }
                    table.AppendChild(tr);
                }

                body.AppendChild(table);
                body.AppendChild(sectionProps);
            }

            return Task.FromResult(stream.ToArray());
        }

        public Task<byte[]> GenerateImportTemplateAsync()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Şablon");

            string[] templateHeaders =
            [
                "Göndərən h/h", "Hərbi hissə", "Göndərilmə №", "Göndərilmə tarixi",
                "Daxil olma №", "Daxil olma tarixi", "Rütbə", "Rəsmiləşdirildiyi vəzifə",
                "Vəzifə", "Soyad", "Ad", "Ata adı", "Təyin olunma tarixi",
                "Buraxılış forması (1=Tam məxfi, 2=Məxfi)", "DTX-a göndərilmə №", "DTX-a göndərilmə tarixi",
                "İcraçı", "Vərəqə №", "Vərəqə tarixi", "İmtina", "Geri qaytarılma", "Qeyd"
            ];

            for (int i = 0; i < templateHeaders.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = templateHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }

        private static string[] GetRowValues(Information item, int rowNum)
        {
            return
            [
                rowNum.ToString(),
                item.SenderMilitaryBase?.Name ?? "",
                item.MilitaryBase?.Name ?? "",
                item.SentSerialNumber,
                item.SentDate.ToString("dd.MM.yyyy"),
                item.ReceivedSerialNumber,
                item.ReceivedDate.ToString("dd.MM.yyyy"),
                item.MilitaryRank?.Name ?? "",
                item.RegardingPosition,
                item.Position,
                item.Lastname ?? "",
                item.Firstname,
                item.Fathername ?? "",
                item.AssignmentDate.ToString("dd.MM.yyyy"),
                item.PrivacyLevel == PrivacyLevel.TopSecret ? "Tam məxfi" : "Məxfi",
                item.SendAwaySerialNumber,
                item.SendAwayDate.ToString("dd.MM.yyyy"),
                item.Executor?.FullInfo ?? "",
                item.FormalizationSerialNumber,
                item.FormalizationDate?.ToString("dd.MM.yyyy"),
                item.RejectionInfo ?? "",
                item.SentBackInfo ?? "",
                item.Note ?? ""
            ];
        }
    }
}