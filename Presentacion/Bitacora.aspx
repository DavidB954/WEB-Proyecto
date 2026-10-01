<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Bitacora.aspx.cs" Inherits="Presentacion.bITACORA" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
        
    <div class="form-container bitacora">
        <h2 id="hBitacoraEventos" runat="server" data-i18n="1">
            Bitacora de Eventos
        </h2>


        <div class="section-card filtros-bitacora">

            <div class="campo-filtro">
                <asp:Label ID="lblFechaDesde" class="label-base" runat="server" Text="Fecha Desde" data-i18n="1"></asp:Label>
                <asp:TextBox runat="server" CssClass="input-base" ID="fechaDesde" TextMode="Date"></asp:TextBox>
            </div>
            <div class="campo-filtro">
                <asp:Label ID="lblFechaHasta" class="label-base" runat="server" Text="Fecha Hasta" data-i18n="1"></asp:Label>
                <asp:TextBox runat="server" CssClass="input-base" ID="fechaHasta" TextMode="Date"></asp:TextBox>
            </div>

            <div class="campo-filtro">
                <asp:Label ID="lblFiltroUsuario" class="label-base" runat="server" Text="Usuario" data-i18n="1"></asp:Label>
                <asp:DropDownList runat="server" ID="ddlUsuarios" CssClass="ddl-base">
                </asp:DropDownList>
            </div>

            <div class="campo-filtro">
                <asp:Label ID="lblFiltroModulo" class="label-base" runat="server" Text="Modulo" data-i18n="1"></asp:Label>
                <asp:DropDownList runat="server" ID="ddlModulos" CssClass="ddl-base">
                </asp:DropDownList>
            </div>

            <div class="campo-filtro">
                <asp:Label ID="lblFiltroCriticidad" class="label-base" runat="server" Text="Criticidad" data-i18n="1"></asp:Label>
                <asp:DropDownList runat="server" ID="ddlCriticidad" CssClass="ddl-base">
                </asp:DropDownList>
            </div>

            <div class="campo-filtro">
                <asp:Label ID="lblFiltroIP" class="label-base" runat="server" Text="IP" data-i18n="1"></asp:Label>
                <asp:TextBox runat="server" ID="txtIP" CssClass="input-base">
                </asp:TextBox>
            </div>

        </div>

        <div class="acciones-filtro">
            <asp:Button runat="server" Text="Filtrar" ID="btnFiltrar" CssClass="btn-base btn-primary" data-i18n="1" OnClick="btnFiltrar_Click" />
            <asp:Button runat="server" Text="Limpiar Filtros" ID="btnLimpiar" CssClass="btn-base btn-danger" data-i18n="1" OnClick="btnLimpiar_Click" />
        </div>
        <div class="grid-container datos-bitacora">
            <asp:Label ID="lblPaginas" runat="server" CssClass="info-paginas"></asp:Label>
            <asp:GridView ID="gvBitacora" runat="server" AutoGenerateColumns="false" CssClass="grid-crud"
                AllowPaging="true" PageSize="10" OnPageIndexChanging="gvBitacora_PageIndexChanging"
                OnRowDataBound="gvBitacora_RowDataBound">
                <PagerSettings Mode="NextPreviousFirstLast"
                    FirstPageText="« Primera" PreviousPageText="‹ Anterior"
                    NextPageText="Siguiente ›" LastPageText="Ultima »"
                    Position="Bottom" />
                <PagerStyle HorizontalAlign="Center" CssClass="grid-pager" />
                <Columns >
                    <asp:BoundField DataField="IdBitacora" HeaderText="ID" ReadOnly="True" Visible="false" />
                    <asp:BoundField DataField="IdUsuario" HeaderText="ID Usuario" />
                    <asp:BoundField DataField="FechaHora" HeaderText="Fecha y Hora" DataFormatString="{0:dd/MM/yyyy HH:mm:ss}" />
                    <asp:BoundField DataField="Accion" HeaderText="Accion" />
                    <asp:BoundField DataField="Criticidad" HeaderText="Criticidad" />
                    <asp:BoundField DataField="Modulo" HeaderText="Modulo" />
                    <asp:BoundField DataField="IP" HeaderText="IP" />
                    <asp:BoundField DataField="Descripcion" HeaderText="Descripcion" />
                    <asp:BoundField DataField="NombreMaquina" HeaderText="NombreMaquina" />
                </Columns>

            </asp:GridView>

        </div>

    </div>

</asp:Content>