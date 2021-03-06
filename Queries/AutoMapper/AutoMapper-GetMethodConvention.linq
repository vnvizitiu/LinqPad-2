<Query Kind="Program">
  <NuGetReference>AutoMapper</NuGetReference>
  <Namespace>AutoMapper</Namespace>
  <Namespace>AutoMapper.Configuration</Namespace>
  <Namespace>AutoMapper.Configuration.Conventions</Namespace>
  <Namespace>AutoMapper.Execution</Namespace>
  <Namespace>AutoMapper.Mappers</Namespace>
  <Namespace>AutoMapper.QueryableExtensions</Namespace>
  <Namespace>AutoMapper.QueryableExtensions.Impl</Namespace>
</Query>

void Main()
{
	var customer = new Customer
	{
		Name = "George Costanza"
	};
	var order = new Order
	{
		Customer = customer
	};
	var bosco = new Product
	{
		Name = "Bosco",
		Price = 4.99m
	};
	order.AddOrderLineItem(bosco, 15);

	// Configure AutoMapper

	Mapper.Initialize(cfg => cfg.CreateMap<Order, OrderDto>());

	// Perform mapping

	OrderDto dto = Mapper.Map<Order, OrderDto>(order);

	dto.CustomerName.Equals("George Costanza").Dump();
	dto.Total.Equals(74.85m).Dump();
}

public class OrderDto
{
	public string CustomerName { get; set; }
	public decimal Total { get; set; }
}

public class Order
{
	private readonly IList<OrderLineItem> _orderLineItems = new List<OrderLineItem>();

	public Customer Customer { get; set; }

	public OrderLineItem[] GetOrderLineItems()
	{
		return _orderLineItems.ToArray();
	}

	public void AddOrderLineItem(Product product, int quantity)
	{
		_orderLineItems.Add(new OrderLineItem(product, quantity));
	}

	public decimal GetTotal()
	{
		return _orderLineItems.Sum(li => li.GetTotal());
	}
}

public class Product
{
	public decimal Price { get; set; }
	public string Name { get; set; }
}

public class OrderLineItem
{
	public OrderLineItem(Product product, int quantity)
	{
		Product = product;
		Quantity = quantity;
	}

	public Product Product { get; private set; }
	public int Quantity { get; private set; }

	public decimal GetTotal()
	{
		return Quantity * Product.Price;
	}
}

public class Customer
{
	public string Name { get; set; }
}