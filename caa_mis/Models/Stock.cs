using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace caa_mis.Models
{
    public class Stock
    {
        public int ID { get; set; }

        [Required]
        public int BranchID { get; set; }

        public Branch Branch { get; set; }

        [Required]
        public int? ItemID { get; set; }
        [JsonIgnore]
        public Item Item { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MinLevel { get; set; }
        public ICollection<TransactionItem> TransactionItems { get; set; } = new HashSet<TransactionItem>();
    }
}
