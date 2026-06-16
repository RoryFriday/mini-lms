using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutsController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutsController(ICheckoutService checkoutService) => _checkoutService = checkoutService;

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<CheckoutRecordDto>> Checkout(CheckoutDto dto)
    {
        try
        {
            var result = await _checkoutService.CheckoutBookAsync(GetUserId(), dto.BookId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/return")]
    public async Task<ActionResult<CheckoutRecordDto>> Return(int id)
    {
        try
        {
            var result = await _checkoutService.ReturnBookAsync(GetUserId(), id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<CheckoutRecordDto>>> GetMyCheckouts(
        [FromQuery] bool activeOnly = false)
    {
        var result = await _checkoutService.GetUserCheckoutsAsync(GetUserId(), activeOnly);
        return Ok(result);
    }

    [Authorize(Roles = "Librarian,Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CheckoutRecordDto>>> GetAllCheckouts(
        [FromQuery] bool activeOnly = false)
    {
        var result = await _checkoutService.GetAllCheckoutsAsync(activeOnly);
        return Ok(result);
    }

    [Authorize(Roles = "Librarian,Admin")]
    [HttpGet("book/{bookId}")]
    public async Task<ActionResult<IEnumerable<CheckoutRecordDto>>> GetBookCheckouts(int bookId)
    {
        var result = await _checkoutService.GetBookCheckoutsAsync(bookId);
        return Ok(result);
    }
}
