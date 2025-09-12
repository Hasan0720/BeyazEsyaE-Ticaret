document.addEventListener("DOMContentLoaded", function () {
    console.log("Yönetici paneli JavaScript yüklendi.");

    // Layout ve UI işlevleri
    const sidebar = document.getElementById("sidebar");
    const menuToggle = document.getElementById("menu-toggle");

    // Sidebar menü ve offcanvas menü açma / kapatma
    if (sidebar && menuToggle) {
        menuToggle.addEventListener("click", function (event) {
            event.stopPropagation();
            sidebar.classList.toggle("collapsed");
            console.log("Sidebar:", sidebar.classList.contains("collapsed") ? "collapsed" : "açık");
        });

        // Menü dışına tıklanınca menüyü kapatma işlemi
        document.addEventListener("click", function (event) {
            if (!sidebar.classList.contains("collapsed") &&
                !sidebar.contains(event.target) &&
                !menuToggle.contains(event.target)) {
                sidebar.classList.add("collapsed");
                console.log("Dışarı tıklandı, sidebar kapatıldı.");
            }
        });
    }

    // Kartlara tıklanınca highlight efektinin eklenmesi
    document.querySelectorAll(".card").forEach(card => {
        card.addEventListener("click", function () {
            card.style.backgroundColor = "#e9f2ff";
            setTimeout(() => card.style.backgroundColor = "white", 500);
        });
    });

    // Bootstrap alertlerini otomatik kapatma işlemi
    setTimeout(function () {
        document.querySelectorAll('.alert').forEach(function (alert) {
            try {
                new bootstrap.Alert(alert).close();
            } catch { alert.remove(); }
        });
    }, 5000);
});

// Ürün görsel yükleme işlemi
document.addEventListener("DOMContentLoaded", function () {
    const dropArea = document.getElementById('drop-area');
    const fileInput = document.getElementById('fileElem');
    const gorselInput = document.getElementById('gorsel');
    const previewContainer = document.getElementById('preview-container');
    const previewImage = document.getElementById('preview-image');
    const dropDefault = document.getElementById('drop-default');
    const removeBtn = document.getElementById('btn-remove-image');

    if (!dropArea || !fileInput || !gorselInput || !previewContainer || !previewImage || !dropDefault) return;

    dropArea.addEventListener('click', function (e) {
        if (!e.target.closest('#btn-remove-image')) fileInput.click();
    });

    ['dragenter', 'dragover'].forEach(ev =>
        dropArea.addEventListener(ev, e => { e.preventDefault(); dropArea.classList.add('hover'); })
    );
    ['dragleave', 'drop'].forEach(ev =>
        dropArea.addEventListener(ev, e => { e.preventDefault(); dropArea.classList.remove('hover'); })
    );

    dropArea.addEventListener('drop', e => uploadFile(e.dataTransfer.files[0]));
    fileInput.addEventListener('change', () => uploadFile(fileInput.files[0]));

    if (removeBtn) {
        removeBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            previewContainer.classList.add('d-none');
            dropDefault.classList.remove('d-none');
            gorselInput.value = "";
            previewImage.src = "#";
            fileInput.value = "";
        });
    }

    // Görseli yükleme fonksiyonu
    function uploadFile(file) {
        if (!file || !file.type.startsWith('image/')) return;
        const formData = new FormData();
        formData.append('file', file);

        fetch('/Urun/GorselYukle', { method: 'POST', body: formData })
            .then(res => res.ok ? res.text() : Promise.reject("Yükleme hatası"))
            .then(url => {
                previewImage.src = url;
                previewContainer.classList.remove('d-none');
                dropDefault.classList.add('d-none');
                gorselInput.value = url;
            })
            .catch(err => alert("Yükleme başarısız: " + err));
    }
});

// Modal yönetim fonksiyonu
function closeBsModal(id) {
    const el = document.getElementById(id);
    if (!el) return;
    const modal = bootstrap.Modal.getOrCreateInstance(el);
    modal.hide();

    document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
    document.body.classList.remove('modal-open');
    document.body.style.removeProperty('padding-right');
}

