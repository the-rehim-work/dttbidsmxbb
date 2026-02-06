using Microsoft.AspNetCore.Http;

namespace dttbidsmxbb.Models.DTOs
{
    public class DataTableRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string SearchValue { get; set; } = string.Empty;
        public int SortColumnIndex { get; set; }
        public string SortDirection { get; set; } = "asc";

        public static DataTableRequest Parse(IFormCollection form)
        {
            return new DataTableRequest
            {
                Draw = int.TryParse(form["draw"], out var d) ? d : 1,
                Start = int.TryParse(form["start"], out var s) ? s : 0,
                Length = int.TryParse(form["length"], out var l) ? l : 10,
                SearchValue = form["search[value]"].ToString().Trim(),
                SortColumnIndex = int.TryParse(form["order[0][column]"], out var sc) ? sc : 0,
                SortDirection = form["order[0][dir]"].ToString().Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? "desc" : "asc"
            };
        }
    }
}