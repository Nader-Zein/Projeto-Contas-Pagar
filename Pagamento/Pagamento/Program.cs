using System.Globalization;
using Microsoft.Extensions.Configuration;
using Pagamento.DAO;
using System.Collections.Generic; 

namespace Pagamento
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            var defaultCulture = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            // Adiciona serviços ao container
            builder.Services.AddControllersWithViews();

            var configuration = builder.Configuration;


            builder.Services.AddScoped<CategoriaDAO>(sp => new CategoriaDAO(configuration));
            builder.Services.AddScoped<FormaPagamentoDAO>(sp => new FormaPagamentoDAO(configuration));
            builder.Services.AddScoped<ProdutoDAO>(sp => new ProdutoDAO(configuration));
            builder.Services.AddScoped<FornecedorDAO>(sp => new FornecedorDAO(configuration));
            builder.Services.AddScoped<CidadeDAO>(sp => new CidadeDAO(configuration));
            builder.Services.AddScoped<EstadoDAO>(sp => new EstadoDAO(configuration));

            builder.Services.AddScoped<ClienteDAO>(sp => new ClienteDAO(configuration));
            builder.Services.AddScoped<CondicaoPagamentoDAO>(sp => new CondicaoPagamentoDAO(configuration));
            builder.Services.AddScoped<ContaAPagarDAO>(sp => new ContaAPagarDAO(configuration));
            builder.Services.AddScoped<ContaAReceberDAO>(sp => new ContaAReceberDAO(configuration));
            builder.Services.AddScoped<FuncionarioDAO>(sp => new FuncionarioDAO(configuration));
            builder.Services.AddScoped<MarcaDAO>(sp => new MarcaDAO(configuration));
            builder.Services.AddScoped<PaisDAO>(sp => new PaisDAO(configuration));
            builder.Services.AddScoped<ParcelaCondicaoPagamentoDAO>(sp => new ParcelaCondicaoPagamentoDAO(configuration));
            builder.Services.AddScoped<UnidadeMedidaDAO>(sp => new UnidadeMedidaDAO(configuration));
            builder.Services.AddScoped<ProdutoFornecedorDAO>(sp => new ProdutoFornecedorDAO(configuration));

            
            builder.Services.AddScoped<CompraDAO>(sp => new CompraDAO(
                configuration,
                sp.GetRequiredService<ParcelaCondicaoPagamentoDAO>(),
                sp.GetRequiredService<ContaAPagarDAO>(),
                sp.GetRequiredService<CondicaoPagamentoDAO>(),
                sp.GetRequiredService<ProdutoDAO>(),
                sp.GetRequiredService<ProdutoFornecedorDAO>()));

            builder.Services.AddScoped<VendaDAO>(sp => new VendaDAO(
                configuration,
                sp.GetRequiredService<ProdutoDAO>(),
                sp.GetRequiredService<ParcelaCondicaoPagamentoDAO>(),
                sp.GetRequiredService<CondicaoPagamentoDAO>(),
                sp.GetRequiredService<ContaAReceberDAO>()));











            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
