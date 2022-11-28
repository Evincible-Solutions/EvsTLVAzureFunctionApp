using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

namespace EvsTLVfunctionApp
{
    public static class evsTLVs
    {
        [FunctionName("evsTLVs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string responseMessage = string.Empty;
            string Seller = string.Empty;
            string TaxNo = string.Empty;
            string dateTime = string.Empty;
            string Total = string.Empty;
            string Tax = string.Empty;

            Seller = req.Query["Seller"];
            TaxNo = req.Query["TaxNo"];
            dateTime = req.Query["dateTime"];
            Total = req.Query["Total"];
            Tax = req.Query["Tax"];           
            
            
            if (!String.IsNullOrWhiteSpace(Seller) && !String.IsNullOrWhiteSpace(TaxNo) && !String.IsNullOrWhiteSpace(dateTime) && !String.IsNullOrWhiteSpace(Total) && !String.IsNullOrWhiteSpace(Tax))
            {
                DateTime dt;
                double _Total;
                double _Tax;
                try
                {
                    dt = Convert.ToDateTime(dateTime);
                }
                catch (Exception ex)
                {
                    throw new Exception("Invalid Date Time ", ex);
                }
                try
                {
                    _Total = Convert.ToDouble(Total);
                }
                catch (Exception ex)
                {
                    throw new Exception("Invalid Total Amount ", ex);
                }
                try
                {
                    _Tax = Convert.ToDouble(Tax);
                }
                catch (Exception ex)
                {
                    throw new Exception("Invalid Tax Amount ", ex);
                }
                responseMessage = TLV.TLVs(Seller, TaxNo, dateTime, _Total, _Tax);
            }
            else
            {
                responseMessage = "Seller, VAT No, Datetime, Total Amount, Tax Amount is required";
            }

            return new OkObjectResult(responseMessage);
        }
    }

    public class TLV
    {
        static byte[] Seller;
        static byte[] VatNo;
        static byte[] dateTime;
        static byte[] Total;
        static byte[] Tax;

        public static string TLVs(String Seller, String TaxNo, string dateTime, Double Total, Double Tax)
        {
            TLV.Seller = Encoding.UTF8.GetBytes(Seller);
            TLV.VatNo = Encoding.UTF8.GetBytes(TaxNo);

            TLV.dateTime = Encoding.UTF8.GetBytes(dateTime.ToString());
            TLV.Total = Encoding.UTF8.GetBytes(Total.ToString());
            TLV.Tax = Encoding.UTF8.GetBytes(Tax.ToString());
            return ToBase64();
        }

        private String getasText(int Tag, byte[] Value)
        {
            return (Tag).ToString("X2") + (Value.Length).ToString("X2") + BitConverter.ToString(Value).Replace("-", string.Empty);
        }

        private static byte[] getBytes(int id, byte[] Value)
        {
            byte[] val = new byte[2 + Value.Length];
            val[0] = (byte)id;
            val[1] = (byte)Value.Length;
            Value.CopyTo(val, 2);
            return val;
        }

        private String getString()
        {
            String TLV_Text = "";
            TLV_Text += this.getasText(1, TLV.Seller);
            TLV_Text += this.getasText(2, TLV.VatNo);
            TLV_Text += this.getasText(3, TLV.dateTime);
            TLV_Text += this.getasText(4, TLV.Total);
            TLV_Text += this.getasText(5, TLV.Tax);
            return TLV_Text;
        }

        public override string ToString()
        {
            return this.getString();
        }

        private static String ToBase64()
        {
            List<byte> TLV_Bytes = new List<byte>();
            TLV_Bytes.AddRange(getBytes(1, TLV.Seller));
            TLV_Bytes.AddRange(getBytes(2, TLV.VatNo));
            TLV_Bytes.AddRange(getBytes(3, TLV.dateTime));
            TLV_Bytes.AddRange(getBytes(4, TLV.Total));
            TLV_Bytes.AddRange(getBytes(5, TLV.Tax));
            return Convert.ToBase64String(TLV_Bytes.ToArray());
        }
    }
}
