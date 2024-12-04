namespace WebApiProj.Models;

public class SalesOrder
{
    public int SalesOrderDetailID { get; set; }
    public int ProductID { get; set; }
    public string ProductCategory { get; set; } = string.Empty!;
    public string ProductName { get; set; } = string.Empty!;
    public string? Color { get; set; } = string.Empty!;
    public string? Size { get; set; } = string.Empty!;
    public decimal UnitPrice { get; set; }
    public decimal UnitPriceDiscount { get; set; }
    public Int16 OrderQty { get; set; }
    public decimal LineTotal { get; set; }
    public DateTime OrderDate { get; set; }
}