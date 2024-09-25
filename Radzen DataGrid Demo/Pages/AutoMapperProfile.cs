using AutoMapper;

namespace Radzen_DataGrid_Demo.Pages
{
    public class OrdersProfile : Profile
    {
        public OrdersProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest =>
                        dest.OrderDetailsDto,
                    opt => opt.MapFrom(src => src.OrderDetails));

            CreateMap<OrderDto, Order>()
                .ForMember(dest =>
                        dest.OrderDetails,
                    opt => opt.MapFrom(src => src.OrderDetailsDto));

            CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(dest =>
                        dest.OrderDto,
                    opt => opt.MapFrom(src => src.Order));

            CreateMap<OrderDetailDto, OrderDetail>()
                .ForMember(dest =>
                        dest.Order,
                    opt => opt.MapFrom(src => src.OrderDto));
        }

    }
}
