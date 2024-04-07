using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UVM1._5.Models;
using System.Diagnostics;
using System;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.CodeAnalysis;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Data;
using System.Drawing;
using System.IO;

namespace UVM1._5.Controllers
{
    public class ItemController : Controller
    {
        public List<string> Brands()
        {
            List<string> brands = new List<string>();
            brands.Add("Fender");
            brands.Add("Gibson");
            brands.Add("Yamaha");
            brands.Add("Martin");
            brands.Add("Taylor");
            brands.Add("PRS");
            brands.Add("G&L");

            return brands;
        }

        public List<string> Categories()
        {
            List<string> catname = new List<string>();
            List<Pair> categories = DBQuery.GetOptions("Category");
             foreach(var cat in categories)
            {
                catname.Add(cat.Name);
            }



            return catname;

        }

        
        // GET: ItemController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ItemController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ItemController/Create
        public ActionResult Create()
        {
            Item item = new Item();
            ViewBag.categories = DBQuery.GetOptions("Category");
            ViewBag.locations = DBQuery.GetOptions("Locations");
            ViewBag.brands = DBQuery.GetOptions("Brands", "Brand_Name");
            return View(item);
        }

        // POST: ItemController/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateItem(string b, string c, string dsc, string det, 
            [Bind("Location,Model,Color,Year,Serial,Condition,Retail,Cost")] Item item)
        {

            item.Brand = new Pair(b);
            item.Category = new Pair(c);
            item.Description = dsc;
            item.Details = det;
            item.Retail += .99m;
            List<Pair> brands = DBQuery.GetOptions("Brands");
            List<Pair> cats = DBQuery.GetOptions("Category");

            item.Brand.Value = CheckName(item.Brand.Name, brands);
            item.Category.Value = CheckName(item.Category.Name, cats);


            if(item.Brand.Value == -1)
            {
                item.Brand.Value = DBQuery.Insert
                    ($"insert into Brands (Brand_Name) \nOutput Inserted.Id \nValues ('{b}');");
            }

            if(item.Category.Value == -1)
            {
                item.Category.Value = 26;
            }
            

            int image_id = AddItem(item);
            AddImageRows(image_id);

            return View("../Home/index");
        }

        [HttpGet]
        public async Task<JsonResult> GenerateDesc(string yy, string brnd, string mod)
        {

            OpenAIController ai = new OpenAIController();

            return new JsonResult(Ok(await ai.GetDescription($"{yy} {brnd} {mod}")));

        }

        [HttpGet]
        public async Task<JsonResult> GetCat(string brnd, string mod)
        {

            OpenAIController ai = new OpenAIController();

            return new JsonResult(Ok(await ai.GetCategory(Categories(),$"{brnd} {mod}")));

        }

        [HttpGet]
        public async Task<JsonResult> GetYear(string brnd, string mod, string ser)
        {

            OpenAIController ai = new OpenAIController();

            return new JsonResult(Ok(await ai.GetYear(brnd, ser)));

        }

        public ActionResult List()
        {
            List<ListVM?> list = new List<ListVM?>();
            string select = "select * from item\r\njoin Brands on Brand = Brands.Id\r\njoin Category on Category = Category.Id;";
            DataTable items = DBQuery.SelectAll(select);

            for(int i = 0; i < items.Rows.Count; i++)
            {


                ListVM item = new ListVM()
                {
                    Id = Convert.ToInt32(items.Rows[i][0]),
                    Location = Convert.ToInt32(items.Rows[i][1]),
                    Brand = (string?)items.Rows[i][15],
                    Model = (string?)items.Rows[i][3],
                    Year = (string?)items.Rows[i][4],
                    Color = (string?)items.Rows[i][5],
                    Category = (string?)items.Rows[i][17],
                    Cost = (decimal?)items.Rows[i][10],
                    Retail = (decimal?)items.Rows[i][11],
                };
                item.DisplayIMG = DBQuery.GetImage(item.Id); 
                list.Add(item);
          
            }
            return View(list);
        }

        // GET: ItemController/Edit/5
        public ActionResult EditItem(int id)
        {
           

            ViewBag.categories = DBQuery.GetOptions("Category");
            ViewBag.locations = DBQuery.GetOptions("Locations");
            ViewBag.brands = DBQuery.GetOptions("Brands", "Brand_Name");

            Item item = GetItem(id);

            return View(item);
        }

        // POST: ItemController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditItem()
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public void UpdateItemInfo(int _id, string brnd, string mod, string _cat, string _serial, string _color, string _year, string _desc, string _det, decimal _retail, decimal _cost, int _cond )
        {
            List<Pair> brands = DBQuery.GetOptions("Brands");
            List<Pair> cats = DBQuery.GetOptions("Category");

            Item item = new Item()
            {
                Id = _id,
                Brand = new Pair(brnd),
                Model = mod,
                Category = new Pair(_cat),
                Serial = _serial,
                Color = _color,
                Year = _year,
                Description = _desc,
                Details = _det,
                Retail = _retail,
                Cost = _cost,
                Condition = new Pair("", _cond)

            };

            item.Brand.Value = CheckName(item.Brand.Name, brands);
            item.Category.Value = CheckName(item.Category.Name, cats);


            if (item.Brand.Value == -1)
            {
                item.Brand.Value = DBQuery.Insert
                    ($"insert into Brands (Brand_Name) \nOutput Inserted.Id \nValues ('{brnd}');");
            }

            if (item.Category.Value == -1)
            {
                item.Category.Value = 26;
            }
            System.Diagnostics.Debug.WriteLine(_id.ToString() , "testing ", brnd, mod);
            item.Description = CorrectPunctuation(item.Description);    
            DBQuery.UpdateItemInfo(item);

        }

