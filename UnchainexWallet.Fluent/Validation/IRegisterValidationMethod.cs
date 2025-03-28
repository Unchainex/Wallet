namespace UnchainexWallet.Fluent.Validation;

public interface IRegisterValidationMethod
{
	void RegisterValidationMethod(string propertyName, ValidateMethod validateMethod);
}
