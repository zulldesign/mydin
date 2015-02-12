<%@ Page Language="C#" Inherits="Bitrix.UI.BXJsPage" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Security.Cryptography" %>
<script runat="server">
    //Ид культуры
    private int _cultureID = 0;    
    //Имя культуры
    private string _cultureName = string.Empty;
    //Названия дней недели
    private string[] _dayNames = null;
    //Сокращённые названия дней недели
    private string[] _abbreviatedDayNames = null;
    //Названия месяцев
    private string[] _monthNames = null;
    //Сокращённые названия месяцев
    private string[] _abbreviatedMonthNames = null;
    private string _amDesignator = string.Empty;
    private string _pmDesignator = string.Empty;
    private int _calendarTwoDigitYearMax = 2029;
    private string _timeSeparator = ":";
    private Dictionary<string, string> _patternsDic = new Dictionary<string, string>();
    

    protected override void OnPreInit(EventArgs e)
    {
        string  incomingEtag = Request.Headers["If-None-Match"], 
                incomingLwTimeStr = Request.Headers["If-Modified-Since"];
        DateTime? incomingLwTime = null;

        bool    isEtagMatched = false,
                isLwDateMatched = false,
                notModified = false;
        
        string phPath = System.Web.Hosting.HostingEnvironment.MapPath(AppRelativeVirtualPath);
        System.IO.FileInfo fi = new System.IO.FileInfo(phPath);
        DateTime lwTime = fi.LastWriteTimeUtc;
        

        string currentEtag = string.Concat(fi.Name, lwTime.Ticks.ToString());
        Encoder encoder = Encoding.UTF8.GetEncoder();
        byte[] buffer = new byte[encoder.GetByteCount(currentEtag.ToCharArray(), 0, currentEtag.Length, true)];
        encoder.GetBytes(currentEtag.ToCharArray(), 0, currentEtag.Length, buffer, 0, true);
        using (MD5 md5 = MD5CryptoServiceProvider.Create())
        {
            buffer = md5.ComputeHash(buffer);             
        }
        currentEtag = string.Concat("\"", BitConverter.ToString(buffer).Replace("-", string.Empty), "\"");
        
        isEtagMatched = incomingEtag != null && string.Equals(incomingEtag, currentEtag, StringComparison.Ordinal);

        if (incomingLwTimeStr != null) 
        {
            try
            {
                incomingLwTime = DateTime.ParseExact(incomingLwTimeStr, "r", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                isLwDateMatched = Math.Abs((incomingLwTime.Value.Ticks - lwTime.Ticks)) < TimeSpan.TicksPerSecond;   
            }
            catch (FormatException /*exc*/) { }
        }

        if (incomingEtag != null && incomingLwTimeStr != null)
            notModified = isEtagMatched && isLwDateMatched;
        else if (incomingEtag != null)
            notModified = isEtagMatched;
        else if (incomingLwTimeStr != null)
            notModified = isLwDateMatched;

        if (notModified)
        {
            Response.Clear();
            Response.StatusCode = (int)System.Net.HttpStatusCode.NotModified;
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetLastModified(lwTime);
            Response.Cache.SetETag(currentEtag);
            Response.SuppressContent = true;
            Response.AddHeader("Content-Length", "0");

            try
            {
                Response.End();
            }
            catch (System.Threading.ThreadAbortException /*exception*/) { }
        }
             
        
        CultureInfo ci = null;
        string cultureName = Request["cultureName"];

        if (string.IsNullOrEmpty(cultureName))
            ci = CultureInfo.InvariantCulture;
        else 
        {
            try
            {
                ci = CultureInfo.GetCultureInfo(cultureName);
            }
            catch (Exception /*exc*/) 
            {
                ci = CultureInfo.InvariantCulture;
            }
        }
        _cultureID = ci.LCID;
        _cultureName = ci.Name;
        DateTimeFormatInfo dtf = ci.DateTimeFormat;
        _dayNames = dtf.DayNames;
        _abbreviatedDayNames = dtf.AbbreviatedDayNames;
        _monthNames = dtf.MonthNames;
        _abbreviatedMonthNames = dtf.AbbreviatedMonthNames;
        _amDesignator = dtf.AMDesignator;
        _pmDesignator = dtf.PMDesignator;
        _calendarTwoDigitYearMax = dtf.Calendar.TwoDigitYearMax;
        _timeSeparator = dtf.TimeSeparator;
                
        _patternsDic.Add("shortDate", dtf.ShortDatePattern);
        _patternsDic.Add("longDate", dtf.LongDatePattern);
        _patternsDic.Add("shortTime", dtf.ShortTimePattern);
        _patternsDic.Add("longTime", dtf.LongTimePattern);
        _patternsDic.Add("fullDateTime", dtf.FullDateTimePattern);
        _patternsDic.Add("monthDay", dtf.MonthDayPattern);
        _patternsDic.Add("sortableDateTime", dtf.SortableDateTimePattern);
        _patternsDic.Add("yearMonth", dtf.YearMonthPattern);
        _patternsDic.Add("invariantSortableDateTime", "MM/dd/yyyy hh:mm:ss");

        Response.ContentType = "text/javascript";
        Response.Cache.VaryByParams["*"] = true;

        Response.Cache.SetExpires( DateTime.Now.AddHours(1));
        Response.Cache.SetMaxAge(new TimeSpan(0, 1, 0, 0));
        Response.Cache.SetCacheability(HttpCacheability.Public);
        Response.Cache.SetLastModified(lwTime);
        Response.Cache.SetValidUntilExpires(true);
        Response.Cache.AppendCacheExtension("pre-check=3600");
        Response.Cache.SetETag(currentEtag);          
        base.OnPreInit(e);
        
        
    }

    protected string GetCultureID()
    {
        return _cultureID.ToString();
    }
    
    protected string GetCultureName() 
    {
        return _cultureName;
    }

    //protected bool DisplayTime
    //{
    //    get { return displayTime; }
    //}

    private string Array2JSON(string[] source) 
    {
        int length = source != null ? source.Length : 0;
        if (length == 0)
            return "[]";
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < length; i++) 
        {
            if(i != 0)
                sb.Append(",");
            sb.Append("\"");
            sb.Append(JSEncode(source[i]));
            sb.Append("\"");
        }
        sb.Append("]");
        return sb.ToString();
    }
    protected string GetDayNamesJSON() 
    {
        return Array2JSON(_dayNames);
    }
    protected string GetAbbreviatedDayNamesJSON()
    {
        return Array2JSON(_abbreviatedDayNames);
    }
    protected string GetMonthNamesJSON()
    {
        return Array2JSON(_monthNames);
    }    
    protected string GetAbbreviatedMonthNamesJSON()
    {
        return Array2JSON(_abbreviatedMonthNames);
    }
    protected string GetAMDesignator() 
    {
        return _amDesignator;
    }
    protected string GetPMDesignator() 
    {
        return _pmDesignator;
    }
    protected string GetCalendarTwoDigitYearMax() 
    {
        return _calendarTwoDigitYearMax.ToString();
    }
    protected string GetTimeSeparator()
    {
        return _timeSeparator;
    }
    protected string GetPatternsJSON() 
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        bool isFirst = true;
        foreach (string key in _patternsDic.Keys)
        {
            if (!isFirst)
                sb.Append(",");
            else
                isFirst = false;
            sb.Append(JSEncode(key));
            sb.Append(":\"");
            sb.Append(JSEncode(_patternsDic[key]));
            sb.Append("\"");
        }
        sb.Append("}");
        return sb.ToString();    
    }                       
</script>

