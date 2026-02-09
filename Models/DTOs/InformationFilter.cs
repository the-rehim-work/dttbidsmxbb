using System.Globalization;

namespace dttbidsmxbb.Models.DTOs
{
    public class InformationFilter
    {
        public List<int> MilitaryBaseIds { get; set; } = [];
        public List<int> SenderMilitaryBaseIds { get; set; } = [];
        public List<int> MilitaryRankIds { get; set; } = [];
        public List<int> ExecutorIds { get; set; } = [];
        public List<int> PrivacyLevels { get; set; } = [];

        public DateOnly? SentDateFrom { get; set; }
        public DateOnly? SentDateTo { get; set; }
        public DateOnly? ReceivedDateFrom { get; set; }
        public DateOnly? ReceivedDateTo { get; set; }
        public DateOnly? AssignmentDateFrom { get; set; }
        public DateOnly? AssignmentDateTo { get; set; }
        public DateOnly? SendAwayDateFrom { get; set; }
        public DateOnly? SendAwayDateTo { get; set; }
        public DateOnly? FormalizationDateFrom { get; set; }
        public DateOnly? FormalizationDateTo { get; set; }

        public string? SentSerialNumberQuery { get; set; }
        public string? ReceivedSerialNumberQuery { get; set; }
        public string? RegardingPositionQuery { get; set; }
        public string? PositionQuery { get; set; }
        public string? FirstnameQuery { get; set; }
        public string? LastnameQuery { get; set; }
        public string? FathernameQuery { get; set; }
        public string? FormalizationSentSerialQuery { get; set; }
        public string? FormalizationSerialQuery { get; set; }
        public string? RejectionInfoQuery { get; set; }
        public string? SentBackInfoQuery { get; set; }
        public string? NoteQuery { get; set; }

        public string? LastnameNull { get; set; }
        public string? FathernameNull { get; set; }
        public string? RejectionInfoNull { get; set; }
        public string? SentBackInfoNull { get; set; }
        public string? NoteNull { get; set; }
        public string? FormalizationSentSerialNull { get; set; }
        public string? FormalizationSentDateNull { get; set; }
        public string? FormalizationSerialNull { get; set; }
        public string? FormalizationDateNull { get; set; }

        public bool HasAnyFilter =>
            MilitaryBaseIds.Count > 0 || SenderMilitaryBaseIds.Count > 0 ||
            MilitaryRankIds.Count > 0 || ExecutorIds.Count > 0 || PrivacyLevels.Count > 0 ||
            SentDateFrom.HasValue || SentDateTo.HasValue ||
            ReceivedDateFrom.HasValue || ReceivedDateTo.HasValue ||
            AssignmentDateFrom.HasValue || AssignmentDateTo.HasValue ||
            SendAwayDateFrom.HasValue || SendAwayDateTo.HasValue ||
            FormalizationDateFrom.HasValue || FormalizationDateTo.HasValue ||
            !string.IsNullOrEmpty(SentSerialNumberQuery) ||
            !string.IsNullOrEmpty(ReceivedSerialNumberQuery) ||
            !string.IsNullOrEmpty(RegardingPositionQuery) ||
            !string.IsNullOrEmpty(PositionQuery) ||
            !string.IsNullOrEmpty(FirstnameQuery) ||
            !string.IsNullOrEmpty(LastnameQuery) ||
            !string.IsNullOrEmpty(FathernameQuery) ||
            !string.IsNullOrEmpty(FormalizationSentSerialQuery) ||
            !string.IsNullOrEmpty(FormalizationSerialQuery) ||
            !string.IsNullOrEmpty(RejectionInfoQuery) ||
            !string.IsNullOrEmpty(SentBackInfoQuery) ||
            !string.IsNullOrEmpty(NoteQuery) ||
            !string.IsNullOrEmpty(LastnameNull) || !string.IsNullOrEmpty(FathernameNull) ||
            !string.IsNullOrEmpty(RejectionInfoNull) || !string.IsNullOrEmpty(SentBackInfoNull) ||
            !string.IsNullOrEmpty(NoteNull) ||
            !string.IsNullOrEmpty(FormalizationSentSerialNull) ||
            !string.IsNullOrEmpty(FormalizationSentDateNull) ||
            !string.IsNullOrEmpty(FormalizationSerialNull) ||
            !string.IsNullOrEmpty(FormalizationDateNull);

