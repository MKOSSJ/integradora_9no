using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces
{
    public interface ICargaAcademicaService
    {
        Task<IEnumerable<CargaAcademicaResponseDto>> GetAll();

        Task<CargaAcademicaResponseDto> GetById(Guid publicId);

        Task<CargaAcademicaResponseDto> Create(CargaAcademicaRequestDto request, long actorId);

        Task<CargaAcademicaResponseDto> Update(Guid publicId, CargaAcademicaRequestDto request, long actorId);

        Task<bool> Delete(Guid publicId, long actorId);

        Task<CargaAcademicaResponseDto> UpdateGrupo(Guid publicId, ActualizarGrupoCargaAcademicaDto request, long actorId, CancellationToken cancellationToken = default);
    }
}
