using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;


namespace UI
{
    public partial class RoleList : System.Web.UI.UserControl, Bitrix.UI.IRoleList
    {
        protected void Page_Load(object sender, EventArgs e)
        {
			BXRoleCollection roleCollection = BXRoleManager.GetList(null, new Bitrix.DataLayer.BXOrderBy_old("RoleName", "Asc"));
			
			Roles.Items.Clear();
			foreach (BXRole r in roleCollection)
				Roles.Items.Add(new ListItem(r.Title, r.RoleName));

            if (selectedRoles == null)
                return;

            foreach (string role in selectedRoles)
            {
                ListItem li = Roles.Items.FindByValue(role);
                if (li != null)
                    li.Selected = true;
            }
        }

        public string[] selectedRoles;
        public string[] SelectedRoles
        {
            get
            {
                List<string> roles = new List<string>();

                foreach (ListItem li in Roles.Items)
                {
                    if (li.Selected)
                        roles.Add(li.Value);
                }
                return roles.ToArray();
            }

            set
            {
                selectedRoles = value;
                foreach (string role in selectedRoles)
                {
                    ListItem li = Roles.Items.FindByValue(role);
                    if (li != null)
                        li.Selected = true;
                }
            }
        }

        public string Str
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                SelectedRoles = BXStringUtility.StringToList(value);
            }
        }
    }

}