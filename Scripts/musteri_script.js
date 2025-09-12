document.addEventListener("DOMContentLoaded", function () {
    console.log("Müşteri sayfası yüklendi.");

    // Sepet linki animasyonu (varsa kullanılabilir)
    const sepetLink = document.querySelector('a[href="/Musteri/Sepet"]');
    if (sepetLink) {
        sepetLink.addEventListener("click", function () {
            sepetLink.classList.add("clicked");
        });
    }

    // Navbar aktif sayfa işaretlemesi
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.navbar-nav .nav-link').forEach(link => {
        const href = link.getAttribute('href').toLowerCase();
        if (path.includes(href)) {
            link.classList.add("active");
        }
    });

    // Sepete ekle butonları stok kontrolü, stok olmadığı durumda sidebar menü açılamayacak
    document.querySelectorAll(".sepete-ekle-btn").forEach(btn => {
        const stok = parseInt(btn.getAttribute("data-stok")) || 0;
        if (stok <= 0) {
            btn.setAttribute("data-no-offcanvas", "1");
            btn.removeAttribute("data-bs-toggle");
            btn.removeAttribute("data-bs-target");
        }
    });

    // Sayfa açıldığında sepetteki ürünlerin çekilmesi ve herhangi bir değişiklikte sidebarın güncellenmesi
    fetch('/Sepet/GetSepetUrunler')
        .then(res => res.json())
        .then(data => {
            guncelleSidebar(data.urunler);
        });


    // Sayfa açıldığında sepet rozetininin güncellenmesi
    fetch("/Sepet/RozetAdet")
        .then(response => response.json())
        .then(data => {
            const rozet = document.getElementById('cart-item-count');
            if (rozet && data.adet !== undefined) {
                rozet.textContent = data.adet;
            }
        });

    // Sepet sayfası olup olmadığının kontrol edilmesi eğer sepet ise ekstra eventler eklenecek
    if (window.location.pathname.toLowerCase().includes("/sepet")) {
        aktifSepetSayfasiEventleriEkle();
    }

});

// Adet kontrol fonksiyonu, 
function adetKontrol(input, stokAdet) {
    let value = parseInt(input.value);

    // Negatif veya 0 girilirse 1 olacak
    if (isNaN(value) || value < 1) {
        value = 1;
    }

    // Eğer stok 2’den büyük olsa bile maksimum 2 olacak
    let maxAdet = stokAdet > 2 ? 2 : stokAdet;

    // Maksimum sınırı aşsa bile mkasimum sınıra düşür
    if (value > maxAdet) {
        value = maxAdet;
    }

    input.value = value;
}

// Sepete ekleme işlemleri
document.addEventListener("click", function (e) {
    if (e.target.classList.contains("sepete-ekle-btn")) {
        let urunId = e.target.getAttribute("data-urun-id");
        let stokAdet = parseInt(e.target.getAttribute("data-stok")) || 0;

        // Stok yoksa hiçbir yerel DOM değişikliği yapılmıyor
        if (stokAdet > 0) {
            let mevcutUrun = document.querySelector(`#sepet-icerik .sepet-urun[data-urun-id="${urunId}"]`);

            if (mevcutUrun) {
                // Ürün zaten varsa adedi arttırıyor
                let input = mevcutUrun.querySelector(".adet-input");
                let mevcutAdet = parseInt(input.value);
                let maxAdet = stokAdet > 2 ? 2 : stokAdet;

                if (mevcutAdet < maxAdet) {
                    input.value = mevcutAdet + 1;
                } else {
                    alert("Bu üründen en fazla " + maxAdet + " adet alabilirsiniz.");
                }
                adetKontrol(input, stokAdet);
            } else {
                // Yeni ürün için blok ekleme
                let maxAdet = stokAdet > 2 ? 2 : stokAdet;
                let urunHTML = `
          <div class="sepet-urun d-flex justify-content-between align-items-center mb-2" data-urun-id="${urunId}">
            <span>Ürün ${urunId}</span>
            <input type="number" value="1" class="form-control adet-input" style="width:60px;"
                   min="1" max="${maxAdet}"
                   onchange="adetKontrol(this, ${stokAdet})" oninput="adetKontrol(this, ${stokAdet})" />
          </div>`;
                document.querySelector("#sepet-icerik").insertAdjacentHTML("beforeend", urunHTML);
            }
        }

        // Sunucuya fetch ile gönderme
        const adetInput = e.target.closest('.urun-kart')?.querySelector('.adet-input');
        const adet = adetInput ? (parseInt(adetInput.value) || 1) : 1;

        fetch('/Sepet/SepeteEkle', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ urunId, adet })
        })
            .then(res => res.json())
            .then(data => {
                if (data.basarili) {
                    if (Array.isArray(data.urunler)) {
                        guncelleSidebar(data.urunler);
                    }
                    if (data.mesaj) {
                        gosterAjaxMesaj(data.mesaj, data.mesajTipi || 'success');
                    }
                } else {
                    gosterAjaxMesaj(data.mesaj || "Sepete eklenirken hata oluştu.", data.mesajTipi || 'danger');
                }
            })
            .catch(() => {
                gosterAjaxMesaj("Sepete ekleme sırasında beklenmeyen bir hata oluştu.", "danger");
            });

    }
});

