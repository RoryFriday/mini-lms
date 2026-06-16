using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService) => _bookService = bookService;

    [HttpGet]
    public async Task<ActionResult<PagedResult<BookDto>>> Search(
        [FromQuery] string? query,
        [FromQuery] string? genre,
        [FromQuery] bool? available,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _bookService.SearchAsync(new BookSearchDto(query, genre, available, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        return book == null ? NotFound() : Ok(book);
    }

    [Authorize(Roles = "Librarian,Admin")]
    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(CreateBookDto dto)
    {
        var book = await _bookService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [Authorize(Roles = "Librarian,Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<BookDto>> Update(int id, UpdateBookDto dto)
    {
        var book = await _bookService.UpdateAsync(id, dto);
        return book == null ? NotFound() : Ok(book);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _bookService.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
