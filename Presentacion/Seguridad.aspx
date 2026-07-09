<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master"  AutoEventWireup="true" CodeBehind="Seguridad.aspx.cs" Inherits="Presentacion.Seguridad" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
        <div>
            <div class="grid-container seguridad">
                <h2>Seguridad</h2>

                <asp:Panel ID="pnlIntegridad" runat="server" CssClass="alerta-integridad" Visible="false">
                    <h3>Se detectaron problemas de integridad</h3>
                    <asp:BulletedList ID="lstMensajesIntegridad" runat="server" CssClass="lista-integridad"></asp:BulletedList>
                </asp:Panel>

                 <asp:Label CssClass="label-base" runat="server">Ruta del backup: </asp:Label>
                 <asp:TextBox ID="txtRutaBackup" CssClass="input-base" runat="server"></asp:TextBox>

                 <div class="acciones-seguridad">
                     <asp:Button ID="btnBackUp" CssClass="btn-base btn-success" runat="server" Text="Generar BackUp" OnClick="btnBackUp_Click"/>
                     <asp:Button ID="btnRestore" CssClass="btn-base btn-success" runat="server" Text="Restaurar BD" OnClick="btnRestore_Click"/>
                     <asp:Button ID="btnRecalcular" CssClass="btn-base btn-success" runat="server" Text="Recalcular DV" OnClick="btnRecalcular_Click"/>

                 </div>

                 <asp:Label ID="lblMensaje" runat="server" CssClass="label-base"></asp:Label>

            </div>



        </div>
    

</asp:Content>
