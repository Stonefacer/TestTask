using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Ninject;

using LogParser;
using LogParser.LinesSource;
using GeoLocation;
using Database;

using HttpServerLogParser.Settings;

namespace HttpServerLogParser
{
    public class NinjectCommon
    {
        /// <summary>
        /// Maximal size of file can be stored in memory
        /// </summary>
        private const long MaximalFileSize = 100 * 1024 * 1024; // 100 MB

        private static NinjectCommon _instance;

        public static NinjectCommon Instance()
        {
            if(_instance == null)
            {
                _instance = new NinjectCommon();
            }
            return _instance;
        }

        private IKernel _kernel;

        public IKernel Kernel { get => _kernel; }

        private NinjectCommon()
        {
            _kernel = new StandardKernel();
            _kernel.Bind<ApplicationSettings>().ToSelf().InSingletonScope();
            _kernel.Bind<Worker>().ToSelf().InThreadScope();
        }

        /// <summary>
        /// Create bind for IDatabaseProvider interface
        /// </summary>
        /// <param name="connectionString">database connection string</param>
        public void BindDatabaseProvider(string connectionString)
        {
            _kernel.Bind<IDatabaseProvider>().ToMethod(x => new DatabaseProvider(connectionString, false)).InTransientScope(); // must be disposed manually
        }

        /// <summary>
        /// Create bind for ILinesSource will be used in parser
        /// </summary>
        /// <param name="fileInfo">file need to process</param>
        public void BindLinesSource(FileInfo fileInfo)
        {
            var fileSettings = Kernel.Get<FileSettings>();
            if (fileInfo.Length < MaximalFileSize)
            {
                _kernel.Bind<ILinesSource>().ToMethod(x => LinesSourceMemory.CreateFromFile(fileInfo)).InSingletonScope();
            }
            else
            {
                _kernel.Bind<ILinesSource>().ToMethod(x => new LinesSourceFile(fileInfo, fileSettings.FileReader.MaxBufferSize)).InSingletonScope();
            }
        }

        /// <summary>
        /// Bind FileSettings class and all dependencies
        /// </summary>
        public void BindFileSettings(FileSettings fileSettings)
        {
            _kernel.Bind<FileSettings>().ToConstant(fileSettings);
            _kernel.Bind<IParser>().To<Parser>().InTransientScope().WithConstructorArgument("skippibleExtensions", fileSettings.Parser.SkippibaleExtensions);
            _kernel.Bind<IGeoLocationFinder>().To<GeoLocationFreegeoip>().InSingletonScope()
                .WithConstructorArgument("freegeoipHostname", fileSettings.Geolocation.Server)
                .WithConstructorArgument("maxTriesCount", fileSettings.Geolocation.MaxTriesCount)
                .WithConstructorArgument("tryTimeout", fileSettings.Geolocation.TryTimeout)
                .WithConstructorArgument("maxCacheSize", fileSettings.Geolocation.MaxCacheSize);
        }
    }
}
