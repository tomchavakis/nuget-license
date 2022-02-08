namespace NuGetUtility.Test.Helper
{
    /// <summary>
    ///     credit: https://stackoverflow.com/a/29823390/1199089
    /// </summary>
    internal static class TypeExtensions
    {
        public static bool IsOfGenericType(this Type typeToCheck, Type genericType)
        {
            return typeToCheck.IsOfGenericType(genericType, out var _);
        }

        public static bool IsOfGenericType(this Type typeToCheck, Type genericType, out Type? concreteGenericType)
        {
            while (true)
            {
                concreteGenericType = null;

                if (genericType == null)
                {
                    throw new ArgumentNullException(nameof(genericType));
                }

                if (!genericType.IsGenericTypeDefinition)
                {
                    throw new ArgumentException("The definition needs to be a GenericTypeDefinition",
                        nameof(genericType));
                }

                if ((typeToCheck == null) || (typeToCheck == typeof(object)))
                {
                    return false;
                }

                if (typeToCheck == genericType)
                {
                    concreteGenericType = typeToCheck;
                    return true;
                }

                if ((typeToCheck.IsGenericType ? typeToCheck.GetGenericTypeDefinition() : typeToCheck) == genericType)
                {
                    concreteGenericType = typeToCheck;
                    return true;
                }

                if (genericType.IsInterface)
                {
                    foreach (var i in typeToCheck.GetInterfaces())
                    {
                        if (i.IsOfGenericType(genericType, out concreteGenericType))
                        {
                            return true;
                        }
                    }
                }

                typeToCheck = typeToCheck.BaseType!;
            }
        }
    }
}
