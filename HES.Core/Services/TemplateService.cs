using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Accounts;
using HES.Core.Models.Web.DataTableComponent;
using HES.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class TemplateService : ITemplateService, IDisposable
    {
        private readonly IAsyncRepository<Template> _templateRepository;

        public TemplateService(IAsyncRepository<Template> repository)
        {
            _templateRepository = repository;
        }

        public IQueryable<Template> Query()
        {
            return _templateRepository.Query();
        }

        public async Task<Template> GetByIdAsync(string id)
        {
            return await _templateRepository.GetByIdAsync(id);
        }

        public async Task<List<Template>> GetTemplatesAsync(DataLoadingOptions<TemplateFilter> dataLoadingOptions)
        {
            var query = _templateRepository.Query();

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

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetTemplatesCountAsync(DataLoadingOptions<TemplateFilter> dataLoadingOptions)
        {
            var query = _templateRepository.Query();

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

            return await query.CountAsync();
        }

        public async Task<List<Template>> GetTemplatesAsync()
        {
            return await _templateRepository.Query().ToListAsync();
        }

        public async Task<Template> CreateTmplateAsync(Template template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            var accountExist = await _templateRepository
              .Query()
              .Where(x => x.Name == template.Name && x.Id != template.Id)
              .AnyAsync();

            if (accountExist)
                throw new AlreadyExistException("Template with current name already exists.");

            template.Urls = Validation.VerifyUrls(template.Urls);

            return await _templateRepository.AddAsync(template);
        }

        public async Task UnchangedTemplateAsync(Template template)
        {
            await _templateRepository.UnchangedAsync(template);
        }

        public async Task EditTemplateAsync(Template template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            var accountExist = await _templateRepository
               .Query()
               .Where(x => x.Name == template.Name && x.Id != template.Id)
               .AnyAsync();

            if (accountExist)
                throw new AlreadyExistException("Template with current name already exists.");

            template.Urls = Validation.VerifyUrls(template.Urls);

            await _templateRepository.UpdateAsync(template);
        }

        public async Task DeleteTemplateAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            var template = await GetByIdAsync(id);
            if (template == null)
            {
                throw new Exception("Template does not exist.");
            }
            await _templateRepository.DeleteAsync(template);
        }

        public async Task<bool> ExistAsync(Expression<Func<Template, bool>> predicate)
        {
            return await _templateRepository.ExistAsync(predicate);
        }

        public void Dispose()
        {
            _templateRepository.Dispose();
        }
    }
}