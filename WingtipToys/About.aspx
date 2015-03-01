<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="WingtipToys.About" Title="About" MasterPageFile="~/Site.Master" %>
<%@ Register assembly="Navigation" namespace="Navigation" tagprefix="nav" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
        <h2>Your app description page.</h2>
    </hgroup>

    <article>
        <p>        
            Use this area to provide additional information.
        </p>

        <p>        
            Use this area to provide additional information.
        </p>

        <p>        
            Use this area to provide additional information.
        </p>
    </article>

    <aside>
        <h3>Aside Title</h3>
        <p>        
            Use this area to provide additional information.
        </p>
        <ul>
            <li><nav:NavigationHyperLink runat="server" Action="Home" Text="Home" /></li>
            <li><nav:NavigationHyperLink runat="server" Action="About" Text="About" /></li>
            <li><nav:NavigationHyperLink runat="server" Action="Contact" Text="Contact" /></li>
        </ul>
    </aside>
</asp:Content>