using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UVM1._5.Models;
using System.Diagnostics;
using System;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.CodeAnalysis;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            List<string> cats = new List<string>();
            cats.Add("Effect Pedal");
            cats.Add("Solidbody Electric Guitar");
            cats.Add("Acousitc-Electric Guitar");
            cats.Add("Effect Processor");
            cats.Add("Guitar Combo Amplifier");
            cats.Add("Guitar Head Amplifier");
            cats.Add("Solid Body Bass");
            cats.Add("Folk Instrument");
            cats.Add("Electronic Drum Set");
            cats.Add("Single Kick Pedal");
            cats.Add("Double kick Pedal");
            cats.Add("Cymbal Stand");
            cats.Add("Acoustic Guitar");
            cats.Add("Bass Combo Amplifier");
            cats.Add("Speaker Cabinet");
            cats.Add("Keyboard");
            cats.Add("Pa Powered Speaker");
            cats.Add("Pa Passive Speaker");
            cats.Add("Hollowbody electric guitars");
            cats.Add("Extended range electric guitars");


            return cats;

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

            List<Pair> brands = DBQuery.GetOptions("Brands");
            List<Pair> cats = DBQuery.GetOptions("Category");

            int? brandVal = CheckName(item.Brand.Name, brands);
            item.Category.Value = CheckName(item.Category.Name, cats);

            if(brandVal != -1)
            {
                item.Brand.Value = brandVal;
            }
            else
            {
                item.Brand.Value = DBQuery.Insert($"insert into Brands (Brand_Name) Values ('{b}');");
            }




            /*   try
               {
                   AddItem(item);

                   return View("../Home/index");
               }
               catch
               {
                   ViewBag.categories = DBQuery.GetOptions("Category");
                   ViewBag.locations = DBQuery.GetOptions("Locations");
                   ViewBag.brands = DBQuery.GetOptions("Brands", "Brand_Name");
                   System.Diagnostics.Debug.WriteLine(b + det + "Hello Timmy");
                   return View();
               }*/

            AddItem(item);

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

        // GET: ItemController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ItemController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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
                if(name == item.Name)
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
                "Category, Description, Details, Cost, Retail, Created) Output Inserted.Id " +
                $"\n\bValues ({item.Location}, {item.Brand.Value}, '{item.Model}', '{item.Year}'," +
                $"\n\b '{item.Color}', {item.Condition.Value}, {item.Category.Value}, '{item.Description}', " +
                $"\n\b '{item.Details}', {item.Cost}, {item.Retail}, GetDate()); ";


            return DBQuery.Insert(insert);
        }
        
        public string CorrectPunctuation(string desc)
        {
            string corrected = desc;
            for(int i = 0; i < desc.Length; i++)
            {
                if (desc[i] == '\'')
                {
                    corrected = corrected.Insert(i+1, "\'");
                }
            }

            return corrected;
        }
    }
}
