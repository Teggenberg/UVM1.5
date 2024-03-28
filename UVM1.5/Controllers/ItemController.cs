using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UVM1._5.Models;
using System.Diagnostics;
using System;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.CodeAnalysis;

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
            ViewBag.categories = Categories();
            ViewBag.locations = DBQuery.GetOptions("Locations");
            ViewBag.brands = Brands();
            return View(item);
        }

        // POST: ItemController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Location,Brand,Model,Color,Year,Category,Serial,Condition,Retail,Cost,Description,Details")] Item item)
        {

           

           
            try
            {
                ViewBag.categories = Categories();
                ViewBag.locations = DBQuery.GetOptions("Locations");
                ViewBag.brands = Brands();
                //return RedirectToAction(nameof(Index));
                return View(item);
            }
            catch
            {
                return View();
            }
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
    }
}
