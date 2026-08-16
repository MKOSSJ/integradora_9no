namespace Plandi.Dto.Common;

public sealed class ConflictException(string message) : AppException(message);
