# 🛒 Beyaz Eşya E-Ticaret Projesi

Bu proje, **ASP.NET MVC (.NET Framework 4.8)** kullanılarak geliştirilmiş, beyaz eşya satıcıları için tasarlanmış bir **e-ticaret platformudur**.  
Gereksinim analizi, iş akışları, veritabanı tasarımı, kullanıcı arayüzü geliştirme ve test aşamaları tamamlanarak hayata geçirilmiştir.

---

## 📌 İçindekiler
- [Proje Hakkında](#-proje-hakkında)
- [Özellikler](#-özellikler)
- [Teknik Detaylar](#-teknik-detaylar)
- [Proje Yapısı](#-proje-yapısı)
- [Kurulum](#-kurulum)
- [Kullanım](#-kullanım)
- [Ekran Görüntüleri](#-ekran-görüntüleri)

---

## 📖 Proje Hakkında
**Beyaz Eşya E-Ticaret Projesi**, küçük ve orta ölçekli beyaz eşya satıcılarının ürünlerini çevrimiçi ortamda satışa sunabilecekleri bir e-ticaret platformudur.  

Proje, iki ana bölümden oluşur:
- **Yönetici Paneli**: Kategori, ürün, sipariş, tedarik, fatura ve raporlama gibi tüm yönetim işlemlerini sağlar.
- **Müşteri Paneli**: Son kullanıcıların ürünleri inceleyebilmesi, sepete ekleyebilmesi, sipariş oluşturabilmesi, hesabını yönetebilmesi ve site yöneticisiyle iletişim kurabilmesine imkan tanır.

---

## ✨ Özellikler

### Yönetici Paneli
- Kategori, Marka ve Model yönetimi
- Ürün ekleme, düzenleme, stok takibi
- Sipariş durum yönetimi (Hazırlanıyor, Kargoda, Teslim Edildi vb.)
- Tedarikçilerden ürün alımı ve stok güncelleme
- Raporlama: Satışlar, stok durumu, en çok satılan ürünler
- Fatura oluşturma ve görüntüleme

### Müşteri Paneli
- Ürünleri kategori, marka ve modele göre listeleme
- Ürün detay sayfası, benzer ürünler önerisi
- Sepete ürün ekleme, adet güncelleme, ürün silme
- Sipariş oluşturma ve sipariş geçmişini görüntüleme
- Kullanıcı hesabı yönetimi (bilgi güncelleme, şifre değişikliği)
- İletişim formu aracılığıyla geri bildirim gönderme

### Genel Özellikler
- Responsive (mobil uyumlu) tasarım
- Dinamik fiyat ve stok bilgileri
- Modern ve kullanıcı dostu arayüz
- Basit ve anlaşılır kod yapısı, kolay genişletilebilir mimari

---

## 💻 Teknik Detaylar
- ASP.NET MVC (C#) – .NET Framework 4.8
- MS SQL Server (Database-First yaklaşımı, EDMX)
- Entity Framework 6
- HTML5, CSS3, Bootstrap 5, JavaScript
- Git + GitHub

---

## 📂 Proje Yapısı
BeyazEsyaE-Ticaret/
- App_Start # MVC yapılandırma dosyaları
- Controllers # Controller sınıfları
- Models # ViewModel’ler ve Entity sınıfları
- Service # Dışa aktarılacak fatura şablonu
- Uploads # Yüklenen resimlerin kaydedildiği klasör
- Views # Razor View sayfaları
-   Shared # Layout dosyaları
-   Musteri # Müşteri paneli sayfaları
-   Yonetici # Yönetici paneli sayfaları
- Content # CSS, ikon ve statik içerikler
- Scripts # JavaScript dosyaları

---

## 🚀 Kurulum

1. Repoyu bilgisayarınıza klonlayın:
   ```bash
   git clone https://github.com/<kullanici_adiniz>/BeyazEsyaE-Ticaret.git
2. Visual Studio’da BeyazEsyaE-Ticaret.sln dosyasını açın.
3. Web.config dosyasında yer alan connection string’i kendi SQL Server ayarınıza göre düzenleyin.
4. Projeyi Debug veya Run ile başlatın.

---

## 🖥️ Kullanım

### Yönetici panelinde:
- Yeni kategoriler ekleyebelir veya mevcut kategorileri düzenleyebilirsiniz.
- Yeni ürün ekleyebilir, düzenleyebilirsiniz ayrıca yeni marka veya modeller de ekleyebilirsiniz.
- Müşterilerin vermiş olduğu siparişleri görüntüleyebilir ve sipariş durumlarını yönetebilirsiniz.
- Listelenen tedarikçilerden istediğinizi seçerek sunulan ürünler üzerinden ürün stoklarını güncelleyebilirsiniz.
- Listelenen faturası oluşturulmamış siparişler için fatura oluşturabilirsiniz ve oluşturduğunuz faturaları PDF olarak indirebilirsiniz.
- Raporları görüntüleyerek satış, stok gibi verileri inceleyebilirsiniz.

### Müşteri panelinde:
- Listelenen ürünleri inceleyebilir ve detaylarını görüntüleyebilirsiniz.
- Sepete ekleyebilir ve sepet üzerinde değişiklikler yaparak sipariş oluşturabilirsiniz.
- Hesabınızı yönetebilir ve geçmiş siparişlerinizi görüntüleyerek PDF olarak faturaları indirebilirsiniz.
- İletişim formu aracılığıyla geri bildirim gönderebilirsiniz.

---

## 🖼️ Ekran Görüntüleri
- Veri Tabanı Yapısı
<img width="1633" height="870" alt="Database ER Diyagram Yeni" src="https://github.com/user-attachments/assets/292191e0-c8e0-4f65-bb9b-a355b729975e" />

---

- Yönetici Paneli Ana Sayfası (Rapor Sayfası)
<img width="1919" height="1024" alt="rapor test verisiyle" src="https://github.com/user-attachments/assets/6129ed15-700b-40cc-9719-db2cd658e4c4" />

---

- Müşteri Paneli Ana Sayfası
<img width="1919" height="1020" alt="ana sayfa verili ilk" src="https://github.com/user-attachments/assets/9597a873-78b6-4958-bef1-06a0b7039325" />

---

- Fatura Dışa Aktarma Örneği
<img width="1919" height="1022" alt="fatura dışa aktarma" src="https://github.com/user-attachments/assets/dba10d99-0c0f-47f7-a1cb-e864fda85400" />
