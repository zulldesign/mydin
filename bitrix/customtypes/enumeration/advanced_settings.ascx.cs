using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Bitrix;

using Bitrix.UI;
using Bitrix.Services;
using Bitrix.DataTypes;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.Services.Text;

public partial class BXCustomTypeEnumerationAdvancedSettings : BXControl, IBXCustomTypeAdvancedSetting
{
	//FIELDS
	private const int AdditionalRows = 3;
	protected List<EnumerationItem> state = new List<EnumerationItem>();
	protected bool noDefault;
	private BXCustomField field;
	private bool loadedOnInit;
	//private string validationGroup;
	private BXCustomFieldEnumCollection enums;


	//PROPERTIES
	private BXCustomFieldEnumCollection Enums
	{
		get
		{
			if (enums == null)
				enums = BXCustomFieldEnum.GetList(field.Id, field.FieldType, BXTextEncoder.EmptyTextEncoder);
			return enums;
		}
	}
	protected bool MultipleBehavior
	{
		get
		{
			return field == null || field.Multiple;
		}
	}
	protected string DefaultFormName
	{
		get
		{
			return UniqueID + "$def";
		}
	}
	protected string DeleteFormName
	{
		get
		{
			return UniqueID + "$del";
		}
	}
	protected string IdFormName
	{
		get
		{
			return UniqueID + "$id";
		}
	}
	protected string TitleFormPrefix
	{
		get
		{
			return UniqueID + "$title$";
		}
	}
	protected string XmlIdFormPrefix
	{
		get
		{
			return UniqueID + "$xmlid$";
		}
	}
	protected string SortFormPrefix
	{
		get
		{
			return UniqueID + "$sort$";
		}
	}
	public string ValidationGroup
	{
		get
		{
			return Validator.ValidationGroup;
		}
		set
		{
			Validator.ValidationGroup = value;
		}
	}