// Marka / model modalları kapandığında input alanlarının temizlenmesi
document.addEventListener("DOMContentLoaded", function () {
    ["markaModal", "modelModal"].forEach(id => {
        const modalEl = document.getElementById(id);
        if (!modalEl) return;
        modalEl.addEventListener("hidden.bs.modal", function () {
            modalEl.querySelectorAll("input[type=text], input[type=number], textarea").forEach(i => i.value = "");
            modalEl.querySelectorAll("select").forEach(s => s.selectedIndex = 0);
        });
    });
});

// Marka / model işlemleri
function markaEkle() {
    const ad = (document.getElementById("yeniMarkaAdi").value || "").trim();
    if (!ad) return gosterAjaxMesaj("Lütfen marka adı girin.", "warning");

    fetch('/Urun/MarkaEkleAjax', {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: `markaAdi=${encodeURIComponent(ad)}`
    })
        .then(res => res.json())
        .then(res => {
            const tip = res?.type || (res?.success ? "success" : "danger");
            if (res?.success) {
                document.getElementById("marka_ID").insertAdjacentHTML("beforeend", `<option value="${res.id}">${res.ad}</option>`);
                document.getElementById("marka_ID").value = res.id;
                document.getElementById("modelMarkaSelect").insertAdjacentHTML("beforeend", `<option value="${res.id}">${res.ad}</option>`);
                populateModels(res.id, null);
                document.getElementById("yeniMarkaAdi").value = "";
                closeBsModal("markaModal");
            }
            gosterAjaxMesaj(res?.message || "Marka eklenemedi.", tip);
        })
        .catch(() => gosterAjaxMesaj("Marka ekleme sırasında hata oluştu.", "danger"));
}

function modelEkle() {
    const ad = (document.getElementById("yeniModelAdi").value || "").trim();
    const markaID = parseInt(document.getElementById("modelMarkaSelect").value, 10) || 0;
    if (!markaID || !ad) return gosterAjaxMesaj("Lütfen marka seçin ve model adını girin.", "warning");

    fetch('/Urun/ModelEkleAjax', {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: `modelAdi=${encodeURIComponent(ad)}&markaID=${markaID}`
    })
        .then(res => res.json())
        .then(res => {
            const tip = res?.type || (res?.success ? "success" : "danger");
            if (res?.success) {
                document.getElementById("marka_ID").value = res.markaID;
                populateModels(res.markaID, res.id);
                document.getElementById("yeniModelAdi").value = "";
                document.getElementById("modelMarkaSelect").selectedIndex = 0;
                closeBsModal("modelModal");
            }
            gosterAjaxMesaj(res?.message || "Model eklenemedi.", tip);
        })
        .catch(() => gosterAjaxMesaj("Model ekleme sırasında hata oluştu.", "danger"));
}

// Seçilen markaya ait modelleri dropdown listeye yükleme işlemi
function populateModels(markaId, selectedModelId) {
    const modelDropdown = document.getElementById("model_ID");
    if (!markaId) {
        modelDropdown.innerHTML = '<option value="">-- Model Seçiniz --</option>';
        return;
    }
    fetch(`/Urun/ModelGetir?markaID=${markaId}`)
        .then(res => res.json())
        .then(data => {
            modelDropdown.innerHTML = '<option value="">-- Model Seçiniz --</option>';
            data.forEach(it => {
                const opt = document.createElement("option");
                opt.value = it.model_ID;
                opt.text = it.model_adi;
                modelDropdown.add(opt);
            });
            if (selectedModelId) modelDropdown.value = selectedModelId;
        })
        .catch(() => gosterAjaxMesaj("Modeller getirilemedi.", "danger"));
}

// Marka değişikliklerinde model dropdown listesinin otomatik olarak güncellenmesi
document.addEventListener("DOMContentLoaded", function () {
    const markaSel = document.getElementById("marka_ID");
    if (markaSel) {
        markaSel.addEventListener("change", function () {
            populateModels(this.value, null);
        });
    }
});

