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
                // Bu kontrol, veritabanı boşsa (yeni oluşturulduysa) veri eklenmesini sağlar.
                // İlk çalıştırma için BU KONTROLÜ YORUM SATIRINDAN ÇIKARINIZ ve tüm veritabanını silip Add-Migration / Update-Database yapınız.
                // Veriler eklendikten sonra bu if bloğu tekrar AKTİF HALE GETİRİLMELİDİR.
                // if (context.Categories.Any() || context.Products.Any() || context.Users.Any())
                // {
                //     return;
                // }

                // 1. Kullanıcıları Seed Etme
                if (!context.Users.Any())
                {
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
                    Console.WriteLine("Başlangıç kullanıcıları eklendi.");
                }

                // TÜM KATEGORİ VE ÜRÜN EKLEME KISMI
                if (!context.Categories.Any() && !context.Products.Any())
                {
                    // SEVİYE 1: Cinsiyet/Yaş Grubu Kategorileri
                    var kadin = new Category { Name = "Kadın", Type = "Cinsiyet", Value = "Kadın", Slug = "kadin", Gender = "Kadın" };
                    var erkek = new Category { Name = "Erkek", Type = "Cinsiyet", Value = "Erkek", Slug = "erkek", Gender = "Erkek" };
                    var anneCocuk = new Category { Name = "Anne & Çocuk", Type = "Yaş Grubu", Value = "Anne & Çocuk", Slug = "anne-cocuk", Gender = "Çocuk" };

                    context.Categories.AddRange(kadin, erkek, anneCocuk);
                    context.SaveChanges();

                    // SEVİYE 2 (YENİ "ANA KATEGORİ"): Ürün Grubu Kategorileri

                    // KADIN İÇİN
                    var kadinUstGiyim = new Category { Name = "Üst Giyim", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Üst Giyim", Slug = "kadin-ust-giyim", Gender = "Kadın" };
                    var kadinAltGiyim = new Category { Name = "Alt Giyim", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Alt Giyim", Slug = "kadin-alt-giyim", Gender = "Kadın" };
                    var kadinDisGiyim = new Category { Name = "Dış Giyim", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Dış Giyim", Slug = "kadin-dis-giyim", Gender = "Kadın" };
                    var kadinMayoBikini = new Category { Name = "Mayo & Bikini", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Mayo & Bikini", Slug = "kadin-mayo-bikini", Gender = "Kadın" };
                    var kadinAyakkabi = new Category { Name = "Ayakkabı", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Ayakkabı", Slug = "kadin-ayakkabi", Gender = "Kadın" };
                    var kadinCanta = new Category { Name = "Çanta", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Çanta", Slug = "kadin-canta", Gender = "Kadın" };
                    var kadinAksesuarCanta = new Category { Name = "Aksesuar & Çanta", ParentCategory = kadin, Type = "Ürün Grubu", Value = "Aksesuar & Çanta", Slug = "kadin-aksesuar-canta", Gender = "Kadın" };


                    // ERKEK İÇİN
                    var erkekUstGiyim = new Category { Name = "Üst Giyim", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Üst Giyim", Slug = "erkek-ust-giyim", Gender = "Erkek" };
                    var erkekAltGiyim = new Category { Name = "Alt Giyim", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Alt Giyim", Slug = "erkek-alt-giyim", Gender = "Erkek" };
                    var erkekDisGiyim = new Category { Name = "Dış Giyim", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Dış Giyim", Slug = "erkek-dis-giyim", Gender = "Erkek" };
                    var erkekAyakkabi = new Category { Name = "Ayakkabı", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Ayakkabı", Slug = "erkek-ayakkabi", Gender = "Erkek" };
                    var erkekCanta = new Category { Name = "Çanta", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Çanta", Slug = "erkek-canta", Gender = "Erkek" };
                    var erkekSaatAksesuar = new Category { Name = "Saat & Aksesuar", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Saat & Aksesuar", Slug = "erkek-saat-aksesuar", Gender = "Erkek" };
                    var erkekBuyukBeden = new Category { Name = "Büyük Beden", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Büyük Beden", Slug = "erkek-buyuk-beden", Gender = "Erkek" };
                    var erkekIcGiyim = new Category { Name = "İç Giyim", ParentCategory = erkek, Type = "Ürün Grubu", Value = "İç Giyim", Slug = "erkek-ic-giyim", Gender = "Erkek" };
                    var erkekKisiselBakim = new Category { Name = "Kişisel Bakım", ParentCategory = erkek, Type = "Ürün Grubu", Value = "Kişisel Bakım", Slug = "erkek-kisisel-bakim", Gender = "Erkek" };


                    // ANNE & ÇOCUK İÇİN
                    var bebekAna = new Category { Name = "Bebek", ParentCategory = anneCocuk, Type = "Yaş Grubu Kategori", Value = "Bebek", Slug = "bebek-ana", Gender = "Çocuk" };
                    var kizCocukAna = new Category { Name = "Kız Çocuk", ParentCategory = anneCocuk, Type = "Yaş Grubu Kategori", Value = "Kız Çocuk", Slug = "kiz-cocuk-ana", Gender = "Çocuk" };
                    var erkekCocukAna = new Category { Name = "Erkek Çocuk", ParentCategory = anneCocuk, Type = "Yaş Grubu Kategori", Value = "Erkek Çocuk", Slug = "erkek-cocuk-ana", Gender = "Çocuk" };
                    var bebekBakim = new Category { Name = "Bebek Bakım", ParentCategory = anneCocuk, Type = "Bölüm", Value = "Bebek Bakım", Slug = "bebek-bakim", Gender = "Çocuk" };
                    var cocukOyuncak = new Category { Name = "Oyuncak", ParentCategory = anneCocuk, Type = "Bölüm", Value = "Oyuncak", Slug = "cocuk-oyuncak", Gender = "Çocuk" };
                    var beslenmeEmzirme = new Category { Name = "Beslenme & Emzirme", ParentCategory = anneCocuk, Type = "Bölüm", Value = "Beslenme & Emzirme", Slug = "beslenme-emzirme", Gender = "Çocuk" };


                    context.Categories.AddRange(
                        kadinUstGiyim, kadinAltGiyim, kadinDisGiyim, kadinMayoBikini, kadinAyakkabi, kadinCanta, kadinAksesuarCanta,
                        erkekUstGiyim, erkekAltGiyim, erkekDisGiyim, erkekAyakkabi, erkekCanta, erkekSaatAksesuar, erkekBuyukBeden, erkekIcGiyim, erkekKisiselBakim,
                        bebekAna, kizCocukAna, erkekCocukAna, bebekBakim, cocukOyuncak, beslenmeEmzirme
                    );
                    context.SaveChanges();

                    // SEVİYE 3 (YENİ "ALT KATEGORİ"): Ürün Tipi Kategorileri

                    // KADIN ÜST GİYİM ALTINDAKİLER
                    var kadinTisort = new Category { Name = "Tişört", ParentCategory = kadinUstGiyim, Type = "Ürün Tipi", Value = "Tişört", Slug = "kadin-ust-giyim-tisort", Gender = "Kadın" };
                    var kadinElbise = new Category { Name = "Elbise", ParentCategory = kadinUstGiyim, Type = "Ürün Tipi", Value = "Elbise", Slug = "kadin-ust-giyim-elbise", Gender = "Kadın" };
                    var kadinGomlek = new Category { Name = "Gömlek", ParentCategory = kadinUstGiyim, Type = "Ürün Tipi", Value = "Gömlek", Slug = "kadin-ust-giyim-gomlek", Gender = "Kadın" };
                    var kadinBluz = new Category { Name = "Bluz", ParentCategory = kadinUstGiyim, Type = "Ürün Tipi", Value = "Bluz", Slug = "kadin-ust-giyim-bluz", Gender = "Kadın" };
                    var kadinKazak = new Category { Name = "Kazak", ParentCategory = kadinUstGiyim, Type = "Ürün Tipi", Value = "Kazak", Slug = "kadin-ust-giyim-kazak", Gender = "Kadın" };
                    var kadinHirka = new Category { Name = "Hırka", ParentCategory = kadinUstGiyim, Type = "Ürün Tipi", Value = "Hırka", Slug = "kadin-ust-giyim-hirka", Gender = "Kadın" };
                    var kadinSweatshirt = new Category { Name = "Sweatshirt", ParentCategory = kadinUstGiyim, Type = "Ürün Tipi", Value = "Sweatshirt", Slug = "kadin-ust-giyim-sweatshirt", Gender = "Kadın" };

                    // KADIN ALT GİYİM ALTINDAKİLER
                    var kadinKotPantolon = new Category { Name = "Kot Pantolon", ParentCategory = kadinAltGiyim, Type = "Ürün Tipi", Value = "Kot Pantolon", Slug = "kadin-alt-giyim-kot-pantolon", Gender = "Kadın" };
                    var kadinPantolon = new Category { Name = "Pantolon", ParentCategory = kadinAltGiyim, Type = "Ürün Tipi", Value = "Pantolon", Slug = "kadin-alt-giyim-pantolon", Gender = "Kadın" };
                    var kadinEtek = new Category { Name = "Etek", ParentCategory = kadinAltGiyim, Type = "Ürün Tipi", Value = "Etek", Slug = "kadin-alt-giyim-etek", Gender = "Kadın" };

                    // KADIN DIŞ GİYİM ALTINDAKİLER
                    var kadinMont = new Category { Name = "Mont", ParentCategory = kadinDisGiyim, Type = "Ürün Tipi", Value = "Mont", Slug = "kadin-dis-giyim-mont", Gender = "Kadın" };
                    var kadinCeket = new Category { Name = "Ceket", ParentCategory = kadinDisGiyim, Type = "Ürün Tipi", Value = "Ceket", Slug = "kadin-dis-giyim-ceket", Gender = "Kadın" };
                    var kadinKotCeket = new Category { Name = "Kot Ceket", ParentCategory = kadinDisGiyim, Type = "Ürün Tipi", Value = "Kot Ceket", Slug = "kadin-dis-giyim-kot-ceket", Gender = "Kadın" };
                    var kadinKaban = new Category { Name = "Kaban", ParentCategory = kadinDisGiyim, Type = "Ürün Tipi", Value = "Kaban", Slug = "kadin-dis-giyim-kaban", Gender = "Kadın" };
                    var kadinTrenckot = new Category { Name = "Trençkot", ParentCategory = kadinDisGiyim, Type = "Ürün Tipi", Value = "Trençkot", Slug = "kadin-dis-giyim-trenckot", Gender = "Kadın" };
                    var kadinYagmurlukRuzgarlik = new Category { Name = "Yağmurluk & Rüzgarlık", ParentCategory = kadinDisGiyim, Type = "Ürün Tipi", Value = "Yağmurluk & Rüzgarlık", Slug = "kadin-dis-giyim-yagmurluk-ruzgarlik", Gender = "Kadın" };
                    var kadinPalto = new Category { Name = "Palto", ParentCategory = kadinDisGiyim, Type = "Ürün Tipi", Value = "Palto", Slug = "kadin-dis-giyim-palto", Gender = "Kadın" };

                    // KADIN MAYO & BİKİNİ ALTINDAKİLER
                    var kadinBikini = new Category { Name = "Bikini", ParentCategory = kadinMayoBikini, Type = "Ürün Tipi", Value = "Bikini", Slug = "kadin-mayo-bikini-bikini", Gender = "Kadın" };
                    var kadinMayo = new Category { Name = "Mayo", ParentCategory = kadinMayoBikini, Type = "Ürün Tipi", Value = "Mayo", Slug = "kadin-mayo-bikini-mayo", Gender = "Kadın" };

                    // KADIN AYAKKABI ALTINDAKİLER
                    var kadinTopukluAyakkabi = new Category { Name = "Topuklu Ayakkabı", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Topuklu Ayakkabı", Slug = "kadin-ayakkabi-topuklu", Gender = "Kadın" };
                    var kadinSneaker = new Category { Name = "Sneaker", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Sneaker", Slug = "kadin-ayakkabi-sneaker", Gender = "Kadın" };
                    var kadinGunlukAyakkabi = new Category { Name = "Günlük Ayakkabı", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Günlük Ayakkabı", Slug = "kadin-ayakkabi-gunluk", Gender = "Kadın" };
                    var kadinBabet = new Category { Name = "Babet", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Babet", Slug = "kadin-ayakkabi-babet", Gender = "Kadın" };
                    var kadinSandalet = new Category { Name = "Sandalet", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Sandalet", Slug = "kadin-ayakkabi-sandalet", Gender = "Kadın" };
                    var kadinBot = new Category { Name = "Bot", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Bot", Slug = "kadin-ayakkabi-bot", Gender = "Kadın" };
                    var kadinCizme = new Category { Name = "Çizme", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Çizme", Slug = "kadin-ayakkabi-cizme", Gender = "Kadın" };
                    var kadinKarBotu = new Category { Name = "Kar Botu", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Kar Botu", Slug = "kadin-ayakkabi-kar-botu", Gender = "Kadın" };
                    var kadinLoafer = new Category { Name = "Loafer", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Loafer", Slug = "kadin-ayakkabi-loafer", Gender = "Kadın" };
                    var kadinEvTerligi = new Category { Name = "Ev Terliği", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Ev Terliği", Slug = "kadin-ayakkabi-ev-terligi", Gender = "Kadın" };
                    var kadinKosuAyakkabisi = new Category { Name = "Koşu Ayakkabısı", ParentCategory = kadinAyakkabi, Type = "Ürün Tipi", Value = "Koşu Ayakkabısı", Slug = "kadin-ayakkabi-kosu", Gender = "Kadın" };

                    // KADIN ÇANTA ALTINDAKİLER
                    var kadinOmuzCantasi = new Category { Name = "Omuz Çantası", ParentCategory = kadinCanta, Type = "Ürün Tipi", Value = "Omuz Çantası", Slug = "kadin-canta-omuz", Gender = "Kadın" };
                    var kadinSirtCantasi = new Category { Name = "Sırt Çantası", ParentCategory = kadinCanta, Type = "Ürün Tipi", Value = "Sırt Çantası", Slug = "kadin-canta-sirt", Gender = "Kadın" };
                    var kadinBelCantasi = new Category { Name = "Bel Çantası", ParentCategory = kadinCanta, Type = "Ürün Tipi", Value = "Bel Çantası", Slug = "kadin-bel-cantasi", Gender = "Kadın" };
                }
            }
        }
    }
}