if(typeof(JCCalendar) == 'undefined') {
function JCCalendar()
{
	var _this = this;
	this.mess = {};
	this.floatDiv = null;
	this.content = null;
	this.dateInitial = new Date();
	this.dateCurrent = null;
	this.dateCreate = new Date();
	this.bTime = false;
	this.bFirst = true;
	this.menu = null;
	this.form = this.field = this.fieldFrom = this.fieldTo = null;
	this._format = null;
	this._fireInputChangeEvent = false;
	
	/* Applying styles */
	var head = document.getElementsByTagName("HEAD");
	if(head)
	{
		var link = document.createElement("LINK");
		link.rel = 'stylesheet';
		link.href = '<%= JSEncode(BXThemeHelper.AddAbsoluteThemePathAndVersion("calendar.css")) %>';
		head[0].appendChild(link);
	}

	this._tryGetElement = function(elem)
	{
		if(elem && typeof(elem) == "object" && "nodeType" in elem && elem.nodeType == 1)
			return elem;
		if((typeof(elem) == "string" || elem instanceof String) && elem.length > 0)
			return document.getElementById(elem);
		return null;
	}
	
	/* Main functions */
	this.Show = function(anchor, field, fieldFrom, fieldTo, bTime, serverTime, format,fireChangeEvent)
	{

		if(this.floatDiv)
			this.Close();
		var anchorEl = this._tryGetElement(anchor);

		this.form = top.jsUtils.FindParentObject(anchorEl, 'form');
		this.field = this._tryGetElement(field);
		this.fieldFrom = this._tryGetElement(fieldFrom);
		this.fieldTo = this._tryGetElement(fieldTo);

		if (fireChangeEvent && typeof(fireChangeEvent)=='boolean' ) this._fireInputChangeEvent = fireChangeEvent;
		this._format = format ? format : null;
		if(this._format)
			this.bTime = Bitrix.DateTimeUtility.isFormatContainsTime(this._format);
		else
			this.bTime = bTime;
			
		var difference = serverTime*1000 - (this.dateCreate.valueOf() - this.dateCreate.getTimezoneOffset()*60000);
		
		this.dateCurrent = this.field ? this.ParseDate(this.field.value, this._format) : null;
		if(this.dateCurrent)
		{
			this.dateInitial.setTime(this.dateCurrent.valueOf());
		}
		else if(this.bFirst)
		{
			this.dateInitial.setTime((new Date()).valueOf() + difference);
			this.dateInitial.setHours(0, 0, 0);
		}

		var div = document.body.appendChild(document.createElement("DIV"));
		div.id = "calendar_float_div";
		div.className = "bx-calendar-float";
		div.style.position = 'absolute';
		div.style.left = '-1000px';
		div.style.top = '-1000px';
		div.style.zIndex = 16000;

		this.hoursSpin = new JCSpinner('hours');
		this.minutesSpin = new JCSpinner('minutes');
		this.secondsSpin = new JCSpinner('seconds');

		div.innerHTML = 
			'<div class="bx-calendar-title">'+
			'<table cellspacing="0" width="100%">'+
			'	<tr>'+
			'		<td width="100%" class="bx-calendar-title-text" onmousedown="jsFloatDiv.StartDrag(arguments[0], document.getElementById(\'calendar_float_div\'));" id="calendar_float_title">'+this.mess["title"]+'</td><td width="0%"><a class="bx-calendar-close" onclick="jsCalendar.Close();" href="javascript:void(0);" title="'+this.mess["close"]+'"></a></td></tr>'+
			'</table>'+
			'</div>'+
			'<div class="bx-calendar-content"></div>'+
			'<div class="bx-calendar-time" align="center" style="display:'+(this.bTime? 'block':'none')+'">'+
			'<form name="float_calendar_time">'+
			'<table cellspacing="0">'+
			'	<tr>'+
			'		<td>'+this.mess["hour"]+'</td>'+
			'		<td><input type="text" name="hours" value="'+this.Number(this.dateInitial.getHours())+'" size="2" title="'+this.mess['hour_title']+'" onchange="jsCalendar.TimeChange(this);" onblur="jsCalendar.TimeChange(this);"></td>'+
			'		<td>'+this.hoursSpin.Show('jsCalendar.hoursSpin')+'</td>'+
			'		<td>&nbsp;'+this.mess["minute"]+'</td>'+
			'		<td><input type="text" name="minutes" value="'+this.Number(this.dateInitial.getMinutes())+'" size="2" title="'+this.mess['minute_title']+'" onchange="jsCalendar.TimeChange(this);" onblur="jsCalendar.TimeChange(this);"></td>'+
			'		<td>'+this.hoursSpin.Show('jsCalendar.minutesSpin')+'</td>'+
			'		<td>&nbsp;'+this.mess["second"]+'</td>'+
			'		<td><input type="text" name="seconds" value="'+this.Number(this.dateInitial.getSeconds())+'" size="2" title="'+this.mess['second_title']+'" onchange="jsCalendar.TimeChange(this);" onblur="jsCalendar.TimeChange(this);"></td>'+
			'		<td>'+this.hoursSpin.Show('jsCalendar.secondsSpin')+'</td>'+
			'		<td>&nbsp;</td>'+
			'		<td><a title="'+this.mess["set_time"]+'" href="javascript:void(0);" onclick="jsCalendar.CurrentTime('+difference+');" class="bx-calendar-time bx-calendar-set-time"></a></td>'+
			'		<td><a title="'+this.mess["clear_time"]+'" href="javascript:void(0);" onclick="jsCalendar.ClearTime();" class="bx-calendar-time bx-calendar-clear-time"></a></td>'+
			'	</tr>'+
			'</table>'+
			'</form>'+
			'</div>'+
			'<table cellspacing="0" class="bx-calendar-timebar">'+
			'	<tr>'+
			'		<td align="center"><a id="calendar_time_button" hidefocus="true" tabindex="-1" title="'+(this.bTime? this.mess["time_hide"]:this.mess["time"])+'" href="javascript:void(0);" onclick="jsCalendar.ToggleTime();" class="bx-calendar-button '+(this.bTime? 'bx-calendar-arrow-up':'bx-calendar-arrow-down')+'"></a></td>'+
			'	</tr>'+
			'</table>';
		this.floatDiv = div;
		this.content = top.jsUtils.FindChildObject(this.floatDiv, 'div', 'bx-calendar-content');
		this.content.innerHTML = this.GetMonthPage();

		var		pos = jsUtils.AlignToPos(Bitrix.ElementPositioningUtility.getElementRect(anchorEl), 275, 255);

		//var pos = top.jsUtils.GetRealPos(anchorEl);
		//pos["bottom"]+=2;
		//pos = top.jsUtils.AlignToPos(pos, div.offsetWidth, div.offsetHeight);

		jsFloatDiv.Show(div, pos["left"], pos["top"], 'nan', this.shadowPath);
		
		setTimeout(function(){top.jsUtils.addEvent(document, "click", _this.CheckClick)}, 10);
		top.jsUtils.addEvent(document, "keypress", _this.OnKeyPress);
		
		this.bFirst = false;
	}
	this.GetMonthPage = function()
	{
		var dtf = Bitrix.DateTimeUtility.getCurrentCultureDateTimeFormat();
		var aMonths = dtf.getMonthNames();
		//var aMonths = [this.mess["jan"], this.mess["feb"], this.mess["mar"], this.mess["apr"], this.mess["may"], this.mess["jun"], this.mess["jul"], this.mess["aug"], this.mess["sep"], this.mess["okt"], this.mess["nov"], this.mess["des"]];
		var daysArr = dtf.getAbbreviatedDayNames();
		var initYear = this.dateInitial.getFullYear(), initMonth = this.dateInitial.getMonth(), initDay = this.dateInitial.getDate();
		var today = new Date();
		today.setHours(this.dateInitial.getHours(), this.dateInitial.getMinutes(), this.dateInitial.getSeconds());
		var bCurMonth = (today.getFullYear() == initYear && today.getMonth() == initMonth);

		document.getElementById('calendar_float_title').innerHTML = aMonths[initMonth]+', '+initYear;

		var s = '';
		s += 
			'<div style="width:100%;">'+
			'<table cellspacing="0" class="bx-calendar-toolbar">'+
			'<tr>'+
				'<td><a title="'+this.mess["prev_mon"]+'" href="javascript:void(0);" onclick="jsCalendar.NavigateMonth('+(initMonth-1)+');" class="bx-calendar-button bx-calendar-left"></a></td>'+
				'<td width="50%"></td>'+
				'<td><a title="'+(bCurMonth? this.mess["curr_day"]:this.mess["curr"])+'" href="javascript:void(0);" onclick="'+(bCurMonth? 'jsCalendar.InsertDate(\''+today.valueOf()+'\')':'jsCalendar.NavigateToday()')+';" class="bx-calendar-button bx-calendar-today"></a></td>'+
				'<td><a title="'+this.mess["per_mon"]+'" href="javascript:void(0);" onclick="jsCalendar.InsertPeriod(\''+this.getMonthFirst().valueOf()+'\', \''+this.getMonthLast().valueOf()+'\');" class="bx-calendar-button bx-calendar-menu">'+aMonths[initMonth]+'</a></td>'+
				'<td><a title="'+this.mess["month"]+'" href="javascript:void(0)" onclick="jsCalendar.MenuMonth(this);" class="bx-calendar-button bx-calendar-arrow"></a></td>'+
				'<td><a title="'+this.mess["per_year"]+'" href="javascript:void(0);" onclick="jsCalendar.InsertPeriod(\''+this.getYearFirst().valueOf()+'\', \''+this.getYearLast().valueOf()+'\');" class="bx-calendar-button bx-calendar-menu">'+initYear+'</a></td>'+
				'<td><a title="'+this.mess["year"]+'" href="javascript:void(0)" onclick="jsCalendar.MenuYear(this);" class="bx-calendar-button bx-calendar-arrow"></a></td>'+
				'<td width="50%"></td>'+
				'<td><a title="'+this.mess["next_mon"]+'" href="javascript:void(0);" onclick="jsCalendar.NavigateMonth('+(initMonth+1)+');" class="bx-calendar-button bx-calendar-right"></a></td>'+
			'</tr>'+
			'</table>';
		s += 
			'<div class="bx-calendar">'+
			'<div style="width:100%;">'+
			'<table cellspacing="0">'+
			'<tr class="bx-calendar-head">'+
			'<td class="bx-calendar-week">&nbsp;</td>'+
			'<td>'+ daysArr[1] +'</td>'+
			'<td>'+ daysArr[2] +'</td>'+
			'<td>'+ daysArr[3] +'</td>'+
			'<td>'+ daysArr[4] +'</td>'+
			'<td>'+ daysArr[5] +'</td>'+
			'<td>'+ daysArr[6] +'</td>'+
			'<td>'+ daysArr[0] +'</td>'+			
			<%--
			//'<td>'+this.mess["mo"]+'</td>'+
			//'<td>'+this.mess["tu"]+'</td>'+
			//'<td>'+this.mess["we"]+'</td>'+
			//'<td>'+this.mess["th"]+'</td>'+
			//'<td>'+this.mess["fr"]+'</td>'+
			//'<td>'+this.mess["sa"]+'</td>'+
			//'<td>'+this.mess["su"]+'</td>'+
			--%>
			'</tr>';

		var firstDate = new Date(initYear, initMonth, 1, this.dateInitial.getHours(), this.dateInitial.getMinutes(), this.dateInitial.getSeconds());
		var firstDay = firstDate.getDay()-1;
		if(firstDay == -1)
			firstDay = 6;
	
		var date = new Date();
		var bBreak = false;
		for(var i=0; i<6; i++)
		{
			var row = i*7;
			date.setTime(firstDate.valueOf());
			date.setDate(1-firstDay+row);
			if(i > 0 && date.getDate() == 1)
				break;

			var nWeek = this.WeekNumber(date);
			s += '<tr><td class="bx-calendar-week"><a title="'+this.mess["per_week"]+'" href="javascript:void(0);" onclick="jsCalendar.InsertPeriod(\''+date.valueOf()+'\', \'';

			date.setTime(firstDate.valueOf());
			date.setDate(1-firstDay+row+6);
			s += date.valueOf()+'\');">'+nWeek+'</a></td>';
			
			for(var j=0; j<7; j++)
			{
				date.setTime(firstDate.valueOf());
				date.setDate(1-firstDay+row+j);
				var d = date.getDate();
	
				if(i > 0 && d == 1)
					bBreak = true;
	
				var sClass = '';
				if(row+j+1 > firstDay && !bBreak)
				{
					if(d == today.getDate() && bCurMonth)
						sClass += ' bx-calendar-today';
					if(this.dateCurrent && d == this.dateCurrent.getDate() && initMonth == this.dateCurrent.getMonth() && initYear == this.dateCurrent.getFullYear())
						sClass += ' bx-calendar-current';
				}
				if(j==5 || j==6)
					sClass += ' bx-calendar-holiday';
				if(!(row+j+1 > firstDay && !bBreak))
					sClass += ' bx-calendar-inactive';

				s += '<td'+(sClass != ''? ' class="'+sClass+'"':'')+'>';
				s += '<div class="bx-calendar-date-container" onmouseover="this.className=\'bx-calendar-date-container-hovered\';" onmouseout="this.className=\'bx-calendar-date-container\';">';
				s += '<a title="'+this.mess["date"]+'" href="javascript:void(0);" onclick="jsCalendar.InsertDate(\''+date.valueOf()+'\')">'+d+'</a>';
				s += '</div>';
				s += '</td>';
			}
			s += '</tr>';
			if(bBreak)
				break;
		}
		s += 
			'</table>'+
			'</div>'+
			'</div>'+
			'</div>';
		return s;			
	}

	/* Dates arithmetics */
	this.WeekNumber = function(date)
	{
		var firstYearDate = new Date(date.getFullYear(), 0, 1);
		var firstYearDay = firstYearDate.getDay()-1;
		if(firstYearDay == -1)
			firstYearDay = 6;

		var correcredDate = new Date(date.valueOf());
		correcredDate.setHours(0, 0, 0);
		
		var nDays = Math.round((correcredDate.valueOf()-firstYearDate.valueOf())/(24*60*60*1000));
		var nWeek = Math.round((nDays-(7-firstYearDay))/7+1);
		if(firstYearDay < 4)
			nWeek++;
			
		if(nWeek > 52)
		{
			firstYearDate = new Date(correcredDate.getFullYear()+1, 0, 1);
			firstYearDay = firstYearDate.getDay()-1;
			if(firstYearDay == -1)
				firstYearDay = 6;
			if(firstYearDay < 4)
				nWeek = 1;
		}
		return nWeek;
	}
	
	this.NavigateToday = function()
	{
		var h = this.dateInitial.getHours(), m = this.dateInitial.getMinutes(), s = this.dateInitial.getSeconds();
		this.dateInitial.setTime((new Date()).valueOf());
		this.dateInitial.setHours(h, m, s);
		this.content.innerHTML = jsCalendar.GetMonthPage();
	}

	this.NavigateMonth = function(mon)
	{
		this.dateInitial.setMonth(mon);
		this.content.innerHTML = jsCalendar.GetMonthPage();
	}

	this.NavigateYear = function(year)
	{
		this.dateInitial.setFullYear(year);
		this.content.innerHTML = jsCalendar.GetMonthPage();
	}
	
	this.getMonthFirst = function()
	{
		var d = new Date();
		d.setTime(this.dateInitial.valueOf());
		d.setDate(1);
		return d;
	}

	this.getMonthLast = function()
	{
		var d = new Date();
		d.setTime(this.dateInitial.valueOf());
		d.setMonth(d.getMonth()+1);
		d.setDate(0);
		return d;
	}

	this.getYearFirst = function()
	{
		var d = new Date();
		d.setTime(this.dateInitial.valueOf());
		d.setMonth(0);
		d.setDate(1);
		return d;
	}

	this.getYearLast = function()
	{
		var d = new Date();
		d.setTime(this.dateInitial.valueOf());
		d.setFullYear(d.getFullYear()+1);
		d.setMonth(0);
		d.setDate(0);
		return d;
	}

	/* Input / Output */
	this.InsertDaysBack = function(input, days)
	{
		if(days != '')
		{
			var d = new Date();
			if(days > 0)
				d.setTime(d.valueOf() - days*24*60*60*1000);
			input.value = this.FormatDate(d, top.dotNetVars.exFormatDateTime);
			input.disabled = true;
		}
		else
		{
			input.disabled = false;
			input.value = '';
		}
	}

	this.ValueToString = function(value)
	{
		var date = new Date();
		date.setTime(value);
		if(this.bTime)
		{
			var form = document.float_calendar_time;
			date.setHours(parseInt(form.hours.value, 10));
			date.setMinutes(parseInt(form.minutes.value, 10));
			date.setSeconds(parseInt(form.seconds.value, 10));
		}
		return this.FormatDate(date, this._format);
	}

	this.CurrentTime = function(difference)
	{
		var time = new Date();
		time.setTime(time.valueOf() + difference);

		var form = document.float_calendar_time;
		form.hours.value = time.getHours();
		form.minutes.value = time.getMinutes();
		form.seconds.value = time.getSeconds();

		form.hours.onchange();
		form.minutes.onchange();
		form.seconds.onchange();
	}

	this.ClearTime = function()
	{
		var form = document.float_calendar_time;
		form.hours.value = form.minutes.value = form.seconds.value = '00';
	}

	this.InsertDate = function(value)
	{
		if(this.field) this.field.value = this.ValueToString(value);
		this.Close();
		if ( this._fireInputChangeEvent && this.field && typeof(this.field.onchange)=='function')
		    this.field.onchange(); 
	}

	this.InsertPeriod = function(value1, value2)
	{
		if(this.fieldFrom != null &&  this.fieldFrom != '' && this.fieldTo != null && this.fieldTo != '')
		{
			if(this.fieldFrom) this.fieldFrom.value = this.ValueToString(value1);
			if(this.fieldTo) this.fieldTo.value = this.ValueToString(value2);
		}
		else
		{
			if(this.field) this.field.value = this.ValueToString(value1);
		}
		this.Close();
	}

	this.Number = function(val)
	{
		return (val < 10? '0'+val : val);
	}

	this.FormatDate = function(date, format)
	{
		return Bitrix.DateTimeUtility.formatLocale(date, format ? format : this.bTime ? dotNetVars.exFormatDateTime : dotNetVars.exFormatDate);
	}
	
	this.ParseDate = function(str, format)
	{
		return format ? Bitrix.DateTimeUtility.parseLocaleExact(str, format) : Bitrix.DateTimeUtility.parseLocale(str);
	}

	/* Navigation interface */
	this.MenuMonth = function(a)
	{
		//var aMonths = [this.mess["jan"], this.mess["feb"], this.mess["mar"], this.mess["apr"], this.mess["may"], this.mess["jun"], this.mess["jul"], this.mess["aug"], this.mess["sep"], this.mess["okt"], this.mess["nov"], this.mess["des"]];
		var dtf = Bitrix.DateTimeUtility.getCurrentCultureDateTimeFormat();
		var aMonths = dtf.getMonthNames();
		var items = [];
		var mon = this.dateInitial.getMonth();
		for(var i = 0; i < 12; i++){
			items[i] = {'ICONCLASS': (mon == i? 'checked':''), 'TEXT': aMonths[i], 'ONCLICK': 'jsCalendar.NavigateMonth('+i+')', 'DEFAULT': ((new Date()).getMonth() == i? true:false)};
		}
		this.ShowMenu(a, items);
	}

	this.MenuYear = function(a)
	{
		var items = [];
		var y = this.dateInitial.getFullYear();
		for(var i=0; i<11; i++)
		{
			item_year = y-5+i;
			items[i] = {'ICONCLASS': (y == item_year? 'checked':''), 'TEXT': item_year, 'ONCLICK': 'jsCalendar.NavigateYear('+item_year+')', 'DEFAULT': ((new Date()).getFullYear() == item_year? true:false)};
		}
		this.ShowMenu(a, items);
	}

	this.ShowMenu = function(a, items)
	{
		if(!this.menu)
		{
			this.menu = new PopupMenu('calendar_float_menu');
			this.menu.Create(parseInt(this.floatDiv.style.zIndex)+10, 3);
			this.menu.OnClose = function()
			{
				setTimeout(
					function(){
						if(_this.floatDiv) 
							top.jsUtils.addEvent(document, "click", _this.CheckClick);
					}, 10);
				top.jsUtils.addEvent(document, "keypress", _this.OnKeyPress);
			}
		}
		if(this.menu.IsVisible())
			return;

        this.menu.SetItems(items);
		this.menu.BuildItems();
		var pos = top.jsUtils.GetRealPos(a);
		pos["bottom"]+=1;

		top.jsUtils.removeEvent(document, "click", _this.CheckClick);
		top.jsUtils.removeEvent(document, "keypress", _this.OnKeyPress);
		this.menu.PopupShow(pos);
	}

	this.ToggleTime = function()
	{
		var div = top.jsUtils.FindChildObject(this.floatDiv, 'div', 'bx-calendar-time');
		var a = document.getElementById('calendar_time_button');
		if(div.style.display == 'none')
		{
			div.style.display = 'block';
			if(!a.className || a.length == 0)
				a.className = "bx-calendar-button bx-calendar-arrow-up";
			else
				{
					var className = a.className.replace(/\s*bx-calendar-arrow-down\s*/, "");
					if(className.length > 0)
					{
						if(className.charAt(className.length - 1) != ' ')
							className += " ";
						className += "bx-calendar-arrow-up";
					}
					else
						className = "bx-calendar-button bx-calendar-arrow-up";
					a.className = className;
				}
			a.title = this.mess['time_hide'];
		}
		else
		{
			div.style.display = 'none';
			if(!a.className || a.length == 0)
				a.className = "bx-calendar-button bx-calendar-arrow-down";
			else
				{
					var className = a.className.replace(/\s*bx-calendar-arrow-up\s*/, "");
					if(className.length > 0)
					{
						if(className.charAt(className.length - 1) != ' ')
							className += " ";
						className += "bx-calendar-arrow-down";
					}
					else
						className = "bx-calendar-button bx-calendar-arrow-down";
					a.className = className;
				}
			a.title = this.mess['time'];
		}
		a.blur();
		jsFloatDiv.AdjustShadow(this.floatDiv);
	}
	
	this.TimeChange = function(input)
	{
		this.bTime = true;

		var val = parseInt(input.value, 10);
		if(isNaN(val))
			val = '00';
		else if(val < 0)
		{
			if(input.name == 'hours')
				val = '23';
			else
				val = '59';
		}
		else if(input.name == 'hours' && val > 23 || val > 59)
			val = '00';
		else
			val = this.Number(val);
		
		input.value = val;
	}
	
	/* Window operations: close, drag, move */
	this.Close =  function()
	{
		top.jsUtils.removeEvent(document, "click", _this.CheckClick);
		top.jsUtils.removeEvent(document, "keypress", _this.OnKeyPress);

		jsFloatDiv.Close(this.floatDiv);

		this.floatDiv.parentNode.removeChild(this.floatDiv);
		this.floatDiv = null;
	}

	this.OnKeyPress = function(e)
	{
		if(!e) e = window.event
		if(!e) return;
		if(e.keyCode == 27)
			_this.Close();
	}
	
	this.CheckClick = function(e)
	{
		var div = _this.floatDiv;
		if(!div)
			return;

		var windowSize = jsUtils.GetWindowSize();
		var x = e.clientX + windowSize.scrollLeft;
		var y = e.clientY + windowSize.scrollTop;

		var arPos = jsUtils.GetRealPos(div);
		/*region*/
		if(x >= arPos.left && x <= arPos.right && y >= arPos.top && y <= arPos.bottom)
			return;

		//var x = e.clientX + document.body.scrollLeft;
		//var y = e.clientY + document.body.scrollTop;

		/*region*/
		//var posLeft = parseInt(div.style.left);
		//var posTop = parseInt(div.style.top);
		//var posRight = posLeft + div.offsetWidth;
		//var posBottom = posTop + div.offsetHeight;
		//if(x >= posLeft && x <= posRight && y >= posTop && y <= posBottom)
		//	return;

		_this.Close();
	}
}
}

