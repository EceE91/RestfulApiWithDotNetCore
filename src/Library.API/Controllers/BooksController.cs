﻿using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController :Controller
    {
        // constructor injection
        private ILibraryRepository _libraryRepository;
        private ILogger<BooksController> _logger;
        private IUrlHelper _urlHelper;

        public BooksController(ILibraryRepository libraryRepository,
            ILogger<BooksController> logger, IUrlHelper urlHelper)

        {
            _logger = logger;
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper; // we added this to create links in the url
        }

        [HttpGet(Name = "GetBooksForAuthor")]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);

            // run through the book list and create link for each of them 
            booksForAuthor = booksForAuthor.Select(book =>
            {
                book = CreateLinksForBook(book);
                return book;
            });

            var wrapper = new LinkedCollectionResourceWrapperDto<BookDto>(booksForAuthor);

            return Ok(CreateLinksForBooks(wrapper));
        }

        // get single book from the repo
        [HttpGet("{id}", Name ="GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId,id);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var booksForAuthor = Mapper.Map<BookDto>(bookForAuthorFromRepo);
            return Ok(CreateLinksForBook(booksForAuthor));
        }



        [HttpPost(Name = "CreateBookForAuthor")]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if( book == null)
            {
                return BadRequest(); // 400 - consumer made a mistake
            }

            if(book.Title == book.Description)
            {
                ModelState.AddModelError(nameof(BookForCreationDto), "Title and description cannot be the same");
            }

            if (!ModelState.IsValid) // rules comes from data annotations in dto class
            {
                return new UnprocessableEntityObjectResult(ModelState);
                //422 unprocessable entity error
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Book>(book);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"An error occured while creating book with AuthorId {authorId}");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor", 
                new {authorId, id = bookToReturn.Id },
                CreateLinksForBook(bookToReturn));
        }


        [HttpDelete("{id}", Name = "DeleteBookForAuthor")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
                return NotFound();

            _libraryRepository.DeleteBook(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
                throw new Exception($"Error deleting the book with author id {authorId}");

            _logger.LogInformation(100,$"Book {id} for author {authorId} was deleted.");

            return NoContent(); // 204 delete successful but no content has retured
        }

        [HttpPut("{id}",Name = "UpdateBookForAuthor")] // put is idempotent
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book )
        {
            if (book == null)
                return BadRequest();

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                //return NotFound();
                // upserting (insert or update), if the bookid doesn't exist then create it
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting fails for bookid {id}");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor", new {authorId, id = bookToReturn.Id}, bookToReturn);
            }

            // map

            // apply update

            // map back to entity 
            Mapper.Map(book, bookForAuthorFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);
            if (!_libraryRepository.Save())
                throw new Exception($"Error updating book with bookid {id}");

            return NoContent();
        }

        [HttpPatch("{id}",Name = "PartiallyUpdateBookForAuthor")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id, 
            [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            // we use BookForUpdateDto as parameter because it contains ID, and ID can can be updated by mistake
            if (patchDoc == null)
                return BadRequest();

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();


            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id); // return bookentity

            if (bookForAuthorFromRepo == null)
            {
                // upserting
                var bookDto = new BookForUpdateDto();
                patchDoc.ApplyTo(bookDto,ModelState);

                if (bookDto.Title == bookDto.Description)
                    ModelState.AddModelError(nameof(BookForUpdateDto), "Description and title cannot be the same");

                TryValidateModel(bookDto);

                if (!ModelState.IsValid)
                    return new UnprocessableEntityObjectResult(ModelState); // 422 unprocessable error

                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"An error occured while creating book with AuthorId {authorId}");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor",new {authorId, id = bookToReturn.Id}, bookToReturn);
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

            //patchDoc.ApplyTo(bookToPatch, ModelState);

            // no need to add modelstate, since we added logger in configure method in startup.cs
            patchDoc.ApplyTo(bookToPatch);

            if (bookToPatch.Title == bookToPatch.Description)
                ModelState.AddModelError(nameof(BookForUpdateDto),"Description and title cannot be the same");

            TryValidateModel(bookToPatch);

            if(!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState); // 422 unprocessable error

            // add validation

            Mapper.Map(bookToPatch, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);
            if (!_libraryRepository.Save())
                throw new Exception($"Error patching book with bookid {id}");

            // content-type is application-jsonpatch on postman
            return NoContent();
            //  {
            //     "op": "replace",
            //     "path": "/title", => replace only title
            //     "value": "A Game of Thrones - Updated"
            //}
        }

        private BookDto CreateLinksForBook(BookDto book)
        {
            book.Links.Add(new LinkDto( _urlHelper.Link(
                    "GetBookForAuthor", new {id = book.Id}),
                    "self",
                    "GET"));

            book.Links.Add(new LinkDto(_urlHelper.Link(
                    "DeleteBookForAuthor", new { id = book.Id }),
                "delete_book",
                "DELETE"));

            book.Links.Add(new LinkDto(_urlHelper.Link(
                    "UpdateBookForAuthor", new { id = book.Id }),
                "update_book",
                "PUT"));

            book.Links.Add(new LinkDto(_urlHelper.Link(
                    "PartiallyUpdateBookForAuthor", new { id = book.Id }),
                "partially_update_book",
                "PATCH"));

            return book;
        }

        private LinkedCollectionResourceWrapperDto<BookDto> CreateLinksForBooks
            (LinkedCollectionResourceWrapperDto<BookDto> booksWrapper)
        {
            // link to self

            booksWrapper.Links.Add(new LinkDto(_urlHelper.Link("GetBooksForAuthor",new {}),
                "self",
                "GET"));

            return booksWrapper;
        }

    }
}
