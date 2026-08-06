using Microsoft.EntityFrameworkCore;
using Plandi.Dto.Enums;

namespace Plandi.Library.Models;

public static class DataSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedRoles(modelBuilder);
        SeedUsuarios(modelBuilder);
        SeedUsuarioRoles(modelBuilder);
        SeedAcademias(modelBuilder);
        SeedCarreras(modelBuilder);
        SeedCicloPeriodos(modelBuilder);
        SeedGrupos(modelBuilder);
        SeedAsignaturas(modelBuilder);
        SeedAcademiaUsuarios(modelBuilder);
        SeedCargaAcademica(modelBuilder);
        SeedDocumentos(modelBuilder);
        SeedProgramasAsignatura(modelBuilder);
        SeedPlaneaciones(modelBuilder);
        SeedPlaneacionRelaciones(modelBuilder);
        SeedPlaneacionContenido(modelBuilder);
        SeedChat(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>().HasData(
            new Rol
            {
                Id = 1,
                PublicId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Nombre = "Administrador",
                Descripcion = "Usuario con acceso total al sistema",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Rol
            {
                Id = 2,
                PublicId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Nombre = "Docente",
                Descripcion = "Profesor que participa en planeaciones didácticas",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Rol
            {
                Id = 3,
                PublicId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Nombre = "Revisor",
                Descripcion = "Usuario encargado de revisar planeaciones didácticas",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Rol
            {
                Id = 4,
                PublicId = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Nombre = "Director",
                Descripcion = "Usuario encargado de aprobación final",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                PublicId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Nombre = "Admin",
                ApellidoPaterno = "Sistema",
                Email = "admin@uth.edu.mx",
                PasswordHash = "DEV_HASH_SOLO_PRUEBAS",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Usuario
            {
                Id = 2,
                PublicId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Nombre = "Juan",
                ApellidoPaterno = "Pérez",
                Email = "juan.perez@uth.edu.mx",
                PasswordHash = "DEV_HASH_SOLO_PRUEBAS",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Usuario
            {
                Id = 3,
                PublicId = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                Nombre = "Ana",
                ApellidoPaterno = "Torres",
                Email = "ana.torres@uth.edu.mx",
                PasswordHash = "DEV_HASH_SOLO_PRUEBAS",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Usuario
            {
                Id = 4,
                PublicId = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                Nombre = "María",
                ApellidoPaterno = "López",
                Email = "maria.lopez@uth.edu.mx",
                PasswordHash = "DEV_HASH_SOLO_PRUEBAS",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedUsuarioRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsuarioRol>().HasData(
            new UsuarioRol { UsuarioId = 1, RolId = 1, CreatedAt = new DateTime(2026, 1, 1) },
            new UsuarioRol { UsuarioId = 2, RolId = 2, CreatedAt = new DateTime(2026, 1, 1) },
            new UsuarioRol { UsuarioId = 3, RolId = 2, CreatedAt = new DateTime(2026, 1, 1) },
            new UsuarioRol { UsuarioId = 4, RolId = 3, CreatedAt = new DateTime(2026, 1, 1) }
        );
    }

    private static void SeedAcademias(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Academia>().HasData(
            new Academia
            {
                Id = 1,
                PublicId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                Nombre = "Desarrollo de Software",
                Descripcion = "Academia relacionada con programación, bases de datos y desarrollo web",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Academia
            {
                Id = 2,
                PublicId = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                Nombre = "Inglés",
                Descripcion = "Academia de asignaturas de inglés",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedCarreras(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carrera>().HasData(
            new Carrera
            {
                Id = 1,
                PublicId = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Nombre = "Ingeniería en Tecnologías de la Información e Innovación Digital",
                Clave = "ITI",
                Nivel = "Ingeniería",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedCicloPeriodos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CicloEscolar>().HasData(
            new CicloEscolar
            {
                Id = 1,
                PublicId = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                Nombre = "2026-2027",
                FechaInicio = new DateTime(2026, 9, 1),
                FechaFin = new DateTime(2027, 8, 31),
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );

        modelBuilder.Entity<Periodo>().HasData(
            new Periodo
            {
                Id = 1,
                PublicId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
                CicloEscolarId = 1,
                Nombre = "Septiembre-Diciembre 2026",
                FechaInicio = new DateTime(2026, 9, 1),
                FechaFin = new DateTime(2026, 12, 20),
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedGrupos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Grupo>().HasData(
            new Grupo
            {
                Id = 1,
                PublicId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
                Nombre = "ITI-701",
                Cuatrimestre = 7,
                CarreraId = 1,
                PeriodoId = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Grupo
            {
                Id = 2,
                PublicId = Guid.Parse("70000000-0000-0000-0000-000000000002"),
                Nombre = "ITI-702",
                Cuatrimestre = 7,
                CarreraId = 1,
                PeriodoId = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedAsignaturas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asignatura>().HasData(
            new Asignatura
            {
                Id = 1,
                PublicId = Guid.Parse("80000000-0000-0000-0000-000000000001"),
                AcademiaId = 1,
                Nombre = "Aplicaciones Web",
                Clave = "AW-701",
                Cuatrimestre = 7,
                HorasTotales = 75,
                HorasSemana = 5,
                Creditos = 5,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Asignatura
            {
                Id = 2,
                PublicId = Guid.Parse("80000000-0000-0000-0000-000000000002"),
                AcademiaId = 2,
                Nombre = "Inglés VII",
                Clave = "ING-701",
                Cuatrimestre = 7,
                HorasTotales = 60,
                HorasSemana = 4,
                Creditos = 4,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedAcademiaUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademiaUsuario>().HasData(
            new AcademiaUsuario
            {
                AcademiaId = 1,
                UsuarioId = 2,
                RolEnAcademia = RolAcademia.Docente,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new AcademiaUsuario
            {
                AcademiaId = 1,
                UsuarioId = 3,
                RolEnAcademia = RolAcademia.Docente,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new AcademiaUsuario
            {
                AcademiaId = 1,
                UsuarioId = 4,
                RolEnAcademia = RolAcademia.Revisor,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedCargaAcademica(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CargaAcademica>().HasData(
            new CargaAcademica
            {
                Id = 1,
                PublicId = Guid.Parse("90000000-0000-0000-0000-000000000001"),
                PeriodoId = 1,
                GrupoId = 1,
                AsignaturaId = 1,
                DocenteId = 2,
                RevisorId = 4,
                AcademiaId = 1,
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new CargaAcademica
            {
                Id = 2,
                PublicId = Guid.Parse("90000000-0000-0000-0000-000000000002"),
                PeriodoId = 1,
                GrupoId = 2,
                AsignaturaId = 1,
                DocenteId = 3,
                RevisorId = 4,
                AcademiaId = 1,
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedDocumentos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Documento>().HasData(
            new Documento
            {
                Id = 1,
                PublicId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
                TipoDocumento = TipoDocumento.ProgramaAsignatura,
                Titulo = "Programa de Asignatura - Aplicaciones Web",
                NombreOriginal = "programa-aplicaciones-web.pdf",
                NombreGuardado = "programa-aplicaciones-web-dev.pdf",
                Extension = ".pdf",
                MimeType = "application/pdf",
                TamanoBytes = 204800,
                RutaStorage = "documentos/programas-asignatura/2026/programa-aplicaciones-web-dev.pdf",
                HashSha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                SubidoPorId = 1,
                FechaSubida = new DateTime(2026, 1, 1),
                Estado = EstadoDocumento.Procesado,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedProgramasAsignatura(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProgramaAsignatura>().HasData(
            new ProgramaAsignatura
            {
                Id = 1,
                PublicId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"),
                DocumentoId = 1,
                AsignaturaId = 1,
                AcademiaId = 1,
                NombreAsignatura = "Aplicaciones Web",
                ClaveAsignatura = "AW-701",
                Carrera = "Ingeniería en Tecnologías de la Información e Innovación Digital",
                Cuatrimestre = 7,
                Competencia = "Desarrollar aplicaciones web utilizando tecnologías actuales y buenas prácticas de programación.",
                Proposito = "El alumno desarrollará aplicaciones web funcionales aplicando arquitectura por capas.",
                Creditos = 5,
                HorasTotales = 75,
                HorasSemana = 5,
                TextoExtraido = "Texto extraído de prueba del programa de asignatura.",
                JsonExtraido = """
                {
                  "unidades": [
                    {
                      "numero": "I",
                      "nombre": "Introducción a las aplicaciones web",
                      "resultado_aprendizaje": "El alumno identificará los componentes básicos de una aplicación web.",
                      "temas": [
                        "Cliente-servidor",
                        "HTTP",
                        "APIs REST"
                      ]
                    },
                    {
                      "numero": "II",
                      "nombre": "Desarrollo de APIs",
                      "resultado_aprendizaje": "El alumno desarrollará servicios web usando arquitectura por capas.",
                      "temas": [
                        "Controladores",
                        "Servicios",
                        "DTOs",
                        "Entity Framework Core"
                      ]
                    }
                  ]
                }
                """,
                UltimaModificacionPorId = 1,
                FechaUltimaModificacion = new DateTime(2026, 1, 1),
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedPlaneaciones(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlaneacionDidactica>().HasData(
            new PlaneacionDidactica
            {
                Id = 1,
                PublicId = Guid.Parse("cccccccc-0000-0000-0000-000000000001"),
                PeriodoId = 1,
                AsignaturaId = 1,
               
                RevisorId = 4,
                
                Estado = EstadoPlaneacion.EnProceso,
                UltimaModificacionPorId = 2,
                FechaUltimaModificacion = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedPlaneacionRelaciones(ModelBuilder modelBuilder)
    {
       
    }

    private static void SeedPlaneacionContenido(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlaneacionUnidad>().HasData(
            new PlaneacionUnidad
            {
                Id = 1,
                PublicId = Guid.Parse("dddddddd-0000-0000-0000-000000000001"),
                PlaneacionDidacticaId = 1,
                NumeroUnidad = 1,
                NombreUnidad = "Introducción a las aplicaciones web",
                PropositoEsperado = "El alumno identificará los componentes básicos de una aplicación web.",
                HorasTotales = 20,
                Orden = 1,
                UltimaModificacionPorId = 2,
                FechaUltimaModificacion = new DateTime(2026, 1, 1),
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new PlaneacionUnidad
            {
                Id = 2,
                PublicId = Guid.Parse("dddddddd-0000-0000-0000-000000000002"),
                PlaneacionDidacticaId = 1,
                NumeroUnidad = 2,
                NombreUnidad = "Desarrollo de APIs",
                PropositoEsperado = "El alumno desarrollará servicios web usando arquitectura por capas.",
                HorasTotales = 30,
                Orden = 2,
                UltimaModificacionPorId = 3,
                FechaUltimaModificacion = new DateTime(2026, 1, 1),
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }

    private static void SeedChat(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>().HasData(
            new Chat
            {
                Id = 1,
                PublicId = Guid.Parse("ffffffff-0000-0000-0000-000000000001"),
                PlaneacionDidacticaId = 1,
                Titulo = "Chat - Planeación Aplicaciones Web",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );

        modelBuilder.Entity<ChatParticipante>().HasData(
            new ChatParticipante
            {
                ChatId = 1,
                UsuarioId = 2,
                RolEnChat = "DOCENTE",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new ChatParticipante
            {
                ChatId = 1,
                UsuarioId = 3,
                RolEnChat = "DOCENTE",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new ChatParticipante
            {
                ChatId = 1,
                UsuarioId = 4,
                RolEnChat = "REVISOR",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );

        modelBuilder.Entity<ChatMensaje>().HasData(
            new ChatMensaje
            {
                Id = 1,
                PublicId = Guid.Parse("11111111-aaaa-0000-0000-000000000001"),
                ChatId = 1,
                UsuarioId = 4,
                Mensaje = "Favor de revisar que las actividades correspondan con los resultados de aprendizaje.",
                TipoMensaje = "OBSERVACION",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new ChatMensaje
            {
                Id = 2,
                PublicId = Guid.Parse("11111111-aaaa-0000-0000-000000000002"),
                ChatId = 1,
                UsuarioId = 2,
                Mensaje = "De acuerdo, revisaremos la Unidad I y ajustaremos las evidencias.",
                TipoMensaje = "TEXTO",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );

        modelBuilder.Entity<PlaneacionObservacion>().HasData(
            new PlaneacionObservacion
            {
                Id = 1,
                PublicId = Guid.Parse("22222222-aaaa-0000-0000-000000000001"),
                PlaneacionDidacticaId = 1,
                PlaneacionUnidadId = 1,
                RevisorId = 4,
                Seccion ="Unidad 1 ",
                Comentario = "La evidencia de la Unidad I debe estar mejor relacionada con el resultado de aprendizaje.",
                Estado = "ABIERTA",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }
}