# IOTECH Giriş Task - MQTT Tabanlı Server & Client Uygulaması

Bu proje; **C# (.NET 8)** ve **WPF (Windows Presentation Foundation)** teknolojileri kullanılarak, **MQTTnet** kütüphanesi üzerinden çift yönlü ve gerçek zamanlı haberleşen bir **Server - Client (Sunucu - İstemci)** mimarisi olarak geliştirilmiştir.



- **Merkezi Sunucu & Durum Yönetimi**: Sunucu uygulaması (`iotechgiristask`), dahili bir MQTT broker barındırır. Lamba durumu (Açık/Kapalı) ve metin alanı verisini hafızasında kalıcı olarak tutar. Kendi arayüzünden yapılan değişiklikleri anında istemcilere iletirken, istemcilerden gelen emirleri de işler.
- **İstemci Başlangıç Durumu & Senkronizasyon**: İstemci uygulaması (`iotechgiristaskclient`) açıldığında başlangıçta lamba ve sayı değerleri `null` durumdadır (hiçbir yerel varsayımda bulunmaz). Sunucuya bağlandığı anda durum talep eder ve MQTT `Retain` (Kalıcı Mesaj) altyapısı sayesinde sunucunun en son güncel lamba durumunu ve üretilmiş en son rastgele sayıyı anında alıp arayüzüne yansıtır.
- **Çift Yönlü Etkileşim**:
  - Clientt üzerinde butona basıldığında servere benim mevcut durumumu tersine çevir isteği gider, server ise clientin son lamba state durumunu tersine çevirir
  - Clientten rastgele text butonuna basıldığında servere bir rastgele sayı oluştur isteği gönderilir, server rastgele say üretip sonrasında clientin rastgele sayı değerini günceller
  - Client kapatılıp tekrar açıldığında son lamba ve rastgele textdeğerlerini sunucudan tekrar çekerek bir senkronizasyon sağlar.
- **Canlı Ağ Trafiği Takibi**:  ek konsol izleme aracı (`iotechgiristaskmonitor`) ile client ve server arasındaki tüm veri akışı okunabilir formatta izlenebilir

---

## 📁 Proje Yapısı

Çözüm (`iotechgiristask.slnx`) içerisinde 3 ana proje yer almaktadır:

1. **`iotechgiristask` (Server - WPF)**:
   - Dahili MQTT Broker'ı (Port: `1883`) başlatır ve yönetir.
   - **1. Grup (Lamba Grubu)**: Lambayı simgeleyen grafik (`Ellipse`) ve durumu değiştiren buton.
     - Açık Durum: Açık Sarı (`#FFEB3B`)
     - Kapalı Durum: Koyu Gri (`#424242`)
   - **2. Grup (Text Grubu)**: Klavyeden giriş yapılabilen metin alanı (`TextBox`) ve rastgele sayı üreten buton.
   - Tüm durumları bellekte saklar ve değişiklikleri anlık olarak istemcilere yayınlar.

2. **`iotechgiristaskclient` (Client - WPF)**:
   - Başlangıçta lamba ve rastgele sayı değerleri `null` olarak açılır.
   - Sunucuya bağlanır ve mevcut durumları çekerek arayüzü günceller.
   - **Lamba Butonu**: Rengi ve yazısı sunucudaki lamba açıkken açık sarı, kapalıyken koyu gri olur. Tıklandığında sunucudaki lambayı açar/kapatır.
   - **Text Butonu**: Üzerindeki değer sunucudaki metin/sayı alanıyla her an senkronizedir. Tıklandığında sunucudan yeni bir rastgele sayı üretmesini talep eder.
   - Kapanıp tekrar açılsa bile sunucudaki son değeri alarak devam eder.

3. **`iotechgiristaskmonitor` (Trafik İzleyici - Konsol Uygulaması)**:
   - Sunucu ile istemciler arasındaki tüm MQTT paketlerini yakalar.
   - Paketin yönünü (`[CLIENT -> SERVER]`, `[SERVER -> CLIENT]`), zaman damgasını, konusunu (`Topic`) ve içeriğini (`Payload`) renkli olarak konsola yansıtır.

---

## 📡 MQTT İletişim Mimarisi (Topic Listesi)

| Topic | Yön | Açıklama |
|---|---|---|
| `iotech/lamp/state` | Server ➔ Client | Lambanın güncel durumu (`1`: Açık, `0`: Kapalı). *Retained* |
| `iotech/lamp/set` | Client ➔ Server | İstemcinin lamba durumunu değiştirme talebi (`1` veya `0`) |
| `iotech/text/state` | Server ➔ Client | Güncel metin kutusu değeri veya üretilen son rastgele sayı. *Retained* |
| `iotech/text/random`| Client ➔ Server | İstemcinin yeni bir rastgele sayı üretilmesi talebi |
| `iotech/get_state`  | Client ➔ Server | İstemci açılışında son durumları isteme (Senkronizasyon) |

> **MQTT Retain Özelliği**: Sunucunun yayınladığı durum mesajları broker üzerinde tutulur. Böylece ağa yeni katılan herhangi bir istemci anında en güncel veriyi teslim alır.

---

## 🚀 Nasıl Çalıştırılır?

Projeyi derlemek ve çalıştırmak için bilgisayarınızda **.NET 8 SDK** bulunmalıdır.

### 1. Sunucu Uygulamasını Başlatın (Önce Server açılmalıdır)
```bash
dotnet run --project iotechgiristask/iotechgiristask.csproj
```

### 2. İstemci Uygulamasını Başlatın
```bash
dotnet run --project iotechgiristaskclient/iotechgiristaskclient.csproj
```
*(Birden fazla istemci penceresi açarak çoklu istemci senkronizasyonunu test edebilirsiniz.)*

### 3. MQTT Trafiğini İzleyin (Opsiyonel)
```bash
dotnet run --project iotechgiristaskmonitor/iotechgiristaskmonitor.csproj
```

---

## 🛠️ Kullanılan Teknolojiler ve Kütüphaneler
- **Framework**: .NET 8 (C# 12)
- **Arayüz (UI)**: WPF (Windows Presentation Foundation) - XAML
- **Haberleşme Protokolü**: MQTT (Message Queuing Telemetry Transport)
- **MQTT Kütüphanesi**: `MQTTnet` ve `MQTTnet.Server` (v5.2.0.1603)
