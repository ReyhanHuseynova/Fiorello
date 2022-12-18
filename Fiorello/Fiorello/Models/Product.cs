using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fiorello.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }
        public bool IsDeactive { get; set; }    

        public int CategoryId { get; set; }
        public Category Category { get; set; }
        [NotMapped]
        public IFormFile Photo { get; set; }
    }
}
