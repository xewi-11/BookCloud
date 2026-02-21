using BookCloud.Data;
using BookCloud.Helpers;
using BookCloud.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<FotoUsuario>();
builder.Services.AddSession(options =>
{

    options.IdleTimeout = TimeSpan.FromMinutes(10);

});

builder.Services.AddTransient<RepositoryUsuarios>();

builder.Services.AddDbContext<BookCloudContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookCloud")));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
