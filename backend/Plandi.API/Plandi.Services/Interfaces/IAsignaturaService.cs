using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces
{
    public interface IAsignaturaService
    {
        Task<IEnumerable<AsignaturaResponseDto>> GetAll();

        Task<AsignaturaResponseDto> GetById(Guid publicId);

        Task<AsignaturaResponseDto> Create(AsignaturaRequestDto request);

        Task<AsignaturaResponseDto> Update(Guid publicId, AsignaturaRequestDto request);

        Task<bool> Delete(Guid publicId);
    }
}