// Sepet sidebar menü güncelleme fonksiyonu
function guncelleSidebar(urunler) {
    const sepetIcerik = document.getElementById('sepet-icerik');
    const toplamFiyatSpan = document.getElementById('sepet-toplam-fiyat');
    const rozet = document.getElementById('cart-item-count');

    sepetIcerik.innerHTML = '';
    let toplam = 0;
    let toplamAdet = 0;

    // Sepete yeni eklenen ürünleri sidebar menüye ekleme
    urunler.forEach(u => {
        const urunDiv = document.createElement('div');
        urunDiv.className = 'sepet-urun-item d-flex align-items-start border-bottom py-2 mb-2 position-relative';
        urunDiv.setAttribute('data-urunid', u.urun_ID);

        urunDiv.innerHTML = `
            <img src="${u.Gorsel}" alt="${u.UrunAdi}"
                 style="width: 50px; height: 50px; object-fit: contain;"
                 class="me-2" />
            <div class="flex-grow-1">
                <div class="d-flex justify-content-between align-items-start">
                    <span class="fw-bold small">${u.UrunAdi}</span>
                    <button class="btn btn-sm btn-outline-danger p-1 urun-sil-btn"
                            data-urunid="${u.urun_ID}" title="Ürünü Kaldır">
                        <i class="fa fa-trash"></i>
                    </button>
                </div>
                <div class="text-muted small">Birim Fiyat:
                    ${u.BirimFiyat.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}
                </div>
                <div class="text-muted small">KDV (%20):
                    ${u.KdvTutar.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}
                </div>
                <div class="urun-toplam-fiyat fw-bold mt-1">
                    ${u.ToplamFiyat.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}
                </div>
                <input type="number"
                       class="form-control form-control-sm adet-input mt-1"
                       value="${u.Adet}" min="1" max="2"
                       data-urunid="${u.urun_ID}"
                       data-birimfiyat="${u.BirimFiyat}"
                       style="width: 60px;" />
            </div>
        `;

        sepetIcerik.appendChild(urunDiv);

        toplam += u.ToplamFiyat;
        toplamAdet += u.Adet;
    });

    if (urunler.length === 0) {
        sepetIcerik.innerHTML = '<p class="text-muted bos-sepet">Sepetinizde ürün bulunmamaktadır.</p>';
    }

    // Toplam fiyat ve rozet değerlerinin güncellenmesi
    toplamFiyatSpan.textContent = toplam.toLocaleString("tr-TR", { style: "currency", currency: "TRY" });
    if (rozet) rozet.textContent = toplamAdet;

    // Adet güncelleme işlemi
    document.querySelectorAll('#sepet-icerik .adet-input').forEach(input => {
        input.addEventListener('input', function () {
            let adet = parseInt(this.value) || 1;
            if (adet < 1) adet = 1;

            const birimFiyat = parseFloat(this.getAttribute('data-birimfiyat'));
            const toplamFiyatDiv = this.closest('.d-flex').querySelector('.urun-toplam-fiyat');
            toplamFiyatDiv.textContent = (birimFiyat * adet).toLocaleString("tr-TR", { style: "currency", currency: "TRY" });

            // Veritabanına adet güncellemesini gönder
            const urunID = parseInt(this.getAttribute("data-urunid"));

            fetch("/Sepet/GuncelleAdet", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ urunId: urunID, adet: adet })
            })
                .then(res => res.json())
                .then(r => {
                    if (r && r.mesaj) {
                        gosterAjaxMesaj(r.mesaj, r.mesajTipi || "success");
                    }
                    return fetch("/Sepet/GetSepetUrunler");
                })
                .then(res => res.json())
                .then(data => {
                    guncelleSidebar(data.urunler);
                })
                .catch(() => {
                    gosterAjaxMesaj("Adet güncellenemedi.", "danger");
                });

        });
    });

    // Ürün silme işlemi
    document.querySelectorAll(".urun-sil-btn").forEach(btn => {
        btn.addEventListener("click", function () {
            const urunID = parseInt(this.dataset.urunid);
            fetch("/Sepet/Sil", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ urunId: urunID })
            })
                .then(res => res.json())
                .then(json => {
                    if (json.basarili) {
                        if (Array.isArray(json.sepet)) {
                            guncelleSidebar(json.sepet);
                        }
                        if (json.mesaj) {
                            gosterAjaxMesaj(json.mesaj, json.mesajTipi || "info");
                        }
                    } else {
                        gosterAjaxMesaj(json.mesaj || "Ürün silinemedi.", json.mesajTipi || "danger");
                    }
                })
                .catch(() => {
                    gosterAjaxMesaj("Ürün silinirken hata oluştu.", "danger");
                });

        });
    });
}

