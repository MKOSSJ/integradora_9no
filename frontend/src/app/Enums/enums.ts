export enum EstadoPlaneacion {
  Borrador = 1,
  EnProceso = 2,
  EnRevision = 3,
  CorreccionSolicitada = 4,
  Aprobada = 5,
  Rechazada = 6,
  Generada = 7
}

export enum TipoDocumento {
  ProgramaAsignatura = 1,
  PlantillaPlaneacion = 2,
  UsuariosCsv = 3,
  PlaneacionGenerada = 4,
  Anexo = 5
}

export enum EstadoDocumento {
  Subido = 1,
  Procesando = 2,
  Procesado = 3,
  Error = 4
}

export enum RolAcademia {
  Docente = 1,
  Revisor = 2,
  Director = 3
}

export enum FaseSecuencia {
  Apertura = 1,
  Desarrollo = 2,
  Cierre = 3
}

export enum TipoEvaluacion {
  Conceptual = 1,
  Producto = 2,
  Desempeno = 3,
  Ensayo = 4,
  EstudioDeCaso = 5,
  AnalisisDeDesempeno = 6,
  Proyecto = 7,
  Practica = 8,
  Reporte = 9,
  Exposicion = 10,
  Otro = 99
}

export enum EstrategiaApertura {
  PreguntasGeneradoras = 1,
  PreguntasGuia = 2,
  PreguntasExploratorias = 3,
  PreguntasLiterales = 4,
  PreguntasIntercaladas = 5,
  SQA = 6,
  IdentificacionExpectativas = 7,
  LluviaIdeas = 8,
  AnalisisArticulos = 9,
  DinamicasPresentacion = 10,
  Analogia = 11,
  ClaseMagistral = 12,
  TecnicaExpositiva = 13,
  MapaMental = 14,
  MapaConceptual = 15,
  DiagramaCausaEfecto = 16,
  DiagramaFlujo = 17,
  DiagramaArbol = 18,
  DiagramaRadial = 19,
  DiagramaJerarquico = 20,
  DiagramaVenn = 21,
  TablaRelacional = 22,
  Esquema = 23,
  RedSemantica = 24,
  CuadroSinoptico = 25,
  CuadroComparativo = 26,
  LineaTiempo = 27,
  Organigrama = 28,
  ConstelacionPalabras = 29,
  ArbolProblemas = 30,
  SecuenciaHechos = 31,
  AnalisisEvidencias = 32,
  MatrizClasificacion = 33,
  MatrizInduccion = 34,
  AsistenciaConferencia = 35,
  Entrevista = 36,
  VisitaEmpresa = 37,
  LecturaDocumentos = 38,
  LecturaComentada = 39,
  Investigacion = 40,
  WebQuest = 41,
  PresentacionMultimedia = 42,
  Cuestionario = 43,
  Asamblea = 44,
  Congreso = 45,
  Coloquio = 46,
  Foro = 47,
  Simposio = 48,
  Seminario = 49,
  Panel = 50,
  MesaRedonda = 51,
  DialogoPhilips66 = 52,
  Resumen = 53,
  Subrayado = 54,
  CartografiaConceptual = 55,
  ElaboracionCarteles = 56,
  Demostracion = 57,
  TutoriaPares = 58,
  Murmullos = 59,
  EjerciciosEscritos = 60,
  EjerciciosCienciasExactas = 61,
  OrganizadoresInformacion = 62,
  ClasificacionConceptos = 63,
  AnalisisSemejanzasDiferencias = 64,
  AnalisisVentajasDesventajas = 65,
  ProyectoInvestigacion = 66,
  GrupoFocal = 67,
  Debate = 68,
  Correlacion = 69,
  Ensayo = 70,
  QQQ = 71,
  Sintesis = 72,
  VHeuristica = 73,
  PracticaGuiada = 74,
  PracticaSemiguiada = 75
}

export enum EstrategiaDesarrollo {
  Dramatizacion = 1,
  EstudioCaso = 2,
  Debate = 3,
  Foro = 4,
  Panel = 5,
  Simposio = 6,
  Seminario = 7,
  MesaRedonda = 8,
  Coloquio = 9,
  Ensayo = 10,
  Taller = 11,
  TutoriaPares = 12,
  AprendizajeCooperativo = 13,
  AprendizajeBasadoProblemas = 14,
  AprendizajePorProyectos = 15,
  Simulacion = 16,
  JuegoRoles = 17,
  AprendizajeSituado = 18,
  PracticaLaboratorio = 19,
  GrupoFocal = 20,
  Estancias = 21,
  Estadias = 22
}

export enum EstrategiaCierre {
  CuestionarioReflexion = 1,
  SQA = 2,
  PresentacionMultimedia = 3,
  PresentacionResultadosABP = 4,
  MapaMental = 5,
  MapaConceptual = 6,
  DiagramaCausaEfecto = 7,
  TablaRelacional = 8,
  Esquema = 9,
  RedSemantica = 10,
  CuadroSinoptico = 11,
  CuadroComparativo = 12,
  Ensayo = 13,
  VideoTestimonial = 14,
  AnalisisArticulos = 15,
  Debate = 16,
  Foro = 17,
  Simposio = 18,
  Seminario = 19,
  Coloquio = 20,
  Panel = 21,
  MesaRedonda = 22,
  PresentacionReportePracticas = 23,
  SeguimientoPares = 24
}

export enum AgenteEvaluador {
  Autoevaluacion = 1,
  Coevaluacion = 2,
  Heteroevaluacion = 3
}