using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Closure.Legacy;
using NSubstitute;
using Xunit;

namespace Closure.Tests
{
    public class LegacyConstructTests
    {
        readonly ITranslationsStore translationsStore;
        readonly ITranslationsDownloader translationsDownloader;
        readonly ILanguageServiceApi languageServiceApi;

        public LegacyConstructTests()
        {
            translationsStore = Substitute.For<ITranslationsStore>();
            translationsDownloader = Substitute.For<ITranslationsDownloader>();
            languageServiceApi = Substitute.For<ILanguageServiceApi>();
        } 

        [Fact]
        public void ManualConstruction()
        {
            var service = new LanguageService(translationsStore, translationsDownloader, languageServiceApi);
            var viewModel = new LanguageViewModel(service);
        }

        [Fact]
        public void AutoConstruction()
        {
            var container = new WindsorContainer();
            container
                .Register(Component.For<ITranslationsStore>().Instance(translationsStore))
                .Register(Component.For<ITranslationsDownloader>().Instance(translationsDownloader))
                .Register(Component.For<ILanguageServiceApi>().Instance(languageServiceApi))
                .Register(Component.For<ILanguageService, LanguageService>())
                .Register(Component.For<LanguageViewModel>())
                ;

            var viewModel = container.Resolve<LanguageViewModel>();
        }
    }
}