// Sepet sayfasına özel eventleri ekleyen fonksiyon
function aktifSepetSayfasiEventleriEkle() {
    // Sepet sayfasında adet input değişimi
    document.querySelectorAll('.sepet-urun .adet-input').forEach(input => {
        input.addEventListener('input', function () {
            let stokAdet = parseInt(this.getAttribute("data-stok")) || 2;
            adetKontrol(this, stokAdet);

            const adet = parseInt(this.value);

            const urunID = this.dataset.urunid;
            const kart = this.closest('.sepet-urun');
            const birimFiyat = parseFloat(kart.querySelector('.urun-birim-fiyat').dataset.birimfiyat);

            // Toplam fiyatı güncelle
            const toplamFiyat = birimFiyat * adet;
            kart.querySelector('.urun-toplam-fiyat').textContent = toplamFiyat.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' });

            // Sunucuya fetch ile gönderme
            fetch('/Sepet/GuncelleAdet', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ urunId: parseInt(urunID), adet: adet })
            })
                .then(res => res.json())
                .then(r => {
                    if (r && r.mesaj) {
                        gosterAjaxMesaj(r.mesaj, r.mesajTipi || "success");
                    }
                    sepetOzetiniGuncelle();
                })
                .catch(() => {
                    gosterAjaxMesaj("Adet güncellenemedi.", "danger");
                });

        });
    });

    // Sepet sayfasında ürün silme
    document.querySelectorAll('.sepet-urun .urun-sil-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const urunID = this.dataset.urunid;
            fetch('/Sepet/Sil', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ urunId: parseInt(urunID) })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.basarili) {
                        const kart = document.querySelector(`.sepet-urun[data-urunid="${urunID}"]`);
                        if (kart) kart.remove();

                        sepetOzetiniGuncelle();

                        if (data.mesaj) {
                            gosterAjaxMesaj(data.mesaj, data.mesajTipi || "info");
                        }

                        const kalanUrunler = document.querySelectorAll('.sepet-urun');
                        if (kalanUrunler.length === 0) {
                            const urunlerKolon = document.querySelector('.col-md-8');
                            const ozetKolon = document.querySelector('.col-md-4');
                            if (urunlerKolon) urunlerKolon.remove();
                            if (ozetKolon) ozetKolon.remove();

                            const mesaj = document.createElement('div');
                            mesaj.className = 'alert alert-info text-center mt-4';
                            mesaj.innerHTML = '<i class="fa fa-info-circle me-2"></i> Sepetinizde ürün bulunmamaktadır.';
                            const container = document.querySelector('.row')?.parentElement;
                            if (container) container.appendChild(mesaj);
                        }
                    } else {
                        gosterAjaxMesaj(data.mesaj || "Ürün silinemedi.", data.mesajTipi || "danger");
                    }
                })
                .catch(() => {
                    gosterAjaxMesaj("Ürün silinirken hata oluştu.", "danger");
                });

        });
    });

}

