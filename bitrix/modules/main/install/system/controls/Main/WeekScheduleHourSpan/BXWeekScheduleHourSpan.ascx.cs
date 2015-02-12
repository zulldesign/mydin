using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using Bitrix;

public partial class BXWeekScheduleHourSpanControl : BXControl
{
    public BXWeekScheduleHourSpanControl()
    {
    }

    public BXWeekScheduleHourSpanControl(BXWeekScheduleHourSpan source)
    {
        if (source == null) 
            throw new ArgumentNullException("source");

        _from = source.FromHourOfWeek;
        _till = source.TillHourOfWeek;
    }

    private int _from = 0;
    /// <summary>
    /// Час недели от 0 до 168 с которого начинается период.
    /// </summary>
    public int FromHourOfWeek
    {
        get { return _from; }
        set
        {
            if (value < 0 || value > 168)
                throw new ArgumentOutOfRangeException("value", value, GetMessageRaw("Error.InvalidHourOfWeekRange"));
            _from = value;
        }
    }

    public DayOfWeek FromDayOfWeek
    {
        get 
        {
            return BXWeekScheduleHourSpan.GetDayOfWeek(_from, true);
        }

        set 
        {
            int hourOfDay = _from > 0 ? BXWeekScheduleHourSpan.GetHourOfDay(_from, true) : 0;
            _from = BXWeekScheduleHourSpan.GetFirstHourOfDay(value) + hourOfDay;
        }
    }

    public int FromHourOfDay
    {
        get
        {
            return BXWeekScheduleHourSpan.GetHourOfDay(_from, true);
        }

        set
        {
            int firstHourOfDay = BXWeekScheduleHourSpan.GetFirstHourOfDay(BXWeekScheduleHourSpan.GetDayOfWeek(_from, true));
            _from = firstHourOfDay + value;
        }
    }

    private int _till = 0;
    /// <summary>
    /// Час недели от 0 до 168 на котором заканчивается период.       
    /// </summary>
    public int TillHourOfWeek
    {
        get { return _till; }
        set
        {
            if (value < 0 || value > 168)
                throw new ArgumentOutOfRangeException("value", value, GetMessageRaw("Error.InvalidHourOfWeekRange"));
            _till = value;
        }
    }

    public DayOfWeek TillDayOfWeek
    {
        get
        {
            return BXWeekScheduleHourSpan.GetDayOfWeek(_till, false);
        }
        set
        {
            int hourOfDay = _till > 0 ? BXWeekScheduleHourSpan.GetHourOfDay(_till, true) : 0;
            _till = BXWeekScheduleHourSpan.GetFirstHourOfDay(value) + hourOfDay;
        }
    }

    public int TillHourOfDay
    {
        get
        {
            return BXWeekScheduleHourSpan.GetHourOfDay(_till, false);
        }
        set
        {
            int firstHourOfDay = BXWeekScheduleHourSpan.GetFirstHourOfDay(BXWeekScheduleHourSpan.GetDayOfWeek(_till, true));
            _till = firstHourOfDay + value;
        }
    }

    public BXWeekScheduleHourSpan Item
    {
        get { return new BXWeekScheduleHourSpan(_from, _till); }
    }
}
