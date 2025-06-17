﻿using Project.Business.Interface.Repositories;
using Project.Business.Model;
using Project.DbManagement;
using Project.DbManagement.Entity;
using SERP.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Interface
{
    public interface IVoucherRepository : IRepository<Voucher, VoucherQueryModel>
    {
        protected const string MessageNotFound = "Voucher not found";
        Task<Voucher> SaveAsync(Voucher article);
        Task<IEnumerable<Voucher>> SaveAsync(IEnumerable<Voucher> vouchers);
        Task<Voucher> FindByCodeAsync(string code);
        Task<bool> IsVoucherCodeExist(string code, Guid? voucherId);
        Task<List<Voucher>> RecommendVoucherAllShop();
    }
}
