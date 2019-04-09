using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class BookForCreationDto
    {
        // create payload
        public string Title { get; set; }
        public string Description { get; set; }

        // we did not include authorid here (but we did in bookdto as it is a foreign key)
        // because we might end up posting to book's resource for author A would
        // create a book for author B
    }
}
