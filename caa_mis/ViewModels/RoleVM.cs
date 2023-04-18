using caa_mis.Models;

namespace caa_mis.ViewModels
{
    public class RoleVM
    {
        public string RoleId { get; set; }

        public string RoleName { get; set; }

        public bool Assigned { get; set; }

        public Archived Status { get; set; }
    }
}
