using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Reflection;

using Bitrix;
using Bitrix.UI;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.Main;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using System.Web.Script.Serialization;

public partial class bitrix_admin_Captcha : Bitrix.UI.BXAdminPage
{
    public class GenericDictionary : IEnumerator, IEnumerable
    {
        private Dictionary<string, object> _dict = new Dictionary<string, object>();

        public void Add<T>(string key, T value) where T : class
        {
            if (_dict.ContainsKey(key))
            {
                _dict[key] = value;
            }
            else
                _dict.Add(key, value);
        }

        public T GetValue<T>(string key) where T : class
        {
            if (_dict.ContainsKey(key))
            {
                return _dict[key] as T;
            }
            else
                return null;
        }

        private int _index;

        public bool MoveNext()
        {
            _index++;
            return (_index < _dict.Count);
        }

        public void Reset()
        {
            _index = -1;
        }

        public object Current
        {
            get
            {
                try
                {
                    int current = 0;
                    object res = null;
                    foreach (KeyValuePair<string, object> k in _dict)
                    {
                        if (current == _index)
                        {
                            res = k.Value;
                            break;
                        }
                        else
                        {
                            current++;
                        }
                    }
                    return res;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
            
        }

        public IEnumerator GetEnumerator()
        {
            return (_dict as IEnumerable).GetEnumerator();
        }

        public int Count
        { 
            get 
            {
                return _dict.Count;
            }
            set { }
        }

    }

    public class OptionValue<Inner,Front>
    {
        public Inner InnerValue;
        public Front FrontValue;

        public OptionValue(Inner inn, Front fr)
        {
            this.InnerValue = inn;
            this.FrontValue = fr;
        }
    }

    public T GetOptionFromDB<T>(string name, T defaultValue)
    {
        return BXOptionManager.GetOption<T>("main", "Captcha_" + name, defaultValue);
    }

    private void SetOptionToDB<T>(string name, T value)
    {
        BXOptionManager.SetOption<T>("main", "Captcha_" + name, value);
    }

    private class ValueHelpter
    {
        private BXCaptchaImage defaultOptions = new BXCaptchaImage();

        public T GetOptionFromDB<T>(string name, T defaultValue)
        {
            return BXOptionManager.GetOption<T>("main", "Captcha_" + name, defaultValue);
        }

        private void SetOptionToDB<T>(string name, T value)
        {
            BXOptionManager.SetOption<T>("main", "Captcha_" + name, value);
        }

        public int GetInt(string key)
        {
            return this.GetOptionFromDB<int>(key, this.GetDefaultInt(key));
        }

        public int GetDefaultInt(string key)
        {
            switch (key)
            {
				case "TextLength":
					return defaultOptions.TextLength;
                case "TextTransparency":
                    return defaultOptions.TextTransparency;
                case "NumCircles":
                    return defaultOptions.NumCircles;
                case "NumLines":
                    return defaultOptions.NumLines;
                case "LeftOffset":
                    return defaultOptions.LeftOffset;
                case "FontSize":
                    return defaultOptions.FontSize;
                case "VertAngleOfDipFrom":
                    return defaultOptions.VertAngleOfDipFrom;
                case "VertAngleOfDipTo":
                    return defaultOptions.VertAngleOfDipTo;
                case "SymbolDistanceFrom":
                    return defaultOptions.SymbolDistanceFrom;
                case "SymbolDistanceTo":
                    return defaultOptions.SymbolDistanceTo;
                default:
                    return 0;
            }
        }

        public Color GetColor(string key)
        {
            string d = System.Drawing.ColorTranslator.ToHtml(this.GetDefaultColor(key));
            string c = this.GetOptionFromDB<string>(key, d);
            return System.Drawing.ColorTranslator.FromHtml(c);
        }

