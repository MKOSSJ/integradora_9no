export enum RentTypeId
{ 
   
    Mensualidad = 1,
    Condonacion = 2,
    Extension = 3,
}

export enum PriceLevel
{ 
    UnitPrice = 0,
    UnitPrice1 = 1,
    UnitPrice2 = 2,
    UnitPrice3 = 3,
}

export enum InventoryView
{ 
    Critical = 1,
    Warning = 2,
    Other = 3,
    ByCategory = 4,
    WithDiscrepancia = 5,
    ItemsUpdated = 6
}

export enum Roles {
    Unknown = 0,
    User = 1,
    Admin = 2,
    
    Ventas = 3,
    SysAdmin = 13,
    Root = 14
}

export enum CompanyStatus
{
    Inactive = 0,
    PendingPayment = 1,
    Expired = 2,
    Active = 3,
    Free = 4
}

export enum AlertLevel
{
    Info = 0,
    Sucess =1,
    Warning =2,
    Error = 3
}

export enum UserStatus
{
    Desconectado = 0,
    Conectado =1,
    Deshabilitado =2,
    Desconocido = 3
}

export enum StatusVentaEnum {
    Completada,
    Cancelada ,
    Reembolsado
}

export enum MeasureType {
    Desconocido = 0,
    Ud = 1,
    Kg = 2,
    L = 3,
    m = 4
}
export enum ReturnReason {
  Defectuoso = 1,
  Caducado = 2,
  ErrorVenta = 3,
  ClienteNoQuiso = 4,
  Otro = 5
}

export enum DaysForWeeks {
  Week1 = 7,
  Weeks2 = 14,
  Weeks3 = 21,
  Weeks4 = 28,
  Weeks5 = 35
}

export function getReturnReasonLabel(reason: ReturnReason): string {
  switch (reason) {
    case ReturnReason.Defectuoso:
      return 'Producto defectuoso';
    case ReturnReason.Caducado:
      return 'Caducado';
    case ReturnReason.ErrorVenta:
      return 'Error en la venta';
    case ReturnReason.ClienteNoQuiso:
      return 'Cliente no lo quiso';
    case ReturnReason.Otro:
      return 'Otro';
  }
}

export function getDaysForWeeksLabel(week: DaysForWeeks): string {
  switch (week) {
    case DaysForWeeks.Week1:
      return '1 semana';
    case DaysForWeeks.Weeks2:
      return '2 semanas';
    case DaysForWeeks.Weeks3:
      return '3 semanas';
    case DaysForWeeks.Weeks4:
      return '4 semanas';
    case DaysForWeeks.Weeks5:
      return '5 semanas';
  }
}



export function getCompanyStatusEnumName(value: number): string {
    const name = CompanyStatus[value];
    if (name === undefined) {
        return CompanyStatus.Inactive.toString();
    }
    return CompanyStatus[value];  
}

export function getUserStatusEnumName(value: number): string {
    const name = UserStatus[value];
    if (name === undefined) {
        return UserStatus.Desconocido.toString();
    }
    return UserStatus[value];  
}