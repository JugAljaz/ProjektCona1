﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProjektCona1.Models;
using X.PagedList;

namespace ProjektCona1.Controllers
{
    public class PodatkiController : Controller
    {
        private pc1Context db = new pc1Context();

        // GET: Podatki
        public ActionResult Index(int? page)
        {
            var dataTable = (from x in db.Podatkis
                        orderby x.Id descending
                        select x).Take(12000);
            
            var dataGraph = (from x in dataTable
                        orderby x.Id descending
                        select x).Take(108);
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfData = dataTable.ToPagedList(pageNumber, 60); // will only contain 25 products max because of the pageSize

            ViewBag.OnePageOfData = onePageOfData;

            var tpovp = from t in dataGraph
                        group t by new
                        {
                            t.Cas
                        } into g
                        select new
                        {
                            PovpTemp = g.Average(p => p.Temp),
                            g.Key.Cas
                        };

            var vlpovp = from t in dataGraph
                        group t by new
                        {
                            t.Cas
                        } into g
                        select new
                        {
                            PovpVlg = g.Average(p => p.Vlaga),
                            g.Key.Cas
                        };

            ViewData["TempAvg"] = tpovp;
            ViewData["VlagaAvg"] = vlpovp;

            return View(dataTable);
        }

        public ActionResult Postaja(int? id)
        {
            int stevilka = id ?? 1;

            var data = (from x in db.Podatkis
                        where x.IdPostaje == stevilka
                        orderby x.Id descending
                        select x).Take(100);

            var dataDan = (from x in db.Podatkis
                        where x.IdPostaje == stevilka
                        where x.Cas.Minute == 0
                        orderby x.Id descending
                        select new { x.Cas, x.Temp, x.Vlaga, x.Padavine }).Take(25);
            
            var dataTeden = (from x in db.Podatkis
                             where x.IdPostaje == stevilka
                             orderby x.Id descending
                             group x by DbFunctions.TruncateTime(x.Cas) into g
                             select new
                             {
                                 datum = g.Key,
                                 tempMax = g.Max(z => z.Temp),
                                 tempMin = g.Min(z => z.Temp),
                                 padSUM = g.Sum(z => z.Padavine),
                                 vlagaAVG = g.Average(z=>z.Vlaga)
                             }).Take(7);

            var dataMesec = (from x in db.Podatkis
                             where x.IdPostaje == stevilka
                             orderby x.Id descending
                             group x by DbFunctions.TruncateTime(x.Cas) into g
                             select new
                             {
                                 datum = g.Key,
                                 tempMax = g.Max(z => z.Temp),
                                 tempMin = g.Min(z => z.Temp),
                                 padSUM = g.Sum(z=>z.Padavine),
                                 vlagaAVG = g.Average(z => z.Vlaga)
                             }).Take(30);

            ViewData["Dan"] = dataDan;
            ViewData["Teden"] = dataTeden;
            ViewData["Mesec"] = dataMesec;
            ViewData["id"] = stevilka;

            return View(data);
        }

        public ActionResult PostajaLive (int? id)
        {
            int stevilka = id ?? 1;

            var data = (from x in db.Podatkis
                        where x.IdPostaje == stevilka
                        orderby x.Id descending
                        select x).Take(10);

            var model = from x in data
                        orderby x.Id
                        select x;

            ViewData["id"] = stevilka;

            return View(model);
        }

        public ActionResult _PostajaLive(int? id)
        {
            int stevilka = id ?? 1;

            var data = (from x in db.Podatkis
                        where x.IdPostaje == stevilka
                        orderby x.Id descending
                        select x).Take(10);

            var model = from x in data
                        orderby x.Id
                        select x;

            return View("_podatki", model);
        }

    }
}