        public Color GetDefaultColor(string key)
        { 
            switch(key)
            {
                case "BackgroundColorFrom":
                    return defaultOptions.BackgroundColorFrom;
                case "BackgroundColorTo":
                    return defaultOptions.BackgroundColorTo;
                case "CircleColorFrom":
                    return defaultOptions.CircleColorFrom;
                case "CircleColorTo":
                    return defaultOptions.CircleColorTo;
                case "FontColorFrom":
                    return defaultOptions.FontColorFrom;
                case "FontColorTo":
                    return defaultOptions.FontColorTo;
                case "LineColorFrom":
                    return defaultOptions.LineColorFrom;
                case "LineColorTo":
                    return defaultOptions.LineColorTo;
                case "BorderColor":
                    return defaultOptions.BorderColor;
                default:
                    return Color.Black;
            }
        }

        public bool GetBool(string key)
        {
            return this.GetOptionFromDB<bool>(key, this.GetDefaultBool(key));
        }

        public bool GetDefaultBool(string key)
        { 
            switch (key)
            {
                case "TextUnderLines":
                    return defaultOptions.TextUnderLines;
                case "NonlinearDistortion":
                    return defaultOptions.NonlinearDistortion;
                default:
                    return false;
            }
                
        }

        public string GetString(string key)
        {
            return this.GetOptionFromDB<string>(key, this.GetDefaultString(key));
        }

        public string GetDefaultString(string key)
        {
            switch (key)
            { 
                case "RandomTextChars":
                    return defaultOptions.TextChars;
                case "FontWhitelist":
                    return defaultOptions.FontWhitelist;
                default:
                    return string.Empty;
            }
        }

        public string[] GetStringArray(string key)
        {
            string res = this.GetOptionFromDB<string>(key, this.GetDefaultString(key));
            string[] values = res.Split(';');
            return values;
        }

        public static Color HexToColor(string hex)
        {
            return System.Drawing.ColorTranslator.FromHtml(hex);
        }

        public static string ColorToHex(Color c)
        {
            return System.Drawing.ColorTranslator.ToHtml(c);
        }
    }

    private bool ValidateColor(string color)
    {
        Regex r = new Regex("^#(?:[0-9a-fA-F]{3}){1,2}$");
        if (color[0] != '#')
        {
            color.Insert(0, "#");
        }
        if (r.Match(color).Success)
        {
            return true;
        }
        return false;
    }

    private bool ColorSmaller(string small, string big)
    {
        Color csmall = System.Drawing.ColorTranslator.FromHtml(small);
        Color cbig = System.Drawing.ColorTranslator.FromHtml(big);
        if (csmall.R <= cbig.R && csmall.G <= cbig.G && csmall.B <= cbig.B)
            return true;
        else 
            return false;
    }

    private bool IntSmaller(string small, string big)
    {
        int ismall = int.Parse(small);
        int ibig = int.Parse(big);
        return (ismall <= ibig);
    }

    private void DoError(string paramName)
    {
        errorMessage.AddErrorMessage(GetMessage("WrongPrarameter") + ": " + GetMessage(paramName));
    }

    private void DoError(string paramName1,string paramName2)
    {
        errorMessage.AddErrorMessage(GetMessage("WrongPrarameters") + ": \"" + GetMessage(paramName1) +
            "\"" + " , " + "\"" + GetMessage(paramName2) + "\"");        
    }

