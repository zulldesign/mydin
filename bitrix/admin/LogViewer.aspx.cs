using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Security;

public partial class bitrix_admin_LogViewer : Bitrix.UI.BXAdminPage
{
	protected override void OnInit(EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

		MasterTitle = Encode(Title);

		string[] typeNames = Enum.GetNames(typeof(BXLogMessageType));
		for (int i = 0; i < typeNames.Length; i++)
		{
			type.Values.Add(new ListItem(GetMessageRaw("EventType." + typeNames[i]), i.ToString()));
		}
		base.OnInit(e);
	}

	protected void Grid_Select(object sender, BXSelectEventArgs e)
	{
		e.Data = new DataView(FillTable(e.PagingOptions, e.SortExpression));
	}

	protected BXSqlCommand MakeListCommand(BXFormFilter f, BXPagingOptions paging, string sortExp)
	{
		string pagingQuery = "select top {5} * from (select	(row_number() over ({3})) as __RowNumber__, {0} from {1} {2} ) _x_ where __RowNumber__ > {4} order by __RowNumber__";
		string nonPagingQuery = "select	{0} from {1} {2} {3}";

		string select = "*";
		string from = "b_Log";
		string where = string.Empty;
		string orderby = string.Empty;

		BXSqlCommand qcmd = new BXSqlCommand();

		if (f != null)
		{
			SqlDbType t;
			BXSqlFilterCollection fc = new BXSqlFilterCollection();
			foreach (BXFormFilterItem item in f)
			{
				string fn1 = item.filterName.ToLowerInvariant();

				switch (fn1)
				{
					case "type":
						t = SqlDbType.TinyInt;
						break;
					case "id":
						t = SqlDbType.Int;
						break;
					case "code":
						t = SqlDbType.Int;
						break;
					case "source":
						t = SqlDbType.NVarChar;
						break;
					case "occured":
						t = SqlDbType.DateTime;
						break;
					case "title":
						t = SqlDbType.NVarChar;
						break;
					case "message":
						t = SqlDbType.NVarChar;
						break;
					default:
						continue;
				}

				fc.Add(new BXSqlFilter(item.filterName, item.filterOperator, item.filterValue, t));
			}

			BXSqlFilterAnd commandTextWhereAnd = new BXSqlFilterAnd(fc);
			foreach (SqlParameter p in commandTextWhereAnd.FilterParameter)
				qcmd.Parameters.Add(p);

			string commandTextWhere = commandTextWhereAnd.FilterString;

			if (!String.IsNullOrEmpty(commandTextWhere))
				where = string.Format("WHERE {0}", commandTextWhere);
		}

		if (!String.IsNullOrEmpty(sortExp))
			orderby = string.Format("ORDER BY {0}", sortExp);


		if (paging != null && paging.startRowIndex >= 0 && paging.maximumRows > 0)
			qcmd.CommandText = string.Format(pagingQuery, select, from, where, orderby, paging.startRowIndex, paging.maximumRows);
		else
			qcmd.CommandText = string.Format(nonPagingQuery, select, from, where, orderby);

		return qcmd;
	}

	protected DataTable FillTable(BXPagingOptions pagingOpt, string sortExpression)
	{
		DataTable result = new DataTable();

		result.Columns.Add("num", typeof(int));
		result.Columns.Add("id", typeof(int));
		result.Columns.Add("type", typeof(int));
		result.Columns.Add("source", typeof(string));
		result.Columns.Add("code", typeof(int));
		result.Columns.Add("title", typeof(string));
		result.Columns.Add("message", typeof(string));
		result.Columns.Add("occured", typeof(DateTime));
		result.Columns.Add("isvisible", typeof(bool));

		string sortExp = sortExpression;
		if (String.IsNullOrEmpty(sortExp))
			sortExp = "id DESC";
		else
		{
			BXOrderBy_old sortOrder = BXDatabaseHelper.ConvertOrderBy(sortExpression);
			Dictionary<string, BXFieldDescription> fieldsd = new Dictionary<string, BXFieldDescription>();
			fieldsd.Add("id", new BXFieldDescription("id", SqlDbType.Int));
			fieldsd.Add("type", new BXFieldDescription("type", SqlDbType.TinyInt));
			fieldsd.Add("source", new BXFieldDescription("source", SqlDbType.NVarChar));
			fieldsd.Add("code", new BXFieldDescription("code", SqlDbType.Int));
			fieldsd.Add("occured", new BXFieldDescription("occured", SqlDbType.DateTime));
			sortExpression = BXDatabaseHelper.GetOrderBy(sortOrder, fieldsd);
		}

		BXFormFilter filter = Filter.CurrentFilter;

		using (BXSqlCommand cmd = MakeListCommand(filter, pagingOpt, sortExp))
		using (BXSqlDataReader reader = cmd.ExecuteReader())
			while (reader.Read())
			{
				DataRow row = result.NewRow();
				row["num"] = reader["__RowNumber__"];
				row["id"] = reader["id"];
				row["type"] = reader["type"];
				row["source"] = Regex.Replace(Encode(reader.ValueRaw<string>("source")), @"^[\s]*\(ServerName:[\s]*([^\)]*?)[\s]*\)", @"(<span style=""color:green"">$1</span>)", RegexOptions.Multiline).Replace("\n", "\n<br/>");
				row["code"] = reader["code"];
				row["title"] = reader.Value<string>("title");
				row["message"] = Regex.Replace(Encode(reader.ValueRaw<string>("message")), @"^[\s]*(\p{Lu}[\p{L}\s]*\:)", @"<b>$1</b>", RegexOptions.Multiline).Replace("\n", "\n<br/>").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
				row["occured"] = reader["occured"];
				row["isvisible"] = true;
				result.Rows.Add(row);
			}

		return result;
	}

	protected void Grid_SelectCount(object sender, Bitrix.UI.BXSelectCountEventArgs e)
	{
		string query = "select {0} from {1} {2} {3}";

		string select = "count(*)";
		string from = "b_Log";
		string where = string.Empty;
		string orderby = string.Empty;

		BXFormFilter f = Filter.CurrentFilter;

		using (BXSqlCommand qcmd = new BXSqlCommand())
		{
			if (f != null)
			{
				SqlDbType t;
				BXSqlFilterCollection fc = new BXSqlFilterCollection();
				foreach (BXFormFilterItem item in f)
				{
					string fn1 = item.filterName.ToLowerInvariant();

					switch (fn1)
					{
						case "type":
							t = SqlDbType.TinyInt;
							break;
						case "id":
							t = SqlDbType.Int;
							break;
						case "code":
							t = SqlDbType.Int;
							break;
						case "source":
							t = SqlDbType.NVarChar;
							break;
						case "occured":
							t = SqlDbType.DateTime;
							break;
						case "title":
							t = SqlDbType.NVarChar;
							break;
						case "message":
							t = SqlDbType.NVarChar;
							break;
						default:
							continue;
					}

					fc.Add(new BXSqlFilter(item.filterName, item.filterOperator, item.filterValue, t));
				}

				BXSqlFilterAnd commandTextWhereAnd = new BXSqlFilterAnd(fc);
				foreach (SqlParameter p in commandTextWhereAnd.FilterParameter)
					qcmd.Parameters.Add(p);

				string commandTextWhere = commandTextWhereAnd.FilterString;

				if (!String.IsNullOrEmpty(commandTextWhere))
					where = string.Format("WHERE {0}", commandTextWhere);
			}

			qcmd.CommandText = string.Format(query, select, from, where, orderby);
			e.Count = (int)qcmd.ExecuteScalar();
		}
	}
}
