/*
	This file was generated and should not be modified directly
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Net.Data;

namespace Bam.Net.UserAccounts.Data
{
    public class PasswordQuestionPagedQuery: PagedQuery<PasswordQuestionColumns, PasswordQuestion>
    { 
		public PasswordQuestionPagedQuery(PasswordQuestionColumns orderByColumn, PasswordQuestionQuery query, Database db = null) : base(orderByColumn, query, db) { }
    }
}