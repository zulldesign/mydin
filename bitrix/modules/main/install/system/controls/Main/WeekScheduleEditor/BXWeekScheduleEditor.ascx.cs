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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Script.Serialization;
using System.ComponentModel;

[PersistChildren(false)]
[ParseChildren(true, "HourSpans")]
[DefaultProperty("HourSpans")]
public partial class BXWeekScheduleEditorControl : BXControl
{
    Collection<BXWeekScheduleHourSpanControl> _hourSpans = null;
    [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
    public Collection<BXWeekScheduleHourSpanControl> HourSpans
    {
        get
        {
            if (_hourSpans != null)
                return _hourSpans;

            _hourSpans = new Collection<BXWeekScheduleHourSpanControl>();
            if (IsPostBack)
            {
                string dataStr = Request.Form[DataElementUniqueID];
                if (dataStr != null)
                {
                    BXWeekScheduleHourSpanJSONConverter hourConverter = new BXWeekScheduleHourSpanJSONConverter(CultureInfo.InvariantCulture.DateTimeFormat);
                    System.Web.Script.Serialization.JavaScriptSerializer jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    jsSerializer.RegisterConverters(new JavaScriptConverter[] { hourConverter });
                    StringBuilder hoursJavascript = new StringBuilder();
                    List<BXWeekScheduleHourSpanSequence> sqLst = jsSerializer.Deserialize<List<BXWeekScheduleHourSpanSequence>>(dataStr);

                    foreach (BXWeekScheduleHourSpanSequence sq in sqLst)
                        foreach(BXWeekScheduleHourSpan sp in sq)
                            _hourSpans.Add(new BXWeekScheduleHourSpanControl(sp));
                }
            }
            return _hourSpans;
        }
    }

    protected override void OnPreRender(EventArgs e)
    {
        ClientScriptManager sm = Page.ClientScript;
        if (!sm.IsStartupScriptRegistered(GetType(), "WeekScheduleEditorFirstDayOfWeek"))
        {
            DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            sm.RegisterClientScriptInclude(GetType(), "WeekScheduleEditor", VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/weekscheduleeditor/js/week_schedule_ed.js"));
            sm.RegisterStartupScript(
                GetType(),
                "WeekScheduleEditorFirstDayOfWeek",
                string.Concat("window.setTimeout(function(){Bitrix.WeekScheduleEditor.firstDayOfWeek = Bitrix.WeekDay.", firstDayOfWeek.ToString("g").ToLowerInvariant(), ";}, 10);"),
                true
                );
        }
        if (!sm.IsStartupScriptRegistered(GetType(), "WeekScheduleEditorAbbreviatedDayNames"))
        {
            string[] abbrDayNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
            StringBuilder abbrDayNamesJavascript = new StringBuilder("Bitrix.WeekScheduleEditor.abbreviatedDays = {");
            for (int i = 0; i < abbrDayNames.Length; i++)
            {
                if (i > 0)
                    abbrDayNamesJavascript.Append(", ");
                abbrDayNamesJavascript.AppendFormat("{0}:\"{1}\"", Enum.GetName(typeof(DayOfWeek), i).ToLowerInvariant(), abbrDayNames[i]);
            }
            abbrDayNamesJavascript.Append("}");

            sm.RegisterStartupScript(
                GetType(),
                "WeekScheduleEditorAbbreviatedDayNames",
                string.Concat("window.setTimeout(function(){", abbrDayNamesJavascript.ToString(), ";}, 10);"),
                true
                );
        }
        if (!sm.IsStartupScriptRegistered(GetType(), "WeekScheduleEditorDayNames"))
        {
            string[] dayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;
            StringBuilder dayNamesJavascript = new StringBuilder("Bitrix.WeekScheduleEditor.days = {");
            dayNamesJavascript.Append("all:\"\"");
            for (int i = 0; i < dayNames.Length; i++)
                dayNamesJavascript.Append(", ").AppendFormat("{0}:\"{1}\"", Enum.GetName(typeof(DayOfWeek), i).ToLowerInvariant(), dayNames[i]);
            dayNamesJavascript.Append("}");

            sm.RegisterStartupScript(
                GetType(),
                "WeekScheduleEditorDayNames",
                string.Concat("window.setTimeout(function(){", dayNamesJavascript.ToString(), ";}, 10);"),
                true
                );
        }
        if (!sm.IsStartupScriptRegistered(GetType(), "WeekScheduleEditorSummryText"))
        {
            StringBuilder summaryTextJavascript = new StringBuilder("Bitrix.WeekScheduleEditor.summaryText = {");
            summaryTextJavascript.Append("title:\"").Append(GetMessageJS("Summary.Title")).Append("\", displayEveryDay:\"").Append(GetMessageJS("Summary.DisplayEveryDay")).Append("\", displayNever:\"").Append(GetMessageJS("Summary.DisplayNever")).Append("\"}");
            sm.RegisterStartupScript(
                GetType(),
                "WeekScheduleEditorSummryText",
                string.Concat("window.setTimeout(function(){", summaryTextJavascript.ToString(), ";}, 10);"),
                true
                );
        }
        if (!sm.IsStartupScriptRegistered(GetType(), "WeekScheduleEditorLegendText"))
        {
            StringBuilder legendTextJavascript = new StringBuilder("Bitrix.WeekScheduleEditor.legendText = {");
            legendTextJavascript.Append("choiced:\"").Append(GetMessageJS("Legend.Choiced")).Append("\", notChoiced:\"").Append(GetMessageJS("Legend.NotChoiced")).Append("\"}");
            sm.RegisterStartupScript(
                GetType(),
                "WeekScheduleEditorLegendText",
                string.Concat("window.setTimeout(function(){", legendTextJavascript.ToString(), ";}, 10);"),
                true
                );
        }
        sm.RegisterStartupScript(
            GetType(),
            string.Concat("WeekScheduleEditorConstructor_", ClientID),
            string.Concat("window.setTimeout(function(){Bitrix.WeekScheduleEditor.create('", ClientID, "', '", DataElementClientID, "', ", EnableSummary.ToString().ToLowerInvariant(), ").construct('", ContainerClientID, "');}, 10);"),
            true
            );

        sm.RegisterOnSubmitStatement(
            GetType(),
            string.Concat("WeekScheduleEditorConstructor_", ClientID),
            string.Concat("Bitrix.WeekScheduleEditor.getItemById('", ClientID, "')._externalizeData();")
            );
        base.OnPreRender(e);
    }

    protected override void Render(HtmlTextWriter writer)
    {
        writer.AddAttribute(HtmlTextWriterAttribute.Id, ContainerClientID);
        writer.RenderBeginTag(HtmlTextWriterTag.Div);
        writer.AddAttribute(HtmlTextWriterAttribute.Id, DataElementClientID);
        writer.AddAttribute(HtmlTextWriterAttribute.Name, DataElementUniqueID);
        writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
        IList<BXWeekScheduleHourSpanControl> hourControls = HourSpans;
        if (hourControls.Count > 0)
        {
            BXWeekScheduleHourSpan[] hours = new BXWeekScheduleHourSpan[hourControls.Count];
            for (int i = 0; i < hourControls.Count; i++)
                hours[i] = hourControls[i].Item;

            BXWeekScheduleHourSpanJSONConverter hourConverter = new BXWeekScheduleHourSpanJSONConverter(CultureInfo.InvariantCulture.DateTimeFormat);
            System.Web.Script.Serialization.JavaScriptSerializer jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            jsSerializer.RegisterConverters(new JavaScriptConverter[] { hourConverter });
            StringBuilder hoursJavascript = new StringBuilder();
            jsSerializer.Serialize(hours, hoursJavascript);

            writer.AddAttribute(HtmlTextWriterAttribute.Value, hoursJavascript.ToString(), true);
        }
        writer.RenderBeginTag(HtmlTextWriterTag.Input);
        writer.RenderEndTag();
        writer.RenderEndTag();
    }

    private bool _enableSummary = false;
    public bool EnableSummary
    {
        get { return _enableSummary; }
        set { _enableSummary = value; }
    }

    private string _containerClientID = null;
    private string ContainerClientID
    {
        get
        {
            return _containerClientID ?? (_containerClientID = string.Concat(ClientID, "_Container"));
        }
    }

    private string _dataElementClientID = null;
    private string DataElementClientID
    {
        get
        {
            return _dataElementClientID ?? (_dataElementClientID = string.Concat(ClientID, "_Data"));
        }
    }

    private string _dataElementUniqueID = null;
    private string DataElementUniqueID
    {
        get
        {
            return _dataElementUniqueID ?? (_dataElementUniqueID = string.Concat(UniqueID, "_Data"));
        }
    }
}
