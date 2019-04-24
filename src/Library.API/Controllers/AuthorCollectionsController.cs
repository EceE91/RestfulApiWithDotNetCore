using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

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

        //// we get AuthorForCreationDto as type IEnumerable (list)
        //// because this time we want to create a list of authors
        //[HttpPost()]
        //public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
        //{
        //    if(authorCollection == null)
        //    {
        //        return BadRequest();
        //    }

        //    var authorEntities = Mapper.Map<IEnumerable<Author>>(authorCollection);

        //    foreach (var item in authorEntities)
        //    {
        //        _libraryRepository.AddAuthor(item);
        //    }

        //    if (!_libraryRepository.Save())
        //    {
        //        throw new Exception("Creating author collection is failed"); 
        //    }

        //    var authorCollectionToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
        //    var idsAsString = string.Join(",", authorCollectionToReturn.Select(x => x.Id));            
        //    return CreatedAtRoute("GetAuthorCollection", new { ids = idsAsString}, authorCollectionToReturn);
        //    //return Ok();
        //}

        //[HttpGet("({ids})", Name = "GetAuthorCollection")]
        //public IActionResult GetAuthorCollection([ModelBinder(binderType:typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        //{
        //    if(ids == null)
        //    {
        //        return BadRequest();
        //    }

        //    var authorEntities = _libraryRepository.GetAuthors(ids);

        //    // check if all the authors have been found
        //    if (ids.Count() != authorEntities.Count())
        //        return NotFound();

        //    var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
        //    return Ok(authorsToReturn);
        //}

    }
}
