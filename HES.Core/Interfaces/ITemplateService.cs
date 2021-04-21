using HES.Core.Entities;
using HES.Core.Models.Accounts;
using HES.Core.Models.DataTableComponent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ITemplateService
    {
        Task<Template> GetTemplateByIdAsync(string templateId);
        Task<List<Template>> GetTemplatesAsync();
        Task<List<Template>> GetTemplatesAsync(DataLoadingOptions<TemplateFilter> dataLoadingOptions);
        Task<int> GetTemplatesCountAsync(DataLoadingOptions<TemplateFilter> dataLoadingOptions);
        void UnchangedTemplate(Template template);
        Task<Template> CreateTmplateAsync(Template template);
        Task EditTemplateAsync(Template template);
        Task DeleteTemplateAsync(string templateId);
    }
}