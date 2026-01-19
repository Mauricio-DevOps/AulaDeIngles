var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// ✅ Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Session (tem que ficar depois do UseRouting e antes do MapRazorPages)
app.UseSession();

// Redirect root to Login or Library depending on session
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        var auth = context.Session.GetString("auth");
        var dest = auth == "ok" ? "/Library" : "/Login";
        context.Response.Redirect(dest);
        return;
    }

    await next();
});

app.MapRazorPages();

app.Run();
