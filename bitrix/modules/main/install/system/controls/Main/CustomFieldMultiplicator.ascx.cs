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
using System.Collections.Generic;
using Bitrix.Services;
using Bitrix.Configuration;


public partial class bitrix_ui_CustomFieldMultiplicator : BXControl, INamingContainer, IBXCustomTypeEdit
{
    //FIELDS
    private BXCustomField field;
    private BXCustomProperty value;
	private BXCustomType type;
	private int quantity = 0;

	private string QuantityId
	{
		get
		{
			return UniqueID + "$q";
		}
	}
	public int Quantity
	{
		get
		{
			if (quantity == 0)
				if (Request[QuantityId] != null)
					quantity = int.Parse(Request[QuantityId]);
				else
					if (value != null && value.Values.Count != 0)
						quantity = value.Values.Count;
					else
						quantity = 1;

			return (quantity > BXConfigurationUtility.Constants.MaxFieldQuantity) ? BXConfigurationUtility.Constants.MaxFieldQuantity : quantity;
		}
		set
		{
			quantity = value;
		}
	}
	public BXCustomField Field
	{
		get
		{
			return field;
		}
		set
		{
			field = value;
		}
	}

	protected override void OnPreRender(EventArgs e)
	{
		ScriptManager.RegisterHiddenField(Page, QuantityId, Quantity.ToString());

		int visible = 0;
		foreach (Container c in Editors.Controls)
			if (c.ContentVisible)
				visible++;

		foreach (Container c in Editors.Controls)
			c.DeleteButton.Visible = visible > 1;

		base.OnPreRender(e);
	}
    void imgBtn_Click(object sender, ImageClickEventArgs e)
    {
        ImageButton imgBtn = (ImageButton)sender;

		Container toHide = null;
		int visible = 0;

		foreach (Container c in Editors.Controls)
		{
			if (c.DeleteButton == imgBtn)
				toHide = c;
			if (c.ContentVisible)
				visible++;
		}

		if (toHide == null)
			throw new InvalidOperationException("Unknown delete button operation is not available");

		if (!toHide.ContentVisible)
			throw new InvalidOperationException("Delete operation is not available");

		if (visible < 2)
			throw new InvalidOperationException("Can't delete last element");

		toHide.ContentVisible = false;
    }
    protected void Button1_Click(object sender, EventArgs e)
    {
		if (Quantity >= BXConfigurationUtility.Constants.MaxFieldQuantity)
			return;
		AddNew(null, Quantity);
		Quantity++;
    }

	private void Bind()
	{
		//edits.Clear();
		for (int i = 0; i < Quantity; i++)
		{
			BXCustomProperty itemValue = null;
			if (value != null && i < value.Values.Count)
				itemValue = new BXCustomProperty(value.Name, value.FieldId, value.DbType, false, value.Values[i], type.IsFile);

			AddNew(itemValue, i);
		}
	}
	void AddNew(BXCustomProperty value, int index)
	{
		Container c = new Container(type, field, value);
		c.ID = "c" + index;
		c.DeleteButton.Click += imgBtn_Click;
		c.ValidationGroup = ValidationGroup;

		Editors.Controls.Add(c);
	}

    #region IBXCustomTypeEdit Members
    private string validationGroup = String.Empty;
    public string ValidationGroup
    {
        get
        {
            return validationGroup;
        }
        set
        {
            validationGroup = value;
            foreach (Container c in Editors.Controls)
                c.ValidationGroup = value;
        }
    }

    public void Initialize(BXCustomField currentField, BXCustomProperty currentValue)
    {
        field = currentField;
        value = currentValue;
		type = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
		Bind();
    }

    public void Save(BXCustomPropertyCollection storage)
    {
        if (field == null)
            return;

        List<object> values = new List<object>();

        BXCustomPropertyCollection tempStorage = new BXCustomPropertyCollection();
		bool allOriginal = true;
        for(int i = 0; i < Editors.Controls.Count; i++)
        {
			Container c = (Container)Editors.Controls[i];
						
			object originalValue = (value != null && i < value.Values.Count) ? value.Values[i] : null;
			if (!c.ContentVisible)
			{
				if (originalValue != null)
					allOriginal = false;
				continue;
			}

			tempStorage.Clear();
            c.Editor.Save(tempStorage);
			if (tempStorage.ContainsKey(field.Name))
			{
				values.AddRange(tempStorage[field.Name].Values);
				allOriginal = false;
			}
			else
				values.Add(originalValue);
        }

		if (allOriginal)
			return;

		List<object> filteredValues = new List<object>();
		foreach (object value in values)
		{
			if (value != null && !filteredValues.Contains(value))
				filteredValues.Add(value);
		}

		storage[field.Name] = new BXCustomProperty(field.Name, field.Id, type.DbType, true, filteredValues.ToArray(), type.IsFile);
    }
    #endregion

	//NESTED CLASSES
	class Container : Control, INamingContainer
	{
		bool? visible;
		ImageButton deleteButton;
		IBXCustomTypeEdit editor;
		Control editorRaw;
		
		public Container(BXCustomType type, BXCustomField field, BXCustomProperty value)
		{
			editorRaw = type.Edit;
			editorRaw.ID = string.Format("ed");
			editor = (IBXCustomTypeEdit)editorRaw;
			editor.Initialize(field, value);
			Controls.Add(editorRaw);

			deleteButton = new ImageButton();
			deleteButton.ID = string.Format("bt");
			deleteButton.ImageUrl =  VirtualPathUtility.ToAbsolute("~/bitrix/images/delete_button.gif");
			deleteButton.ImageAlign = ImageAlign.AbsMiddle;
			deleteButton.CausesValidation = false;
			Controls.Add(deleteButton);
	
			Literal br = new Literal();
			br.Text = "<br/>";
			Controls.Add(br);
		}
		
		public bool ContentVisible
		{
			get
			{
				if (!visible.HasValue)
					ContentVisible = string.IsNullOrEmpty(Page.Request.Form[UniqueID]);
				return visible.Value;
			}
			set
			{
				visible = value;
				editorRaw.Visible = value;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (!visible.HasValue)
				ContentVisible = string.IsNullOrEmpty(Page.Request.Form[UniqueID]);
		}

		public ImageButton DeleteButton
		{
			get
			{
				return deleteButton;
			}
		}
		public IBXCustomTypeEdit Editor
		{
			get
			{
				return editor;
			}
		}
		public string ValidationGroup
		{
			get
			{
				return editor.ValidationGroup;
			}
			set
			{
				editor.ValidationGroup = value;
			}
		}
		protected override void Render(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
			writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
			if (!ContentVisible)
				writer.AddAttribute(HtmlTextWriterAttribute.Value, "N");
			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();
			
			if (ContentVisible)
				base.Render(writer);
		}
	}
}
