using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Closure.Legacy
{
    public class Language
    {
        public string Code { get; }

        public Language(string code)
        {
            Code = code;
        }
    }

    public interface ILanguageService
    {
        Task LoadTranslationsAsync();
        Task RefreshTranslationsAsync();
        string GetTranslation(string key);
        Task<ICollection<Language>> GetLanguagesAsync();
        Task ChangeLanguageAsync(Language language);
    }

    public interface ITranslationsStore
    {
        Task StoreTranslationsAsync(IDictionary<string, string> translations);
        Task<IDictionary<string, string>> GetTranslationsAsync();
    }

    public interface ITranslationsDownloader
    {
        Task<IDictionary<string, string>> DownloadTranslationsAsync();
    }

    public interface ILanguageServiceApi
    {
        Task<ICollection<Language>> GetLanguagesAsync();
        Task ChangeLanguageAsync(Language language);
    }

    public class LanguageService : ILanguageService
    {
        private readonly ITranslationsStore translationsStore;
        private readonly ITranslationsDownloader translationsDownloader;
        private readonly ILanguageServiceApi languageServiceApi;

        private IDictionary<string, string> translationsCache;

        public LanguageService(
            ITranslationsStore translationsStore, 
            ITranslationsDownloader translationsDownloader, 
            ILanguageServiceApi languageServiceApi)
        {
            this.translationsStore = translationsStore;
            this.translationsDownloader = translationsDownloader;
            this.languageServiceApi = languageServiceApi;
        }

        public async Task LoadTranslationsAsync()
        {
            translationsCache = await translationsStore.GetTranslationsAsync();
        }

        public async Task RefreshTranslationsAsync()
        {
            var translations = await translationsDownloader.DownloadTranslationsAsync();
            await translationsStore.StoreTranslationsAsync(translationsCache);
            translationsCache = translations;
        }

        public string GetTranslation(string key)
        {
            return translationsCache[key];
        }

        public Task<ICollection<Language>> GetLanguagesAsync()
        {
            return languageServiceApi.GetLanguagesAsync();
        }

        public async Task ChangeLanguageAsync(Language language)
        {
            await languageServiceApi.ChangeLanguageAsync(language);
            await RefreshTranslationsAsync();
        }
    }

    public class LanguageViewModel
    {
        private readonly ILanguageService languageService;

        public LanguageViewModel(ILanguageService languageService)
        {
            this.languageService = languageService;
            ChangeLanguageCommand = new AsyncCommand<Language>(languageService.ChangeLanguageAsync);
        }

        public ICommand ChangeLanguageCommand { get; set; }

        public string ViewTitle { get; set; }

        public ICollection<Language> Languages { get; set; }

        public async Task StartAsync()
        {
            Languages = await languageService.GetLanguagesAsync();
            ViewTitle = languageService.GetTranslation("LanguageViewModel_Title");
        }
    }
}