var jsCalendar = new JCCalendar();
jsCalendar.shadowPath = '<%= Bitrix.Services.Js.BXJSUtility.Encode(Bitrix.UI.BXThemeHelper.SimpleCombineWithCurrentThemePath("images/shadow.png")) %>';

jsCalendar.mess = {
	'css_ver': '1156336678',
	'title': '<%= GetMessage("Title") %>',
	'date': '<%= GetMessage("Date") %>',
	'prev_mon': '<%= GetMessage("PrevMon") %>',
	'next_mon': '<%= GetMessage("NextMon") %>',
	'curr': '<%= GetMessage("Curr") %>',
	'curr_day': '<%= GetMessage("CurrDay") %>',
	'per_week': '<%= GetMessage("PerWeek") %>',
	'per_mon': '<%= GetMessage("PerMon") %>',
	'per_year': '<%= GetMessage("Year") %>',
	'close': '<%= GetMessage("Close") %>',
	'month': '<%= GetMessage("Month") %>',
	'year': '<%= GetMessage("Year") %>',
	'time': '<%= GetMessage("Time") %>',
	'time_hide': '<%= GetMessage("TimeHide") %>',
	'hour': '<%= GetMessage("Hour") %>',
	'minute': '<%= GetMessage("Minute") %>',
	'second': '<%= GetMessage("Second") %>',
	'hour_title': '<%= GetMessage("HourTitle") %>',
	'minute_title': '<%= GetMessage("MinuteTitle") %>',
	'second_title': '<%= GetMessage("SecondTitle") %>',
	'set_time': '<%= GetMessage("SetTime") %>',
	'clear_time': '<%= GetMessage("ClearTime") %>'
};

