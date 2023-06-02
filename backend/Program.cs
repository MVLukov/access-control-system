using System.Net;
using System.Text.Json.Serialization;
using ACS.Hubs;
using ACS.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace ACS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
            builder.Services.AddDbContext<ACSDbContext>(options =>
            {
                options.UseMySql("Server=localhost;Database=acs_db;Uid=user;Pwd=User@123;Port=3307", new MySqlServerVersion(new Version(8, 0, 29)));
            });
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
            builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.Cookie.Name = "X-AUTH";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.MaxAge = options.ExpireTimeSpan;
                    options.LoginPath = "/Home/Login";
                    options.LogoutPath = "/Home/Logout";
                    options.SlidingExpiration = false;
                });
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // builder.Services.AddControllers().AddJsonOptions(x =>
            //     x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            using (var scope = app.Services.CreateScope())
            {
                var service = scope.ServiceProvider;
                var context = service.GetService<ACSDbContext>();

                if (context.Users.Any() == false)
                {
                    byte[] salt;
                    string hashedPassword = PasswordHelper.HashPassword("Admin@123", out salt);

                    context.Add(new User() { Username = "Admin", PasswordHash = hashedPassword, PasswordSalt = salt });
                    context.SaveChanges();
                }
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy(new CookiePolicyOptions() { MinimumSameSitePolicy = SameSiteMode.Strict });
            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    context.Items.Add("isAuthenticated", true);
                }

                await next(context);
            });

            app.MapHub<TagHub>("/tagHub");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}