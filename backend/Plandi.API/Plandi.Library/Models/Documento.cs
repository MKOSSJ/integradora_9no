using Plandi.Dto.Enums;
using System;
using System.Collections.Generic;
using System.Text;


namespace Plandi.Library.Models
{
    public class Documento : BaseEntity
    {
        public TipoDocumento TipoDocumento { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string NombreOriginal { get; set; } = string.Empty;

        public string NombreGuardado { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public string MimeType { get; set; } = string.Empty;

        public long TamanoBytes { get; set; }

        public string RutaStorage { get; set; } = string.Empty;

        public string? HashSha256 { get; set; }

        public long SubidoPorId { get; set; }
        public Usuario SubidoPor { get; set; } = null!;

        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

        public EstadoDocumento Estado { get; set; } = EstadoDocumento.Subido;

        public ProgramaAsignatura? ProgramaAsignatura { get; set; }
    }
}
