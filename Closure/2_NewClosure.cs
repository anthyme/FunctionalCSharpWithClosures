using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Closure.CommandsQueries;
using Closure.Legacy;

namespace Closure.New
{
    public delegate string GetTranslationQuery(string key);
    public delegate Task<ICollection<Language>> GetLanguagesQuery();
    public delegate Task ChangeLanguageCommand(Language language);
    public delegate Task LoadTranslationsCommand();
    public delegate Task RefreshTranslationsCommand();

    public class Queries
    {
        public static GetTranslationQuery GetTranslationQuery(ITranslationsCache translationsCache)
        {
            return key => translationsCache.Get()[key];
        }

        public static GetLanguagesQuery GetLanguagesQuery(ILanguageServiceApi languageServiceApi)
        {
            return languageServiceApi.GetLanguagesAsync;
        }
    }

    public class Comamnds
    {
        public static ChangeLanguageCommand ChangeLanguageCommand(
            RefreshTranslationsCommand refreshTranslationsCommand, 
            ILanguageServiceApi languageServiceApi)
        {
            return async language =>
            {
                await languageServiceApi.ChangeLanguageAsync(language);
                await refreshTranslationsCommand();
            };
        }

        public static LoadTranslationsCommand LoadTranslationsCommand(
            ITranslationsCache translationsCache, 
            ITranslationsDownloader translationsDownloader)
        {
            return async () =>
            {
                var translations = await translationsDownloader.DownloadTranslationsAsync();
                translationsCache.Set(translations);
            };
        }

        public static RefreshTranslationsCommand RefreshTranslationsCommand(
            ITranslationsStore translationsStore,
            ITranslationsCache translationsCache,
            ITranslationsDownloader translationsDownloader)
        {
            return async () =>
            {
                var translations = await translationsDownloader.DownloadTranslationsAsync();
                await translationsStore.StoreTranslationsAsync(translations);
                translationsCache.Set(translations);
            };
        }
    }

    public class LanguageViewModel
    {
        private readonly GetLanguagesQuery getLanguagesQuery;
        private readonly GetTranslationQuery getTranslationQuery;

        public LanguageViewModel(GetLanguagesQuery getLanguagesQuery, GetTranslationQuery getTranslationQuery, ChangeLanguageCommand changeLanguageCommand)
        {
            this.getLanguagesQuery = getLanguagesQuery;
            this.getTranslationQuery = getTranslationQuery;
            ChangeLanguageCommand = new AsyncCommand<Language>(x => changeLanguageCommand(x));
        }

        public ICommand ChangeLanguageCommand { get; set; }

        public string ViewTitle { get; set; }

        public ICollection<Language> Languages { get; set; }

        public async Task StartAsync()
        {
            Languages = await getLanguagesQuery();
            ViewTitle = getTranslationQuery("LanguageViewModel_Title");
        }
    }
}