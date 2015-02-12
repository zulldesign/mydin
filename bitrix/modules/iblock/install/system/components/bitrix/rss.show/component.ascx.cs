using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Xml;

using Bitrix;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.UI;

public class BXTemplateRssImage
{
	public string url;
	public int width;
	public int height;
}

public class BXTemplateRssItem
{
	public BXTemplateRssItem(){}

	public string Name;
	// Отформатированная дата публикации элемента
	public string DisplayDate;
	// Ссылка на страницу детального просмотра
	public string DetailUrl;
	// Описание
	public string Description;
	// Изображение
	public BXTemplateRssImage Image;
	public string Category;
}

public partial class RssShow : BXComponent
{
	#region Results

	public new string Name
	{ get {return HttpUtility.HtmlEncode(Results.Get("Title", ""));}}
	public string Link
	{ get {return HttpUtility.HtmlEncode(Results.Get("Link", ""));}}
	public string RssDescription
	{ get {return HttpUtility.HtmlEncode(Results.Get("Description", ""));}}	
	public DateTime BuildDate
	{ get { return Results.Get<DateTime>("BuildDate"); } }	
	public int TTL
	{ get {return Results.Get<int>("Description");}}	
	public BXTemplateRssImage Image
	{ get {return Results.Get<BXTemplateRssImage>("Image");}}	

	// Список выбранных элементов
	public List<BXTemplateRssItem> Items
	{ get { return (List<BXTemplateRssItem>)Results["Items"]; } }
	#endregion

	protected void Page_Load(object sender, EventArgs e)
	{
		Parameters["RssURL"] = Parameters.Get("RssURL", "");
		Parameters["ElementsCount"] = Parameters.Get("ElementsCount", 0).ToString();

		if (!IsCached())
		{
			string rssURL = Parameters.Get("RssURL", "");
			XmlDocument rssDoc = null;
			try
			{
				XmlReader reader = XmlReader.Create(rssURL);
				rssDoc = new XmlDocument();
				rssDoc.Load(reader);
			}
			catch (Exception excc)
			{
				rssDoc = null;
				Results["msg"] = GetMessage("Error.AnErrorHasOccurredWhileReading")+" ("+HttpUtility.HtmlEncode(excc.Message)+")";
			}

			List<BXTemplateRssItem> items = new List<BXTemplateRssItem>();
			Results["Items"] = items;

			if (rssDoc != null)
			{
				XmlNode rssChannel = rssDoc.SelectSingleNode("/rss/channel");
				XmlNode n;
				
				n =  rssChannel.SelectSingleNode("title");
				Results["Title"] = n != null ? n.InnerText : "";
				
				n =  rssChannel.SelectSingleNode("description");
				Results["Description"] = n != null ? n.InnerText : "";
				
				n = rssChannel.SelectSingleNode("link");
				Results["Link"] = n != null ? n.InnerText : "";

				if (rssChannel.SelectSingleNode("lastBuildDate") != null)
					Results["BuildDate"] = System.DateTime.Parse(rssChannel.SelectSingleNode("lastBuildDate").InnerText);
				if (rssChannel.SelectSingleNode("ttl") != null)
					Results["TTL"] = System.Int32.Parse(rssChannel.SelectSingleNode("ttl").InnerText);

                int intVar;
				XmlNode xmlRssEnclosure = rssChannel.SelectSingleNode("image");
				if (xmlRssEnclosure != null)
				{
                    
					BXTemplateRssImage encImage = new BXTemplateRssImage();
                    if (xmlRssEnclosure.Attributes["height"]!=null)
                        if (int.TryParse(xmlRssEnclosure.Attributes["height"].InnerText, out intVar))
                            encImage.height = intVar;
                    if (xmlRssEnclosure.Attributes["width"] != null)
                        if (int.TryParse(xmlRssEnclosure.Attributes["width"].InnerText, out intVar))
                            encImage.width = intVar;
                    /*
					encImage.height = System.Int32.Parse(xmlRssEnclosure.SelectSingleNode("height").InnerText);
					encImage.width = System.Int32.Parse(xmlRssEnclosure.SelectSingleNode("width").InnerText);
                     */
					encImage.url = HttpUtility.HtmlEncode(xmlRssEnclosure.SelectSingleNode("url").InnerText);
					Results["Image"] = encImage;
				}

				int cnt = 0;
				XmlNodeList xmlItems = rssChannel.SelectNodes("item");
				foreach (XmlNode xmlItem in xmlItems)
				{
					cnt++;

					BXTemplateRssItem item = new BXTemplateRssItem();
					item.Name = HttpUtility.HtmlEncode(xmlItem.SelectSingleNode("title").InnerText);
					item.Description = xmlItem.SelectSingleNode("description").InnerText;
					item.DetailUrl = HttpUtility.HtmlEncode(xmlItem.SelectSingleNode("link").InnerText);

                    if (xmlItem.SelectSingleNode("pubDate") != null)
                    {
                        if (xmlItem.SelectSingleNode("pubDate").InnerText.Length > 0)
                            item.DisplayDate = ParseDate(xmlItem.SelectSingleNode("pubDate").InnerText).ToString("d");
                        else item.DisplayDate = "";
                    }
                    else
                        item.DisplayDate = "";

					if (xmlItem.SelectSingleNode("category") != null)
						item.Category = HttpUtility.HtmlEncode(xmlItem.SelectSingleNode("category").InnerText);

					XmlNode xmlEnclosure = xmlItem.SelectSingleNode("enclosure");
					if (xmlEnclosure != null)
					{
                        
						item.Image = new BXTemplateRssImage();
                        if(xmlEnclosure.Attributes["height"]!=null)
                            if(int.TryParse(xmlEnclosure.Attributes["height"].InnerText,out intVar))
						        item.Image.height = intVar;

                        if (xmlEnclosure.Attributes["width"] != null)
                            if (int.TryParse(xmlEnclosure.Attributes["width"].InnerText, out intVar))
						        item.Image.width = intVar;

						item.Image.url = HttpUtility.HtmlEncode(xmlEnclosure.Attributes["url"].InnerText);
					}

					items.Add(item);

					if (Parameters.Get<int>("ElementsCount") > 0 && cnt >= Parameters.Get<int>("ElementsCount"))
						break;
				}
			}

			IncludeComponentTemplate();
		}
	}

