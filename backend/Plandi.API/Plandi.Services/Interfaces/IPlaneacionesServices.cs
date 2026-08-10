using Plandi.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plandi.Services.Interfaces
{
    public interface IPlaneacionCaratulaService
    {
        Task<PlaneacionCaratulaDto?> GetByPlaneacionIdAsync(long planeacionId);
        Task<PlaneacionCaratulaDto> CreateAsync(long planeacionId, CreatePlaneacionCaratulaDto dto);
        Task<PlaneacionCaratulaDto> UpdateAsync(long id, UpdatePlaneacionCaratulaDto dto);
        Task DeleteAsync(long id);
    }

    public interface IPlaneacionTemaService
    {
        Task<PlaneacionTemaDto> GetByIdAsync(long id);
        Task<List<PlaneacionTemaDto>> GetByUnidadIdAsync(long unidadId);
        Task<PlaneacionTemaDto> CreateAsync(long unidadId, CreatePlaneacionTemaDtos dto);
        Task<PlaneacionTemaDto> UpdateAsync(long id, UpdatePlaneacionTemaDtos dto);
        Task DeleteAsync(long id);
    }

    public interface IPlaneacionEvaluacionService
    {
        Task<PlaneacionEvaluacionDto> GetByIdAsync(long id);
        Task<List<PlaneacionEvaluacionDto>> GetByUnidadIdAsync(long unidadId);
        Task<PlaneacionEvaluacionDto> CreateAsync(long unidadId, CreatePlaneacionEvaluacionDto dto);
        Task<PlaneacionEvaluacionDto> UpdateAsync(long id, UpdatePlaneacionEvaluacionDto dto);
        Task DeleteAsync(long id);
    }

    public interface IPlaneacionSecuenciaService
    {
        Task<PlaneacionSecuenciaDto> GetByIdAsync(long id);
        Task<List<PlaneacionSecuenciaDto>> GetByUnidadIdAsync(long unidadId);
        Task<PlaneacionSecuenciaDto> CreateAsync(long unidadId, CreatePlaneacionSecuenciaDto dto);
        Task<PlaneacionSecuenciaDto> UpdateAsync(long id, UpdatePlaneacionSecuenciaDto dto);
        Task DeleteAsync(long id);
    }

    public interface IPlaneacionReferenciaService
    {
        Task<PlaneacionReferenciaDto> GetByIdAsync(long id);
        Task<List<PlaneacionReferenciaDto>> GetByPlaneacionIdAsync(long planeacionId);
        Task<PlaneacionReferenciaDto> CreateAsync(long planeacionId, CreatePlaneacionReferenciaDto dto);
        Task<PlaneacionReferenciaDto> UpdateAsync(long id, UpdatePlaneacionReferenciaDto dto);
        Task DeleteAsync(long id);
    }
}
