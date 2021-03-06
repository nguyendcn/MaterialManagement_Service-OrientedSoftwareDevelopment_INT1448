﻿using AutoMapper;
using INT1448.Application.Infrastructure.Core;
using INT1448.Application.Infrastructure.DTOs;
using INT1448.Application.Infrastructure.ViewModels;
using INT1448.Application.IServices;
using INT1448.Shared.CommonType;
using INT1448.Shared.Filters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Linq;
using INT1448.Application.Infrastructure.RequestTypes;

namespace INT1448.WebApi.Controllers
{
    [RoutePrefix("api/books")]
    public class BookController : ApiControllerBase
    {
        private IBookService _bookService;
        private IBookImageManagerService _bookImageManagerService;
        private IBookAuthorService _bookAuthorService;
        private IMapper _mapper;

        public BookController(IBookService bookService,
            IBookImageManagerService bookImageManagerService,
            IBookAuthorService bookAuthorService,
            IMapper mapper)
        {
            this._bookService = bookService;
            this._bookImageManagerService = bookImageManagerService;
            this._bookAuthorService = bookAuthorService;
            this._mapper = mapper;
        }

        [Route("getall")]
        [HttpGet]
        [ValidateModelAttribute]
        public async Task<HttpResponseMessage> GetAll()
        {
            HttpRequestMessage requestMessage = this.Request;
            Func<Task<HttpResponseMessage>> HandleRequest = async () =>
            {
                HttpResponseMessage response = null;

                IEnumerable<BookDTO> bookDtos = await _bookService.GetAll();

                response = requestMessage.CreateResponse(HttpStatusCode.OK, bookDtos, JsonMediaTypeFormatter.DefaultMediaType);

                return response;
            };

            return await CreateHttpResponseAsync(requestMessage, HandleRequest);
        }

        [Route("getalltoview")]
        [HttpGet]
        [ValidateModelAttribute]
        public async Task<HttpResponseMessage> GetAllToView()
        {
            HttpRequestMessage requestMessage = this.Request;
            
            Func<Task<HttpResponseMessage>> HandleRequest = async () =>
            {
                HttpResponseMessage response = null;

                IEnumerable<BookViewModel> bookDtos = await _bookService.GetAllToView();

                response = requestMessage.CreateResponse(HttpStatusCode.OK, bookDtos, JsonMediaTypeFormatter.DefaultMediaType);

                return response;
            };

            return await CreateHttpResponseAsync(requestMessage, HandleRequest);
        }

        [Route("getbyid/{id:int}")]
        [HttpGet]
        [ValidateModelAttribute]
        [IDFilterAttribute]
        public async Task<HttpResponseMessage> GetById(int id)
        {
            HttpRequestMessage request = this.Request;
            Func<Task<HttpResponseMessage>> HandleRequest = async () =>
            {
                HttpResponseMessage response = null;
                BookDTO bookDto = null;

                bookDto = await _bookService.GetById(id);

                if (bookDto == null)
                {
                    var message = new NotificationResponse("true", "Not found.");
                    response = request.CreateResponse(HttpStatusCode.NotFound, message, JsonMediaTypeFormatter.DefaultMediaType);
                    return response;
                }
                response = request.CreateResponse(HttpStatusCode.OK, bookDto);
                return response;
            };

            return await CreateHttpResponseAsync(request, HandleRequest);
        }

        [Route("getbyidtoview/{id:int}")]
        [HttpGet]
        [ValidateModelAttribute]
        [IDFilterAttribute]
        public async Task<HttpResponseMessage> GetByIdToView(int id)
        {
            HttpRequestMessage request = this.Request;

            Func<Task<HttpResponseMessage>> HandleRequest = async () =>
            {
                HttpResponseMessage response = null;
                BookViewModel bookVM = null;

                bookVM = await _bookService.GetByIdToView(id);

                if (bookVM == null)
                {
                    var message = new NotificationResponse("true", "Not found.");
                    response = request.CreateResponse(HttpStatusCode.NotFound, message, JsonMediaTypeFormatter.DefaultMediaType);
                    return response;
                }
                response = request.CreateResponse(HttpStatusCode.OK, bookVM);
                return response;
            };

            return await CreateHttpResponseAsync(request, HandleRequest);
        }

        [Route("update")]
        [HttpPut]
        [ValidateModelAttribute]
        public async Task<HttpResponseMessage> Update(BookUpdateRequest book)
        {
            HttpRequestMessage request = this.Request;
            Func<Task<HttpResponseMessage>> HandleRequest = async () =>
            {
                HttpResponseMessage response = null;

                BookDTO bookToUpdate = _mapper.Map<BookDTO>(book);

                await _bookService.Update(bookToUpdate);
                await _bookService.SaveToDb();

                await _bookImageManagerService.Update(book.BookImages, book.Id);

                await _bookAuthorService.Update(book.BookAuthors, book.Id);

                response = this.Request.CreateResponse(HttpStatusCode.OK, new NotificationResponse("true", "Update book successed."));
                return response;
            };

            return await CreateHttpResponseAsync(request, HandleRequest);
        }

        [Route("create")]
        [HttpPost]
        [ValidateModelAttribute]
        public async Task<HttpResponseMessage> Create(BookCreateRequest bookCreate)
        {
            HttpRequestMessage request = this.Request;

            Func<Task<HttpResponseMessage>> HandleRequest = async () =>
            {
                HttpResponseMessage response = null;

                BookDTO bookToAdd = _mapper.Map<BookDTO>(bookCreate);

                BookDTO bookAdded = await _bookService.Add(bookToAdd);

                IEnumerable<string> imagePaths = bookCreate.BookImages;

                IEnumerable<int> bookAuthorIds = bookCreate.BookAuthors;

                foreach(string imagePath in imagePaths)
                {
                    await _bookImageManagerService.Add(new BookImageDTO { BookId = bookAdded.ID, ImagePath = imagePath });
                }

                foreach(int id in bookAuthorIds)
                {
                    await _bookAuthorService.Add(new BookAuthorDTO { BookID = bookAdded.ID, AuthorID = id });
                }

                response = request.CreateResponse(HttpStatusCode.OK, new NotificationResponse("true", "Book added to database."));
                return response;
            };

            return await CreateHttpResponseAsync(request, HandleRequest);
        }

        [Route("delete/{id:int}")]
        [HttpDelete]
        [ValidateModelAttribute]
        [IDFilterAttribute]
        public async Task<HttpResponseMessage> Delete(int id)
        {
            HttpRequestMessage request = this.Request;
            Func<Task<HttpResponseMessage>> HandleRequest = async () =>
            {
                HttpResponseMessage response = null;

                BookDTO bookDtoDeleted = await _bookService.Delete(id);

                await _bookImageManagerService.DeleteAllByBookId(id);

                await _bookImageManagerService.SaveToDb();
                await _bookService.SaveToDb();

                response = request.CreateResponse(HttpStatusCode.OK, new NotificationResponse("true", "Delete book successed."));
                return response;
            };

            return await CreateHttpResponseAsync(request, HandleRequest);
        }

    }
}