using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class BookForCreationDto
    {
        [Required(ErrorMessage ="Title cannot be empty")]
        [MaxLength(100)]
        // create payload
        public string Title { get; set; }
        [MaxLength(500,ErrorMessage ="Max lenght for Description cannot be bigger than 500 chars")]
        public string Description { get; set; }

        // we did not include authorid here (but we did in bookdto as it is a foreign key)
        // because we might end up posting to book's resource for author A would
        // create a book for author B
    }
}
