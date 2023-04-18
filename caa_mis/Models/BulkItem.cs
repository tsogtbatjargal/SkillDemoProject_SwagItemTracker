using System.ComponentModel.DataAnnotations;
namespace caa_mis.Models
{
    public class BulkItem
    {
        public int ID { get; set; }

        [Required]
        public int ItemID { get; set; }
        public Item Item { get; set; }


        [Required]
        public int BulkID { get; set; }
        public Bulk Bulk { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
