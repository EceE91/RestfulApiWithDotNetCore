﻿using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;

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


        [HttpGet("{id}",Name ="GetAuthor")]
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
               // return StatusCode(500, "An unexpected error happened. Try again later");
          //  }
        }


        // create author POST request
        [HttpPost()]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
             if(author == null)
            {
                return BadRequest(); // return 400 error
            }

            var authorEntity = Mapper.Map<Author>(author);

            // the entity has not been added to the database yet, it has been added to the dbcontext
            // which represents a session with the database.
            _libraryRepository.AddAuthor(authorEntity);
            // To persist the changes we have to save on the repo.
            // if it fails we will return 500 error

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save");
               // return StatusCode(500, "An error occured");
            }

            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id}, authorToReturn);
        }

        //[HttpGet("{id}")]
        //public IActionResult BlockAuthorCreation(Guid id)
        //{
        //    if (_libraryRepository.AuthorExists(id))
        //    {
        //        return new StatusCodeResult(StatusCodes.Status409Conflict);
        //    }

        //    return NotFound();
        //}


        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
                return NotFound();

            _libraryRepository.DeleteAuthor(authorFromRepo);

            if (!_libraryRepository.Save())
                throw new Exception($"Author with author id {id} cannot be deleted");

            return NoContent();
        }
    }
}
