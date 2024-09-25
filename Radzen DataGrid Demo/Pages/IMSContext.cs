using Microsoft.EntityFrameworkCore;

namespace Radzen_DataGrid_Demo.Pages;

public class IMSContext : DbContext
{
    public IMSContext(DbContextOptions<IMSContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<OrderDetail> OrdersDetail { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data for Orders
        var orders = new List<Order>();
        for (int i = 1; i <= 10; i++)
        {
            orders.Add(new Order
            {
                Id = i,
                OrderDateTime = DateTime.Now.AddDays(-i),
                CustomerId = 1000 + i,
                Status = i % 2 == 0 ? "Completed" : "Pending",
                DoneBy = $"user{i % 3 + 1}"
            });
        }
        modelBuilder.Entity<Order>().HasData(orders);

        // Seed data for OrderDetails
        var orderDetails = new List<OrderDetail>();
        int detailId = 1;
        foreach (var order in orders)
        {
            for (int j = 1; j <= 3; j++)
            {
                orderDetails.Add(new OrderDetail
                {
                    Id = detailId++,
                    ProductCode = $"P{detailId:000}",
                    ProductName = $"Product {detailId}",
                    Quantity = j * 10,
                    BuyUnitPrice = 100 + j * 10,
                    CostRatio = 10 + j,
                    UnitCost = (100 + j * 10) * (10 + j) / 100 + (100 + j * 10),
                    TotalBuyPrice = (100 + j * 10) * j * 10,
                    SellUnitPrice = 150 + j * 10,
                    TotalSellPrice = (150 + j * 10) * j * 10,
                    ShippingNumber = $"SN{detailId:000}",
                    Status = j % 2 == 0 ? "Completed" : "Pending",
                    TrackingNumber = $"TN{detailId:000}",
                    Description = $"Description {detailId}",
                    Currency = "USD",
                    CustomerStockCode = $"CSC{detailId:000}",
                    CustomerOrderNumber = $"CON{detailId:000}",
                    IsActive = 1,
                    TotalUnitCost = ((100 + j * 10) * (10 + j) / 100 + (100 + j * 10)) * j * 10,
                    OrderId = order.Id,
                    VendorId = j,
                    Warehouse = $"Warehouse {j}",
                    PaymentStatus = "Pending Payment",
                    CompletionDateTime = j % 2 == 0 ? DateTime.Now.AddDays(-j) : null,
                    ShippingWeek = j,
                    PoNotes = $"PO Notes {detailId}"
                });
            }
        }
        modelBuilder.Entity<OrderDetail>().HasData(orderDetails);
    }
}
