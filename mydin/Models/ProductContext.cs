using System.Data.Entity;
namespace mydin.Models
{
    public class ProductContext : DbContext
    {
        public ProductContext()
            : base("mydin")
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CartItem> ShoppingCartItems { get; set; }
    }
}