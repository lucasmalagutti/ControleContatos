using ControleDeContatos.Data;
using ControleDeContatos.Helper;
using ControleDeContatos.Repositorio;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // Iniciar o serviço de banco de dados
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<BancoContent>();
                context.Database.Migrate(); // Aplicar migrações pendentes, se houver
            }
            catch (Exception ex)
            {
                // Lidar com erros de migração ou inicialização do banco de dados
                Console.WriteLine("Erro ao iniciar o banco de dados: " + ex.Message);
            }
        }

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Program>();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<BancoContent>(options =>
                    options.UseSqlServer(context.Configuration.GetConnectionString("DataBase")));

                services.AddControllersWithViews();

                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                services.AddScoped<ISessao, Sessao>();
                services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
                services.AddScoped<IContatoRepositorio, ContatoRepositorio>();
                services.AddScoped<IEmail, Email>();

                services.AddSession(o =>
                {
                    o.Cookie.HttpOnly = true;
                    o.Cookie.IsEssential = true;
                });
            });

    // Configure method for the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession(); // Adicionando o middleware de sessão

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}");
        });
    }
}
