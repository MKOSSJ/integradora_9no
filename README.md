** cargar el proyecto

dotnet restore

dotnet build

** cargar las herramientas

dotnet add package Microsoft.EntityFrameworkCore.Design

dotnet add package Microsoft.EntityFrameworkCore.Tools

dotnet add package Pomelo.EntityFrameworkCore.MySql

dotnet tool install --global dotnet-ef

** cargar las migraciones

dotnet ef database update



                                        +----------------+
                                        |    Usuario     |
                                        +----------------+
                                          |           |
                              UsuarioRol  |           | AcademiaUsuario
                                          |           |
                                          |           |
                                 +--------+           +--------+
                                 |                             |
                          +-------------+              +---------------+
                          |     Rol     |              |   Academia    |
                          +-------------+              +---------------+
                                                         |
                                                         |
                                                         |
                                                   +------------+
                                                   | Asignatura |
                                                   +------------+
                                                       |
                +--------------------------------------+---------------------------+
                |                                      |                           |
                |                                      |                           |
      ProgramaAsignatura                    PlaneacionDidactica            CargaAcademica
                |                                      |                           |
                |                                      |                           |
           Documento                         +---------+----------+                |
                                             |         |          |                |
                                             |         |          |                |
                                   PlaneacionUnidad    |    PlaneacionDocente      |
                                             |         |          |                |
                                             |         |          |                |
                                   PlaneacionActividad |      Usuario (Docente)    |
                                                       |
                                               PlaneacionGrupo
                                                       |
                                                     Grupo
                                                       |
                             +-------------------------+----------------------+
                             |                                                |
                          Carrera                                         Periodo
                                                                              |
                                                                              |
                                                                       CicloEscolar

PlaneacionDidactica
        |
        +--------------------+
        |                    |
        |                    |
 PlaneacionObservacion      Chat
                                  |
                   +--------------+--------------+
                   |                             |
          ChatParticipante                 ChatMensaje
                   |                             |
                Usuario                      Usuario
