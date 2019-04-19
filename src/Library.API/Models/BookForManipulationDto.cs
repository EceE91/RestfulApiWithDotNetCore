using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{

    // I have defined this class as abstract, because I do not want that class to be
    // used on its own. It must be derived from.
    // An abstract class is intended only to be a base class of other classes
    public abstract class BookForManipulationDto
    {
        // create payload
        [Required(ErrorMessage = "Title cannot be empty")]
        [MaxLength(100)]

        // by making description property virtual, we let it to be overriden in subclasses
        // but it doesn't have to be overriden
        // I did this virtual because I want description to be required in BookForUpdateDto,
        // but I don't want it to be required in BookForCreationDto
        public string Title { get; set; }
        [MaxLength(500, ErrorMessage = "Max length for Description cannot be bigger than 500 chars")]
        public virtual string Description { get; set; }
    }
}
