using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Closure.Legacy;

namespace Closure.CommandsQueries
{
    public interface ITranslationsCache
    {
        IDictionary<string, string> Get();
        void Set(IDictionary<string, string> translations);
    }

    public interface IGetLanguagesQuery
    {
        Task<ICollection<Language>> ExecuteAsync();
    }

    public interface IGetTranslationQuery
    {
        string Execute(string key);
    }

    public interface IChangeLanguageCommand
    {
        Task ExecuteAsync(Language language);
    }

    public interface ILoadTranslationsCommand
    {
        Task ExecuteAsync();
    }

    public interface IRefreshTranslationsCommand
    {
        Task ExecuteAsync();
    }

    public class LoadTranslationsCommand : ILoadTranslationsCommand
    {
        private readonly ITranslationsStore translationsStore;

        public LoadTranslationsCommand(ITranslationsStore translationsStore)
        {
            this.translationsStore = translationsStore;
        }

        public async Task ExecuteAsync()
        {
            await translationsStore.GetTranslationsAsync();
        }
    }

    public class RefreshTranslationsCommand : IRefreshTranslationsCommand
    {
        private readonly ITranslationsCache translationsCache;
        private readonly ITranslationsStore translationsStore;
        private readonly ITranslationsDownloader translationsDownloader;


        public RefreshTranslationsCommand(ITranslationsCache translationsCache, ITranslationsStore translationsStore,
            ITranslationsDownloader translationsDownloader)
        {
            this.translationsCache = translationsCache;
            this.translationsStore = translationsStore;
            this.translationsDownloader = translationsDownloader;
        }

        public async Task ExecuteAsync()
        {
            var translations = await translationsDownloader.DownloadTranslationsAsync();
            await translationsStore.StoreTranslationsAsync(translations);
            translationsCache.Set(translations);
        }
    }

    public class GetTranslationQuery : IGetTranslationQuery
    {
        private readonly ITranslationsCache translationsCache;

        public GetTranslationQuery(ITranslationsCache translationsCache)
        {
            this.translationsCache = translationsCache;
        }

        public string Execute(string key)
        {
            return translationsCache.Get()[key];
        }
    }

    public class GetLanguagesQuery : IGetLanguagesQuery
    {
        private readonly ILanguageServiceApi languageServiceApi;

        public GetLanguagesQuery(ILanguageServiceApi languageServiceApi)
        {
            this.languageServiceApi = languageServiceApi;
        }

        public Task<ICollection<Language>> ExecuteAsync()
        {
            return languageServiceApi.GetLanguagesAsync();
        }
    }

    public class ChangeLanguageCommand : IChangeLanguageCommand
    {
        private readonly ILanguageServiceApi languageServiceApi;
        private readonly IRefreshTranslationsCommand refreshTranslationsCommand;

        public ChangeLanguageCommand(ILanguageServiceApi languageServiceApi,IRefreshTranslationsCommand refreshTranslationsCommand)
        {
            this.languageServiceApi = languageServiceApi;
            this.refreshTranslationsCommand = refreshTranslationsCommand;
        }

        public async Task ExecuteAsync(Language language)
        {
            await languageServiceApi.ChangeLanguageAsync(language);
            await refreshTranslationsCommand.ExecuteAsync();
        }
    }

    public class LanguageViewModel
    {
        private readonly IGetLanguagesQuery getLanguagesQuery;
        private readonly IGetTranslationQuery getTranslationQuery;

        public LanguageViewModel(IGetLanguagesQuery getLanguagesQuery, IGetTranslationQuery getTranslationQuery, IChangeLanguageCommand changeLanguageCommand)
        {
            this.getLanguagesQuery = getLanguagesQuery;
            this.getTranslationQuery = getTranslationQuery;

            ChangeLanguageCommand = new AsyncCommand<Language>(x => changeLanguageCommand.ExecuteAsync(x));
        }

        public ICommand ChangeLanguageCommand { get; set; }

        public string ViewTitle { get; set; }

        public ICollection<Language> Languages { get; set; }

        public async Task StartAsync()
        {
            Languages = await getLanguagesQuery.ExecuteAsync();
            ViewTitle = getTranslationQuery.Execute("LanguageViewModel_Title");
        }
    }
}