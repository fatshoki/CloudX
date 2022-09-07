using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Specifications;

public sealed class BasketWithItemsSpecification : Specification<Basket>, ISingleResultSpecification
{
    //wtf? is this a method? what's it returning? what's 
    public BasketWithItemsSpecification(int basketId)
    {
        Query
            .Where(b => b.Id == basketId)
            .Include(b => b.Items);
    }

    public BasketWithItemsSpecification(string buyerId)
    {
        Query
            .Where(b => b.BuyerId == buyerId)
            .Include(b => b.Items);
    }
}