// Tedarik modülü
function hesaplaToplamMaliyet() {
    const adet1 = document.getElementById("adet");
    const birim1 = document.getElementById("birimMaliyet");
    const toplam1 = document.getElementById("toplamMaliyet");
    if (!adet1 || !birim1 || !toplam1) return;
    const adet = parseFloat((adet1.value || "0").replace(',', '.')) || 0;
    const birim = parseFloat((birim1.value || "0").replace(',', '.')) || 0;
    toplam1.value = (adet * birim).toFixed(2);
}
document.getElementById("adet")?.addEventListener("input", hesaplaToplamMaliyet);
document.addEventListener("DOMContentLoaded", hesaplaToplamMaliyet);

// Tablolar
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".genel-tablo-stil").forEach(function (table) {
        let firstRow = table.querySelector("tbody tr td");
        if (firstRow && firstRow.hasAttribute("colspan")) return;
        $(table).DataTable({
            pageLength: 10,
            lengthChange: false,
            ordering: false,
            searching: false,
            language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json" }
        });
    });
});

// Tedarikçi ürünleri sayfası popover filtre yapısı
document.addEventListener("DOMContentLoaded", function () {
    const tedarikFiltreBtn = document.getElementById("tedarikFiltreBtn");
    if (!tedarikFiltreBtn) return;
    const baseUrl = tedarikFiltreBtn.dataset.baseUrl;
    const template = document.getElementById("tedarikFiltreTemplate");

    const popover = new bootstrap.Popover(tedarikFiltreBtn, {
        html: true,
        content: () => template.innerHTML,
        placement: "bottom",
        sanitize: false,
        trigger: "manual"
    });

    tedarikFiltreBtn.addEventListener("click", () => {
        tedarikFiltreBtn.getAttribute("aria-describedby") ? popover.hide() : popover.show();
    });

    document.addEventListener("click", e => {
        const popoverEl = document.querySelector(".popover");
        if (!popoverEl || popoverEl.contains(e.target) || tedarikFiltreBtn.contains(e.target)) return;
        popover.hide();
    });

    tedarikFiltreBtn.addEventListener("shown.bs.popover", function () {
        const popoverEl = document.querySelector(".popover");
        if (!popoverEl) return;
        const applyBtn = popoverEl.querySelector('[data-act="apply"]');
        const clearBtn = popoverEl.querySelector('[data-act="clear"]');

        applyBtn?.addEventListener("click", function () {
            const selectedBrands = [...popoverEl.querySelectorAll('input[name="marka"]:checked')].map(cb => cb.value);
            const selectedCategory = popoverEl.querySelector('input[name="kategori"]:checked')?.value || "";
            const url = new URL(baseUrl, window.location.origin);
            selectedBrands.forEach(id => url.searchParams.append("marka", id));
            if (selectedCategory) url.searchParams.set("kategori", selectedCategory);
            window.location.href = url.toString();
        });

        clearBtn?.addEventListener("click", () => window.location.href = baseUrl);
    });
});


// Lightbox yapısı
function openLightbox(image) {
    document.getElementById("lightboxImage").src = image.src;
    document.getElementById("lightbox").style.display = "flex";
}
function closeLightbox() { document.getElementById("lightbox").style.display = "none"; }
document.addEventListener("keydown", e => { if (e.key === "Escape") closeLightbox(); });
document.addEventListener("click", e => { if (e.target === document.getElementById("lightbox")) closeLightbox(); });


