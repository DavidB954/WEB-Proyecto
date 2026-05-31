<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Bitacora.aspx.cs" Inherits="Presentacion.bITACORA" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
        
    <div class="form-container bitacora">
        <h2>
            Bitacora de Eventos
        </h2>
        
        
        <div class="section-card filtros-bitacora">

            <div class="campo-filtro">
                <asp:Label class="label-base" runat="server" Text="Fecha Desde"></asp:Label>
                <asp:TextBox runat="server" CssClass="input-base" ID="fechaDesde" TextMode="Date"></asp:TextBox>
            </div>
            <div class="campo-filtro">
                <asp:Label class="label-base" runat="server" Text="Fecha Hasta"></asp:Label>
                <asp:TextBox runat="server" CssClass="input-base" ID="fechaHasta" TextMode="Date"></asp:TextBox>
            </div>
            
            <div class="campo-filtro">
                <asp:Label class="label-base" runat="server" Text="Usuario"></asp:Label>
                <asp:DropDownList runat="server" ID="ddlUsuarios" CssClass="ddl-base">
                </asp:DropDownList>
            </div>
            
            <div class="campo-filtro">
                <asp:Label class="label-base" runat="server" Text="Módulo"></asp:Label>
                <asp:DropDownList runat="server" ID="ddlModulos" CssClass="ddl-base">
                </asp:DropDownList>
            </div>

            <div class="campo-filtro">
                <asp:Label class="label-base" runat="server" Text="IP"></asp:Label>
                <asp:TextBox runat="server" ID="txtIP" CssClass="input-base">
                </asp:TextBox>
            </div>

        </div>
        
        <div class="acciones-filtro">
            <asp:Button runat="server" Text="Filtrar" ID="btnFiltrar" CssClass="btn-base btn-primary" />
            <asp:Button runat="server" Text="Limpiar Filtros" ID="btnLimpiar" CssClass="btn-base btn-danger" />
        </div>
        <div class="grid-container datos-bitacora">
            <asp:GridView ID="gvBitacora" runat="server" AutoGenerateColumns="false"  CssClass="grid-crud">
                <Columns >
                    <asp:BoundField DataField="IdBitacora" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="IdUsuario" HeaderText="ID Usuario" />
                    <asp:BoundField DataField="FechaHora" HeaderText="Fecha y Hora" DataFormatString="{0:dd/MM/yyyy HH:mm:ss}" />
                    <asp:BoundField DataField="Accion" HeaderText="Accion" />
                    <asp:BoundField DataField="Modulo" HeaderText="Módulo" />
                    <asp:BoundField DataField="IP" HeaderText="IP" />
                    <asp:BoundField DataField="Descripcion" HeaderText="Descripcion" />
                    <asp:BoundField DataField="NombreMaquina" HeaderText="NombreMaquina" />
                </Columns>

            </asp:GridView>

        </div>

    </div>

</asp:Content>