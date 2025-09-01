using Microsoft.AspNetCore.Mvc;
using dotnet_store.Models;

namespace dotnet_store.Controllers;

public class SliderController : Controller
{
    public ActionResult Index()
    {
        // Örnek veri - gerçek uygulamada veritabanından gelecek
        var sliders = new List<SliderGetModel>
        {
            new SliderGetModel { Id = 1, Baslik = "Slider 1", Resim = "slider-1.jpeg", Aktif = true, Index = 1 },
            new SliderGetModel { Id = 2, Baslik = "Slider 2", Resim = "slider-2.jpeg", Aktif = true, Index = 2 },
            new SliderGetModel { Id = 3, Baslik = "Slider 3", Resim = "slider-3.jpeg", Aktif = false, Index = 3 }
        };
        
        return View(sliders);
    }
    
    public ActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    public ActionResult Create(SliderGetModel model)
    {
        if (ModelState.IsValid)
        {
            // Burada veritabanına kayıt işlemi yapılacak
            return RedirectToAction("Index");
        }
        return View(model);
    }
    
    public ActionResult Edit(int id)
    {
        // Örnek veri - gerçek uygulamada veritabanından gelecek
        var slider = new SliderGetModel { Id = id, Baslik = "Slider " + id, Resim = "slider-" + id + ".jpeg", Aktif = true, Index = id };
        return View(slider);
    }
    
    [HttpPost]
    public ActionResult Edit(int id, SliderGetModel model)
    {
        if (ModelState.IsValid)
        {
            // Burada veritabanında güncelleme işlemi yapılacak
            return RedirectToAction("Index");
        }
        return View(model);
    }
    
    public ActionResult Delete(int id)
    {
        // Burada veritabanından silme işlemi yapılacak
        return RedirectToAction("Index");
    }
}
