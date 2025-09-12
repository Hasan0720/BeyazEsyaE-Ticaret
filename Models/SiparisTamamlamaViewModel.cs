// Müşteri panelindeki sipariş tamamlama sayfasında kullanılan ViewModel sınıfıdır.
// Bu model, siparişin son aşamasında kullanıcının fatura ve teslimat adresini onaylaması, varsa yeni bir adres girmesi ve sipariş özetini görmesi için gerekli tüm verileri içerir.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class SiparisTamamlamaViewModel
    {
        public string MusteriAdSoyad {  get; set; }
        public string Telefon {  get; set; }
        public string Adres { get; set;}

        [MinLength(10, ErrorMessage = "Yeni adres en az 10 karakterden oluşmalıdır.")]
        public string YeniAdres { get; set; }
        public bool KayitliAdresSeciliMi { get; set; }
        public List<SiparisUrunViewModel> Urunler { get; set; }
        public decimal ToplamTutar { get; set; }
    }
}