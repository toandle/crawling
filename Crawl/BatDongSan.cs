﻿using HtmlAgilityPack;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Crawl.Helpers;

namespace Crawl
{
    public class BatDongSan
    {
        private readonly ITestOutputHelper _output;

        public BatDongSan(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public async void RunAsync()
        {
            for (int i = 1; i <= 7537; i++)
            {
                var url = string.Format("https://batdongsan.com.vn/nha-dat-ban/p{0}", i);
                var text = await HttpHelper.GetResponseAsync(url);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(text);

                var ids = htmlDocument.DocumentNode
                                      .Descendants()
                                      .Where(x => x.Name == "div" && x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("search-productItem"))
                                      .Select(m => m.Descendants().FirstOrDefault(n => n.Name == "a"))
                                      .Select(x => x.Attributes["href"].Value)
                                      .Select(x => x.Substring(x.LastIndexOf('-')))
                                      .ToList();

                foreach (var id in ids)
                {
                    var result = new Dictionary<string, string>();
                    var path = string.Format("{0}/{1}", url, id);
                    var content = await HttpHelper.GetResponseAsync(path);
                    htmlDocument.LoadHtml(content);
                    var descendants = htmlDocument.DocumentNode.Descendants();

                    var title = descendants
                                           .FirstOrDefault(x => x.Name == "div" && x.Attributes.Contains("class") && x.Attributes["class"].Value == ("pm-title"))
                                           .Descendants()
                                           .FirstOrDefault(x => x.Name == "h1" && x.Attributes.Contains("itemprop") && x.Attributes["itemprop"].Value == ("name"))
                                           .InnerText.Trim().Replace("\r\n", "");

                    var description = descendants
                                                .FirstOrDefault(x => x.Name == "div" && x.Attributes.Contains("class") && x.Attributes["class"].Value == ("pm-desc"))
                                                .InnerText.Trim().Replace("\r\n", "");

                    var price = descendants
                                           .FirstOrDefault(x => x.Name == "div" && x.Attributes.Contains("class") && x.Attributes["class"].Value == ("kqchitiet"))
                                           .Descendants()
                                           .FirstOrDefault(x => x.Name == "strong")
                                           .InnerText.Trim().Replace("\r\n", "").Replace("&nbsp;", "");

                    var area = descendants
                                           .FirstOrDefault(x => x.Name == "div" && x.Attributes.Contains("class") && x.Attributes["class"].Value == ("kqchitiet"))
                                           .Descendants()
                                           .LastOrDefault(x => x.Name == "strong")
                                           .InnerText.Trim().Replace("\r\n", "");

                    var tableDetail = descendants.Where(x => x.Name == "div" && x.Attributes.Contains("class") && x.Attributes["class"].Value == "table-detail");

                    var contacts = tableDetail.LastOrDefault().Descendants().Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "right-content");

                    var data = tableDetail.FirstOrDefault().Descendants().Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "row").Concat(contacts);

                    result.Add("crawl_id", id.Replace("-pr", ""));
                    result.Add("title", title);
                    result.Add("description", description);
                    result.Add("price", price);
                    result.Add("area", area);
                    foreach (var datum in data)
                    {
                        var key = datum.Descendants()
                                       .FirstOrDefault(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("left"))
                                       .InnerText.Trim().Replace("\r\n", "")
                                                        .Replace("Loại tin rao", "type")
                                                        .Replace("Địa chỉ", "address")
                                                        .Replace("Mặt tiền", "facade")
                                                        .Replace("Đường vào", "streetWide")
                                                        .Replace("Hướng nhà", "direction")
                                                        .Replace("Hướng ban công", "direction_of_balcony")
                                                        .Replace("Số tầng", "number_of_floor")
                                                        .Replace("Số phòng ngủ", "number_of_bedrooms")
                                                        .Replace("Số toilet", "number_of_toilet")
                                                        .Replace("Nội thất", "furniture")
                                                        .Replace("Tên liên lạc", "contact_name")
                                                        .Replace("Điện thoại", "phone_number")
                                                        .Replace("Mobile", "mobile");

                        var value = datum.Descendants().FirstOrDefault(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "right").InnerText.Trim().Replace("\r\n", "");
                        if (!result.Keys.Contains(key) && key != "Email")
                        {
                            result.Add(key, value);
                        }
                    }

                    string jsonObject = JsonConvert.SerializeObject(result, Formatting.Indented);
                    FileHelper.WriteFile("E:\\git\\Personal\\crawling\\Data\\BatDongSan.json", jsonObject);
                    _output.WriteLine(jsonObject);
                }
            }
        }
    }
}
