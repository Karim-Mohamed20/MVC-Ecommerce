using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
    }
}
