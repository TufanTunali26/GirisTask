# GirisTask
C# WPF Server Client Scada Örnek proje


- **server & durum yöneicisi**: Sunucu uygulaması (`iotechgiristask`), dahili bir MQTT broker barındırır. Lamba durumu (Açık/Kapalı) ve metin alanı verisini hafızasında kalıcı olarak tutar. Kendi arayüzünden yapılan değişiklikleri anında istemcilere iletirken, istemcilerden gelen emirleri de işler.
- **Client Başlangıç durumu & senkronizasyon**: İstemci uygulaması (`iotechgiristaskclient`) açıldığında başlangıçta lamba ve sayı değerleri `null` durumdadır (hiçbir yerel varsayımda bulunmaz). Sunucuya bağlandığı anda durum talep eder ve MQTT `Retain` (Kalıcı Mesaj) altyapısı sayesinde sunucunun en son güncel lamba durumunu ve üretilmiş en son rastgele sayıyı anında alıp arayüzüne yansıtır.
- **Client Server etkileşimi**:
  - Clientt üzerinde butona basıldığında servere benim mevcut durumumu tersine çevir isteği gider, server ise clientin son lamba state durumunu tersine çevirir
  - Clientten rastgele text butonuna basıldığında servere bir rastgele sayı oluştur isteği gönderilir, server rastgele say üretip sonrasında clientin rastgele sayı değerini günceller
  - Client kapatılıp tekrar açıldığında son lamba ve rastgele textdeğerlerini sunucudan tekrar çekerek bir senkronizasyon sağlar.
- **trafik takibi ve doğrulama**:  ek konsol izleme aracı (`iotechgiristaskmonitor`) ile client ve server arasındaki tüm veri akışı okunabilir formatta izlenebilir




| Topic | Yön | Açıklama |
|---|---|---|
| `iotech/lamp/state` | Server ➔ Client | Lambanın güncel durumu (`1`: Açık, `0`: Kapalı). *Retained* |
| `iotech/lamp/set` | Client ➔ Server | İstemcinin lamba durumunu değiştirme talebi (`1` veya `0`) |
| `iotech/text/state` | Server ➔ Client | Güncel metin kutusu değeri veya üretilen son rastgele sayı. *Retained* |
| `iotech/text/random`| Client ➔ Server | İstemcinin yeni bir rastgele sayı üretilmesi talebi |
| `iotech/get_state`  | Client ➔ Server | İstemci açılışında son durumları isteme (Senkronizasyon) |

