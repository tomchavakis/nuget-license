// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Test.Helper.Type
{
    /// <summary>
    ///     credit: https://stackoverflow.com/a/29823390/1199089
    /// </summary>
    internal static class TypeExtensions
    {
        public static bool IsOfGenericType(this System.Type typeToCheck, System.Type genericType)
        {
            return typeToCheck.IsOfGenericType(genericType, out System.Type? _);
        }

        public static bool IsOfGenericType(this System.Type typeToCheck,
            System.Type genericType,
            out System.Type? concreteGenericType)
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
                    foreach (System.Type i in typeToCheck.GetInterfaces())
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
