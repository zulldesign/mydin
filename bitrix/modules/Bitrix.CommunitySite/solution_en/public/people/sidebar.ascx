<%@ Control Language="C#" %>

<bx:IncludeComponent id="NewBoys" 
	runat="server" 
	componentname="bitrix:user.list" 
	template="newboys" 
	UserProfileUrlTemplate="<%$ Options:Bitrix.CommunitySite:UserProfileUrlTemplate %>" 
	FilterByGender="None" 
	FilterByMonthOfBirth="" 
	FilterByDayOfBirth="" 
	SortBy="CreationDate" 
	SortDirection="Desc" 
	ProhibitedUserIds="" 
	PagingAllow="False" 
	PagingRecordsPerPage="10" 
	CacheMode="Auto" 
	CacheDuration="3600" />

<bx:IncludeComponent 
	id="SideBar" 
	runat="server" 
	componentname="bitrix:includeArea"  
	template=".default" 
	Mode="File" 
	FilePath="<%$ Options:Bitrix.CommunitySite:SiteFolder:{0}sidebar.ascx %>" 
	AllowEditing="True" 
/>