// Sipariş özeti kısmını güncelleme fonksiyonu
function sepetOzetiniGuncelle() {
    let toplam = 0;
    document.querySelectorAll('.sepet-urun .adet-input').forEach(input => {
        const adet = parseInt(input.value) || 1;
        const birimFiyat = parseFloat(input.closest('.sepet-urun').querySelector('.urun-birim-fiyat').dataset.birimfiyat);
        toplam += adet * birimFiyat;
    });

    const kdv = toplam * 0.2;
    const genelToplam = toplam + kdv;

    // Sayfadaki alanlar güncelleniyor
    document.getElementById('araToplam').textContent = toplam.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' });
    document.getElementById('KDV').textContent = kdv.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' });
    document.getElementById('genelToplam').textContent = genelToplam.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' });

    // Navbar rozetinin fetch edilerek güncellenmesi
    fetch("/Sepet/RozetAdet")
        .then(res => res.json())
        .then(data => {
            const rozet = document.getElementById("cart-item-count");
            if (rozet) rozet.textContent = data.adet;
        });
}

// Adet input değişikliklerinde fiyat alanlarının tekrar hesaplanarak güncellenmesi
document.addEventListener("input", function (e) {
    if (e.target.classList.contains("adet-input")) {
        const adet = parseInt(e.target.value) || 1;
        const container = e.target.closest(".col-4");

        const birimFiyat = parseFloat(container.querySelector(".urun-birim-fiyat")
            .getAttribute("data-birimfiyat"));

        const kdvOrani = 0.20;
        const kdvTutar = birimFiyat * adet * kdvOrani;
        const kdvDahilToplam = (birimFiyat * adet) + kdvTutar;

        container.querySelector(".urun-kdv-tutar").textContent = "₺" + kdvTutar.toLocaleString("tr-TR", { minimumFractionDigits: 2 });
        container.querySelector(".urun-toplam-fiyat").textContent = "₺" + kdvDahilToplam.toLocaleString("tr-TR", { minimumFractionDigits: 2 });
    }
});

// Stok kontrolünün tüm adet input alanlarına ekleme işlemi
document.querySelectorAll(".adet-input").forEach(input => {
    const stokAdet = parseInt(input.getAttribute("data-stok")) || 2;
    input.addEventListener("input", function () {
        adetKontrol(this, stokAdet);
    });
    input.addEventListener("change", function () {
        adetKontrol(this, stokAdet);
    });
});

// Siparişi tamamlama sayfası için özel işlemler
document.addEventListener("DOMContentLoaded", function () {
    const yeniAdresInput = document.getElementById("YeniAdresInput");
    if (!yeniAdresInput) return; // bu sayfada değilsek çık

    const adresRadios = document.querySelectorAll('input[name="KayitliAdresSeciliMi"]');
    const tamamlaBtn = document.getElementById("siparisiTamamlaBtn");
    const sozlesme1 = document.getElementById("sozlesme1");
    const sozlesme2 = document.getElementById("sozlesme2");
    const sozlesme3 = document.getElementById("sozlesme3");
    const yeniAdresHata = document.querySelector('[data-valmsg-for="YeniAdres"]'); // MVC validasyon alanı

    // Kayıtlı / yeni adres görünürlüğü
    function toggleAdres() {
        const secili = document.querySelector('input[name="KayitliAdresSeciliMi"]:checked');
        const yeniMi = secili && (secili.value || "").toString().toLowerCase() === "false";

        if (yeniMi) {
            yeniAdresInput.style.display = "block";
        } else {
            yeniAdresInput.style.display = "none";
            // Kayıtlı adrese dönüldüğünde input alanının ve eski hata mesajının temizlenmesi
            yeniAdresInput.value = "";
            if (yeniAdresHata) {
                yeniAdresHata.textContent = "";
                yeniAdresHata.classList.remove("field-validation-error");
                yeniAdresHata.classList.add("field-validation-valid");
            }
        }
    }

    adresRadios.forEach(r => r.addEventListener("change", toggleAdres));
    toggleAdres(); // ilk yüklemede varsayılan görünüm

    // Sözleşmeler işaretli mi kontrolü
    function checkSozlesmeler() {
        const hepsi = !!(sozlesme1 && sozlesme1.checked) &&
            !!(sozlesme2 && sozlesme2.checked) &&
            !!(sozlesme3 && sozlesme3.checked);
        if (tamamlaBtn) tamamlaBtn.disabled = !hepsi;
    }

    sozlesme1 && sozlesme1.addEventListener("change", checkSozlesmeler);
    sozlesme2 && sozlesme2.addEventListener("change", checkSozlesmeler);
    sozlesme3 && sozlesme3.addEventListener("change", checkSozlesmeler);
    checkSozlesmeler();
});