	private DateTime ParseDate(string s)
	{
		if (s.EndsWith("UT"))
			s = s.Remove(s.Length - 2) + "+0000";
		else if (s.EndsWith("GMT"))
			s = s.Remove(s.Length - 3) + "+0000";
		else if (s.EndsWith("EST"))
			s = s.Remove(s.Length - 3) + "-0500";
		else if (s.EndsWith("EDT"))
			s = s.Remove(s.Length - 3) + "-0400";
		else if (s.EndsWith("CST"))
			s = s.Remove(s.Length - 3) + "-0600";
		else if (s.EndsWith("CDT"))
			s = s.Remove(s.Length - 3) + "-0500";
		else if (s.EndsWith("MST"))
			s = s.Remove(s.Length - 3) + "-0700";
		else if (s.EndsWith("MDT"))
			s = s.Remove(s.Length - 3) + "-0600";
		else if (s.EndsWith("PST"))
			s = s.Remove(s.Length - 3) + "-0800";
		else if (s.EndsWith("PDT"))
			s = s.Remove(s.Length - 3) + "-0700";
		try
		{
			return DateTime.Parse(s);
		}
		catch
		{
			return DateTime.MinValue;
		}
	}

	protected override void PreLoadComponentDefinition()
	{
		base.Title = GetMessage("RssShow.Title");
		base.Description = GetMessage("RssShow.Description");
		base.Icon = "images/rss_in.gif";

        Group = new BXComponentGroup("rss", "RSS", 150, BXComponentGroup.Content);

		ParamsDefinition.Add(BXParametersDefinition.Cache);

		ParamsDefinition.Add(
			"RssURL",
			new BXParamText(
				GetMessageRaw("RssUrl"),
				"http://www.1c-bitrix.ru/rss.php",
				BXCategory.Main
			)
		);
		ParamsDefinition["RssURL"].RefreshOnDirty = true;


		ParamsDefinition.Add(
			"ElementsCount",
			new BXParamText(
				GetMessageRaw("DisplayedElementsCount"),
				"20",
				BXCategory.Main
			)
		);
		ParamsDefinition["ElementsCount"].RefreshOnDirty = true;
	}

	protected override void LoadComponentDefinition()
	{
		base.LoadComponentDefinition();
	}
}
