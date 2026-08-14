using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Plandi.Dto;
using Plandi.Dto.Common;

namespace Plandi.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<PagedResult<UsuarioResponseDto>> GetAllUsers(int page, int pageSize);
    }
}
