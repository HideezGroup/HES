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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly IApplicationDbContext _applicationDbContext;

        public TemplateService(IApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Template> GetByIdAsync(string id)
        {
            return await _applicationDbContext.Templates.FindAsync(id);
        }

        public async Task<List<Template>> GetTemplatesAsync(DataLoadingOptions<TemplateFilter> dataLoadingOptions)
        {
            var query = _applicationDbContext.Templates.AsQueryable();

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
            var query = _applicationDbContext.Templates.AsQueryable();

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
            return await _applicationDbContext.Templates.ToListAsync();
        }

        public async Task<Template> CreateTmplateAsync(Template template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            var accountExist = await _applicationDbContext.Templates
                .AsQueryable()
                .Where(x => x.Name == template.Name && x.Id != template.Id)
                .AnyAsync();

            if (accountExist)
                throw new AlreadyExistException("Template with current name already exists.");

            template.Urls = Validation.VerifyUrls(template.Urls);

            var result = _applicationDbContext.Templates.Add(template);
            await _applicationDbContext.SaveChangesAsync();

            return result.Entity;
        }

        public void UnchangedTemplate(Template template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            _applicationDbContext.Unchanged(template);
        }

        public async Task EditTemplateAsync(Template template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            var accountExist = await _applicationDbContext.Templates
               .Where(x => x.Name == template.Name && x.Id != template.Id)
               .AnyAsync();

            if (accountExist)
                throw new AlreadyExistException("Template with current name already exists.");

            template.Urls = Validation.VerifyUrls(template.Urls);

            _applicationDbContext.Templates.Update(template);
            await _applicationDbContext.SaveChangesAsync();
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

            _applicationDbContext.Templates.Remove(template);
            await _applicationDbContext.SaveChangesAsync();
        }
    }
}