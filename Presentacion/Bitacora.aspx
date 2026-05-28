<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Bitacora.aspx.cs" Inherits="Presentacion.bITACORA" %>
<asp:Content 
    ID="Content1"
    ContentPlaceHolderID="ContentPlaceHolder1"
    runat="server">

    <!-- TODO TU CONTENIDO -->


        <div >
            Bitacora de Eventos
            <br />
            <div class="filtros-bitacora">
                <asp:Label runat="server" Text="Fecha Desde: "></asp:Label>
                <asp:Calendar runat="server" ID="fechaDesde"></asp:Calendar>
                <asp:Label runat="server" Text="Fecha Hasta: "></asp:Label>
                <asp:Calendar runat="server" ID="fechaHasta"></asp:Calendar>
                <asp:Label runat="server" Text="Usuario"></asp:Label>
                <asp:DropDownList runat="server" ID="ddlUsuarios"></asp:DropDownList>
                <asp:Label runat="server" Text="Modulo"></asp:Label>
                <asp:DropDownList runat="server" ID="ddlModulos"></asp:DropDownList>
                <asp:Label runat="server" Text="IP"></asp:Label>
                <asp:TextBox runat="server" ID="txtIP"></asp:TextBox>
                <asp:Button runat="server" Text="Filtrar" ID="btnFiltrar" />
                <asp:Button runat="server" Text="Limpiar Filtros" ID="btnLimpiar" />
            </div>

            <div class="datos-bitacora">
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