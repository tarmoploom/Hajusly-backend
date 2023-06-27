using System.Text;
using Microsoft.EntityFrameworkCore;
using Hajusly;
using Hajusly.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var installTempData = true;
var useInMemoryDatabase = true;

if (useInMemoryDatabase)
{
    builder.Services.AddDbContext<AppDbContext>(OptionsBuilderConfigurationExtensions =>
        OptionsBuilderConfigurationExtensions.UseInMemoryDatabase("development"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opts =>
        opts.UseNpgsql(builder.Configuration.GetConnectionString("default")));
}

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder => {
    builder
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials()
    .AllowAnyMethod()
    .AllowAnyHeader();
}));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


var app = builder.Build();

using (var scope = ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
using (var context = scope.ServiceProvider.GetService<AppDbContext>()) {
    context?.Database.EnsureDeleted(); // reset db
    context?.Database.EnsureCreated();

    if (installTempData) {
        
        var env = ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var seeder = new DbSeeder(env, context!);
        seeder.SeedData<Student>("SampleData/Students.json");
        seeder.SeedData<Course>("SampleData/Courses1.json");
        seeder.SeedData<Module>("SampleData/Modules1.json");
        seeder.SeedData<StudentInCourse>("SampleData/StudentInCourse.json");
    }
    
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
