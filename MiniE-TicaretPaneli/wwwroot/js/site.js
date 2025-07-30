// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    function loadMainCategories() {
        console.log("Ana kategori dropdown yükleniyor...");
        var $dropdown = $('#parentCategoryDropdown');
        if ($dropdown.length === 0) {
            console.error("#parentCategoryDropdown bulunamadı!");
            return;
        }
        // (Eski $.getJSON('/Admin/GetRootCategories', ...) kodu kaldırıldı)
        // Kategori dropdown'u zaten view'da fetch ile dolduruluyor.
        // Bu fonksiyonun içeriği artık gereksiz hale geldi.
        // Ancak, fonksiyonun korunması ve çağrılması view'da hala kullanılıyor olabilir.
        // Eğer view'da kategori dropdown'u farklı bir şekilde dolduruluyorsa, bu fonksiyonun içeriği değiştirilmelidir.
        // Şimdilik, fonksiyonun içeriği boş bırakıldı.
    }
    // Sadece AddCategory sayfasında çalışsın
    if ($('#parentCategoryDropdown').length) {
        loadMainCategories();
    }

    // Checkout sayfası: adres seçimine göre yeni adres formunu göster/gizle ve seçili kartı vurgula
    if ($('input[name="addressOption"]').length) {
        function toggleNewAddressForm() {
            var selected = $('input[name="addressOption"]:checked').val();
            // Adres kartı vurgusu
            $('input[name="addressOption"]').closest('label.address-card').removeClass('selected');
            $('input[name="addressOption"]:checked').closest('label.address-card').addClass('selected');
            if (selected === 'new') {
                $('#newAddressForm').show();
                // Radio'yu seçili tut
                $('#addressOptionNew').prop('checked', true);
            } else {
                $('#newAddressForm').hide();
            }
        }
        // Sayfa ilk açıldığında hiçbiri seçili değilse ilk radio'yu seç
        if ($('input[name="addressOption"]:checked').length === 0) {
            $('input[name="addressOption"]').first().prop('checked', true);
        }
        toggleNewAddressForm();
        $('input[name="addressOption"]').on('change', toggleNewAddressForm);
    }
    // Kart seçiminde seçili kartı vurgula ve yeni kart formunu göster/gizle
    if ($('input[name="selectedCardId"]').length) {
        function toggleNewCardForm() {
            var selected = $('input[name="selectedCardId"]:checked').val();
            // Kart kartı vurgusu
            $('input[name="selectedCardId"]').closest('label.list-group-item').removeClass('selected');
            $('input[name="selectedCardId"]:checked').closest('label.list-group-item').addClass('selected');
            if (!selected) {
                $('#newCardForm').show();
            } else {
                $('#newCardForm').hide();
            }
        }
        toggleNewCardForm();
        $('input[name="selectedCardId"]').on('change', toggleNewCardForm);
    }
});