// AJAX mesaj yönetimi ve alert mesajların otomatik kapanması işlemi
function gosterAjaxMesaj(mesaj, tip) {
    if (!mesaj) return;
    const container = document.getElementById('ajax-mesaj-container');
    if (!container) return console.error("Mesaj container bulunamadı.");
    const ikonlar = {
        success: 'fa-solid fa-check-circle',
        info: 'fa-solid fa-circle-info',
        warning: 'fa-solid fa-triangle-exclamation',
        danger: 'fa-solid fa-circle-xmark'
    };
    const ikonClass = ikonlar[tip] || 'fa-solid fa-bell';
    const alertHTML = `
        <div class="alert alert-${tip} alert-dismissible fade show" role="alert">
            <i class="${ikonClass} me-2"></i> ${mesaj}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`;
    container.insertAdjacentHTML('beforeend', alertHTML);

    const sonAlert = container.lastElementChild;
    setTimeout(() => {
        try { new bootstrap.Alert(sonAlert).close(); } catch { sonAlert.remove(); }
    }, 5000);
}

// Fatura dışa aktar butonuna tıklandığında görüntülenecek başarı mesajı ve otomatik kapanması işlemi
document.addEventListener('click', function (e) {
    const btn = e.target.closest('.fatura-disa-aktar-btn');
    if (!btn) return;
    setTimeout(() => gosterAjaxMesaj("Fatura dışa aktarıldı.", "success"), 5000);
});

// Ürün modülünde ekleme ve düzenleme sayfalarında açıklama alanı için madde ekleme işlemi
document.addEventListener('DOMContentLoaded', function () {
    const textarea = document.getElementById('aciklama');
    const bullet = '• ';
    const placeholderText = 'Ürün açıklaması...';
    const fakePlaceholder = bullet + placeholderText;

    // Placeholderı gösterme fonksiynou
    function showPlaceholder() {
        textarea.value = fakePlaceholder;
        textarea.style.color = 'gray';
        textarea.setAttribute('data-placeholder-active', 'true');
    }

    // İlk açılışta placeholder göster
    if (textarea.value.trim() === '' || textarea.value.trim() === bullet.trim()) {
        showPlaceholder();
    }

    // Metin girileceğinde placeholder kaybolur sadece madde işareti kalır
    textarea.addEventListener('focus', function () {
        if (this.getAttribute('data-placeholder-active') === 'true') {
            this.value = bullet;
            this.style.color = 'black';
            this.removeAttribute('data-placeholder-active');
        }
    });

    // Açıklama alanı boş bırakıldıysa placeholder geri gelsin
    textarea.addEventListener('blur', function () {
        if (this.value.trim() === '' || this.value.trim() === bullet.trim()) {
            showPlaceholder();
        }
    });

    // "Enter" tuşuna basıldığında madde işareti ekleyerek alt satıra geçiyor
    textarea.addEventListener('keydown', function (event) {
        if (event.key === 'Enter') {
            // Eğer placeholder aktifse Enter çalışmasın
            if (this.getAttribute('data-placeholder-active') === 'true') {
                event.preventDefault();
                return;
            }

            event.preventDefault();
            const cursorPosition = this.selectionStart;
            const value = this.value;

            const newValue = value.substring(0, cursorPosition) + '\n' + bullet + value.substring(cursorPosition);
            this.value = newValue;

            this.selectionStart = this.selectionEnd = cursorPosition + bullet.length + 1;
        }
    });

    // Kullanıcı yazmaya başladığında placeholder varsa kaldırılır
    textarea.addEventListener('input', function () {
        if (this.getAttribute('data-placeholder-active') === 'true' && this.value !== fakePlaceholder) {
            this.value = bullet; // sadece bullet kalsın
            this.style.color = 'black';
            this.removeAttribute('data-placeholder-active');
        }

        // tamamen silindiyse placeholder geri gelsin
        if (this.value.trim() === '') {
            showPlaceholder();
        }
    });

    // Form gönderiminde placeholder veya bullet temizlenir
    const form = textarea.closest('form');
    if (form) {
        form.addEventListener('submit', function () {
            let content = textarea.value;

            if (textarea.getAttribute('data-placeholder-active') === 'true') {
                textarea.value = '';
            } else {
                textarea.value = content.replace(/^•\s*/gm, '').trim();
            }
        });
    }
});