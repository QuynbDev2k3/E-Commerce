﻿using LinqKit;
using Microsoft.EntityFrameworkCore;
using Project.Business.Interface.Repositories;
using Project.Business.Model;
using Project.DbManagement;
using Project.DbManagement.Entity;
using SERP.Framework.Business;
using SERP.Framework.Common;
using SERP.Framework.Common.Extensions;
using SERP.Framework.DB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Business.Implement
{
    public class ProductCategoriesRelationRepository : IProductCategoriesRelationRepository
    {
        private readonly ProjectDbContext _context;

        public ProductCategoriesRelationRepository(ProjectDbContext context)
        {
            _context = context;
        }

        public async Task<ProductCategoriesRelation> FindAsync(Guid id)
        {
            var res = await _context.ProductCategoriesRelations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return res;
        }

        public async Task<IEnumerable<ProductCategoriesRelation>> ListAllAsync(ProductCategoriesRelationQueryModel queryModel)
        {
            var query = BuildQuery(queryModel);
            var resId = await query.Select(x => x.Id).ToListAsync();
            var res = await ListByIdsAsync(resId);
            return res;
        }

        public async Task<IEnumerable<ProductCategoriesRelation>> ListByIdsAsync(IEnumerable<Guid> ids)
        {
            var res = await _context.ProductCategoriesRelations.Where(x => ids.Contains(x.Id)).ToListAsync();
            return res;
        }

        public async Task<Pagination<ProductCategoriesRelation>> GetAllAsync(ProductCategoriesRelationQueryModel queryModel)
        {
            queryModel.Sort = QueryUtils.FormatSortInput(queryModel.Sort);
            IQueryable<ProductCategoriesRelation> queryable = BuildQuery(queryModel);
            string sortExpression = string.Empty;
            if (string.IsNullOrWhiteSpace(queryModel.Sort) || queryModel.Sort.Equals("-LastModifiedOnDate"))
            {
                queryable = queryable.OrderByDescending(x => x.LastModifiedOnDate);
            }
            else
            {
                sortExpression = queryModel.Sort;
            }

            return await queryable.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, sortExpression);
        }

        private IQueryable<ProductCategoriesRelation> BuildQuery(ProductCategoriesRelationQueryModel queryModel)
        {
            IQueryable<ProductCategoriesRelation> query = _context.ProductCategoriesRelations.AsNoTracking().Where(x => x.IsDeleted == false);

            if (queryModel.Id.HasValue)
            {
                query = query.Where(x => x.Id == queryModel.Id.Value);
            }
            if (queryModel.IdSanPham.HasValue)
            {
                query = query.Where(X => X.IdProduct == queryModel.IdSanPham.Value);
            }
            if(queryModel.IdDanhMuc.HasValue)
            {
                query = query.Where(x => x.CategoriesId == queryModel.IdDanhMuc.Value);
            }
            if (queryModel.ListId != null && queryModel.ListId.Any())
            {
                query = query.Where(x => queryModel.ListId.Contains(x.Id));
            }

            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
                var expressionStarter = LinqKit.PredicateBuilder.New<ProductCategoriesRelation>();
                foreach (string ts in queryModel.ListTextSearch)
                {
                    expressionStarter = expressionStarter.Or(p => p.ProductName.Contains(ts.ToLower()) || p.Description.Contains(ts.ToLower()));
                }

                query = query.Where(expressionStarter);
            }

            if (!string.IsNullOrWhiteSpace(queryModel.FullTextSearch))
            {
                string fullTextSearch = queryModel.FullTextSearch.ToLower();
                query = query.Where(x => x.ProductName.Contains(fullTextSearch));
            }

            if (!string.IsNullOrEmpty(queryModel.TenSanPham))
            {
                query = query.Where(x => x.ProductName.Contains(queryModel.TenSanPham));
            }

            if (!string.IsNullOrEmpty(queryModel.Description))
            {
                query = query.Where(x => x.Description.Contains(queryModel.Description));
            }

            return query;
        }

        public async Task<int> GetCountAsync(ProductCategoriesRelationQueryModel queryModel)
        {
            var query = BuildQuery(queryModel);
            var res = await query.CountAsync();
            return res;
        }

        public async Task<ProductCategoriesRelation> SaveAsync(ProductCategoriesRelation entity)
        {
            var res = await SaveAsync(new[] { entity });
            return res.FirstOrDefault();
        }

        public virtual async Task<IEnumerable<ProductCategoriesRelation>> SaveAsync(IEnumerable<ProductCategoriesRelation> entities)
        {
            var updated = new List<ProductCategoriesRelation>();

            foreach (var entity in entities)
            {
                var exist = await _context.ProductCategoriesRelations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);

                if (exist == null)
                {
                    entity.CreateTracking(entity.Id);
                    entity.UpdateTracking(entity.Id);
                    _context.ProductCategoriesRelations.Add(entity);
                    updated.Add(entity);
                }
                else
                {
                    _context.Entry(exist).State = EntityState.Detached;
                    exist.ProductName = entity.ProductName;
                    exist.CategoryName = entity.CategoryName;
                    exist.RelationType = entity.RelationType;
                    exist.Order = entity.Order;
                    exist.IsPublish = entity.IsPublish;
                    exist.PublishStartDate = entity.PublishStartDate;
                    exist.PublishEndDate = entity.PublishEndDate;
                    exist.PublishOnDate = entity.PublishOnDate;
                    exist.Status = entity.Status;
                    exist.Description = entity.Description;

                    entity.UpdateTracking(entity.Id);
                    _context.ProductCategoriesRelations.Update(exist);
                    updated.Add(exist);
                }
            }
            await _context.SaveChangesAsync();

            return updated;
        }

        public async Task<ProductCategoriesRelation> DeleteAsync(Guid id)
        {
            var exist = await FindAsync(id);
            if (exist == null) throw new Exception(IProductCategoriesRelationRepository.MessageNoTFound);
            exist.IsDeleted = true;
            _context.ProductCategoriesRelations.Update(exist);
            await _context.SaveChangesAsync();
            return exist;
        }

        public Task<IEnumerable<ProductCategoriesRelation>> DeleteAsync(Guid[] deleteIds)
        {
            throw new NotImplementedException();
        }
    }
}
