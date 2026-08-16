using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces
{
    public interface ICicloEscolarService
    {
        Task<IEnumerable<CicloEscolarResponseDto>> GetAll();

        Task<CicloEscolarResponseDto> GetById(Guid publicId);

        Task<CicloEscolarResponseDto> Create(CicloEscolarRequestDto request, long actorId);

        Task<CicloEscolarResponseDto> Update(Guid publicId, CicloEscolarRequestDto request, long actorId);

        Task<bool> Delete(Guid publicId, long actorId);
    }
}
