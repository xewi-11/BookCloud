using BookCloud.Data;
using BookCloud.Helpers;
using BookCloud.Hubs; // ✅ Agregar
using BookCloud.Repositories;
using BookCloud.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<FotoUsuario>();
builder.Services.AddSingleton<FolderHelper>();
builder.Services.AddSingleton<FotoLibro>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
});

builder.Services.AddTransient<IRepositoryUsuarios, RepositoryUsuarios>();
builder.Services.AddTransient<IRepositoryLibros, RepositoryLibros>();
builder.Services.AddTransient<IRepositoryWallet, RepositoryWallet>();
builder.Services.AddTransient<IRepositoryPedidos, RepositoryPedidos>();
builder.Services.AddScoped<IRepositoryPagos, RepositoryPagos>();
builder.Services.AddTransient<IRepositoryFavoritos, RepositoryFavoritos>();
builder.Services.AddTransient<IRepositoryChats, RepositoryChats>(); // ✅ Agregar

builder.Services.AddDbContext<BookCloudContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookCloud")));

// ✅ Agregar SignalR
builder.Services.AddSignalR();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

// ✅ Mapear el Hub de SignalR
app.MapHub<ChatHub>("/chatHub");

app.Run();
