using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using Unity;
using Unity.Mvc5;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Data.Infrastructure;
using Paralogamadha.Data.Repositories;
using Paralogamadha.Services;

namespace Paralogamadha.Web
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // 1. Register Unit of Work (The primary data access point)
            container.RegisterType<IUnitOfWork, UnitOfWork>();

            // 2. Register Core Services
            container.RegisterType<IAuthService, AuthService>();
            container.RegisterType<ITranslationService, TranslationService>();
            container.RegisterType<ISeoService, SeoService>();

            // 3. Register Phase 3 Specific Repositories (referenced in Phase3Repositories.cs)
            container.RegisterType<ISongsRepository, SongsRepository>();
            container.RegisterType<IVideosRepository, VideosRepository>();
            container.RegisterType<IVirtualTourRepository, VirtualTourRepository>();
            container.RegisterType<IHistoryRepository, HistoryRepository>();
            container.RegisterType<ILiveTVRepository, LiveTVRepository>();

            // 4. Register File Upload Service with Physical Path from Web.config
            string uploadPath = HttpContext.Current.Server.MapPath(
                WebConfigurationManager.AppSettings["UploadPath"] ?? "~/uploads");
            container.RegisterInstance<IFileUploadService>(new FileUploadService(uploadPath));

            // 5. Register Email Service with SMTP Settings
            // Note: Update these values with your actual provider credentials
            container.RegisterInstance<IEmailService>(new EmailService(
                host: "smtp.sendgrid.net",
                port: 587,
                user: "apikey",
                password: "YOUR_SENDGRID_API_KEY",
                fromEmail: "noreply@paralogamadha.org",
                fromName: "Paralogamadha Basilica"
            ));

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}