if(typeof(JCSpinner) == 'undefined') {
function JCSpinner(name)
{
	var _this = this;
	this.name = name;
	this.mousedown = false;
	
	this.Show = function(name)
	{
		var s = 
			'<table cellspacing="0" class="spin">'+
			'	<tr><td><a hidefocus="true" tabindex="-1" href="javascript:void(0);" onmousedown="'+name+'.Start(1);" class="bx-calendar-spin bx-calendar-spin-up"></a></td></tr>'+
			'	<tr><td><a hidefocus="true" tabindex="-1" href="javascript:void(0);" onmousedown="'+name+'.Start(-1);" class="bx-calendar-spin bx-calendar-spin-down"></a></td></tr>'+
			'</table>';
		return s;
	}
	
	this.Start = function(delta)
	{
		this.mousedown = true;
		top.jsUtils.addEvent(document, "mouseup", _this.MouseUp);
		this.ChangeValue(delta, true);
	}
	
	this.ChangeValue = function(delta, bFirst)
	{
		if(!this.mousedown)
			return;

		var input = document.float_calendar_time.elements[this.name];
		input.value = parseInt(input.value, 10) + delta;
		input.onchange();
		setTimeout(function(){_this.ChangeValue(delta, false)}, (bFirst? 1000:150));
	}
	
	this.MouseUp = function()
	{
		_this.mousedown = false;
		top.jsUtils.removeEvent(document, "mouseup", _this.MouseUp);
	}
}
}
if(typeof(Bitrix) == 'undefined'){
	var Bitrix = new Object();
}

