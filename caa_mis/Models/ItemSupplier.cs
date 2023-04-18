namespace caa_mis.Models
{
    public class ItemSupplier
    {
        public int SupplierID { get; set; }
        public Supplier Supplier { get; set; }

        public int ItemID { get; set; }
        public Item Item { get; set; }

    }
}
