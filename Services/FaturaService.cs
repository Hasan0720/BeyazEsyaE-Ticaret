// Faturala işlemlerinin yönetimi ve PDF olarak dışa aktarma işlevselliği sağlar.

using iTextSharp.text; // PDF dokümanı oluşturmak için iTextSharp kütüphanesi
using iTextSharp.text.pdf; // PDF özelliklerini kullanmak için iTextSharp kütüphanesi
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Services
{
    // Fatura işlemlerinin yönetimini sağlayan service sınıfıdır.
    public class FaturaService
    {
        ETicaretEntities db = new ETicaretEntities();

        // Seçilen fatura ve sipariş ID'lerine göre fatura ve sipariş bilgilerini veri tabanından çekerek PDF dosyası üretir.
        public byte[] FaturaDisaAktar(int faturaId)
        {
            // Fatura bilgilerini bulur.
            var faturalar = db.tbl_fatura.FirstOrDefault(fatura => fatura.fatura_ID == faturaId);
            if (faturalar == null)
            {
                return null;
            }

            // Sipariş bilgilerini bulur.
            var siparisler = db.tbl_siparis.FirstOrDefault(siparis => siparis.siparis_ID == faturalar.siparis_ID);
            if (siparisler == null)
            {
                return null;
            }

            // MemoryStream kullanarak PDF dosyasını bellekte oluşturur.
            using (var ms = new MemoryStream())
            {
                // PDF belgesini A4 boyutunda oluşturur ve kenar boşluklarını ayarlar.
                Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Türkçe karakter desteği için font ayarları
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf");
                BaseFont basefont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                // Farklı metin stilleri için font nesneleri tanımlar.
                var ustBaslikFont = new Font(basefont, 16, Font.BOLD, BaseColor.BLACK);
                var normalFont = new Font(basefont, 12, Font.NORMAL, BaseColor.BLACK);
                var baslikFont = new Font(basefont, 12, Font.BOLD, BaseColor.BLACK);

                // Üstbilgi için iki sütunlu bir yapıda bir tablo oluşturur.
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 60, 40 });

                // Solda firma bilgilerini içeren bir hücre oluşturur ve tabloya ekler.
                PdfPCell firmaCell = new PdfPCell();
                firmaCell.Border = 0;
                firmaCell.AddElement(new Paragraph("BEYAZ EŞYA E-TİCARET SİSTEMİ", ustBaslikFont));
                firmaCell.AddElement(new Paragraph("Adres: Antalya, Türkiye", normalFont));
                firmaCell.AddElement(new Paragraph("Tel: +90 212 123 45 67", normalFont));
                firmaCell.AddElement(new Paragraph("beyazvitrin0@gmail.com", normalFont));
                headerTable.AddCell(firmaCell);

                // Sağda fatura bilgilerini içeren bir hücre oluşturur ve tabloya ekler.
                PdfPCell faturaCell = new PdfPCell();
                faturaCell.Border = 0;
                faturaCell.AddElement(new Paragraph($"FATURA", ustBaslikFont));
                faturaCell.AddElement(new Paragraph($"Fatura No: {faturalar.fatura_ID}", normalFont));
                faturaCell.AddElement(new Paragraph($"Tarih: {faturalar.fatura_tarihi:dd.MM.yyyy}", normalFont));
                headerTable.AddCell(faturaCell);

                // Üstbilgi tablosunu belgeye ekler.
                doc.Add(headerTable);
                doc.Add(new Paragraph(" "));

                // Müşteri bilgilerini belgeye ekler.
                Paragraph alici = new Paragraph("ALICI BİLGİLERİ", baslikFont);
                doc.Add(alici);
                doc.Add(new Paragraph(siparisler.teslimat_adresi, normalFont));
                doc.Add(new Paragraph($"{siparisler.tbl_kullanici.ad} {siparisler.tbl_kullanici.soyad}", normalFont));
                doc.Add(new Paragraph(siparisler.tbl_kullanici.telefon_no, normalFont));
                doc.Add(new Paragraph(siparisler.tbl_kullanici.email, normalFont));
                doc.Add(new Paragraph(" ", normalFont));

                // Ürünleri göstermek için 5 sütunlu bir tablo oluşturur.
                PdfPTable urunTable = new PdfPTable(5);
                urunTable.WidthPercentage = 100;
                urunTable.SetWidths(new float[] { 30, 7, 18, 20, 25 });

                // Ürün tablosundaki sütun başlıklarını belirler ve tabloya ekler.
                string[] headers = { "Ürün Adı", "Adet", "Birim Fiyat", "KDV Tutarı", "Toplam (KDV Dahil)" };
                foreach (var h in headers)
                {
                    var cell = new PdfPCell(new Phrase(h, baslikFont))
                    {
                        BackgroundColor = new BaseColor(230, 230, 230),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5,
                        NoWrap = true
                    };
                    urunTable.AddCell(cell);
                }

                decimal araToplam = 0;

                // Her sipariş detay kaydı için ürün tablosuna yeni bir satır eklenir.
                foreach (var detay in siparisler.tbl_siparisDetay)
                {
                    decimal toplam = detay.adet * detay.birim_fiyat;
                    decimal kdvOran = 0.20m;
                    decimal kdvTutari = toplam * kdvOran;
                    araToplam += toplam;

                    urunTable.AddCell(new PdfPCell(new Phrase(detay.tbl_urun.urun_adi, normalFont)) { Padding = 5 });
                    urunTable.AddCell(new PdfPCell(new Phrase(detay.adet.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });
                    urunTable.AddCell(new PdfPCell(new Phrase(detay.birim_fiyat.ToString("C"), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5, NoWrap = true });
                    urunTable.AddCell(new PdfPCell(new Phrase(kdvTutari.ToString("C"), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5, NoWrap = true });
                    urunTable.AddCell(new PdfPCell(new Phrase((toplam + kdvTutari).ToString("C"), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5, NoWrap = true });
                }

                // ürün tablosunu belgeye ekler.
                doc.Add(urunTable);

                // KDV ve genel toplamı hesaplar.
                decimal kdvToplam = araToplam * 0.20m;
                decimal genelToplam = araToplam + kdvToplam;

                // Toplam fiyat alanlarını göstermek için yeni bir tablo oluşturur.
                doc.Add(new Paragraph(" "));
                PdfPTable totalsTable = new PdfPTable(2);
                totalsTable.WidthPercentage = 40;
                totalsTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalsTable.PaddingTop = 5;

                totalsTable.AddCell(new PdfPCell(new Phrase("Ara Toplam", normalFont)) { BackgroundColor = new BaseColor(245, 245, 245) });
                totalsTable.AddCell(new PdfPCell(new Phrase(araToplam.ToString("C"), normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                totalsTable.AddCell(new PdfPCell(new Phrase("KDV Tutarı (%20)", normalFont)) { BackgroundColor = new BaseColor(245, 245, 245) });
                totalsTable.AddCell(new PdfPCell(new Phrase(kdvToplam.ToString("C"), normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                totalsTable.AddCell(new PdfPCell(new Phrase("Genel Toplam", normalFont)) { BackgroundColor = new BaseColor(220, 220, 220) });
                totalsTable.AddCell(new PdfPCell(new Phrase(genelToplam.ToString("C"), baslikFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

                // Toplam fiyat tablosunu belgeye ekler.
                doc.Add(totalsTable);

                doc.Add(new Paragraph(" "));

                // Altbilgi metni oluşturur.
                Paragraph footer = new Paragraph("Bizi tercih ettiğiniz için teşekkür ederiz.", baslikFont);
                footer.Alignment = Element.ALIGN_CENTER;

                // Altbilgiyi belgeye ekler.
                doc.Add(footer);

                // Belgeyi kapatır ve byte dizisi olarak döndürür.
                doc.Close();
                return ms.ToArray();
            }
        }
    }
}