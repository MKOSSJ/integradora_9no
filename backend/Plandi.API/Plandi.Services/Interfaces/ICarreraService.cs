using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plandi.Dto.Catalogos;

namespace Plandi.Services.Interfaces
{
    public interface ICarreraService
    {
        Task<IEnumerable<CarreraResponseDto>> GetAll();

        Task<CarreraResponseDto> GetById(Guid publicId);

        Task<CarreraResponseDto> Create(CarreraRequestDto request, long actorId);

        Task<CarreraResponseDto> Update(Guid publicId, CarreraRequestDto request, long actorId);

        Task<bool> Delete(Guid publicId, long actorId);
    }
}
