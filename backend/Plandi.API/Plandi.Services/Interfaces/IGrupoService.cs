using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces
{
    public interface IGrupoService
    {
        Task<IEnumerable<GrupoResponseDto>> GetAll();

        Task<GrupoResponseDto> GetById(Guid publicId);

        Task<GrupoResponseDto> Create(GrupoRequestDto request);

        Task<GrupoResponseDto> Update(Guid publicId, GrupoRequestDto request);

        Task<bool> Delete(Guid publicId);
    }
}
