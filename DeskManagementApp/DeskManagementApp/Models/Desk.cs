using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DeskManagementApp.Models
{
    public class Desk
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Floor { get; set; }
        public IdentityUser? User { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set;}
    }
}
