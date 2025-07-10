// Data/SeedData.cs
using Microsoft.EntityFrameworkCore;
using MiniE_TicaretPaneli.Models;
using System.Linq;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace MiniE_TicaretPaneli.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Geçici olarak bu kontrolü yorum satırı yapın ki veritabanı sıfırdan doldurulabilsin
                // if (context.Categories.Any() || context.Products.Any() || context.Users.Any())
                // {
                //     return;
                // }

                // 1. Kullanıcıları Seed Etme (Aynı kalır)
                context.Users.AddRange(
                    new User
                    {
                        Username = "admin",
                        PasswordHash = "adminpass",
                        Role = UserRole.Admin,
                        FirstName = "Admin",
                        LastName = "User",
                        Email = "admin@example.com",
                        PhoneNumber = null
                    },
                    new User
                    {
                        Username = "customer1",
                        PasswordHash = "customerpass",
                        Role = UserRole.Customer,
                        FirstName = "Customer",
                        LastName = "One",
                        Email = "customer1@example.com",
                        PhoneNumber = "5551234567"
                    }
                );
                context.SaveChanges();

                // 2. Kategorileri Seed Etme (GÖRSELDEKİ DETAYLARA GÖRE TAMAMEN GÜNCEL VE DOĞRU SIRALI)

                // Ana Cinsiyet/Yaş Grubu Kategorileri (ParentId = null)
                var kadin = new Category { Name = "Kadın", Type = "Cinsiyet", Value = "Kadın", Slug = "kadin", Gender = "Kadın" };
                var erkek = new Category { Name = "Erkek", Type = "Cinsiyet", Value = "Erkek", Slug = "erkek", Gender = "Erkek" };
                var anneCocuk = new Category { Name = "Anne & Çocuk", Type = "Yaş Grubu", Value = "Anne & Çocuk", Slug = "anne-cocuk", Gender = "Çocuk" };

                context.Categories.AddRange(
                    kadin, erkek, anneCocuk
                );
                context.SaveChanges();

                // Alt Kategoriler (İlk Seviye - Cinsiyet Kategorilerine Bağlı)
                // KADIN ALT KATEGORİLERİ
                var kadinGiyim = new Category { Name = "Giyim", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Giyim", Slug = "kadin-giyim", Gender = "Kadın" };
                var kadinAyakkabi = new Category { Name = "Ayakkabı", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Ayakkabı", Slug = "kadin-ayakkabi", Gender = "Kadın" };
                var kadinCantaAna = new Category { Name = "Çanta", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Çanta", Slug = "kadin-canta-ana", Gender = "Kadın" };
                var kadinAksesuarCantaAna = new Category { Name = "Aksesuar & Çanta", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Aksesuar & Çanta", Slug = "kadin-aksesuar-canta-ana", Gender = "Kadın" };

                // ERKEK ALT KATEGORİLERİ
                var erkekGiyim = new Category { Name = "Giyim", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Giyim", Slug = "erkek-giyim", Gender = "Erkek" };
                var erkekAyakkabi = new Category { Name = "Ayakkabı", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Ayakkabı", Slug = "erkek-ayakkabi", Gender = "Erkek" };
                var erkekCantaAna = new Category { Name = "Çanta", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Çanta", Slug = "erkek-canta-ana", Gender = "Erkek" };
                var erkekSaatAksesuar = new Category { Name = "Saat & Aksesuar", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Saat & Aksesuar", Slug = "erkek-saat-aksesuar", Gender = "Erkek" };
                var erkekBuyukBeden = new Category { Name = "Büyük Beden", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Büyük Beden", Slug = "erkek-buyuk-beden", Gender = "Erkek" };
                var erkekIcGiyim = new Category { Name = "İç Giyim", ParentCategory = erkek, Type = "Ürün Grubu", Value = "İç Giyim", Slug = "erkek-ic-giyim", Gender = "Erkek" };
                var erkekKisiselBakim = new Category { Name = "Kişisel Bakım", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Kişisel Bakım", Slug = "erkek-kisisel-bakim", Gender = "Erkek" };
                var erkekSporOutdoorAna = new Category { Name = "Spor & Outdoor", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Spor & Outdoor", Slug = "erkek-spor-outdoor-ana", Gender = "Erkek" };
                var erkekElektronikAna = new Category { Name = "Elektronik", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Elektronik", Slug = "erkek-elektronik-ana", Gender = "Erkek" };
                var erkekKitapKirtasiyeHobiAna = new Category { Name = "Kitap & Kırtasiye & Hobi", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Kitap & Kırtasiye & Hobi", Slug = "erkek-kitap-kirtasiye-hobi-ana", Gender = "Erkek" };


                // ANNE & ÇOCUK ALT KATEGORİLERİ
                var bebek = new Category { Name = "Bebek", ParentCategory = anneCocuk, Type = "Yaş Grubu", Value = "Bebek", Slug = "bebek", Gender = "Çocuk" };
                var kizCocuk = new Category { Name = "Kız Çocuk", ParentCategory = anneCocuk, Type = "Yaş Grubu", Value = "Kız Çocuk", Slug = "kiz-cocuk", Gender = "Çocuk" };
                var erkekCocuk = new Category { Name = "Erkek Çocuk", ParentCategory = anneCocuk, Type = "Yaş Grubu", Value = "Erkek Çocuk", Slug = "erkek-cocuk", Gender = "Çocuk" };
                var bebekBakimAna = new Category { Name = "Bebek Bakım", ParentCategory = anneCocuk, Type = "Bölüm", Value = "Bebek Bakım", Slug = "bebek-bakim-ana", Gender = "Çocuk" };
                var cocukOyuncakAna = new Category { Name = "Oyuncak", ParentCategory = anneCocuk, Type = "Bölüm", Value = "Oyuncak", Slug = "cocuk-oyuncak-ana", Gender = "Çocuk" };
                var beslenmeEmzirmeAna = new Category { Name = "Beslenme & Emzirme", ParentCategory = anneCocuk, Type = "Bölüm", Value = "Beslenme & Emzirme", Slug = "beslenme-emzirme-ana", Gender = "Çocuk" };


                context.Categories.AddRange(
                    kadinGiyim, kadinAyakkabi, kadinCantaAna, kadinAksesuarCantaAna,
                    erkekGiyim, erkekAyakkabi, erkekCantaAna, erkekSaatAksesuar, erkekBuyukBeden, erkekIcGiyim, erkekKisiselBakim, erkekSporOutdoorAna, erkekElektronikAna, erkekKitapKirtasiyeHobiAna,
                    bebek, kizCocuk, erkekCocuk, bebekBakimAna, cocukOyuncakAna, beslenmeEmzirmeAna
                );
                context.SaveChanges();

                // Daha Spesifik Alt Kategoriler (Ürün Tipleri - Bu değişkenler ürünlerden önce tanımlanmalı!)

                // KADIN GİYİM ALTINDAKİLER
                var kadinTisort = new Category { Name = "Tişört", ParentCategory = kadinGiyim, Type = "Ürün Tipi", Value = "Tişört", Slug = "kadin-tisort", Gender = "Kadın" };
                var kadinElbise = new Category { Name = "Elbise", ParentCategory = kadinGiyim, Type = "Ürün Tipi", Value = "Elbise", Slug = "kadin-elbise", Gender = "Kadın" };
                var kadinEtek = new Category { Name = "Etek", ParentCategory = kadinGiyim, Type = "Ürün Tipi", Value = "Etek", Slug = "kadin-etek", Gender = "Kadın" };
                var kadinGomlek = new Category { Name = "Gömlek", ParentCategory = kadinGiyim, Type = "Ürün Tipi", Value = "Gömlek", Slug = "kadin-gomlek", Gender = "Kadın" };
                var kadinKotPantolon = new Category { Name = "Kot Pantolon", ParentCategory = kadinGiyim, Type = "Ürün Tipi", Value = "Kot Pantolon", Slug = "kadin-kot-pantolon", Gender = "Kadın" };
                var kadinBikini = new Category { Name = "Bikini", ParentCategory = kadinGiyim, Type = "Ürün Tipi", Value = "Bikini", Slug = "kadin-bikini", Gender = "Kadın" };

                // KADIN AYAKKABI ALTINDAKİLER
                var kadinTopukluAyakkabi = new Category { Name = "Topuklu Ayakkabı", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Topuklu Ayakkabı", Slug = "topuklu-ayakkabi", Gender = "Kadın" };
                var kadinSneaker = new Category { Name = "Sneaker", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Sneaker", Slug = "kadin-sneaker", Gender = "Kadın" };
                var kadinGunlukAyakkabi = new Category { Name = "Günlük Ayakkabı", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Günlük Ayakkabı", Slug = "kadin-gunluk-ayakkabi", Gender = "Kadın" };
                var kadinBabet = new Category { Name = "Babet", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Babet", Slug = "kadin-babet", Gender = "Kadın" };
                var kadinSandalet = new Category { Name = "Sandalet", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Sandalet", Slug = "kadin-sandalet", Gender = "Kadın" };
                var kadinBot = new Category { Name = "Bot", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Bot", Slug = "kadin-bot", Gender = "Kadın" };
                var kadinCizme = new Category { Name = "Çizme", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Çizme", Slug = "kadin-cizme", Gender = "Kadın" };
                var kadinKarBotu = new Category { Name = "Kar Botu", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Kar Botu", Slug = "kadin-kar-botu", Gender = "Kadın" };
                var kadinLoafer = new Category { Name = "Loafer", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Loafer", Slug = "kadin-loafer", Gender = "Kadın" };
                var kadinEvTerligi = new Category { Name = "Ev Terliği", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Ev Terliği", Slug = "kadin-ev-terligi", Gender = "Kadın" };
                var kadinKosuAyakkabisi = new Category { Name = "Koşu Ayakkabısı", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Koşu Ayakkabısı", Slug = "kadin-kosu-ayakkabisi", Gender = "Kadın" };

                // KADIN ÇANTA ALTINDAKİLER
                var kadinOmuzCantasi = new Category { Name = "Omuz Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Omuz Çantası", Slug = "kadin-omuz-cantasi", Gender = "Kadın" };
                var kadinSirtCantasi = new Category { Name = "Sırt Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Sırt Çantası", Slug = "kadin-sirt-cantasi", Gender = "Kadın" };
                var kadinBelCantasi = new Category { Name = "Bel Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Bel Çantası", Slug = "kadin-bel-cantasi", Gender = "Kadın" };
                var kadinOkulCantasi = new Category { Name = "Okul Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Okul Çantası", Slug = "kadin-okul-cantasi", Gender = "Kadın" };
                var kadinLaptopCantasi = new Category { Name = "Laptop Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Laptop Çantası", Slug = "kadin-laptop-cantasi", Gender = "Kadın" };
                var kadinPortfoy = new Category { Name = "Portföy", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Portföy", Slug = "kadin-portfoy", Gender = "Kadın" };
                var kadinPostaciCantasi = new Category { Name = "Postacı Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Postacı Çantası", Slug = "kadin-postaci-cantasi", Gender = "Kadın" };
                var kadinElCantasi = new Category { Name = "El Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "El Çantası", Slug = "kadin-el-cantasi", Gender = "Kadın" };
                var kadinKanvasCanta = new Category { Name = "Kanvas Çanta", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Kanvas Çanta", Slug = "kadin-kanvas-cantasi", Gender = "Kadın" };
                var kadinMakyajCantasi = new Category { Name = "Makyaj Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Makyaj Çantası", Slug = "kadin-makyaj-cantasi", Gender = "Kadın" };
                var kadinAbiyeCanta = new Category { Name = "Abiye Çanta", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Abiye Çantası", Slug = "kadin-abiye-cantasi", Gender = "Kadın" };
                var kadinCaprazCanta = new Category { Name = "Çapraz Çanta", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Çapraz Çanta", Slug = "kadin-capraz-cantasi", Gender = "Kadın" };
                var kadinBezCanta = new Category { Name = "Bez Çanta", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Bez Çanta", Slug = "kadin-bez-cantasi", Gender = "Kadın" };
                var kadinAnneBebekCantasi = new Category { Name = "Anne Bebek Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Anne Bebek Çantası", Slug = "kadin-anne-bebek-cantasi", Gender = "Kadın" };
                var kadinEvrakCantasi = new Category { Name = "Evrak Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Evrak Çantası", Slug = "kadin-evrak-cantasi", Gender = "Kadın" };
                var kadinToteCanta = new Category { Name = "Tote Çanta", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Tote Çanta", Slug = "kadin-tote-cantasi", Gender = "Kadın" };
                var kadinBeslenmeCantasi = new Category { Name = "Beslenme Çantası", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Beslenme Çantası", Slug = "kadin-beslenme-cantasi", Gender = "Kadın" };
                var kadinKartlik = new Category { Name = "Kartlık", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Kartlık", Slug = "kadin-kartlik", Gender = "Kadın" };
                var kadinCuzdan = new Category { Name = "Cüzdan", ParentCategory = kadinCantaAna, Type = "Ürün Tipi", Value = "Cüzdan", Slug = "kadin-cuzdan", Gender = "Kadın" };


                // ERKEK GİYİM ALTINDAKİLER
                var erkekTisort = new Category { Name = "Tişört", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Tişört", Slug = "erkek-tisort", Gender = "Erkek" };
                var erkekSort = new Category { Name = "Şort", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Şort", Slug = "erkek-sort", Gender = "Erkek" };
                var erkekGomlek = new Category { Name = "Gömlek", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Gömlek", Slug = "erkek-gomlek", Gender = "Erkek" };
                var erkekEsofman = new Category { Name = "Eşofman", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Eşofman", Slug = "erkek-esofman", Gender = "Erkek" };
                var erkekPantolon = new Category { Name = "Pantolon", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Pantolon", Slug = "erkek-pantolon", Gender = "Erkek" };
                var erkekCeket = new Category { Name = "Ceket", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Ceket", Slug = "erkek-ceket", Gender = "Erkek" };
                var erkekKotPantolon = new Category { Name = "Kot Pantolon", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Kot Pantolon", Slug = "erkek-kot-pantolon", Gender = "Erkek" };
                var erkekYelek = new Category { Name = "Yelek", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Yelek", Slug = "erkek-yelek", Gender = "Erkek" };
                var erkekKazak = new Category { Name = "Kazak", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Kazak", Slug = "erkek-kazak", Gender = "Erkek" };
                var erkekMont = new Category { Name = "Mont", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Mont", Slug = "erkek-mont", Gender = "Erkek" };
                var erkekDeriMont = new Category { Name = "Deri Mont", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Deri Mont", Slug = "erkek-deri-mont", Gender = "Erkek" };
                var erkekKaban = new Category { Name = "Kaban", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Kaban", Slug = "erkek-kaban", Gender = "Erkek" };
                var erkekHirka = new Category { Name = "Hırka", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Hırka", Slug = "erkek-hirka", Gender = "Erkek" };
                var erkekPalto = new Category { Name = "Palto", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Palto", Slug = "erkek-palto", Gender = "Erkek" };
                var erkekYagmurluk = new Category { Name = "Yağmurluk", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Yağmurluk", Slug = "erkek-yagmurluk", Gender = "Erkek" };
                var erkekBlazer = new Category { Name = "Blazer", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Blazer", Slug = "erkek-blazer", Gender = "Erkek" };
                var erkekPolar = new Category { Name = "Polar", ParentCategory = erkekGiyim, Type = "Ürün Tipi", Value = "Polar", Slug = "erkek-polar", Gender = "Erkek" };

                // ERKEK AYAKKABI ALTINDAKİLER
                var erkekSporAyakkabi = new Category { Name = "Spor Ayakkabı", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Spor Ayakkabı", Slug = "erkek-spor-ayakkabi", Gender = "Erkek" };
                var erkekGunlukAyakkabi = new Category { Name = "Günlük Ayakkabı", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Günlük Ayakkabı", Slug = "erkek-gunluk-ayakkabi", Gender = "Erkek" };
                var erkekYuruyusAyakkabisi = new Category { Name = "Yürüyüş Ayakkabısı", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Yürüyüş Ayakkabısı", Slug = "erkek-yuruyus-ayakkabisi", Gender = "Erkek" };
                var erkekKrampon = new Category { Name = "Krampon", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Krampon", Slug = "erkek-krampon", Gender = "Erkek" };
                var erkekSneaker = new Category { Name = "Sneaker", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Sneaker", Slug = "erkek-sneaker", Gender = "Erkek" };
                var erkekKlasik = new Category { Name = "Klasik", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Klasik", Slug = "erkek-klasik-ayakkabi", Gender = "Erkek" };
                var erkekBot = new Category { Name = "Bot", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Bot", Slug = "erkek-bot", Gender = "Erkek" };
                var erkekKarBotu = new Category { Name = "Kar Botu", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Kar Botu", Slug = "erkek-kar-botu", Gender = "Erkek" };
                var erkekLoafer = new Category { Name = "Loafer", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Loafer", Slug = "erkek-loafer", Gender = "Erkek" };
                var erkekDeriAyakkabi = new Category { Name = "Deri Ayakkabı", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Deri Ayakkabı", Slug = "erkek-deri-ayakkabi", Gender = "Erkek" };
                var erkekEvTerligi = new Category { Name = "Ev Terliği", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Ev Terliği", Slug = "erkek-ev-terligi", Gender = "Erkek" };
                var erkekKosuAyakkabisi = new Category { Name = "Koşu Ayakkabısı", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Koşu Ayakkabısı", Slug = "erkek-kosu-ayakkabisi", Gender = "Erkek" };
                var erkekCizme = new Category { Name = "Çizme", ParentCategory = erkekAyakkabi, Type = "Ürün Tipi", Value = "Çizme", Slug = "erkek-cizme", Gender = "Erkek" };

                // ERKEK ÇANTA ALTINDAKİLER
                var erkekSirtCantasi = new Category { Name = "Sırt Çantası", ParentCategory = erkekCantaAna, Type = "Ürün Tipi", Value = "Sırt Çantası", Slug = "erkek-sirt-cantasi", Gender = "Erkek" };
                var erkekSporCanta = new Category { Name = "Spor Çanta", ParentCategory = erkekCantaAna, Type = "Ürün Tipi", Value = "Spor Çanta", Slug = "erkek-spor-cantasi", Gender = "Erkek" };
                var erkekLaptopCantasi = new Category { Name = "Laptop Çantası", ParentCategory = erkekCantaAna, Type = "Ürün Tipi", Value = "Laptop Çantası", Slug = "erkek-laptop-cantasi", Gender = "Erkek" };
                var erkekValizBavul = new Category { Name = "Valiz & Bavul", ParentCategory = erkekCantaAna, Type = "Ürün Tipi", Value = "Valiz & Bavul", Slug = "erkek-valiz-bavul", Gender = "Erkek" };
                var erkekPostaciCantasi = new Category { Name = "Postacı Çantası", ParentCategory = erkekCantaAna, Type = "Ürün Tipi", Value = "Postacı Çantası", Slug = "erkek-postaci-cantasi", Gender = "Erkek" };
                var erkekBelCantasi = new Category { Name = "Bel Çantası", ParentCategory = erkekCantaAna, Type = "Ürün Tipi", Value = "Bel Çantası", Slug = "erkek-bel-cantasi", Gender = "Erkek" };
                var erkekEvrakCantasi = new Category { Name = "Evrak Çantası", ParentCategory = erkekCantaAna, Type = "Ürün Tipi", Value = "Evrak Çantası", Slug = "erkek-evrak-cantasi", Gender = "Erkek" };
                var erkekBezCanta = new Category { Name = "Bez Çanta", ParentCategory = erkekCantaAna, Type = "Ürün Tipi", Value = "Bez Çanta", Slug = "erkek-bez-cantasi", Gender = "Erkek" };

                // ERKEK SAAT & AKSESUAR ALTINDAKİLER
                var erkekSaat = new Category { Name = "Saat", ParentCategory = erkekSaatAksesuar, Type = "Ürün Tipi", Value = "Saat", Slug = "erkek-saat", Gender = "Erkek" };
                var erkekGunesGozlugu = new Category { Name = "Güneş Gözlüğü", ParentCategory = erkekSaatAksesuar, Type = "Ürün Tipi", Value = "Güneş Gözlüğü", Slug = "erkek-gunes-gozlugu", Gender = "Erkek" };
                var erkekCuzdan = new Category { Name = "Cüzdan", ParentCategory = erkekSaatAksesuar, Type = "Ürün Tipi", Value = "Cüzdan", Slug = "erkek-cuzdan", Gender = "Erkek" };
                var erkekKemer = new Category { Name = "Kemer", ParentCategory = erkekSaatAksesuar, Type = "Ürün Tipi", Value = "Kemer", Slug = "erkek-kemer", Gender = "Erkek" };
                var erkekCantaAksesuar = new Category { Name = "Çanta", ParentCategory = erkekSaatAksesuar, Type = "Ürün Tipi", Value = "Çanta", Slug = "erkek-canta-aksesuar", Gender = "Erkek" };
                var erkekSapka = new Category { Name = "Şapka", ParentCategory = erkekSaatAksesuar, Type = "Ürün Tipi", Value = "Şapka", Slug = "erkek-sapka", Gender = "Erkek" };
                var erkekAtki = new Category { Name = "Atkı", ParentCategory = erkekSaatAksesuar, Type = "Ürün Tipi", Value = "Atkı", Slug = "erkek-atki", Gender = "Erkek" };
                var erkekBere = new Category { Name = "Bere", ParentCategory = erkekSaatAksesuar, Type = "Ürün Tipi", Value = "Bere", Slug = "erkek-bere", Gender = "Erkek" };
                var erkekEldiven = new Category { Name = "Eldiven", ParentCategory = erkekSaatAksesuar, Type = "Ürün Tipi", Value = "Eldiven", Slug = "erkek-eldiven", Gender = "Erkek" };
                var erkekBoyunluk = new Category { Name = "Boyunluk", ParentCategory = erkekSaatAksesuar, Type = "Ürün Tipi", Value = "Boyunluk", Slug = "erkek-boyunluk", Gender = "Erkek" };

                // ERKEK BÜYÜK BEDEN ALTINDAKİLER
                var erkekBuyukBedenSweatshirt = new Category { Name = "Sweatshirt", ParentCategory = erkekBuyukBeden, Type = "Ürün Tipi", Value = "Sweatshirt", Slug = "erkek-buyuk-beden-sweatshirt", Gender = "Erkek" };
                var erkekBuyukBedenTshirt = new Category { Name = "T-shirt", ParentCategory = erkekBuyukBeden, Type = "Ürün Tipi", Value = "T-shirt", Slug = "erkek-buyuk-beden-tshirt", Gender = "Erkek" };
                var erkekBuyukBedenPantolon = new Category { Name = "Pantolon", ParentCategory = erkekBuyukBeden, Type = "Ürün Tipi", Value = "Pantolon", Slug = "erkek-buyuk-beden-pantolon", Gender = "Erkek" };
                var erkekBuyukBedenMont = new Category { Name = "Mont", ParentCategory = erkekBuyukBeden, Type = "Ürün Tipi", Value = "Mont", Slug = "erkek-buyuk-beden-mont", Gender = "Erkek" };
                var erkekBuyukBedenGomlek = new Category { Name = "Gömlek", ParentCategory = erkekBuyukBeden, Type = "Ürün Tipi", Value = "Gömlek", Slug = "erkek-buyuk-beden-gomlek", Gender = "Erkek" };
                var erkekBuyukBedenKazak = new Category { Name = "Kazak", ParentCategory = erkekBuyukBeden, Type = "Ürün Tipi", Value = "Kazak", Slug = "erkek-buyuk-beden-kazak", Gender = "Erkek" };
                var erkekBuyukBedenHirka = new Category { Name = "Hırka", ParentCategory = erkekBuyukBeden, Type = "Ürün Tipi", Value = "Hırka", Slug = "erkek-buyuk-beden-hirka", Gender = "Erkek" };
                var erkekBuyukBedenEsofman = new Category { Name = "Eşofman", ParentCategory = erkekBuyukBeden, Type = "Ürün Tipi", Value = "Eşofman", Slug = "erkek-buyuk-beden-esofman", Gender = "Erkek" };

                // ERKEK İÇ GİYİM ALTINDAKİLER
                var erkekBoxer = new Category { Name = "Boxer", ParentCategory = erkekIcGiyim, Type = "Ürün Tipi", Value = "Boxer", Slug = "erkek-boxer", Gender = "Erkek" };
                var erkekIclik = new Category { Name = "İçlik", ParentCategory = erkekIcGiyim, Type = "Ürün Tipi", Value = "İçlik", Slug = "erkek-iclik", Gender = "Erkek" };
                var erkekCorap = new Category { Name = "Çorap", ParentCategory = erkekIcGiyim, Type = "Ürün Tipi", Value = "Çorap", Slug = "erkek-corap", Gender = "Erkek" };
                var erkekPijama = new Category { Name = "Pijama", ParentCategory = erkekIcGiyim, Type = "Ürün Tipi", Value = "Pijama", Slug = "erkek-pijama", Gender = "Erkek" };
                var erkekAtleta = new Category { Name = "Atleta", ParentCategory = erkekIcGiyim, Type = "Ürün Tipi", Value = "Atleta", Slug = "erkek-atleta", Gender = "Erkek" };

                // ERKEK KİŞİSEL BAKIM ALTINDAKİLER
                var erkekParfum = new Category { Name = "Parfüm", ParentCategory = erkekKisiselBakim, Type = "Ürün Tipi", Value = "Parfüm", Slug = "erkek-parfum", Gender = "Erkek" };
                var erkekCinselSaglik = new Category { Name = "Cinsel Sağlık", ParentCategory = erkekKisiselBakim, Type = "Ürün Tipi", Value = "Cinsel Sağlık", Slug = "erkek-cinsel-saglik", Gender = "Erkek" };
                var erkekTirasSonrasiUrunler = new Category { Name = "Tıraş Sonrası Ürünler", ParentCategory = erkekKisiselBakim, Type = "Ürün Tipi", Value = "Tıraş Sonrası Ürünler", Slug = "erkek-tiras-sonrasi", Gender = "Erkek" };
                var erkekTirasBicagi = new Category { Name = "Tıraş Bıçağı", ParentCategory = erkekKisiselBakim, Type = "Ürün Tipi", Value = "Tıraş Bıçağı", Slug = "erkek-tiras-bicagi", Gender = "Erkek" };
                var erkekDeodorant = new Category { Name = "Deodorant", ParentCategory = erkekKisiselBakim, Type = "Ürün Tipi", Value = "Deodorant", Slug = "erkek-deodorant", Gender = "Erkek" };

                // ANNE & ÇOCUK ALTINDAKİLER
                // BEBEK ALTINDAKİLER
                var bebekTakimlari = new Category { Name = "Bebek Takımları", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Bebek Takımları", Slug = "bebek-takimlari", Gender = "Çocuk" };
                var bebekAyakkabi = new Category { Name = "Ayakkabı", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Ayakkabı", Slug = "bebek-ayakkabi", Gender = "Çocuk" };
                var bebekHastaneCikisi = new Category { Name = "Hastane Çıkışı", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Hastane Çıkışı", Slug = "bebek-hastane-cikisi", Gender = "Çocuk" };
                var bebekYenidoganKiyafetleri = new Category { Name = "Yenidoğan Kıyafetleri", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Yenidoğan Kıyafetleri", Slug = "bebek-yenidogan-kiyafetleri", Gender = "Çocuk" };
                var bebekTulum = new Category { Name = "Tulum", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Tulum", Slug = "bebek-tulum", Gender = "Çocuk" };
                var bebekBodyZibin = new Category { Name = "Body & Zıbın", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Body & Zıbın", Slug = "bebek-body-zibin", Gender = "Çocuk" };
                var bebekTisortAtlet = new Category { Name = "Tişört & Atlet", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Tişört & Atlet", Slug = "bebek-tisort-atlet", Gender = "Çocuk" };
                var bebekTayt = new Category { Name = "Tayt", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Tayt", Slug = "bebek-tayt", Gender = "Çocuk" };
                var bebekSort = new Category { Name = "Şort", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Şort", Slug = "bebek-sort", Gender = "Çocuk" };
                var bebekGomlek = new Category { Name = "Gömlek", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Gömlek", Slug = "bebek-gomlek", Gender = "Çocuk" };
                var bebekMont = new Category { Name = "Mont", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Mont", Slug = "bebek-mont", Gender = "Çocuk" };
                var bebekPatigi = new Category { Name = "Bebek Patiği", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Bebek Patigi", Slug = "bebek-patigi", Gender = "Çocuk" };
                var bebekHirka = new Category { Name = "Hırka", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Hırka", Slug = "bebek-hirka", Gender = "Çocuk" };
                var bebekBattaniye = new Category { Name = "Battaniye", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Battaniye", Slug = "bebek-battaniye", Gender = "Çocuk" };
                var bebekAltUstTakim = new Category { Name = "Alt Üst Takım", ParentCategory = bebek, Type = "Ürün Tipi", Value = "Alt Ust Takim", Slug = "bebek-alt-ust-takim", Gender = "Çocuk" };

                // KIZ ÇOCUK ALTINDAKİLER
                var kizCocukElbise = new Category { Name = "Elbise", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Elbise", Slug = "kiz-cocuk-elbise", Gender = "Çocuk" };
                var kizCocukSweatshirt = new Category { Name = "Sweatshirt", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Sweatshirt", Slug = "kiz-cocuk-sweatshirt", Gender = "Çocuk" };
                var kizCocukSporAyakkabi = new Category { Name = "Spor Ayakkabı", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Spor Ayakkabı", Slug = "kiz-cocuk-spor-ayakkabi", Gender = "Çocuk" };
                var kizCocukEsofman = new Category { Name = "Eşofman", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Eşofman", Slug = "kiz-cocuk-esofman", Gender = "Çocuk" };
                var kizCocukIcGiyimPijama = new Category { Name = "İç Giyim & Pijama", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "İç Giyim & Pijama", Slug = "kiz-cocuk-ic-giyim-pijama", Gender = "Çocuk" };
                var kizCocukTisortAtlet = new Category { Name = "Tişört & Atlet", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Tişört & Atlet", Slug = "kiz-cocuk-tisort-atlet", Gender = "Çocuk" };
                var kizCocukTayt = new Category { Name = "Tayt", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Tayt", Slug = "kiz-cocuk-tayt", Gender = "Çocuk" };
                var kizCocukGunlukAyakkabi = new Category { Name = "Günlük Ayakkabı", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Günlük Ayakkabı", Slug = "kiz-cocuk-gunluk-ayakkabi", Gender = "Çocuk" };
                var kizCocukSort = new Category { Name = "Şort", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Şort", Slug = "kiz-cocuk-sort", Gender = "Çocuk" };
                var kizCocukGomlek = new Category { Name = "Gömlek", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Gömlek", Slug = "kiz-cocuk-gomlek", Gender = "Çocuk" };
                var kizCocukMont = new Category { Name = "Mont", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Mont", Slug = "kiz-cocuk-mont", Gender = "Çocuk" };
                var kizCocukOyunEvi = new Category { Name = "Çocuk Oyun Evi", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Çocuk Oyun Evi", Slug = "kiz-cocuk-oyun-evi", Gender = "Çocuk" };
                var kizCocukOyuncakBebek = new Category { Name = "Oyuncak Bebek", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Oyuncak Bebek", Slug = "kiz-cocuk-oyuncak-bebek", Gender = "Çocuk" };
                var kizCocukOyuncakMutfak = new Category { Name = "Oyuncak Mutfak", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Oyuncak Mutfak", Slug = "kiz-cocuk-oyuncak-mutfak", Gender = "Çocuk" };
                var kizCocukKaban = new Category { Name = "Kaban", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Kaban", Slug = "kiz-cocuk-kaban", Gender = "Çocuk" };
                var kizCocukAbiyeElbise = new Category { Name = "Abiye & Elbise", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Abiye & Elbise", Slug = "kiz-cocuk-abiye-elbise", Gender = "Çocuk" };
                var kizCocukCeket = new Category { Name = "Ceket", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Ceket", Slug = "kiz-cocuk-ceket", Gender = "Çocuk" };
                var kizCocukPantolon = new Category { Name = "Pantolon", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Pantolon", Slug = "kiz-cocuk-pantolon", Gender = "Çocuk" };
                var kizCocukKazak = new Category { Name = "Kazak", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Kazak", Slug = "kiz-cocuk-kazak", Gender = "Çocuk" };
                var kizCocukBot = new Category { Name = "Bot", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Bot", Slug = "kiz-cocuk-bot", Gender = "Çocuk" };
                var kizCocukKrampon = new Category { Name = "Krampon", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Krampon", Slug = "kiz-cocuk-krampon", Gender = "Çocuk" };
                var kizCocukSapkaBereEldiven = new Category { Name = "Şapka & Bere & Eldiven", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Şapka & Bere & Eldiven", Slug = "kiz-cocuk-sapka-bere-eldiven", Gender = "Çocuk" };
                var kizCocukTakimElbise = new Category { Name = "Takım Elbise", ParentCategory = kizCocuk, Type = "Ürün Tipi", Value = "Takım Elbise", Slug = "kiz-cocuk-takim-elbise", Gender = "Çocuk" };

                // ERKEK ÇOCUK ALTINDAKİLER
                var erkekCocukSweatshirt = new Category { Name = "Sweatshirt", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Sweatshirt", Slug = "erkek-cocuk-sweatshirt", Gender = "Çocuk" };
                var erkekCocukSporAyakkabi = new Category { Name = "Spor Ayakkabı", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Spor Ayakkabı", Slug = "erkek-cocuk-spor-ayakkabi", Gender = "Çocuk" };
                var erkekCocukEsofman = new Category { Name = "Eşofman", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Eşofman", Slug = "erkek-cocuk-esofman", Gender = "Çocuk" };
                var erkekCocukIcGiyimPijama = new Category { Name = "İç Giyim & Pijama", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "İç Giyim & Pijama", Slug = "erkek-cocuk-ic-giyim-pijama", Gender = "Çocuk" };
                var erkekCocukTisortAtlet = new Category { Name = "Tişört & Atlet", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Tişört & Atlet", Slug = "erkek-cocuk-tisort-atlet", Gender = "Çocuk" };
                var erkekCocukGunlukAyakkabi = new Category { Name = "Günlük Ayakkabı", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Günlük Ayakkabı", Slug = "erkek-cocuk-gunluk-ayakkabi", Gender = "Çocuk" };
                var erkekCocukSort = new Category { Name = "Şort", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Şort", Slug = "erkek-cocuk-sort", Gender = "Çocuk" };
                var erkekCocukGomlek = new Category { Name = "Gömlek", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Gömlek", Slug = "erkek-cocuk-gomlek", Gender = "Çocuk" };
                var erkekCocukMont = new Category { Name = "Mont", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Mont", Slug = "erkek-cocuk-mont", Gender = "Çocuk" };
                var erkekCocukOyunEvi = new Category { Name = "Çocuk Oyun Evi", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Çocuk Oyun Evi", Slug = "erkek-cocuk-oyun-evi", Gender = "Çocuk" };
                var erkekCocukOyuncakTraktor = new Category { Name = "Oyuncak Traktör", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Oyuncak Traktör", Slug = "erkek-cocuk-oyuncak-traktor", Gender = "Çocuk" };
                var erkekCocukAkuluAraba = new Category { Name = "Akülü Araba", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Akülü Araba", Slug = "erkek-cocuk-akulu-araba", Gender = "Çocuk" };
                var erkekCocukKumandaliAraba = new Category { Name = "Kumandalı Araba", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Kumandalı Araba", Slug = "erkek-cocuk-kumandali-araba", Gender = "Çocuk" };
                var erkekCocukBisiklet = new Category { Name = "Bisiklet", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Bisiklet", Slug = "erkek-cocuk-bisiklet", Gender = "Çocuk" };
                var erkekCocukBoxer = new Category { Name = "Boxer", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Boxer", Slug = "erkek-cocuk-boxer", Gender = "Çocuk" };
                var erkekCocukIclik = new Category { Name = "İçlik", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "İçlik", Slug = "erkek-cocuk-iclik", Gender = "Çocuk" };
                var erkekCocukBot = new Category { Name = "Bot", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Bot", Slug = "erkek-cocuk-bot", Gender = "Çocuk" };
                var erkekCocukKrampon = new Category { Name = "Krampon", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Krampon", Slug = "erkek-cocuk-krampon", Gender = "Çocuk" };
                var erkekCocukSapkaBereEldiven = new Category { Name = "Şapka & Bere & Eldiven", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Şapka & Bere & Eldiven", Slug = "erkek-cocuk-sapka-bere-eldiven", Gender = "Çocuk" };
                var erkekCocukTakimElbise = new Category { Name = "Takım Elbise", ParentCategory = erkekCocuk, Type = "Ürün Tipi", Value = "Takım Elbise", Slug = "erkek-cocuk-takim-elbise", Gender = "Çocuk" };

                // BEBEK BAKIM ALTINDAKİLER
                var bebekBezi = new Category { Name = "Bebek Bezi", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Bebek Bezi", Slug = "bebek-bezi", Gender = "Çocuk" };
                var bebekKremYaglar = new Category { Name = "Krem & Yağlar", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Krem & Yağlar", Slug = "bebek-krem-yaglar", Gender = "Çocuk" };
                var bebekSampuan = new Category { Name = "Bebek Şampuanı", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Bebek Şampuanı", Slug = "bebek-sampuan", Gender = "Çocuk" };
                var bebekSabunlari = new Category { Name = "Bebek Sabunları", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Bebek Sabunları", Slug = "bebek-sabunlari", Gender = "Çocuk" };
                var bebekDeterjanlari = new Category { Name = "Bebek Deterjanları", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Bebek Deterjanları", Slug = "bebek-deterjanlari", Gender = "Çocuk" };
                var bebekVucutKremi = new Category { Name = "Bebek Vücut Kremi", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Bebek Vucut Kremi", Slug = "bebek-vucut-kremi", Gender = "Çocuk" };
                var bebekIslakMendil = new Category { Name = "Islak Mendil", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Islak Mendil", Slug = "bebek-islak-mendil", Gender = "Çocuk" };
                var bebekTaragi = new Category { Name = "Bebek Tarağı", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Bebek Tarağı", Slug = "bebek-taragi", Gender = "Çocuk" };
                var bebekYagi = new Category { Name = "Bebek Yağı", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Bebek Yağı", Slug = "bebek-yagi", Gender = "Çocuk" };
                var bebekBuharMakinesi = new Category { Name = "Bebek Buhar Makinesi", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Bebek Buhar Makinesi", Slug = "bebek-buhar-makinesi", Gender = "Çocuk" };
                var bebekAtesOlcer = new Category { Name = "Bebek Ateş Ölçer", ParentCategory = bebekBakimAna, Type = "Ürün Tipi", Value = "Bebek Ateş Olcer", Slug = "bebek-ates-olcer", Gender = "Çocuk" };

                // ÇOCUK OYUNCAK ALTINDAKİLER
                var egiticiOyuncaklar = new Category { Name = "Eğitici Oyuncaklar", ParentCategory = cocukOyuncakAna, Type = "Ürün Tipi", Value = "Egitici Oyuncaklar", Slug = "egitici-oyuncaklar", Gender = "Çocuk" };
                var oyuncakAyakkabi = new Category { Name = "Oyuncak Ayakkabı", ParentCategory = cocukOyuncakAna, Type = "Ürün Tipi", Value = "Oyuncak Ayakkabı", Slug = "oyuncak-ayakkabi", Gender = "Çocuk" };
                var robotOyuncak = new Category { Name = "Robot Oyuncak", ParentCategory = cocukOyuncakAna, Type = "Ürün Tipi", Value = "Robot Oyuncak", Slug = "robot-oyuncak", Gender = "Çocuk" };
                var kumandaliOyuncak = new Category { Name = "Kumandalı Oyuncak", ParentCategory = cocukOyuncakAna, Type = "Ürün Tipi", Value = "Kumandali Oyuncak", Slug = "kumandali-oyuncak", Gender = "Çocuk" };
                var cocukCizimTableti = new Category { Name = "Çocuk Çizim Tableti", ParentCategory = cocukOyuncakAna, Type = "Ürün Tipi", Value = "Çocuk Çizim Tableti", Slug = "cocuk-cizim-tableti", Gender = "Çocuk" };

                // BESLENME & EMZİRME ALTINDAKİLER
                var biberonEmzik = new Category { Name = "Biberon & Emzik", ParentCategory = beslenmeEmzirmeAna, Type = "Ürün Tipi", Value = "Biberon & Emzik", Slug = "biberon-emzik", Gender = "Çocuk" };
                var gogusPompasi = new Category { Name = "Göğüs Pompası", ParentCategory = beslenmeEmzirmeAna, Type = "Ürün Tipi", Value = "Gogus Pompasi", Slug = "gogus-pompasi", Gender = "Çocuk" };
                var mamaSandalyesi = new Category { Name = "Mama Sandalyesi", ParentCategory = beslenmeEmzirmeAna, Type = "Ürün Tipi", Value = "Mama Sandalyesi", Slug = "mama-sandalyesi", Gender = "Çocuk" };
                var mamaOnlugu = new Category { Name = "Mama Önlüğü", ParentCategory = beslenmeEmzirmeAna, Type = "Ürün Tipi", Value = "Mama Onlugu", Slug = "mama-onlugu", Gender = "Çocuk" };
                var alistirmaBardagi = new Category { Name = "Alıştırma Bardağı", ParentCategory = beslenmeEmzirmeAna, Type = "Ürün Tipi", Value = "Alistirma Bardagi", Slug = "alistirma-bardagi", Gender = "Çocuk" };
                var biberonTemizleyici = new Category { Name = "Biberon Temizleyici", ParentCategory = beslenmeEmzirmeAna, Type = "Ürün Tipi", Value = "Biberon Temizleyici", Slug = "biberon-temizleyici", Gender = "Çocuk" };
                var biberonSeti = new Category { Name = "Biberon Seti", ParentCategory = beslenmeEmzirmeAna, Type = "Ürün Tipi", Value = "Biberon Seti", Slug = "biberon-seti", Gender = "Çocuk" };
                var bebekMamasi = new Category { Name = "Bebek Maması", ParentCategory = beslenmeEmzirmeAna, Type = "Ürün Tipi", Value = "Bebek Mamasi", Slug = "bebek-mamasi", Gender = "Çocuk" };
                var kavanozMama = new Category { Name = "Kavanoz Mama", ParentCategory = beslenmeEmzirmeAna, Type = "Ürün Tipi", Value = "Kavanoz Mama", Slug = "kavanoz-mama", Gender = "Çocuk" };
                var sterilizator = new Category { Name = "Sterilizatör", ParentCategory = beslenmeEmzirmeAna, Type = "Ürün Tipi", Value = "Sterilizator", Slug = "sterilizator", Gender = "Çocuk" };

                context.Categories.AddRange(
                    // KADIN
                    kadinTisort, kadinElbise, kadinEtek, kadinGomlek, kadinKotPantolon, kadinBikini,
                    kadinTopukluAyakkabi, kadinSneaker, kadinGunlukAyakkabi, kadinBabet, kadinSandalet, kadinBot, kadinCizme, kadinKarBotu, kadinLoafer, kadinEvTerligi, kadinKosuAyakkabisi,
                    kadinOmuzCantasi, kadinSirtCantasi, kadinBelCantasi, kadinOkulCantasi, kadinLaptopCantasi, kadinPortfoy, kadinPostaciCantasi, kadinElCantasi, kadinKanvasCanta, kadinMakyajCantasi, kadinAbiyeCanta, kadinCaprazCanta, kadinBezCanta, kadinAnneBebekCantasi, kadinEvrakCantasi, kadinToteCanta, kadinBeslenmeCantasi, kadinKartlik, kadinCuzdan,

                    // ERKEK
                    erkekTisort, erkekSort, erkekGomlek, erkekEsofman, erkekPantolon, erkekCeket, erkekKotPantolon, erkekYelek, erkekKazak, erkekMont, erkekDeriMont, erkekKaban, erkekHirka, erkekPalto, erkekYagmurluk, erkekBlazer, erkekPolar,
                    erkekSporAyakkabi, erkekGunlukAyakkabi, erkekYuruyusAyakkabisi, erkekKrampon, erkekSneaker, erkekKlasik, erkekBot, erkekKarBotu, erkekLoafer, erkekDeriAyakkabi, erkekEvTerligi, erkekKosuAyakkabisi, erkekCizme,
                    erkekSirtCantasi, erkekSporCanta, erkekLaptopCantasi, erkekValizBavul, erkekPostaciCantasi, erkekBelCantasi, erkekEvrakCantasi, erkekBezCanta,
                    erkekSaat, erkekGunesGozlugu, erkekCuzdan, erkekKemer, erkekCantaAksesuar, erkekSapka, erkekAtki, erkekBere, erkekEldiven, erkekBoyunluk,
                    erkekBuyukBedenSweatshirt, erkekBuyukBedenTshirt, erkekBuyukBedenPantolon, erkekBuyukBedenMont, erkekBuyukBedenGomlek, erkekBuyukBedenKazak, erkekBuyukBedenHirka, erkekBuyukBedenEsofman,
                    erkekBoxer, erkekIclik, erkekCorap, erkekPijama, erkekAtleta,
                    erkekParfum, erkekCinselSaglik, erkekTirasSonrasiUrunler, erkekTirasBicagi, erkekDeodorant,

                    // ÇOCUK
                    bebekTakimlari, bebekAyakkabi, bebekHastaneCikisi, bebekYenidoganKiyafetleri, bebekTulum, bebekBodyZibin, bebekTisortAtlet, bebekTayt, bebekSort, bebekGomlek, bebekMont, bebekPatigi, bebekHirka, bebekBattaniye, bebekAltUstTakim,
                    kizCocukElbise, kizCocukSweatshirt, kizCocukSporAyakkabi, kizCocukEsofman, kizCocukIcGiyimPijama, kizCocukTisortAtlet, kizCocukTayt, kizCocukGunlukAyakkabi, kizCocukSort, kizCocukGomlek, kizCocukMont, kizCocukOyunEvi, kizCocukOyuncakBebek, kizCocukOyuncakMutfak, kizCocukKaban, kizCocukAbiyeElbise, kizCocukCeket, kizCocukPantolon, kizCocukKazak, kizCocukBot, kizCocukKrampon, kizCocukSapkaBereEldiven, kizCocukTakimElbise,
                    erkekCocukSweatshirt, erkekCocukSporAyakkabi, erkekCocukEsofman, erkekCocukIcGiyimPijama, erkekCocukTisortAtlet, erkekCocukGunlukAyakkabi, erkekCocukSort, erkekCocukGomlek, erkekCocukMont, erkekCocukOyunEvi, erkekCocukOyuncakTraktor, erkekCocukAkuluAraba, erkekCocukKumandaliAraba, erkekCocukBisiklet, erkekCocukBoxer, erkekCocukIclik, erkekCocukBot, erkekCocukKrampon, erkekCocukSapkaBereEldiven, erkekCocukTakimElbise,
                    bebekBezi, bebekKremYaglar, bebekSampuan, bebekSabunlari, bebekDeterjanlari, bebekVucutKremi, bebekIslakMendil, bebekTaragi, bebekYagi, bebekBuharMakinesi, bebekAtesOlcer,
                    egiticiOyuncaklar, oyuncakAyakkabi, robotOyuncak, kumandaliOyuncak, cocukCizimTableti,
                    biberonEmzik, gogusPompasi, mamaSandalyesi, mamaOnlugu, alistirmaBardagi, biberonTemizleyici, biberonSeti, bebekMamasi, kavanozMama, sterilizator
                );
                context.SaveChanges();

                // 3. Örnek Kıyafet Ürünleri Oluşturma (Aynı kalır, sadece kategori atamaları kontrol edilir)
                context.Products.AddRange(
                    new Product
                    {
                        Name = "Kadın V Yaka Tişört",
                        Description = "Rahat kesim, %100 pamuklu, V yaka kadın tişört. Günlük kullanım için ideal.",
                        Price = 129.90f,
                        ImageUrl = "/images/kadin-v-yaka-tisort.jpg",
                        Stock = 100,
                        MainCategory = kadin,
                        SubCategory = kadinTisort,
                        Brand = "ModaLine",
                        Gender = "Kadın",
                        AvailableSizes = "S,M,L,XL",
                        AvailableColors = "Beyaz,Siyah,Gri,Lacivert",
                        Material = "Pamuk",
                        Pattern = "Düz"
                    },
                    new Product
                    {
                        Name = "Erkek Skinny Jean Pantolon",
                        Description = "Modern skinny fit kesim, esnek denim kumaş mavi jean pantolon. Şık ve rahat.",
                        Price = 299.50f,
                        ImageUrl = "/images/erkek-skinny-jean.jpg",
                        Stock = 75,
                        MainCategory = erkek,
                        SubCategory = erkekPantolon,
                        Brand = "UrbanCode",
                        Gender = "Erkek",
                        AvailableSizes = "28,30,32,34,36",
                        AvailableColors = "Mavi,Açık Mavi,Siyah",
                        Material = "Denim",
                        Pattern = "Düz"
                    },
                    new Product
                    {
                        Name = "Çocuk Dinozor Desenli Sweatshirt",
                        Description = "Yumuşak iç astarlı, dinozor desenli, kapüşonlu sweatshirt. Erkek çocuklar için ideal.",
                        Price = 179.00f,
                        ImageUrl = "/images/cocuk-dinozor-sweatshirt.jpg",
                        Stock = 60,
                        MainCategory = anneCocuk,
                        SubCategory = erkekCocukSweatshirt,
                        Brand = "MinikAdımlar",
                        Gender = "Çocuk",
                        AvailableSizes = "2-3Y,4-5Y,6-7Y,8-9Y",
                        AvailableColors = "Yeşil,Mavi,Gri",
                        Material = "Polyester Karışım",
                        Pattern = "Desenli"
                    },
                    new Product
                    {
                        Name = "Kadın Çiçek Desenli Elbise",
                        Description = "Yazlık, hafif kumaş, diz üstü çiçek desenli elbise. Şık ve havadar.",
                        Price = 349.99f,
                        ImageUrl = "/images/kadin-cicekli-elbise.jpg",
                        Stock = 40,
                        MainCategory = kadin,
                        SubCategory = kadinElbise,
                        Brand = "BohoChic",
                        Gender = "Kadın",
                        AvailableSizes = "S,M,L",
                        AvailableColors = "Kırmızı,Mavi,Sarı",
                        Material = "Viskon",
                        Pattern = "Çiçekli"
                    },
                    new Product
                    {
                        Name = "Erkek Bomber Ceket",
                        Description = "Klasik bomber kesim, su itici kumaş erkek ceket. Sonbahar ve ilkbahar için ideal.",
                        Price = 450.00f,
                        ImageUrl = "/images/erkek-bomber-ceket.jpg",
                        Stock = 30,
                        MainCategory = erkek,
                        SubCategory = erkekCeket,
                        Brand = "StreetStyle",
                        Gender = "Erkek",
                        AvailableSizes = "S,M,L,XL,XXL",
                        AvailableColors = "Siyah,Haki,Lacivert",
                        Material = "Polyester",
                        Pattern = "Düz"
                    },
                     new Product
                     {
                         Name = "Kadın İki Parça Bikini Takımı",
                         Description = "Yazlık, plaj için şık ve rahat bikini takımı.",
                         Price = 189.90f,
                         ImageUrl = "/images/kadin-bikini-takimi.jpg",
                         Stock = 25,
                         MainCategory = kadin,
                         SubCategory = kadinBikini,
                         Brand = "BeachStyle",
                         Gender = "Kadın",
                         AvailableSizes = "S,M,L",
                         AvailableColors = "Mavi,Beyaz,Desenli",
                         Material = "Likra",
                         Pattern = "Desenli"
                     }
                );
                context.SaveChanges();
            }
        }
    }
}