    private bool ValidateFields()
    {
        //int preset;
        //if (!int.TryParse(this.Preset.Text, out preset))
        //{
        //    DoError("presets");
        //    return false;
        //}
		int textLength;
		if (!int.TryParse(this.TextLength.Text, out textLength) || textLength <= 0 || textLength >= 100)
        {
            DoError(this.TextLength.ID);
            return false;
        }

        int textTransparency;
        if (!int.TryParse(this.TextTransparency.Text, out textTransparency) || textTransparency<0 || textTransparency>100)
        {
            DoError(this.TextTransparency.ID);
            return false;
        }

        int numLines;
        if (!int.TryParse(this.NumLines.Text, out numLines))
        {
            DoError(this.NumLines.ID);
            return false;
        }

        int numCircles;
        if (!int.TryParse(this.NumCircles.Text, out numCircles))
        {
            DoError(this.NumCircles.ID);
            return false;
        }

        int leftOffset;
        if (!int.TryParse(this.LeftOffset.Text,out leftOffset))
        {
            DoError(this.LeftOffset.ID);
            return false;        
        }

        int fontSize;
        if (!int.TryParse(this.FontSize.Text,out fontSize))
        {
            DoError(this.FontSize.ID);
            return false;        
        }

        int symbolDistanceFrom;
        if (!int.TryParse(this.SymbolDistanceFrom.Text, out symbolDistanceFrom))
        {
            DoError(this.SymbolDistanceFrom.ID);
            return false;        
        }

        int symbolDistanceTo;
        if (!int.TryParse(this.SymbolDistanceTo.Text, out symbolDistanceTo))
        {
            DoError(this.SymbolDistanceTo.ID);
            return false;
        }

        if (this.RandomTextChars.Text.Trim().Length == 0)
        {
            DoError(this.RandomTextChars.ID);
            return false;            
        }

        int vertAngleOfDipFrom;
        if (!int.TryParse(this.VertAngleOfDipFrom.Text, out vertAngleOfDipFrom) || vertAngleOfDipFrom < -360 || vertAngleOfDipFrom > 360)
        {
            DoError(this.VertAngleOfDipFrom.ID);
            return false;
        }

        int vertAngleOfDipTo;
        if (!int.TryParse(this.VertAngleOfDipTo.Text, out vertAngleOfDipTo) && vertAngleOfDipTo < -360 && vertAngleOfDipTo > 360)
        {
            DoError(this.VertAngleOfDipTo.ID);
            return false;
        }

        //проверяем цвета
        if (!ValidateColor(this.BackgroundColorFrom.Text))
        {
            DoError(this.BackgroundColorFrom.ID);
            return false;
        }
        if (!ValidateColor(this.BackgroundColorTo.Text))
        {
            DoError(this.BackgroundColorTo.ID);
            return false;
        }
        if (!ValidateColor(this.CircleColorFrom.Text))
        {
            DoError(this.CircleColorFrom.ID);
            return false;
        }
        if (!ValidateColor(this.CircleColorTo.Text))
        {
            DoError(this.CircleColorTo.ID);
            return false;
        }
        if (!ValidateColor(this.FontColorFrom.Text))
        {
            DoError(this.FontColorFrom.ID);
            return false;
        }
        if (!ValidateColor(this.FontColorTo.Text))
        {
            DoError(this.FontColorFrom.ID);
            return false;
        }
        if (!ValidateColor(this.LineColorFrom.Text))
        {
            DoError(this.LineColorFrom.ID);
            return false;
        }
        if (!ValidateColor(this.LineColorTo.Text))
        {
            DoError(this.LineColorTo.ID);
            return false;
        }
        if (!ValidateColor(this.BorderColor.Text))
        {
            DoError(this.BorderColor.ID);
            return false;
        }
        //validate fonts

        foreach (ListItem l in this.FontWhitelist.Items)
        {
            if (l.Selected && Array.BinarySearch(InnerDefaultFonts, l.Value) < 0)
            {
                DoError(this.FontWhitelist.ID);
                return false;
            }
        }

        return true;
    }

    private bool CombineFields()
    {
        if (!ColorSmaller(this.BackgroundColorFrom.Text,this.BackgroundColorTo.Text))
        {
            DoError(this.BackgroundColorFrom.ID, this.BackgroundColorTo.ID);
            return false;
        }

        if (!ColorSmaller(this.CircleColorFrom.Text, this.CircleColorTo.Text))
        {
            DoError(this.CircleColorFrom.ID, this.CircleColorTo.ID);
            return false;
        }

        if (!ColorSmaller(this.LineColorFrom.Text, this.LineColorTo.Text))
        {
            DoError(this.LineColorFrom.ID, this.LineColorTo.ID);
            return false;        
        }

        if (!ColorSmaller(this.FontColorFrom.Text, this.FontColorTo.Text))
        {
            DoError(this.FontColorFrom.ID, this.FontColorTo.ID);
            return false;                    
        }
        if (!IntSmaller(this.VertAngleOfDipFrom.Text,this.VertAngleOfDipTo.Text))
        {
            DoError(this.VertAngleOfDipFrom.ID, this.VertAngleOfDipTo.ID);
            return false;                            
        }
        if (!IntSmaller(this.SymbolDistanceFrom.Text, this.SymbolDistanceTo.Text))
        {
            DoError(this.SymbolDistanceFrom.ID, this.SymbolDistanceTo.ID);
            return false;
        }

        return true;
    }