        // GET: ItemController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ItemController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public int? CheckName(string name, List<Pair> list)
        {
            foreach(var item in list)
            {
                if(name.ToUpper() == item.Name.ToUpper())
                {
                    return item.Value!;
                }
            }

            return -1;

        }

        public int AddItem(Item item)
        {
            item.Description = CorrectPunctuation(item.Description);

            string insert = "Insert into Item (Loc, Brand, Model, YYYY, Color, Condition, " +
                "Category, Description, Details, Cost, Retail, Created, Serial) Output Inserted.Id " +
                $"\n\bValues ({item.Location}, {item.Brand.Value}, '{item.Model}', '{item.Year}'," +
                $"\n\b '{item.Color}', {item.Condition.Value}, {item.Category.Value}, '{item.Description}', " +
                $"\n\b '{item.Details}', {item.Cost}, {item.Retail}, GetDate(), '{item.Serial}');";


            return DBQuery.Insert(insert);
        }
        
        public string CorrectPunctuation(string desc)
        {
            string corrected = desc;
            int index = 1;
            for(int i = 0; i < desc.Length; i++)
            {
                if (desc[i] == '\'')
                {
                    corrected = corrected.Insert(i+index, "\'");
                    index ++;   
                }
            }

            return corrected;
        }

        [HttpPost]
        public IActionResult UploadPhotos(int id, IFormFile? image1, IFormFile? image2, IFormFile? last, IFormFile? image3)
        {
            System.Diagnostics.Debug.WriteLine("This is where the photos would be uploaded");

            Item item = new Item();

            //int item_id = int.Parse(id);
            if(image1 != null)
            {
                DBQuery.AddImageToDB(id, 1, ImageHandler.ConvertImageFile(image1));
            }

            if(image2 != null)
            {
                DBQuery.AddImageToDB(id, 2, ImageHandler.ConvertImageFile(image2));
            }

            if (image3 != null)
            {
                DBQuery.AddImageToDB(id, 3, ImageHandler.ConvertImageFile(image3));
            }

            if (last != null)
            {
                DBQuery.AddImageToDB(id, 4, ImageHandler.ConvertImageFile(last));
            }


            ViewBag.categories = DBQuery.GetOptions("Category");
            ViewBag.locations = DBQuery.GetOptions("Locations");
            ViewBag.brands = DBQuery.GetOptions("Brands", "Brand_Name");

            item = GetItem(id);

            return View("EditItem", item);
        }

        public Item GetItem(int id)
        {
            Item item = null;   
            string select = $"select * from item" +
                $"\r\njoin Brands on Brand = Brands.Id" +
                "\r\njoin Category on Category = Category.Id" +
                $"\r\nwhere item.Id = {id};";
            DataTable items = DBQuery.SelectAll(select);

            for (int i = 0; i < items.Rows.Count; i++)
            {
                item = new Item()
                {
                    Id = Convert.ToInt32(items.Rows[i][0]),
                    Location = Convert.ToInt32(items.Rows[i][1]),
                    Brand = new Pair((string?)items.Rows[i][15], Convert.ToInt32(items.Rows[i][2])),
                    Model = (string?)items.Rows[i][3],
                    Year = (string?)items.Rows[i][4],
                    Color = (string?)items.Rows[i][5],
                    Condition = new Pair("test", Convert.ToInt32(items.Rows[i][6])),
                    Category = new Pair((string?)items.Rows[i][17], Convert.ToInt32(items.Rows[i][7])),
                    Description = (string?)items.Rows[i][8],
                    Details = (string?)items.Rows[i][9],
                    Cost = (decimal?)items.Rows[i][10],
                    Retail = (decimal?)items.Rows[i][11],
                    Serial = (string?)items.Rows[i][13]
                };

                item.Images = DBQuery.GetImages(item.Id);

            }

            OpenAIController ai = new OpenAIController();
            ai.GPTVision();

            TempFile(item.Images[0], "");

            return item;
        }

        public void AddImageRows(int? Id)
        {
            

            for(int i = 1; i < 5;  i++)
            {
                string query = "Insert into Item_Image (Item_Id, Position)" +
                $"\r\nValues ({Id},{i})";
                DBQuery.Insert(query);
            }


        }

        public void TempFile(byte[] img, string path)
        {
            if(img != null)
            {
                byte[] filedata = img;
                string extension = "jpg"; // "pdf", etc


                //string filename = System.IO.Path.GetTempFileName() + "." + extension; // Makes something like "C:\Temp\blah.tmp.pdf"
                string filename = "wwwroot/images/temp.jpg";
                System.IO.File.WriteAllBytes(filename, filedata);

                /*var process = Process.Start(filename);
                // Clean up our temporary file...
                process.Exited += (s, e) => System.IO.File.Delete(filename);*/
                System.Diagnostics.Debug.WriteLine(filename);
                //System.IO.File.Delete(filename);
                System.Diagnostics.Debug.WriteLine("\n\n\nplease work");
                //System.IO.File.Delete(filename);

            }

        }
    }
}
