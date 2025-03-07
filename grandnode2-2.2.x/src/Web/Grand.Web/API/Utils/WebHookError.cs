namespace Grand.Web.API.Utils
{
    public struct WebHookError
    {
        public static string StackTrace = "StackTrace";
        public struct CreateOrder
        {

            public static string ProductMap = "Ürün Eşleşmelerinde problem var. Ürün SKU'larını kontrol ediniz.";
            public static string OrderCouldNotBeCreated = "Sipariş oluşturulurken bir hata meydana geldi";
            public static string StoreNotFound = "Mağaza bulunamadı.";

            public struct Validation
            {
                public static string CustomerNotNull = "Müşteri alanı boş olamaz";
                public static string CustomerEmailNotNull = "Müşteri e-posta alanı boş olamaz";
                public static string OrderItemsNotNull = "Sipariş ögeleri boş olamaz";
                public static string SkuNotNull = "SKU bilgisi boş olamaz.";
                public static string AddressNotNull = "Adres bilgisi boş olamaz.";
                public static string PaymentMethodNotNull = "Ödeme tipi bilgisi boş olamaz.";
                public static string PaymentStatusNotValid = "Ödeme durumu bilgisini kontrol ediniz.";
            }
        }
    }
}
