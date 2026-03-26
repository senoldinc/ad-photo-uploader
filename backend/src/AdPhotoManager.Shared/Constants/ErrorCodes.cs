namespace AdPhotoManager.Shared.Constants;

public static class ErrorCodes
{
    // Authentication errors
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string INSUFFICIENT_PERMISSIONS = "INSUFFICIENT_PERMISSIONS";
    public const string INVALID_TOKEN = "INVALID_TOKEN";
    public const string INVALID_REFRESH_TOKEN = "INVALID_REFRESH_TOKEN";
    public const string UNAUTHORIZED_ORGANIZATION = "UNAUTHORIZED_ORGANIZATION";
    public const string INVALID_AUTH_CODE = "INVALID_AUTH_CODE";
    public const string INVALID_STATE = "INVALID_STATE";
    public const string AUTH_SERVICE_ERROR = "AUTH_SERVICE_ERROR";
    public const string INVALID_RETURN_URL = "INVALID_RETURN_URL";

    // User errors
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string INVALID_PAGE_SIZE = "INVALID_PAGE_SIZE";
    public const string INVALID_SEARCH_QUERY = "INVALID_SEARCH_QUERY";

    // Sync errors
    public const string SYNC_IN_PROGRESS = "SYNC_IN_PROGRESS";
    public const string AD_CONNECTION_ERROR = "AD_CONNECTION_ERROR";

    // Photo errors
    public const string INVALID_PHOTO_FORMAT = "INVALID_PHOTO_FORMAT";
    public const string PHOTO_TOO_LARGE = "PHOTO_TOO_LARGE";
    public const string OUTPUT_SIZE_EXCEEDED = "OUTPUT_SIZE_EXCEEDED";
    public const string INVALID_QUALITY = "INVALID_QUALITY";
    public const string PHOTO_NOT_FOUND = "PHOTO_NOT_FOUND";
    public const string AD_UPLOAD_FAILED = "AD_UPLOAD_FAILED";
    public const string AD_DELETE_FAILED = "AD_DELETE_FAILED";
    public const string IMAGE_PROCESSING_ERROR = "IMAGE_PROCESSING_ERROR";
    public const string PHOTO_VALIDATION_ERROR = "PHOTO_VALIDATION_ERROR";
    public const string PHOTO_UPLOAD_ERROR = "PHOTO_UPLOAD_ERROR";
    public const string PHOTO_DELETE_ERROR = "PHOTO_DELETE_ERROR";

    // General errors
    public const string DATABASE_ERROR = "DATABASE_ERROR";
    public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";

    // Turkish error messages
    public static readonly Dictionary<string, string> Messages = new()
    {
        { UNAUTHORIZED, "Kimlik doğrulama gerekli" },
        { INSUFFICIENT_PERMISSIONS, "Bu işlem için yeterli yetkiniz yok" },
        { INVALID_TOKEN, "Geçersiz veya süresi dolmuş token" },
        { INVALID_REFRESH_TOKEN, "Geçersiz veya süresi dolmuş yenileme token'ı" },
        { UNAUTHORIZED_ORGANIZATION, "Bu organizasyondan kullanıcılar sisteme erişemez" },
        { INVALID_AUTH_CODE, "Geçersiz yetkilendirme kodu" },
        { INVALID_STATE, "Geçersiz state parametresi" },
        { AUTH_SERVICE_ERROR, "Kimlik doğrulama servisi hatası" },
        { INVALID_RETURN_URL, "Geçersiz yönlendirme URL'si" },
        { USER_NOT_FOUND, "Kullanıcı bulunamadı" },
        { INVALID_PAGE_SIZE, "Sayfa boyutu 1 ile 100 arasında olmalıdır" },
        { INVALID_SEARCH_QUERY, "Geçersiz arama sorgusu" },
        { SYNC_IN_PROGRESS, "Bir senkronizasyon zaten devam ediyor" },
        { AD_CONNECTION_ERROR, "Active Directory bağlantı hatası" },
        { INVALID_PHOTO_FORMAT, "Sadece JPEG formatı desteklenmektedir" },
        { PHOTO_TOO_LARGE, "Fotoğraf boyutu 500 KB'dan küçük olmalıdır" },
        { OUTPUT_SIZE_EXCEEDED, "İşlenmiş fotoğraf 100 KB limitini aşıyor. Lütfen kaliteyi düşürün." },
        { INVALID_QUALITY, "Geçersiz kalite değeri (30-100 arası olmalıdır)" },
        { PHOTO_NOT_FOUND, "Kullanıcının fotoğrafı bulunamadı" },
        { AD_UPLOAD_FAILED, "Active Directory'ye yükleme başarısız oldu" },
        { AD_DELETE_FAILED, "Active Directory'den silme başarısız oldu" },
        { IMAGE_PROCESSING_ERROR, "Görüntü işleme hatası" },
        { PHOTO_VALIDATION_ERROR, "Fotoğraf doğrulama hatası" },
        { PHOTO_UPLOAD_ERROR, "Fotoğraf yükleme hatası" },
        { PHOTO_DELETE_ERROR, "Fotoğraf silme hatası" },
        { DATABASE_ERROR, "Veritabanı hatası" },
        { INTERNAL_SERVER_ERROR, "Sunucu hatası" }
    };

    public static string GetMessage(string errorCode)
    {
        return Messages.TryGetValue(errorCode, out var message) ? message : errorCode;
    }
}
