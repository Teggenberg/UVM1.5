namespace UVM1._5.Models
{
    public class Item
    {
        public int? Id { get; set; }
        public int? Location { get; set; }
        public string? Year { get; set; }
        public Pair? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public Pair? Category { get; set; }
        public string? Serial { get; set; }
        public Pair? Condition { get; set; }
        public decimal?  Retail {  get; set; }
        public decimal? Cost { get; set; }
        public string? Description { get; set; }
       
        public string? Details { get; set; }
        public List<byte[]> Images { get; set; }
    }


    
}
