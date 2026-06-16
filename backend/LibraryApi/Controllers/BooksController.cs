using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;
using LibraryApi.Services;
using LibraryApi.Services.Ai;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IAiSearchService _aiSearchService;

    public BooksController(IBookService bookService, IAiSearchService aiSearchService)
    {
        _bookService = bookService;
        _aiSearchService = aiSearchService;
    }

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

    [HttpGet("ai-search/status")]
    public IActionResult AiSearchStatus()
    {
        return Ok(new { available = _aiSearchService.IsAvailable });
    }

    [HttpPost("ai-search")]
    public async Task<ActionResult<AiSearchResult>> AiSearch([FromBody] AiSearchRequestDto dto)
    {
        if (!_aiSearchService.IsAvailable)
            return BadRequest(new { message = "AI search is not available. No AI provider is configured." });

        if (string.IsNullOrWhiteSpace(dto.Query))
            return BadRequest(new { message = "Query is required." });

        try
        {
            var result = await _aiSearchService.SearchAsync(dto.Query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "AI search failed. Falling back to standard search.", error = ex.Message });
        }
    }
}
