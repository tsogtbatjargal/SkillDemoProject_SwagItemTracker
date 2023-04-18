using caa_mis.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace caa_mis.ViewModels
{
    public class UserVM
    {
        public string Id { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Roles")]
        public List<string> UserRoles { get; set; }

        public bool Assigned { get; set; }

        public Archived Status { get; set; }
    }

}
