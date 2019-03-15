using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Helpers;
using AutoMapper;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        // yukarıya route ekleyerek bütün http methodlarının api/authors ile başlayacağını söylemiş olduk. HttpGet'e de birşey yazmayarak 
        // direk route'tan gelen api/authors ile erişilebileceğini öylemiş olduk
        [HttpGet()]
        public IActionResult GetAuthors()
        {
            var authorsFromRepo = _libraryRepository.GetAuthors();

            //var authors = new List<AuthorDto>();
            //foreach (var author in authorsFromRepo)
            //{
            //    authors.Add(new AuthorDto() {
            //        Id = author.Id,
            //        Name = $"{author.FirstName} {author.LastName}",
            //        Genre = author.Genre,
            //        Age = author.DateOfBirth.GetCurrentAge()
            //    });
            //}

            // use automapper instead
            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            //return new JsonResult(authors);
            return Ok(authors); // return 200 if successful
        }


        [HttpGet("{id}")]
        public ActionResult GetAuthor(Guid id)
        {
          // since we added a exception handler middleware we do not need try-catch anymore
          //  try
          //  {
                var authorFromRepo = _libraryRepository.GetAuthor(id);

                if (authorFromRepo == null)
                    return NotFound();

                var author = Mapper.Map<AuthorDto>(authorFromRepo);
                return Ok(author);
          //  }catch(Exception e)
          //  {
                return StatusCode(500, "An unexpected error happened. Try again later");
          //  }
        }
    }
}
