using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MigrationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Old
            var con = new SqlConnection(@"Server=JAHANGIR\JR;Database=Sample;Integrated Security=true");
            using (con)
            {
                con.Open();

                var cmdCreateTable = new SqlCommand("if object_id('dbo.WipfliMigrationXMLData') is null create table WipfliMigrationXMLData(Id int identity primary key, CompanyYearEnd NVARCHAR(300), LoanServiceAllowed NVARCHAR(300), PlanType NVARCHAR(300), IRSPlanNumber NVARCHAR(300), HardshipsAllowed NVARCHAR(300), EffectiveDate NVARCHAR(300), RothDeferrals NVARCHAR(300), AutoDefApply NVARCHAR(300), AutoEnrollmentPerc NVARCHAR(300), AIIncrementPerc NVARCHAR(300), AIEndingPerc NVARCHAR(300));", con);
                cmdCreateTable.ExecuteNonQuery();



                string folderPath = @"H:\Office\XML_batch2";
                DirectoryInfo di = new DirectoryInfo(folderPath);
                FileInfo[] rgFiles = di.GetFiles("*.xml");
                DataTable dt = new DataTable();
                foreach (var item in rgFiles)
                {
                    string XMLFile = folderPath + "" + item;

                    var doc = XDocument.Load(item.FullName);

                    XElement element = doc.Root.Descendants("Plan_Common_Data").FirstOrDefault();

                    var rows = doc.Descendants("Plan_Common_Data").Select(el => new XMLValueModel
                    {
                        EmployerTaxableYear = el.Attribute("EmployerTaxableYear").Value ?? "",
                        PlansLoanPermittedToPolicy = el.Attribute("PlansLoanPermittedToPolicy").Value ?? "",
                        ContrType = el.Attribute("ContrType").Value ?? "",
                        PlanNumOther = el.Attribute("PlanNumOther").Value ?? "",
                        InServDistrHardship = el.Attribute("InServDistrHardship").Value ?? "",
                        InitialEffectiveDate = el.Attribute("InitialEffectiveDate").Value ?? "",
                        RothYes = el.Attribute("RothYes").Value ?? "",
                        AutoDefApply = el.Attribute("AutoDefApply").Value ?? "",
                        AutoDeferProvisionPercentage = el.Attribute("AutoDeferProvisionPercentage").Value ?? "",
                        AutoDeferProvIncreasepercPerYr = el.Attribute("AutoDeferProvIncreasepercPerYr").Value ?? "",
                        AutoDeferProvIncreasePerc1 = el.Attribute("AutoDeferProvIncreasePerc1").Value ?? "",
                    });

                    dt.Rows.Add(rows);
                }










                using(var bulkCopy = new SqlBulkCopy(con.ConnectionString, SqlBulkCopyOptions.KeepIdentity))
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, "");
                    }

                    bulkCopy.BulkCopyTimeout = 600;
                    bulkCopy.DestinationTableName = "dbo.WipfliMigrationXMLData";
                    bulkCopy.WriteToServer(dt);
                }








                //var cmdInsertXml = new SqlCommand("insert into t(doc) values (@doc);", con);
                //var pDoc = cmdInsertXml.Parameters.Add("@doc", System.Data.SqlDbType.Xml);

                //var doc = XDocument.Parse("<root><cn/><cn/><cn/></root>");
                //pDoc.Value = doc.CreateReader();

                //cmdInsertXml.ExecuteNonQuery();

                //var cmdRetrieveXml = new SqlCommand("select id, doc from t", con);
                //using (var rdr = cmdRetrieveXml.ExecuteReader())
                //{
                //    while (rdr.Read())
                //    {
                //        var xr = rdr.GetSqlXml(1);
                //        var rd = XDocument.Parse(xr.Value);
                //        Console.WriteLine(rd.ToString());

                //    }
                //}
            } 
            #endregion
        }
    }
}
