// Müşteri hesap bilgileri sayfasında (profil görüntüleme ve güncelleme) kullanılan ViewModel sınıfıdır.
// Güncelleme için email adresi ve telefon alanları geçerli formatta olmalıdır.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class HesapViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad alanı en fazla 50 karakterden oluşabilir.")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad alanı en fazla 50 karakterden oluşabilir.")]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [RegularExpression(@"^(?!.*\.\.)[a-z0-9._%/*\-+]+@[a-z.]+\.[a-z]{2,}$", ErrorMessage = "Lütfen geçerli formatta bir email adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        [RegularExpression(@"^0[0-9]{10}$", ErrorMessage = "Telefon numarası 0 ile başlamalı ve 11 haneli olmalıdır.")]
        public string Telefon { get; set; }

        [Required(ErrorMessage = "Adres alanı zorunludur.")]
        public string Adres { get; set; }
    }
}