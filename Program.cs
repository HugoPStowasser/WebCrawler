﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text.Json;
using MongoDB.Driver;
using WebCrawler.Model;
using WebCrawler.Constants;
using MongoDB.Bson;

namespace WebCrawler
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            string baseUrl = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc";
            string jsonFilename = "output.json";
            DateTime startTime = DateTime.Now;
            DateTime endTime;

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");
            using (IWebDriver driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl(baseUrl);
                HtmlDocument mainPage = new HtmlDocument();
                mainPage.LoadHtml(driver.PageSource);

                int numPages = await GetNumPages(mainPage, baseUrl);

                List<CrawlerData> allData = new List<CrawlerData>();

                for (int page = 1; page <= numPages; page++)
                {
                    string url = $"{baseUrl}/page/{page}";
                    var pageContent = await ExtractPageContent(driver, url);
                    var document = new HtmlDocument();
                    document.LoadHtml(pageContent);

                    List<CrawlerData> data = ExtractDataFromPage(document);
                    allData.AddRange(data);

                    SavePageHtml(pageContent, $"page_{page}.html"); // Salvar o HTML da página em um arquivo separado
                }

                SaveDataToJson(allData, jsonFilename);

                endTime = DateTime.Now;

                SaveDataToDatabase(startTime, endTime, numPages, allData);

                driver.Quit();
            }
         
            Console.WriteLine("Web crawling completed. " +
                $"\nStart time: {startTime} " +
                $"\nEnd time: {endTime}");
        }

        private static async Task<int> GetNumPages(HtmlDocument document, string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = await web.LoadFromWebAsync(url);
            HtmlNode pagination = doc.DocumentNode.SelectSingleNode("//ul[contains(@class, 'pagination')]");

            int numPages = 0;

            if (pagination != null)
            {
                HtmlNodeCollection pageLinks = pagination.SelectNodes(".//a[@href]");
                if (pageLinks != null && pageLinks.Count > 0)
                {
                    foreach (HtmlNode pageLink in pageLinks)
                    {
                        string pageText = pageLink.InnerText.Trim();
                        if (!string.IsNullOrEmpty(pageText) && int.TryParse(pageText, out int pageNumber))
                        {
                            numPages = Math.Max(numPages, pageNumber);
                        }
                        else if (pageText == "...")
                        {
                            HtmlNode nextPageLink = pageLink.NextSibling;
                            if (nextPageLink != null)
                            {
                                string nextPageText = nextPageLink.InnerText.Trim();
                                if (int.TryParse(nextPageText, out int nextPageNumber))
                                {
                                    numPages = Math.Max(numPages, nextPageNumber - 1);
                                }
                            }
                        }
                    }
                }
            }

            return numPages;
        }

        static async Task<string> ExtractPageContent(IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);
            await Task.Delay(2000); // Aguarda um tempo para o conteúdo ser carregado

            return driver.PageSource;
        }

        static List<CrawlerData> ExtractDataFromPage(HtmlDocument document)
        {
            List<CrawlerData> data = new List<CrawlerData>();

            HtmlNodeCollection rowNodes = document.DocumentNode.SelectNodes("//div[@id='content']//table/tbody/tr");

            if (rowNodes != null)
            {
                foreach (HtmlNode rowNode in rowNodes)
                {
                    HtmlNodeCollection cellNodes = rowNode.SelectNodes(".//td");

                    string ipAddress = cellNodes[1].InnerText.Trim();
                    string port = cellNodes[2].InnerText.Trim();
                    string country = cellNodes[3].InnerText.Trim();
                    string protocol = cellNodes[6].InnerText.Trim();

                    CrawlerData rowData = new CrawlerData()
                    {
                        IPAddress = ipAddress,
                        Port = port, 
                        Country = country,
                        Protocol = protocol
                    };

                    data.Add(rowData);
                }
            }

            return data;
        }

        static void SaveDataToJson(List<CrawlerData> data, string filename)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            string outputFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
            Directory.CreateDirectory(outputFolderPath);

            string fullFilePath = Path.Combine(outputFolderPath, filename);
            File.WriteAllText(fullFilePath, json);
        }

        static void SavePageHtml(string pageContent, string filename)
        {
            string outputFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
            Directory.CreateDirectory(outputFolderPath);

            string fullFilePath = Path.Combine(outputFolderPath, filename);
            File.WriteAllText(fullFilePath, pageContent);
        }

        static void SaveDataToDatabase(DateTime startTime, DateTime endTime, int numPages, List<CrawlerData> data)
        {
            MongoClient client = new MongoClient(DbConstants.ConnectionString);
            IMongoDatabase database = client.GetDatabase(DbConstants.DatabaseName);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(DbConstants.CollectionName);

            List<BsonDocument> bsonDocuments = data.Select(d => d.ToBsonDocument()).ToList();

            BsonDocument dbCrawlerData = new BsonDocument
            {
                { "StartTime", startTime },
                { "EndTime", endTime },
                { "NumPages", numPages },
                { "NumLines", bsonDocuments.Count },
                { "Data", new BsonArray(bsonDocuments) }
            };

            collection.InsertOne(dbCrawlerData);
        }
    }
}
