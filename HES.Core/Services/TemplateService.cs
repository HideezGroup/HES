using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Accounts;
using HES.Core.Models.DataTableComponent;
using HES.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly IApplicationDbContext _dbContext;

        public TemplateService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Template> GetTemplateByIdAsync(string templateId)
        {
            return await _dbContext.Templates.FindAsync(templateId);
        }

        public async Task<List<Template>> GetTemplatesAsync()
        {
            return await _dbContext.Templates.ToListAsync();
        }

        public async Task<List<Template>> GetTemplatesAsync(DataLoadingOptions<TemplateFilter> dataLoadingOptions)
        {
            return await TemplateQuery(dataLoadingOptions).Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetTemplatesCountAsync(DataLoadingOptions<TemplateFilter> dataLoadingOptions)
        {
            return await TemplateQuery(dataLoadingOptions).CountAsync();
        }

        private IQueryable<Template> TemplateQuery(DataLoadingOptions<TemplateFilter> dataLoadingOptions)
        {
            var query = _dbContext.Templates.AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Name))
                {
                    query = query.Where(x => x.Name.Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Urls))
                {
                    query = query.Where(x => x.Urls.Contains(dataLoadingOptions.Filter.Urls, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Apps))
                {
                    query = query.Where(x => x.Apps.Contains(dataLoadingOptions.Filter.Apps, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Urls.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Apps.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(Template.Name):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                    break;
                case nameof(Template.Urls):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Urls) : query.OrderByDescending(x => x.Urls);
                    break;
                case nameof(Template.Apps):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Apps) : query.OrderByDescending(x => x.Apps);
                    break;
            }

            return query;
        }

        public void UnchangedTemplate(Template template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            _dbContext.Unchanged(template);
        }

        public async Task<Template> CreateTmplateAsync(Template template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            await ThrowIfTemplateExistAsync(template);

            template.Urls = Validation.VerifyUrls(template.Urls);

            var result = _dbContext.Templates.Add(template);
            await _dbContext.SaveChangesAsync();

            return result.Entity;
        }

        public async Task EditTemplateAsync(Template template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            await ThrowIfTemplateExistAsync(template);

            template.Urls = Validation.VerifyUrls(template.Urls);

            _dbContext.Templates.Update(template);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTemplateAsync(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                throw new ArgumentNullException(nameof(templateId));
            }

            var template = await GetTemplateByIdAsync(templateId);
            if (template == null)
            {
                throw new HESException(HESCode.TemplateNotFound);
            }

            _dbContext.Templates.Remove(template);
            await _dbContext.SaveChangesAsync();
        }

        private async Task ThrowIfTemplateExistAsync(Template template)
        {
            var templateExist = await _dbContext.ExistAsync<Template>(x => x.Name == template.Name && x.Id != template.Id);
            if (templateExist)
            {
                throw new HESException(HESCode.TemplateExist);
            }
        }
    }
}