	//METHODS
	private void ReadState()
	{
		state.Clear();
		string[] ids = Request.Form.GetValues(IdFormName);
		if (ids == null)
			return;

		string[] defaults = Request.Form.GetValues(DefaultFormName);
		string[] deletes = Request.Form.GetValues(DeleteFormName);

		foreach (string id in ids)
		{
			EnumerationItem item = new EnumerationItem();
			item.IdValue = id;
			item.Title = Request.Form[TitleFormPrefix + id];
			item.XmlId = Request.Form[XmlIdFormPrefix + id];
			int sort;
			if (int.TryParse(Request.Form[SortFormPrefix + id], out sort))
				item.Sort = sort;
			item.IsDefault = (defaults != null && Array.IndexOf(defaults, id) != -1);
			if (!item.IsNew)
				item.Delete = (deletes != null && Array.IndexOf(deletes, id) != -1);
			state.Add(item);
		}
	}
	private void SetState(List<BXParamsBag<object>> state)
	{
		this.state.Clear();
		foreach (BXParamsBag<object> row in state)
			this.state.Add(new EnumerationItem(row));
	}
	protected void OptimizeState(bool addEmpty)
	{
		for (int i = state.Count - 1; i >= 0; i--)
			if (state[i].IsEmpty)
				state.RemoveAt(i);

		int count = 0;
		foreach (EnumerationItem item in state)
			if (item.IsNew)
				item.Id = count++;

		//Сбрасывание лишних Default 
		noDefault = true;
		foreach (EnumerationItem item in state)
			if (item.IsDefault)
			{
				if (!MultipleBehavior && !noDefault)
				{
					item.IsDefault = false;
					continue;
				}

				noDefault = false;
				if (MultipleBehavior)
					break;
			}

		if (addEmpty)
			for (int i = 0; i < AdditionalRows; i++)
				state.Add(new EnumerationItem(count++));
	}

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		Validator.ClientValidationFunction = ClientID + "_Validate";
	}
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		if (!loadedOnInit && IsPostBack)
			ReadState();
	}

	protected void Validator_ServerValidate(object sender, ServerValidateEventArgs e)
	{
		e.IsValid = true;
		
		foreach (EnumerationItem item in state)
		{
			bool valid = true;
			
			if (!item.IsNew && string.IsNullOrEmpty(item.Title) && !item.Delete)
				valid = false;
			
			if (item.IsNew && string.IsNullOrEmpty(item.Title) && !string.IsNullOrEmpty(item.XmlId))
				valid = false;
				
			item.IsValid = valid;
			
			if (!valid)
				e.IsValid = false;	
		}
	}


	#region IBXAdvancedSetting Implementation
	public void Initialize(BXCustomField currentField)
	{
		field = currentField;

		if (field != null)
		{
			state.Clear();
			foreach (BXCustomFieldEnum e in Enums)
				state.Add(new EnumerationItem(e));
		}

		// Because this happens in non-dialog editor on every hit
		// loadedOnInit = true;
	}
	public object GetSettings()
	{
		List<BXParamsBag<object>> data = new List<BXParamsBag<object>>();
		foreach (EnumerationItem item in state)
			if (!item.IsEmpty)
				data.Add(item.Store());
		return data;
	}
	public void SetSettings(object settings)
	{
		List<BXParamsBag<object>> data = settings as List<BXParamsBag<object>>;

		if (data == null)
			return;

		SetState(data);
		// Because this happens in dialog editor only on first hit
		loadedOnInit = true;
	}
	public void Save()
	{
		OptimizeState(false);
		BXCustomFieldEnumCollection delete = new BXCustomFieldEnumCollection();

		foreach (BXCustomFieldEnum e in Enums)
			foreach (EnumerationItem item in state)
				if (!item.IsNew && e.Id == item.Id)
				{
					if (item.Delete)
						e.Delete();
					else
					{
						e.Value = item.Title;
						e.XmlId = item.XmlId;
						e.Sort = item.Sort;
						e.Default = item.IsDefault;
						e.Save();
					}
					break;
				}

		state.RemoveAll(delegate(EnumerationItem item)
		{
			return !item.IsNew && item.Delete;
		});

		foreach (EnumerationItem item in state)
			if (item.IsNew)
			{
				BXCustomFieldEnum e = new BXCustomFieldEnum();
				e.FieldId = field.Id;
				e.FieldType = field.FieldType;
				e.Value = item.Title;
				e.XmlId = item.XmlId;
				e.Sort = item.Sort;
				e.Default = item.IsDefault;
				e.Save();

				item.Id = e.Id;
				item.IsNew = false;
			}
	}
	#endregion

	//NESTED CLASSES
	protected enum EnumerationMode
	{
		SingleSelection,
		MultipleSelection
	}
	protected class EnumerationItem
	{
		public int Id;
		public bool Delete;
		public string Title;
		public string XmlId;
		public bool IsDefault;
		public bool IsNew;
		public int Sort = 100;
		public bool IsValid = true;

		public bool IsEmpty
		{
			get
			{
				return IsNew && string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(XmlId);
			}
		}
		public string IdValue
		{
			get
			{
				return (IsNew ? "@" : string.Empty) + Id;
			}
			set
			{
				IsNew = false;
				if (value.Length > 0 && value[0] == '@')
				{
					value = value.Substring(1);
					IsNew = true;
				}

				if (!int.TryParse(value, out Id))
					IsNew = true;
			}
		}

		public EnumerationItem(BXParamsBag<object> data)
		{
			Id = data.GetInt("Id");
			Title = data.GetString("Title");
			XmlId = data.GetString("XmlId");
			IsDefault = data.GetBool("Default");
			IsNew = data.GetBool("New");
			Sort = data.GetInt("Sort");
			Delete = data.GetBool("Delete");
		}
		public EnumerationItem(BXCustomFieldEnum data)
		{
			Id = data.Id;
			Title = data.Value;
			XmlId = data.XmlId;
			IsDefault = data.Default;
			Sort = data.Sort;
		}
		public EnumerationItem()
		{

		}
		public EnumerationItem(int newId)
		{
			IsNew = true;
			Id = newId;
		}

		public BXParamsBag<object> Store()
		{
			BXParamsBag<object> data = new BXParamsBag<object>();
			data.Add("Id", Id);
			data.Add("Title", Title);
			data.Add("XmlId", XmlId);
			data.Add("Default", IsDefault);
			data.Add("Delete", Delete);
			data.Add("New", IsNew);
			data.Add("Sort", Sort);
			return data;
		}
	}
}
