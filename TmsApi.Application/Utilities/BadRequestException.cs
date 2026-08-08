namespace TmsApi.Application.Utilities;

public class BadRequestException(string message) : Exception(message);
