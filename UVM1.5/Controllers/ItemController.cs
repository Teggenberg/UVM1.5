﻿using Microsoft.AspNetCore.Http;
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
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

            

            return View("../Home/Success");
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
            string select = "select * from item" +
                "\r\njoin Brands on Brand = Brands.Id" +
                "\r\njoin Category on Category = Category.Id" +
                "\r\nOrder by Created desc;";
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

            /*OpenAIController ai = new OpenAIController();
            string prompt = $" Does this photo show  a {item.Brand.Name} {item.Model} {item.Category.Name}. Please respond with yes or no.";
            ai.CheckImage(prompt, "https://uvmprototype.com/Images/temp.jpg");*/

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

        public ActionResult WebList()
        {
            List<ListVM?> list = new List<ListVM?>();
            string select = "select * from item" +
                "\r\njoin Brands on Brand = Brands.Id" +
                "\r\njoin Category on Category = Category.Id" +
                "\r\nOrder by Created desc;";
            DataTable items = DBQuery.SelectAll(select);

            for (int i = 0; i < items.Rows.Count; i++)
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
        public ActionResult WebView(int id)
        {


          

            Item item = GetItem(id);

            item.Condition.Name = Condition(item.Condition.Value);

            return View(item);
        }

		public ActionResult PhotoList()
		{
			List<ListVM?> list = new List<ListVM?>();
			string select = "select * from item" +
                "\r\njoin Brands on Brand = Brands.Id" +
                "\r\njoin Category on Category = Category.Id" +
                "\r\nOrder by Created desc;";
			DataTable items = DBQuery.SelectAll(select);

			for (int i = 0; i < items.Rows.Count; i++)
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
                if(item.DisplayIMG != null)
                {
					list.Add(item);
				}
				

			}
			return View(list);
		}

        public ActionResult Validate(int id)
        {
            Item item = GetItem(id);

            item.Condition.Name = Condition(item.Condition.Value);

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> ValidatePhotos(int id)
        {
            Item item = GetItem(id);
            int i = 1;
            foreach(var img in item.Images)
            {
                if(img.Img != null)
                {
                    TempFile(img.Img, $"temp{i}.jpg");
                    i++;
                }
            }
            i = 1;
            OpenAIController ai = new OpenAIController();
            string prompt = $"Does this image contain a {item.Category.Name} " +
                $"or part of a {item.Category.Name}? Please respond with 'Yes' or 'No' only.";
            
            foreach (var img in item.Images)
            {
                if (img.Img != null)
                {
                    string url = $"https://uvmprototype.com/Images/temp{i}.jpg";
                    string isValid = await ai.CheckImage(url, prompt);
                    if (isValid[0] == 'N' || isValid[0] == 'n')
                    {
                        DBQuery.FlagImage(item.Id, img.Position);
                    }
                    else
                    {
                        DBQuery.VerifyImage(item.Id, img.Position); 
                    }
                    isValid = CorrectPunctuation(isValid);
                    string res = "Insert into AI_responses (Item, Response)" +
                        $"\r\n values ({item.Id}, '{isValid}')";
                    DBQuery.Insert(res);

                }
                i++;
                
            }

            item = GetItem(id);


            return View("Validate",item);

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
                "\r\njoin Locations on LocationID = Loc" +
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
                    Serial = (string?)items.Rows[i][13],
                    LocName = (string?)items.Rows[i][19],
                };

                item.Images = DBQuery.GetImages(item.Id);

            }


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

        public void TempFile(byte[] img, string filename)
        {
            if(img != null)
            {
                byte[] filedata = img;
                string extension = "jpg"; // "pdf", etc



                string path = $"wwwroot/images/{filename}";
                System.IO.File.WriteAllBytes(path, filedata);

                System.Diagnostics.Debug.WriteLine(filename);



            }

        }

        public string Condition(int? condition)
        {
            switch(condition)
            {
                case 0:
                    {
                        return "Poor";
                    }
                case 1:
                    {
                        return "Fair";
                    }
                case 2:
                    {
                        return "Good";
                    }
                case 3:
                    {
                        return "Great";
                    }
                default:
                    {
                        return "Excellent";
                    }
            }

        }
    }
}
