namespace UVM1._5.Models
{
    public class ListVM
    {

        public int? Id { get; set; }
        public int? Location { get; set; }
        public string? Year { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public string? Category { get; set; }
        public decimal? Retail { get; set; }
        public decimal? Cost { get; set; }
        public byte[] DisplayIMG { get; set; }
    }
}
