using Application.Common;
using Application.Interfaces;
using Application.Mapping;
using Application.Services.Account;
using Application.Services.Number;
using Application.Services.OTP;
using Application.Services.PurchaseOrders;
using Application.Services.Signatures;
using Application.Services.SMS;
using Domain.Common;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NewProject;
using NewProject.Extensions;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// تكوين الخدمات
builder.Services
    .AddCustomLocalization()
    .AddCustomServices()
    .AddAutoMapper(typeof(UserProfile).Assembly)
    .AddCustomDbContext(builder.Configuration)
    .AddCustomIdentity()
    .AddHttpClient();

var app = builder.Build();

// تكوين خطوط HTTP
app.ConfigurePipeline();

// تشغيل التطبيق
app.Run();