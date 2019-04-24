using System;

namespace Library.API.Models
{
    public class AuthorDto
    {
        // create payload
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Genre { get; set; }
    }
}
