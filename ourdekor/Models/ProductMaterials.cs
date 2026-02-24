namespace ourdekor.Models
{
    public class ProductMaterials
    {
        public int id { get; set; }
        public int ProductId { get; set; }
        public int MaterialId { get; set; }
        public decimal count { get; set; }

        public virtual Products Products { get; set; }
        public virtual Materials Materials { get; set; }
    }
}
