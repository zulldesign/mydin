<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SqlStatistics.aspx.cs" Inherits="bitrix_dialogs_SqlStatistics" %>
<html >
<head runat="server">
</head>
<body>
    <form id="form1" runat="server">
     <bx:BXPageAsDialogBehaviour runat="server" ID="Behaviour" />
    <div >
		<div style="height: 200px; overflow: auto; border-bottom: dashed 1px black; padding-bottom: 5px">
			<table id="BXSqlStatisticsList" style="width:100%"></table>
		</div>
		<div style="height: 300px; padding: 5px" id="BXSqlStatisticsDetail">
		</div>
		<script type="text/javascript">
			window.setTimeout(function()
			{
				var data = window["<%= JSEncode(Request.QueryString["data"]) %>"];
				var table = document.getElementById("BXSqlStatisticsList");
				var detail =  document.getElementById("BXSqlStatisticsDetail");
				var desc = document.getElementById("BXSqlStatisticsDescription");
				
				desc.parentNode.innerHTML = data.description;
				
				for(var i = 0; i < data.items.length; i++)
				{
					var row = table.insertRow(-1);
					row.dataId = i;
					row.onclick = function()
					{
						detail.innerHTML = data.items[this.dataId].detail;
						return false;
					};
					
					var cell;
					
					cell = row.insertCell(-1);
					cell.innerHTML = i + 1;
					
					cell = row.insertCell(-1);
					var anchor = document.createElement('A');
					anchor.href = 'javascript:void(0);';
					anchor.dataId = i;
					anchor.innerHTML = data.items[i].title;
					cell.appendChild(anchor);
					
					cell = row.insertCell(-1);
					cell.innerHTML = data.items[i].comment;
					
					cell = row.insertCell(-1);
					cell.innerHTML = data.items[i].time;
				}		
			}, 0);
		</script>
    </div>
    </form>
</body>
</html>
