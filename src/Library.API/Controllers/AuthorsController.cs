using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Remotion.Linq.Clauses;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;

        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper,IPropertyMappingService propertyMappingService)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
        }

        // yukarıya route ekleyerek bütün http methodlarının api/authors ile başlayacağını söylemiş olduk. HttpGet'e de birşey yazmayarak 
        // direk route'tan gelen api/authors ile erişilebileceğini öylemiş olduk
        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {

            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            var authorsFromRepo = _libraryRepository.GetAuthors(authorsResourceParameters);

            var previousPageLink = authorsFromRepo.HasPrevious
                ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage)
                : null;

            var nextPageLink = authorsFromRepo.HasNext
                ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage)
                : null;


            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination", 
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            return Ok(authors); // return 200 if successful
        }

        private string CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        orderBy = authorsResourceParameters.OrderBy,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        genre = authorsResourceParameters.Genre,
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize
                    });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        orderBy = authorsResourceParameters.OrderBy,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        genre = authorsResourceParameters.Genre,
                        pageNumber = authorsResourceParameters.PageNumber + 1,
                        pageSize = authorsResourceParameters.PageSize
                    });

                default:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        orderBy = authorsResourceParameters.OrderBy,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        genre = authorsResourceParameters.Genre,
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize
                    });
            }
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
