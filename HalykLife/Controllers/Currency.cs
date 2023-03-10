using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Xml;
using Newtonsoft;
using Newtonsoft.Json;
using HalykLife.Models;
using System.Text;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace HalykLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Currency : ControllerBase
    {
        private readonly TestContext tcontext = new();
        private readonly IConfiguration _configuration;
        private string? _currency;
        public Currency(IConfiguration configuration)
        {
            _configuration = configuration;
        
        }

        /// <summary>
        ///     Метод предназначен для сохранение данных из публичного API нац. банка в локальную базу данных.
        /// </summary>
        /// <param name="date">
        ///     Принимает в качестве параметра string, дату в формате dd.mm.yyyy
        /// </param>
        /// <example>Пример запроса
        ///     /api/Currency/save/09.02.2023
        /// </example>
        /// <returns>
        ///     Возвращает количество сохранненых записей
        /// </returns>
        [HttpGet("save/{date}")]
        public JsonResult Save(string date)
        {
            _currency = _configuration.GetSection("Urls").GetSection("get_rates").Value;
            if (tcontext.RCurrencies.Any(x => x.ADate == DateTime.Parse(date)))
                return new JsonResult("Данные на эту дату уже сохранены");
            XmlElement? root = GetXml(_currency + date);
            List<RCurrency>? list = new();
            DateTime ADate = Convert.ToDateTime(root?["date"]?.InnerText);
            foreach (XmlNode node in root?.SelectNodes("item"))
            {
                list.Add(new RCurrency()
                {
                    Title = node["fullname"]?.InnerText,
                    Code = node["title"]?.InnerText,
                    Value = Decimal.Parse(node["description"]?.InnerText, CultureInfo.InvariantCulture),
                    ADate = ADate
                });
            }
            tcontext.RCurrencies.AddRange(list);
            tcontext.SaveChanges();
            return new JsonResult(list.Count);
        }

        /// <summary>
        ///     Метод предназначен для получение данных определенной валюты на определенную дату,
        /// </summary>
        /// <param name="date">
        ///     Принимает в качестве параметра string, дату в формате yyyy-mm-dd
        /// </param>
        /// <param name="code">
        ///     Принимает в качестве параметра string, код валюты например "USD"
        /// </param>
        /// <returns>
        ///     Возвращает запись или же массив записей
        ///     Пример
        ///     [
        ///         {
        ///             "id": 40,
        ///             "title": "АВСТРАЛИЙСКИЙ ДОЛЛАР",
        ///             "code": "AUD",
        ///             "value": 318.23,
        ///             "aDate": "2023-02-09T00:00:00"
        ///         }
        ///     ]
        /// </returns>
        [HttpGet("{date}/{code}")]
        public JsonResult GetCurrencyOnDate(string date, string? code)
        {
            List<RCurrency> list;
            list= tcontext.RCurrencies.FromSql($"sp_GetRates @A_DATE ={DateTime.Parse(date)}, @CODE ={code}").ToList();
            return list.Count > 0 ? new JsonResult(list) : new JsonResult("Записи отсутствуют");
        }

        /// <summary>
        ///     Метод предназначен для получение данных о валютах на определенную дату,
        /// </summary>
        /// <param name="date">
        ///     Принимает в качестве параметра string, дату в формате yyyy-mm-dd
        /// </param>
        /// <returns>
        ///     Возвращает массив записей 
        ///     Пример
        ///     [
        ///         {
        ///             "id": 40,
        ///             "title": "АВСТРАЛИЙСКИЙ ДОЛЛАР",
        ///             "code": "AUD",
        ///             "value": 318.23,
        ///             "aDate": "2023-02-09T00:00:00"
        ///         }
        ///         ...
        /// </returns>        
        [HttpGet("{date}")]
        public JsonResult GetCurrenciesOnDate(string date)
        {
            List<RCurrency> list;
            list = tcontext.RCurrencies.FromSql($"sp_GetRates @A_DATE ={DateTime.Parse(date)}").ToList();
            return list.Count > 0 ? new JsonResult(list) : new JsonResult("Записи отсутствуют");
        }

        public static XmlElement? GetXml(string url)
        {
            XmlTextReader? xtr = new(url);
            XmlDocument xd = new();
            xd.Load(xtr);
            XmlElement? root = xd.DocumentElement;
            return root;
        }
    }    
}
