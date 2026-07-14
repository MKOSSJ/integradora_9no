using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Plandi.Library.Models;
using Plandi.Services.Interfaces;

namespace Plandi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;
        private readonly IUsuarioService _usuarioService;
        private readonly AppDbContext _dBContext;

        public UsuarioController(AppDbContext dBContext, IUsuarioService usuarioService, ILogger<UsuarioController> logger)
        {
  
            _usuarioService = usuarioService;
            _dBContext = dBContext;
            _logger = logger;

        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _usuarioService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll.ApiError");
                return StatusCode(500, new { message = "$_Excepcion_Ocurrida" });
            }
        }

    }
}