if(typeof(Bitrix.DateTimeUtility) == 'undefined') {
Bitrix.DateTimeUtility = function Bitrix$DateTimeUtility(){
	this._initialized = false;
	this._timePartCheckRegex = new RegExp("h+|H+|m+|s+|F+|f+");
}

Bitrix.DateTimeUtility.prototype = {
	initialize: function(){
		this._initialized = true;
	},
	isFormatContainsTime: function(format){
		if(!format) throw "format is not valid!";
		var expFormat = this._expandFormat(format, this._getCurrentCultureDateTimeFormat());
		var result = this._timePartCheckRegex.test(expFormat);
		return result;
	},
	_getStandardFormatArray: function(){
		return ["s", "F", "D", "T", "d", "t", "M", "m", "Y", "y","U","u"];
	},
	_expandFormat: function(format, dtf){
	    if (!format) format = "F";
	    if (format.length === 1){
	        switch (format){
	        case "d":
	            return dtf.getShortDatePattern();
	        case "D":
	            return dtf.getLongDatePattern();
	        case "t":
	            return dtf.getShortTimePattern();
	        case "T":
	            return dtf.getLongTimePattern();
	        case "F":
	            return dtf.getFullDateTimePattern();
	        case "M": case "m":
	            return dtf.getMonthDayPattern();
	        case "s":
	            return dtf.getSortableDateTimePattern();
	        case "Y": case "y":
	            return dtf.getYearMonthPattern();
	        case "U": case "u":
	                return dtf.getUniversalSortableDateTimePattern();
	        default:
	            throw "Unknown format string '" + format + "'!";
	        }
	    }
	    return format;	
	},
	_expandYear: function(value, dtf){
        if (year < 100){
	        var curr = new Date().getFullYear();
	        year += curr - (curr % 100);
	        if (year > dtf.getCalendarTwoDigitYearMax)
	            year -= 100;
	    }
	    return year;	
	},	
	_getTokenRegExp: function(){
        return /dddd|ddd|dd|d|MMMM|MMM|MM|M|yyyy|yy|y|hh|h|HH|H|mm|m|ss|s|tt|t|fff|ff|f|zzz|zz|z/g;
	},
	_parseInt: function(value){
		return parseInt(value.replace(/^[\s0]+(\d+)$/,"$1"));	
	},
	_trimString: function(value){
		return value.replace(/^\s*(.*?)\s*$/, "$1");
	},
    _toUpperCaseString: function(value){
		return value.split("\u00A0").join(' ').toUpperCase();
    },	
	_findStringInArray: function(arr, str, ignoreCase){
		if(!str || !(arr && arr instanceof Array))
			return -1;
			
		var length = arr.length;
		var strUpper = null;
		for(var i = 0; i < length; i++){
			if(ignoreCase === true){
				if(!strUpper) strUpper = this._toUpperCaseString(str);
				if(strUpper == this._toUpperCaseString(arr[i])) return i;
			}
			else if(str == arr[i])
				return i;
		}
		return -1;
	},
	_getMonthIndex: function(value, dtf){
		var monthArr = dtf.getMonthNames();
		return this._findStringInArray(monthArr, value, true);
	},
	_getAbbrMonthIndex: function(value, dtf){
		var monthArr = dtf.getAbbreviatedMonthNames();
		return this._findStringInArray(monthArr, value, true);	
	},
	_getDayIndex: function(value, dtf){
		var dayArr = dtf.getDayNames();
		return this._findStringInArray(dayArr, value, true);		
	},
	_getAbbrDayIndex: function(value, dtf){
		var dayArr = dtf.getAbbreviatedDayNames();
		return this._findStringInArray(dayArr, value, true);	
	},
	_getParseRegExp: function(format){
		var expFormat = format;
		expFormat = expFormat.replace(/([\^\$\.\*\+\?\|\[\]\(\)\{\}])/g, "\\\\$1");
		var regexpStr = "^";
	    var groups = new Array();
	    var index = 0;
	    var quoteCount = 0;
	    var tokenRegExp = this._getTokenRegExp();
	    var match;
		function appendPreOrPostMatch(preMatch){
			var quoteCount = 0;
		    var escaped = false;
		    for (var i = 0, il = preMatch.length; i < il; i++) {
		        var c = preMatch.charAt(i);
		        switch (c) {
		        case '\'':
		            if (escaped) regexpStr = regexpStr.concat("'");
		            else quoteCount++;
		            escaped = false;
		            break;
		        case '\\':
		            if (escaped) regexpStr = regexpStr.concat("\\");
		            escaped = !escaped;
		            break;
		        default:
		            regexpStr = regexpStr.concat(c);
		            escaped = false;
		            break;
		        }
		    }
		    return quoteCount;	
		}
		
        while ((match = tokenRegExp.exec(expFormat)) !== null) {
			var preMatch = expFormat.slice(index, match.index);
			index = tokenRegExp.lastIndex;
			quoteCount += appendPreOrPostMatch(preMatch);
			if ((quoteCount%2) === 1) {
				regexpStr = regexpStr.concat(match[0]);
				continue;
			}
			switch (match[0]) {
	            case 'dddd': case 'ddd':
	            case 'MMMM': case 'MMM':
	                regexpStr = regexpStr.concat("(\\D+)");
	                break;
	            case 'tt': case 't':
	                regexpStr = regexpStr.concat("(\\D*)");
	                break;
	            case 'yyyy':
	                regexpStr = regexpStr.concat("(\\d{4})");
	                break;
	            case 'fff':
	                regexpStr = regexpStr.concat("(\\d{3})");
	                break;
	            case 'ff':
	                regexpStr = regexpStr.concat("(\\d{2})");
	                break;
	            case 'f':
	                regexpStr = regexpStr.concat("(\\d)");
	                break;
	            case 'dd': case 'd':
	            case 'MM': case 'M':
	            case 'yy': case 'y':
	            case 'HH': case 'H':
	            case 'hh': case 'h':
	            case 'mm': case 'm':
	            case 'ss': case 's':
	                regexpStr = regexpStr.concat("(\\d\\d?)");
	                break;
	            case 'zzz':
	                regexpStr = regexpStr.concat("([+-]?\\d\\d?:\\d{2})");
	                break;
	            case 'zz': case 'z':
	                regexpStr = regexpStr.concat("([+-]?\\d\\d?)");
	                break;
			}
			groups.push(match[0]);
		}
	    appendPreOrPostMatch(expFormat.slice(index));
	    regexpStr = regexpStr.concat("$");
	    regexpStr = regexpStr.replace(/\s+/g, "\\s+");
	    var parseRegExp = {'regExp': regexpStr, 'groups': groups};
	    return parseRegExp;	
	},
	_getCurrentCultureID: function(){
		return "dotNetVars" in window && "currentCultureInfoID" in window["dotNetVars"] ? window["dotNetVars"]["currentCultureInfoID"] : null;
	},
	_getCurrentCultureDateTimeFormat: function(){
		var currentCultureID = this._getCurrentCultureID();
		if(!currentCultureID) throw "Could not find current culture ID!";
		var result = Bitrix.DateTimeFormatInfo.getEntry(currentCultureID);
		if(!result) throw "Could not find DateTimeFormatInfo for '" + currentCultureID + "'!";
		return result;
	},
	parseLocaleExact: function(value, format){
        value = this._trimString(value);
		var dtf = this._getCurrentCultureDateTimeFormat();
		var expFormat = this._expandFormat(format, dtf);
		var parseInfo = dtf.getParseRegExp(expFormat);
		if(!parseInfo){
			parseInfo = this._getParseRegExp(expFormat);
			if(!parseInfo) throw "Could not create parse info for '" + expFormat + "'!";
			dtf.setParseRegExp(expFormat, parseInfo);
		}
		var match = new RegExp(parseInfo.regExp).exec(value);
        if (match !== null){
			var groups = parseInfo.groups;
			var year = null, month = null, date = null, weekDay = null;
			var hour = 0, min = 0, sec = 0, msec = 0, tzMinOffset = null;
			var pmHour = false;
            for (var j = 0, jl = groups.length; j < jl; j++){
	            var matchGroup = match[j+1];
	            if (matchGroup) {
	                switch (groups[j]) {
	                    case 'dd': case 'd':
	                        date = this._parseInt(matchGroup);
	                        if ((date < 1) || (date > 31)) return null;
	                        break;
	                    case 'MMMM':
	                        month = this._getMonthIndex(matchGroup, dtf);
	                        if ((month < 0) || (month > 11)) return null;
	                        break;
	                    case 'MMM':
	                        month = this._getAbbrMonthIndex(matchGroup, dft);
	                        if ((month < 0) || (month > 11)) return null;
	                        break;
	                    case 'M': case 'MM':
	                        var month = this._parseInt(matchGroup) - 1;
	                        if ((month < 0) || (month > 11)) return null;
	                        break;
	                    case 'y': case 'yy':
	                        year = this._expandYear(dtf, this._parseInt(matchGroup));
	                        if ((year < 0) || (year > 9999)) return null;
	                        break;
	                    case 'yyyy':
	                        year = this._parseInt(matchGroup);
	                        if ((year < 0) || (year > 9999)) return null;
	                        break;
	                    case 'h': case 'hh':
	                        hour = this._parseInt(matchGroup);
	                        if (hour === 12) hour = 0;
	                        if ((hour < 0) || (hour > 11)) return null;
	                        break;
	                    case 'H': case 'HH':
	                        hour = this._parseInt(matchGroup);
	                        if ((hour < 0) || (hour > 23)) return null;
	                        break;
	                    case 'm': case 'mm':
	                        min = this._parseInt(matchGroup);
	                        if ((min < 0) || (min > 59)) return null;
	                        break;
	                    case 's': case 'ss':
	                        sec = this._parseInt(matchGroup);
	                        if ((sec < 0) || (sec > 59)) return null;
	                        break;
	                    case 'tt': case 't':
	                        var upperToken = matchGroup.toUpperCase();
	                        pmHour = (upperToken === dtf.getPMDesignator().toUpperCase());
	                        if (!pmHour && (upperToken !== dtf.getAMDesignator().toUpperCase())) return null;
	                        break;
	                    case 'f':
	                        msec = this._parseInt(matchGroup) * 100;
	                        if ((msec < 0) || (msec > 999)) return null;
	                        break;
	                    case 'ff':
	                        msec = this._parseInt(matchGroup) * 10;
	                        if ((msec < 0) || (msec > 999)) return null;
	                        break;
	                    case 'fff':
	                        msec = this._parseInt(matchGroup);
	                        if ((msec < 0) || (msec > 999)) return null;
	                        break;
	                    case 'dddd':
	                        weekDay = this._getDayIndex(matchGroup, dtf);
	                        if ((weekDay < 0) || (weekDay > 6)) return null;
	                        break;
	                    case 'ddd':
	                        weekDay = this._getAbbrDayIndex(matchGroup, dtf);
	                        if ((weekDay < 0) || (weekDay > 6)) return null;
	                        break;
	                    case 'zzz':
	                        var offsets = matchGroup.split(/:/);
	                        if (offsets.length !== 2) return null;
	                        var hourOffset = this._parseInt(offsets[0]);
	                        if ((hourOffset < -12) || (hourOffset > 13)) return null;
	                        var minOffset = this._parseInt(offsets[1]);
	                        if ((minOffset < 0) || (minOffset > 59)) return null;
	                        tzMinOffset = (hourOffset * 60) + (matchGroup.startsWith('-')? - minOffset : minOffset);
	                        break;
	                    case 'z': case 'zz':
	                        var hourOffset = this._parseInt(matchGroup);
	                        if ((hourOffset < -12) || (hourOffset > 13)) return null;
	                        tzMinOffset = hourOffset * 60;
	                        break;
					}
				}
			}
			var result = new Date();
	        if (year === null) {
	            year = result.getFullYear();
	        }
	        if (month === null) {
	            month = result.getMonth();
	        }
	        if (date === null) {
	            date = result.getDate();
	        }
	        result.setFullYear(year, month, date);
	        if (result.getDate() !== date) return null;
	        if ((weekDay !== null) && (result.getDay() !== weekDay)) {
	            return null;
	        }
	        if (pmHour && (hour < 12)) {
	            hour += 12;
	        }
	        result.setHours(hour, min, sec, msec);
	        if (tzMinOffset !== null) {
				var adjustedMin = result.getMinutes() - (tzMinOffset + result.getTimezoneOffset());
	            result.setHours(result.getHours() + parseInt(adjustedMin/60), adjustedMin%60);
	        }
	        return result;
		}		
	},
	parseLocale: function(value){
		var result = null;
		if("dotNetVars" in window){
			var dotNetVars = window["dotNetVars"];
			if("exFormatDateTime" in dotNetVars)
				result = this.parseLocaleExact(value, dotNetVars["exFormatDateTime"]);
			if(!result && "exFormatDate" in dotNetVars)
				result = this.parseLocaleExact(value, dotNetVars["exFormatDate"]);
		}
		if(!result){
			var stdPatterns = this._getStandardFormatArray();
			for(var i = 0; i < stdPatterns.length; i++){
				if((result = this.parseLocaleExact(value, stdPatterns[i])))
					break;
			}
		}
		if(!result){
			var dtf = this._getCurrentCultureDateTimeFormat();
			var patterns = dtf.getPatternsArray();
			
			for(var j = 0; j < patterns.length; j++){
				if((result = this.parseLocaleExact(value, patterns[j])))
					break;
			}
		}
		return result;
	},
	formatLocale: function(value, format){
		if(!value || !(value instanceof Date))
			throw "value is not valid!";
		if(!(format && (typeof(format) == 'string' || format instanceof String)))
			format = "F";

	    var dtf = this._getCurrentCultureDateTimeFormat();
	    format = this._expandFormat(format, dtf);

		var result = "";
	    function addLeadingZero(num){
	        if (num < 10) {
	            return '0' + num;
	        }
	        return num.toString();
	    }

	    function addLeadingZeros(num){
	        if (num < 10){
	            return '00' + num;
	        }
	        if (num < 100){
	            return '0' + num;
	        }
	        return num.toString();
	    }

		function appendPreOrPostMatch(preMatch){
			var quoteCount = 0;
		    var escaped = false;
		    for (var i = 0, il = preMatch.length; i < il; i++) {
		        var c = preMatch.charAt(i);
		        switch (c) {
		        case '\'':
		            if (escaped) result = result.concat("'");
		            else quoteCount++;
		            escaped = false;
		            break;
		        case '\\':
		            if (escaped) result = result.concat("\\");
		            escaped = !escaped;
		            break;
		        default:
		            result = result.concat(c);
		            escaped = false;
		            break;
		        }
			}
			return quoteCount;	
		}		
		
	    var hour = 0;
	    var quoteCount = 0;
	    var tokenRegExp = this._getTokenRegExp();
	    for (;;){
			var index = tokenRegExp.lastIndex;
			var ar = tokenRegExp.exec(format);
			var preMatch = format.slice(index, ar ? ar.index : format.length);
	        quoteCount += appendPreOrPostMatch(preMatch);
	        if (!ar) break;
			if ((quoteCount%2) === 1){
	            result = result.concat(append(ar[0]));
	            continue;
	        }
	        switch (ar[0]){
				case "dddd":
					result = result.concat(dtf.getDayNames()[value.getDay()]);
					break;
				case "ddd":
					result = result.concat(dtf.getAbbreviatedDayNames()[value.getDay()]);
					break;
				case "dd":
	                result = result.concat(addLeadingZero(value.getDate()));
					break;
				case "d":
	                result = result.concat(value.getDate());
					break;
				case "MMMM":
	                result = result.concat(dtf.getMonthNames()[value.getMonth()]);
					break;
				case "MMM":
	                result = result.concat(dtf.getAbbreviatedMonthNames()[value.getMonth()]);
					break;
				case "MM":
	                result = result.concat(addLeadingZero(value.getMonth() + 1));
					break;
				case "M":
	                result = result.concat(value.getMonth() + 1);
					break;
				case "yyyy":
	                result = result.concat(value.getFullYear());
					break;
				case "yy":
	                result = result.concat(addLeadingZero(value.getFullYear()%100));
					break;
				case "y":
	                result = result.concat(value.getFullYear()%100);
					break;
				case "hh":
	                hour = value.getHours()%12;
					if (hour === 0) hour = 12;
					result = result.concat(addLeadingZero(hour));
					break;
				case "h":
		            hour = value.getHours()%12;
		            if (hour === 0) hour = 12;
		            result = result.concat(hour);
		            break;
				case "HH":
	                result = result.concat(addLeadingZero(value.getHours()));
					break;
				case "H":
	                result = result.concat(value.getHours());
					break;
				case "mm":
	                result = result.concat(addLeadingZero(value.getMinutes()));
					break;
				case "m":
	                result = result.concat(value.getMinutes());
					break;
				case "ss":
	                result = result.concat(addLeadingZero(value.getSeconds()));
					break;
				case "s":
	                result = result.concat(value.getSeconds());
					break;
				case "tt":
	                result = result.concat((value.getHours() < 12) ? dtf.getAMDesignator() : dtf.getPMDesignator());
					break;
				case "t":
	                result = result.concat(((value.getHours() < 12) ? dtf.getAMDesignator() : dtf.getPMDesignator()).charAt(0));
					break;
				case "f":
	                result = result.concat(addLeadingZeros(value.getMilliseconds()).charAt(0));
					break;
				case "ff":
	                result = result.concat(addLeadingZeros(value.getMilliseconds()).substr(0, 2));
					break;
				case "fff":
	                result = result.concat(addLeadingZeros(value.getMilliseconds()));
					break;
				case "z":
	                hour = value.getTimezoneOffset()/60;
					result = result.concat(((hour >= 0) ? '+' : '-') + Math.floor(Math.abs(hour)));
					break;
				case "zz":
	                hour = value.getTimezoneOffset()/60;
					result = result.concat(((hour >= 0) ? '+' : '-') + addLeadingZero(Math.floor(Math.abs(hour))));
					break;
				case "zzz":
	                hour = value.getTimezoneOffset()/60;
					result = result.concat(((hour >= 0) ? '+' : '-') + addLeadingZero(Math.floor(Math.abs(hour))) +
	                dtf.getTimeSeparator() + addLeadingZero(Math.abs(value.getTimezoneOffset()%60)));
					break;
	        }
	    }
	    return result;		
	}
}

Bitrix.DateTimeUtility._instance = null;
Bitrix.DateTimeUtility.getInstance = function(){
	if(this._instance == null){
		this._instance = new Bitrix.DateTimeUtility();
		this._instance.initialize();
	}
	return this._instance;
}

Bitrix.DateTimeUtility.parseLocaleExact = function(value, format){
	return this.getInstance().parseLocaleExact(value, format);
}

Bitrix.DateTimeUtility.parseLocale = function(value){
	return this.getInstance().parseLocale(value);
}

Bitrix.DateTimeUtility.formatLocale = function(value, format){
	return this.getInstance().formatLocale(value, format);
}

Bitrix.DateTimeUtility.isFormatContainsTime =  function(format){
	return this.getInstance().isFormatContainsTime(format);	
}

Bitrix.DateTimeUtility.getCurrentCultureID = function(){
	return this.getInstance()._getCurrentCultureID();	
}

Bitrix.DateTimeUtility.getCurrentCultureDateTimeFormat = function(){
	return this.getInstance()._getCurrentCultureDateTimeFormat();
}


if(typeof(Bitrix.DateTimeUtility.registerClass) == "function" && !Type.isClass(Bitrix.DateTimeUtility))
	Bitrix.DateTimeUtility.registerClass("Bitrix.DateTimeUtility");
}