// Sözleşme linklerini modalda açma
document.querySelectorAll(".sozlesme-link").forEach(link => {
    link.addEventListener("click", function (e) {
        e.preventDefault();
        const title = this.dataset.title;
        const contentUrl = this.dataset.content;

        document.getElementById("sozlesmeModalLabel").textContent = title;

        fetch(contentUrl)
            .then(res => res.text())
            .then(html => {
                document.getElementById("sozlesmeModalBody").innerHTML = html;
                new bootstrap.Modal(document.getElementById("sozlesmeModal")).show();
            })
            .catch(() => {
                document.getElementById("sozlesmeModalBody").innerHTML = "<p>İçerik yüklenemedi.</p>";
            });
    });
});

// Şifre alanlarında göz ikonu ile göster gizle özelliği ekleme
document.querySelectorAll('.toggle-password').forEach(btn => {
    btn.addEventListener('click', function () {
        const targetId = this.getAttribute('data-target');
        const input = document.getElementById(targetId);
        const icon = this.querySelector('i');

        if (input.type === "password") {
            input.type = "text";
            icon.classList.remove("fa-eye");
            icon.classList.add("fa-eye-slash");
        } else {
            input.type = "password";
            icon.classList.remove("fa-eye-slash");
            icon.classList.add("fa-eye");
        }
    });
});

// Şifre sıfırlama modalını otomatik açma
document.addEventListener("DOMContentLoaded", function () {
    var el = document.getElementById('resetFlags');
    if (!el) return;

    var t = el.dataset.token || "";
    var shouldOpen = (el.dataset.openreset === "true");

    if (shouldOpen && t) {
        var tokenInput = document.getElementById('resetToken');
        if (tokenInput) tokenInput.value = t;

        var modalEl = document.getElementById('sifreSifirlaModal');
        if (modalEl) {
            var m = new bootstrap.Modal(modalEl);
            m.show();
        }
    }
});

// Kategori sekmeleriyle filtreleme
document.querySelectorAll('.kategori-btn').forEach(btn => {
    btn.addEventListener('click', function (e) {
        e.preventDefault();

        const kategoriId = this.getAttribute('data-kategori');
        const url = new URL(window.location.href);

        // Kategori seçimi
        if (kategoriId) {
            url.searchParams.set("kategoriID", kategoriId);
        } else {
            url.searchParams.delete("kategoriID");
        }

        // Marka / fiyat varsa onları koru
        window.location.href = url.toString();
    });
});

