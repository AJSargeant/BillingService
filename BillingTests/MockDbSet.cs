using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace BillingTests
{
    public class MockDbSet<TEntity> : Mock<DbSet<TEntity>> where TEntity : class
    {
        List<TEntity> data;

        public MockDbSet(List<TEntity> dataSource = null)
        {
            data = (dataSource ?? new List<TEntity>());
            var queryable = data.AsQueryable();

            //Set up data accessor Linq
            this.As<IQueryable<TEntity>>().Setup(e => e.Provider).Returns(queryable.Provider);
            this.As<IQueryable<TEntity>>().Setup(e => e.Expression).Returns(queryable.Expression);
            this.As<IQueryable<TEntity>>().Setup(e => e.ElementType).Returns(queryable.ElementType);
            this.As<IQueryable<TEntity>>().Setup(e => e.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            //Set up data mutator Linq
            this.Setup(e => e.Add(It.IsAny<TEntity>())).Callback<TEntity>(data.Add);
            this.Setup(e => e.Remove(It.IsAny<TEntity>())).Callback<TEntity>(this.Remove);
            this.Setup(e => e.RemoveRange(It.IsAny<IEnumerable<TEntity>>())).Callback<IEnumerable<TEntity>>(this.RemoveRange);
        }

        private void Remove(TEntity item)
        {
            data.Remove(item);
        }

        private void RemoveRange(IEnumerable<TEntity> items)
        {
            foreach (TEntity e in items)
            {
                data.Remove(e);
            }
        }
    }
}
