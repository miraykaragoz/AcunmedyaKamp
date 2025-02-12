using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using SatisProjesi.Models;

namespace SatisProjesi.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDbConnection _connection;

        public HomeController(IDbConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            var sql = @"SELECT t.*,
                                                             m.MusteriAdi,
                                                             m.MusteriSoyadi,
                                                             d.DurumAdi
                                                             FROM TBLTeklif t
                                                             LEFT JOIN TBLMusteri m ON t.MusteriId = m.Id
                                                             LEFT JOIN TBLDurumlar d ON t.DurumId = d.Id";
            
            var satisList = _connection.Query<SatisListesiLeftJoin>(sql).ToList();
            var durumlarListesi = _connection.Query<TBLDurumlar>("SELECT * FROM TBLDurumlar").ToList();
            var musteriListesi = _connection.Query<TBLMusteri>("SELECT * FROM TBLMusteri").ToList();
            
            var teklifListesi = _connection.Query<TBLTeklif>("SELECT * FROM TBLTeklif").ToList();
            foreach (var durum in durumlarListesi)
            {
                durum.Teklifler = new List<TBLTeklif>();

                foreach (var teklif in teklifListesi)
                {
                    if (teklif.DurumId == durum.Id)
                    {
                        durum.Teklifler.Add(teklif);
                    }
                }
            }
            
            var model = new IndexViewModel()
            {
                Teklifler = satisList,
                Durumlar = durumlarListesi,
                Musteriler = musteriListesi
            };
            
            return View(model);
        }

        #region Teklif

        [HttpPost]
        public IActionResult InsertTeklif(TBLTeklif teklif) 
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var sql = @"INSERT INTO TBLTeklif
                                                    (MusteriId,DurumId,TeklifBaslik,Fiyat)
                                                    VALUES
                                                    (@MusteriId,@DurumId,@TeklifBaslik,@Fiyat)";
            
            var insertTeklif = _connection.Execute(sql, teklif);
            
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public IActionResult UpdateTeklif(int Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index");
            }

            var sql = @"SELECT t.*,
                                                             m.MusteriAdi,
                                                             m.MusteriSoyadi,
                                                             d.DurumAdi
                                                             FROM TBLTeklif t
                                                             LEFT JOIN TBLMusteri m ON t.MusteriId = m.Id
                                                             LEFT JOIN TBLDurumlar d ON t.DurumId = d.Id
                                                             WHERE t.Id = @Id";
            
            var selectedTeklif = _connection.QuerySingleOrDefault<SatisListesiLeftJoin>(sql, new {Id});
            var durumlarListesi = _connection.Query<TBLDurumlar>("SELECT * FROM TBLDurumlar").ToList();
            var musteriListesi = _connection.Query<TBLMusteri>("SELECT * FROM TBLMusteri").ToList();
            
            var model = new UpdateTeklifViewModel()
            {
                Teklifler = selectedTeklif,
                Durumlar = durumlarListesi,
                Musteriler = musteriListesi
            };

            return View(model); 
        }

        [HttpPost]
        public IActionResult UpdateTeklif(TBLTeklif teklif)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var sql = @"UPDATE TBLTeklif
                                                     SET
                                                     MusteriId = @MusteriId,
                                                     DurumId = @DurumId,
                                                     TeklifBaslik = @TeklifBaslik,
                                                     Fiyat = @Fiyat
                                                     WHERE Id = @Id";
            
            var updateTeklif = _connection.Execute(sql, teklif);

            return RedirectToAction("Index"); 
        }

        public IActionResult DeleteTeklif(int Id) 
        {
            if (Id == null)
            {
                return RedirectToAction("Index");
            }
            
            var deleteTeklif = _connection.Execute("DELETE FROM TBLTeklif WHERE Id = @Id", new {Id});
            
            return RedirectToAction("Index");
        }

        #endregion

        #region Durum

        [HttpPost]
        public IActionResult InsertDurum(TBLDurumlar durum) 
        {
           if (string.IsNullOrEmpty(durum.DurumAdi) || durum.DurumSirasi <= 0)
            {
                return RedirectToAction("Index");
            }

            var sql = @"INSERT INTO TBLDurumlar
                                                      (DurumAdi,DurumSirasi)
                                                      VALUES
                                                      (@DurumAdi,@DurumSirasi)";
            
            var insertDurum = _connection.Execute(sql, durum);
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult UpdateDurum(int Id) 
        {
            if (Id == null)
            {
                return RedirectToAction("Index");
            }
            
            var selectedDurum = _connection.QuerySingle<TBLDurumlar>("SELECT * FROM TBLDurumlar WHERE Id = @Id", new {Id});

            return View(selectedDurum);
        }

        [HttpPost]
        public IActionResult UpdateDurum(TBLDurumlar durum)
        {
            if (string.IsNullOrEmpty(durum.DurumAdi) || durum.DurumSirasi <= 0)
            {
                return RedirectToAction("Index");
            }

            var sql = @"UPDATE TBLDurumlar
                                                    SET
                                                    DurumAdi = @DurumAdi,
                                                    DurumSirasi =@DurumSirasi
                                                    WHERE Id=@Id";
            
            var updateDurum = _connection.Execute(sql, durum);
            
            return RedirectToAction("Index");
        }

        public IActionResult DeleteDurum(int Id) 
        {
            if (Id == null)
            {
                return RedirectToAction("Index");
            }
            
            var deleteDurum = _connection.Execute("DELETE FROM TBLDurumlar Where Id = @Id", new {Id}); 
            
            return RedirectToAction("Index");
        }

        #endregion

        #region Musteri

        [HttpPost]
        public IActionResult InsertMusteri(TBLMusteri musteri)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var sql = @"INSERT INTO TBLMusteri
                                                      (MusteriAdi,MusteriSoyadi,Eposta,Telefon)
                                                      VALUES
                                                      (@MusteriAdi,@MusteriSoyadi,@Eposta,@Telefon)";
            
            var insertMusteri = _connection.Execute(sql, musteri);
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult UpdateMusteri(int Id) 
        {
            if (Id == null)
            {
                return RedirectToAction("Index");
            }
            
            var selectedMusteri = _connection.QuerySingleOrDefault<TBLMusteri>("SELECT * FROM TBLMusteri Where Id = @Id", new {Id});
            
            return View(selectedMusteri); 
        }

        [HttpPost]
        public IActionResult UpdateMusteri(TBLMusteri musteri) 
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var sql = @"UPDATE TBLMusteri
                                                      SET
                                                      MusteriAdi = @MusteriAdi,
                                                      MusteriSoyadi = @MusteriSoyadi,
                                                      Eposta = @Eposta,
                                                      Telefon = @Telefon
                                                      WHERE Id = @Id";
            
            var updateMusteri = _connection.Execute(sql, musteri);
            
            return RedirectToAction("Index");
        }
        
        public IActionResult DeleteMusteri(int Id) 
        {
            if (Id == null)
            {
                return RedirectToAction("Index");
            }
            
            var deleteMusteri = _connection.Execute("DELETE FROM TBLMusteri WHERE Id = @Id", new { Id });
            
            return RedirectToAction("Index");
        }

        #endregion
    }
}
