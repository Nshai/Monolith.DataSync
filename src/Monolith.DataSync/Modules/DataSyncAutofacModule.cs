using System.Reflection;
using Autofac;
using IntelliFlo.Platform.AutoMapper;
using Microservice.DataSync.Utilities.TimeZone;
using NodaTime;
using NodaTime.TimeZones;
using Module = Autofac.Module;

namespace Microservice.DataSync.Modules
{
    public class DataSyncAutofacModule : Module
    {
        private readonly object[] lifeTimeScopeTags;

        public DataSyncAutofacModule(params object[] lifeTimeScopeTags)
        {
            this.lifeTimeScopeTags = lifeTimeScopeTags;
        }

        protected override void Load(ContainerBuilder builder)
        {
           
            // Register auto-mappers
            // TODO - isnt this called automatically by some other container?
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
             .Where(t => t.Name.EndsWith("AutoMapperModule"))
             .As<IModule>()
             .SingleInstance();

            //Register Timezone infrastructure
            builder.RegisterType<TimeZoneConverter>().As<ITimeZoneConverter>().InstancePerMatchingLifetimeScope(lifeTimeScopeTags);
            builder.Register(c => BuildDateTimeZoneProvider()).As<IDateTimeZoneProvider>().InstancePerMatchingLifetimeScope(lifeTimeScopeTags);
        }

        public static IDateTimeZoneProvider BuildDateTimeZoneProvider()
        {
            IDateTimeZoneProvider provider;
            var assembly = Assembly.GetAssembly(typeof(TimeZoneConverter));
            using (var stream = assembly
                .GetManifestResourceStream($"{typeof(TimeZoneConverter).Namespace}.timezoneinfo.nzd"))
            {
                var source = TzdbDateTimeZoneSource.FromStream(stream);
                provider = new DateTimeZoneCache(source);
            }

            return provider;
        }
    }
}
