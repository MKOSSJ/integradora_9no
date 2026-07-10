using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Plandi.Dto;

namespace Plandi.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioResponseDto>> GetAllUsers();
    }
}
