using AutoMapper;
using Plandi.Dto;
using Plandi.Library.Models;

namespace Plandi.Services
{
    public class PlaneacionesProfile : Profile
    {
        public PlaneacionesProfile()
        {
            // PlaneacionDidactica Mappings
            CreateMap<PlaneacionDidactica, PlaneacionDidacticaDto>()
                .ForMember(dest => dest.Caratula, opt => opt.MapFrom(src => src.Caratula))
                .ForMember(dest => dest.Unidades, opt => opt.MapFrom(src => src.Unidades))
                .ForMember(dest => dest.Referencias, opt => opt.MapFrom(src => src.Referencias))
                .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones));

            CreateMap<CreatePlaneacionDidacticaDto, PlaneacionDidactica>();
            CreateMap<UpdatePlaneacionDidacticaDto, PlaneacionDidactica>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<PlaneacionDidactica, PlaneacionDidacticaSimpleDto>();

            // PlaneacionCaratula Mappings
            CreateMap<PlaneacionCaratula, PlaneacionCaratulaDto>();
            CreateMap<CreatePlaneacionCaratulaDto, PlaneacionCaratula>();
            CreateMap<UpdatePlaneacionCaratulaDto, PlaneacionCaratula>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // PlaneacionUnidad Mappings
            CreateMap<PlaneacionUnidad, PlaneacionUnidadDto>()
                .ForMember(dest => dest.Temas, opt => opt.MapFrom(src => src.Temas))
                .ForMember(dest => dest.Evaluaciones, opt => opt.MapFrom(src => src.Evaluaciones))
                .ForMember(dest => dest.Secuencias, opt => opt.MapFrom(src => src.Secuencias));

            CreateMap<CreatePlaneacionUnidadDto, PlaneacionUnidad>();
            CreateMap<UpdatePlaneacionUnidadDto, PlaneacionUnidad>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<PlaneacionUnidad, PlaneacionUnidadSimpleDto>();

            // PlaneacionTema Mappings
            CreateMap<PlaneacionTema, PlaneacionTemaDto>();
            CreateMap<CreatePlaneacionTemaDtos, PlaneacionTema>();
            CreateMap<UpdatePlaneacionTemaDtos, PlaneacionTema>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // PlaneacionEvaluacion Mappings
            CreateMap<PlaneacionEvaluacion, PlaneacionEvaluacionDto>();
            CreateMap<CreatePlaneacionEvaluacionDto, PlaneacionEvaluacion>();
            CreateMap<UpdatePlaneacionEvaluacionDto, PlaneacionEvaluacion>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // PlaneacionSecuencia Mappings
            CreateMap<PlaneacionSecuencia, PlaneacionSecuenciaDto>();
            CreateMap<CreatePlaneacionSecuenciaDto, PlaneacionSecuencia>();
            CreateMap<UpdatePlaneacionSecuenciaDto, PlaneacionSecuencia>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // PlaneacionReferencia Mappings
            CreateMap<PlaneacionReferencia, PlaneacionReferenciaDto>();
            CreateMap<CreatePlaneacionReferenciaDto, PlaneacionReferencia>();
            CreateMap<UpdatePlaneacionReferenciaDto, PlaneacionReferencia>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // PlaneacionObservacion Mappings
            CreateMap<PlaneacionObservacion, PlaneacionObservacionDto>();
            CreateMap<CreatePlaneacionObservacionDto, PlaneacionObservacion>();
            CreateMap<UpdatePlaneacionObservacionDto, PlaneacionObservacion>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