    private void NormilizeColors()
    {
        foreach (WebControl wc in BXTabControlTab1.Controls)
        {
            if (wc.Attributes["DataType"] != null && wc.Attributes["DataType"]=="color")
            {
                TextBox c = (TextBox)wc;
                c.Text = c.Text.Trim();
                if (c.Text[0] != '#')
                {
                    c.Text.Insert(0, "#");
                }
            }
        }
    }

    protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
    {
        if (e.CommandName == "cancel")
        {
            GetOptionsFromDB();
            return;
        }

        NormilizeColors();
        
        if (!ValidateFields())
        {
            return;
        }
        else if (!CombineFields())
        {
            return;
        }
        /*
        else if (!CheckFonts())
        {
            Response.Write("лишние символы");
        }*/
        else
        {
            SetOptionsToDB(this.Preset.SelectedValue);
            Response.Redirect(Request.Url.AbsoluteUri);
        }
    }

    private void SetOptionsToDB(string preset)
    {
        SetOptionToDB<string>("Preset", preset);
		SetOptionToDB<int>(this.TextLength.ID, int.Parse(this.TextLength.Text));
        SetOptionToDB<int>(this.TextTransparency.ID, int.Parse(this.TextTransparency.Text));
        SetOptionToDB<string>(this.BackgroundColorFrom.ID, this.BackgroundColorFrom.Text);
        SetOptionToDB<string>(this.BackgroundColorTo.ID, this.BackgroundColorTo.Text);
        SetOptionToDB<int>(this.NumCircles.ID, int.Parse(this.NumCircles.Text));
        SetOptionToDB<string>(this.CircleColorFrom.ID, CircleColorFrom.Text);
        SetOptionToDB<string>(this.CircleColorTo.ID, this.CircleColorTo.Text);
        SetOptionToDB<bool>(this.TextUnderLines.ID, this.TextUnderLines.Checked);
        SetOptionToDB<int>(this.NumLines.ID, int.Parse(this.NumLines.Text));
        SetOptionToDB<string>(this.LineColorFrom.ID, this.LineColorFrom.Text);
        SetOptionToDB<string>(this.LineColorTo.ID, this.LineColorTo.Text);
        SetOptionToDB<int>(this.LeftOffset.ID, int.Parse(this.LeftOffset.Text));
        SetOptionToDB<int>(this.FontSize.ID, int.Parse(this.FontSize.Text));
        SetOptionToDB<string>(this.FontColorFrom.ID, this.FontColorFrom.Text);
        SetOptionToDB<string>(this.FontColorTo.ID, this.FontColorTo.Text);
        SetOptionToDB<int>(this.VertAngleOfDipFrom.ID, int.Parse(this.VertAngleOfDipFrom.Text));
        SetOptionToDB<int>(this.VertAngleOfDipTo.ID, int.Parse(this.VertAngleOfDipTo.Text));
        SetOptionToDB<int>(this.SymbolDistanceFrom.ID, int.Parse(this.SymbolDistanceFrom.Text));
        SetOptionToDB<int>(this.SymbolDistanceTo.ID, int.Parse(this.SymbolDistanceTo.Text));
        SetOptionToDB<bool>(this.NonlinearDistortion.ID, this.NonlinearDistortion.Checked);
        SetOptionToDB<string>(this.BorderColor.ID, this.BorderColor.Text);
        SetOptionToDB<string>(this.FontWhitelist.ID, ListBoxToString(this.FontWhitelist));
        SetOptionToDB<string>(this.RandomTextChars.ID, this.RandomTextChars.Text);
    }

