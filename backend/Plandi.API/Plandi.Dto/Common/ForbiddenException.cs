namespace Plandi.Dto.Common;

public sealed class ForbiddenException(string message) : AppException(message);
