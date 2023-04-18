using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace caa_mis.Models
{
    public class Item
    {
        public int ID { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = "You cannot leave the category blank.")]        
        public int CategoryID { get; set; }
        [JsonIgnore]
        public Category Category { get; set; }

        [Display(Name = "SKU Number")]
        [Required(ErrorMessage = "You cannot leave the SKU blank.")]
        [StringLength(255)]
        public string SKUNumber { get; set; }

        [Required(ErrorMessage = "You cannot leave the name blank.")]
        [StringLength(255)]
        public string Name { get; set; }


        [Required(ErrorMessage = "You cannot leave the description blank.")]
        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(255)]
        public string Scale { get; set; }

        [Required(ErrorMessage = "You cannot leave the cost blank.")]
        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }

        [Display(Name = "Manufacturer")]
        public int? ManufacturerID { get; set; }
        public Manufacturer Manufacturer { get; set; }

        [Display(Name = "Status")]
        [Required]
        public int ItemStatusID { get; set; }
        [Display(Name = "Minimum Stock Level")]
        [Required]
        public int MinLevel { get; set; }
        [JsonIgnore]
        public ItemStatus ItemStatus { get; set; }        

        public ICollection<BulkItem> BulkItems { get; set; } = new HashSet<BulkItem>();
        public ICollection<EventItem> EventItems { get; set; } = new HashSet<EventItem>();
        public ICollection<Stock> Stocks { get; set; } = new HashSet<Stock>();
        public ICollection<ItemSupplier> ItemSuppliers { get; set; } = new HashSet<ItemSupplier>();
        public ICollection<TransactionItem> TransactionItems { get; set; } = new HashSet<TransactionItem>();
        public ItemPhoto ItemPhoto { get; set; }
        [JsonIgnore]
        public ItemThumbnail ItemThumbnail { get; set; }
    }

}