    private Color HexToColor(string hex)
    {
        return System.Drawing.ColorTranslator.FromHtml(hex);
    }

    private string ColorToHex(Color c)
    {
        return System.Drawing.ColorTranslator.ToHtml(c);
    }

    private string ListBoxToString(ListBox listbox)
    { 
        string[] result = new string[listbox.Items.Count];
        int cnt = 0;
        foreach (ListItem l in listbox.Items)
        {
            if (l.Selected)
            {
                result[cnt] = l.Value;
                cnt++;
            }
        }
        Array.Resize<string>(ref result, cnt);
        return string.Join(";",result);
    }

    private void GetOptionsFromDB()
    {
        ValueHelpter helper = new ValueHelpter();
        
        foreach (WebControl c in this.BXTabControlTab1.Controls)
        {
            if (c.Attributes["DataType"] != null)
            {
                string type = c.Attributes["DataType"];
                
                switch (type)
                {
                    case "int":
                        if (c.GetType().ToString() == "System.Web.UI.WebControls.TextBox")
                        {
                            TextBox tb1 = (TextBox)c;
                            int ivalue = helper.GetInt(c.ID);
                            string fivalue = ivalue.ToString();
                            tb1.Text = helper.GetInt(c.ID).ToString();
                            CaptchaOptions.Add<OptionValue<int, string>>(c.ID, new OptionValue<int, string>(ivalue, fivalue));
                        }
                        else if (c.GetType().ToString() == "System.Web.UI.WebControls.DropDownList")
                        {
                            DropDownList dbl = (DropDownList)c;
                            int selectedOption = helper.GetInt(c.ID);
                            foreach (ListItem li in dbl.Items)
                                li.Selected = li.Value == selectedOption.ToString();
                            CaptchaOptions.Add<OptionValue<int, string>>(c.ID, new OptionValue<int, string>(selectedOption, selectedOption.ToString()));
                            break;
                        }
                        break;
                    case "color":
                        TextBox tb2 = (TextBox)c;
                        Color cvalue = helper.GetColor(c.ID);
                        tb2.Text = ValueHelpter.ColorToHex(cvalue);
                        string fcvalue = tb2.Text;
                        CaptchaOptions.Add<OptionValue<Color, string>>(c.ID, new OptionValue<Color, string>(cvalue, fcvalue));
                        break;
                    case "bool":
                        CheckBox cb = (CheckBox)c;
                        cb.Checked = helper.GetBool(c.ID);
                        bool bvalue = helper.GetBool(c.ID);
                        CaptchaOptions.Add<OptionValue<bool, string>>(c.ID, new OptionValue<bool, string>(bvalue, bvalue == true ? "Y" : "N"));
                        break;
                    case "string":
                        TextBox tb3 = (TextBox)c;
                        tb3.Text = helper.GetString(c.ID);
                        CaptchaOptions.Add<OptionValue<string, string>>(c.ID, new OptionValue<string, string>(tb3.Text, tb3.Text));
                        break;
                    case "string_array":
                        ListBox p = (ListBox)c;
                        string[] selectedOptions = helper.GetStringArray(c.ID);
                        foreach (ListItem li in p.Items)
                            if (Array.BinarySearch(selectedOptions, li.Value) >= 0)
                                li.Selected = true;
                        CaptchaOptions.Add<OptionValue<string[], string>>(c.ID, new OptionValue<string[], string>(selectedOptions, string.Join(";", selectedOptions)));
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private bool TryParseInt32(string s, out int x)
    {
        if (string.IsNullOrEmpty(s))
        {
            x = 0;
            return false;
        }

        return int.TryParse(s, out x);
    }

    private string[] defaultFonts = null;
    private string[] InnerDefaultFonts
    {
        get { return defaultFonts ?? (defaultFonts = InnerCaptcha.AvailableFontFamilies); }
    }

    public GenericDictionary CaptchaOptions = new GenericDictionary();


    private BXCaptchaImage captcha = null;
    private BXCaptchaImage InnerCaptcha
    {
        get { return captcha ?? (captcha = new BXCaptchaImage()); }
    }

    protected string GetCaptchaPresetJson()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.RegisterConverters(new JavaScriptConverter[] { new BXCaptchaImagePresetJsonConv() });
        return serializer.Serialize(BXCaptchaImage.Presets); 
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
            BXAuthentication.AuthenticationRequired();
        //при ?show_captcha=Y обращаемся к странице как к картинке
        if (!string.IsNullOrEmpty(Request["show_captcha"]) && Request["show_captcha"] == "Y")
        {
            BXCaptchaImage ci = InnerCaptcha;
            Response.ContentType = "image/jpeg";
            Response.StatusCode = 200;

            int i = 0;

			if (TryParseInt32(Request["TextLength"], out i))
                ci.TextLength = i;

            if (TryParseInt32(Request["TextTransparency"], out i))
                ci.TextTransparency = i;

			if ((!string.IsNullOrEmpty(Request["BackgroundColorFrom"]) && !string.IsNullOrEmpty(Request["BackgroundColorTo"]))
                && (ValidateColor(Request["BackgroundColorFrom"]) && ValidateColor(Request["BackgroundColorTo"]))
                && (ColorSmaller(Request["BackgroundColorFrom"], Request["BackgroundColorTo"]))
                )
            {
                    ci.BackgroundColorFrom = System.Drawing.ColorTranslator.FromHtml(Request["BackgroundColorFrom"]);
                    ci.BackgroundColorTo = System.Drawing.ColorTranslator.FromHtml(Request["BackgroundColorTo"]);
            }

            if (TryParseInt32(Request["NumCircles"], out i))
                ci.NumCircles = i;

            if ((!string.IsNullOrEmpty(Request["CircleColorFrom"]) && !string.IsNullOrEmpty(Request["CircleColorTo"]))
                && (ValidateColor(Request["CircleColorFrom"]) && ValidateColor(Request["CircleColorTo"]))
                && (ColorSmaller(Request["CircleColorFrom"], Request["CircleColorTo"]))
                )
            {
                ci.CircleColorFrom = System.Drawing.ColorTranslator.FromHtml(Request["CircleColorFrom"]);
                ci.CircleColorTo = System.Drawing.ColorTranslator.FromHtml(Request["CircleColorTo"]);
            } 
            if (!string.IsNullOrEmpty(Request["TextUnderLines"]))
                ci.TextUnderLines = (Request["TextUnderLines"] == "Y") ? true : false;

            if (TryParseInt32(Request["NumLines"], out i))
                ci.NumLines = i;

            if ((!string.IsNullOrEmpty(Request["LineColorFrom"]) && !string.IsNullOrEmpty(Request["LineColorTo"]))
                && (ValidateColor(Request["LineColorFrom"]) && ValidateColor(Request["LineColorTo"]))
                && (ColorSmaller(Request["LineColorFrom"], Request["LineColorTo"]))
                )
            {
                ci.LineColorFrom = System.Drawing.ColorTranslator.FromHtml(Request["LineColorFrom"]);
                ci.LineColorTo = System.Drawing.ColorTranslator.FromHtml(Request["LineColorTo"]);
            }

            if (TryParseInt32(Request["LeftOffset"], out i))
                ci.LeftOffset = i;

            if (TryParseInt32(Request["FontSize"], out i))
                ci.FontSize = i;

            if ((!string.IsNullOrEmpty(Request["FontColorFrom"]) && !string.IsNullOrEmpty(Request["FontColorTo"]))
                && (ValidateColor(Request["FontColorFrom"]) && ValidateColor(Request["FontColorTo"]))
                && (ColorSmaller(Request["FontColorFrom"], Request["FontColorTo"]))
                )
            {
                ci.FontColorFrom = System.Drawing.ColorTranslator.FromHtml(Request["FontColorFrom"]);
                ci.FontColorTo = System.Drawing.ColorTranslator.FromHtml(Request["FontColorTo"]);
            }

            if (TryParseInt32(Request["VertAngleOfDipFrom"], out i))
                ci.VertAngleOfDipFrom = i;

            if (TryParseInt32(Request["VertAngleOfDipTo"], out i))
                ci.VertAngleOfDipTo = i;

            if (TryParseInt32(Request["SymbolDistanceFrom"], out i))
                ci.SymbolDistanceFrom = i;

            if (TryParseInt32(Request["SymbolDistanceTo"], out i))
                ci.SymbolDistanceTo = i;

            if (!string.IsNullOrEmpty(Request["NonlinearDistortion"]))
                ci.NonlinearDistortion = Request["NonlinearDistortion"] == "Y" ? true : false;
            if (!string.IsNullOrEmpty(Request["BorderColor"]) && ValidateColor(Request["BorderColor"]))
                ci.BorderColor = System.Drawing.ColorTranslator.FromHtml(Request["BorderColor"]);
            if (!string.IsNullOrEmpty(Request["FontWhitelist"]))
                ci.FontWhitelist = Request["FontWhitelist"];
            if (!string.IsNullOrEmpty(Request["RandomTextChars"]))
                ci.TextChars = Request["RandomTextChars"];

            ci.RenderToStream(Response.OutputStream);
            Response.End();
        }
        else
        {
            BXPage.Scripts.RequireUtils();
            BXPage.RegisterScriptInclude("~/bitrix/js/Main/BXColorPicker.js");
            BXPage.RegisterThemeStyle("colorpicker.css");

            this.Preset.Items.Add(new ListItem(GetMessage("UserDefinedPreset"), "UserDefinedPreset"));
            this.Preset.Items.Add(new ListItem(GetMessage("Retro"), "Retro"));
            this.Preset.Items.Add(new ListItem(GetMessage("Waves"), "Waves"));
            this.Preset.Items.Add(new ListItem(GetMessage("Lines"), "Lines"));
            this.Preset.Items.Add(new ListItem(GetMessage("Negative"), "Negative"));
            this.Preset.Items.Add(new ListItem(GetMessage("Green"), "Green"));
            this.Preset.Items.Add(new ListItem(GetMessage("Warm"), "Warm"));
            this.Preset.Items.Add(new ListItem(GetMessage("Pencil"), "Pencil"));
            this.Preset.Items.Add(new ListItem(GetMessage("YellowPaint"), "YellowPaint"));
            this.Preset.Items.Add(new ListItem(GetMessage("Drops"), "Drops"));
            this.Preset.Items.Add(new ListItem(GetMessage("Soft"), "Soft"));

            for (int i = 0; i < InnerDefaultFonts.Length; i++)
                this.FontWhitelist.Items.Add(new ListItem(InnerDefaultFonts[i], InnerDefaultFonts[i]));

            //Library.Items.Add(new ListItem("Auto", BXCaptchaImageLibrary.Auto.ToString("D")));
            //Library.Items.Add(new ListItem("GD", BXCaptchaImageLibrary.GD.ToString("D")));
            //Library.Items.Add(new ListItem("GDI+", BXCaptchaImageLibrary.GDIPlus.ToString("D")));

            foreach (WebControl c in BXTabControlTab1.Controls)
                if (c.Attributes["DataType"] != null && c.Attributes["DataType"] == "color")
                    c.Attributes["onclick"] = string.Concat("Bitrix.UI.ColorPicker.Instantiate().Toggle(this, ", c.ClientID, "_OnPickColor, { Default : '", GetMessageJS("ByDefault"), "'});");

            this.Preset.Attributes.Add("onchange", "set_presets()");
            GetOptionsFromDB();
            MasterTitle = GetMessage("MasterTitle");
            Page.Title = GetMessage("PageTitle");
        }
    }
}
