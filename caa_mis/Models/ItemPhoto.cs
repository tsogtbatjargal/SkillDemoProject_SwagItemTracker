using System.ComponentModel.DataAnnotations;

namespace caa_mis.Models
{
    public class ItemPhoto
    {
        public int ID { get; set; }
        [ScaffoldColumn(false)]
        public byte[] Content { get; set; }
        [StringLength(255)]
        public string MimeType { get; set; }
        public int ItemID { get; set; }
        public Item Item { get; set; }
    }
}
