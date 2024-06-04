
```markdown
# .NET 7 Web API Projesi

Bu proje, döviz kuru sorgulama, belge yönetimi, çalışan CRUD işlemleri ve JWT ile kullanıcı kimlik doğrulama gibi çeşitli işlevler sağlayan bir .NET 7 Web API uygulamasıdır.

## İçindekiler

- [Başlarken](#başlarken)
- [Gereksinimler](#gereksinimler)
- [Kurulum](#kurulum)
- [Yapılandırma](#yapılandırma)
- [Uygulamayı Çalıştırma](#uygulamayı-çalıştırma)
- [API Uç Noktaları](#api-uç-noktaları)
  - [Döviz Kurları](#döviz-kurları)
  - [Belge Yönetimi](#belge-yönetimi)
  - [Çalışan CRUD](#çalışan-crud)
  - [Kimlik Doğrulama](#kimlik-doğrulama)

## Başlarken

Bu rehber, projeyi yerel makinenizde çalıştırmak için ihtiyacınız olan adımları içerir.

### Gereksinimler

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

### Kurulum

1. Bu projeyi yerel makinenize klonlayın:

   ```sh
   git clone https://github.com/kullanıcı_adı/proje_adı.git
   ```

2. Proje dizinine gidin:

   ```sh
   cd proje_adı
   ```

3. Gerekli NuGet paketlerini indirin:

   ```sh
   dotnet restore
   ```

### Yapılandırma

1. `appsettings.json` dosyasını açın ve aşağıdaki ayarları yapılandırın:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=your_database;User Id=your_user;Password=your_password;"
     },
     "JwtSettings": {
       "SecretKey": "your_secret_key",
       "Issuer": "your_issuer",
       "Audience": "your_audience",
       "ExpiryMinutes": 60
     },
     "ConfigurationSettings": {
       "ConnectionString": "Server=your_server;Database=your_database;User Id=your_user;Password=your_password;"
     }
   }
   ```

2. `ConfigurationSettings` ve `JwtSettings` bölümlerini kendi ayarlarınıza göre güncelleyin.

### Uygulamayı Çalıştırma

1. Uygulamayı çalıştırmak için aşağıdaki komutu kullanın:

   ```sh
   dotnet run
   ```

2. Tarayıcınızda `https://localhost:7064` adresine gidin.

### API Uç Noktaları

#### Döviz Kurları

- **GET** `/api/getMultipleExchangeRates`
  - Açıklama: Belirli bir temel para birimi için birden fazla hedef para biriminin döviz kurlarını getirir.
  - Parametreler:
    - `baseCurrency` (string): Temel para birimi
    - `targetCurrencies` (string): Virgülle ayrılmış hedef para birimleri
  - Örnek İstek: `GET /api/getMultipleExchangeRates?baseCurrency=USD&targetCurrencies=EUR,GBP`

#### Belge Yönetimi

- **POST** `/Documents`
  - Açıklama: Yeni bir belge ekler.
  - Gövde: `Document` modeli
- **GET** `/Documents`
  - Açıklama: Tüm belgeleri getirir.
- **GET** `/Documents/{id}`
  - Açıklama: Belirtilen ID'ye sahip belgeyi getirir.
- **DELETE** `/Documents/{id}`
  - Açıklama: Belirtilen ID'ye sahip belgeyi siler.

#### Çalışan CRUD

- **GET** `/EmployeeCrud/{id}`
  - Açıklama: Belirtilen ID'ye sahip çalışanı getirir.
- **POST** `/EmployeeCrud`
  - Açıklama: Yeni bir çalışan ekler.
  - Gövde: `Employee` modeli
- **PUT** `/EmployeeCrud/{id}`
  - Açıklama: Mevcut bir çalışanı günceller.
  - Gövde: `Employee` modeli
- **DELETE** `/EmployeeCrud/{id}`
  - Açıklama: Belirtilen ID'ye sahip çalışanı siler.

#### Kimlik Doğrulama

- **POST** `/Auth/Register`
  - Açıklama: Yeni bir kullanıcı kaydeder.
  - Gövde: `User` modeli
- **POST** `/Auth/Login`
  - Açıklama: Kullanıcı giriş yapar ve JWT alır.
  - Gövde: `User` modeli

### İletişim

Sorularınız veya geri bildirimleriniz için lütfen [canberk.ok@gmail.com] üzerinden benimle iletişime geçin.
```

Bu README dosyası, projenizin işlevlerini ve yapılandırma adımlarını profesyonel bir şekilde açıklamaktadır. Herhangi bir sorunuz varsa veya eklemek istediğiniz başka bilgiler varsa, lütfen bana bildirin!
