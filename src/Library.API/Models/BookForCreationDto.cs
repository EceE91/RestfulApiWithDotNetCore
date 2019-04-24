namespace Library.API.Models
{
    public class BookForCreationDto : BookForManipulationDto
    {       
        // we did not include authorid here (but we did in bookdto as it is a foreign key)
        // because we might end up posting to book's resource for author A would
        // create a book for author B
    }
}
