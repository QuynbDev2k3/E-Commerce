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

namespace Project.Business.Implement
{
    public class CartRepository : ICartRepository
    {
        private readonly ProjectDbContext _context;

        public CartRepository(ProjectDbContext context)
        {
            _context = context;
        }
        public async Task<Cart> FindAsync(Guid id)
        {
            var res = await _context.Carts.FindAsync(id);
            return res;
        }
        public async Task<IEnumerable<Cart>> ListAllAsync(CartQueryModel queryModel)
        {
            var query = BuildQuery(queryModel);
            var resId = await query.Select(x => x.Id).ToListAsync();
            var res = await ListByIdsAsync(resId);
            return res;

        }

        public async Task<IEnumerable<Cart>> ListByIdsAsync(IEnumerable<Guid> ids)
        {
            var res = await _context.Carts.Where(x => ids.Contains(x.Id)).ToListAsync();
            return res;
        }

        public async Task<Pagination<Cart>> GetAllAsync(CartQueryModel queryModel)
        {
            CartQueryModel cartQueryModel = queryModel;

            queryModel.Sort = QueryUtils.FormatSortInput(queryModel.Sort);
            IQueryable<Cart> queryable = BuildQuery(queryModel);
            string sortExpression = string.Empty;
            if (string.IsNullOrWhiteSpace(queryModel.Sort) || queryModel.Sort.Equals("-LastModifiedOnDate"))
            {
                queryable = queryable.OrderByDescending((Cart x) => x.LastModifiedOnDate);
            }
            else
            {
                sortExpression = queryModel.Sort;
            }

            return await queryable.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, sortExpression);
        }

        private IQueryable<Cart> BuildQuery(CartQueryModel queryModel)
        {
            IQueryable<Cart> query = _context.Carts.AsNoTracking().Where(x => x.IsDeleted != true);

            if (queryModel.Id.HasValue)
            {
                query = query.Where((Cart x) => x.Id == queryModel.Id.Value);
            }

            if (queryModel.ListId != null && queryModel.ListId.Any())
            {
                query = query.Where((Cart x) => queryModel.ListId.Contains(x.Id));
            }

            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
                ExpressionStarter<Cart> expressionStarter = LinqKit.PredicateBuilder.New<Cart>();
                foreach (string ts in queryModel.ListTextSearch)
                {
                    expressionStarter = expressionStarter.Or((Cart p) => p.Description.Contains(ts.ToLower()));
                }

                query = query.Where(expressionStarter);
            }

            if (queryModel.IdTaiKhoan.HasValue)
            {
                query = query.Where(x => x.IdUser == queryModel.IdTaiKhoan.Value);
            }

            if (queryModel.IdThongTinLienHe.HasValue)
            {
                query = query.Where(x => x.IdContact == queryModel.IdThongTinLienHe.Value);
            }

            if (queryModel.Status.HasValue)
            {
                query = query.Where(x => x.Status == queryModel.Status);
            }

            if (!string.IsNullOrEmpty(queryModel.Description))
            {
                query = query.Where(x => x.Description.Contains(queryModel.Description));
            }

            //if (queryModel.MetaDataQueries != null && queryModel.MetaDataQueries.Any())
            //{
            //    string text = queryModel.BuildMetadataQuery<NodeEntity>(conditionParameters);
            //    if (!string.IsNullOrEmpty(text17))
            //    {
            //        text = text + " AND (" + text17.Replace("MetadataEntity", MetadataEntityTableName) + ")";
            //    }
            //}

            //if (query.RelationQuery != null)
            //{
            //    string text18 = BuildRelationQueryExpression(query.RelationQuery, conditionParameters);
            //    if (!string.IsNullOrEmpty(text18))
            //    {
            //        text = text + " AND " + text18;
            //    }
            //}


            return query;
        }

        public async Task<int> GetCountAsync(CartQueryModel queryModel)
        {
            var query = BuildQuery(queryModel);
            var res = await query.CountAsync();
            return res;
        }

        public async Task<Cart> SaveAsync(Cart cart)
        {
            var res = await SaveAsync(new[] { cart });
            return res.FirstOrDefault();
        }

        public virtual async Task<IEnumerable<Cart>> SaveAsync(IEnumerable<Cart> carts)
        {
            var updated = new List<Cart>();

            foreach (var cart in carts)
            {
                var exist = await _context.Carts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                            x.Id == cart.Id
                    );

                if (exist == null)
                {
                    cart.CreateTracking(cart.Id);
                    cart.UpdateTracking(cart.Id);
                    _context.Carts.Add(cart);
                    updated.Add(cart);
                }
                else
                {
                    _context.Entry(exist).State = EntityState.Detached;
                    exist.IdUser = cart.IdUser;
                    exist.IdContact = cart.IdContact;
                    exist.Status = cart.Status;
                    exist.Description = cart.Description;

                    cart.UpdateTracking(cart.Id);
                    _context.Carts.Update(exist);
                    updated.Add(exist);
                }
            }
            await _context.SaveChangesAsync();

            return updated;
        }



        public async Task<Cart> DeleteAsync(Guid Id)
        {
            var exist = await FindAsync(Id);
            if (exist == null) throw new Exception(ICartRepository.MessageNoTFound);
            exist.IsDeleted = true;
            _context.Carts.Update(exist);
            _context.SaveChangesAsync();
            return exist;
        }

        public Task<IEnumerable<Cart>> DeleteAsync(Guid[] deleteIds)
        {
            throw new NotImplementedException();
        }

        public async Task<Cart> GetCartByUserId(Guid userId)
        {
            return await _context.Carts
                .Where(x => x.IdUser == userId && !x.IsDeleted.Value && x.Status == 1)
                .OrderByDescending(x => x.LastModifiedOnDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Cart>> LocCartTheoNhieuDK(CartQueryModel cartQueryModel)
        {
            var query = _context.Carts.AsQueryable();

            if (cartQueryModel.IdTaiKhoan.HasValue)
            {
                query = query.Where(c => c.IdUser == cartQueryModel.IdTaiKhoan);
            }

            if (cartQueryModel.IdThongTinLienHe.HasValue)
            {
                query = query.Where(c => c.IdContact == cartQueryModel.IdThongTinLienHe);
            }

            if (cartQueryModel.Status != null)
            {
                query = query.Where(c => c.Status == cartQueryModel.Status);
            }

            if (!string.IsNullOrWhiteSpace(cartQueryModel.Description))
            {
                query = query.Where(c => c.Description.Contains(cartQueryModel.Description));
            }

            return await query.ToListAsync();
        }
    }
}
