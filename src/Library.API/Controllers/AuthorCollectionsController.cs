using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authorCollections")]
    public class AuthorCollectionsController : Controller
    {
        private ILibraryRepository _libraryRepository { get; set; }

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        // we get AuthorForCreationDto as type IEnumerable (list)
        // because this time we want to create a list of authors
        [HttpPost]
        public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
        {
            if(authorCollection == null)
            {
                return BadRequest();
            }

            var authorEntities = Mapper.Map<IEnumerable<Author>>(authorCollection);

            foreach (var item in authorEntities)
            {
                _libraryRepository.AddAuthor(item);
            }

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating author collection is failed"); 
            }

            return Ok();
        }
    }
}
