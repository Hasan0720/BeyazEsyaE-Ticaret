// Kullanıcı giriş işlemleri için gerekli verileri taşımak ve doğrulamak amacıyla kullanılan ViewModel sınıfıdır.
// Giriş için email adresi geçerli formatta olmalıdır

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class GirisViewModel
    {
        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [RegularExpression(@"^(?!.*\.\.)[a-z0-9._%/*\-+]+@[a-z.]+\.[a-z]{2,}$", ErrorMessage = "Lütfen geçerli formatta bir email adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Şifreniz en az 8, en fazla 20 karakterden oluşabilir.")]
        public string Sifre { get; set; }
        public bool BeniHatirla { get; set; }
    }
}