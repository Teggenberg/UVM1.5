namespace UVM1._5.Models
{
    public class Item
    {
        public int? Id { get; set; }
        public int? Location { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Category { get; set; }
        public string? Serial { get; set; }
        public int? Condition { get; set; }
        public float?  Retail {  get; set; }
        public float? Cost { get; set; }
        public string? Description { get; set; }
        public List<string>? Details { get; set; }

    }


    
}
