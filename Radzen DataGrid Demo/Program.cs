using Microsoft.EntityFrameworkCore;
using Radzen_DataGrid_Demo.Pages;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure DbContextFactory to use InMemoryDatabase
builder.Services.AddDbContextFactory<IMSContext>(options =>
    options.UseInMemoryDatabase("InMemoryDb"));

// Register repositories and use cases
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddScoped<IViewOrdersUseCase, ViewOrdersUseCase>();
builder.Services.AddScoped<IAddOrderDetailUseCase, AddOrderDetailUseCase>();
builder.Services.AddScoped<IEditOrderDetailUseCase, EditOrderDetailUseCase>();
builder.Services.AddScoped<IViewOrderDetailsByOrderIdUseCase, ViewOrderDetailsByOrderIdUseCase>();

// Auto Mapper Configurations
builder.Services
    .AddAutoMapper(typeof(Program).Assembly);

//Aspnetcore_Environment
builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

var scope = app.Services.CreateScope();

var imsContext = scope.ServiceProvider.GetRequiredService<IMSContext>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