        private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd.MM.yyyy", "dd/MM/yyyy"];

        public static InformationFilter Parse(IFormCollection form)
        {
            return new InformationFilter
            {
                MilitaryBaseIds = ParseIntList(form["f_militaryBaseIds"]),
                SenderMilitaryBaseIds = ParseIntList(form["f_senderMilitaryBaseIds"]),
                MilitaryRankIds = ParseIntList(form["f_militaryRankIds"]),
                ExecutorIds = ParseIntList(form["f_executorIds"]),
                PrivacyLevels = ParseIntList(form["f_privacyLevels"]),

                SentDateFrom = ParseDate(form["f_sentDateFrom"]),
                SentDateTo = ParseDate(form["f_sentDateTo"]),
                ReceivedDateFrom = ParseDate(form["f_receivedDateFrom"]),
                ReceivedDateTo = ParseDate(form["f_receivedDateTo"]),
                AssignmentDateFrom = ParseDate(form["f_assignmentDateFrom"]),
                AssignmentDateTo = ParseDate(form["f_assignmentDateTo"]),
                SendAwayDateFrom = ParseDate(form["f_sendAwayDateFrom"]),
                SendAwayDateTo = ParseDate(form["f_sendAwayDateTo"]),
                FormalizationDateFrom = ParseDate(form["f_formalizationDateFrom"]),
                FormalizationDateTo = ParseDate(form["f_formalizationDateTo"]),

                SentSerialNumberQuery = TextValue(form["f_sentSerialNumberQuery"]),
                ReceivedSerialNumberQuery = TextValue(form["f_receivedSerialNumberQuery"]),
                RegardingPositionQuery = TextValue(form["f_regardingPositionQuery"]),
                PositionQuery = TextValue(form["f_positionQuery"]),
                FirstnameQuery = TextValue(form["f_firstnameQuery"]),
                LastnameQuery = TextValue(form["f_lastnameQuery"]),
                FathernameQuery = TextValue(form["f_fathernameQuery"]),
                FormalizationSentSerialQuery = TextValue(form["f_formalizationSentSerialQuery"]),
                FormalizationSerialQuery = TextValue(form["f_formalizationSerialQuery"]),
                RejectionInfoQuery = TextValue(form["f_rejectionInfoQuery"]),
                SentBackInfoQuery = TextValue(form["f_sentBackInfoQuery"]),
                NoteQuery = TextValue(form["f_noteQuery"]),

                LastnameNull = NullFilterValue(form["f_lastnameNull"]),
                FathernameNull = NullFilterValue(form["f_fathernameNull"]),
                RejectionInfoNull = NullFilterValue(form["f_rejectionInfoNull"]),
                SentBackInfoNull = NullFilterValue(form["f_sentBackInfoNull"]),
                NoteNull = NullFilterValue(form["f_noteNull"]),
                FormalizationSentSerialNull = NullFilterValue(form["f_formalizationSentSerialNull"]),
                FormalizationSentDateNull = NullFilterValue(form["f_formalizationSentDateNull"]),
                FormalizationSerialNull = NullFilterValue(form["f_formalizationSerialNull"]),
                FormalizationDateNull = NullFilterValue(form["f_formalizationDateNull"])
            };
        }

        private static List<int> ParseIntList(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return [];
            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => int.TryParse(x.Trim(), out var v) ? v : (int?)null)
                      .Where(x => x.HasValue)
                      .Select(x => x!.Value)
                      .ToList();
        }

        private static DateOnly? ParseDate(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (DateOnly.TryParseExact(raw.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            return null;
        }

        private static string? NullFilterValue(string? raw)
        {
            var val = raw?.Trim().ToLower();
            return val is "null" or "notnull" ? val : null;
        }

        private static string? TextValue(string? raw)
        {
            var val = raw?.Trim();
            return string.IsNullOrEmpty(val) ? null : val;
        }
    }
}