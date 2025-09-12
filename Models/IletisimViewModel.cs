// İletişim formu aracılığıyla gönderilen verileri taşımak ve doğrulamak için kullanılan ViewModel sınıfıdır.
// Mesaj göndermek için email adresi geçerli formatta olmalıdır.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class IletisimViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad alanı en fazla 50 karakterden oluşabilir.")]
        public string Ad {  get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad alanı en fazla 50 karakterden oluşabilir.")]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [RegularExpression(@"^(?!.*\.\.)[a-z0-9._%/*\-+]+@[a-z.]+\.[a-z]{2,}$", ErrorMessage = "Lütfen geçerli formatta bir email adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mesaj alanı zorunludur.")]
        [MinLength(20, ErrorMessage = "Mesaj alanı en az 20 karakter olmalıdır.")]
        public string Mesaj { get; set; }
    }
}