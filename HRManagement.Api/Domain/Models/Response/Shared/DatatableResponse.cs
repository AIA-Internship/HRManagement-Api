namespace HRManagement.Api.Domain.Models.Response.Shared
{
    public record DatatableResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<T> Data { get; set; } = new List<T>(); // Initialize with an empty list to avoid null

        public DatatableResponse() { } // Default constructor

        public DatatableResponse(List<T> data, int draw, int recordsTotal, int recordsFiltered)
        {
            Data = data ?? new List<T>(); // Ensure Data is not null
            Draw = draw < 0 ? 0 : draw; // Ensure Draw is non-negative
            RecordsTotal = recordsTotal < 0 ? 0 : recordsTotal; // Ensure RecordsTotal is non-negative
            RecordsFiltered = recordsFiltered < 0 ? 0 : recordsFiltered; // Ensure RecordsFiltered is non-negative
        }
    }
}
