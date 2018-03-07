using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Rhisis.Database.Structures;

namespace Rhisis.Database.Repositories
{
    /// <summary>
    /// Friend repository.
    /// </summary>
    public sealed class FriendRepository : RepositoryBase<Friend>
    {
        /// <summary>
        /// Creates and initialize the <see cref="FriendRepository"/>.
        /// </summary>
        /// <param name="context"></param>
        public FriendRepository(DbContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Include other objects for each requests.
        /// </summary>
        /// <returns></returns>
        protected override IQueryable<Friend> GetQueryable()
        {
            return base.GetQueryable();
        }
    }
}
