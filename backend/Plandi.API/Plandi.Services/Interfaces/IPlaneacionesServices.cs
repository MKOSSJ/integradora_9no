using Plandi.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plandi.Services.Interfaces
{
    public interface IPlaneacionCaratulaService
    {
        Task<PlaneacionCaratulaDto?> GetByPlaneacionIdAsync(Guid planeacionPublicId);
        Task<PlaneacionCaratulaDto> CreateAsync(Guid planeacionPublicId, CreatePlaneacionCaratulaDto dto);
        Task<PlaneacionCaratulaDto> UpdateAsync(Guid publicId, UpdatePlaneacionCaratulaDto dto);
        Task DeleteAsync(Guid publicId);
    }

    public interface IPlaneacionTemaService
    {
        Task<PlaneacionTemaDto> GetByIdAsync(Guid publicId);
        Task<List<PlaneacionTemaDto>> GetByUnidadIdAsync(Guid unidadPublicId);
        Task<PlaneacionTemaDto> CreateAsync(Guid unidadPublicId, CreatePlaneacionTemaDtos dto);
        Task<PlaneacionTemaDto> UpdateAsync(Guid publicId, UpdatePlaneacionTemaDtos dto);
        Task DeleteAsync(Guid publicId);
    }

    public interface IPlaneacionEvaluacionService
    {
        Task<PlaneacionEvaluacionDto> GetByIdAsync(Guid publicId);
        Task<List<PlaneacionEvaluacionDto>> GetByUnidadIdAsync(Guid unidadPublicId);
        Task<PlaneacionEvaluacionDto> CreateAsync(Guid unidadPublicId, CreatePlaneacionEvaluacionDto dto);
        Task<PlaneacionEvaluacionDto> UpdateAsync(Guid publicId, UpdatePlaneacionEvaluacionDto dto);
        Task DeleteAsync(Guid publicId);
    }

    public interface IPlaneacionSecuenciaService
    {
        Task<PlaneacionSecuenciaDto> GetByIdAsync(Guid publicId);
        Task<List<PlaneacionSecuenciaDto>> GetByUnidadIdAsync(Guid unidadPublicId);
        Task<PlaneacionSecuenciaDto> CreateAsync(Guid unidadPublicId, CreatePlaneacionSecuenciaDto dto);
        Task<PlaneacionSecuenciaDto> UpdateAsync(Guid publicId, UpdatePlaneacionSecuenciaDto dto);
        Task DeleteAsync(Guid publicId);
    }

    public interface IPlaneacionReferenciaService
    {
        Task<PlaneacionReferenciaDto> GetByIdAsync(Guid publicId);
        Task<List<PlaneacionReferenciaDto>> GetByPlaneacionIdAsync(Guid planeacionPublicId);
        Task<PlaneacionReferenciaDto> CreateAsync(Guid planeacionPublicId, CreatePlaneacionReferenciaDto dto);
        Task<PlaneacionReferenciaDto> UpdateAsync(Guid publicId, UpdatePlaneacionReferenciaDto dto);
        Task DeleteAsync(Guid publicId);
    }
}
