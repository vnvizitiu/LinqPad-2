<Query Kind="Program">
  <NuGetReference>AutoMapper</NuGetReference>
  <NuGetReference>Unity</NuGetReference>
  <NuGetReference>WindowsAzure.ServiceBus</NuGetReference>
  <Namespace>Microsoft.Azure</Namespace>
  <Namespace>Microsoft.Practices.ObjectBuilder2</Namespace>
  <Namespace>Microsoft.Practices.ServiceLocation</Namespace>
  <Namespace>Microsoft.Practices.Unity</Namespace>
  <Namespace>Microsoft.Practices.Unity.Configuration</Namespace>
  <Namespace>Microsoft.Practices.Unity.Configuration.ConfigurationHelpers</Namespace>
  <Namespace>Microsoft.Practices.Unity.ObjectBuilder</Namespace>
  <Namespace>Microsoft.Practices.Unity.StaticFactory</Namespace>
  <Namespace>Microsoft.Practices.Unity.Utility</Namespace>
  <Namespace>Microsoft.ServiceBus</Namespace>
  <Namespace>Microsoft.ServiceBus.Channels</Namespace>
  <Namespace>Microsoft.ServiceBus.Common</Namespace>
  <Namespace>Microsoft.ServiceBus.Configuration</Namespace>
  <Namespace>Microsoft.ServiceBus.Description</Namespace>
  <Namespace>Microsoft.ServiceBus.Management</Namespace>
  <Namespace>Microsoft.ServiceBus.Messaging</Namespace>
  <Namespace>Microsoft.ServiceBus.Messaging.Amqp</Namespace>
  <Namespace>Microsoft.ServiceBus.Messaging.Amqp.Serialization</Namespace>
  <Namespace>Microsoft.ServiceBus.Messaging.Configuration</Namespace>
  <Namespace>Microsoft.ServiceBus.Tracing</Namespace>
  <Namespace>Microsoft.ServiceBus.Web</Namespace>
</Query>

void Main()
{
	sendTestMessage();
	receiveTestMessage();
}

private readonly string _queueConnectionString  = "Endpoint=sb://etech-onprem-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=8etT4mg3BVSBPGfoi72zSGSYBeaKR8v3P5jk2etKTH0=";
private readonly string _queueName = "test";

private QueueClient _queueClient;

private BrokeredMessage _receivedMessage = null;

void receiveTestMessage()
{
	var client = QueueClient.CreateFromConnectionString(_queueConnectionString, _queueName);
	var message = client.Receive(TimeSpan.FromSeconds(2)); 
	if (message == null) return;
	Console.WriteLine(message.GetBody<string>());
	message.Complete();
}

void sendTestMessage()
{
	var client = QueueClient.CreateFromConnectionString(_queueConnectionString, _queueName);
	client.Send(new BrokeredMessage("Hello World!"));
}


public bool TryGetMessage<T>(out T message, Action<Exception> onError = null)
{
	try
	{
		message = GetMessage<T>();
	}
	catch (Exception ex)
	{
		onError?.Invoke(ex);
		message = default(T);
		return false;
	}

	return message != null;
}

public T GetMessage<T>()
{
	_receivedMessage = ReceiveMessage();
	if (_receivedMessage == null)
	{
		return default(T);
	}

	object receivedType;
	var expectedType = typeof(T).ToString();
	//TODO use message.ContentType instead
	var isPayloadTypeProvided = _receivedMessage.Properties.TryGetValue("PayloadType", out receivedType);
	if (isPayloadTypeProvided && expectedType.Equals(receivedType.ToString()))
	{
		return _receivedMessage.GetBody<T>();
	}

	Abandon();
	throw new MessageTypeMismatchException(expectedType, receivedType?.ToString());
}

private BrokeredMessage ReceiveMessage()
{
	try
	{
		return _queueClient.Receive(TimeSpan.FromSeconds(2));
	}
	catch (MessagingException ex)
	{
		if (ex.IsTransient)
		{
			Thread.Sleep(TimeSpan.FromSeconds(10));
		}

		throw;
	}
}

public void Abandon()
{
	_receivedMessage?.Abandon();
	_receivedMessage = null;
}

public void Complete()
{
	_receivedMessage?.Complete();
	_receivedMessage = null;
}

public class MessageTypeMismatchException : Exception
{
	private readonly string _expectedType;
	private readonly string _receivedType;

	public MessageTypeMismatchException(string expectedType, string receivedType)
	{
		_expectedType = expectedType;
		_receivedType = receivedType ?? string.Empty;
	}

	public override string Message => $"Body type mismatched, expected type was {_expectedType}, received {_receivedType}";
}