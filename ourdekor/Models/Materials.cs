namespace ourdekor.Models
{
    public class Materials
    {
        public int id {  get; set; }
        public int MaterialTypeId { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public decimal count_in_stock { get; set; }
        public decimal min_count {  get; set; }
        public decimal count_in_pack { get; set; }
        public string unit { get; set; }

        public virtual MaterialType MaterialType { get; set; }
    }
}
