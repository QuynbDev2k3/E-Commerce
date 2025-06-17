﻿using Project.Common;
using SERP.Metadata.Models.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SERP.Framework.Common;

namespace Project.Business.Model
{
    public class CustomerQueryModel : PaginationRequest, IListMetadataFilterQuery
    {
        public new Guid? Id { get; set; }
        public string? Code { get; set; }
        public Guid? TTTHIDMain { get; set; }
        public List<Guid>? TTLHRelatedIds { get; set; }
        public string? Ten { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public string? Description { get; set; }
        public string? UserNameTaiKhoan { get; set; }
        public List<MetadataFilterQuery> MetaDataQueries { get; set; } = new List<MetadataFilterQuery>();
    }
}
