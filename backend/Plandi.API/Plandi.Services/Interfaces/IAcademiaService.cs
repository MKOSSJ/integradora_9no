using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces
{
    public interface IAcademiaService
    {
        Task<IEnumerable<AcademiaResponseDto>> GetAll();

        Task<AcademiaResponseDto> GetById(Guid publicId);

        Task<AcademiaResponseDto> Create(AcademiaRequestDto request, long actorId);

        Task<AcademiaResponseDto> Update(Guid publicId, AcademiaRequestDto request, long actorId);

        Task<bool> Delete(Guid publicId, long actorId);

        Task<IEnumerable<AcademiaUsuarioResponseDto>> GetUsuarios(Guid academiaPublicId);

        Task<AcademiaUsuarioResponseDto> AsignarUsuario(Guid academiaPublicId, AcademiaUsuarioRequestDto request);

        Task<bool> DesasignarUsuario(Guid academiaPublicId, Guid usuarioPublicId);
    }
}
