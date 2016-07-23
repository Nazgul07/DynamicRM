using Microsoft.VisualStudio.TestTools.UnitTesting;
using DynamicRM;
using System.Data.SqlClient;

namespace UnitTests
{
	[TestClass]
	public class DynamicRMTests
	{
		[TestMethod]
		public void TestCrudMethods()
		{
			DynamicSqlConnection context = new DynamicSqlConnection(new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Database=DynamicRM;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True"));

			dynamic entity = new Entity("Customers");
			entity.FirstName = "John";
			context.Insert(entity);

			//make sure it got inserted by querying
			entity = context.QuerySingle("SELECT * FROM CUSTOMERS");
			entity.EntityTableName = "Customers";
			entity.FirstName = "Doe";
			context.Update(entity);
			
			//query again to ensure it updated
			entity = context.QuerySingle("SELECT * FROM CUSTOMERS");
			Assert.AreEqual(entity.FirstName, "Doe");

			entity.EntityTableName = "Customers";
			context.Delete(entity);
		}
	}
}
