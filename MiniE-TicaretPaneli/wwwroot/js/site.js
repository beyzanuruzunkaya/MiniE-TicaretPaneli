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
        $.getJSON('/Admin/GetRootCategories', function (data) {
            console.log("Gelen ana kategoriler:", data);
            $dropdown.empty();
            $dropdown.append($('<option>', { value: '', text: '-- Üst Kategori Seçiniz --' }));
            $.each(data, function (i, cat) {
                var text = cat.name;
                $dropdown.append($('<option>', { value: cat.id, text: text }));
            });
            console.log("Dropdown'a eklenen option sayısı:", $dropdown.find('option').length);
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error("AJAX hata:", textStatus, errorThrown);
        });
    }
    // Sadece AddCategory sayfasında çalışsın
    if ($('#parentCategoryDropdown').length) {
        loadMainCategories();
    }
});
