# Web Hook API

GrandNode için sistem haricinde dışardan gelen siparişleri işleyen end-point.

## Installation

1. **Mongo DB**'yi ayağa kaldırın.
2. Ardından **'testdb'** isminde bir veritabanı oluşturun.
3. Uygulamayı ayağa kaldırın ve **GrandNode** kurulumunu yapın.

## Api Workflow

İlgili end-point **Grand.Web** projesi içine konumlanmıştır.

Uygulamayla birlikte ayağa kalkar ve **https://localhost:44350/api/webhook** adresine yapılan **POST** isteklerini kabul eder.

End-point'i test etmek için **Solution Items** altındaki **Postman** dökümanını (**web_hook_order_request.json**) kullanabilirsiniz.

![image](https://github.com/user-attachments/assets/b9d39e3c-4bac-4ab9-80f3-5ec40b68e0ce)  ![image](https://github.com/user-attachments/assets/8e7193e0-c1d4-402f-aa73-ef4f702c9a2c)



## Validation

End-point'e gönderilen modeldeki alanlar **Fluent Validation** yardımıyla validate edilmektedir. (**WebHookOrderModelValidator**)

- **SKU**, **Store** ve **Order Items** kontrolleri gibi mantıksal kontroller end-point içerisinde yapılmaktadır.

## Authorization

End-point'e gelen isteklerin **headers** bölümünde **X-Api-Key** değeri beklenmektedir. Bu değer gönderilmezse veya yanlış olursa end-point **401 Unauthorized** dönecektir. (**CreateOrderAuthorizeAttribute**)

## Retry Mechanism

End-point'in hataya düştüğü senaryoları tekrar handle edebilmek için **Polly** kütüphanesiyle basit bir retry mekanizması kurulmuştur.
- Yalnızca sistem hataları ele alınır validasyonlarda oluşan client bazlı hatalarda retry yapılmaz.
- Eğer sistem **order oluşturma** aşamasında hataya düşerse 3 kez belli aralıklarla tekrar çalışacaktır.
- Tüm bu işlemler sonucunda hala hata alınıyorsa istek sonlanacaktır.

#### Alternatif Kapsamlı Çözüm

**Queue** ve **BackgroundJob** kullanarak daha kapsamlı, yönetilebilir ve izlenebilir bir retry mekanizması kurulabilir. (**Queue-Based Retry**)

## Logging

End-point içerisinde herhangi bir sistem hatası oluşursa **Serilog** yardımıyla **AppData** altındaki **Logs** klasörüne hatalar loglanmaktadır.

## Idempotency Check

End-point, gelen istekte benzersiz ve ilgili requesti temsil eden bir **IdempotencyKey** değeri bekler. Client ve end-point arasında tutarlılık olması açısından isteğin başında bu değere sahip order olup olmadığını kontrol eder.

- Eğer bu key'e ait bir order varsa direkt o order'ın **id**'sini döner.

## Transactional Customer and Order Create

End-point **order** oluşturmadan önce eğer müşteri sistemde kayıtlı değilse yeni müşteri oluşturur.

- İstisna olarak, yeni müşteri kaydından sonra **order** oluşturma aşamasında bir hata alınırsa, oluşturulan müşteri veritabanından silinir.
