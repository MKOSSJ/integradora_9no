using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces
{
    public interface IPeriodoService
    {
        Task<IEnumerable<PeriodoResponseDto>> GetAll();

        Task<PeriodoResponseDto> GetById(Guid publicId);

        Task<PeriodoResponseDto> Create(PeriodoRequestDto request);

        Task<PeriodoResponseDto> Update(Guid publicId, PeriodoRequestDto request);

        Task<bool> Delete(Guid publicId);
    }
}
