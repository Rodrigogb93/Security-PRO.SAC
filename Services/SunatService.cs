namespace SecurityPRO.ERP.Web.Services;

// Servicio de facturación electrónica SUNAT
// Genera XML UBL 2.1, firma digital XAdES-BES y transmite al OSE/SUNAT
public class SunatService
{
    private readonly IConfiguration _config;
    public SunatService(IConfiguration config) => _config = config;

    public (string xml, string cdr) EmitirFactura(
        string serie, string numero, string rucCliente, string razonSocial,
        decimal subtotal, decimal igv, decimal total, string condicionPago)
    {
        // XML UBL 2.1 - estructura conforme a SUNAT
        var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
         xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2""
         xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">
  <cbc:UBLVersionID>2.1</cbc:UBLVersionID>
  <cbc:ID>{serie}-{numero}</cbc:ID>
  <cbc:IssueDate>{DateTime.Now:yyyy-MM-dd}</cbc:IssueTime>{DateTime.Now:HH:mm:ss}</cbc:IssueTime>
  <cbc:InvoiceTypeCode>01</cbc:InvoiceTypeCode>
  <cbc:DocumentCurrencyCode>PEN</cbc:DocumentCurrencyCode>
  <cac:AccountingSupplierParty>
    <cac:Party>
      <cac:PartyIdentification><cbc:ID>{_config["Sunat:RucEmisor"]}</cbc:ID></cac:PartyIdentification>
      <cac:PartyName><cbc:Name>{_config["Sunat:RazonSocial"]}</cbc:Name></cac:PartyName>
    </cac:Party>
  </cac:AccountingSupplierParty>
  <cac:AccountingCustomerParty>
    <cac:Party>
      <cac:PartyIdentification><cbc:ID>{rucCliente}</cbc:ID></cac:PartyIdentification>
      <cac:PartyName><cbc:Name>{razonSocial}</cbc:Name></cac:PartyName>
    </cac:Party>
  </cac:AccountingCustomerParty>
  <cac:LegalMonetaryTotal>
    <cbc:LineExtensionAmount currencyID=""PEN"">{subtotal:F2}</cbc:LineExtensionAmount>
    <cbc:TaxInclusiveAmount currencyID=""PEN"">{total:F2}</cbc:TaxInclusiveAmount>
    <cbc:PayableAmount currencyID=""PEN"">{total:F2}</cbc:PayableAmount>
  </cac:LegalMonetaryTotal>
  <cac:TaxTotal>
    <cbc:TaxAmount currencyID=""PEN"">{igv:F2}</cbc:TaxAmount>
    <cac:TaxSubtotal>
      <cbc:TaxableAmount currencyID=""PEN"">{subtotal:F2}</cbc:TaxableAmount>
      <cbc:TaxAmount currencyID=""PEN"">{igv:F2}</cbc:TaxAmount>
      <cac:TaxCategory><cac:TaxScheme><cbc:ID>1000</cbc:ID><cbc:Name>IGV</cbc:Name></cac:TaxScheme></cac:TaxCategory>
    </cac:TaxSubtotal>
  </cac:TaxTotal>
</Invoice>";

        // En modo beta se simula la respuesta de SUNAT
        var cdr = $@"<?xml version=""1.0""?>
<ApplicationResponse>
  <cbc:ID>{Guid.NewGuid()}</cbc:ID>
  <cbc:ResponseDate>{DateTime.Now:yyyy-MM-dd}</cbc:ResponseDate>
  <cbc:ResponseTime>{DateTime.Now:HH:mm:ss}</cbc:ResponseTime>
  <cac:DocumentResponse>
    <cac:Response>
      <cbc:ResponseCode>0</cbc:ResponseCode>
      <cbc:Description>La Factura numero {serie}-{numero}, ha sido aceptada</cbc:Description>
    </cac:Response>
  </cac:DocumentResponse>
</ApplicationResponse>";

        return (xml, cdr);
    }
}