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
using Bitrix;
using Bitrix.UI;
using Bitrix.Services;
using System.Collections.Generic;
using Bitrix.Configuration;


public partial class bitrix_ui_CustomFieldList : BXControl, IBXCustomFieldList
{
	//FIELDS
	List<IBXCustomTypeEdit> edits = new List<IBXCustomTypeEdit>();
    BXCustomPropertyCollection fieldsValue = new BXCustomPropertyCollection();
    private string entityId;
    private bool editMode;
	private bool hasItems;
	
	//METHODS
	protected void Page_Load(object sender, EventArgs e)
	{
		CustomFields.Rows[0].Visible = EditMode;

		BXCustomFieldCollection fields = BXCustomEntityManager.GetFields(EntityId);
		fields.RemoveAll(delegate (BXCustomField f)
		{
			return !f.EditInList;
		});
		hasItems = fields.Count > 0;
        foreach (BXCustomField field in fields)
		{
			
			HtmlTableRow newRow = new HtmlTableRow();

			HtmlTableCell leftCell = new HtmlTableCell();
			leftCell.Attributes.Add("class", "field-name");
			leftCell.VAlign = "top";
			leftCell.Align = "right";
			leftCell.Width = "50%";

			if (field.Mandatory)
			{
				Literal required = new Literal();
				required.Text = @"<span class=""required"">*</span>";
				leftCell.Controls.Add(required);
			}

            if (EditMode)
            {
                HyperLink link = new HyperLink();
                link.Text = field.EditFormLabel;
                link.NavigateUrl = string.Format("~/bitrix/admin/CustomFieldEdit.aspx?id={0}&{1}={2}", field.Id.ToString(), BXConfigurationUtility.Constants.BackUrl, HttpUtility.UrlEncode(Request.RawUrl));
                leftCell.Controls.Add(link);
            }
            else
            {
                Label lbName = new Label();
                lbName.Text = field.EditFormLabel;
                leftCell.Controls.Add(lbName);
            }

			Label lb = new Label();
			lb.Text = ":";
			leftCell.Controls.Add(lb);
			newRow.Cells.Add(leftCell);

			HtmlTableCell rightCell = new HtmlTableCell();

			BXCustomType ct = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
			IBXCustomTypeEdit editor = null;
			if (field.EditInList)
				try
				{
					if (field.Multiple && ct.IsClonable)
					{
						Control mul = LoadControl("CustomFieldMultiplicator.ascx");
						editor = mul as IBXCustomTypeEdit;
						mul.ID = "MUL_" + field.Id.ToString();

						if (ScriptManager.GetCurrent(Page) != null)
						{
							UpdatePanel panel = new UpdatePanel();
							panel.UpdateMode = UpdatePanelUpdateMode.Conditional;
							panel.ContentTemplateContainer.Controls.Add(mul);
							rightCell.Controls.Add(panel);
						}
						else
							rightCell.Controls.Add(mul);
					}
					else
					{
						Control ed = ct.Edit;
						editor = ed as IBXCustomTypeEdit;
						rightCell.Controls.Add(ed);
					}
				}
				catch
				{
				}

			newRow.Cells.Add(rightCell);
			CustomFields.Rows.Add(newRow);

			if (editor != null)
			{
				editor.Initialize(field,fieldsValue[field.Name]);
				editor.ValidationGroup = ValidationGroup;
				edits.Add(editor);
			}
		}
	}

    //PROPERTIES
    public string EntityId
    {
        get { return entityId; }
        set { entityId = value; }
    }

    public bool EditMode
    {
        get { return editMode; }
        set { editMode = value; }
    }

	public bool HasItems
	{
		get
		{
			return hasItems;
		}
	}

	#region IBXCustomFieldList Members
	
	private string validationGroup;
	public string ValidationGroup
	{
		get
		{
			return validationGroup;
		}
		set
		{
			validationGroup = value;
		}
	}

    public BXCustomPropertyCollection Save()
    {
        BXCustomPropertyCollection result = new BXCustomPropertyCollection();
        foreach (IBXCustomTypeEdit edit in edits)
            edit.Save(result);
        return result;
    }

    public new void Load(BXCustomPropertyCollection properties)
    {
        fieldsValue = properties;
    }
    #endregion
}