if(typeof(Bitrix.DateTimeFormatInfo) == 'undefined') {

Bitrix.DateTimeFormatInfo = function(){
	this._initialized = false;
	this._cultureID = ""; 
	this._cultureName = "";
	this._dayNames = null;	
	this._abbreviatedDayNames = null;
	this._monthNames = null;	
	this._abbreviatedMonthNames = null;	
	this._amDesignator = "";
	this._pmDesignator = "";
	this._calendarTwoDigitYearMax = 1999;
	this._timeSeparator = ":";
	this._patterns = new Object();
	
	this._parseRegExp = null;
}

Bitrix.DateTimeFormatInfo.prototype = {
	initialize: function(cultureID, cultureName, dayNames, abbreviatedDayNames, monthNames, abbreviatedMonthNames, amDesignator, pmDesignator, calendarTwoDigitYearMax, timeSeparator, patterns){
		this._initialized = true;
		if(!(typeof(cultureID) == "string" || cultureID instanceof String) || cultureID.length == 0) throw "cultureID is not defined!";
		this._cultureID = cultureID;
		this._cultureName = cultureName;
		this._dayNames = dayNames;
		this._abbreviatedDayNames = abbreviatedDayNames;
		this._monthNames = monthNames;
		this._abbreviatedMonthNames = abbreviatedMonthNames;
		this._amDesignator = amDesignator;
		this._pmDesignator = pmDesignator;
		this._calendarTwoDigitYearMax = calendarTwoDigitYearMax;
		this._timeSeparator = timeSeparator;
		if(patterns && typeof(patterns) == 'object'){
			this._patterns["shortDate"] = "shortDate" in patterns ? patterns["shortDate"] : "MM/dd/yyyy";
			this._patterns["longDate"] = "longDate" in patterns ? patterns["longDate"] : "dddd, dd MMMM yyyy";
			this._patterns["shortTime"] = "shortTime" in patterns ? patterns["shortTime"] : "HH:mm";
			this._patterns["longTime"] = "longTime" in patterns ? patterns["longTime"] : "HH:mm:ss";
			this._patterns["fullDateTime"] = "fullDateTime" in patterns ? patterns["fullDateTime"] : "dddd, dd MMMM yyyy HH:mm:ss";
			this._patterns["monthDay"] = "monthDay" in patterns ? patterns["monthDay"] : "MMMM dd";
			this._patterns["sortableDateTime"] = "sortableDateTime" in patterns ? patterns["sortableDateTime"] : "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
			this._patterns["yearMonth"] = "yearMonth" in patterns ? patterns["yearMonth"] : "yyyy MMMM";
			this._patterns["invariantSortableDateTime"] = "invariantSortableDateTime" in patterns ? patterns["invariantSortableDateTime"] : "MM/dd/yyyy HH:mm:ss";					
		}	
	},
	getCultureName: function(){
		return this._cultureName; 
	},
	getDayNames: function(){
		return this._dayNames;
	},
	getAbbreviatedDayNames: function(){
		return this._abbreviatedDayNames;
	},
	getMonthNames: function(){
		return this._monthNames;
	},
	getAbbreviatedMonthNames: function(){
		return this._abbreviatedMonthNames;
	},
	getAMDesignator: function(){
		return this._amDesignator;
	},
	getPMDesignator: function(){
		return this._pmDesignator;
	},
	getCalendarTwoDigitYearMax: function(){
		return this._calendarTwoDigitYearMax;
	},
	getShortDatePattern: function(){
		return this._patterns["shortDate"];
	},
	getLongDatePattern: function(){
		return this._patterns["longDate"];
	},
	getShortTimePattern: function(){
		return this._patterns["shortTime"] ;
	},
	getLongTimePattern: function(){
		return this._patterns["longTime"];
	},	
	getFullDateTimePattern: function(){
		return this._patterns["fullDateTime"];
	},	
	getMonthDayPattern: function(){
		return this._patterns["monthDay"];
	},
	getSortableDateTimePattern: function(){
		return this._patterns["sortableDateTime"];
	},	
	getYearMonthPattern: function(){
		return this._patterns["yearMonth"];
	},
	getUniversalSortableDateTimePattern: function(){
		return this._patterns["invariantSortableDateTime"];
	},
    getUniversalSortableDatePattern: function(){
		return this._patterns["invariantSortableDate"];
	},
	getPatternsArray: function(){
		var result = new Array();
		result.push(this._patterns["sortableDateTime"]);
		result.push(this._patterns["fullDateTime"]);
		result.push(this._patterns["longTime"]);
		result.push(this._patterns["longDate"]);
		result.push(this._patterns["shortTime"]);
		result.push(this._patterns["shortDate"]);
		result.push(this._patterns["monthDay"]);
		result.push(this._patterns["yearMonth"]);
		result.push(this._patterns["invariantSortableDateTime"]);
		result.push(this._patterns["invariantSortableDate"]);			
		return result;
	},
	getTimeSeparator: function(){
		return this._timeSeparator;
	},
	getParseRegExp: function(format){
	    
		if(!this._parseRegExp) return null;
		return format in this._parseRegExp ? this._parseRegExp[format] : null;
	},
	setParseRegExp: function(format, regex){
		if(!this._parseRegExp)
			this._parseRegExp = new Object();
		this._parseRegExp[format] = regex;
	}
}

Bitrix.DateTimeFormatInfo._entries = null;
Bitrix.DateTimeFormatInfo.getEntry = function(cultureID){
	if(this._entries == null) return null;
	return cultureID in this._entries ? this._entries[cultureID] : null;
}
Bitrix.DateTimeFormatInfo.createEntry = function(cultureID, cultureName, dayNames, abbreviatedDayNames, monthNames, abbreviatedMonthNames, amDesignator, pmDesignator, calendarTwoDigitYearMax, timeSeparator, patterns){
	var entry = this.getEntry(cultureID);
	if(entry) throw "DateTimeFormatInfo '" + cultureID + "' already exists!";
	entry = new Bitrix.DateTimeFormatInfo();
	entry.initialize(cultureID, cultureName, dayNames, abbreviatedDayNames, monthNames, abbreviatedMonthNames, amDesignator, pmDesignator, timeSeparator, calendarTwoDigitYearMax, patterns);
	if(this._entries == null) this._entries = new Object();
	this._entries[cultureID] = entry;
	return entry;
}

Bitrix.DateTimeFormatInfo.createEntry("<%= JSEncode(GetCultureID())%>", "<%= JSEncode(GetCultureName())%>", <%= GetDayNamesJSON() %>, <%= GetAbbreviatedDayNamesJSON() %>, <%= GetMonthNamesJSON() %>, <%= GetAbbreviatedMonthNamesJSON() %>, "<%= JSEncode(GetAMDesignator())%>", "<%= JSEncode(GetPMDesignator())%>", <%= GetCalendarTwoDigitYearMax()%>, "<%= JSEncode(GetTimeSeparator())%>", <%= GetPatternsJSON() %>);
}
if(typeof(Sys) !== "undefined")Sys.Application.notifyScriptLoaded();

