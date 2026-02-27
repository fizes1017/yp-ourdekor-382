using ourdekor.Models;

namespace ourdekor.Models
{
    public class Products
    {
        public int id {  get; set; }
        public int ProductTypeId { get; set; }
        public string? name { get; set; }
        public string? article { get; set; }
        public decimal min_price { get; set; }
        public decimal width { get; set; }
        
        public virtual ProductType? ProductType { get; set; }
        public virtual ICollection<ProductMaterials>? ProductMaterials { get; set; }
    }
}