// Ürünler sayfası popover filtre yapısı
document.addEventListener("DOMContentLoaded", function () {
    const urunFiltreBtn = document.getElementById("urunFiltreBtn");

    if (urunFiltreBtn) {
        const baseUrl = urunFiltreBtn.dataset.baseUrl;
        const template = document.getElementById("urunFiltreTemplate");

        const cloneTemplate = () => {
            const wrapper = document.createElement("div");
            wrapper.innerHTML = template.innerHTML;
            return wrapper;
        };

        const popover = new bootstrap.Popover(urunFiltreBtn, {
            html: true,
            content: cloneTemplate,
            placement: "bottom",
            sanitize: false,
            trigger: "manual"
        });

        urunFiltreBtn.addEventListener("click", function () {
            const isOpen = urunFiltreBtn.getAttribute("aria-describedby");
            if (isOpen) {
                popover.hide();
            } else {
                popover.show();
            }
        });

        document.addEventListener("click", function (e) {
            const popoverEl = document.querySelector(".popover");
            if (!popoverEl || popoverEl.contains(e.target) || urunFiltreBtn.contains(e.target)) return;
            popover.hide();
        });

        urunFiltreBtn.addEventListener("shown.bs.popover", function () {
            const popoverEl = document.querySelector(".popover");
            if (!popoverEl) return;

            const applyBtn = popoverEl.querySelector('[data-act="apply"]');
            const clearBtn = popoverEl.querySelector('[data-act="clear"]');

            if (applyBtn) {
                applyBtn.addEventListener("click", function () {
                    const selectedBrands = [...popoverEl.querySelectorAll('input[name="marka"]:checked')].map(cb => cb.value);
                    const minPrice = popoverEl.querySelector('input[name="minFiyat"]').value;
                    const maxPrice = popoverEl.querySelector('input[name="maxFiyat"]').value;

                    const url = new URL(baseUrl, window.location.origin);
                    const currentParams = new URLSearchParams(window.location.search);
                    if (currentParams.has("kategoriID")) {
                        url.searchParams.set("kategoriID", currentParams.get("kategoriID"));
                    }

                    // Marka/fiyat parametrelerini ekle
                    selectedBrands.forEach(brandId => url.searchParams.append("marka", brandId));
                    if (minPrice) url.searchParams.set("minFiyat", minPrice);
                    if (maxPrice) url.searchParams.set("maxFiyat", maxPrice);

                    window.location.href = url.toString();
                });
            }

            if (clearBtn) {
                clearBtn.addEventListener("click", function () {
                    const url = new URL(baseUrl, window.location.origin);

                    // 🔑 Eğer kategori filtresi seçiliyse onu koru
                    const currentParams = new URLSearchParams(window.location.search);
                    if (currentParams.has("kategoriID")) {
                        url.searchParams.set("kategoriID", currentParams.get("kategoriID"));
                    }

                    window.location.href = url.toString();
                });
            }
        });
    }
});

// Lightbox açma
function openLightbox(image) {
    const lightbox = document.getElementById("lightbox");
    const lightboxImage = document.getElementById("lightboxImage");

    // Tıklanan resmi lightbox içine yükle
    lightboxImage.src = image.src;

    // Lightbox göster
    lightbox.style.display = "flex";
}

// Lightbox kapama
function closeLightbox() {
    const lightbox = document.getElementById("lightbox");
    lightbox.style.display = "none";
}

// ESC tuşu ile kapatma
document.addEventListener("keydown", function (event) {
    if (event.key === "Escape") {
        closeLightbox();
    }
});

// Lightbox dışına tıklayınca kapatma
document.addEventListener("click", function (event) {
    const lightbox = document.getElementById("lightbox");
    if (event.target === lightbox) {
        closeLightbox();
    }
});

// Sipariş geçmişi sayfası sipariş durumuna göre filtreleme işlemi
document.addEventListener("DOMContentLoaded", function () {
    if (!window.location.pathname.toLowerCase().includes("/hesap/siparisgecmisi")) {
        return;
    }

    const filterButtons = document.querySelectorAll(".filter-btn");
    const siparisKartlari = document.querySelectorAll("#siparisListesi > .col-12");
    const bosMesaj = document.getElementById("bosKategoriMesaji");

    filterButtons.forEach(btn => {
        btn.addEventListener("click", function () {
            filterButtons.forEach(b => b.classList.remove("active"));
            this.classList.add("active");

            const filter = this.dataset.filter;
            let visibleCount = 0;

            siparisKartlari.forEach(kart => {
                const durum = kart.dataset.durum;

                if (filter === "Tümü" || durum === filter) {
                    kart.style.display = "block";
                    visibleCount++;
                } else {
                    kart.style.display = "none";
                }
            });

            if (visibleCount === 0) {
                bosMesaj.classList.remove("d-none");
            } else {
                bosMesaj.classList.add("d-none");
            }
        });
    });
});

