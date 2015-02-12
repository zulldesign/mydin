<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.UI.BXComponent" EnableViewState="false" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.Components" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.UI" %>

<script runat="server">
	protected override void PreLoadComponentDefinition()
	{
		Title = GetMessage("Title");
		Description = GetMessage("Description");
	}

    protected override void OnLoad(EventArgs e)
    {
        AbortCache();
        CacheMode = BXCacheMode.None;
        IncludeComponentTemplate();
                
        base.OnLoad(e);
    }
    
    protected override void OnPreRender(EventArgs e)
    {
        int recCount = Component.Results.Get("PagingTotalRecordCount", -1);

        if (
            !Component.ComponentCache.GetBool("PagingShow")
            || (
                !string.Equals(Parameters.GetString("PagingPosition"), Parameters.GetString("CurrentPosition"), StringComparison.InvariantCultureIgnoreCase)
                && !string.Equals(Parameters.GetString("PagingPosition"), "both", StringComparison.InvariantCultureIgnoreCase)
            )
            || recCount < 1
        )
        {
            Visible = false;
            return;
        }

        string urlTemplate = Component.Results.Get("PagingPageUrlTemplate", string.Empty);
        int maxPages = Parameters.GetInt("PagingMaxPages", 5);

        BXPagingHelper paging = Component.Results.Get<BXPagingHelper>("PagingHelper") ?? new BXDirectPagingHelper(recCount, 5);
        int count = Math.Max(paging.Count, 1);
        int selected = (count > 0) ? Math.Min(count, Math.Max(1, Component.Results.Get("PagingPageIndex", 1))) : 0;

        BXPagingOptions pagingOptions = paging.GetPagingOptions(selected) ?? new BXPagingOptions(0, 0);

        bool enableShowAll = Parameters.GetBool("PagingAllowAll");
        bool showAll = enableShowAll && Component.Results.Get("PagingShowAll", false);

        int firstRec = showAll ? 1 : (pagingOptions.startRowIndex + 1);
        int lastRec = showAll ? recCount : (pagingOptions.startRowIndex + pagingOptions.maximumRows);

        List<BXParamsBag<object>> pages = new List<BXParamsBag<object>>();
        pages.Add(null);
        for (int i = 1; i <= count; i++)
        {
            BXParamsBag<object> p = new BXParamsBag<object>();
            p["url"] = (i == 1 && Component.Results.ContainsKey("PagingDefaultPageUrl")) ? Component.Results.Get<string>("PagingDefaultPageUrl") : string.Format(urlTemplate, paging.GetOuterIndex(i));
            p["selected"] = (i == selected && !(enableShowAll && showAll));
            pages.Add(p);
        }

        int leftElement = (selected > 0 && selected <= count) ? selected : 1;
        int rightElement = leftElement + 1;
        List<int> middleRange = new List<int>();
        while ((leftElement > 0 && leftElement <= count) || (rightElement > 0 && rightElement <= count))
        {
            if (middleRange.Count >= maxPages)
                break;
            if (leftElement > 0 && leftElement <= count)
                middleRange.Add(leftElement--);
            if (middleRange.Count >= maxPages)
                break;
            if (rightElement > 0 && rightElement <= count)
                middleRange.Add(rightElement++);
        }
        middleRange.Sort();

        List<int> boundedRange = new List<int>();
    	int borderSize = Math.Max(1, Parameters.GetInt("PagingBoundedBorderSize", 2));
		for (int i = 0; i < borderSize; i++)
		{
			if (count <= 2*i)
				break;
			boundedRange.Add(i + 1);
			
			if (count <= 2*i + 1)
				break;
			boundedRange.Add(count - i);
		}
		if (count > borderSize * 2)
        {
            leftElement = (selected > 0 && selected <= count) ? selected : 1;
            rightElement = leftElement + 1;
            while ((leftElement > 0 && leftElement <= count) || (rightElement > 0 && rightElement <= count))
            {
				if (boundedRange.Count >= maxPages + borderSize * 2)
                    break;
                if (leftElement > 0 && leftElement <= count)
                {
                    if (!boundedRange.Contains(leftElement))
                        boundedRange.Add(leftElement);
                    leftElement--;
                }
				if (boundedRange.Count >= maxPages + borderSize * 2)
                    break;
                if (rightElement > 0 && rightElement <= count)
                {
                    if (!boundedRange.Contains(rightElement))
                        boundedRange.Add(rightElement);
                    rightElement++;
                }
            }
        }
        boundedRange.Sort();

        Results["Title"] = Parameters.Get("PagingTitle");
        Results["Pages"] = pages;
        Results["MiddleRange"] = middleRange.ToArray();
        Results["BoundedRange"] = boundedRange.ToArray();
        Results["FirstPage"] = (count > 1 && selected != 1) ? pages[1].Get<string>("url") : null;
        Results["LastPage"] = (count > 1 && selected != count) ? pages[count].Get<string>("url") : null;
        Results["PrevPage"] = (selected - 1 > 0 && selected - 1 <= count && !showAll) ? pages[selected - 1].Get<string>("url") : null;
        Results["NextPage"] = (selected + 1 > 0 && selected + 1 <= count && !showAll) ? pages[selected + 1].Get<string>("url") : null;

        Results["RecordsCount"] = recCount;
        Results["RecordsShown"] = lastRec - firstRec + 1;
        Results["FirstRecordShown"] = firstRec;
        Results["LastRecordShown"] = lastRec;

        Results["EnableShowAll"] = enableShowAll;
        Results["ShowAll"] = showAll;

        Results["DefaultPageUrl"] = pages[1].Get<string>("url");
        Results["ShowAllUrl"] = Component.Results.Get("PagingShowAllUrl", null);
                
        base.OnPreRender(e);
    }
</script>

