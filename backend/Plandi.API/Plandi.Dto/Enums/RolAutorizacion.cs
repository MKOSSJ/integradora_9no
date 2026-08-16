namespace Plandi.Dto.Enums;

// Identificadores estables de los roles funcionales. La base de datos conserva
// sus claves actuales; el servicio de autorización los resuelve por nombre.
public enum RolAutorizacion
{
    Docente = 1,
    Revisor = 2,
    Director = 3
}