// Ana sayfadaki ürün corrousel yapılarını responsive olarak gruplama
document.addEventListener("DOMContentLoaded", function () {
    function groupCarouselItems(carouselId) {
        let width = window.innerWidth;
        let itemsPerSlide = 3; // varsayılan masaüstü

        if (width < 576) {
            itemsPerSlide = 1; // mobilde
        } else if (width < 992) {
            itemsPerSlide = 2; // tabletlerde
        }

        let carouselInner = document.querySelector(`#${carouselId} .carousel-inner`);
        if (!carouselInner) return;

        let products = Array.from(carouselInner.querySelectorAll(".col-12.col-sm-6.col-lg-4"));
        if (products.length === 0) return;

        carouselInner.innerHTML = "";

        for (let i = 0; i < products.length; i += itemsPerSlide) {
            let chunk = products.slice(i, i + itemsPerSlide);
            let itemDiv = document.createElement("div");
            itemDiv.className = "carousel-item" + (i === 0 ? " active" : "");

            let rowDiv = document.createElement("div");
            rowDiv.className = "row";

            chunk.forEach(p => rowDiv.appendChild(p));
            itemDiv.appendChild(rowDiv);
            carouselInner.appendChild(itemDiv);
        }
    }

    // Tüm carrouselları yeniden gruplama
    function applyGrouping() {
        groupCarouselItems("oneCikanCarousel");
        groupCarouselItems("cokSatanCarousel");
        groupCarouselItems("dusukStokCarousel");
        groupCarouselItems("yeniGelenCarousel");
    }

    applyGrouping();
    window.addEventListener("resize", applyGrouping);
});

// Şifre sıfırlama veya hatalı girişlerde modal açma
$(document).ready(function () {
    var openForgot = '@Request.QueryString["openForgot"]';
    var hasError = '@(TempData["ResetError"] != null ? "true" : "false")';

    if (openForgot === "1" || hasError === "true") {
        var myModal = new bootstrap.Modal(document.getElementById('sifremiUnuttumModal'));
        myModal.show();
    }
});

// AJAX üzerinden gelen mesajları kullanıcıya gösteren fonksiyon
function gosterAjaxMesaj(mesaj, mesajTipi) {
    if (!mesaj) return;

    const container = document.getElementById('ajax-mesaj-container');
    if (!container) {
        console.error('AJAX mesajları için "#ajax-mesaj-container" ID\'li element bulunamadı.');
        return;
    }

    // Mesaj tipine göre doğru ikonu seçimi
    const ikonlar = {
        success: 'fa-solid fa-check-circle',
        info: 'fa-solid fa-circle-info',
        warning: 'fa-solid fa-triangle-exclamation',
        danger: 'fa-solid fa-circle-xmark'
    };
    const ikonClass = ikonlar[mesajTipi] || 'fa-solid fa-bell'; // Varsayılan ikon

    // Bootstrapt alert HTML şablonu
    const alertHTML = `
        <div class="alert alert-${mesajTipi} alert-dismissible fade show" role="alert">
            <i class="${ikonClass} me-2"></i> ${mesaj}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', alertHTML);

    // Otomatik kapatma mekanizması
    const sonAlert = container.lastElementChild;
    setTimeout(() => {
        if (sonAlert) {
            try {
                const bsAlert = new bootstrap.Alert(sonAlert);
                bsAlert.close();
            } catch (error) {
                sonAlert.remove();
            }
        }
    }, 5000);
}

// AJAX tabanlı dinamik alrtlerin otomatik kapanması
setTimeout(function () {
    var alerts = document.querySelectorAll('.dinamik-alert');
    alerts.forEach(function (alert) {
        var bsAlert = new bootstrap.Alert(alert);
        bsAlert.close();
    });
}, 5000);

// Fatura görüntüle butonuna tıklandığında görüntülenecek başarı mesajı ve  otomatik kapanması işlemi
document.addEventListener('click', function (e) {
    const a = e.target.closest('.faturayi-goruntule-btn');
    if (!a) return;
    setTimeout(function () {
        gosterAjaxMesaj("Fatura dışa aktarıldı.", "success");
    }, 5000);
});