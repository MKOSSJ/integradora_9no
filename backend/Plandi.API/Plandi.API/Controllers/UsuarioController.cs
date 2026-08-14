using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandi.Dto;
using Plandi.Dto.Common;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Director")]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService, ILogger<UsuarioController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize is < 1 or > 100)
                return BadRequest(ApiResponse<PagedResult<UsuarioResponseDto>>.Fail("Page debe ser mayor a cero y PageSize debe estar entre 1 y 100."));
            try
            {
                var users = await _usuarioService.GetAllUsers(page, pageSize);
                return Ok(ApiResponse<PagedResult<UsuarioResponseDto>>.Ok(users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll.ApiError");
                return StatusCode(500, ApiResponse<PagedResult<UsuarioResponseDto>>.Fail("Ocurrió un error interno al obtener los usuarios."));
            }
        }
    }
}
