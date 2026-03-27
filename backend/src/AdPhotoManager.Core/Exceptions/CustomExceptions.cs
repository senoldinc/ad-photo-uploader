namespace AdPhotoManager.Core.Exceptions;

public class AdConnectionException : Exception
{
    public AdConnectionException() : base("Active Directory bağlantı hatası")
    {
    }

    public AdConnectionException(string message) : base(message)
    {
    }

    public AdConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class UserNotFoundException : Exception
{
    public UserNotFoundException() : base("Kullanıcı bulunamadı")
    {
    }

    public UserNotFoundException(string message) : base(message)
    {
    }

    public UserNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class PhotoValidationException : Exception
{
    public PhotoValidationException() : base("Fotoğraf doğrulama hatası")
    {
    }

    public PhotoValidationException(string message) : base(message)
    {
    }

    public PhotoValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
