using System.ComponentModel.DataAnnotations;

namespace caa_mis.Models
{
    public class EventItem
    {
        public int ID { get; set; }

        [Required]
        public int ItemID { get; set; }
        public Item Item { get; set; }


        [Required]
        public int EventID { get; set; }
        public Event Event { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int? ReturnedQuantity { get; set; }
    }
}
