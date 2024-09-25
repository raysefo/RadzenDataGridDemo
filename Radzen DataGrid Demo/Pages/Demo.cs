using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Radzen_DataGrid_Demo.Pages
{
    public interface IViewOrdersUseCase
    {
        IQueryable<Order?> Execute();
    }
    public class ViewOrdersUseCase : IViewOrdersUseCase
    {
        private readonly IOrderRepository _orderRepository;

        public ViewOrdersUseCase(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public IQueryable<Order?> Execute()
        {
            return _orderRepository.GetOrders();
        }
    }
    public interface IOrderRepository
    {
        IQueryable<Order?> GetOrders();
    }
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbContextFactory<IMSContext> _db;

        public OrderRepository(IDbContextFactory<IMSContext> db)
        { _db = db; }


        public IQueryable<Order?> GetOrders()
        {
            var ctx = _db.CreateDbContext();
            ctx.Database.EnsureCreated();
            IQueryable<Order> result = ctx.Orders
                .Include(d => d.OrderDetails) // Include OrderDetails without filtering here
                .Where(o => o.OrderDetails.Any(od => od.IsActive == 1)) // Filter orders with active OrderDetails
                .OrderByDescending(s => s.Id);

            return result;
        }



    }


    public interface IAddOrderDetailUseCase
    {
        Task<OrderDetail> ExecuteAsync(OrderDetail orderDetail);
    }
    public class AddOrderDetailUseCase : IAddOrderDetailUseCase
    {
        private readonly IOrderDetailRepository _orderDetailRepository;

        public AddOrderDetailUseCase(IOrderDetailRepository orderDetailRepository)
        {
            this._orderDetailRepository = orderDetailRepository;
        }
        public async Task<OrderDetail> ExecuteAsync(OrderDetail orderDetail)
        {
            return await _orderDetailRepository.AddOrderDetailAsync(orderDetail);
        }
    }
    public interface IOrderDetailRepository
    {
        Task<OrderDetail> AddOrderDetailAsync(OrderDetail orderDetail);

        Task<OrderDetail> UpdateOrderDetailAsync(OrderDetail orderDetail);

        Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderId(int orderId);
    }
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly IDbContextFactory<IMSContext> _db;

        public OrderDetailRepository(IDbContextFactory<IMSContext> db)
        {
            _db = db;
        }

        public async Task<OrderDetail> AddOrderDetailAsync(OrderDetail orderDetail)
        {
            await using var ctx = await _db.CreateDbContextAsync();
            await ctx.Database.EnsureCreatedAsync();
            if (ctx.OrdersDetail.Any(x =>
                    x.Id == orderDetail.Id)) return null;


            //Calculated Fields
            orderDetail.TotalBuyPrice = orderDetail.BuyUnitPrice * orderDetail.Quantity;
            orderDetail.TotalSellPrice = orderDetail.Quantity * orderDetail.SellUnitPrice;
            orderDetail.UnitCost = orderDetail.BuyUnitPrice * (orderDetail.CostRatio / 100) + orderDetail.BuyUnitPrice;
            orderDetail.TotalUnitCost = orderDetail.Quantity * orderDetail.UnitCost;
            orderDetail.IsActive = 1; //Active
            orderDetail.PaymentStatus = "Pending Payment";
            orderDetail.Status = "Order Opened";

            ctx.OrdersDetail.Add(orderDetail);
            await ctx.SaveChangesAsync();

            return orderDetail;
        }

        public async Task<OrderDetail> UpdateOrderDetailAsync(OrderDetail orderDetail)
        {
            await using var ctx = await _db.CreateDbContextAsync();
            await ctx.Database.EnsureCreatedAsync();
            //detail
            var detail = await ctx.OrdersDetail
                .Include(o => o.Order)
                .SingleAsync(x => x.Id == orderDetail.Id);

            //Order Statuses
            var statusCompleted = await ctx.OrdersDetail
                .Where(x => x.OrderId == orderDetail.OrderId && x.Id != orderDetail.Id)
                .AllAsync(c => c.Status == "Completed");
            var statusContinues = await ctx.OrdersDetail
                .Where(x => x.OrderId == orderDetail.OrderId && x.Id != orderDetail.Id)
                .AnyAsync(c => c.Status != "Completed");

            if (orderDetail != null && detail != null)
            {
                detail.Quantity = orderDetail.Quantity;
                detail.CostRatio = orderDetail.CostRatio;
                detail.Description = orderDetail.Description;
                detail.ProductCode = orderDetail.ProductCode;
                detail.ProductName = orderDetail.ProductName;
                detail.BuyUnitPrice = orderDetail.BuyUnitPrice;
                detail.ShippingNumber = orderDetail.ShippingNumber;
                detail.ShippingWeek = orderDetail.ShippingWeek;
                detail.Status = orderDetail.Status;
                detail.TotalBuyPrice = orderDetail.BuyUnitPrice * orderDetail.Quantity;
                detail.TotalSellPrice = orderDetail.Quantity * orderDetail.SellUnitPrice;
                detail.SellUnitPrice = orderDetail.SellUnitPrice;
                detail.UnitCost = orderDetail.BuyUnitPrice * (orderDetail.CostRatio / 100) + orderDetail.BuyUnitPrice;
                detail.TotalUnitCost = detail.UnitCost * orderDetail.Quantity;
                detail.Currency = orderDetail.Currency;
                detail.CustomerOrderNumber = orderDetail.CustomerOrderNumber;
                detail.CustomerStockCode = orderDetail.CustomerStockCode;
                detail.OrderId = orderDetail.OrderId;
                detail.TrackingNumber = orderDetail.TrackingNumber;
                detail.Warehouse = orderDetail.Warehouse;
                detail.PoNotes = orderDetail.PoNotes;
                detail.PaymentStatus = orderDetail.PaymentStatus;
                if (statusCompleted && orderDetail.Status == "Completed")
                    detail.Order.Status = "Completed";
                if (statusContinues || orderDetail.Status != "Completed")
                    detail.Order.Status = "Continues";


                if (detail.Status == "Completed")
                {
                    detail.CompletionDateTime = orderDetail.CompletionDateTime ?? DateTime.Now.Date;
                }
                else
                {
                    detail.CompletionDateTime = null;
                }

                ctx.OrdersDetail.Update(detail);
                await ctx.SaveChangesAsync();
            }

            return detail;
        }
        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderId(int orderId)
        {
            await using var ctx = await _db.CreateDbContextAsync();
            await ctx.Database.EnsureCreatedAsync();
            return await ctx.OrdersDetail
                .Where(x => x.OrderId == orderId && x.IsActive == 1)
                .Include(x => x.Order).ToListAsync();
        }
    }

    public interface IEditOrderDetailUseCase
    {
        Task<OrderDetail> ExecuteAsync(OrderDetail orderDetail);
    }
    public class EditOrderDetailUseCase : IEditOrderDetailUseCase
    {
        private readonly IOrderDetailRepository _orderDetailRepository;

        public EditOrderDetailUseCase(IOrderDetailRepository orderDetailRepository)
        {
            this._orderDetailRepository = orderDetailRepository;
        }

        public async Task<OrderDetail> ExecuteAsync(OrderDetail orderDetail)
        {
            return await _orderDetailRepository.UpdateOrderDetailAsync(orderDetail);
        }
    }


    public interface IViewOrderDetailsByOrderIdUseCase
    {
        Task<IEnumerable<OrderDetail>> ExecuteAsync(int orderId);
    }
    public class ViewOrderDetailsByOrderIdUseCase : IViewOrderDetailsByOrderIdUseCase
    {
        private readonly IOrderDetailRepository _orderDetailRepository;
        public ViewOrderDetailsByOrderIdUseCase(IOrderDetailRepository orderDetailRepository)
        {
            _orderDetailRepository = orderDetailRepository;
        }

        public async Task<IEnumerable<OrderDetail>> ExecuteAsync(int orderId)
        {
            return await _orderDetailRepository.GetOrderDetailsByOrderId(orderId);
        }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDateTime { get; set; }
        public int CustomerId { get; set; }
        public string Status { get; set; }
        public string DoneBy { get; set; }
        public List<OrderDetailDto> OrderDetailsDto { get; set; }


    }
    public class Order
    {

        public int Id { get; set; }

        [Required]
        public DateTime OrderDateTime { get; set; }
        [Required]
        [MaxLength(250)]
        public int CustomerId { get; set; }
        public string Status { get; set; }
        [MaxLength(50)]
        public string DoneBy { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }



    }
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public double BuyUnitPrice { get; set; }
        public double CostRatio { get; set; }
        public double UnitCost { get; set; }
        public double TotalBuyPrice { get; set; }
        public double? SellUnitPrice { get; set; }
        public double? TotalSellPrice { get; set; }
        public string? ShippingNumber { get; set; }
        public string? Status { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Description { get; set; }
        public string? Currency { get; set; }
        public string? CustomerStockCode { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public int IsActive { get; set; }
        public double? TotalUnitCost { get; set; }
        public int OrderId { get; set; }
        public int VendorId { get; set; }
        public string? Warehouse { get; set; }
        public string? PaymentStatus { get; set; }
        public OrderDto OrderDto { get; set; }
        public DateTime? CompletionDateTime { get; set; }
        public int? ShippingWeek { get; set; }
        public string? PoNotes { get; set; }
    }
    public class OrderDetail
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProductCode { get; set; }

        [MaxLength(250)]
        public string? ProductName { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public double BuyUnitPrice { get; set; }

        public double CostRatio { get; set; }
        public double UnitCost { get; set; }
        public double TotalBuyPrice { get; set; }
        public double? SellUnitPrice { get; set; }
        public double? TotalSellPrice { get; set; }

        [MaxLength(150)]
        public string? ShippingNumber { get; set; }

        public string? Status { get; set; }

        [MaxLength(150)]
        public string? TrackingNumber { get; set; }

        [MaxLength(400)]
        public string? Description { get; set; }

        public string? Currency { get; set; }
        public string? CustomerStockCode { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public int IsActive { get; set; }
        public double? TotalUnitCost { get; set; }
        public int OrderId { get; set; }
        public int VendorId { get; set; }
        public string? Warehouse { get; set; }
        public string? PaymentStatus { get; set; }
        public Order Order { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CompletionDateTime { get; set; }


        public int? ShippingWeek { get; set; }
        public string? PoNotes { get; set; }
    }
}
