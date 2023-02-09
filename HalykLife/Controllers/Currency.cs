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
        private string _currency = "https://nationalbank.kz/rss/get_rates.cfm?fdate=";
        private TestContext tcontext = new TestContext();

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
        public JsonResult save(string date)
        {
            if (tcontext.RCurrencies.Any(x => x.ADate == DateTime.Parse(date)))
                return new JsonResult("Данные на эту дату уже сохранены");
            XmlElement root = getXml(_currency + date);
            List<RCurrency> list = new List<RCurrency>();
            DateTime ADate = Convert.ToDateTime(root["date"].InnerText);
            foreach (XmlNode node in root.SelectNodes("item"))
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
        ///     Метод предназначен для получение данных на определенной валюты на определенную дату,
        ///     или же всех валют на определенную дату
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
        ///         ...
        /// </returns>
        [HttpGet("{date}/{code?}")]
        public JsonResult show(string date, string? code)
        {
            List<RCurrency> list = new List<RCurrency>();
            if (code == null)
            {       
                list = tcontext.RCurrencies.FromSql($"sp_GetRates @A_DATE ={DateTime.Parse(date)}").ToList();
                return list.Count > 0 ? new JsonResult(list) : new JsonResult("Записи отсутствуют");
            }
            list= tcontext.RCurrencies.FromSql($"sp_GetRates @A_DATE ={DateTime.Parse(date)}, @CODE ={code}").ToList();
            return list.Count > 0 ? new JsonResult(list) : new JsonResult("Записи отсутствуют");

        }

        public static XmlElement? getXml(string url)
        {
            XmlTextReader xtr = new XmlTextReader(url);
            XmlDocument xd = new XmlDocument();
            xd.Load(xtr);
            XmlElement? root = xd.DocumentElement;
            return root;
        }
    }    
}
